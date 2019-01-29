using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Core;
using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.Recipes.DefineProcess.Jobs;
using Paillave.Etl.Recipes.DefineProcess.StreamTypes.Config;

namespace Paillave.Etl.Recipes.DefineProcess
{
    [TestClass]
    public class SimpleJobThrowException
    {
        #region InlineMethodWay
        [TestMethod]
        public void InlineMethodWay()
        {
            var config = new SimpleConfigStreamType { Divider = 0 };
            var task = StreamProcessRunner.CreateAndExecuteAsync(config, SimpleJob.Job1);
            try
            {
                task.Wait();
                Assert.Fail("the execution should not be successfull");
            }
            catch (AggregateException ex)
            {
                JobExecutionException jobExecutionException = ex.InnerException as JobExecutionException;
                if (jobExecutionException == null) throw;
                Assert.IsInstanceOfType(jobExecutionException.InnerException, typeof(DivideByZeroException));
                Assert.IsInstanceOfType((jobExecutionException.TraceEvent?.Content as UnhandledExceptionStreamTraceContent)?.Exception, typeof(DivideByZeroException));
            }
        }
        #endregion

        #region StaticMethodWay
        [TestMethod]
        public void StaticMethodWay()
        {
            var runner = StreamProcessRunner.Create<SimpleConfigStreamType>(SimpleJob.Job1);
            var config = new SimpleConfigStreamType { Divider = 0 };
            var task = runner.ExecuteAsync(config);
            try
            {
                task.Wait();
                Assert.Fail("the execution should not be successfull");
            }
            catch (AggregateException ex)
            {
                JobExecutionException jobExecutionException = ex.InnerException as JobExecutionException;
                if (jobExecutionException == null) throw;
                Assert.IsInstanceOfType(jobExecutionException.InnerException, typeof(DivideByZeroException));
                Assert.IsInstanceOfType((jobExecutionException.TraceEvent?.Content as UnhandledExceptionStreamTraceContent)?.Exception, typeof(DivideByZeroException));
            }
        }
        #endregion

        #region InstanceMethodWay
        [TestMethod]
        public void InstanceMethodWay()
        {
            var runner = new StreamProcessRunner<SimpleConfigStreamType>(SimpleJob.Job1);
            var config = new SimpleConfigStreamType { Divider = 0 };
            var task = runner.ExecuteAsync(config);
            try
            {
                task.Wait();
                Assert.Fail("the execution should not be successfull");
            }
            catch (AggregateException ex)
            {
                JobExecutionException jobExecutionException = ex.InnerException as JobExecutionException;
                if (jobExecutionException == null) throw;
                Assert.IsInstanceOfType(jobExecutionException.InnerException, typeof(DivideByZeroException));
                Assert.IsInstanceOfType((jobExecutionException.TraceEvent?.Content as UnhandledExceptionStreamTraceContent)?.Exception, typeof(DivideByZeroException));
            }
        }
        #endregion
    }
}
