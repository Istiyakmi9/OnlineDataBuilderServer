using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public class CompanySetting
    {
        public int SettingId {set; get;}
        public int CompanyId {set; get;}
        public int ProbationPeriodInDays {set; get;}
        public int NoticePeriodInDays { set; get; }
    }
}
