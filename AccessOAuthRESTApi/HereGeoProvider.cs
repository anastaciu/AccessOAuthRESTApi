using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AccessOAuthRESTApi
{
    public class HereGeoProvider
    {
        private readonly AsyncRetryPolicy<WebResponse> _asyncRetryPolicy;

        public HereGeoProvider()
        {
            _asyncRetryPolicy = Policy<WebResponse>.Handle<Exception>(
                ex =>
                {
                    return ex.Message == @"O servidor remoto devolveu um erro: (429) Too Many Requests.";
                }
                ).WaitAndRetryAsync(1000, times => TimeSpan.FromMilliseconds(times * 100));
        }

        public async Task<int> GetDrivingDistance(string fromLat, string fromLng, string toLat, string toLng)
        {
            string composedFrom = string.Concat(fromLat, ",", fromLng);

            string composedTo = string.Concat(toLat, ",", toLng);
            string endpoint = string.Concat(HereURLs.MAIN_ENDPOINT, HereURLs.DISTANCE_BY_LAT_LONG);
            try
            {
                var response = await _asyncRetryPolicy.ExecuteAsync(async () =>
                {
                    WebRequest request = WebRequest.Create(HereURLs.GetUrl(endpoint, composedFrom, composedTo, HereURLs.API_KEY));
                    return await request.GetResponseAsync();
                });

                string content;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    content = reader.ReadToEnd();
                }
                int distance = int.Parse(JObject.Parse(content)["routes"][0]["sections"][0]["summary"]["length"].ToString());
                return distance;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw e;
            }
        }

        public async Task<GeoReference> GetPostalCodeCoordinateAsync(string postalcode, string city)
        {

            try
            {
                WebRequest request;
                if (postalcode.Length > 0 && city.Length > 0)
                {
                    request = WebRequest.Create(HereURLs.GetUrl(HereURLs.ENDPOINT_CITY_ZIP + HereURLs.LOCATION_AND_ZIP_URL, city, postalcode, HereURLs.API_KEY));
                }
                else if (postalcode.Length == 0)
                {
                    request = WebRequest.Create(HereURLs.GetUrl(HereURLs.ENDPOINT_CITY_ZIP + HereURLs.LOCATION_URL, city, HereURLs.API_KEY));
                }
                else
                {
                    request = WebRequest.Create(HereURLs.GetUrl(HereURLs.ENDPOINT_CITY_ZIP + HereURLs.ZIP_URL, postalcode, HereURLs.API_KEY));
                }

                WebResponse response = await request.GetResponseAsync();
                string content;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    content = reader.ReadToEnd();
                }
                JObject hereMapasResponse = JObject.Parse(content);

                return new GeoReference()
                {
                    Lat = hereMapasResponse["items"][0]["position"]["lat"].ToString(),
                    Lon = hereMapasResponse["items"][0]["position"]["lng"].ToString()
                };
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
                throw e;
            }
        }

        private static class HereURLs
        {
            public const string API_KEY = @"7VXS5gqCib5FXDY_zuQ0frsLPJG8js7qwrJoXM_zYt8";
            public const string MAIN_ENDPOINT = @"https://router.hereapi.com";
            public const string ENDPOINT_CITY_ZIP = @"https://geocode.search.hereapi.com";
            public const string LOCATION_AND_ZIP_URL = @"/v1/geocode?qq=country=portugal;city={0};postalCode={1}&apikey={2}";
            public const string LOCATION_URL = @"/v1/geocode?qq=country=portugal;city={0}&apikey={1}";
            public const string ZIP_URL = @"/v1/geocode?qq=country=portugal;postalCode={0}&apikey={1}";
            public const string DISTANCE_BY_LAT_LONG = @"/v8/routes?transportMode=car&origin={0}&destination={1}&return=summary&apiKey={2}";

            public static string GetUrl(string url, params string[] vars)
            {
                if (!string.IsNullOrWhiteSpace(url) && vars != null)
                {
                    return string.Format(url, vars);
                }
                return string.Empty;
            }
        }
    }
}
