using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Infare_task_final
{
    // Manages the processing and analysis of flight data, including web scraping and CSV file generation.
    public class FlightDataProcessor
    {
        // Web scraper for fetching flight data from a specified URL.
        private readonly WebScraper _webScraper;
        // CSV writer for outputting flight combinations and analysis to CSV files.
        private readonly CsvWriter _csvWriter;

        public FlightDataProcessor()
        {
            _webScraper = new WebScraper();
            _csvWriter = new CsvWriter();
        }
        // Processes a single flight search context, scraping data, generating combinations, and writing to a CSV file.
        public async Task ProcessAndWriteFlightData(FlightSearchContext context)
        {
            // Attempt to scrape flight data using the provided context URL.
            var flightData = await _webScraper.ScrapeFlightData(context.Url);

            // Assign the scraped flight data to the context
            context.FlightData = flightData;
            // Check if valid flight data was returned.
            if (flightData?.Body?.Data != null)
            {
                // Generate flight combinations based on the scraped data.
                var flightCombinations = MakeFlightCombinations(flightData);
                // Identify the cheapest flight combination from those generated.
                var cheapestCombinations = FindCheapestFlightCombinations(flightCombinations);
                // Write the flight combinations and the cheapest combination to a CSV file, returning the file's name.
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
            var scrapingTasks = new List<Task<(FlightData FlightData, FlightSearchContext Context)>>();

            // Initiate scraping tasks for all contexts concurrently
            foreach (var context in contexts)
            {
                scrapingTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var flightData = await _webScraper.ScrapeFlightData(context.Url);
                        return (flightData, context);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred processing data for {context.DepartureAirport}-{context.ArrivalAirport}: {ex.Message}");
                        return (null, context); // Return null flightData on error
                    }
                }));
            }

            var results = await Task.WhenAll(scrapingTasks);

            // Prepare data for WriteMultipleCombinationsAsync
            var blocks = results.Select(result => {
                var flightCombinations = MakeFlightCombinations(result.FlightData);
                var cheapestCombinations = FindCheapestFlightCombinations(flightCombinations);
                return (flightCombinations, cheapestCombinations, result.Context);
            }).Where(block => block.flightCombinations != null && block.cheapestCombinations != null).ToList();

            // Check if there are any blocks to write
            if (blocks.Any())
            {
                // Call the WriteMultipleCombinationsAsync method with the prepared blocks
                await _csvWriter.WriteMultipleCombinationsAsync(blocks);
            }

            Console.WriteLine("All flight data processing completed.");
        }

        // Making Flight Combinations. 
        // Price and tax calculation, constructing flight numbers called within this method. 


        private List<FlightCombination> MakeFlightCombinations(FlightData flightData)
        {
            var combinations = new List<FlightCombination>();
            var totalPriceMap = flightData.Body.Data.TotalAvailabilities.ToDictionary(k => k.RecommendationId, v => v.Total);

            // Group journeys by RecommendationId to handle them as distinct possible combinations.
            foreach (var journeyGroup in flightData.Body.Data.Journeys.GroupBy(j => j.RecommendationId))
            {

                // Process outbound and inbound journeys separately.
                var outboundJourneys = journeyGroup.Where(j => j.Direction == "I" && j.Flights.Count <= 2).ToList();
                var inboundJourneys = journeyGroup.Where(j => j.Direction == "V" && j.Flights.Count <= 2).ToList();

                // Create combinations for each outbound journey.
                foreach (var outbound in outboundJourneys)
                {

                    // Handle outbound-only combinations if there are no inbound journeys.
                    if (!inboundJourneys.Any()) 
                    {

                        // Calculate total taxes and price for the combination.
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
                    else           
                    
                    
                    {
                        // Create combinations with both outbound and inbound journeys.
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
        private List<FlightCombination> FindCheapestFlightCombinations(List<FlightCombination> combinations)
        {
            // This dictionary will hold the cheapest combination for each RecommendationId.
            var cheapestCombinationsDict = new Dictionary<int, FlightCombination>();

            foreach (var combination in combinations)
            {
                // Check if the RecommendationId already has a recorded combination.
                if (cheapestCombinationsDict.TryGetValue(combination.RecommendationId, out var currentCheapest))
                {
                    // If the current combination is cheaper than the recorded one, update the dictionary.
                    if (combination.TotalPrice < currentCheapest.TotalPrice)
                    {
                        cheapestCombinationsDict[combination.RecommendationId] = combination;
                    }
                }
                else
                {
                    // If this is the first combination encountered for this RecommendationId, add it to the dictionary.
                    cheapestCombinationsDict[combination.RecommendationId] = combination;
                }
            }

            // Extract the values from the dictionary to get a list of the cheapest combinations.
            var cheapestCombinations = cheapestCombinationsDict.Values.ToList();

            return cheapestCombinations;
        }





    }
}
