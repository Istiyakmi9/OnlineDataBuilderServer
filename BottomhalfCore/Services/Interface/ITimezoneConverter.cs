using System;

namespace BottomhalfCore.Services.Interface
{
    public interface ITimezoneConverter
    {
        DateTime ToUtcTime(DateTime now);
        DateTime ToIstTime(DateTime now);
        int GetWeekNumberOfMonth(DateTime date, bool IsLastDayOfWeek);
        int MondaysInMonth(DateTime thisMonth);
        double GetBusinessDays(DateTime startD, DateTime endD);
    }
}
