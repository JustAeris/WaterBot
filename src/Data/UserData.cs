using System;
using DSharpPlus.Entities;

namespace WaterBot.Data
{
    public class UserData
    {
        public ulong UserId { get; set; }

        public TimeSpan WakeTime { get; set; }

        public TimeSpan SleepTime { get; set; }

        public int AmountPerInterval { get; set; }
    }
}
