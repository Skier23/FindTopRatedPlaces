using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleLocationsNearMe
{

    // Some of these api classes come from: https://github.com/axismb/Google-Places
    public class Place
    {
        [JsonProperty("name")]
        public string Name { get; set; }        // Name
        [JsonProperty("vicinity")]
        public string Address { get; set; }     // Address
        [JsonProperty("place_id")]
        public string PlaceId { get; set; }     //Place Id
        [JsonProperty("types")]
        public string[] Types { get; set; }     // Categories
        [JsonProperty("opening_hours")]
        public Opening Opened { get; set; }     // Returns null if unavaiable
        [JsonProperty("geometry")]
        public Geometry Geo { get; set; }       // Co-ordinates

        public Detail Details { get; set; }

        async public Task<Detail> GetDetails()
        {
            if (Details != null)
            {
                return Details;
            }
            Details = await API.GetDetails(PlaceId);
            return Details;
        }
    }

    // List of supported place types by the google places api
    public static class PlaceTypes
    {
        public static HashSet<string> SupportedPlaceTypes = new HashSet<string> { "accounting",
            "airport",
            "amusement_park",
            "aquarium",
            "art_gallery",
            "atm",
            "bakery",
            "bank",
            "bar",
            "beauty_salon",
            "bicycle_store",
            "book_store",
            "bowling_alley",
            "bus_station",
            "cafe",
            "campground",
            "car_dealer",
            "car_rental",
            "car_repair",
            "car_wash",
            "casino",
            "cemetery",
            "church",
            "city_hall",
            "clothing_store",
            "convenience_store",
            "courthouse",
            "dentist",
            "department_store",
            "doctor",
            "drugstore",
            "electrician",
            "electronics_store",
            "embassy",
            "fire_station",
            "florist",
            "funeral_home",
            "furniture_store",
            "gas_station",
            "gym",
            "hair_care",
            "hardware_store",
            "hindu_temple",
            "home_goods_store",
            "hospital",
            "insurance_agency",
            "jewelry_store",
            "laundry",
            "lawyer",
            "library",
            "light_rail_station",
            "liquor_store",
            "local_government_office",
            "locksmith",
            "lodging",
            "meal_delivery",
            "meal_takeaway",
            "mosque",
            "movie_rental",
            "movie_theater",
            "moving_company",
            "museum",
            "night_club",
            "painter",
            "park",
            "parking",
            "pet_store",
            "pharmacy",
            "physiotherapist",
            "plumber",
            "police",
            "post_office",
            "primary_school",
            "real_estate_agency",
            "restaurant",
            "roofing_contractor",
            "rv_park",
            "school",
            "secondary_school",
            "shoe_store",
            "shopping_mall",
            "spa",
            "stadium",
            "storage",
            "store",
            "subway_station",
            "supermarket",
            "synagogue",
            "taxi_stand",
            "tourist_attraction",
            "train_station",
            "transit_station",
            "travel_agency",
            "university",
            "veterinary_care",
            "zoo" };
    }

    // List of common restaurant chains so that we can filter these out of the final list to significantly reduce the number of api calls required.
    public static class RestaurantChains
    {
        // It's better to check against the exact names when possible to avoid any cases where a non-fast-food restaurant has a name that includes something like kfc
        // because then it would be filtered out if only using a contains check
        public static List<string> RestaurantChainsExactSet = new List<string> { "mcdonalds",
            "starbucks",
            "chickfila",
            "tacobell",
            "wendys",
            "burgerking",
            "dunkin",
            "subway",
            "dominospizza",
            "chipotlemexicangrill",
            "sonicdrivein",
            "pizzahut",
            "pizzahutexpress",
            "panerabread",
            "kfc",
            "popeyeslouisianakitchen",
            "arbys",
            "dairyqueen",
            "dairyqueen(treat)",
            "dairyqueengrill&chill",
            "littlecaesarspizza",
            "pandaexpress",
            "olivegardenitalianrestaurant",
            "papajohnspizza",
            "buffalowildwings",
            "applebeesgrill+bar",
            "chilisgrill&bar",
            "texasroadhouse",
            "ihop",
            "outbacksteakhouse",
            "zaxbyschickenfingers&buffalowings",
            "hardees",
            "crackerbarreloldcountrystore",
            "dennys",
            "fiveguys",
            "jerseymikessubs",
            "longhornsteakhouse",
            "thecheesecakefactory",
            "bojangles",
            "redrobingourmetburgersandbrews",
            "firehousesubs",
            "qdobamexicaneats",
            "wafflehouse",
            "krispykreme",
            "tgifridays",
            "goldencorralbuffet&grill",
            "mcalistersdeli",
            "hooters",
            "moessouthwestgrill",
            "jasonsdeli",
            "chickensaladchick",
            "jimmyjohns",
            "costcowholesale",
            "costcofoodcourt",
            "shell",
            "wawa",
            "cookout",
            "ocharleysrestaurant&bar",
            "cicis",
            "redlobster",
            "captainds",
            "shoneys",
            "chucke.cheese",
            "sheetz",
            "longjohnsilvers",
            "bp",
        };

        // Some restaurants have to use a contains check rather than an equal because they have a location in their name frequently. Technically this could probably also use a startswith call for all restaurants in one list.
        public static List<string> RestaurantChainsContainsSet = new List<string> { "firehousesubs", "deeprunroadhouse" };

    }

    public class Detail
    {
        [JsonProperty("name")]
        public string Name { get; set; }        // Name
        [JsonProperty("rating")]
        public decimal Rating = -5;             // Rating
        [JsonProperty("price_level")]
        public int Price = -5;                  // Price Rating
        [JsonProperty("formatted_address")]
        public string Address { get; set; }     // Address
        [JsonProperty("formatted_phone_number")]
        public string Phone { get; set; }       // Phone Number
        [JsonProperty("opening_hours")]
        public Opening Open { get; set; }       // Business Hours
        [JsonProperty("user_ratings_total")]
        public int TotalRatings = -1;
        [JsonProperty("website")]
        public string Website { get; set; }
    }

    public class Opening
    {
        [JsonProperty("open_now")]
        public bool Now = false;                // Currently open
        [JsonProperty("periods")]
        public Period[] Periods { get; set; }   // Opened time frames
    }

    public class Period
    {
        [JsonProperty("open")]
        public Range Open { get; set; }         // Opening time
        [JsonProperty("close")]
        public Range Close { get; set; }        // Closing time
        public override string ToString()
        {
            DateTime? openTime = Open?.ParseTime();
            DateTime? closeTime = Close?.ParseTime();
            return $@"{openTime?.ToString("ddd")} {openTime?.ToString("hh:mm tt")} - {closeTime?.ToString("hh:mm tt")}";
        }
    }

    public class Range
    {
        [JsonProperty("day")]
        public int Day { get; set; }
        [JsonProperty("time")]
        public short Time { get; set; }

        public DateTime ParseTime()
        {
            DateTime dt = DateTime.Today.AddDays(Day - (int)DateTime.Today.DayOfWeek);
            return new DateTime(dt.Year, dt.Month, dt.Day, Time / 100, Time % 100, 0);
        }
    }

    public class Geometry
    {
        [JsonProperty("location")]
        public Location Location { get; set; }
    }

    public class Location
    {
        [JsonProperty("lat")]
        public double Latitude { get; set; }
        [JsonProperty("lng")]
        public double Longitude { get; set; }
    }

    public class Response
    {
        [JsonProperty("result")]
        public Detail Detail { get; set; }
        [JsonProperty("results")]
        public List<Place> Places { get; set; }
        [JsonProperty("next_page_token")]
        public string Next { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class ResponseV2
    {
        [JsonProperty("places")]
        public List<PlaceV2> Places { get; set; }

        public override string ToString()
        {
            return GetType().GetProperties()
                .Select(info => (info.Name, Value: info.GetValue(this, null) ?? "(null)"))
                .Aggregate(
                    new StringBuilder(),
                    (sb, pair) => sb.AppendLine($"{pair.Name}: {pair.Value}"),
                    sb => sb.ToString());
        }
    }

    public class LocalizedText
    {
        [JsonProperty("text")]
        public string Name { get; set; }

        [JsonProperty("languageCode")]
        public string LanguageCode { get; set; }

        public override string ToString()
        {
            return GetType().GetProperties()
                .Select(info => (info.Name, Value: info.GetValue(this, null) ?? "(null)"))
                .Aggregate(
                    new StringBuilder(),
                    (sb, pair) => sb.AppendLine($"{pair.Name}: {pair.Value}"),
                    sb => sb.ToString());
        }
    }

    public class LocationV2
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }
        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }

    public enum PriceLevel { 
        PRICE_LEVEL_UNSPECIFIED, PRICE_LEVEL_FREE, PRICE_LEVEL_INEXPENSIVE, PRICE_LEVEL_MODERATE, PRICE_LEVEL_EXPENSIVE, PRICE_LEVEL_VERY_EXPENSIVE 

    }

    public static class PriceLevelExtensions
    {
        public static string ToFriendlyString(this PriceLevel me)
        {
            switch (me)
            {
                case PriceLevel.PRICE_LEVEL_UNSPECIFIED:
                    return "";
                case PriceLevel.PRICE_LEVEL_FREE:
                    return "Free";
                case PriceLevel.PRICE_LEVEL_INEXPENSIVE:
                    return "Inexpensive";
                case PriceLevel.PRICE_LEVEL_MODERATE:
                    return "Moderate Price";
                case PriceLevel.PRICE_LEVEL_EXPENSIVE:
                    return "Expensive";
                case PriceLevel.PRICE_LEVEL_VERY_EXPENSIVE:
                    return "Very Expensive";
                default:
                    return "";
            }
        }
    }

    public class OpeningHours
    {
        [JsonProperty("periods")]
        public PeriodV2[]? Periods { get; set; }     // Address

        public override string ToString()
        {
            return GetType().GetProperties()
                .Select(info => (info.Name, Value: info.GetValue(this, null) ?? "(null)"))
                .Aggregate(
                    new StringBuilder(),
                    (sb, pair) => sb.AppendLine($"{pair.Name}: {pair.Value}"),
                    sb => sb.ToString());
        }
    }

    public class PeriodV2
    {
        [JsonProperty("open")]
        public Point? Open { get; set; }     // Address
        [JsonProperty("close")]
        public Point? Close { get; set; }     // Address

        public override string ToString()
        {
            // Assuming that the day value corresponds to the System.DayOfWeek enum, with 0 being Sunday.
            string openTime = Open?.ToString() ?? "Closed";
            string closeTime = Close?.ToString() ?? "Closed";
            return $"{openTime} - {closeTime}";
        }
    }

    public class Point
    {
        [JsonProperty("day")]
        public int? Day { get; set; }     // Address

        [JsonProperty("hour")]
        public int? Hour { get; set; }     // Address

        [JsonProperty("minute")]
        public int? Minute { get; set; }     // Address

        public override string ToString()
        {
            if (Day.HasValue && Hour.HasValue && Minute.HasValue)
            {
                // Convert the day to DayOfWeek, assuming 0 = Sunday, 1 = Monday, etc.
                var dayOfWeek = (DayOfWeek)(Day.Value % 7);
                var time = new DateTime(1, 1, 1, Hour.Value, Minute.Value, 0);
                // Format the time to be compatible with Google Sheets
                return $"{dayOfWeek} {time:hh:mm tt}";
            }
            else
            {
                return "Time not specified";
            }
        }
    }

    public class DetailsV2
    {
        [JsonProperty("shortFormattedAddress")]
        public string? Address { get; set; }     // Address

        [JsonProperty("location")]
        public LocationV2? Coordinates { get; set; }       // Co-ordinates

        [JsonProperty("primaryTypeDisplayName")]
        public LocalizedText? PrimaryType { get; set; }       // ie Italian-Restaurant

        [JsonProperty("nationalPhoneNumber")]
        public string? PhoneNumber { get; set; }       // Phone Number

        [JsonProperty("rating")]
        public decimal Rating { get; set; }              // Rating

        [JsonProperty("userRatingCount")]
        public int TotalRatings { get; set; }

        [JsonProperty("googleMapsUri")]
        public string? GoogleMapsUri { get; set; }       // Google Maps uri

        [JsonProperty("websiteUri")]
        public string? WebsiteUri { get; set; }       //website

        [JsonProperty("priceLevel")]
        public PriceLevel? Price { get; set; }                  // Price Level


        // TODO OPENING HOURS
        [JsonProperty("currentOpeningHours")]
        public OpeningHours? HoursOpen { get; set; }       //website


        [JsonProperty("editorialSummary")]
        public LocalizedText? EditorialSummary { get; set; }        // Name

        [JsonProperty("reservable")]
        public bool? Reservable { get; set; }        // Name

        [JsonProperty("servesBreakfast")]
        public bool? ServesBreakfast { get; set; }        // Name

        [JsonProperty("servesLunch")]
        public bool? ServesLunch { get; set; }        // Name

        [JsonProperty("servesDinner")]
        public bool? ServesDinner { get; set; }        // Name

        [JsonProperty("servesVegetarianFood")]
        public bool? ServesVegatarian { get; set; }        // Name

        [JsonProperty("servesDessert")]
        public bool? ServesDessert { get; set; }        // Name

        [JsonProperty("outdoorSeating")]
        public bool? OutdoorSeating { get; set; }        // Name


        public override string ToString()
        {
            return GetType().GetProperties()
                .Select(info => (info.Name, Value: info.GetValue(this, null) ?? "(null)"))
                .Aggregate(
                    new StringBuilder(),
                    (sb, pair) => sb.AppendLine($"{pair.Name}: {pair.Value}"),
                    sb => sb.ToString());
        }
    }
    
    public class PlaceV2
    {
        [JsonProperty("displayName")]
        public LocalizedText? Name { get; set; }        // Name
        [JsonProperty("id")]
        public string? PlaceId { get; set; }     //Place Id

        public DetailsV2? Details { get; set; }

        async public Task<DetailsV2> GetDetails()
        {
            if (Details != null)
            {
                return Details;
            }
            Details = await APIV2.GetDetails(PlaceId);
            return Details;
        }

        public override string ToString()
        {
            return GetType().GetProperties()
                .Select(info => (info.Name, Value: info.GetValue(this, null) ?? "(null)"))
                .Aggregate(
                    new StringBuilder(),
                    (sb, pair) => sb.AppendLine($"{pair.Name}: {pair.Value}"),
                    sb => sb.ToString());
        }
    }
}
