using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Accounts
{
    public class Payroll: CreationInfo
    {
        public string PayFrequency { get; set; }
        public string PayCycleStart { get; set; }
        public string PayPeriodEnd { get; set; }
        public string PayDayinMonth { get; set; }
        public string PayDayPeriod { get; set; }
        public bool IsExcludeWeekly { get; set; }
        public bool IsExcludeHoliday { get; set; }
    }
}
