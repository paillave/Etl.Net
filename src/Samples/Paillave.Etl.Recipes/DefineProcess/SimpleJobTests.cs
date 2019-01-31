using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Recipes.DefineProcess.Jobs;
using Paillave.Etl.Recipes.DefineProcess.StreamTypes.Config;

namespace Paillave.Etl.Recipes.DefineProcess
{
    [TestClass]
    public class SimpleJobTests
    {
        #region InlineMethodWay
        [TestMethod]
        public void InlineMethodWay()
        {
            var config = new SimpleConfigStreamType { Divider = 10 };
            var task = StreamProcessRunner.CreateAndExecuteAsync(config, SimpleJob.Job1);
            task.Wait();
            CollectionAssert.AreEquivalent(new[] { $"{100 / 10} times hello world!" }, config.Messages.ToArray());
        }
        #endregion

        #region StaticMethodWay
        [TestMethod]
        public void StaticMethodWay()
        {
            var runner = StreamProcessRunner.Create<SimpleConfigStreamType>(SimpleJob.Job1);
            var config = new SimpleConfigStreamType { Divider = 10 };
            var task = runner.ExecuteAsync(config);
            task.Wait();
            CollectionAssert.AreEquivalent(new[] { $"{100 / 10} times hello world!" }, config.Messages.ToArray());
        }
        #endregion

        #region InstanceMethodWay
        [TestMethod]
        public void InstanceMethodWay()
        {
            var runner = new StreamProcessRunner<SimpleConfigStreamType>(SimpleJob.Job1);
            var config = new SimpleConfigStreamType { Divider = 10 };
            var task = runner.ExecuteAsync(config);
            task.Wait();
            CollectionAssert.AreEquivalent(new[] { $"{100 / 10} times hello world!" }, config.Messages.ToArray());
        }
        #endregion
    }
}
