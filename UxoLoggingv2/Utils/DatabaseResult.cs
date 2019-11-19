
using Newtonsoft.Json;
using System.Collections;

namespace UxoLoggingv2.Utils
{
    public class DatabaseResult
    {
        [JsonProperty("ConnectionString")]
        public string ConnectionString { get; set; }

        [JsonProperty("Database")]
        public string Database { get; set; }

        [JsonProperty("Query")]
        public string Query { get; set; }

        [JsonProperty("Headers")]
        public string[] Headers { get; set; }

        [JsonProperty("Results")]
        public ArrayList Results { get; set; }
    }
}
