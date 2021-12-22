using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GoogleLocationsNearMe
{
    class API
    {
        // Some of these api calls come from: https://github.com/axismb/Google-Places

        private static HttpClient client { get; set; }
        internal static String apiKey;
        static API()
        {
            apiKey = "";
            client = new HttpClient();
            client.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/place/");
        }

        static async public Task<Response> FindNearbyPlaces(string placeType, string latitude, string longitude, int range=3500)
        {
            try
            {
                string url = String.Format("nearbysearch/json?key={0}&location={1},{2}&radius={4}&type={3}", apiKey, latitude, longitude, placeType, range);
                var resp = await client.GetAsync(url);
                if (resp.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject(await resp.Content.ReadAsStringAsync(), typeof(Response)) as Response;
                }
                else
                {
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
        /// Gets another set of 20 places that match a previous query.
        /// </summary>
        /// <param name="token">Next Token to fetch.</param>
        async public static Task<Response> GetNext(string token)
        {
            try
            {
                var resp = await client.GetAsync(String.Format("nearbysearch/json?key={0}&pagetoken={1}", apiKey, token));
                if (resp.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject(await resp.Content.ReadAsStringAsync(), typeof(Response)) as Response;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get full details of specified place ID.
        /// </summary>
        /// <param name="placeId">ID of place.</param>
        async public static Task<Detail> GetDetails(string placeId)
        {
            try
            {
                var resp = await client.GetAsync(String.Format("details/json?key={0}&placeid={1}&fields=rating%2Cuser_ratings_total%2Cformatted_phone_number%2Cwebsite%2Copening_hours", apiKey, placeId));
                if (resp.IsSuccessStatusCode)
                {
                    return (JsonConvert.DeserializeObject(await resp.Content.ReadAsStringAsync(), typeof(Response)) as Response).Detail;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

    }
}
