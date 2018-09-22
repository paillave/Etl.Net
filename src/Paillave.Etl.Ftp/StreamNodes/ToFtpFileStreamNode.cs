using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Paillave.Etl.Ftp.StreamNodes
{
    public class ToFtpFileArgs<TParams>
    {
        public IStream<Stream> Stream { get; set; }
        public IStream<TParams> ParamStream { get; set; }
        public Func<TParams, string> GetOutputFilePath { get; set; }
        public Func<TParams, FtpConnectionInfo> GetConnectionInfo { get; set; }
    }
    public class ToFtpFileStreamNode<TParams> : StreamNodeBase<Stream, IStream<Stream>, ToFtpFileArgs<TParams>>
    {
        public override bool IsAwaitable => true;
        public ToFtpFileStreamNode(string name, ToFtpFileArgs<TParams> args) : base(name, args)
        {
        }
        protected override IStream<Stream> CreateOutputStream(ToFtpFileArgs<TParams> args)
        {
            var outputObservable = args.Stream.Observable.CombineWithLatest(args.ParamStream.Observable, (l, r) =>
             {
                 WriteFtpFile(args.GetConnectionInfo(r), args.GetOutputFilePath(r), l);
                 return l;
             }, true);
            return base.CreateUnsortedStream(outputObservable);
        }
        private void WriteFtpFile(FtpConnectionInfo connectionInfo, string outputPath, Stream stream)
        {
            UriBuilder uriBuilder = new UriBuilder("ftp", connectionInfo.Server, connectionInfo.PortNumber, outputPath);

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = new NetworkCredential(connectionInfo.Login, connectionInfo.Password);

            byte[] fileContents;
            stream.Position = 0;
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                fileContents = ms.ToArray();
            }

            request.ContentLength = fileContents.Length;

            using (Stream requestStream = request.GetRequestStream())
                requestStream.Write(fileContents, 0, fileContents.Length);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                if (response.StatusCode != FtpStatusCode.CommandOK)
                    throw new FtpUploadException(response.StatusCode, response.StatusDescription);
        }
    }
}
