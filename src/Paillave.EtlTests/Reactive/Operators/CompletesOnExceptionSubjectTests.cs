using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass()]
    public class CompletesOnExceptionSubjectTests
    {
        [TestCategory(nameof(CompletesOnExceptionSubjectTests))]
        [TestMethod]
        public void PushSimpleError()
        {
            var errors = new List<Exception>();
            var values = new List<int>();
            var tmp = new PushSubject<int>();
            tmp.Subscribe(values.Add, () => { }, errors.Add);
            var task = tmp.CompletesOnException(errors.Add).ToTaskAsync();
            var exception = new Exception();
            tmp.PushException(exception);
            Assert.AreSame(exception, errors[0], "the exception should be retrieved");
            Assert.IsTrue(task.Wait(5000), "the stream should be completed");
        }
    }
}