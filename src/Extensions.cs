using System;
using System.Collections.Generic;

namespace WaterBot
{
    public static class Extensions
    {
        public static IEnumerable<IEnumerable<char>> ChunkBy(this string input, int n)
        {
            var list = new List<char>();
            int i = 0;
            foreach (var element in input)
            {
                list.Add(element);
                i++;
                if (i == n)
                {
                    yield return list;
                    list = new List<char>();
                    i = 0;
                }
            }
            if (i != 0)
                yield return list;
        }

        public static TimeSpan KeepHoursMinutes(this TimeSpan ts) => new TimeSpan(ts.Hours, ts.Minutes, 0);
    }
}
