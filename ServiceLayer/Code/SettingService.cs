﻿using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ServiceLayer.Code
{
    public class SettingService : ISettingService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly FileLocationDetail _fileLocationDetail;
        private readonly IFileService _fileService;
        public SettingService(IDb db, CurrentSession currentSession, FileLocationDetail fileLocationDetail, IFileService fileService)
        {
            _db = db;
            _currentSession = currentSession;
            _fileLocationDetail = fileLocationDetail;
            _fileService = fileService;
        }

        public string AddUpdateComponentService(SalaryComponents salaryComponents)
        {
            salaryComponents = _db.Get<SalaryComponents>("");
            return null;
        }

        public dynamic GetSalaryComponentService()
        {
            List<SalaryComponents> salaryComponents = _db.GetList<SalaryComponents>("sp_salary_components_get");
            PfEsiSetting pfEsiSettings = _db.Get<PfEsiSetting>("sp_pf_esi_setting_get");
            var value = new { SalaryComponent = salaryComponents, PfEsiSettings = pfEsiSettings };
            return value;
        }

        public OrganizationSettings InsertUpdateCompanyDetailService(OrganizationSettings organizationSettings, IFormFileCollection fileCollection)
        {
            OrganizationSettings org = null;
            if (organizationSettings.OrganizationId <= 0)
                throw new HiringBellException("Invalid organization detail submitted. Please login again.");

            if (string.IsNullOrEmpty(organizationSettings.Email))
                throw new HiringBellException("Invalid organization email.");

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
            int i = 0;
            string signatureWithStamp = String.Empty;
            string signatureWithoutStamp = String.Empty;
            string companyLogo = String.Empty;

            while (i < fileCollection.Count)
            {
                if (fileCollection[i].Name == "signwithStamp")
                {
                    signatureWithStamp = Path.Combine(_fileLocationDetail.RootPath, _fileLocationDetail.LogoPath, fileCollection[i].Name);
                } else if (fileCollection[i].Name == "signwithoutStamp")
                {
                    signatureWithoutStamp = Path.Combine(_fileLocationDetail.RootPath, _fileLocationDetail.LogoPath, fileCollection[i].Name);
                } else
                {
                    companyLogo = Path.Combine(_fileLocationDetail.RootPath, _fileLocationDetail.LogoPath, fileCollection[i].Name);
                }

                i++;
            }
            
            if (File.Exists(signatureWithStamp))
            {
                File.Delete(signatureWithoutStamp);
                File.Delete(signatureWithStamp);
                File.Delete(companyLogo);

            }
            else
            {
                FileDetail fileDetailWSig = new FileDetail();
                fileDetailWSig.DiskFilePath = Path.Combine(_fileLocationDetail.RootPath, signatureWithoutStamp);

                FileDetail fileDetailWOSig = new FileDetail();
                fileDetailWOSig.DiskFilePath = Path.Combine(_fileLocationDetail.RootPath, signatureWithoutStamp);
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
                File.Delete(signatureWithoutStamp);
                File.Delete(signatureWithStamp);
                File.Delete(companyLogo);
                throw new HiringBellException("Fail to insert or update.");
            }
            else
            {
                if (fileCollection.Count > 0)
                {
                    var files = fileCollection.Select(x => new Files
                    {
                        FileUid = organizationSettings.FileId,
                        FileName = x.Name,
                        Email = organizationSettings.Email,
                        FileExtension = string.Empty
                    }).ToList<Files>();
                    _fileService.SaveFile(_fileLocationDetail.LogoPath, files, fileCollection, (organizationSettings.OrganizationId).ToString());

                    var fileInfo = (from n in files
                                    select new
                                    {
                                        FileId = n.FileUid,
                                        FileOwnerId = (organizationSettings.OrganizationId),
                                        FilePath = n.FilePath,
                                        FileName = n.FileName,
                                        FileExtension = n.FileExtension,
                                        ItemStatusId = 0,
                                        PaidOn = DateTime.Now,
                                        UserTypeId = (int)UserType.Admin,
                                        CreatedBy = _currentSession.CurrentUserDetail.UserId,
                                        UpdatedBy = _currentSession.CurrentUserDetail.UserId,
                                        CreatedOn = DateTime.Now,
                                        UpdatedOn = DateTime.Now
                                    }); ;

                    DataTable table = Converter.ToDataTable(fileInfo);
                    _db.StartTransaction(IsolationLevel.ReadUncommitted);
                    int insertedCount = _db.BatchInsert("sp_Files_InsUpd", table, false);
                    _db.Commit();
                }
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
                bank.GSTINNumber = bankDetail.GSTINNumber;
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
            List<SalaryComponents> salaryComponents = _db.GetList<SalaryComponents>("sp_salary_components_get");
            var pfsetting = salaryComponents.Find(x => x.ComponentId == PfSetting.ComponentId);
            var esisetting = salaryComponents.Find(x => x.ComponentId == EsiSetting.ComponentId);

            if (pfsetting == null)
                pfsetting = PfSetting;
            if (esisetting == null)
                esisetting = EsiSetting;
            DbParam[] param = new DbParam[]
            {
                new DbParam (PfSetting.ComponentId, typeof(string), "_ComponentId"),
                new DbParam (PfSetting.CalculateInPercentage, typeof(bool), "_CalculateInPercentage"),
                new DbParam (pfsetting.EmployeeContribution, typeof(decimal), "_EmployeeContribution"),
                new DbParam (PfSetting.IsActive, typeof(bool), "_IsActive"),
                new DbParam (PfSetting.TaxExempt, typeof(string), "_TaxExempt"),
                new DbParam (PfSetting.ComponentTypeId, typeof(int), "_ComponentTypeId"),
                new DbParam (PfSetting.IncludeInPayslip, typeof(bool), "_IncludeInPayslip"),
                new DbParam (pfsetting.ComponentDescription, typeof(string), "_ComponentDescription"),
                new DbParam (pfsetting.MaxLimit, typeof(decimal), "_MaxLimit"),
                new DbParam (PfSetting.EmployerContribution, typeof(decimal), "_EmployerContribution"),
                new DbParam (pfsetting.IsOpted, typeof(bool), "_IsOpted"),
                new DbParam (pfsetting.PercentageValue, typeof(decimal), "_PercentageValue"),
                new DbParam (pfsetting.Formula, typeof(string), "_Formula"),
                new DbParam (pfsetting.IsAdHoc, typeof(bool), "_IsAdHoc"),
                new DbParam (pfsetting.AdHocId, typeof(int), "_AdHocId"),
                new DbParam (pfsetting.SectionMaxLimit, typeof(decimal), "_SectionMaxLimit"),
                new DbParam (pfsetting.IsAffectInGross, typeof(bool), "_IsAffectInGross"),
                new DbParam (pfsetting.RequireDocs, typeof(bool), "_RequireDocs"),
                new DbParam (_currentSession.CurrentUserDetail.UserId, typeof(long), "_Admin")
            };
            value = _db.ExecuteNonQuery("sp_salary_components_insupd", param, true);
            if (string.IsNullOrEmpty(value))
                throw new HiringBellException("Unable to update PF Setting.");

            param = new DbParam[]
            {
                new DbParam (EsiSetting.ComponentId, typeof(string), "_ComponentId"),
                new DbParam (esisetting.CalculateInPercentage, typeof(bool), "_CalculateInPercentage"),
                new DbParam (EsiSetting.EmployeeContribution, typeof(decimal), "_EmployeeContribution"),
                new DbParam (EsiSetting.IsActive, typeof(bool), "_IsActive"),
                new DbParam (EsiSetting.TaxExempt, typeof(string), "_TaxExempt"),
                new DbParam (EsiSetting.ComponentTypeId, typeof(int), "_ComponentTypeId"),
                new DbParam (EsiSetting.IncludeInPayslip, typeof(bool), "_IncludeInPayslip"),
                new DbParam (esisetting.ComponentDescription, typeof(string), "_ComponentDescription"),
                new DbParam (EsiSetting.MaxLimit, typeof(decimal), "_MaxLimit"),
                new DbParam (EsiSetting.EmployerContribution, typeof(decimal), "_EmployerContribution"),
                new DbParam (esisetting.IsOpted, typeof(bool), "_IsOpted"),
                new DbParam (esisetting.PercentageValue, typeof(decimal), "_PercentageValue"),
                new DbParam (esisetting.Formula, typeof(string), "_Formula"),
                new DbParam (esisetting.IsAdHoc, typeof(bool), "_IsAdHoc"),
                new DbParam (esisetting.AdHocId, typeof(int), "_AdHocId"),
                new DbParam (esisetting.SectionMaxLimit, typeof(decimal), "_SectionMaxLimit"),
                new DbParam (esisetting.IsAffectInGross, typeof(bool), "_IsAffectInGross"),
                new DbParam (esisetting.RequireDocs, typeof(bool), "_RequireDocs"),
                new DbParam (_currentSession.CurrentUserDetail.UserId, typeof(long), "_Admin")
            };
            value = _db.ExecuteNonQuery("sp_salary_components_insupd", param, true);
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
            BankDetail result = _db.Get<BankDetail>("sp_bank_accounts_get_by_orgId", new { OrganizationId });
            return result;
        }

        public Payroll InsertUpdatePayrollSetting(Payroll payroll)
        {
            Payroll result = null;
            result = _db.Get<Payroll>("sp_bank_accounts_get_by_orgId", null);

            if (result == null)
                result = payroll;
            else
            {
                payroll.PayDayPeriod = result.PayDayPeriod;
                payroll.PayPeriodEnd = result.PayPeriodEnd;
                payroll.PayDayinMonth = result.PayDayinMonth;
                payroll.PayCycleMonth = result.PayCycleMonth;
                payroll.PayFrequency = result.PayFrequency;
            }

            var status = _db.Execute<BankDetail>("sp_bank_accounts_intupd",
                result,
                true
            );

            if (string.IsNullOrEmpty(status))
            {
                throw new HiringBellException("Fail to insert or update.");
            }


            return result;
        }

        public string InsertUpdateSalaryStructure(List<SalaryStructure> salaryStructure)
        {
            var status = string.Empty;

            return status;
        }
    }
}
