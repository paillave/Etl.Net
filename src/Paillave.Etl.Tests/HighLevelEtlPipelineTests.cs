using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Xunit;

namespace Paillave.Etl.Tests;

/// <summary>
/// High-level ETL pipeline tests with multiple branches, lookups, joins,
/// aggregations and unions, executed on synthetic in-memory datasets in the
/// 200k–500k rows range. They mirror the shape of
/// <c>Tutorials/Paillave.Etl.Samples/TestImport.cs</c> but at a much larger
/// scale and without depending on file connectors or EF Core.
/// </summary>
[Collection("MemorySensitive")]
public class HighLevelEtlPipelineTests
{
    // -------------------------------------------------------------------
    // Synthetic domain model — a small star schema:
    //   * Customer    (1 .. NbCustomers)
    //   * Product     (1 .. NbProducts) with a Category
    //   * Order header (one per order, bound to a Customer)
    //   * OrderLine   (several per order, bound to a Product)
    //
    // Generated deterministically so reference values can be computed
    // with plain LINQ in O(N).
    // -------------------------------------------------------------------

    private const int NbCustomers = 5_000;
    private const int NbProducts = 2_000;
    private const int NbOrders = 50_000;
    private const int LinesPerOrder = 4; // => 200_000 order lines
    private const int Categories = 25;

    private sealed class CustomerRow { public int Id { get; set; } public string Name { get; set; } = ""; public string Country { get; set; } = ""; }
    private sealed class ProductRow { public int Id { get; set; } public string Name { get; set; } = ""; public int CategoryId { get; set; } public decimal UnitPrice { get; set; } }
    private sealed class OrderRow { public int Id { get; set; } public int CustomerId { get; set; } public DateTime Date { get; set; } }
    private sealed class OrderLineRow { public int OrderId { get; set; } public int ProductId { get; set; } public int Quantity { get; set; } }

    private static IEnumerable<CustomerRow> GenerateCustomers()
    {
        var countries = new[] { "FR", "DE", "US", "JP", "BR", "IN", "GB", "ES" };
        for (int i = 1; i <= NbCustomers; i++)
            yield return new CustomerRow { Id = i, Name = $"Customer-{i}", Country = countries[i % countries.Length] };
    }

    private static IEnumerable<ProductRow> GenerateProducts()
    {
        for (int i = 1; i <= NbProducts; i++)
            yield return new ProductRow { Id = i, Name = $"Product-{i}", CategoryId = (i % Categories) + 1, UnitPrice = 1m + (i % 100) };
    }

    private static IEnumerable<OrderRow> GenerateOrders()
    {
        var origin = new DateTime(2024, 1, 1);
        for (int i = 1; i <= NbOrders; i++)
            yield return new OrderRow { Id = i, CustomerId = ((i - 1) % NbCustomers) + 1, Date = origin.AddMinutes(i) };
    }

    private static IEnumerable<OrderLineRow> GenerateOrderLines()
    {
        for (int o = 1; o <= NbOrders; o++)
            for (int l = 0; l < LinesPerOrder; l++)
                yield return new OrderLineRow { OrderId = o, ProductId = ((o * 7 + l * 13) % NbProducts) + 1, Quantity = ((o + l) % 5) + 1 };
    }

    // -------------------------------------------------------------------
    // 1. Multi-branch pipeline: source split into 4 reference streams
    //    + a wide line stream that is joined back to all of them.
    // -------------------------------------------------------------------

    [Fact]
    public async Task FullStarSchemaPipeline_LargeVolume_AllBranchesProduceCorrectCounts()
    {
        var customerCount = 0;
        var productCount = 0;
        var orderCount = 0;
        var enrichedLineCount = 0;
        decimal totalRevenue = 0m;
        int countryRows = 0;
        int categoryRows = 0;

        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            // Five "source" branches built off the same root config row.
            var customers = root
                .CrossApply("gen customers", _ => GenerateCustomers())
                .Do("count customers", _ => Interlocked.Increment(ref customerCount));

            var products = root
                .CrossApply("gen products", _ => GenerateProducts())
                .Do("count products", _ => Interlocked.Increment(ref productCount));

            var orders = root
                .CrossApply("gen orders", _ => GenerateOrders())
                .Do("count orders", _ => Interlocked.Increment(ref orderCount));

            var orderLines = root
                .CrossApply("gen order lines", _ => GenerateOrderLines());

            // Line × Product → line with unit price and category.
            var pricedLines = orderLines
                .Lookup(
                    "join product",
                    products,
                    l => l.ProductId,
                    p => p.Id,
                    (l, p) => new
                    {
                        l.OrderId,
                        l.ProductId,
                        l.Quantity,
                        p.CategoryId,
                        LineAmount = p.UnitPrice * l.Quantity
                    });

            // Line × Order → line with customer.
            var withCustomer = pricedLines
                .Lookup(
                    "join order",
                    orders,
                    l => l.OrderId,
                    o => o.Id,
                    (l, o) => new { l.LineAmount, l.CategoryId, o.CustomerId });

            // Line × Customer → line with country.
            var enriched = withCustomer
                .Lookup(
                    "join customer",
                    customers,
                    l => l.CustomerId,
                    c => c.Id,
                    (l, c) => new { l.LineAmount, l.CategoryId, c.Country })
                .Do("count enriched", _ => Interlocked.Increment(ref enrichedLineCount))
                .Do("revenue", l => InterlockedAdd(ref totalRevenue, l.LineAmount));

            // Branch A: revenue per country.
            enriched
                .Aggregate(
                    "agg by country",
                    l => l.Country,
                    _ => 0m,
                    (acc, l) => acc + l.LineAmount)
                .Do("count countries", _ => Interlocked.Increment(ref countryRows));

            // Branch B: revenue per category.
            enriched
                .Aggregate(
                    "agg by category",
                    l => l.CategoryId,
                    _ => 0m,
                    (acc, l) => acc + l.LineAmount)
                .Do("count categories", _ => Interlocked.Increment(ref categoryRows));
        });

        Assert.False(status.Failed, status.ErrorTraceEvent?.ToString());

        Assert.Equal(NbCustomers, customerCount);
        Assert.Equal(NbProducts, productCount);
        Assert.Equal(NbOrders, orderCount);
        Assert.Equal(NbOrders * LinesPerOrder, enrichedLineCount);

        // Reference revenue computed with plain LINQ.
        var prodById = GenerateProducts().ToDictionary(p => p.Id);
        decimal expected = GenerateOrderLines()
            .Sum(l => prodById[l.ProductId].UnitPrice * l.Quantity);
        Assert.Equal(expected, totalRevenue);

        Assert.Equal(8, countryRows);   // 8 countries
        Assert.Equal(Categories, categoryRows);
    }

    // -------------------------------------------------------------------
    // 2. UnionAll of two large derived branches + GroupBy + Top.
    // -------------------------------------------------------------------

    [Fact]
    public async Task UnionAll_AndGroupBy_OnLargeStream()
    {
        int unionCount = 0;
        int groupCount = 0;
        int topCount = 0;
        var groupSizes = new ConcurrentBag<int>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            var lines = root.CrossApply("gen lines", _ => GenerateOrderLines());

            var evenOrders = lines.Where("even order id", l => l.OrderId % 2 == 0);
            var oddOrders = lines.Where("odd order id", l => l.OrderId % 2 == 1);

            var union = evenOrders.UnionAll("union halves", oddOrders)
                .Do("count union", _ => Interlocked.Increment(ref unionCount));

            // Group by product id and count the occurrences in each group.
            union
                .GroupBy(
                    "group by product",
                    l => l.ProductId,
                    (sub, _) => sub.Aggregate(
                        "count product",
                        l => l.ProductId,
                        _ => 0,
                        (acc, _) => acc + 1))
                .Do("count groups", g =>
                {
                    Interlocked.Increment(ref groupCount);
                    groupSizes.Add(g.Aggregation);
                });

            // Top 100 lines (by physical order).
            lines.Top("top 100", 100)
                .Do("count top", _ => Interlocked.Increment(ref topCount));
        });

        Assert.False(status.Failed, status.ErrorTraceEvent?.ToString());
        Assert.Equal(NbOrders * LinesPerOrder, unionCount);
        Assert.Equal(NbProducts, groupCount);
        Assert.Equal(NbOrders * LinesPerOrder, groupSizes.Sum());
        Assert.Equal(100, topCount);
    }

    // -------------------------------------------------------------------
    // 3. Distinct + Sort + multi-input Combine.
    // -------------------------------------------------------------------

    [Fact]
    public async Task Distinct_Sort_AndCombine_OnLargeStream()
    {
        int distinctCustomers = 0;
        int distinctProducts = 0;
        var combined = new ConcurrentBag<(int customers, int products, int orders)>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            var orders = root.CrossApply("gen orders", _ => GenerateOrders());
            var lines = root.CrossApply("gen lines", _ => GenerateOrderLines());

            // Distinct customers from the orders stream.
            var customerIds = orders
                .Distinct("distinct customer", o => o.CustomerId)
                .Select("to id cust", o => o.CustomerId)
                .Sort("sort customers", id => id)
                .Do("count distinct customer", _ => Interlocked.Increment(ref distinctCustomers));

            // Distinct products from the lines stream.
            var productIds = lines
                .Distinct("distinct product", l => l.ProductId)
                .Select("to id prod", l => l.ProductId)
                .Sort("sort products", id => id)
                .Do("count distinct product", _ => Interlocked.Increment(ref distinctProducts));

            // Aggregate counts to single-element streams and combine all three.
            var customerCount = customerIds.Aggregate("count cust", _ => 0, _ => 0, (acc, _) => acc + 1)
                .Select("project cust", a => a.Aggregation).EnsureSingle("single cust");
            var productCount = productIds.Aggregate("count prod", _ => 0, _ => 0, (acc, _) => acc + 1)
                .Select("project prod", a => a.Aggregation).EnsureSingle("single prod");
            var orderCount = orders.Aggregate("count ord", _ => 0, _ => 0, (acc, _) => acc + 1)
                .Select("project ord", a => a.Aggregation).EnsureSingle("single ord");

            customerCount.Combine(
                "combine three",
                productCount,
                orderCount,
                (c, p, o) => (customers: c, products: p, orders: o))
                .Do("collect", combined.Add);
        });

        Assert.False(status.Failed, status.ErrorTraceEvent?.ToString());
        Assert.Equal(NbCustomers, distinctCustomers);
        Assert.Equal(NbProducts, distinctProducts);
        var single = Assert.Single(combined);
        Assert.Equal(NbCustomers, single.customers);
        Assert.Equal(NbProducts, single.products);
        Assert.Equal(NbOrders, single.orders);
    }

    // -------------------------------------------------------------------
    // 4. Stress / leak regression for the high-level framework: run a
    //    medium-sized pipeline 50 times in a row and verify nothing
    //    accumulates (no OOM, total wall-clock stays bounded).
    // -------------------------------------------------------------------

    [Fact]
    public async Task RepeatedPipelineRuns_DoNotLeakBetweenExecutions()
    {
        const int Iterations = 30;
        long memoryBefore = GC.GetTotalMemory(forceFullCollection: true);

        for (int i = 0; i < Iterations; i++)
        {
            int linesSeen = 0;
            var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
            {
                var lines = root.CrossApply("gen lines", _ => GenerateOrderLines());
                var products = root.CrossApply("gen products", _ => GenerateProducts());
                lines
                    .Lookup("join product", products, l => l.ProductId, p => p.Id,
                        (l, p) => new { l.OrderId, p.CategoryId, l.Quantity })
                    .Aggregate("agg by category", x => x.CategoryId, _ => 0,
                        (acc, x) => acc + x.Quantity)
                    .Do("count", _ => Interlocked.Increment(ref linesSeen));
            });
            Assert.False(status.Failed, status.ErrorTraceEvent?.ToString());
            Assert.Equal(Categories, linesSeen);
        }

        long memoryAfter = GC.GetTotalMemory(forceFullCollection: true);
        long delta = memoryAfter - memoryBefore;
        // Allow plenty of slack: this is a smoke test for unbounded growth,
        // not a precise budget. Before the leak fixes a single run pinned
        // ~1MB+ of operator graph; over 30 runs the residual must stay
        // well below that ceiling.
        Assert.True(
            delta < 30 * 1024 * 1024,
            $"Memory grew by {delta / (1024 * 1024)} MB across {Iterations} runs.");
    }

    // -------------------------------------------------------------------
    // helpers
    // -------------------------------------------------------------------

    private static readonly object _decimalLock = new();
    private static void InterlockedAdd(ref decimal target, decimal value)
    {
        // decimal is not supported by Interlocked.Add.
        lock (_decimalLock) target += value;
    }
}
