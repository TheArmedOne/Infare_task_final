using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infare_task_final
{
    // Represents the complete flight data structure.
    public class FlightData
    {
        public Header Header { get; set; }  // Contains meta-information about the data.
        public Body Body { get; set; } // Contains the actual flight data.
    }
    // Additional classes to suit the response
    public class Header
    {
        public string Message { get; set; }
        public int Code { get; set; }
        public bool Error { get; set; }
        public string BodyType { get; set; }
    }

    public class Body
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public List<Journey> Journeys { get; set; }
        public List<TotalAvailablility> TotalAvailabilities { get; set; }
        
    }

    public class Journey
    {
        public int RecommendationId { get; set; }
        public string Direction { get; set; }
        public List<Flight> Flights { get; set; }
        public double importTaxAdl { get; set; }
        public double importTaxChd { get; set; }
        public double importTaxInf { get; set; }
        public double TotalPrice { get; set; }
        public double Taxes { get; set; }
        public int Identity { get; set; }
        
    }

    public class Flight
    {
        public string number { get; set; }
        public string companyCode { get; set; }
        public Airport AirportDeparture { get; set; }
        public Airport AirportArrival { get; set; }
        public DateTime DateDeparture { get; set; }
        public DateTime DateArrival { get; set; }
        public string FullFlightNumber { get; set; }

    }

    public class Airport
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public class TotalAvailablility
    {
        public int RecommendationId { get; set; }
        public double Total { get; set; }
    }

    public class FlightCombination
    {
        public int RecommendationId { get; set; }
        public Journey OutboundJourney { get; set; }
        public Journey InboundJourney { get; set; }
        public double TotalPrice { get; set; }
        public double Taxes { get; set; }
        public int Identity { get; set; }
        public List<string> OutboundFlightNumbers { get; set; }
        public List<string> InboundFlightNumbers { get; set; }
    }

    public class FlightSearchContext
    {
        public FlightData FlightData { get; set; }
        public string DepartureAirport { get; set; }
        public string ArrivalAirport { get; set; }
        public DateTime OutboundDate { get; set; }
        public DateTime InboundDate { get; set; }
        public string Url { get; private set; } 
        public string ConnectionAirportCode { get; set; }
        // URL Construction when content is created or initiated
        public void ConstructUrl()
        {
            this.Url = $"http://homeworktask.infare.lt/search.php?from={DepartureAirport}&to={ArrivalAirport}&depart={OutboundDate:yyyy-MM-dd}&return={InboundDate:yyyy-MM-dd}";
        }
        // Constructor
        public FlightSearchContext(string departureAirport, string arrivalAirport, DateTime outboundDate, DateTime inboundDate)
        {
            DepartureAirport = departureAirport;
            ArrivalAirport = arrivalAirport;
            OutboundDate = outboundDate;
            InboundDate = inboundDate;
            ConstructUrl(); 
        }
    }

}

