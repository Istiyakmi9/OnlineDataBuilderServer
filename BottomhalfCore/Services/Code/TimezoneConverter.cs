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
    }
}
