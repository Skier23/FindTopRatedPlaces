using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GeoCoordinatePortable;

namespace GoogleLocationsNearMe
{
    public static class Program
    {
        public static GeoCoordinate originCoord;
        static async Task Main(string[] args)
        {
            double originlongitude;
            double originlatitude;

            Console.WriteLine(@"Enter your google maps api key (see for more info: https://developers.google.com/maps/documentation/javascript/get-api-key)");
            APIV2.apiKey = Console.ReadLine();

            Console.WriteLine("Enter the Latitude of the gps coordinate that you want to search from.");
            while (!double.TryParse(Console.ReadLine(), out originlatitude))
            {
                Console.WriteLine("Invalid Input! The input should be a decimal number such as 43.102931");
            }

            Console.WriteLine("Enter the Longitude of the gps coordinate that you want to search from.");
            while (!double.TryParse(Console.ReadLine(), out originlongitude))
            {
                Console.WriteLine("Invalid Input! The input should be a decimal number such as 43.102931");
            }

            originCoord = new GeoCoordinate(originlatitude, originlongitude);
            string placeType = "restaurant";
            Console.WriteLine(@"Enter the place type to search for or just press enter for default (restaurant). For the list of place types, see: https://developers.google.com/maps/documentation/places/web-service/supported_types");
            string tempPlaceTime = Console.ReadLine();
            if (!string.IsNullOrEmpty(tempPlaceTime))
            {
                while (!PlaceTypes.SupportedPlaceTypes.Contains(tempPlaceTime))
                {
                    Console.WriteLine(@$"{tempPlaceTime} is not a supported place type. For all supported place types, see: https://developers.google.com/maps/documentation/places/web-service/supported_types");
                    tempPlaceTime = Console.ReadLine();
                }
                placeType = tempPlaceTime;
            }
            int radius = 35000;
            Console.WriteLine(@"Enter the radius (in meters) that you want to search around the gps coordinate. (Press enter for default of 35,000m or 21.7 miles)");
            string tempPlaceRadius = Console.ReadLine();
            if (!string.IsNullOrEmpty(tempPlaceRadius))
            {
                while (!int.TryParse(tempPlaceRadius, out radius))
                {
                    Console.WriteLine("Invalid Input! The input should be a whole number such as 35000");
                    tempPlaceRadius = Console.ReadLine();
                }
            }

            Console.WriteLine(@"Enter the max api calls if you want to limit api usage and avoid billing charges. Setting a limit here may cause the results to be incomplete. (Press enter for the default of 100000)");
            int maxApiCalls = 100000;
            string maxAPICallsTemp = Console.ReadLine();
            if (!string.IsNullOrEmpty(maxAPICallsTemp))
            {
                while (!int.TryParse(maxAPICallsTemp, out maxApiCalls))
                {
                    Console.WriteLine("Invalid Input! The input should be a whole number such as 35000");
                    maxAPICallsTemp = Console.ReadLine();
                }
            }
            DataProcessor.ApiCallLimit = maxApiCalls;

            Console.WriteLine(@$"Searching for {placeType} in a {radius} meter radius around {originlatitude},{originlongitude}...");
            List<PlaceV2> placeResults = await DataProcessor.FindPlacesInCriteria(originCoord, radius, placeType);

            Console.WriteLine($"Finished processing and found {placeResults.Count} valid results using {DataProcessor.apiCalls} API calls.");
            string filePath = @$"{placeType}Results.csv";
            Console.WriteLine($"Writing results to {filePath}");
            writeToCsv(filePath, placeResults);
            Console.WriteLine($"Finished writing results to {filePath}");
            Console.WriteLine("Press Enter to end");
            Console.ReadLine();
        }

        // Simple helper to return empty string if the string is null
        public static string NullCheck(string toCheck)
        {
            return toCheck == null ? "" : toCheck.ToString();
        }

        // write all the places to a csv file
        public static void writeToCsv(string csvPath, List<PlaceV2> toWrite)
        {
            try
            {
                writeToCsvHelper(csvPath, toWrite);
            }
            catch (Exception e)
            {
                Console.WriteLine("Output file is in use. Close the file and press enter!" + e.Message);
                Console.ReadLine();
                writeToCsvHelper(csvPath, toWrite);
            }
        }

        // CSV writer helper
        public static void writeToCsvHelper(string csvPath, List<PlaceV2> toWrite)
        {
            using (StreamWriter writer = new StreamWriter(new FileStream(csvPath,
               FileMode.Create, FileAccess.Write)))
            {
                writer.WriteLine("sep=,");
                // We write out quite a few different fields of info on each place so that most use cases will have enough information. Also, the googleplaceid is included in the results in case more information is required.
                string placeType = CapitalizeAfter(FirstCharToUpper(DataProcessor.PlaceType).Replace("_", " "), new List<char> { ' ' });
                writer.WriteLine($"{placeType} Name,Primary Type,Price,Editorial Summary,Rating,Number of Ratings,Distance(mi),Website,Google Maps Page,Accept Reservations," +
                    $"Serves Breakfast,Serves Lunch,Serves Dinner,Serves Vegatarian,Serves Dessert,Has Outdoor Seating,Phone,Address,Sun,Mon,Tues,Wed,Thurs,Fri,Sat,Latitude,Longitude,GooglePlaceId");
                foreach (PlaceV2 place in toWrite)
                {
                    string[] hours = new string[7];
                    bool hoursSpecified = place.Details?.HoursOpen is not null;
                    if (hoursSpecified)
                    {
                        foreach (PeriodV2 timePeriod in place.Details.HoursOpen?.Periods)
                        {
                            hours[timePeriod.Open.Day ?? 0] = timePeriod.ToString();
                        }
                    }
                    GeoCoordinate thisCoord = new GeoCoordinate(place.Details.Coordinates.Latitude, place.Details.Coordinates.Longitude);
                    writer.WriteLine($"\"{NullCheck(place.Name.Name)}\",{NullCheck(place.Details?.PrimaryType?.Name)},{(place.Details?.Price ?? PriceLevel.PRICE_LEVEL_UNSPECIFIED).ToFriendlyString()},\"{NullCheck(place.Details?.EditorialSummary?.Name)}\",{place.Details?.Rating},{place.Details?.TotalRatings},{String.Format("{0:0.##}", thisCoord.GetDistanceTo(originCoord) * 0.000621371192)},{NullCheck(place.Details?.WebsiteUri)}," +
                        $"{NullCheck(place.Details?.GoogleMapsUri)},{NullCheck(place.Details?.Reservable?.ToString())},{NullCheck(place.Details?.ServesBreakfast?.ToString())},{NullCheck(place.Details?.ServesLunch?.ToString())},{NullCheck(place.Details?.ServesDinner?.ToString())},{NullCheck(place.Details?.ServesVegatarian?.ToString())},{NullCheck(place.Details?.ServesDessert?.ToString())}," +
                        $"{NullCheck(place.Details?.OutdoorSeating?.ToString())},{NullCheck(place.Details?.PhoneNumber)},\"{NullCheck(place.Details?.Address)}\",{HoursString(hoursSpecified, hours[0])},{HoursString(hoursSpecified, hours[1])}," +
                        $"{HoursString(hoursSpecified, hours[2])},{HoursString(hoursSpecified, hours[3])},{HoursString(hoursSpecified, hours[4])},{HoursString(hoursSpecified, hours[5])},{HoursString(hoursSpecified, hours[6])},{place.Details?.Coordinates?.Latitude + ""},{place.Details?.Coordinates?.Longitude + ""},{place.PlaceId}");
                }
            }
        }

        public static string HoursString(bool hoursOpenSpecified, string? hoursString)
        {
            if (hoursOpenSpecified)
            {
                if (hoursString is null)
                {
                    return "Closed";
                }
                else
                {
                    return hoursString;
                }
            }
            else
            {
                return "";
            }
        }

        public static string FirstCharToUpper(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input[0].ToString().ToUpper() + input.Substring(1)
        };

        public static string CapitalizeAfter(string s, List<char> chars)
        {
            var charsHash = new HashSet<char>(chars);
            StringBuilder sb = new StringBuilder(s);
            for (int i = 0; i < sb.Length - 2; i++)
            {
                if (charsHash.Contains(sb[i]) && sb[i + 1] == ' ')
                    sb[i + 2] = char.ToUpper(sb[i + 2]);
            }
            return sb.ToString();
        }
    }
}

