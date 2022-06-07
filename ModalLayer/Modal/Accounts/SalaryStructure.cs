using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Accounts
{
    public class SalaryStructure : SalaryCommon
    {
        public string ComponentName { get; set; }
        public string Type { get; set; }
        public string TaxExempt { get; set; }
        public bool RequireDocs { get; set; }
        public bool IndividualOverride { get; set; }
        public bool IsComponentEnable { get; set; }
        public bool IsAllowtoOverride { get; set; }
        public string Section { get; set; }
        public decimal SectionMaxLimit { get; set; }
        public bool IsAffectinGross { get; set; }
    }
}
