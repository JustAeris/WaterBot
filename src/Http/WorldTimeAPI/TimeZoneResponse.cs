
using System;

namespace WaterBot.Http.WorldTimeAPI
{
    public struct TimeZoneResponse
    {
        public string Region;
        public string Name;

        public int UtcOffset;

        public TimeZoneResponse(string region, string name, int offset)
        {
            Region = region;
            Name = name;

            UtcOffset = offset;
        }
    }
}
