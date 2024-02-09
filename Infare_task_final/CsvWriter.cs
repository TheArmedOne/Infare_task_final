using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Infare_task_final
{
    // Handles the creation of CSV files from processed flight data.
    public class CsvWriter
    {
        // Base path for storing the generated CSV files.

        private const string BaseDirectoryPath = @"G:\TestData\"; // Base directory for output files
        private const string BaseFileName = "flight_combinations_"; // Base part of the file name

        // Writes flight combinations and the cheapest combination for a single itinerary to a CSV file.

        public string WriteCombinations(List<FlightCombination> combinations, FlightCombination cheapestCombination, FlightSearchContext context)
        {
            try
            {
                // Prepare the CSV header and initial content lines.
                var lines = new List<string>
                {
                    // CSV header
                    "Price,Taxes,outbound 1 airport departure,outbound 1 airport arrival,outbound 1 time departure,outbound 1 time arrival,outbound 1 flight number,outbound 2 airport departure,outbound 2 airport arrival,outbound 2 time departure,outbound 2 time arrival,outbound 2 flight number,inbound 1 airport departure,inbound 1 airport arrival,inbound 1 time departure,inbound 1 time arrival,inbound 1 flight number,inbound 2 airport departure,inbound 2 airport arrival,inbound 2 time departure,inbound 2 time arrival,inbound 2 flight number"
                };

                // Add each flight combination to the content lines.
                foreach (var combination in combinations)
                {
                    var line = new StringBuilder();
                    line.Append($"{combination.TotalPrice},{combination.Taxes},");

                    AppendFlightDetailsOrPlaceholder(combination.OutboundJourney?.Flights ?? new List<Flight>(), line);
                    AppendFlightDetailsOrPlaceholder(combination.InboundJourney?.Flights ?? new List<Flight>(), line);

                    lines.Add(line.ToString());
                }

                // Add a section for the cheapest combination.
                lines.Add("\nCheapest Combinations");

                if (cheapestCombination != null)
                {
                    var line = new StringBuilder();
                    line.Append($"{cheapestCombination.TotalPrice},{cheapestCombination.Taxes},");

                    AppendFlightDetailsOrPlaceholder(cheapestCombination.OutboundJourney?.Flights ?? new List<Flight>(), line);
                    AppendFlightDetailsOrPlaceholder(cheapestCombination.InboundJourney?.Flights ?? new List<Flight>(), line);

                    lines.Add(line.ToString());
                }

                // Generate the file name and path based on the context
                string filePath = GenerateFileName(context);

                // Write all lines to the CSV file
                File.WriteAllLines(filePath, lines);

                // Return the path of the generated file
                return filePath;
            }

            // Currently, if error occurs the exception message is printed to console.
            // If (and likely) required, usage of Nlog, Serilog or a different logging framework can be implemented. 

            catch (IOException ex)
            {
                // Handle file I/O exceptions
                Console.WriteLine($"An error occurred while writing to the file: {ex.Message}");
                return null; // Return null to indicate failure
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return null; // Return null to indicate failure
            }
        }

        
        // Async method for writing multiple combinations
        public async Task WriteMultipleCombinationsAsync(IEnumerable<(List<FlightCombination> Combinations, FlightCombination CheapestCombination, FlightSearchContext Context)> blocks)
        {
            try
            {
                string baseFilePath = Path.Combine(BaseDirectoryPath, "combined_flight_data");
                string filePath = $"{baseFilePath}.csv";
                int fileIndex = 1;
                while (File.Exists(filePath))
                {
                    filePath = $"{baseFilePath}_{fileIndex}.csv";
                    fileIndex++;
                }

                using (var writer = new StreamWriter(filePath, append: true))
                {
                    foreach (var block in blocks)
                    {
                        await writer.WriteLineAsync($"Context: {block.Context.DepartureAirport} to {block.Context.ArrivalAirport}, Dates: {block.Context.OutboundDate:yyyy-MM-dd} to {block.Context.InboundDate:yyyy-MM-dd}");

                        // Write the header for each new itinerary block
                        var header = "Price,Taxes,outbound 1 airport departure,outbound 1 airport arrival,outbound 1 time departure,outbound 1 time arrival,outbound 1 flight number,outbound 2 airport departure,outbound 2 airport arrival,outbound 2 time departure,outbound 2 time arrival,outbound 2 flight number,inbound 1 airport departure,inbound 1 airport arrival,inbound 1 time departure,inbound 1 time arrival,inbound 1 flight number,inbound 2 airport departure,inbound 2 airport arrival,inbound 2 time departure,inbound 2 time arrival,inbound 2 flight number";
                        await writer.WriteLineAsync(header);

                        // Writing all combinations for the current itinerary
                        foreach (var combination in block.Combinations)
                        {
                            var line = FormatCombinationLine(combination);
                            await writer.WriteLineAsync(line);
                        }

                        // Write the header again before the cheapest combination
                        await writer.WriteLineAsync("\nCheapest Combination");
                        await writer.WriteLineAsync(header); // Header repeated for clarity

                        // Writing the cheapest combination for the context
                        var cheapestLine = FormatCombinationLine(block.CheapestCombination);
                        await writer.WriteLineAsync(cheapestLine);

                        // Separator between blocks for better readability, if not the last block
                        if (!blocks.Last().Equals(block))
                        {
                            await writer.WriteLineAsync("\n--------------------------------------------------------------------------------\n");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        //Helper for writing multiple combinations
        private string FormatCombinationLine(FlightCombination combination)
        {
            var line = new StringBuilder();
            line.Append($"{combination.TotalPrice},{combination.Taxes},");

            AppendFlightDetailsOrPlaceholder(combination.OutboundJourney?.Flights ?? new List<Flight>(), line);
            AppendFlightDetailsOrPlaceholder(combination.InboundJourney?.Flights ?? new List<Flight>(), line);

            return line.ToString();
        }

        // File name generation
        private string GenerateFileName(FlightSearchContext context)
        {
            
            string formattedOutboundDate = context.OutboundDate.ToString("yyyy-MM-dd");
            string formattedInboundDate = context.InboundDate.ToString("yyyy-MM-dd");

            
            string fileName = $"{context.DepartureAirport}_{context.ArrivalAirport}_{formattedOutboundDate}-{formattedInboundDate}.csv";
            string filePath = Path.Combine(BaseDirectoryPath, fileName);

            
            int fileIndex = 1;
            while (File.Exists(filePath))
            {
               
                filePath = Path.Combine(BaseDirectoryPath, $"{context.DepartureAirport}_{context.ArrivalAirport}_{formattedOutboundDate}-{formattedInboundDate}_{fileIndex}.csv");
                fileIndex++;
            }

            return filePath;
        }


        // Helper to work with empty flight data, or multiple identifiers. 
        private void AppendFlightDetailsOrPlaceholder(List<Flight> flights, StringBuilder line, int maxFlights = 2)
        {
            for (int i = 0; i < maxFlights; i++)
            {
                if (i < flights.Count)
                {
                    var flight = flights[i];
                    line.Append($"{flight.AirportDeparture.Code},{flight.AirportArrival.Code},{flight.DateDeparture:yyyy-MM-dd HH:mm},{flight.DateArrival:yyyy-MM-dd HH:mm},{flight.FullFlightNumber}");
                }
                else
                {
                    line.Append(",,,,"); 
                }

                if (i < maxFlights - 1) line.Append(","); 
            }
        }



    }
}
