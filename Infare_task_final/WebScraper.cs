using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infare_task_final
{
    public class WebScraper
    {
        public async Task<FlightData> ScrapeFlightData(string url)
        {
            try
            {
                var (responseBody, contentType, statusCode) = await HttpClientHelper.GetAsync(url);

                // Check if the response is HTML
                if (contentType.Contains("text/html") && responseBody.Contains("<h1>Route Not Available</h1>"))
                {
                    Console.WriteLine("Route not available.");
                    return null; // Could be updated to handle as needed
                }

                // Proceed given JSON response
                if (statusCode == HttpStatusCode.OK && contentType.Contains("application/json"))
                {
                    
                    FlightData flightData = JsonConvert.DeserializeObject<FlightData>(responseBody);
                    FlightUtils.ConstructFlightNumber(flightData);
                    Console.WriteLine("Deserialized FlightData identities:");
                    foreach (var journey in flightData.Body.Data.Journeys)
                    {
                        Console.WriteLine($"RecommendationId: {journey.RecommendationId}, Identity: {journey.Identity}");
                    }
                    return flightData;
                }

                // Handle unexpected response, could be updated
                Console.WriteLine($"Unexpected response: Status Code {statusCode}, Content-Type {contentType}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                return null;
            }
        }
    }
}
