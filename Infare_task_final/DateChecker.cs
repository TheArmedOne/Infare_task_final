using System;
using System.IO;
using System.Threading.Tasks;

namespace Infare_task_final
{
    public class DateChecker
    {
        private readonly WebScraper webScraper = new WebScraper();
        private const string CsvFilePath = @"D:\Testdata\flight_availability.csv";

        public async Task CheckFlightDataForDates(string fromAirport, string toAirport)
        {
            DateTime startDate = new DateTime(2024, 2, 8);
            DateTime endDate = new DateTime(2024, 2, 28);

            using (StreamWriter file = new StreamWriter(CsvFilePath, false)) // Overwrite existing file
            {
                file.WriteLine("Date,Response,Content"); // CSV Headers

                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    string outboundDate = date.ToString("yyyy-MM-dd");
                    string returnDate = date.AddDays(7).ToString("yyyy-MM-dd"); // Return date is one week after the departure date

                    string url = FlightUtils.GetSearchUrl(fromAirport, toAirport, outboundDate, returnDate);

                    var response = await webScraper.ScrapeFlightData(url);
                    string responseType = response == null ? "HTML" : "JSON";
                    string contentSnippet = responseType == "HTML" ? "Route Not Available" : "Flight Data Available";

                    file.WriteLine($"{outboundDate},{responseType},{contentSnippet}");

                    await Task.Delay(1000); // 1000ms delay to avoid overloading the API
                }
            }

            Console.WriteLine("Completed checking dates and stored results in CSV.");
        }
    }
}
