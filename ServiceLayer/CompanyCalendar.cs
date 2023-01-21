using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer
{
    public class CompanyCalendar : ICompanyCalendar
    {
        private List<Calendar> _calendars;
        private readonly IDb _db;
        private readonly CurrentSession _session;

        public CompanyCalendar(IDb db, CurrentSession session)
        {
            _db = db;
            _session = session;
        }

        public async Task LoadHolidayCalendar()
        {
            if (_calendars == null)
                _calendars = _db.GetList<Calendar>("sp_company_calendar_get_by_company", new { CompanyId = _session.CurrentUserDetail.CompanyId });

            await Task.CompletedTask;
        }

        public async Task<bool> IsHoliday(DateTime date)
        {
            bool flag = false;

            var records = _calendars.FirstOrDefault(x => x.EventDate.Date == date.Date);
            if (records != null)
                flag = true;

            return await Task.FromResult(flag);
        }

        public async Task<bool> IsHolidayBetweenTwoDates(DateTime fromDate, DateTime toDate)
        {
            bool flag = false;

            var records = _calendars.Where(x => x.EventDate.Date >= fromDate.Date && x.EventDate.Date <= toDate.Date);
            if (records.Any())
                flag = true;

            return await Task.FromResult(flag);
        }

        public async Task<List<Calendar>> GetHolidayBetweenTwoDates(DateTime fromDate, DateTime toDate)
        {
            var holidays = _calendars.Where(x => x.EventDate.Date >= fromDate.Date && x.EventDate.Date <= toDate.Date).ToList<Calendar>();
            return await Task.FromResult(holidays);
        }

        public async Task<bool> IsWeekOff(DateTime date)
        {
            bool flag = false;
            if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                flag = true;

            return await Task.FromResult(flag);
        }

        public async Task<bool> IsWeekOffBetweenTwoDates(DateTime fromDate, DateTime toDate)
        {
            bool flag = false;
            while (fromDate.Date <= toDate.Date)
            {
                if (fromDate.DayOfWeek == DayOfWeek.Saturday || fromDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    flag = true;
                    break;
                }
                fromDate.AddDays(1);
            }
            return await Task.FromResult(flag);
        }

        public async Task<List<DateTime>> GetWeekOffBetweenTwoDates(DateTime fromDate, DateTime toDate)
        {
            List<DateTime> holidays = new List<DateTime>();
            while (fromDate.Date <= toDate.Date)
            {
                if (fromDate.DayOfWeek == DayOfWeek.Saturday || fromDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    holidays.Add(fromDate);
                }
                fromDate.AddDays(1);
            }
            return await Task.FromResult(holidays);
        }
    }
}
