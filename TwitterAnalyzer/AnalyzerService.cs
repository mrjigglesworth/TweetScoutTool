using Newtonsoft.Json;
using TwitterAnalyzer.Models;

public class AnalyzerService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "https://api.tweetscout.io/api";
    private readonly string _apiKey;

    public AnalyzerService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<AnalyzeTwitterAccountStatsResponse> AnalyzeTwitterAccountStats(string username)
    {
        var response = new AnalyzeTwitterAccountStatsResponse() { IsGem = false };
        try
        {
            Console.WriteLine("Fetching account stats...");

            // Call the followers-stats endpoint
            //string url = $"{_apiBaseUrl}/followers-stats?username={username}";
            FollowersStats stats = await GetFollowerStats(username);

            Console.WriteLine($"Account Stats for @{username}:");
            Console.WriteLine($"- Followers Count: {stats.FollowersCount}");
            Console.WriteLine($"- Influencers Count: {stats.InfluencersCount}");
            Console.WriteLine($"- Projects Count: {stats.ProjectsCount}");
            Console.WriteLine($"- Venture Capitals Count: {stats.VentureCapitalsCount}");

            // Perform gem analysis based on the data
            if (stats.FollowersCount > 1000 && stats.InfluencersCount >= 10)
            {
                Console.WriteLine($"Account @{username} qualifies as a potential gem.");
                response =  new AnalyzeTwitterAccountStatsResponse
                {
                    IsGem = true,
                    TweetScoutScore = GetScore(username).Result.Score
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return response;
    }
    
    private async Task<ScoreResponse> GetScore(string user)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{_apiBaseUrl}/score/{user}"),
            Headers =
            {
                { "Accept", "application/json" },
                { "ApiKey", $"{_apiKey}" },
            },
        };
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ScoreResponse>(body);
        }   
    }

    private async Task<FollowersStats> GetFollowerStats(string user)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{_apiBaseUrl}/followers-stats?username={user}"),
            Headers =
            {
                { "Accept", "application/json" },
                { "ApiKey", $"{_apiKey}" },
            },
        };
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FollowersStats>(body);
        }
    }
}
