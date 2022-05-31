using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ServiceLayer.Code
{
    public class SettingService : ISettingService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        public SettingService(IDb db, CurrentSession currentSession)
        {
            _db = db;
            _currentSession = currentSession;
        }

        public string AddUpdateComponentService(SalaryComponents salaryComponents)
        {
            salaryComponents = _db.Get<SalaryComponents>("");
            return null;
        }

        public dynamic GetSalaryComponentService()
        {
            List<SalaryComponents> salaryComponents = _db.GetList<SalaryComponents>("sp_fixed_salary_component_percent_get");
            PfEsiSetting pfEsiSettings = _db.Get<PfEsiSetting>("sp_pf_esi_setting_get");
            var value = new { SalaryComponent = salaryComponents, PfEsiSettings = pfEsiSettings };
            return value;
        }

        public OrganizationSettings InsertUpdateCompanyDetailService(OrganizationSettings organizationSettings)
        {
            OrganizationSettings org = null;
            if (organizationSettings.OrganizationId <= 0)
                throw new HiringBellException("Invalid organization detail submitted. Please login again.");

            if (organizationSettings.OrganizationName == null)
                throw new HiringBellException("Invalid Orgznization Name");

            org = _db.Get<OrganizationSettings>("sp_organization_setting_getById", new { organizationSettings.OrganizationId, organizationSettings.OrganizationName });

            if (org == null)
                org = organizationSettings;
            else
            {
                org.OrganizationName = organizationSettings.OrganizationName;
                org.FirstAddress = organizationSettings.FirstAddress;
                org.SecondAddress = organizationSettings.SecondAddress;
                org.ThirdAddress = organizationSettings.ThirdAddress;
                org.FourthAddress = organizationSettings.FourthAddress;
                org.Email = organizationSettings.Email;
                org.PrimaryPhoneNo = organizationSettings.PrimaryPhoneNo;
                org.SecondaryPhoneNo = organizationSettings.SecondaryPhoneNo;
                org.Fax = organizationSettings.Fax;
                org.Pincode = organizationSettings.Pincode;
                org.FileId = organizationSettings.FileId;
                org.MobileNo = organizationSettings.MobileNo;
                org.City = organizationSettings.City;
                org.Country = organizationSettings.Country;
                org.FullAddress = organizationSettings.FullAddress;
                org.GSTINNumber = organizationSettings.GSTINNumber;
                org.InCorporationDate = organizationSettings.InCorporationDate;
                org.LegalDocumentPath = organizationSettings.LegalDocumentPath;
                org.LegalEntity = organizationSettings.LegalEntity;
                org.LegalNameOfCompany = organizationSettings.LegalNameOfCompany;
                org.PANNumber = organizationSettings.PANNumber;
                org.SectorType = organizationSettings.SectorType;
                org.State = organizationSettings.State;
                org.TradeLicenseNumber = organizationSettings.TradeLicenseNumber;
                org.TypeOfBusiness = organizationSettings.TypeOfBusiness;
            }


            var status = _db.Execute<OrganizationSettings>("sp_organization_detail_intupd",
                new
                {
                    org.OrganizationName,
                    org.FirstAddress,
                    org.SecondAddress,
                    org.ThirdAddress,
                    org.FourthAddress,
                    org.Email,
                    org.PrimaryPhoneNo,
                    org.SecondaryPhoneNo,
                    org.Fax,
                    org.Pincode,
                    org.FileId,
                    org.MobileNo,
                    org.City,
                    org.Country,
                    org.FullAddress,
                    org.GSTINNumber,
                    org.InCorporationDate,
                    org.LegalDocumentPath,
                    org.LegalEntity,
                    org.LegalNameOfCompany,
                    org.OrganizationId,
                    org.PANNumber,
                    org.SectorType,
                    org.State,
                    org.TradeLicenseNumber,
                    org.TypeOfBusiness
                },
                true
            );

            if (string.IsNullOrEmpty(status))
            {
                throw new HiringBellException("Fail to insert or update.");
            }

            return org;
        }

        public BankDetail UpdateCompanyAccountsService(BankDetail bankDetail)
        {
            BankDetail bank = null;

            if (bankDetail.OrganizationId <= 0)
                throw new HiringBellException("Invalid organization detail submitted. Please login again.");


            bank = _db.Get<BankDetail>("sp_bank_accounts_get_by_orgId", new { bankDetail.OrganizationId });

            if (bank == null)
                bank = bankDetail;
            else
            {
                bank.AccountNumber = bankDetail.AccountNumber;
                bank.BankName = bankDetail.BankName;
                bank.Branch = bankDetail.Branch;
                bank.IFSCCode = bankDetail.IFSCCode;
                bank.IsUser = bankDetail.IsUser;
                bank.OpeningDate = bankDetail.OpeningDate;
                bank.BranchCode = bankDetail.BranchCode;
                bank.UserId = bankDetail.UserId;
                bank.OrganizationId = bankDetail.OrganizationId;
                bank.PANNumber = bankDetail.PANNumber;
                bank.GSTINNumber= bankDetail.GSTINNumber;
                bank.TradeLiecenceNumber = bankDetail.TradeLiecenceNumber;
            }

            var status = _db.Execute<BankDetail>("sp_bank_accounts_intupd",
                bank,
                true
            );

            if (string.IsNullOrEmpty(status))
            {
                throw new HiringBellException("Fail to insert or update.");
            }

            return bank;
        }

        public string PfEsiSetting(SalaryComponents PfSetting, SalaryComponents EsiSetting, PfEsiSetting PfesiSetting)
        {
            string value = string.Empty;
            List<SalaryComponents> salaryComponents = _db.GetList<SalaryComponents>("sp_fixed_salary_component_percent_get");
            var pfsetting = salaryComponents.Find(x => x.ComponentId == PfSetting.ComponentId);
            var esisetting = salaryComponents.Find(x => x.ComponentId == EsiSetting.ComponentId);
            DbParam[] param = new DbParam[]
            {
                new DbParam (PfSetting.ComponentId, typeof(string), "_ComponentId"),
                new DbParam (PfSetting.CalculateInPercentage, typeof(bool), "_CalculateInPercentage"),
                new DbParam (pfsetting.EmployeeContribution, typeof(decimal), "_EmployeeContribution"),
                new DbParam (PfSetting.IsDeductions, typeof(bool), "_IsDeductions"),
                new DbParam (PfSetting.IsActive, typeof(bool), "_IsActive"),
                new DbParam (PfSetting.IncludeInPayslip, typeof(bool), "_IncludeInPayslip"),
                new DbParam (pfsetting.ComponentDescription, typeof(string), "_ComponentDescription"),
                new DbParam (pfsetting.Amount, typeof(decimal), "_Amount"),
                new DbParam (PfSetting.EmployerContribution, typeof(decimal), "_EmployerContribution"),
                new DbParam (pfsetting.IsOpted, typeof(bool), "_IsOpted"),
                new DbParam (pfsetting.PercentageValue, typeof(decimal), "_PercentageValue"),
                new DbParam (_currentSession.CurrentUserDetail.UserId, typeof(long), "_Admin")
            };
            value = _db.ExecuteNonQuery("sp_fixed_salary_component_percent_insupd", param, true);
            if (string.IsNullOrEmpty(value))
                throw new HiringBellException("Unable to update PF Setting.");

            param = new DbParam[]
            {
                new DbParam (EsiSetting.ComponentId, typeof(string), "_ComponentId"),
                new DbParam (esisetting.CalculateInPercentage, typeof(bool), "_CalculateInPercentage"),
                new DbParam (EsiSetting.EmployeeContribution, typeof(decimal), "_EmployeeContribution"),
                new DbParam (EsiSetting.IsDeductions, typeof(bool), "_IsDeductions"),
                new DbParam (EsiSetting.IsActive, typeof(bool), "_IsActive"),
                new DbParam (EsiSetting.IncludeInPayslip, typeof(bool), "_IncludeInPayslip"),
                new DbParam (esisetting.ComponentDescription, typeof(string), "_ComponentDescription"),
                new DbParam (EsiSetting.Amount, typeof(decimal), "_Amount"),
                new DbParam (EsiSetting.EmployerContribution, typeof(decimal), "_EmployerContribution"),
                new DbParam (esisetting.IsOpted, typeof(bool), "_IsOpted"),
                new DbParam (esisetting.PercentageValue, typeof(decimal), "_PercentageValue"),
                new DbParam (_currentSession.CurrentUserDetail.UserId, typeof(long), "_Admin")
            };
            value = _db.ExecuteNonQuery("sp_fixed_salary_component_percent_insupd", param, true);
            if (string.IsNullOrEmpty(value))
                throw new HiringBellException("Unable to update PF Setting.");

            param = new DbParam[]
            {
                new DbParam (PfesiSetting.PfEsi_setting_Id, typeof(int), "_PfEsi_setting_Id"),
                new DbParam (PfesiSetting.IsPF_Limit_Amount_Statutory, typeof(bool), "_IsPF_Limit_Amount_Statutory"),
                new DbParam (PfesiSetting.IsPF_Allow_overriding, typeof(bool), "_IsPF_Allow_overriding"),
                new DbParam (PfesiSetting.IsPF_EmployerContribution_Outside_GS, typeof(bool), "_IsPF_EmployerContribution_Outside_GS"),
                new DbParam (PfesiSetting.IsPF_OtherChgarges, typeof(bool), "_IsPF_OtherChgarges"),
                new DbParam (PfesiSetting.IsPFAllowVPF, typeof(bool), "_IsPFAllowVPF"),
                new DbParam (PfesiSetting.IsESI_Allow_overriding, typeof(bool), "_IsESI_Allow_overriding"),
                new DbParam (PfesiSetting.IsESI_EmployerContribution_Outside_GS, typeof(bool), "_IsESI_EmployerContribution_Outside_GS"),
                new DbParam (PfesiSetting.IsESI_Exclude_EmployerShare, typeof(bool), "_IsESI_Exclude_EmployerShare"),
                new DbParam (PfesiSetting.IsESI_Exclude_EmpGratuity, typeof(bool), "_IsESI_Exclude_EmpGratuity"),
                new DbParam (PfesiSetting.IsESI_Restrict_Statutory, typeof(bool), "_IsESI_Restrict_Statutory"),
                new DbParam (PfesiSetting.IsESI_IncludeBonuses_Eligibility, typeof(bool), "_IsESI_IncludeBonuses_Eligibility"),
                new DbParam (PfesiSetting.IsESI_IncludeBonuses_Calculation, typeof(bool), "_IsESI_IncludeBonuses_Calculation"),
                new DbParam (PfesiSetting.IsPF_Employer_LimitContribution, typeof(bool), "_IsPF_Employer_LimitContribution"),
                new DbParam (_currentSession.CurrentUserDetail.UserId, typeof(long), "_Admin")
            };
            value = _db.ExecuteNonQuery("sp_pf_esi_setting_insupd", param, true);
            if (string.IsNullOrEmpty(value))
                throw new HiringBellException("Unable to update PF Setting.");
            return value;
        }

        public List<OrganizationSettings> GetOrganizationInfo()
        {
            List<OrganizationSettings> organizations = _db.GetList<OrganizationSettings>("sp_organization_setting_get");
            return organizations;
        }

        public BankDetail GetOrganizationBankDetailInfoService(int OrganizationId)
        {
            BankDetail result = _db.Get<BankDetail>("sp_bank_accounts_get_by_orgId", new { OrganizationId});
            return result;
        }
    }
}
