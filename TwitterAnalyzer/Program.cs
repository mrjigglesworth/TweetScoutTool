using System.Reflection.Metadata;
using TwitterAnalyzer;
using TwitterAnalyzer.Models;
using TwitterAnalyzer.Utilities;

public class Program
{
    private static readonly string tweetScoutApiKey = "be06ed56-7697-45fc-a2a4-9531d2de0999";

    public static async Task Main(string[] args)
    {
        args = ["GETNEWCOINS"];
        if (args[0] == "GETNEWCOINS")
        {
            if (args[0] == "GETNEWCOINS")
            {
                var pumpFunTokens = AirtableService.FetchAirtableDataForPumpFunNewTokens().Result;
                foreach (var pump in pumpFunTokens)
                {
                    string coinName = pump.Name;
                    string symbol = pump.Symbol;
                    if (pump.Status != null && pump.Status.Replace("]", string.Empty).Replace("[", string.Empty).Replace("\"", string.Empty) == "Low Risk Identified")
                    {
                        await AirtableService.UpdateRecordStatusForNewTokens("PumpFunNewTokens", new[] { "Processing", "Tweetscout Processing" }, symbol);
                        string contractAddress = pump.Mint;

                        try
                        {
                            // Call RugCheck API for analysis
                            Console.WriteLine($"Analyzing twitter data for token: {symbol} (Base64: {pump.Mint})...");
                            // Define the list of possible values
                            List<int> values = new List<int> { 1000, 2000, 3000, 4000, 5000, 6000, 7000 };
                            Random random = new Random();
                            int randomIndex = random.Next(values.Count);
                            int randomValue = values[randomIndex];
                            Thread.Sleep(randomValue);
                            AnalyzerService analyzerService = new AnalyzerService(tweetScoutApiKey);
                            string twitterUrl = await analyzerService.GetTwitterAccountUrlFromIpfs(pump.Uri);
                            string cleanedHandle = Utilities.ExtractTwitterHandleFromEndOfUrl(twitterUrl);
                            Console.WriteLine($"Analyzing Twitter handle: {cleanedHandle}");

                            if (!string.IsNullOrEmpty(cleanedHandle))
                            {
                                var response = await analyzerService.AnalyzeTwitterAccountStats(cleanedHandle);
                                if (response.IsGem)
                                {
                                    await AirtableService.UpdateRecordStatusForNewTokens("PumpFunNewTokens", new[] { "Low Risk Identified", "Tweetscout Verified" }, symbol);
                                }
                                else
                                {
                                    await AirtableService.UpdateRecordStatusForNewTokens("PumpFunNewTokens", new[] { "High Risk Identified", "Tweetscout Failed" }, symbol);
                                }
                            }
                            else
                            {
                                await AirtableService.UpdateRecordStatusForNewTokens("PumpFunNewTokens", new[] { "High Risk Identified", "Tweetscout Failed" }, symbol);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                string jobId = string.Empty;
                try
                {
                    AnalyzerService analyzerService = new AnalyzerService(tweetScoutApiKey);
                    // Step 1: Fetch data from Airtable
                    List<PumpFunTokenData> tokens = await AirtableService.FetchAirtableData();

                    foreach (var token in tokens)
                    {
                        jobId = token.JobID;
                        if (token.Status.Replace("]", string.Empty).Replace("[", string.Empty).Replace("\"", string.Empty) == "Waiting Verification")
                        {
                            if (!string.IsNullOrEmpty(token.SocialMediaURLs) && token.SocialMediaURLs.Split("\n").Length > 0)
                            {
                                // Step 2: Extract Twitter handles/URLs
                                var splitUrls = token.SocialMediaURLs.Split("\n");
                                foreach (var splitUrl in splitUrls)
                                {
                                    if (splitUrl.Contains("x.com") || splitUrl.Contains("twitter.com"))
                                    {
                                        List<string> twitterHandles = Utilities.ExtractTwitterHandles(splitUrl);

                                        if (twitterHandles.Count > 0)
                                        {
                                            // Step 3: Analyze each Twitter handle using TweetScout API
                                            foreach (var handle in twitterHandles)
                                            {
                                                string cleanedHandle = Utilities.ExtractTwitterHandleFromEndOfUrl(handle);
                                                Console.WriteLine($"Analyzing Twitter handle: {cleanedHandle}");

                                                var response = await analyzerService.AnalyzeTwitterAccountStats(cleanedHandle);
                                                if (response.IsGem)
                                                {
                                                    await AirtableService.InsertIntoAirtable("TokenProspects", new Dictionary<string, object>
                                                {
                                                    { "JobID", token.JobID },
                                                    { "Name", token.Name },
                                                    { "TweetScoutScore", response.TweetScoutScore != null ? response.TweetScoutScore : string.Empty },
                                                    { "TweetScoutDateChecked", DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") },
                                                    { "Status", new[] { "Waiting Dexscreener" } }
                                                });

                                                    await AirtableService.UpdateRecordStatus("PumpFunTokens", token.JobID, new[] { "Prospect Identified" }, token.Symbol);
                                                }
                                                else
                                                {
                                                    if (response.TweetScoutScore != null)
                                                    {
                                                        await AirtableService.UpdateRecordStatus("PumpFunTokens", token.JobID, new[] { "Verification Did Not Pass" }, token.Symbol);
                                                    }
                                                    else
                                                    {
                                                        await AirtableService.UpdateRecordStatus("PumpFunTokens", token.JobID, new[] { "Tweetscout Response Error" }, token.Symbol);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            await AirtableService.UpdateRecordStatus("PumpFunTokens", token.JobID, new[] { "Verification Failed" }, token.Symbol);
                                        }
                                    }
                                    else
                                    {
                                        if (splitUrls.Length == 1)
                                        {
                                            await AirtableService.UpdateRecordStatus("PumpFunTokens", token.JobID, new[] { "Verification Failed" }, token.Symbol);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                await AirtableService.UpdateRecordStatus("PumpFunTokens", token.JobID, new[] { "Verification Failed" }, token.Symbol);
                            }

                            //await AirtableService.UpdateRecordStatus("CryptoAnalysis", jobId, new string[] { "Waiting for Dexscreener" }, string.Empty);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    throw ex;
                }

            }
            Environment.Exit(0);
        }
    }
}
