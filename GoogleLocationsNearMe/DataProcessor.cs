using GeoCoordinatePortable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleLocationsNearMe
{
    public static class DataProcessor
    {
        public const double DegreesToRadians = Math.PI / 180.0;
        public const double RadiansToDegrees = 180.0 / Math.PI;
        public const double EarthRadius = 6378137.0;
        public const double sqrt2 = 1.41421;

        public static List<Place> placesOutput;
        public static HashSet<string> placesSet;
        public static List<Place> tempPlaceList;
        public static int apiCalls = 0;
        public static int ApiCallLimit = 100000;
        public static string PlaceType;
        public static List<Place> FindPlacesInCriteria(GeoCoordinate originCoordinate, int totalRadius, string placeType)
        {
            try
            {
                placesOutput = new List<Place>();
                placesSet = new HashSet<string>();
                tempPlaceList = new List<Place>();
                PlaceType = placeType;

                int quadrantDiameter;
                int quadrantRadius;

                // try starting with more subsquares if the area is larger to try to limit the number of api calls
                if (totalRadius > 50000)
                {
                    quadrantDiameter = totalRadius / 8;
                    quadrantRadius = totalRadius / 16;
                }
                else if (totalRadius > 20000)
                {
                    quadrantDiameter = totalRadius / 4;
                    quadrantRadius = totalRadius / 8;
                }
                else
                {
                    quadrantDiameter = totalRadius / 2;
                    quadrantRadius = totalRadius / 4;
                }

                GeoCoordinate NorthCenterCoord = CalculateDerivedPosition(originCoordinate, totalRadius - quadrantRadius, 0);
                GeoCoordinate NorthWestCoord = CalculateDerivedPosition(NorthCenterCoord, totalRadius - quadrantRadius, -90);
                GeoCoordinate thisCoordinate = new GeoCoordinate(NorthCenterCoord.Latitude, NorthCenterCoord.Longitude);

                // we want to divide the area up into a series of equal size squares
                for (int y = quadrantRadius; y <= (2 * totalRadius) - quadrantRadius; y += quadrantDiameter)
                {
                    for (int x = quadrantRadius; x <= (2 * totalRadius) - quadrantRadius; x += quadrantDiameter)
                    {
                        processSquare(thisCoordinate, quadrantRadius);
                        thisCoordinate = CalculateDerivedPosition(thisCoordinate, quadrantDiameter, 90);
                    }
                    thisCoordinate = CalculateDerivedPosition(thisCoordinate, quadrantDiameter, -180);
                    thisCoordinate.Longitude = NorthWestCoord.Longitude;
                }

                processPlaces(tempPlaceList);

                placesOutput = placesOutput.Where(o => o.Details.TotalRatings > 15).OrderByDescending(o => o.Details.Rating).ToList();
                return placesOutput;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        // This will recursively process square grids of the total area to check and keep breaking them up into smaller squares to process if they return the max number of results.
        // This may be possible to multithread these calls to improve performance but it means more api calls/sec which could hit a google api limit
        public static void processSquare(GeoCoordinate center, int squareRadius)
        {
            // The api request searches a circular radius but we are iterating through square areas. The radius of the circle needs to be srt(2) * the radius of the square so that the circle will entirely cover the area of the square.
            // We store the placeid results in a hashset and check them when we add them to the final result to avoid duplicates since there will be overlapping circles.
            int circleRadius = (int)(sqrt2 * (double)squareRadius);
            List<Place> localPlaceList = new List<Place>();
            string next = null;
            apiCalls++;
            if (apiCalls < ApiCallLimit)
            {
                Task<Response> response = API.FindNearbyPlaces(PlaceType, center.Latitude + "", center.Longitude + "", circleRadius);
                response.Wait();
                localPlaceList.AddRange(response.Result.Places);
                next = response.Result.Next;
            }
            while (next != null)
            {
                Thread.Sleep(2000);
                apiCalls++;
                if (apiCalls < ApiCallLimit)
                {
                    var result = API.GetNext(next);
                    result.Wait();
                    while (result.Result.Status.Equals("INVALID_REQUEST"))
                    {
                        Thread.Sleep(1000);
                        apiCalls++;
                        if (apiCalls < ApiCallLimit)
                        {
                            result = API.GetNext(next);
                            result.Wait();
                        }
                    }
                    localPlaceList.AddRange(result.Result.Places);

                    next = result.Result.Next;
                }
            }
            if (localPlaceList.Count == 60)
            {

                // this request is full so divide the square up into 4 smaller subsquares and process them
                int smallerRadius = squareRadius / 2;
                Console.WriteLine($"Info: Full api call at radius {squareRadius}. Recursing downward.");
                GeoCoordinate NorthCenterCoord = CalculateDerivedPosition(center, smallerRadius, 0);
                GeoCoordinate SouthCenterCoord = CalculateDerivedPosition(center, smallerRadius, -180);

                GeoCoordinate NorthWestCoord = CalculateDerivedPosition(NorthCenterCoord, smallerRadius, -90);
                GeoCoordinate NorthEastCoord = CalculateDerivedPosition(NorthCenterCoord, smallerRadius, 90);
                GeoCoordinate SouthWestCoord = CalculateDerivedPosition(SouthCenterCoord, smallerRadius, -90);
                GeoCoordinate SouthEastCoord = CalculateDerivedPosition(SouthCenterCoord, smallerRadius, 90);

                processSquare(NorthWestCoord, smallerRadius);
                processSquare(NorthEastCoord, smallerRadius);
                processSquare(SouthWestCoord, smallerRadius);
                processSquare(SouthEastCoord, smallerRadius);
            }
            else
            {
                tempPlaceList.AddRange(localPlaceList);
            }

        }

        /// <summary>
        /// Calculates the end-point from a given source at a given range (meters) and bearing (degrees).
        /// This methods uses simple geometry equations to calculate the end-point.
        /// </summary>
        /// <param name="source">Point of origin</param>
        /// <param name="range">Range in meters</param>
        /// <param name="bearing">Bearing in degrees</param>
        /// <returns>End-point from the source given the desired range and bearing.</returns>
        /// Source: https://stackoverflow.com/a/1125425/16817983
        public static GeoCoordinate CalculateDerivedPosition(GeoCoordinate source, double range, double bearing)
        {
            var latA = source.Latitude * DegreesToRadians;
            var lonA = source.Longitude * DegreesToRadians;
            var angularDistance = range / EarthRadius;
            var trueCourse = bearing * DegreesToRadians;

            var lat = Math.Asin(
                Math.Sin(latA) * Math.Cos(angularDistance) +
                Math.Cos(latA) * Math.Sin(angularDistance) * Math.Cos(trueCourse));

            var dlon = Math.Atan2(
                Math.Sin(trueCourse) * Math.Sin(angularDistance) * Math.Cos(latA),
                Math.Cos(angularDistance) - Math.Sin(latA) * Math.Sin(lat));

            var lon = ((lonA + dlon + Math.PI) % (Math.PI * 2)) - Math.PI;

            return new GeoCoordinate(
                lat * RadiansToDegrees,
                lon * RadiansToDegrees,
                source.Altitude);
        }

        // Filter out places that are common fast food chains or are already in the output list. Also, get detailed information for each place.
        public static void processPlaces(List<Place> placesToProcess)
        {
            foreach (Place place in placesToProcess)
            {
                if (!placesSet.Contains(place.PlaceId))
                {
                    string filteredName = place.Name.Replace(" ", "").Replace("-", "").Replace(@$"'", "").ToLowerInvariant();

                    bool notFranchiseChain = true;
                    // A list of common restaurant chains have been created to filter out common restaurants. A similar list would need to be made for any other place types that have common franchise chains.
                    if (PlaceType.Equals("restaurant"))
                    {
                        foreach (string chain in RestaurantChains.RestaurantChainsContainsSet)
                        {
                            if (filteredName.Contains(chain))
                            {
                                notFranchiseChain = false;
                            }
                        }
                        foreach (string chain in RestaurantChains.RestaurantChainsExactSet)
                        {
                            if (filteredName.Equals(chain))
                            {
                                notFranchiseChain = false;
                            }
                        }
                    }
                    if (notFranchiseChain)
                    {
                        placesOutput.Add(place);
                        placesSet.Add(place.PlaceId);
                        apiCalls++;
                        if (apiCalls < ApiCallLimit)
                        {
                            var task = place.GetDetails();
                            task.Wait();
                        }
                    }
                }
            }
        }
    }
}
