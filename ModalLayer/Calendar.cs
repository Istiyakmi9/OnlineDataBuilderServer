using System;

namespace ModalLayer
{
    public class Calendar : CreationInfo
    {
        public long CompanyCalendarId { set; get; }
        public int CompanyId { set; get; }
        public DateTime StartDate { set; get; }
        public DateTime EndDate { set; get; }
        public string EventName { set; get; }
        public bool IsHoliday { set; get; }
        public bool IsHalfDay { set; get; }
        public string DescriptionNote { set; get; }
        public int ApplicableFor { set; get; }
        public string Country { get; set; }
        public int Year { set; get; }
        public bool IsPublicHoliday { set; get; }
        public bool IsCompanyCustomHoliday { set; get; }
        public int Total { get; set; }
        public int Index { get; set; }
    }
}
