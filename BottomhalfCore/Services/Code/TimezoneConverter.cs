using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using System;
using System.Linq;
using TimeZoneConverter;

namespace BottomhalfCore.Services.Code
{
    public class TimezoneConverter : ITimezoneConverter
    {
        public DateTime ToUtcTime(DateTime now)
        {
            return TimeZoneInfo.ConvertTimeToUtc(now);
        }

        public static int GetNumberOfWeekdaysInMonth(int year, int month)
        {
            return Enumerable.Range(1, DateTime.DaysInMonth(year, month))
                             .Select(day => new DateTime(year, month, day))
                             .Where(dt => dt.DayOfWeek != DayOfWeek.Sunday &&
                                          dt.DayOfWeek != DayOfWeek.Saturday)
                             .Count();
        }

        public DateTime ToUtcTimeFromMidNightTimeZone(DateTime now, TimeZoneInfo timeZoneInfo)
        {
            var dateTimeUnspec = DateTime.SpecifyKind(now.Date, DateTimeKind.Unspecified);
            var timeZoneDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTimeUnspec, timeZoneInfo);
            return timeZoneDateTime;
        }

        public DateTime UpdateToUTCTimeZoneOnly(DateTime now)
        {
            DateTime utcNow = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond, DateTimeKind.Utc);
            return utcNow;
        }

        public DateTime ToSpecificTimezoneDateTime(TimeZoneInfo timeZoneInfo, DateTime? now = null)
        {
            DateTime present = DateTime.Now;
            if (now != null)
                present = Convert.ToDateTime(now);

            var dateTimeUnspec = DateTime.SpecifyKind(present, DateTimeKind.Unspecified);
            var timeZoneDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTimeUnspec, timeZoneInfo);
            return timeZoneDateTime;
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
        /// This method will return the last day of given month and year. If month and year is 0 then current year and month will be used.
        /// </summary>
        public DateTime GetUtcLastDay(int year = 0, int month = 0)
        {
            DateTime utc = DateTime.UtcNow;
            if (year == 0)
                year = utc.Year;
            if (month == 0)
                month = utc.Month;

            DateTime now = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            DateTime.SpecifyKind(now, DateTimeKind.Utc);
            now = TimeZoneInfo.ConvertTimeToUtc(now);
            return now;
        }

        /// <summary>
        /// This method will return the first day of given month and year.
        /// </summary>
        public DateTime GetFirstDateOfMonth(DateTime presentDate, TimeZoneInfo timeZoneInfo)
        {
            DateTime now = TimeZoneInfo.ConvertTime(presentDate, timeZoneInfo);
            DateTime firstDate = new DateTime(now.Year, now.Month, 1);
            return firstDate;
        }

        /// <summary>
        /// This method will return the last day of given month and year.
        /// </summary>
        public DateTime GetLastDateOfMonth(DateTime presentDate, TimeZoneInfo timeZoneInfo)
        {
            DateTime now = TimeZoneInfo.ConvertTime(presentDate, timeZoneInfo);
            DateTime lastDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
            return lastDate;
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
        /// Convert datetime into specific timezone
        /// </summary>
        public DateTime ToTimeZoneDateTime(DateTime now, TimeZoneInfo timeZoneInfo)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(now, timeZoneInfo);
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

        /// <summary>
        /// Get first day of the present week or specified date.
        /// </summary>
        public DateTime FirstDayOfPresentWeek(DateTime now, TimeZoneInfo timeZoneInfo)
        {
            DateTime workingDate = DateTime.UtcNow;
            if (now != null)
                workingDate = (DateTime)now;

            int day = (int)workingDate.DayOfWeek;
            if (day == 0)
                day = 7;
            day--;
            workingDate = workingDate.AddDays(-day);
            workingDate = TimeZoneInfo.ConvertTimeFromUtc(workingDate, timeZoneInfo);
            return workingDate;
        }

        /// <summary>
        /// Get first day of the present week or specified date.
        /// </summary>
        public DateTime LastDayOfPresentWeek(DateTime now, TimeZoneInfo timeZoneInfo)
        {
            DateTime workingDate = this.FirstDayOfPresentWeek(now, timeZoneInfo);
            workingDate = workingDate.AddDays(6).AddSeconds(-1);
            return workingDate;
        }

        public int TotalWeekEndsBetweenDates(DateTime fromDate, DateTime toDate, TimeZoneInfo timeZoneInfo = null)
        {
            int totalWeekDays = 0;

            if (toDate.Subtract(fromDate).TotalDays > 0)
                throw new HiringBellException("ToDate must be greater then the FromDate.");

            if (timeZoneInfo != null)
            {
                fromDate = TimeZoneInfo.ConvertTimeFromUtc(fromDate, timeZoneInfo);
                toDate = TimeZoneInfo.ConvertTimeToUtc(toDate, timeZoneInfo);
            }

            while (toDate.Subtract(fromDate).TotalDays <= 0)
            {
                if (toDate.DayOfWeek == DayOfWeek.Sunday)
                    totalWeekDays++;

                if (toDate.DayOfWeek == DayOfWeek.Saturday)
                    totalWeekDays++;

                toDate = toDate.AddDays(1);
            }

            return totalWeekDays;
        }

        public int TotalWeekDaysBetweenDates(DateTime fromDate, DateTime toDate, TimeZoneInfo timeZoneInfo = null)
        {
            int totalWeekDays = 0;

            if (toDate.Subtract(fromDate).TotalDays > 0)
                throw new HiringBellException("ToDate must be greater then the FromDate.");

            if (timeZoneInfo != null)
            {
                fromDate = TimeZoneInfo.ConvertTimeFromUtc(fromDate, timeZoneInfo);
                toDate = TimeZoneInfo.ConvertTimeToUtc(toDate, timeZoneInfo);
            }

            while (toDate.Subtract(fromDate).TotalDays <= 0)
            {
                if (toDate.DayOfWeek != DayOfWeek.Sunday && toDate.DayOfWeek != DayOfWeek.Saturday)
                    totalWeekDays++;

                toDate = toDate.AddDays(1);
            }

            return totalWeekDays;
        }

        public int TotalSaturdayBetweenDates(DateTime fromDate, DateTime toDate, TimeZoneInfo timeZoneInfo = null)
        {
            int totalWeekDays = 0;

            if (toDate.Subtract(fromDate).TotalDays > 0)
                throw new HiringBellException("ToDate must be greater then the FromDate.");

            if (timeZoneInfo != null)
            {
                fromDate = TimeZoneInfo.ConvertTimeFromUtc(fromDate, timeZoneInfo);
                toDate = TimeZoneInfo.ConvertTimeToUtc(toDate, timeZoneInfo);
            }

            while (toDate.Subtract(fromDate).TotalDays <= 0)
            {
                if (toDate.DayOfWeek == DayOfWeek.Saturday)
                    totalWeekDays++;

                toDate = toDate.AddDays(1);
            }

            return totalWeekDays;
        }

        public int TotalSundayBetweenDates(DateTime fromDate, DateTime toDate, TimeZoneInfo timeZoneInfo = null)
        {
            int totalWeekDays = 0;

            if (toDate.Subtract(fromDate).TotalDays > 0)
                throw new HiringBellException("ToDate must be greater then the FromDate.");

            if (timeZoneInfo != null)
            {
                fromDate = TimeZoneInfo.ConvertTimeFromUtc(fromDate, timeZoneInfo);
                toDate = TimeZoneInfo.ConvertTimeToUtc(toDate, timeZoneInfo);
            }

            while (toDate.Subtract(fromDate).TotalDays <= 0)
            {
                if (toDate.DayOfWeek == DayOfWeek.Sunday)
                    totalWeekDays++;

                toDate = toDate.AddDays(1);
            }

            return totalWeekDays;
        }
    }
}
