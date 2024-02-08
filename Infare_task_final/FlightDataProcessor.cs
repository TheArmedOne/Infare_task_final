using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Infare_task_final
{
    public class FlightDataProcessor
    {
        private readonly WebScraper _webScraper;
        private readonly CsvWriter _csvWriter;

        public FlightDataProcessor()
        {
            _webScraper = new WebScraper();
            _csvWriter = new CsvWriter();
        }
        // Processing and writing data for SINGLE itinerary
        public async Task ProcessAndWriteFlightData(FlightSearchContext context)
        {
            // Scraping the data
            var flightData = await _webScraper.ScrapeFlightData(context.Url);

            // Assign the scraped flight data to the context
            context.FlightData = flightData;

            if (flightData?.Body?.Data != null)
            {
                // Making flight combinations
                var flightCombinations = MakeFlightCombinations(flightData);
                var cheapestCombinations = FindCheapestFlightCombinations(flightCombinations);
                // Write all combinations to CSV and get the file name
                string fileName = _csvWriter.WriteCombinations(flightCombinations, cheapestCombinations, context);

                Console.WriteLine($"Data successfully written to {fileName}");
            }
            else
            {
                Console.WriteLine("No valid flight data found.");
            }
        }

        // Processing and writing data for MULTIPLE itineraries concurrently. 
        public async Task ProcessAndWriteMultipleFlightData(List<FlightSearchContext> contexts)
        {
            var blocks = new List<(List<FlightCombination> Combinations, List<FlightCombination> CheapestCombinations, FlightSearchContext Context)>();

            foreach (var context in contexts)
            {
                try
                {
                    await Task.Delay(1000);

                    var flightData = await _webScraper.ScrapeFlightData(context.Url);

                    if (flightData?.Body?.Data != null)
                    {
                        context.FlightData = flightData; // Attaching the scraped flight data to the context for usage at later stages

                        var flightCombinations = MakeFlightCombinations(flightData);
                        
                        var cheapestCombinations = FindCheapestFlightCombinations(flightCombinations);

                        // Adding data to blocks list to satisfy multiple file writing
                        blocks.Add((flightCombinations, cheapestCombinations, context));

                        Console.WriteLine($"Data for {context.DepartureAirport}-{context.ArrivalAirport} from {context.OutboundDate:yyyy-MM-dd} to {context.InboundDate:yyyy-MM-dd} successfully processed.");
                    }
                    else
                    {
                        Console.WriteLine($"No valid flight data found for {context.DepartureAirport}-{context.ArrivalAirport} from {context.OutboundDate:yyyy-MM-dd} to {context.InboundDate:yyyy-MM-dd}.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred processing data for {context.DepartureAirport}-{context.ArrivalAirport}: {ex.Message}");
                }
            }

            // After all data is collected, passing the populated block to write data
            if (blocks.Any())
            {
                _csvWriter.WriteMultipleCombinations(blocks);
            }

            Console.WriteLine("All flight data processing completed.");
        }
        
        // Making Flight Combinations. 
        // Price and tax calculation, constructing flight numbers called within this method. 

        
        private List<FlightCombination> MakeFlightCombinations(FlightData flightData)
        {
            var combinations = new List<FlightCombination>();
            var totalPriceMap = flightData.Body.Data.TotalAvailabilities.ToDictionary(k => k.RecommendationId, v => v.Total);

            foreach (var journeyGroup in flightData.Body.Data.Journeys.GroupBy(j => j.RecommendationId))
            {
                var outboundJourneys = journeyGroup.Where(j => j.Direction == "I" && j.Flights.Count <= 2).ToList();
                var inboundJourneys = journeyGroup.Where(j => j.Direction == "V" && j.Flights.Count <= 2).ToList();

                foreach (var outbound in outboundJourneys)
                {
                    if (!inboundJourneys.Any()) // Outbound-only combinations
                    {
                        double totalTaxes = PricingCalculator.CalculateTaxesForCombination(outbound, null);
                        double basePrice = totalPriceMap.TryGetValue(journeyGroup.Key, out double price) ? price : 0;
                        double fullPrice = PricingCalculator.CalculateFullPriceForCombination(basePrice, totalTaxes); 

                        combinations.Add(new FlightCombination
                        {
                            RecommendationId = journeyGroup.Key,
                            OutboundJourney = outbound,
                            InboundJourney = null,
                            TotalPrice = fullPrice,
                            Taxes = totalTaxes,
                            OutboundFlightNumbers = outbound.Flights.Select(f => f.FullFlightNumber).ToList(),
                            InboundFlightNumbers = new List<string>()
                        });
                    }
                    else // Combinations with inbound journeys
                    {
                        foreach (var inbound in inboundJourneys)
                        {
                            double totalTaxes = PricingCalculator.CalculateTaxesForCombination(outbound, inbound);
                            double basePrice = totalPriceMap.TryGetValue(journeyGroup.Key, out double price) ? price : 0;
                            double fullPrice = PricingCalculator.CalculateFullPriceForCombination(basePrice, totalTaxes);

                            combinations.Add(new FlightCombination
                            {
                                RecommendationId = journeyGroup.Key,
                                OutboundJourney = outbound,
                                InboundJourney = inbound,
                                TotalPrice = fullPrice, 
                                Taxes = totalTaxes,
                                OutboundFlightNumbers = outbound.Flights.Select(f => f.FullFlightNumber).ToList(),
                                InboundFlightNumbers = inbound.Flights.Select(f => f.FullFlightNumber).ToList()
                            });
                        }
                    }
                }
            }

            return combinations;
        }
        // Processing of itineraries with connection, taking the connection airport from context
        public async Task ProcessSingleItineraryWithConnection(FlightSearchContext context)
        {
            try
            {
                var flightData = await _webScraper.ScrapeFlightData(context.Url);

                
                if (flightData == null || flightData.Body?.Data == null || !flightData.Body.Data.Journeys.Any())
                {
                    Console.WriteLine("No valid flight data found. Please check your inputs and try again.");
                    return;
                }

                // Checking if connection airport is correctly specified
                if (!string.IsNullOrWhiteSpace(context.ConnectionAirportCode))
                {
                    // Filtering the flight data based on the connection airport before making combinations
                    flightData = FilterFlightDataByConnectionAirport(flightData, context.ConnectionAirportCode);
                }

                context.FlightData = flightData; // Assign the (possibly filtered) flight data back to the context

                var flightCombinations = MakeFlightCombinations(context.FlightData);
                var cheapestCombinations = FindCheapestFlightCombinations(flightCombinations);

                // Writing filtered combinations to .csv
                string fileName = _csvWriter.WriteCombinations(flightCombinations, cheapestCombinations, context);

                Console.WriteLine($"Data successfully written to {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        // helper method for filtering flights with a specific connection airport
        public static FlightData FilterFlightDataByConnectionAirport(FlightData flightData, string connectionAirportCode)
        {
            if (string.IsNullOrEmpty(connectionAirportCode))
            {
                
                return flightData;
            }

            
            var filteredJourneys = flightData.Body.Data.Journeys
                .Where(journey => journey.Flights.Count <= 1 ||
                                  journey.Flights.Any(flight => flight.AirportArrival.Code == connectionAirportCode ||
                                                                flight.AirportDeparture.Code == connectionAirportCode))
                .ToList();
            
            FlightData filteredFlightData = new FlightData
            {
                Header = flightData.Header,
                Body = new Body
                {
                    Data = new Data
                    {
                        Journeys = filteredJourneys,
                        TotalAvailabilities = flightData.Body.Data.TotalAvailabilities 
                    }
                }
            };

            return filteredFlightData;
        }

        // Filtering of cheapest flight combinations for each recommendationId. 
        private List <FlightCombination> FindCheapestFlightCombinations(List<FlightCombination> combinations)
        {
            // Group by RecommendationId and select the cheapest combination within each group
            var cheapestCombinations = combinations
                .GroupBy(c => c.RecommendationId)
                .Select(group => group.OrderBy(c => c.TotalPrice).FirstOrDefault())
                .ToList(); // Ensure the query is executed and results are materialized

            return cheapestCombinations;
        }



        
    }
}
