using System;
using System.Collections.Generic;

namespace WaterBot.Data
{
    public class UserData
    {
        public ulong UserId { get; set; }

        public ulong GuildId { get; set; }

        public TimeSpan WakeTime { get; set; }

        public TimeSpan SleepTime { get; set; }

        public TimeSpan UtcOffset { get; set; }

        public int AmountPerInterval { get; set; }

        public int AmountPerDay { get; set; }

        public bool ReminderEnabled { get; set; }

        public List<TimeSpan> RemindersList { get; set; }

        public TimeSpan LatestReminder { get; set; }

        public override string ToString()
        {
            return
                $"UserID: {UserId}. WakeTime: {WakeTime}. SleepTime: {SleepTime}. AmountPerInterval: {AmountPerInterval}.";
        }

        public static IEnumerable<TimeSpan> CalculateReminders(UserData userData)
        {
            List<TimeSpan> remindersList = new List<TimeSpan>();

            // ReSharper disable once PossibleLossOfFraction
            int remindersNumber = (int)Math.Floor((double)(userData.AmountPerDay / userData.AmountPerInterval)) ;

            TimeSpan remindersTimespan = userData.SleepTime - userData.WakeTime;

            TimeSpan intervalBetweenEachReminder = (remindersTimespan / remindersNumber).KeepHoursMinutes();

            TimeSpan progression = userData.WakeTime.KeepHoursMinutes();

            while (progression < userData.SleepTime)
            {
                remindersList.Add(progression);
                progression += intervalBetweenEachReminder;
                if (progression <= TimeSpan.FromDays(1)) continue;
                remindersList.Add(TimeSpan.FromDays(1) - TimeSpan.FromMinutes(1));
                break;
            }

            return remindersList;
        }

        public static TimeSpan CalculateLatestReminder(IEnumerable<TimeSpan> remindersList, TimeSpan utc)
        {
            TimeSpan latestReminder = TimeSpan.Zero;

            foreach (TimeSpan timeSpan in remindersList)
            {
                if (timeSpan < utc)
                    latestReminder = timeSpan;
                else break;
            }

            return latestReminder;
        }
    }
}
