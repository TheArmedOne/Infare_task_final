using Infare_task_final; // Assuming this is the namespace of your class library
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Select an option:");
        Console.WriteLine("1. Manual insertion of flight itinerary");
        Console.WriteLine("2. Process pre-defined multiple itineraries");
        Console.WriteLine("3. Manual insertion of flight itinerary with a connection airport code");

        string option = Console.ReadLine();

        switch (option)
        {
            case "1":
                await ProcessSingleItinerary();
                break;
            case "2":
                await ProcessMultipleItineraries();
                break;
            case "3":
                await ProcessSingleItineraryWithConnection();
                break;
            default:
                Console.WriteLine("Invalid option selected.");
                break;
        }
    }

    static async Task ProcessSingleItinerary()
    {
        // Prompt user for details with validation
        string departureAirport = PromptForAirportCode("Enter departure airport code:");
        string arrivalAirport = PromptForAirportCode("Enter arrival airport code:");
        string outboundDate = PromptForDate("Enter outbound date (yyyy-mm-dd):");
        string returnDate = PromptForDate("Enter return date (yyyy-mm-dd):");

        FlightSearchContext context = new FlightSearchContext(departureAirport, arrivalAirport, DateTime.Parse(outboundDate), DateTime.Parse(returnDate));
        FlightDataProcessor processor = new FlightDataProcessor();
        await processor.ProcessAndWriteFlightData(context);
    }

    static async Task ProcessSingleItineraryWithConnection()
    {
        // Similar to ProcessSingleItinerary but also prompt for a connection airport code
        string departureAirport = PromptForAirportCode("Enter departure airport code:");
        string arrivalAirport = PromptForAirportCode("Enter arrival airport code:");
        string outboundDate = PromptForDate("Enter outbound date (yyyy-mm-dd):");
        string returnDate = PromptForDate("Enter return date (yyyy-mm-dd):");
        string connectionAirportCode = PromptForAirportCode("Enter connection airport code:");

        FlightSearchContext context = new FlightSearchContext(departureAirport, arrivalAirport, DateTime.Parse(outboundDate), DateTime.Parse(returnDate))
        {
            ConnectionAirportCode = connectionAirportCode
        };
        FlightDataProcessor processor = new FlightDataProcessor();
        await processor.ProcessSingleItineraryWithConnection(context);
    }

    static async Task ProcessMultipleItineraries()
    {
        List<FlightSearchContext> contexts = new List<FlightSearchContext>();

        // Dates range
        DateTime startDate = DateTime.Parse("2024-02-11");
        DateTime endDate = DateTime.Parse("2024-02-15");

        // Airports
        var routes = new List<(string Departure, string Arrival)>
    {
        ("JFK", "AUH"),
        ("MAD", "AUH")
    };

        foreach (var route in routes)
        {
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                contexts.Add(new FlightSearchContext(route.Departure, route.Arrival, date, date.AddDays(4))); // Assuming a fixed return date 4 days later
            }
        }

        FlightDataProcessor processor = new FlightDataProcessor();
        await processor.ProcessAndWriteMultipleFlightData(contexts);
    }

    static string PromptForAirportCode(string prompt)
    {
        Console.WriteLine(prompt);
        return Console.ReadLine().Trim().ToUpper();
    }

    static string PromptForDate(string prompt)
    {
        Console.WriteLine(prompt);
        return Console.ReadLine().Trim();
    }

    // Add validation methods if needed
}
