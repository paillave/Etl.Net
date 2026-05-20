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
/// Reproduces the shape of <c>Tutorials/Paillave.Etl.Samples/TestImport.cs</c>
/// — two correlated source files feeding many normalized entity branches
/// reunited via <c>CorrelateToSingle</c> / <c>Lookup</c> — but on a much
/// richer finance schema (trade blotter + EOD positions) and at a much
/// larger volume. Primary goal: exercise the correlation/lookup machinery
/// hard enough to surface any deadlock in the heavy-normalization path.
/// </summary>
[Collection("MemorySensitive")]
public class FinanceTradeImportPipelineTests
{
    // Volumes: tuned to be heavy enough to stress the correlation buffers
    // (CorrelateToSingle materializes the right stream into a dictionary)
    // while keeping the test bounded for CI.
    private const int NbAccounts = 20_000;
    private const int NbBooks = 6_000;
    private const int NbTraders = 5_000;
    private const int NbDesks = 10;
    private const int NbCounterparties = 8_000;
    private const int NbSecurities = 80_000;
    private const int NbCountries = 15;
    private const int NbCurrencies = 12;
    private const int NbExchanges = 25;
    private const int NbAssetClasses = 6;
    private const int NbSectors = 11;
    private const int NbIndustries = 30;

    private const int NbTrades = 4_000_000;
    private const int NbPositions = 8_000_000;

    // ---------------- raw rows (mirror parsed CSV records) ----------------

    public class TradeTicketRow
    {
        // Trade identifiers
        public string TradeId { get; set; } = "";
        public DateTime TradeDate { get; set; }
        public DateTime SettleDate { get; set; }
        public DateTime BookingDate { get; set; }
        // Account / book / desk
        public string AccountCode { get; set; } = "";
        public string AccountName { get; set; } = "";
        public string AccountType { get; set; } = ""; // INTERNAL / CLIENT
        public string BookCode { get; set; } = "";
        public string BookName { get; set; } = "";
        public string Desk { get; set; } = "";
        public string BusinessLine { get; set; } = "";
        // Trader
        public string TraderId { get; set; } = "";
        public string TraderName { get; set; } = "";
        public string TraderEmail { get; set; } = "";
        // Counterparty
        public string CounterpartyCode { get; set; } = "";
        public string CounterpartyName { get; set; } = "";
        public string CounterpartyLEI { get; set; } = "";
        public string CounterpartyCountry { get; set; } = "";
        public string CounterpartyType { get; set; } = ""; // BANK / BROKER / HF
        // Security
        public string Isin { get; set; } = "";
        public string Cusip { get; set; } = "";
        public string Ticker { get; set; } = "";
        public string SecurityName { get; set; } = "";
        public string AssetClass { get; set; } = "";
        public string Sector { get; set; } = "";
        public string Industry { get; set; } = "";
        public string IssuerCountry { get; set; } = "";
        public string Currency { get; set; } = "";
        public string ExchangeMic { get; set; } = "";
        // Trade economics
        public string Side { get; set; } = ""; // BUY / SELL
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal Commission { get; set; }
        public decimal Fees { get; set; }
        public decimal Tax { get; set; }
        public decimal NetAmount { get; set; }
        // Settlement
        public string ClearingHouse { get; set; } = "";
        public string CustodianCode { get; set; } = "";
    }

    public class PositionRow
    {
        public string AccountCode { get; set; } = "";
        public string Isin { get; set; } = "";
        public DateTime Date { get; set; }
        public decimal Quantity { get; set; }
        public decimal MarketPrice { get; set; }
        public decimal MarketValue { get; set; }
        public decimal BookValue { get; set; }
        public decimal UnrealizedPnL { get; set; }
        public decimal AccruedInterest { get; set; }
        public string Currency { get; set; } = "";
        public decimal FxRate { get; set; }
    }

    // ---------------- normalized entities (output dimensions) ----------------

    public class CountryEntity { public string Code { get; set; } = ""; }
    public class CurrencyEntity { public string Code { get; set; } = ""; }
    public class ExchangeEntity { public string Mic { get; set; } = ""; }
    public class AssetClassEntity { public string Name { get; set; } = ""; }
    public class SectorEntity { public string Name { get; set; } = ""; }
    public class IndustryEntity { public string Name { get; set; } = ""; }
    public class DeskEntity { public string Name { get; set; } = ""; public string BusinessLine { get; set; } = ""; }

    public class AccountEntity
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
    }
    public class BookEntity
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string AccountCode { get; set; } = "";
        public string DeskName { get; set; } = "";
    }
    public class TraderEntity
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string DeskName { get; set; } = "";
    }
    public class CounterpartyEntity
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Lei { get; set; } = "";
        public string Country { get; set; } = "";
        public string Type { get; set; } = "";
    }
    public class SecurityEntity
    {
        public string Isin { get; set; } = "";
        public string Cusip { get; set; } = "";
        public string Ticker { get; set; } = "";
        public string Name { get; set; } = "";
        public string AssetClass { get; set; } = "";
        public string Sector { get; set; } = "";
        public string Industry { get; set; } = "";
        public string IssuerCountry { get; set; } = "";
        public string Currency { get; set; } = "";
        public string ExchangeMic { get; set; } = "";
    }
    public class TradeFact
    {
        public string TradeId { get; set; } = "";
        public DateTime TradeDate { get; set; }
        public string AccountCode { get; set; } = "";
        public string BookCode { get; set; } = "";
        public string TraderId { get; set; } = "";
        public string CounterpartyCode { get; set; } = "";
        public string Isin { get; set; } = "";
        public string Side { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal NetAmount { get; set; }
        public decimal SignedNotional { get; set; }
    }
    public class PositionFact
    {
        public string AccountCode { get; set; } = "";
        public string Isin { get; set; } = "";
        public DateTime Date { get; set; }
        public decimal MarketValueBase { get; set; } // converted via FxRate
        public string Currency { get; set; } = "";
    }

    // ---------------- deterministic synthetic generators ----------------

    private static readonly string[] Countries =
        { "FR", "DE", "US", "GB", "JP", "CH", "IT", "ES", "NL", "SE", "BR", "IN", "CN", "AU", "CA" };
    private static readonly string[] Currencies =
        { "EUR", "USD", "GBP", "JPY", "CHF", "CAD", "AUD", "CNY", "BRL", "INR", "SEK", "HKD" };
    private static readonly string[] AssetClasses =
        { "EQUITY", "GOVT_BOND", "CORP_BOND", "FX_SPOT", "FUTURE", "OPTION" };
    private static readonly string[] Sectors =
        { "Financials", "Energy", "Tech", "Healthcare", "Utilities",
          "Consumer", "Industrial", "Materials", "Telecom", "RealEstate", "Govt" };
    private static readonly string[] Desks =
        { "Cash-Equity", "Equity-Derivs", "Rates", "Credit", "FX-Spot",
          "FX-Options", "Commodities", "Futures", "Repo", "PrimeBrokerage" };
    private static readonly string[] CpTypes = { "BANK", "BROKER", "HF", "PENSION", "INSURER" };
    private static readonly string[] AcctTypes = { "INTERNAL", "CLIENT", "PROP", "FUND" };
    private static readonly string[] Sides = { "BUY", "SELL" };
    private static readonly string[] Mics =
        { "XNYS", "XNAS", "XPAR", "XLON", "XETR", "XTKS", "XSWX",
          "XMIL", "XAMS", "XBRU", "XBOM", "XSHG", "XHKG", "XASX",
          "XTSE", "XBME", "XOSL", "XCSE", "XHEL", "XSTO",
          "XJSE", "XNSE", "XKRX", "XSGX", "XBA1" };

    private static IEnumerable<TradeTicketRow> GenerateTrades()
    {
        var origin = new DateTime(2024, 1, 1);
        for (int i = 1; i <= NbTrades; i++)
        {
            int sec = ((i * 7) % NbSecurities) + 1;
            int acct = ((i * 11) % NbAccounts) + 1;
            int book = ((i * 13) % NbBooks) + 1;
            int trader = ((i * 17) % NbTraders) + 1;
            int cp = ((i * 19) % NbCounterparties) + 1;
            int desk = ((i * 23) % NbDesks);
            int ccyIdx = (sec % NbCurrencies);
            int micIdx = (sec % NbExchanges);
            int countryIdx = (sec % NbCountries);
            int acIdx = (sec % NbAssetClasses);
            int secIdx = (sec % NbSectors);
            int indIdx = (sec % NbIndustries);
            decimal qty = ((i % 1000) + 1) * 10m;
            decimal price = 1m + ((i * 31) % 9999) / 100m;
            decimal gross = qty * price;
            decimal commission = Math.Round(gross * 0.0005m, 2);
            decimal fees = Math.Round(gross * 0.0001m, 2);
            decimal tax = Math.Round(gross * 0.0002m, 2);
            string side = Sides[i % 2];
            decimal net = gross + commission + fees + tax;
            yield return new TradeTicketRow
            {
                TradeId = $"TRD-{i:D8}",
                TradeDate = origin.AddMinutes(i),
                SettleDate = origin.AddDays(2).AddMinutes(i),
                BookingDate = origin.AddMinutes(i),
                AccountCode = $"ACC-{acct:D4}",
                AccountName = $"Account {acct}",
                AccountType = AcctTypes[acct % AcctTypes.Length],
                BookCode = $"BK-{book:D3}",
                BookName = $"Book {book}",
                Desk = Desks[desk],
                BusinessLine = desk < 5 ? "Markets" : "TreasuryAndOther",
                TraderId = $"TR-{trader:D3}",
                TraderName = $"Trader {trader}",
                TraderEmail = $"tr{trader}@example.com",
                CounterpartyCode = $"CP-{cp:D3}",
                CounterpartyName = $"Counterparty {cp}",
                CounterpartyLEI = $"LEI{cp:D17}",
                CounterpartyCountry = Countries[cp % NbCountries],
                CounterpartyType = CpTypes[cp % CpTypes.Length],
                Isin = $"ISIN{sec:D8}",
                Cusip = $"CUS{sec:D6}",
                Ticker = $"TKR{sec:D4}",
                SecurityName = $"Security {sec}",
                AssetClass = AssetClasses[acIdx],
                Sector = Sectors[secIdx],
                Industry = $"Industry-{indIdx:D2}",
                IssuerCountry = Countries[countryIdx],
                Currency = Currencies[ccyIdx],
                ExchangeMic = Mics[micIdx],
                Side = side,
                Quantity = qty,
                Price = price,
                GrossAmount = gross,
                Commission = commission,
                Fees = fees,
                Tax = tax,
                NetAmount = net,
                ClearingHouse = micIdx % 2 == 0 ? "LCH" : "DTCC",
                CustodianCode = $"CUST-{(acct % 5):D2}",
            };
        }
    }

    private static IEnumerable<PositionRow> GeneratePositions()
    {
        var origin = new DateTime(2024, 1, 1);
        for (int i = 1; i <= NbPositions; i++)
        {
            int sec = ((i * 5) % NbSecurities) + 1;
            int acct = ((i * 3) % NbAccounts) + 1;
            int ccyIdx = (sec % NbCurrencies);
            decimal qty = ((i % 5000) + 1) * 1m;
            decimal price = 1m + ((i * 41) % 9999) / 100m;
            decimal mv = qty * price;
            decimal fx = ccyIdx == 0 ? 1m : 1m + (ccyIdx * 0.05m);
            yield return new PositionRow
            {
                AccountCode = $"ACC-{acct:D4}",
                Isin = $"ISIN{sec:D8}",
                Date = origin.AddDays(i % 60),
                Quantity = qty,
                MarketPrice = price,
                MarketValue = mv,
                BookValue = mv * 0.98m,
                UnrealizedPnL = mv * 0.02m,
                AccruedInterest = mv * 0.001m,
                Currency = Currencies[ccyIdx],
                FxRate = fx,
            };
        }
    }

    // ---------------------------------------------------------------------
    // The actual test: builds the same TestImport-shaped pipeline at scale
    // and asserts no deadlock + correct cardinalities + correct totals.
    // ---------------------------------------------------------------------

    [Fact]
    public async Task HeavyFinanceNormalizationPipeline_NoDeadlock_AllBranchesAndCorrelationsConsistent()
    {
        // Counters
        int countryCount = 0, currencyCount = 0, exchangeCount = 0,
            assetClassCount = 0, sectorCount = 0, industryCount = 0,
            deskCount = 0, accountCount = 0, bookCount = 0, traderCount = 0,
            counterpartyCount = 0, securityCount = 0,
            tradeFactCount = 0, positionFactCount = 0;
        decimal tradeNotional = 0m;
        decimal positionMvBase = 0m;

        var lck = new object();
        void AddDecimal(ref decimal target, decimal value)
        {
            lock (lck) target += value;
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(0, root =>
        {
            // ---- two correlated source streams (mirroring TestImport's
            //      "Get portfolio files / Get position files" pair) ----
            var trades = root
                .CrossApply("Materialize trade tickets", _ => GenerateTrades())
                .SetForCorrelation("Correlate trade row");

            var positions = root
                .CrossApply("Materialize positions", _ => GeneratePositions())
                .SetForCorrelation("Correlate position row");

            // ---- 6 simple lookup-style dimensions extracted from trades ----
            // NOTE: lambdas on Correlated streams (Distinct/Select/Lookup/
            // CorrelateToSingle/DoCorrelated) operate on the UNWRAPPED row;
            // the Correlated wrapper is preserved automatically.
            var countryStream = trades
                .Distinct("Distinct issuer country", t => t.IssuerCountry)
                .Select("Project country", t => new CountryEntity { Code = t.IssuerCountry })
                .DoCorrelated("count countries", _ => Interlocked.Increment(ref countryCount));

            var currencyStream = trades
                .Distinct("Distinct currency", t => t.Currency)
                .Select("Project currency", t => new CurrencyEntity { Code = t.Currency })
                .DoCorrelated("count currencies", _ => Interlocked.Increment(ref currencyCount));

            var exchangeStream = trades
                .Distinct("Distinct exchange", t => t.ExchangeMic)
                .Select("Project exchange", t => new ExchangeEntity { Mic = t.ExchangeMic })
                .DoCorrelated("count exchanges", _ => Interlocked.Increment(ref exchangeCount));

            var assetClassStream = trades
                .Distinct("Distinct asset class", t => t.AssetClass)
                .Select("Project asset class", t => new AssetClassEntity { Name = t.AssetClass })
                .DoCorrelated("count asset classes", _ => Interlocked.Increment(ref assetClassCount));

            var sectorStream = trades
                .Distinct("Distinct sector", t => t.Sector)
                .Select("Project sector", t => new SectorEntity { Name = t.Sector })
                .DoCorrelated("count sectors", _ => Interlocked.Increment(ref sectorCount));

            var industryStream = trades
                .Distinct("Distinct industry", t => t.Industry)
                .Select("Project industry", t => new IndustryEntity { Name = t.Industry })
                .DoCorrelated("count industries", _ => Interlocked.Increment(ref industryCount));

            var deskStream = trades
                .Distinct("Distinct desk", t => t.Desk)
                .Select("Project desk", t => new DeskEntity
                {
                    Name = t.Desk,
                    BusinessLine = t.BusinessLine
                })
                .DoCorrelated("count desks", _ => Interlocked.Increment(ref deskCount));

            // ---- account / book / trader / counterparty / security ----
            var accountStream = trades
                .Distinct("Distinct account", t => t.AccountCode)
                .Select("Project account", t => new AccountEntity
                {
                    Code = t.AccountCode,
                    Name = t.AccountName,
                    Type = t.AccountType,
                })
                .DoCorrelated("count accounts", _ => Interlocked.Increment(ref accountCount));

            var bookStream = trades
                .Distinct("Distinct book", t => t.BookCode)
                .Select("Project book", t => new BookEntity
                {
                    Code = t.BookCode,
                    Name = t.BookName,
                    AccountCode = t.AccountCode,
                    DeskName = t.Desk,
                })
                .DoCorrelated("count books", _ => Interlocked.Increment(ref bookCount));

            var traderStream = trades
                .Distinct("Distinct trader", t => t.TraderId)
                .Select("Project trader", t => new TraderEntity
                {
                    Id = t.TraderId,
                    Name = t.TraderName,
                    Email = t.TraderEmail,
                    DeskName = t.Desk,
                })
                .DoCorrelated("count traders", _ => Interlocked.Increment(ref traderCount));

            var counterpartyStream = trades
                .Distinct("Distinct counterparty", t => t.CounterpartyCode)
                .Select("Project counterparty", t => new CounterpartyEntity
                {
                    Code = t.CounterpartyCode,
                    Name = t.CounterpartyName,
                    Lei = t.CounterpartyLEI,
                    Country = t.CounterpartyCountry,
                    Type = t.CounterpartyType,
                })
                .DoCorrelated("count counterparties", _ => Interlocked.Increment(ref counterpartyCount));

            // Securities seen in BOTH source streams: build it from trades
            // (richer schema), and we'll Lookup it from the position stream.
            var securityStream = trades
                .Distinct("Distinct security", t => t.Isin)
                .Select("Project security", t => new SecurityEntity
                {
                    Isin = t.Isin,
                    Cusip = t.Cusip,
                    Ticker = t.Ticker,
                    Name = t.SecurityName,
                    AssetClass = t.AssetClass,
                    Sector = t.Sector,
                    Industry = t.Industry,
                    IssuerCountry = t.IssuerCountry,
                    Currency = t.Currency,
                    ExchangeMic = t.ExchangeMic,
                })
                .DoCorrelated("count securities", _ => Interlocked.Increment(ref securityCount));

            // Force the dimension branches to actually run even though the
            // simple ones (country/currency/...) aren't joined back. Without
            // a downstream consumer they would still execute, but we bind
            // them to a sink to make their completion observable.
            countryStream.DoCorrelated("sink country", _ => { });
            currencyStream.DoCorrelated("sink currency", _ => { });
            exchangeStream.DoCorrelated("sink exchange", _ => { });
            assetClassStream.DoCorrelated("sink asset class", _ => { });
            sectorStream.DoCorrelated("sink sector", _ => { });
            industryStream.DoCorrelated("sink industry", _ => { });
            deskStream.DoCorrelated("sink desk", _ => { });

            // ---- TRADE FACT: 5 chained CorrelateToSingle to reunite branches ----
            // Mirrors TestImport's pattern of "row + dim1, row + dim1 + dim2 ..."
            // but with five dimensions instead of two.
            trades
                .CorrelateToSingle("trade × account", accountStream,
                    (row, acc) => new { Row = row, AccountCode = acc.Code })
                .CorrelateToSingle("trade × book", bookStream,
                    (x, bk) => new { x.Row, x.AccountCode, BookCode = bk.Code })
                .CorrelateToSingle("trade × trader", traderStream,
                    (x, tr) => new { x.Row, x.AccountCode, x.BookCode, TraderId = tr.Id })
                .CorrelateToSingle("trade × counterparty", counterpartyStream,
                    (x, cp) => new
                    {
                        x.Row,
                        x.AccountCode,
                        x.BookCode,
                        x.TraderId,
                        CounterpartyCode = cp.Code,
                    })
                .CorrelateToSingle("trade × security", securityStream,
                    (x, sec) => new TradeFact
                    {
                        TradeId = x.Row.TradeId,
                        TradeDate = x.Row.TradeDate,
                        AccountCode = x.AccountCode,
                        BookCode = x.BookCode,
                        TraderId = x.TraderId,
                        CounterpartyCode = x.CounterpartyCode,
                        Isin = sec.Isin,
                        Side = x.Row.Side,
                        Quantity = x.Row.Quantity,
                        NetAmount = x.Row.NetAmount,
                        SignedNotional = x.Row.Side == "BUY"
                            ? x.Row.NetAmount
                            : -x.Row.NetAmount,
                    })
                .DoCorrelated("count trade facts", f =>
                {
                    Interlocked.Increment(ref tradeFactCount);
                    AddDecimal(ref tradeNotional, f.SignedNotional);
                });

            // ---- POSITION FACT: Lookup-based join on a correlated right
            // side. Position rows carry only AccountCode + ISIN strings;
            // we re-join them to the account & security dimensions via
            // explicit key selectors (not correlation tokens). Lookup on
            // a Correlated × Correlated pair preserves the left correlation.
            positions
                .Lookup("position × account by code",
                    accountStream, p => p.AccountCode, a => a.Code,
                    (p, a) => new { Row = p, AccountCode = a.Code })
                .Lookup("position × security by ISIN",
                    securityStream, p => p.Row.Isin, s => s.Isin,
                    (p, s) => new PositionFact
                    {
                        AccountCode = p.AccountCode,
                        Isin = s.Isin,
                        Date = p.Row.Date,
                        MarketValueBase = p.Row.MarketValue * p.Row.FxRate,
                        Currency = p.Row.Currency,
                    })
                .DoCorrelated("count position facts", f =>
                {
                    Interlocked.Increment(ref positionFactCount);
                    AddDecimal(ref positionMvBase, f.MarketValueBase);
                });
        });

        sw.Stop();

        Assert.False(status.Failed,
            "Pipeline failed (possible deadlock or operator error): "
            + status.ErrorTraceEvent?.ToString());

        // Cardinalities must match the LINQ reference.
        // Stream through generators (no full materialisation — avoids multi-GB
        // heap allocations that would disturb concurrent memory-leak tests).
        var expCountries     = new HashSet<string>();
        var expCurrencies    = new HashSet<string>();
        var expExchanges     = new HashSet<string>();
        var expAssetClasses  = new HashSet<string>();
        var expSectors       = new HashSet<string>();
        var expIndustries    = new HashSet<string>();
        var expDesks         = new HashSet<string>();
        var expAccounts      = new HashSet<string>();
        var expBooks         = new HashSet<string>();
        var expTraders       = new HashSet<string>();
        var expCps           = new HashSet<string>();
        var expSecurities    = new HashSet<string>();
        decimal expectedNotional = 0m;
        foreach (var t in GenerateTrades())
        {
            expCountries.Add(t.IssuerCountry);
            expCurrencies.Add(t.Currency);
            expExchanges.Add(t.ExchangeMic);
            expAssetClasses.Add(t.AssetClass);
            expSectors.Add(t.Sector);
            expIndustries.Add(t.Industry);
            expDesks.Add(t.Desk);
            expAccounts.Add(t.AccountCode);
            expBooks.Add(t.BookCode);
            expTraders.Add(t.TraderId);
            expCps.Add(t.CounterpartyCode);
            expSecurities.Add(t.Isin);
            expectedNotional += t.Side == "BUY" ? t.NetAmount : -t.NetAmount;
        }
        decimal expectedMv = GeneratePositions().Sum(p => p.MarketValue * p.FxRate);

        Assert.Equal(expCountries.Count,    countryCount);
        Assert.Equal(expCurrencies.Count,   currencyCount);
        Assert.Equal(expExchanges.Count,    exchangeCount);
        Assert.Equal(expAssetClasses.Count, assetClassCount);
        Assert.Equal(expSectors.Count,      sectorCount);
        Assert.Equal(expIndustries.Count,   industryCount);
        Assert.Equal(expDesks.Count,        deskCount);
        Assert.Equal(expAccounts.Count,     accountCount);
        Assert.Equal(expBooks.Count,        bookCount);
        Assert.Equal(expTraders.Count,      traderCount);
        Assert.Equal(expCps.Count,          counterpartyCount);
        Assert.Equal(expSecurities.Count,   securityCount);

        // Fact rows must match input volumes (no row dropped or duplicated).
        Assert.Equal(NbTrades, tradeFactCount);
        Assert.Equal(NbPositions, positionFactCount);

        // Aggregate totals must match LINQ reference.
        Assert.Equal(expectedNotional, tradeNotional);
        Assert.Equal(expectedMv, positionMvBase);

        // Heavy normalization must complete in reasonable time. With a
        // deadlock the test would block until the xUnit timeout; we keep
        // a generous upper bound to avoid CI flakes.
        Assert.True(sw.Elapsed < TimeSpan.FromMinutes(30),
            $"Pipeline took {sw.Elapsed} — likely a slowdown or contention.");
    }
}
