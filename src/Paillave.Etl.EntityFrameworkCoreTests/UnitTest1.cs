using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.EntityFrameworkCore.BulkSave;
using Paillave.Etl.EntityFrameworkCore.Core;

namespace Paillave.Etl.EntityFrameworkCoreTests
{
    [TestClass]
    public class UnitTest1
    {
        private class Class1
        {
            public int MyProperty1 { get; set; }
            public string MyProperty2 { get; set; }
            public DateTime MyProperty3 { get; set; }
        }
        private class Class2
        {
            public int MyProperty4 { get; set; }
            public string MyProperty5 { get; set; }
            public DateTime MyProperty6 { get; set; }
        }
        private class Class3
        {
            public int MyProperty7 { get; set; }
            public string MyProperty8 { get; set; }
            public DateTime MyProperty9 { get; set; }
            public Class2 MyProperty10 { get; set; }
        }
        [TestMethod]
        public void TestMethod1()
        {
            var lst1 = Enumerable.Range(1, 100).Select(i => new Class1
            {
                MyProperty1 = i,
                MyProperty2 = i.ToString(),
                MyProperty3 = new DateTime(2000, 1, 1).AddDays(i)
            }).AsQueryable();
            var mcb = MatchCriteriaBuilder.Create(
                (Class3 i) => new { A = i.MyProperty10.MyProperty4, B = i.MyProperty8 },
                (Class1 i) => new { A = i.MyProperty1, B = i.MyProperty2 }
                );
            var p = new Class3 { MyProperty10 = new Class2 { MyProperty4 = 1 }, MyProperty8 = "1" };
            var ce = mcb.GetCriteriaExpression(p);
            var ceT = ((Expression<Func<Class1, bool>>)((Class1 i) => i.MyProperty1 == p.MyProperty10.MyProperty4 && i.MyProperty2 == p.MyProperty8));
            var res = lst1.FirstOrDefault(ce);
            Assert.AreEqual(new DateTime(2000, 1, 1).AddDays(1), res.MyProperty3);
        }
        [TestMethod]
        public void TestMethodWithDefaultCriteria1()
        {
            var lst1 = Enumerable.Range(1, 100).Select(i => new Class1
            {
                MyProperty1 = i,
                MyProperty2 = i.ToString(),
                MyProperty3 = new DateTime(2000, 1, 1).AddDays(i)
            }).AsQueryable();
            var mcb = MatchCriteriaBuilder.Create(
                (Class3 i) => new { A = i.MyProperty10.MyProperty4, B = i.MyProperty8 },
                (Class1 i) => new { A = i.MyProperty1, B = i.MyProperty2 },
                i => i.MyProperty1 % 2 == 0
                );
            var ce = mcb.GetCriteriaExpression(new Class3 { MyProperty10 = new Class2 { MyProperty4 = 1 }, MyProperty8 = "1" });

            var res = lst1.FirstOrDefault(ce);
            Assert.AreEqual(null, res);

            ce = mcb.GetCriteriaExpression(new Class3 { MyProperty10 = new Class2 { MyProperty4 = 2 }, MyProperty8 = "2" });
            res = lst1.FirstOrDefault(ce);
            Assert.AreEqual(new DateTime(2000, 1, 1).AddDays(2), res.MyProperty3);
        }
        [TestMethod]
        public void TestGetSetters()
        {
            var setters = SettersExtractor.GetGetters((Class1 u) => new Class2
            {
                MyProperty4 = u.MyProperty1,
                MyProperty5 = u.MyProperty2
            });
            CollectionAssert.AreEquivalent(new[] { "MyProperty4", "MyProperty5" }, setters.Keys.ToArray());
            CollectionAssert.AreEquivalent(new[] { typeof(Class1).GetMember("MyProperty1")[0], typeof(Class1).GetMember("MyProperty2")[0] }, setters.Values.ToArray());
        }
    }
}
