﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public class CompanySetting
    {
        public CompanySetting()
        {
            SetupRecommandedWeekEnds();
        }

        private void SetupRecommandedWeekEnds()
        {
            OfficialWeekOffDays = new List<DayOfWeek>();
            OfficialWeekOffDays.Add(DayOfWeek.Saturday);
            OfficialWeekOffDays.Add(DayOfWeek.Sunday);
        }

        public int SettingId {set; get;}
        public int CompanyId {set; get;}
        public int ProbationPeriodInDays {set; get;}
        public int NoticePeriodInDays { set; get; }
        public bool IsUseInternationalWeekDays { set; get; } = true;
        public List<DayOfWeek> OfficialWeekOffDays { set; get; }
        public bool IsAccrualLeaveForNoticePeriodOnly { set; get; } // override all rule and allow leave for 2 or 3 months (define as per rule) leaves only.
        public bool IsAccrualLeaveForProbationPeriondOnly { set; get; } // override all rule and allow leave for 2 or 3 months (define as per rule) leaves only.
    }
}