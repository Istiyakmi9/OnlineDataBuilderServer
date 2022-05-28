using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Accounts
{
    public class PfEsiSetting: CreationInfo
	{
		public int PfEsi_setting_Id { set; get; }
		public bool PF_Limit_Amount_Statutory {get; set;}
		public bool PF_Allow_overriding {get; set;}
		public bool PF_EmployerContribution_Outside_GS {get; set;}
		public bool PF_OtherChgarges_Outside_GS {get; set;}
		public bool PF_Employess_Contribute_VPF {get; set;}
		public bool ESI_Allow_overriding {get; set;}
		public bool ESI_EmployerContribution_Outside_GS {get; set;}
		public bool ESI_Exclude_EmployerShare_fromGross { get; set;}
		public bool ESI_Exclude_EmpGratuity_fromGross {get; set;}
		public bool ESI_Restrict_Statutory  {get; set;}
		public bool ESI_IncludeBonuses_OTP_inGross_Eligibility { get; set; }
		public bool ESI_IncludeBonuses_OTP_inGross_Calculation { get; set; }
		public bool PF_IsEmployerPFLimit { get; set; }

	}
}
