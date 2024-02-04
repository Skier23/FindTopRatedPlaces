using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GoogleLocationsNearMe
{
    class APIV2
    {
        // Some of these api calls come from: https://github.com/axismb/Google-Places

        private static HttpClient client { get; set; }
        internal static String apiKey;
        static APIV2()
        {
            apiKey = "";
            client = new HttpClient();
            //client.BaseAddress = new Uri("https://places.googleapis.com/v1/");
        }

        static async public Task<ResponseV2> FindNearbyPlaces(string placeType, double latitude, double longitude, double radius = 500)
        {
            try
            {
                // Create the request object
                var requestBody = new
                {
                    includedTypes = new string[] { placeType },
                    locationRestriction = new
                    {
                        circle = new
                        {
                            center = new { latitude, longitude },
                            radius
                        }
                    },
                    // Include any other parameters you need here
                };

                // Serialize the request object to JSON
                var content = new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json");

                // Clear previous headers to avoid issues with multiple calls
                client.DefaultRequestHeaders.Clear();

                // Add the necessary headers
                client.DefaultRequestHeaders.Add("X-Goog-Api-Key", apiKey);
                client.DefaultRequestHeaders.Add("X-Goog-FieldMask", "places.id,places.displayName");

                // Perform the POST request
                // The full URL for the POST request
                string requestUri = "https://places.googleapis.com/v1/places:searchNearby";
                var resp = await client.PostAsync(requestUri, content);
                if (resp.IsSuccessStatusCode)
                {
                    // Deserialize the response object
                    //Console.WriteLine(await resp.Content.ReadAsStringAsync());
                    return JsonConvert.DeserializeObject<ResponseV2>(await resp.Content.ReadAsStringAsync());
                }
                else
                {
                    // Handle error response
                    Console.WriteLine($"Error: {resp.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
            return null;
        }

        /// <summary>
        /// Get full details of specified place ID.
        /// </summary>
        /// <param name="placeId">ID of place.</param>
        async public static Task<DetailsV2> GetDetails(string placeId)
        {
            try
            {
                // Clear previous headers to avoid issues with multiple calls
                client.DefaultRequestHeaders.Clear();

                // Add the necessary headers
                client.DefaultRequestHeaders.Add("X-Goog-Api-Key", apiKey);
                client.DefaultRequestHeaders.Add("X-Goog-FieldMask", "shortFormattedAddress,location,primaryTypeDisplayName,nationalPhoneNumber,rating,userRatingCount,googleMapsUri,websiteUri,priceLevel,currentOpeningHours,editorialSummary,reservable,servesBreakfast,servesLunch,servesDinner,servesVegetarianFood,servesDessert,outdoorSeating");

                // The full URL for the GET request
                string requestUri = $"https://places.googleapis.com/v1/places/{placeId}";

                // Perform the GET request
                var resp = await client.GetAsync(requestUri);
                if (resp.IsSuccessStatusCode)
                {
                    // Deserialize the response object
                    var jsonResponse = await resp.Content.ReadAsStringAsync();
                    //Console.WriteLine(jsonResponse);
                    return JsonConvert.DeserializeObject<DetailsV2>(jsonResponse);
                }
                else
                {
                    // Handle error response
                    Console.WriteLine($"Error: {resp.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
            return null;
        }

    }
}
