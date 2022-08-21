using System;

namespace BottomhalfCore.Services.Interface
{
    public interface ITimezoneConverter
    {
        DateTime ToUtcTime(DateTime now);
        DateTime UpdateToUTCTimeZoneOnly(DateTime now);
        DateTime ToIstTime(DateTime now);
        DateTime ZeroTime(DateTime now);
        DateTime IstZeroTime(DateTime now);
        int GetWeekNumberOfMonth(DateTime date, bool IsLastDayOfWeek);
        int MondaysInMonth(DateTime thisMonth);
        double GetBusinessDays(DateTime startD, DateTime endD);
        DateTime GetUtcDateTime(int year, int month, int day);
        DateTime GetUtcFirstDay(int year = 0, int month = 0);
        DateTime GetUtcLastDay(int year = 0, int month = 0);
        DateTime LastDayOfWeekUTC(Nullable<DateTime> now = null);
        DateTime FirstDayOfWeekUTC(Nullable<DateTime> now = null);
        DateTime LastDayOfWeekIST(Nullable<DateTime> now = null);
        DateTime FirstDayOfWeekIST(Nullable<DateTime> now = null);
        int TotalWeekEndsBetweenDates(DateTime fromDate, DateTime toDate, TimeZoneInfo timeZoneInfo = null);
        int TotalWeekDaysBetweenDates(DateTime fromDate, DateTime toDate, TimeZoneInfo timeZoneInfo = null);
        int TotalSaturdayBetweenDates(DateTime fromDate, DateTime toDate, TimeZoneInfo timeZoneInfo = null);
        int TotalSundayBetweenDates(DateTime fromDate, DateTime toDate, TimeZoneInfo timeZoneInfo = null);
    }
}
