using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.EntityFrameworkCore;
using Paillave.Etl.Extensions;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.TextFile.Core;
using Paillave.Etl.TextFile;
using Xunit;

namespace Paillave.Etl.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var sw = new Stopwatch();
            var streamProcessRunner = StreamProcessRunner.Create(
                (ISingleStream<int> configStream) => configStream
                    .CrossApplyEnumerable<int, int>("test", i => Enumerable.Range(0, i))
                    .Observable.Count().Do(i => Console.WriteLine($"Count={i}")));

            sw.Start();
            Console.WriteLine("Starting...");
            var tmp = streamProcessRunner.ExecuteAsync(2);
            Console.WriteLine("Started");
            tmp.Wait();
            Console.WriteLine($"Done {!tmp.Result.Failed}");
        }
        private void Tmp(ISingleStream<int> processContextStream)
        {
            var efaNavFileDefinition = FlatFileDefinition.Create(i => new
            {
                ShareCode = i.ToColumn<string>("Share_code"),
                SubFundCode = i.ToColumn<string>("Sub-fund_code"),
                IsinCode = i.ToColumn<string>("Isin_code"),
                ShareCurrency = i.ToColumn<string>("CCY_NAV_share"),
                NavDate = i.ToDateColumn("Valuation_date", "dd/MM/yyyy"),
                NavPerShare = i.ToNumberColumn<double>("NAV_share", "."),
                NumberOfSharesOutstanding = i.ToNumberColumn<double>("Numbre_of_Shares_outstanding", "."),
                TotalNetAsset = i.ToNumberColumn<double>("Total_Net_Assets", "."),
                NetAssetShareType = i.ToNumberColumn<double>("Net_assets_share_type", "."),
            }).IsColumnSeparated(',');

            var fileStream = processContextStream
                .CrossApplyFolderFiles($"get local files", i => "/home/stephane/Desktop/zret", "*.csv", false);
            var navFileStream = fileStream
                .CrossApplyTextFile($"parse nav file", efaNavFileDefinition);

            var managedSubFundStream = processContextStream.EfCoreSelect($"get subfunds from db", i => i.Set<SubFund>());
        }
    }
    public class SubFund
    {

    }
}
