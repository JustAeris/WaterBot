
namespace WaterBot.Http.WorldTimeAPI
{
    public struct TimeZoneResponse
    {
        public string Region;
        public string Name;

        public int Offset;

        public TimeZoneResponse(string region, string name, int offset)
        {
            Region = region;
            Name = name;

            Offset = offset;
        }
    }
}
