using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infare_task_final
{
    public static class FlightAnalysis
    {
        public static void AnalyzeFlightCombinations(List<FlightCombination> combinations)
        {
            Console.WriteLine($"Analyzing {combinations.Count} flight combinations...");

            foreach (var combination in combinations)
            {
                Console.WriteLine($"Recommendation ID: {combination.RecommendationId}");
                Console.WriteLine($"Outbound Journey: Flight Count = {combination.OutboundJourney.Flights.Count}");
                Console.WriteLine($"Inbound Journey: Flight Count = {combination.InboundJourney.Flights.Count}");
                Console.WriteLine($"Total Price: {combination.TotalPrice}");
                Console.WriteLine($"Taxes: {combination.Taxes}");
                Console.WriteLine("----------");
            }
        }
    }
}


