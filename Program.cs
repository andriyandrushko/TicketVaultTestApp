using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

// Mocked data collections to store ticker information and records
var tickerCollection = new List<Tuple<string, int>>();
var recordCollection = new List<Tuple<string, string, long>>();

//AA: Moved magic numbers to constants
const int year = 2023;
const int startMonth = 4;
const int endMonth = 6;

for (int month = startMonth; month <= endMonth; month++)
{
    string apiUrlBase = $"https://tradestie.com/api/v1/apps/reddit?date=";

    //AA: take corect number of days
    int daysInMounth = DateTime.DaysInMonth(year, month);
    
    // Loop through month days
    for (int day = 1; day <= daysInMounth; day++)
    {
        // Construct the API URL base for the current day
        string apiUrlDate = $"{year}-{month:D2}-{day:D2}";

        // Perform the API request
        var responseData = await GetApiData(apiUrlBase + apiUrlDate);

        // Parse and process the response
        ProcessApiData(responseData, tickerCollection, recordCollection, apiUrlBase);
    }
}

// Display the mocked data collections
DisplayDataCollections(tickerCollection, recordCollection);

// Fake call to store both collections in a pretend database
StoreInDatabase(tickerCollection, recordCollection);

static async Task<string> GetApiData(string apiUrl)
{
    int retryCount = 0;
    const int maxRetries = 10;  // AA: Retries count
    TimeSpan delay = TimeSpan.FromSeconds(5); //AA: Initial cooldown time set for 5 seconds

    while (retryCount < maxRetries)
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta;
                    if (retryAfter != null)
                    {
                        delay = retryAfter.Value;
                    }
                }
            }
        }
        catch (Exception) { }

        retryCount++;
        await Task.Delay(delay);
        delay *= 2; // AA: Exponential backoff
    }

    return "";
}

static void ProcessApiData(string responseData, List<Tuple<string, int>> tickerCollection, List<Tuple<string, string, long>> recordCollection, string apiUrlBase)
{
    try
    {
        // Deserialize JSON response
        var data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(responseData);

        // Loop through each data entry
        foreach (var entry in data)
        {
            var ticker = (string)entry.GetValueOrDefault("ticker");
            var sentiment = (string)entry.GetValueOrDefault("sentiment");
            var noOfComments = (long)entry.GetValueOrDefault("no_of_comments");

            // Extract relevant information
            // Mocked data collection insertion for each ticker
            SaveTicker(ticker, tickerCollection);

            // Mocked data collection insertion for each record with foreign key
            SaveRecord(ticker, sentiment, noOfComments, recordCollection);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing data for {apiUrlBase}: {ex.Message}");
    }
}

static void SaveTicker(string ticker, List<Tuple<string, int>> tickerCollection)
{
    //AA: to fix Database grow check if ticker already exists.
    //Add only unique values
    tickerCollection.Add(new Tuple<string, int>(ticker, 0));
    Console.WriteLine($"Saved Ticker: {ticker}");
}

static void SaveRecord(string ticker, string sentiment, long noOfComments, List<Tuple<string, string, long>> recordCollection)
{
    //AA: to fix Database grow check if ticker already exists.
    //Add only unique values
    recordCollection.Add(new Tuple<string, string, long>(ticker, sentiment, noOfComments));
    Console.WriteLine($"Saved Record: Ticker - {ticker}, Sentiment - {sentiment}, Comments - {noOfComments}");
}

static void DisplayDataCollections(List<Tuple<string, int>> tickerCollection, List<Tuple<string, string, long>> recordCollection)
{
    Console.WriteLine("\nMocked Data Collections - Tickers:");
    foreach (var tuple in tickerCollection)
    {
        Console.WriteLine($"Ticker: {tuple.Item1}, Records Count: {tuple.Item2}");
    }

    Console.WriteLine("\nMocked Data Collections - Records:");
    foreach (var tuple in recordCollection)
    {
        Console.WriteLine($"Ticker: {tuple.Item1}, Sentiment: {tuple.Item2}, Comments: {tuple.Item3}");
    }
}

// Fake call to store both collections in a pretend database
static void StoreInDatabase(List<Tuple<string, int>> tickerCollection, List<Tuple<string, string, long>> recordCollection)
{
    Console.WriteLine("\nFake Call: Storing both data collections in a pretend database.");
}

