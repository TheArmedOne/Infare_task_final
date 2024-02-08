using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infare_task_final
{

    public static class FlightUtils
    {

        //deprecated, kept it in if needed to use without FlightSearchContext


/*        public static string GetSearchUrl(string departureAirport, string arrivalAirport, string outboundDate, string returnDate)
        {
            // Print the itinerary details
            Console.WriteLine($"Itinerary Details:\nDeparture Airport: {departureAirport}\nArrival Airport: {arrivalAirport}\nOutbound Date: {outboundDate}\nReturn Date: {returnDate}");

            // Construct and return the URL
            return $"http://homeworktask.infare.lt/search.php?from={departureAirport}&to={arrivalAirport}&depart={outboundDate}&return={returnDate}";
        }
*/

        // Flight Number Construction
        public static void ConstructFlightNumber(FlightData flightData)
        {
            foreach (var journey in flightData.Body.Data.Journeys)
            {
                foreach (var flight in journey.Flights)
                {
                    flight.FullFlightNumber = $"{flight.companyCode}{flight.number}";
                }
            }
        }
    }
}