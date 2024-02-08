using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Infare_task_final
{
    public class CsvWriter
    {
        private const string BaseDirectoryPath = @"G:\TestData\"; // Base directory for output files
        private const string BaseFileName = "flight_combinations_"; // Base part of the file name

        // Writing combinations for single itinerary. 
        public string WriteCombinations(List<FlightCombination> combinations, List<FlightCombination> cheapestCombinations, FlightSearchContext context)
        {
            var lines = new List<string>
    {
        "Price,Taxes,outbound 1 airport departure,outbound 1 airport arrival,outbound 1 time departure,outbound 1 time arrival,outbound 1 flight number,outbound 2 airport departure,outbound 2 airport arrival,outbound 2 time departure,outbound 2 time arrival,outbound 2 flight number,inbound 1 airport departure,inbound 1 airport arrival,inbound 1 time departure,inbound 1 time arrival,inbound 1 flight number,inbound 2 airport departure,inbound 2 airport arrival,inbound 2 time departure,inbound 2 time arrival,inbound 2 flight number"
    };

            
            // Writing all combinations, placeholder appended if no connection flight found
            foreach (var combination in combinations)
            {
                var line = new StringBuilder();
                line.Append($"{combination.TotalPrice},{combination.Taxes},");

                AppendFlightDetailsOrPlaceholder(combination.OutboundJourney?.Flights ?? new List<Flight>(), line);
                AppendFlightDetailsOrPlaceholder(combination.InboundJourney?.Flights ?? new List<Flight>(), line);

                lines.Add(line.ToString());
            }

            // Appending cheapest combinations
            lines.Add("\nCheapest Combinations");
            foreach (var cheapest in cheapestCombinations)
            {
                var line = new StringBuilder();
                line.Append($"{cheapest.TotalPrice},{cheapest.Taxes},");

                AppendFlightDetailsOrPlaceholder(cheapest.OutboundJourney?.Flights ?? new List<Flight>(), line);
                AppendFlightDetailsOrPlaceholder(cheapest.InboundJourney?.Flights ?? new List<Flight>(), line);

                lines.Add(line.ToString());
            }

            string filePath = GenerateFileName(context);
            File.WriteAllLines(filePath, lines);

            return filePath;
        }

        // Writing multiple combinations
        public void WriteMultipleCombinations(IEnumerable<(List<FlightCombination> Combinations, List<FlightCombination> CheapestCombinations, FlightSearchContext Context)> blocks)
        {
            string baseFilePath = Path.Combine("G:\\testdata\\", "combined_flight_data");
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
                    var combinations = block.Combinations;
                    var cheapestCombinations = block.CheapestCombinations;
                    var context = block.Context;

                    // Separator for better readability
                    writer.WriteLine($"Context: Departure Airport: {context.DepartureAirport}, Arrival Airport: {context.ArrivalAirport}, Outbound Date: {context.OutboundDate:yyyy-MM-dd}, Inbound Date: {context.InboundDate:yyyy-MM-dd}, Connection Airport: {context.ConnectionAirportCode ?? "N/A"}");

                    // Header if it's the first block or file is new
                    if (new FileInfo(filePath).Length == 0 || blocks.First().Equals(block))
                    {
                        writer.WriteLine("Price,Taxes,outbound 1 airport departure,outbound 1 airport arrival,outbound 1 time departure,outbound 1 time arrival,outbound 1 flight number,outbound 2 airport departure,outbound 2 airport arrival,outbound 2 time departure,outbound 2 time arrival,outbound 2 flight number,inbound 1 airport departure,inbound 1 airport arrival,inbound 1 time departure,inbound 1 time arrival,inbound 1 flight number,inbound 2 airport departure,inbound 2 airport arrival,inbound 2 time departure,inbound 2 time arrival,inbound 2 flight number");
                    }

                    // Writing all combinations
                    foreach (var combination in combinations)
                    {
                        writer.WriteLine(FormatCombinationLine(combination));
                    }

                    // Appending cheapest combinations
                    writer.WriteLine("\nCheapest Combinations");
                    foreach (var cheapest in cheapestCombinations)
                    {
                        writer.WriteLine(FormatCombinationLine(cheapest));
                    }

                    // Separator between blocks
                    if (!blocks.Last().Equals(block))
                    {
                        writer.WriteLine("\n--------------------------------------------------------------------------------\n");
                    }
                }
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
