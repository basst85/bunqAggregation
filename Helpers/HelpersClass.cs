using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BunqAggregation.Helpers
{

    public class PaymentRequest
    {

        public static void Send(JObject content)
        {
            var http = new HttpClient();
            var body = new StringContent(content.ToString(), Encoding.UTF8, "application/json");
            http.PostAsync("https://bunq-dev.tada.red/api/request/" ,body);
        }
    }
}
