using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterAnalyzer.Models
{
    public class AirtableFetchResponse
    {
        [JsonProperty("records")]
        public List<AirtableRecord> Records { get; set; }
    }
}
