
using Paillave.Etl.Core;
using Paillave.Etl.Http;

namespace Paillave.Etl.Samples
{
   
    class Program10
    {
        static async Task Main(string[] args)
        {
            var httpAdapterProviderParameters = new HttpAdapterProviderParameters
            {
                Method = HttpMethodCustomEnum.Post,
                Body = new Dictionary<string, object>
                {
                    {"name", "{{AdditionalParameters.bodyValueName}}" },
                    {"age", 30},
                    {"city", "New York"}
                }
            };
            var httpAdapterConnectionParameters = new HttpAdapterConnectionParameters
            {
                //Url = "https://www.google.com/search?q={{AdditionalParameters.query}}",
                //Url = "https://www.w3schools.com/{{AdditionalParameters.format}}/{{AdditionalParameters.file}}"

                Url = "http://echo.free.beeceptor.com/sample-request?author={{AdditionalParameters.author}}",
            };

            var connectors = new FileValueConnectors();
            connectors.Register(
                new HttpFileValueProvider(
                    "MyHttpSourceForAdditionalParameters",
                    "Input",
                    "HttpConnection",
                    httpAdapterConnectionParameters,
                    httpAdapterProviderParameters
                )
            );

            var executionOptions = new ExecutionOptions<string[]> { Connectors = connectors };
            var processRunner = StreamProcessRunner.Create<string[]>(Import);
            var res = await processRunner.ExecuteAsync(args, executionOptions);
        }

        public static void Import(ISingleStream<string[]> contextStream)
        {
            contextStream
                .FromConnector("get from http", "MyHttpSourceForAdditionalParameters")
                .Select(
                    "select",
                    i =>
                    {
                        if (((HttpFileValueMetadata)i.Metadata).Parameters.AdditionalParameters == null)
                        {
                            ((HttpFileValueMetadata)i.Metadata).Parameters.AdditionalParameters = new Dictionary<string, string>();
                        }
                        ((HttpFileValueMetadata)i.Metadata).Parameters.AdditionalParameters.Add("file", "xmlhttp_info.txt");
                        ((HttpFileValueMetadata)i.Metadata).Parameters.AdditionalParameters.Add("format", "xml");
                        ((HttpFileValueMetadata)i.Metadata).Parameters.AdditionalParameters.Add("query", "Voiture");



                        ((HttpFileValueMetadata)i.Metadata).Parameters.AdditionalParameters.Add("author", "beeceptor");
                        ((HttpFileValueMetadata)i.Metadata).Parameters.AdditionalParameters.Add("bodyValueName", "Mickael");

                        

                        
                        return i;
                    }
                    )
                 .Do(
                    "print to console",
                    i =>
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            i.GetContent().CopyTo(memoryStream);
                            byte[] contentBytes = memoryStream.ToArray();
                            string contentString = System.Text.Encoding.UTF8.GetString(
                                contentBytes
                            );
                            Console.WriteLine($"result ({contentString}");
                            Console.WriteLine($"result ({i.Name})");
                        }
                    }
                );
        }
    }
}
