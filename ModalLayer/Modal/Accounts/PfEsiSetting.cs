using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Accounts
{
    public class PfEsiSetting: CreationInfo
	{
		public int PfEsi_setting_Id { set; get; }
        public bool IsPF_Limit_Amount_Statutory { set; get;}
        public bool IsPF_Employer_LimitContribution { set; get;}
	    public bool IsPF_Allow_overriding { set; get;}
        public bool IsPF_EmployerContribution_Outside_GS { set; get;}
        public bool IsPF_OtherChgarges { set; get;}
        public bool IsPFAllowVPF { set; get;}
        public bool IsESI_Allow_overriding { set; get;}
        public bool IsESI_EmployerContribution_Outside_GS { set; get;}
        public bool IsESI_Exclude_EmployerShare { set; get;}
        public bool IsESI_Exclude_EmpGratuity { set; get;}
        public bool IsESI_Restrict_Statutory { set; get;}
        public bool IsESI_IncludeBonuses_Eligibility { set; get;}
        public bool IsESI_IncludeBonuses_Calculation { set; get; }


    }
}
