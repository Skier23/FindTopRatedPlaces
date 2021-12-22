using System;
using System.Collections.Generic;
using System.IO;
using GeoCoordinatePortable;

namespace GoogleLocationsNearMe
{
    public static class Program
    {
        public static GeoCoordinate originCoord;
        static void Main(string[] args)
        {
            double originlongitude;
            double originlatitude;

            Console.WriteLine(@"Enter your google maps api key (see for more info: https://developers.google.com/maps/documentation/javascript/get-api-key)");
            API.apiKey = Console.ReadLine();

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
            List<Place> placeResults = DataProcessor.FindPlacesInCriteria(originCoord, radius, placeType);
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
        public static void writeToCsv(string csvPath, List<Place> toWrite)
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
        public static void writeToCsvHelper(string csvPath, List<Place> toWrite)
        {
            using (StreamWriter writer = new StreamWriter(new FileStream(csvPath,
               FileMode.Create, FileAccess.Write)))
            {
                writer.WriteLine("sep=,");
                // We write out quite a few different fields of info on each place so that most use cases will have enough information. Also, the googleplaceid is included in the results in case more information is required.
                writer.WriteLine("Restaurant Name,Rating,Number of Ratings,Distance(mi),Website,Phone,Address,Sun,Mon,Tues,Wed,Thurs,Fri,Sat,Latitude,Longitude,GooglePlaceId");
                foreach (Place place in toWrite)
                {
                    string[] hours = new string[7];
                    if (place.Details?.Open != null)
                    {
                        foreach (Period timePeriod in place.Details.Open?.Periods)
                        {
                            switch (timePeriod.Open.ParseTime().DayOfWeek)
                            {
                                case DayOfWeek.Monday:
                                    hours[1] = timePeriod.ToString();
                                    break;
                                case DayOfWeek.Tuesday:
                                    hours[2] = timePeriod.ToString();
                                    break;
                                case DayOfWeek.Wednesday:
                                    hours[3] = timePeriod.ToString();
                                    break;
                                case DayOfWeek.Thursday:
                                    hours[4] = timePeriod.ToString();
                                    break;
                                case DayOfWeek.Friday:
                                    hours[5] = timePeriod.ToString();
                                    break;
                                case DayOfWeek.Saturday:
                                    hours[6] = timePeriod.ToString();
                                    break;
                                case DayOfWeek.Sunday:
                                    hours[0] = timePeriod.ToString();
                                    break;
                            }
                        }
                    }
                    GeoCoordinate thisCoord = new GeoCoordinate(place.Geo.Location.Latitude, place.Geo.Location.Longitude);
                    writer.WriteLine($"\"{NullCheck(place.Name)}\",{place.Details?.Rating},{place.Details?.TotalRatings},{String.Format("{0:0.##}", thisCoord.GetDistanceTo(originCoord) * 0.000621371192)},{NullCheck(place.Details?.Website)},{NullCheck(place.Details?.Phone)},\"{NullCheck(place.Address)}\",{NullCheck(hours[0])},{NullCheck(hours[1])}," +
                        $"{NullCheck(hours[2])},{NullCheck(hours[3])},{NullCheck(hours[4])},{NullCheck(hours[5])},{NullCheck(hours[6])},{place.Geo.Location.Latitude},{place.Geo.Location.Longitude},{place.PlaceId}");
                }
            }
        }
    }
}

