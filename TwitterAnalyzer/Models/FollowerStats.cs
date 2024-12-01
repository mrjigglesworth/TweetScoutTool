using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterAnalyzer.Models
{
    public class FollowersStats
    {
        [JsonProperty("followers_count")]
        public int FollowersCount { get; set; }

        [JsonProperty("influencers_count")]
        public int InfluencersCount { get; set; }

        [JsonProperty("projects_count")]
        public int ProjectsCount { get; set; }

        [JsonProperty("venture_capitals_count")]
        public int VentureCapitalsCount { get; set; }
    }
}
