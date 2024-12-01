using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterAnalyzer.Models
{
    public class AirtableRecord
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("fields")]
        public Dictionary<string, object> Fields { get; set; }
    }
}
