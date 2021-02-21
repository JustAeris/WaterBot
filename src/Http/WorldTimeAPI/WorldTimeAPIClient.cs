using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WaterBot.Http.WorldTimeAPI
{
    public class WorldTimeApiClient : IDisposable
    {
        public const string BaseUrl = "http://worldtimeapi.org/api/timezone/";

        private HttpClient _client;

        public WorldTimeApiClient()
        {
            _client = new HttpClient();
        }

        public async Task<TimeZoneResponse> GetTimeZone(string zone)
        {
            HttpResponseMessage response = await _client.GetAsync(BaseUrl + zone);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Could not get offset for {zone}: HTTP {(int)response.StatusCode}");

            // Get the content from the response and turn it into a JSON object
            string content = await response.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(content);

            // Read some values from the parsed JSON object
            string timezone = data["timezone"]?.ToString();
            int offset = (int) data["raw_offset"];

            string[] split = timezone?.Split('/');

            if (split != null && split.Length != 2)
                throw new FormatException($"Input string {timezone} is invalid");

            // Return the time zone with split values. The raw offset is returned
            // in seconds, so let's turn that into hours!
            return new TimeZoneResponse(split?[0], split?[1], offset / 3600);
        }

        public async Task<IEnumerable<string>> GetRegions(string region)
        {
            HttpResponseMessage response = await _client.GetAsync(BaseUrl + region);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Could not get regions for {region}: HTTP {(int)response.StatusCode}");

            // Get the content from the response and turn it into a JSON object
            string content = await response.Content.ReadAsStringAsync();
            JArray data = JArray.Parse(content);

            return data.ToObject<List<string>>();
        }

        public void Dispose()
        {
            _client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}