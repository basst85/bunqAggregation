using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Bunq.Sdk.Context;
using Bunq.Sdk.Model.Generated.Endpoint;
using Bunq.Sdk.Model.Generated.Object;

namespace BunqAggregation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            JObject config = Settings.LoadConfig();
            if (!(File.Exists(@"bunq.conf")))
            {
                string apiKey = Environment.GetEnvironmentVariable("BUNQ_API_KEY").ToString();
                var apiContextSetup = ApiContext.Create(ApiEnvironmentType.PRODUCTION, apiKey, "BunqAggregation");
                apiContextSetup.Save();
            }

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
