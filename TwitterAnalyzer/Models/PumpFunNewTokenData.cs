namespace TwitterAnalyzer.Models
{
    public class PumpFunNewTokenData
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Mint { get; set; }
        public string? Status { get; internal set; }
    }
}
