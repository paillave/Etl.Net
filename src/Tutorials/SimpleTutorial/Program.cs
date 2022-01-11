using System;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Zip;
using Paillave.Etl.TextFile;
using Paillave.Etl.SqlServer;
using System.Data.SqlClient;
using System.Linq;

namespace SimpleTutorial
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess1);
            processRunner.DebugNodeStream += (sender, e) => { };
            var res = await processRunner.ExecuteAsync(args[0]);
            Console.Write(res.Failed ? "Failed" : "Succeeded");
            if (res.Failed)
                Console.Write($"{res.ErrorTraceEvent.NodeName}({res.ErrorTraceEvent.NodeTypeName}):{res.ErrorTraceEvent.Content.Message}");
        }
        private static void DefineProcess1(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApplyFolderFiles("get files", "*.txt")
                .CrossApply("parse file", BloombergValuesProvider.Create(FlatFileDefinition.Create(i => new
                {
                    CountryFullName = i.ToColumn("COUNTRY_FULL_NAME"),
                    IndustrySector = i.ToColumn("INDUSTRY_SECTOR"),
                    IndustrySectorNum = i.ToColumn("INDUSTRY_SECTOR_NUM"),
                    EqyShOut = i.ToNumberColumn<double?>("EQY_SH_OUT", "."),
                    EqyShOutActual = i.ToNumberColumn<double?>("EQY_SH_OUT_ACTUAL", "."),
                    EqyFloat = i.ToNumberColumn<double?>("EQY_FLOAT", "."),
                    CrncyAdjMktCap = i.ToNumberColumn<double?>("CRNCY_ADJ_MKT_CAP", "."),
                    HisVolOnPx1 = i.ToNumberColumn<double?>("HIS_VOL_ON_PX:1", "."),
                    HisVolOnPx2 = i.ToNumberColumn<double?>("HIS_VOL_ON_PX:2", "."),
                    MhisCloseOnPx3 = i.ToNumberColumn<double?>("MHIS_CLOSE_ON_PX:3", "."),
                    MhisCloseOnPx4 = i.ToNumberColumn<double?>("MHIS_CLOSE_ON_PX:4", "."),
                    CountryIso = i.ToColumn("COUNTRY_ISO"),
                    DvdExDt = i.ToOptionalDateColumn("DVD_EX_DT", "MM/dd/yyyy"),
                    DvdFreq = i.ToColumn("DVD_FREQ"),
                    EqyDvdRightExDtCurr = i.ToColumn("EQY_DVD_RIGHT_EX_DT_CURR"),
                    SecurityTyp = i.ToColumn("SECURITY_TYP"),
                    PeRatio = i.ToNumberColumn<double?>("PE_RATIO", "."),
                    EqyDvdYld12m = i.ToNumberColumn<double?>("EQY_DVD_YLD_12M", "."),
                })))
                .Do("write to console", i =>
                {

                });
        }

        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApplyFolderFiles("list all required files", "*.zip", true)
                .CrossApplyZipFiles("extract files from zip", "*.csv")
                .CrossApplyTextFile("parse file", FlatFileDefinition.Create(i => new Person
                {
                    Email = i.ToColumn("email"),
                    FirstName = i.ToColumn("first name"),
                    LastName = i.ToColumn("last name"),
                    DateOfBirth = i.ToDateColumn("date of birth", "yyyy-MM-dd"),
                    Reputation = i.ToNumberColumn<int?>("reputation", ".")
                }).IsColumnSeparated(','))
                .Distinct("exclude duplicates based on the Email", i => i.Email)
                .SqlServerSave("upsert using Email as key and ignore the Id", o => o
                    .ToTable("dbo.Person")
                    .SeekOn(p => p.Email)
                    .DoNotSave(p => p.Id))
                .Select("define row to report", i => new { i.Email, i.Id })
                .ToTextFileValue("write summary to file", "report.csv", FlatFileDefinition.Create(i => new
                {
                    Email = i.ToColumn("Email"),
                    Id = i.ToNumberColumn<int>("new or existing Id", ".")
                }).IsColumnSeparated(','))
                .WriteToFile("save log file", i => i.Name);
        }
        private class Person
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime DateOfBirth { get; set; }
            public int? Reputation { get; set; }
        }
    }
}
