using Paillave.Etl.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Paillave.Etl.FileSystem
{
    public class FileSystemValuesProviderArgs<TIn, TOut>
    {
        public Func<TIn, string> GetFolderPath { get; set; }
        public Func<TIn, string> GetSearchPattern { get; set; }
        public bool Recursive { get; set; } = false;
        public Func<IFileValue, TIn, TOut> GetResult { get; set; }
    }
    public class FileSystemValuesProvider<TIn, TOut>(FileSystemValuesProviderArgs<TIn, TOut> args) : ValuesProviderBase<TIn, TOut>
    {
        private FileSystemValuesProviderArgs<TIn, TOut> _args = args;

        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        public override void PushValues(TIn input, Action<TOut> pushValue, CancellationToken cancellationToken, IExecutionContext context)
        {
            var rootFolder = _args.GetFolderPath(input);
            var searchPattern = _args.GetSearchPattern == null ? "*" : _args.GetSearchPattern(input);
            var files = Directory
                .GetFiles(rootFolder, searchPattern, _args.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .ToList();
            var isRootedPath = Path.IsPathRooted(rootFolder);
            foreach (var relativePath in files)
            {
                if (cancellationToken.IsCancellationRequested) break;
                if (isRootedPath)
                    pushValue(_args.GetResult(new FileSystemFileValue(new FileInfo(Path.Combine(rootFolder, relativePath ?? ""))), input));
                else
                    pushValue(_args.GetResult(new FileSystemFileValue(new FileInfo(relativePath ?? "")), input));
            }
        }
    }
}
