## Usage

This project is designed to process flight itinerary data, identify flight combinations, and determine the cheapest options. It includes classes for scraping flight data (`WebScraper`), calculating prices (`PricingCalculator`), managing flight data structures (`FlightData`, `Journey`, `Flight`, etc.), and writing results to CSV files (`CsvWriter`). The `Program.cs` serves as the entry point, providing a simple CLI for interacting with the core functionalities.

### Running the Application

#### Starting the Application:

- Run `Program.cs` to initiate the command-line interface (CLI) which guides you through the available options for processing flight itineraries.

#### Processing Options:

The CLI provides three options:

1. **Manual insertion of a single flight itinerary.**
2. **Automatic processing of pre-defined multiple itineraries.**
3. **Manual insertion of a single flight itinerary with an additional connection airport code.**

#### Inputting Data:

- For options 1 and 3, follow the prompts to input departure and arrival airports, outbound and return dates, and (if applicable) a connection airport code.
- For option 2, the application processes a pre-defined set of itineraries defined within `ProcessMultipleItineraries` method.

#### Flight Data Processing:

- The `FlightDataProcessor` class is instantiated and called to handle the flight data based on the selected option. It utilizes the `WebScraper` to fetch data, processes the data to find all possible combinations and the cheapest flights, and finally, outputs the results to CSV files using `CsvWriter`.

## Future improvements

- Error handling at the moment is incomplete, in some cases where data is inconsistent, unhandled exceptions may occur.
- Adding support for a logging framework instead of printing caught exceptions to console
- Unit testing for critical methods in CsvWriter and FlightDataProcessor
- Further optimization for handling very large datasets
