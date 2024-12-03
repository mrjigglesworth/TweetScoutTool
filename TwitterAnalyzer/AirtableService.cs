using AirtableApiClient;
using TwitterAnalyzer.Models;

namespace TwitterAnalyzer
{
    public static class AirtableService
    {
        private static readonly string airtableApiKey = "patqtbUoOEHJ2ZqX2.64c5794ec2f6d20e65c8667290e70f445eb0f70676ebb256391d9045b6ebc1a0";
        private static readonly string airtableBaseId = "appmbM11KReO4FJfW"; // JP_Project_Work

        public static async Task<List<PumpFunNewTokenData>> FetchAirtableDataForPumpFunNewTokens()
        {
            List<PumpFunNewTokenData> tokens = new List<PumpFunNewTokenData>();

            using (AirtableBase airtableBase = new AirtableBase(airtableApiKey, airtableBaseId))
            {
                string filterFormula = "FIND('Low Risk Identified', {Status}) > 0";
                AirtableListRecordsResponse response = await airtableBase.ListRecords("PumpFunNewTokens", filterByFormula: filterFormula);

                if (response.Success)
                {
                    tokens = response.Records
                        .Select(record => new PumpFunNewTokenData
                        {
                            Symbol = record.GetField("Symbol")?.ToString(),
                            Name = record.GetField("Name")?.ToString(),
                            Uri = record.GetField("Uri")?.ToString(),
                            Status = record.GetField("Status")?.ToString(),
                            Mint = record.GetField("Mint")?.ToString()
                        })
                        .ToList();
                }
                else
                {
                    Console.WriteLine("Error fetching data from PumpFunNewTokens Airtable.");
                }
            }

            return tokens;
        }
        public static async Task<List<PumpFunTokenData>> FetchAirtableData()
        {
            List<PumpFunTokenData> tokens = new List<PumpFunTokenData>();

            using (AirtableBase airtableBase = new AirtableBase(airtableApiKey, airtableBaseId))
            {
                string filterFormula = "FIND('Waiting Verification', {Status}) > 0";
                AirtableListRecordsResponse response = await airtableBase.ListRecords("PumpFunTokens", filterByFormula: filterFormula);

                if (response.Success)
                {
                    tokens = response.Records
                        .Select(record => new PumpFunTokenData
                        {
                            JobID = record.GetField("JobID")?.ToString(),
                            Name = record.GetField("Name")?.ToString(),
                            CoinURL = record.GetField("CoinURL")?.ToString(),
                            CA = record.GetField("CA")?.ToString(),
                            Symbol = record.GetField("Symbol")?.ToString(),
                            SocialMediaURLs = record.GetField("SocialMediaURLs")?.ToString(),
                            Status = record.GetField("Status")?.ToString()
                        })
                        .ToList();
                }
                else
                {
                    Console.WriteLine("Error fetching data from Airtable.");
                }
            }

            return tokens;
        }
        public static async Task InsertIntoAirtable(string tableName, Dictionary<string, object> fields)
        {
            using (AirtableBase airtableBase = new AirtableBase(airtableApiKey, airtableBaseId))
            {
                // Ensure the 'Symbol' field exists in the provided fields
                if (!fields.ContainsKey("Symbol") || fields["Symbol"] == null)
                {
                    Console.WriteLine("The 'Symbol' field is missing or null.");
                    return;
                }

                string coinSymbol = fields["Symbol"].ToString();

                // Create a filter formula to check for existing records with the same Symbol
                string filterFormula = $"{{Symbol}} = '{coinSymbol}'";

                // Search for existing records with the same Symbol
                var existingRecordsResponse = await airtableBase.ListRecords(
                    tableName,
                    filterByFormula: filterFormula
                );

                if (!existingRecordsResponse.Success)
                {
                    Console.WriteLine($"Error checking for existing records: {existingRecordsResponse.AirtableApiError?.ErrorMessage}");
                    return;
                }

                // If a record with the same Symbol exists, do not insert a new record
                if (existingRecordsResponse.Records.Any())
                {
                    Console.WriteLine($"A record with the symbol '{coinSymbol}' already exists. Insertion aborted.");
                    return;
                }

                // Proceed to insert the new record
                Fields fieldsToInsert = new Fields { FieldsCollection = fields };
                var createResponse = await airtableBase.CreateRecord(tableName, fieldsToInsert);

                if (createResponse.Success)
                {
                    Console.WriteLine($"Successfully inserted record into table '{tableName}' with ID: {createResponse.Record.Id}");
                }
                else
                {
                    Console.WriteLine($"Failed to insert record. Error: {createResponse.AirtableApiError?.ErrorMessage}");
                }
            }
        }
        public static async Task UpdateRecordStatus(string tableName, string jobId, string[] newStatus, string symbol)
        {
            using (AirtableBase airtableBase = new AirtableBase(airtableApiKey, airtableBaseId))
            {
                // Step 1: Retrieve the record with the matching JobID
                string filterFormula = string.Empty;
                if(!string.IsNullOrEmpty(symbol))
                {
                    filterFormula = $"AND({{JobID}}='{jobId}', {{Symbol}}='{symbol}')";
                }
                else
                {
                    filterFormula = $"{{JobID}}='{jobId}'";
                }
                AirtableListRecordsResponse listResponse = await airtableBase.ListRecords(tableName, filterByFormula: filterFormula);

                if (!listResponse.Success || listResponse == null)
                {
                    Console.WriteLine($"No records found with JobID: {jobId}");
                    return;
                }

                // Step 2: Update the record
                string recordId = listResponse.Records.First().Id;
                Fields fields = new Fields{};
                fields.AddField("Status", newStatus);

                var updateResponse = await airtableBase.UpdateRecord(tableName, fields, recordId);

                if (updateResponse.Success)
                {
                    Console.WriteLine($"Successfully updated Status for JobID: {jobId} & Symbol: {symbol} to '{newStatus[0]}'.");
                }
                else
                {
                    Console.WriteLine($"Failed to update record for JobID: {jobId} & Symbol: {symbol}. Error: {updateResponse.AirtableApiError?.ErrorMessage}");
                }
            }
        }
        public static async Task UpdateRecordStatusForNewTokens(string tableName, string[] newStatus, string? symbol)
        {
            try
            {
                using (AirtableBase airtableBase = new AirtableBase(airtableApiKey, airtableBaseId))
                {
                    // Step 1: Retrieve the record with the matching JobID
                    string filterFormula = $"{{Symbol}}='{symbol}'";
                    AirtableListRecordsResponse listResponse = await airtableBase.ListRecords(tableName, filterByFormula: filterFormula);

                    if (listResponse == null || !listResponse.Success)
                    {
                        Console.WriteLine($"No records found with Symbol: {symbol}");
                        return;
                    }

                    // Step 2: Update the record
                    if (listResponse != null)
                    {
                        if (listResponse.Records.Any())
                        {
                            string? recordId = listResponse?.Records?.First().Id;
                            Fields fields = new Fields { };
                            fields.AddField("Status", newStatus);

                            if (!string.IsNullOrEmpty(recordId))
                            {
                                var updateResponse = await airtableBase.UpdateRecord(tableName, fields, recordId);

                                if (updateResponse.Success)
                                {
                                    Console.WriteLine($"Successfully updated {tableName} - Status for Symbol: {symbol} to '{newStatus[0]}'.");
                                }
                                else
                                {
                                    Console.WriteLine($"Failed to update record. Error: {updateResponse.AirtableApiError?.ErrorMessage}");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"No existing record found. Did NOT {tableName} - Status for Symbol: {symbol} to '{newStatus[0]}'.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No existing record found. Did NOT update {tableName} - Status for Symbol: {symbol} to '{newStatus[0]}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                throw;
            }
        }

    }
}
