using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.ValuesProviders;
using Renci.SshNet;
using System;
using System.IO;

namespace Paillave.Etl.Sftp
{
    public class WriteToSftpFileArgs<TParams>
    {
        public IStream<IFileValue> Stream { get; set; }
        public ISingleStream<TParams> ParamStream { get; set; }
        public Func<TParams, string> GetOutputFolder { get; set; }
        public Func<TParams, SftpConnectionInfo> GetConnectionInfo { get; set; }
    }
    public class WriteToSftpFileStreamNode<TParams> : StreamNodeBase<IFileValue, IStream<IFileValue>, WriteToSftpFileArgs<TParams>>
    {
        public WriteToSftpFileStreamNode(string name, WriteToSftpFileArgs<TParams> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override IStream<IFileValue> CreateOutputStream(WriteToSftpFileArgs<TParams> args)
        {
            var outputObservable = args.Stream.Observable.CombineWithLatest(args.ParamStream.Observable, (l, r) =>
             {
                 WriteSftpFile(args.GetConnectionInfo(r), args.GetOutputFolder(r), l);
                 return l;
             }, true);
            return base.CreateUnsortedStream(outputObservable);
        }
        private void WriteSftpFile(SftpConnectionInfo sftpConnectionInfo, string outputFolder, IFileValue fileValue)
        {
            var connectionInfo = sftpConnectionInfo.CreateConnectionInfo();
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                var stream = fileValue.GetContent();
                byte[] fileContents;
                stream.Position = 0;
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    fileContents = ms.ToArray();
                }

                client.WriteAllBytes(Path.Combine(outputFolder, fileValue.Name), fileContents);
            }
        }
    }
}
