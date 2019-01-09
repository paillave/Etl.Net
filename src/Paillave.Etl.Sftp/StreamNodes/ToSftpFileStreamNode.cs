using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Paillave.Etl.Sftp.StreamNodes
{
    public class ToSftpFileArgs<TParams>
    {
        public IStream<Stream> Stream { get; set; }
        public ISingleStream<TParams> ParamStream { get; set; }
        public Func<TParams, string> GetOutputFilePath { get; set; }
        public Func<TParams, SftpConnectionInfo> GetConnectionInfo { get; set; }
    }
    public class ToSftpFileStreamNode<TParams> : StreamNodeBase<Stream, IStream<Stream>, ToSftpFileArgs<TParams>>
    {
        public ToSftpFileStreamNode(string name, ToSftpFileArgs<TParams> args) : base(name, args)
        {
        }
        protected override IStream<Stream> CreateOutputStream(ToSftpFileArgs<TParams> args)
        {
            var outputObservable = args.Stream.Observable.CombineWithLatest(args.ParamStream.Observable, (l, r) =>
             {
                 WriteSftpFile(args.GetConnectionInfo(r), args.GetOutputFilePath(r), l);
                 return l;
             }, true);
            return base.CreateUnsortedStream(outputObservable);
        }
        private void WriteSftpFile(SftpConnectionInfo sftpConnectionInfo, string outputPath, Stream stream)
        {
            var connectionInfo = sftpConnectionInfo.CreateConnectionInfo();
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                byte[] fileContents;
                stream.Position = 0;
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    fileContents = ms.ToArray();
                }

                client.WriteAllBytes(outputPath, fileContents);
                // using (var outputSteam = client.OpenWrite(outputPath))
                //     stream.CopyTo(outputSteam);
            }
        }
    }
}
