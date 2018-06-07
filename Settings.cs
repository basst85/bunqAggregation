using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BunqAggregation
{
    public class Settings {
        public static JObject LoadConfig()
        {
            return JObject.Parse(File.ReadAllText(@"Configuration.json"));
        }
    }
}
