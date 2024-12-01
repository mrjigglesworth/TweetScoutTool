using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterAnalyzer.Models
{
    public class AnalyzeTwitterAccountStatsResponse
    {
        public bool IsGem { get; set; }
        public string? TweetScoutScore { get; set; }
    }
}
