using System;
using System.IO;
using System.Linq;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Connector;
using Paillave.Etl.ValuesProviders;
using System.Net;

namespace Paillave.Etl.Ftp
{
    public class FtpFileValueProcessor : FileValueProcessorBase<FtpAdapterConnectionParameters, FtpAdapterProcessorParameters>
    {
        public FtpFileValueProcessor(string code, string name, string connectionName, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProcessorParameters processorParameters)
            : base(code, name, connectionName, connectionParameters, processorParameters) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        protected override void Process(IFileValue fileValue, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var folder = Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");
            UriBuilder uriBuilder = new UriBuilder("ftp", connectionParameters.Server, connectionParameters.PortNumber, Path.Combine(folder, fileValue.Name));

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = new NetworkCredential(connectionParameters.Login, connectionParameters.Password);

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
            push(fileValue);
        }

        protected override void Test(FtpAdapterConnectionParameters connectionParameters, FtpAdapterProcessorParameters processorParameters)
        {
            var fileName = Guid.NewGuid().ToString();
            var folder = Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");
            UriBuilder uriBuilder = new UriBuilder("ftp", connectionParameters.Server, connectionParameters.PortNumber, Path.Combine(folder, fileName));

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = new NetworkCredential(connectionParameters.Login, connectionParameters.Password);

            var stream = new MemoryStream();
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


            FtpWebRequest deleteRequest = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
            deleteRequest.Method = WebRequestMethods.Ftp.DeleteFile;

            deleteRequest.Credentials = new NetworkCredential(connectionParameters.Login, connectionParameters.Password);
            using (FtpWebResponse response = (FtpWebResponse)deleteRequest.GetResponse())
                if (!new[] { FtpStatusCode.CommandOK, FtpStatusCode.FileActionOK }.Contains(response.StatusCode))
                    throw new Exception("Ftp delete request failed");
        }
    }
}