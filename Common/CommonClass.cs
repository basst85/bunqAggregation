using Bunq.Sdk.Context;
using Bunq.Sdk.Model.Generated.Endpoint;
using Bunq.Sdk.Model.Generated.Object;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace bunqAggregation.Common
{
    public class Settings
    {
        public static JObject LoadConfig()
        {
            return JObject.Parse(File.ReadAllText(@"Configuration.json"));
        }
    }

    public class PaymentRequest
    {

        public static void Send(JObject content)
        {
            var http = new HttpClient();
            var body = new StringContent(content.ToString(), Encoding.UTF8, "application/json");
            http.PostAsync(Environment.GetEnvironmentVariable("SERVICE_URL").ToString() + "api/request/" ,body);
        }
    }

    public class Payment
    {

        public static void Execute(JObject content)
        {
            var http = new HttpClient();
            var body = new StringContent(content.ToString(), Encoding.UTF8, "application/json");
            http.PostAsync(Environment.GetEnvironmentVariable("SERVICE_URL").ToString() + "api/payment/", body);
        }
    }

    public class Registraion
    {
        public static void Initialize(IHostingEnvironment env){
            string apiKey = Environment.GetEnvironmentVariable("BUNQ_API_KEY").ToString();

            if (env.EnvironmentName == "Development")
            {
                var currentIpGet = new HttpClient().GetStringAsync("http://ipinfo.io/ip");
                string currentIp = Regex.Replace(currentIpGet.Result.ToString(), @"\t|\n|\r", "");
                List<string> DevelopmentIPs = new List<string>{
                    "123.123.123.123", //replace with own ip
                    currentIp
                };
                Console.WriteLine(apiKey);
                var apiContextSetup = ApiContext.Create(ApiEnvironmentType.SANDBOX, apiKey, "bunqAggregation", DevelopmentIPs);
                apiContextSetup.Save();
            }
            else
            {
                var apiContextSetup = ApiContext.Create(ApiEnvironmentType.PRODUCTION, apiKey, "bunqAggregation");
                apiContextSetup.Save();
            }
        }
    }

}
