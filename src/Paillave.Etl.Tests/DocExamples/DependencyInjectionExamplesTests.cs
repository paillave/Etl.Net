using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Xunit;

namespace Paillave.Etl.Tests.DocExamples;

/// <summary>
/// Examples for <c>documentation/docs/recipes/16_dependencyInjection.md</c>.
///
/// These tests exercise how an <see cref="IServiceProvider"/> attached to
/// <see cref="ExecutionOptions{TConfig}.Services"/> is forwarded to the
/// `Do` and `SelectResolved` overloads that accept a service provider as
/// their second parameter.
/// </summary>
public class DependencyInjectionExamplesTests
{
    // -------- Sample injected services --------------------------------

    public interface INotifier
    {
        void Notify(string message);
        IReadOnlyList<string> Sent { get; }
    }

    private sealed class InMemoryNotifier : INotifier
    {
        private readonly ConcurrentQueue<string> _sent = new();
        public void Notify(string message) => _sent.Enqueue(message);
        public IReadOnlyList<string> Sent => _sent.ToArray();
    }

    public interface IPriceLookup
    {
        decimal GetPrice(string sku);
    }

    private sealed class StaticPriceLookup : IPriceLookup
    {
        private readonly IReadOnlyDictionary<string, decimal> _prices;
        public StaticPriceLookup(IReadOnlyDictionary<string, decimal> prices) => _prices = prices;
        public decimal GetPrice(string sku) => _prices.TryGetValue(sku, out var p) ? p : 0m;
    }

    public class OrderLine
    {
        public string Sku { get; set; } = "";
        public int Quantity { get; set; }
    }

    public class PricedOrderLine
    {
        public string Sku { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
    }

    // ===================================================================
    // Do(name, Action<TIn, IServiceProvider>) — call a service per row
    // ===================================================================

    [Fact]
    public async Task Do_WithInjectedService_CallsServiceForEachRow()
    {
        var notifier = new InMemoryNotifier();
        var services = new ServiceCollection()
            .AddSingleton<INotifier>(notifier)
            .BuildServiceProvider();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            "go",
            root => root
                .CrossApply("seed", _ => new[] { "alpha", "beta", "gamma" })
                .Do("notify", (row, sp) =>
                {
                    var n = sp.GetRequiredService<INotifier>();
                    n.Notify($"processed:{row}");
                }),
            new ExecutionOptions<string> { Services = services });

        Assert.False(status.Failed);
        Assert.Equal(
            new[] { "processed:alpha", "processed:beta", "processed:gamma" },
            notifier.Sent.OrderBy(s => s).ToArray());
    }

    // ===================================================================
    // SelectResolved — enrich rows from an injected service
    // ===================================================================

    [Fact]
    public async Task SelectResolved_EnrichesRowsFromInjectedService()
    {
        var prices = new Dictionary<string, decimal>
        {
            ["A1"] = 10m,
            ["B2"] = 25m,
            ["C3"] = 7.5m,
        };
        var services = new ServiceCollection()
            .AddSingleton<IPriceLookup>(new StaticPriceLookup(prices))
            .BuildServiceProvider();

        var collected = new ConcurrentBag<PricedOrderLine>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            "go",
            root => root
                .CrossApply("seed", _ => new[]
                {
                    new OrderLine { Sku = "A1", Quantity = 2 },
                    new OrderLine { Sku = "B2", Quantity = 1 },
                    new OrderLine { Sku = "C3", Quantity = 4 },
                })
                .SelectResolved("price", (row, sp) =>
                {
                    var lookup = sp.GetRequiredService<IPriceLookup>();
                    return new PricedOrderLine
                    {
                        Sku       = row.Sku,
                        Quantity  = row.Quantity,
                        UnitPrice = lookup.GetPrice(row.Sku),
                    };
                })
                .Do("collect", collected.Add),
            new ExecutionOptions<string> { Services = services });

        Assert.False(status.Failed);
        var byKey = collected.ToDictionary(p => p.Sku);
        Assert.Equal(20m, byKey["A1"].Total);
        Assert.Equal(25m, byKey["B2"].Total);
        Assert.Equal(30m, byKey["C3"].Total);
    }

    // ===================================================================
    // Multiple services resolved together
    // ===================================================================

    [Fact]
    public async Task Pipeline_CanResolveSeveralServices()
    {
        var notifier = new InMemoryNotifier();
        var prices = new Dictionary<string, decimal> { ["A1"] = 10m, ["B2"] = 25m };

        var services = new ServiceCollection()
            .AddSingleton<INotifier>(notifier)
            .AddSingleton<IPriceLookup>(new StaticPriceLookup(prices))
            .BuildServiceProvider();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            "go",
            root => root
                .CrossApply("seed", _ => new[]
                {
                    new OrderLine { Sku = "A1", Quantity = 3 },
                    new OrderLine { Sku = "B2", Quantity = 2 },
                })
                .SelectResolved("price", (row, sp) => new PricedOrderLine
                {
                    Sku       = row.Sku,
                    Quantity  = row.Quantity,
                    UnitPrice = sp.GetRequiredService<IPriceLookup>().GetPrice(row.Sku),
                })
                .Do("audit", (row, sp) =>
                {
                    sp.GetRequiredService<INotifier>()
                      .Notify($"{row.Sku} x{row.Quantity} = {row.Total}");
                }),
            new ExecutionOptions<string> { Services = services });

        Assert.False(status.Failed);
        Assert.Contains("A1 x3 = 30",  notifier.Sent);
        Assert.Contains("B2 x2 = 50",  notifier.Sent);
    }
}
