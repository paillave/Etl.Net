using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl;
using Paillave.Etl.Extensions;

namespace Paillave.EtlTests
{
    [TestClass()]
    public class StreamProcessRunnerTests
    {
        [TestCategory(nameof(StreamProcessRunnerTests))]
        [TestMethod]
        public void OneStreamWithErrorOnCrossApplyTest()
        {
            var task = StreamProcessRunner.CreateAndExecuteAsync(0,
            configStream =>
            {
                configStream
                    .CrossApplyEnumerable("lst1", config => new[] { 1 })
                    .Select("with error", i =>
                    {
                        // throw new Exception();
                        return i;
                    })
                    .ThroughAction("res1", Console.WriteLine);
                configStream
                    .CrossApplyEnumerable("lst2", config => new[] { 1 })
                    .Select("with no error", i =>
                    {
                        throw new Exception();
                        return i;
                    })
                    .ThroughAction("res2", Console.WriteLine);
            }, traceStream => traceStream.ThroughAction("trace", i => System.Diagnostics.Debug.WriteLine(i)));
            try
            {
                System.Diagnostics.Debug.WriteLine("before");
                task.Wait(2000);
                System.Diagnostics.Debug.WriteLine("after");
            }
            catch (Exception ex)
            {
                var jex = ex.InnerException as Paillave.Etl.Core.JobExecutionException;
                var te = jex.TraceEvent;
            }
        }

        [TestCategory(nameof(StreamProcessRunnerTests))]
        [TestMethod]
        public void OneStreamWithErrorTest()
        {
            var task = StreamProcessRunner.CreateAndExecuteAsync(0,
            configStream =>
            {
                configStream
                    .Select("with error", i =>
                    {
                        throw new Exception();
                        return i;
                    })
                    .ThroughAction("res1", Console.WriteLine);
                configStream
                    .Select("with no error", i =>
                    {
                        //  throw new Exception();
                        return i;
                    })
                    .ThroughAction("res2", Console.WriteLine);
            });
            try
            {
                task.Wait(5000);
            }
            catch (Exception ex)
            {
                var jex = ex.InnerException as Paillave.Etl.Core.JobExecutionException;
                var te = jex.TraceEvent;
            }
        }
    }
}