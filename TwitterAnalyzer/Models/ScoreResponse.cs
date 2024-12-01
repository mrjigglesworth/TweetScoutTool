using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterAnalyzer.Models
{
    public class ScoreResponse
    {
        [JsonProperty("score")]
        public string Score { get; set; }
    }
}
