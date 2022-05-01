using BottomhalfCore.Services.Interface;
using System;
using TimeZoneConverter;

namespace BottomhalfCore.Services.Code
{
    public class TimezoneConverter : ITimezoneConverter
    {
        public DateTime ToUtcTime(DateTime now)
        {
            return TimeZoneInfo.ConvertTimeToUtc(now);
        }

        public DateTime ToIstTime(DateTime now)
        {
            TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(now, istTimeZome);
        }

        public DateTime ZeroTime(DateTime now)
        {
            return new DateTime(now.Year, now.Month, now.Day);
        }

        public DateTime IstZeroTime(DateTime now)
        {
            TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(now, istTimeZome);
        }

        public int GetWeekNumberOfMonth(DateTime date, bool IsLastDayOfWeek)
        {
            date = date.Date;
            int weekOfMonth = 0;
            DateTime firstMonthDay = new DateTime(date.Year, date.Month, 1);
            DateTime firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);
            DateTime previousMonthLastMonday = firstMonthDay.AddDays((DayOfWeek.Monday - 7 - firstMonthDay.DayOfWeek) % 7);
            if (firstMonthMonday > date)
            {
                weekOfMonth = 1;
            }
            else
            {
                weekOfMonth = 2;
                int suppliedDay = date.Day / 7;
                if (!IsLastDayOfWeek)
                    suppliedDay += 1;
                var nextDate = previousMonthLastMonday.AddDays(7 * suppliedDay);
                int noOfDays = nextDate.Day - firstMonthMonday.Day;
                weekOfMonth = weekOfMonth + noOfDays / 7;
            }
            return weekOfMonth;
        }

        public int MondaysInMonth(DateTime thisMonth)
        {
            int mondays = 0;
            int month = thisMonth.Month;
            int year = thisMonth.Year;
            int daysThisMonth = DateTime.DaysInMonth(year, month);
            bool isFirstDayIsMonday = false;
            DateTime beginingOfThisMonth = new DateTime(year, month, 1);
            for (int i = 0; i < daysThisMonth; i++)
                if (beginingOfThisMonth.AddDays(i).DayOfWeek == DayOfWeek.Monday)
                {
                    if (i == 0)
                        isFirstDayIsMonday = true;
                    mondays++;
                }

            if (!isFirstDayIsMonday)
                mondays++;
            return mondays;
        }

        public double GetBusinessDays(DateTime startD, DateTime endD)
        {
            double calcBusinessDays =
                1 + ((endD - startD).TotalDays * 5 -
                (startD.DayOfWeek - endD.DayOfWeek) * 2) / 7;

            if (endD.DayOfWeek == DayOfWeek.Saturday) calcBusinessDays--;
            if (startD.DayOfWeek == DayOfWeek.Sunday) calcBusinessDays--;

            return calcBusinessDays;
        }

        public DateTime GetUtcDateTime(int year, int month, int day)
        {
            DateTime now = new DateTime(year, month, day);
            DateTime.SpecifyKind(now, DateTimeKind.Utc);
            return now;
        }

        /// <summary>
        /// This method will return the first day of given month and year. If month and year is 0 then current year and month will be used.
        /// </summary>
        public DateTime GetUtcFirstDay(int year = 0, int month = 0)
        {
            DateTime utc = DateTime.UtcNow;
            if (year == 0)
                year = utc.Year;
            if (month == 0)
                month = utc.Month;

            DateTime now = new DateTime(year, month, 1);
            DateTime.SpecifyKind(now, DateTimeKind.Utc);
            now = TimeZoneInfo.ConvertTimeToUtc(now);
            return now;
        }

        /// <summary>
        /// Get first day of the present week or specified date.
        /// </summary>
        public DateTime FirstDayOfWeekUTC(Nullable<DateTime> now = null)
        {
            DateTime workingDate = DateTime.UtcNow;
            if (now != null)
                workingDate = (DateTime)now;

            return workingDate.AddDays(-(int)workingDate.DayOfWeek);
        }

        /// <summary>
        /// Get first day of the present week or specified date.
        /// </summary>
        public DateTime LastDayOfWeekUTC(Nullable<DateTime> now = null)
        {
            DateTime workingDate = this.FirstDayOfWeekUTC(now);
            return workingDate.AddDays(7).AddSeconds(-1);
        }

        /// <summary>
        /// Get first day of the present week or specified date.
        /// </summary>
        public DateTime FirstDayOfWeekIST(Nullable<DateTime> now = null)
        {
            DateTime workingDate = DateTime.UtcNow;
            if (now != null)
                workingDate = (DateTime)now;
            TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
            int day = (int)workingDate.DayOfWeek;
            if (day == 0)
                day = 7;
            day--;
            workingDate = workingDate.AddDays(-day);
            workingDate = TimeZoneInfo.ConvertTimeFromUtc(workingDate, istTimeZome);
            return workingDate;
        }

        /// <summary>
        /// Get first day of the present week or specified date.
        /// </summary>
        public DateTime LastDayOfWeekIST(Nullable<DateTime> now = null)
        {
            DateTime workingDate = this.FirstDayOfWeekIST(now);
            workingDate = workingDate.AddDays(6).AddSeconds(-1);
            return workingDate;
        }
    }
}
