using System;
using System.Collections.Generic;

namespace BottomhalfCore.Services.Interface
{
    public interface ITimezoneConverter
    {
        DateTime ToUtcTime(DateTime now);
        DateTime ToUtcTimeFromMidNightTimeZone(DateTime now, TimeZoneInfo timeZoneInfo);
        DateTime ToTimeZoneDateTime(DateTime now, TimeZoneInfo timeZoneInfo);
        DateTime UpdateToUTCTimeZoneOnly(DateTime now);
        DateTime ToSpecificTimezoneDateTime(TimeZoneInfo timeZoneInfo, DateTime? now = null);
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
        DateTime FirstDayOfPresentWeek(DateTime now, TimeZoneInfo timeZoneInfo);
        DateTime LastDayOfPresentWeek(DateTime now, TimeZoneInfo timeZoneInfo);
        DateTime GetFirstDateOfMonth(DateTime presentDate, TimeZoneInfo timeZoneInfo);
        DateTime GetLastDateOfMonth(DateTime presentDate, TimeZoneInfo timeZoneInfo);
    }
}
