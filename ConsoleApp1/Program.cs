
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Select an option:");
        Console.WriteLine("1. Manual insertion of flight itinerary");
        Console.WriteLine("2. Manual insertion of multiple itineraries");
        Console.WriteLine("3. Manual insertion of flight itinerary with a connection airport code");
        string option = Console.ReadLine();

        switch (option)
        {
            case "1":
                await ProcessSingleItinerary();
                break;
            case "2":
                // Placeholder for future implementation
                Console.WriteLine("Option 2 selected. Functionality to be implemented.");
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
        // Prompt user for details
        Console.WriteLine("Enter outbound date (dd/mm/yyyy):");
        string outboundDate = Console.ReadLine();
        Console.WriteLine("Enter return date (dd/mm/yyyy):");
        string returnDate = Console.ReadLine();
        Console.WriteLine("Enter departure airport code:");
        string departureAirport = Console.ReadLine();
        Console.WriteLine("Enter arrival airport code:");
        string arrivalAirport = Console.ReadLine();

        // Construct the URL based on user input (assuming a method to do so)
        string url = GetSearchUrl(departureAirport, arrivalAirport, outboundDate, returnDate);

        // Process the data
        FlightDataProcessor processor = new FlightDataProcessor();
        await processor.ProcessAndWriteFlightData(url, ""); // Assumes an output file path or naming convention
    }

    static async Task ProcessSingleItineraryWithConnection()
    {
        // Similar to ProcessSingleItinerary but also ask for a connection airport code
        Console.WriteLine("Enter connection airport code:");
        string connectionAirport = Console.ReadLine();

        // Implement the logic similar to ProcessSingleItinerary
        // Filter journeys to include only those with the specified connection airport
    }

    // Implement GetSearchUrl and any additional required methods
}
