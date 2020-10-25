using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.ValuesProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Paillave.Etl.Ftp
{
    public class WriteToFtpFileArgs<TParams>
    {
        public IStream<IFileValue> Stream { get; set; }
        public ISingleStream<TParams> ParamStream { get; set; }
        public Func<TParams, string> GetOutputFolder { get; set; }
        public Func<TParams, FtpConnectionInfo> GetConnectionInfo { get; set; }
    }
    public class WriteToFtpFileStreamNode<TParams> : StreamNodeBase<IFileValue, IStream<IFileValue>, WriteToFtpFileArgs<TParams>>
    {
        public WriteToFtpFileStreamNode(string name, WriteToFtpFileArgs<TParams> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override IStream<IFileValue> CreateOutputStream(WriteToFtpFileArgs<TParams> args)
        {
            var outputObservable = args.Stream.Observable.CombineWithLatest(args.ParamStream.Observable, (l, r) =>
             {
                 WriteFtpFile(args.GetConnectionInfo(r), args.GetOutputFolder(r), l);
                 return l;
             }, true);
            return base.CreateUnsortedStream(outputObservable);
        }
        private void WriteFtpFile(FtpConnectionInfo connectionInfo, string outputFolder, IFileValue fileValue)
        {
            UriBuilder uriBuilder = new UriBuilder("ftp", connectionInfo.Server, connectionInfo.PortNumber, Path.Combine(outputFolder, fileValue.Name));

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = new NetworkCredential(connectionInfo.Login, connectionInfo.Password);

            var stream = fileValue.GetContent();
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
