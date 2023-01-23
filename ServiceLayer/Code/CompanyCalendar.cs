using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Interface;
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
        private readonly CurrentSession _currentSession;
        private readonly IDb _db;
        private readonly CurrentSession _session;

        public CompanyCalendar(IDb db, CurrentSession session, CurrentSession currentSession)
        {
            _db = db;
            _session = session;
            _currentSession = currentSession;
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

        public List<Calendar> GetAllHolidayService(FilterModel filterModel)
        {
            var result = _db.GetList<Calendar>("SP_company_calender_getby_filter", new
            {
                filterModel.SearchString,
                filterModel.PageIndex,
                filterModel.PageSize,
                filterModel.SortBy
            });
            return result;
        }

        public List<Calendar> HolidayInsertUpdateService(Calendar calendar)
        {
            var existCalendar = new Calendar();
            ValidateCalender(calendar);
            var result = _db.GetList<Calendar>("sp_company_calendar_get_by_company", new { CompanyId = calendar.CompanyId });
            if (result.Count > 0)
            {
                existCalendar = result.Find(x => x.CompanyCalendarId == calendar.CompanyCalendarId);
                if (existCalendar != null)
                {
                    existCalendar.CompanyId = calendar.CompanyId;
                    existCalendar.StartDate = calendar.StartDate;
                    existCalendar.EndDate = calendar.EndDate;
                    existCalendar.EventName = calendar.EventName;
                    existCalendar.IsHoliday = calendar.IsHoliday;
                    existCalendar.IsHalfDay = calendar.IsHalfDay;
                    existCalendar.DescriptionNote = calendar.DescriptionNote;
                    existCalendar.ApplicableFor = calendar.ApplicableFor;
                    existCalendar.Year = calendar.Year;
                    existCalendar.IsPublicHoliday = calendar.IsPublicHoliday;
                    existCalendar.IsCompanyCustomHoliday = calendar.IsCompanyCustomHoliday;
                    existCalendar.Country = calendar.Country;
                }
            }
            existCalendar = calendar;
            existCalendar.AdminId = _currentSession.CurrentUserDetail.UserId;
            var value = _db.Execute<Calendar>("sp_company_calendar_insupd", existCalendar, true);
            if (string.IsNullOrEmpty(value))
                throw HiringBellException.ThrowBadRequest("Fail to insert/ update holiday");

            FilterModel filterModel = new FilterModel
            {
                SearchString = $"1=1 and CompanyId={calendar.CompanyId}"
            };
            return GetAllHolidayService(filterModel);
        }

        private void ValidateCalender(Calendar calendar)
        {
            if (string.IsNullOrEmpty(calendar.DescriptionNote))
                throw HiringBellException.ThrowBadRequest("Decription note is null or empty");

            if (string.IsNullOrEmpty(calendar.Country))
                throw HiringBellException.ThrowBadRequest("Country is null or empty");

            if (string.IsNullOrEmpty(calendar.EventName))
                throw HiringBellException.ThrowBadRequest("Event name is null or empty");

            if (calendar.CompanyId <= 0)
                throw HiringBellException.ThrowBadRequest("Invalid company id");

            if (calendar.StartDate == null)
                throw HiringBellException.ThrowBadRequest("Start date is null or invalid");

            if (calendar.EndDate == null)
                throw HiringBellException.ThrowBadRequest("End date is null or invalid");
        }

        public List<Calendar> DeleteHolidayService(long CompanyCalendarId)
        {
            if (CompanyCalendarId <= 0)
                throw HiringBellException.ThrowBadRequest("Invalid holiday selected. Please select a vlid holiday");

            var result = _db.Execute<Calendar>("sp_company_calender_delete_by_calenderid", new { CompanyCalendarId = CompanyCalendarId }, true);
            if (string.IsNullOrEmpty(result))
                throw HiringBellException.ThrowBadRequest("Fail to delete holiday");

            FilterModel filterModel = new FilterModel
            {
                SearchString = $"1=1 and CompanyId={_session.CurrentUserDetail.CompanyId}"
            };
            return GetAllHolidayService(filterModel);
        }
    }
}
