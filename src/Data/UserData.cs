using System;
using DSharpPlus.Entities;

namespace WaterBot.Data
{
    public class UserData
    {
        public DiscordUser User { get; set; }

        public TimeSpan WakeTime { get; set; }
        public TimeSpan SleepTime { get; set; }

        public int AmountPerInterval { get; set; }

        public UserData(DiscordUser user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public UserData(DiscordUser user, TimeSpan wakeTime, TimeSpan sleepTime, int amountPerInterval)
            : this(user)
        {
            WakeTime = wakeTime;
            SleepTime = sleepTime;

            AmountPerInterval = amountPerInterval;
        }
    }
}
