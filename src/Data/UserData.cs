using System;

namespace WaterBot.Data
{
    public class UserData
    {
        public ulong UserId { get; set; }

        public TimeSpan WakeTime { get; set; }

        public TimeSpan SleepTime { get; set; }

        public int AmountPerInterval { get; set; }

        public bool ReminderEnabled { get; set; }

        public override string ToString()
        {
            return
                $"UserID: {UserId}. WakeTime: {WakeTime}. SleepTime: {SleepTime}. AmountPerInterval: {AmountPerInterval}.";
        }
    }
}
