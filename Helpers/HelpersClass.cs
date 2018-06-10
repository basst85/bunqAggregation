using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace bunqAggregation.Helpers
{

    public class PaymentRequest
    {

        public static void Send(JObject content)
        {
            var http = new HttpClient();
            var body = new StringContent(content.ToString(), Encoding.UTF8, "application/json");
            http.PostAsync(Environment.GetEnvironmentVariable("BUNQAGGREGATION_BASEURL").ToString() + "api/request/" ,body);
        }
    }

    public class Payment
    {

        public static void Execute(JObject content)
        {
            var http = new HttpClient();
            var body = new StringContent(content.ToString(), Encoding.UTF8, "application/json");
            http.PostAsync(Environment.GetEnvironmentVariable("BUNQAGGREGATION_BASEURL").ToString() + "api/payment/", body);
        }
    }

}
