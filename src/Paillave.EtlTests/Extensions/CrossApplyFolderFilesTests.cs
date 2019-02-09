using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl;
using Paillave.Etl.Core;
using Paillave.Etl.Extensions;
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.Extensions
{
    [TestClass()]
    public class CrossApplyFolderFilesTests
    {
        private const string TestFolder = nameof(Paillave.Etl.Extensions) + "." + nameof(CrossApplyFolderFilesTests);
        [TestInitialize]
        public void TestInitialize()
        {
            var tmpPath = Path.GetTempPath();
            var testPath = Path.Combine(tmpPath, TestFolder);
            if (Directory.Exists(testPath))
                Directory.Delete(testPath, true);
            Directory.CreateDirectory(testPath);
            for (int i = 0; i < 2; i++)
            {
                File.Create(Path.Combine(testPath, $"file{i}")).Dispose();
                var folderPath = Path.Combine(testPath, i.ToString());
                Directory.CreateDirectory(folderPath);
                for (int j = 0; j < 4; j++)
                {
                    File.Create(Path.Combine(folderPath, $"{i}-{j}")).Dispose();
                }
            }
        }

        [TestCleanup()]
        public void TestCleanup()
        {
            var tmpPath = Path.GetTempPath();
            Directory.Delete(Path.Combine(tmpPath, TestFolder), true);
        }

        [TestCategory(nameof(CrossApplyFolderFilesTests))]
        [TestMethod]
        public void GetFilesFromFolder()
        {
            var tmpPath = Path.GetTempPath();
            var testPath = Path.Combine(tmpPath, TestFolder);
            var outputList = new List<LocalFilesValue>();

            StreamProcessRunner.CreateAndExecuteAsync(testPath, rootStream =>
            {
                rootStream
                    .CrossApplyFolderFiles("get all files")
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            var expected = Enumerable.Range(0, 2).Select(i => Path.Combine(testPath, $"file{i}")).ToList();
            var actual = outputList.Select(i => i.Name).ToList();
            CollectionAssert.AreEquivalent(expected, actual);
        }

        [TestCategory(nameof(CrossApplyFolderFilesTests))]
        [TestMethod]
        public void GetFilesFromFolderRecursive()
        {
            var tmpPath = Path.GetTempPath();
            var testPath = Path.Combine(tmpPath, TestFolder);
            var outputList = new List<LocalFilesValue>();

            StreamProcessRunner.CreateAndExecuteAsync(testPath, rootStream =>
            {
                rootStream
                    .CrossApplyFolderFiles("get all files", "*", true)
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            var expected = Enumerable.Range(0, 2).Select(i => Path.Combine(testPath, $"file{i}")).Union(Enumerable.Range(0, 2).SelectMany(i => Enumerable.Range(0, 4).Select(j => Path.Combine(testPath, $"{i}", $"{i}-{j}")))).ToList();
            var actual = outputList.Select(i => i.Name).ToList();
            CollectionAssert.AreEquivalent(expected, actual);
        }
    }
}
