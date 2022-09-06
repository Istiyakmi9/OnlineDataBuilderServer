using BottomhalfCore.DatabaseLayer.Common.Code;
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
    public class CompanyService : ICompanyService
    {
        private readonly IDb _db;
        private readonly FileLocationDetail _fileLocationDetail;
        private readonly IFileService _fileService;
        private readonly CurrentSession _currentSession;

        public CompanyService(IDb db, FileLocationDetail fileLocationDetail, IFileService fileService, CurrentSession currentSession)
        {
            _db = db;
            _fileLocationDetail = fileLocationDetail;
            _fileService = fileService;
            _currentSession = currentSession;
        }
        public List<OrganizationDetail> GetAllCompany()
        {
            var result = _db.GetList<OrganizationDetail>("sp_company_get", false);
            return result;
        }

        public List<OrganizationDetail> UpdateCompanyGroup(OrganizationDetail companyGroup, int companyId)
        {
            if (companyId <= 0)
            {
                throw new HiringBellException("Invalid compnay id. Unable to update detail.");
            }

            OrganizationDetail companyGrp = _db.Get<OrganizationDetail>("sp_company_getById", new { CompanyId = companyId });
            if (companyGrp == null)
                throw new HiringBellException("Compnay detail not found");

            companyGrp.Email = companyGroup.Email;
            companyGrp.InCorporationDate = companyGroup.InCorporationDate;
            companyGrp.CompanyDetail = companyGroup.CompanyDetail;


            var value = _db.Execute<OrganizationDetail>("sp_company_intupd", companyGrp, true);
            if (string.IsNullOrEmpty(value))
                throw new HiringBellException("Fail to insert company group.");

            return this.GetAllCompany();
        }

        public List<OrganizationDetail> AddCompanyGroup(OrganizationDetail companyGroup)
        {
            List<OrganizationDetail> companyGrp = null;
            companyGrp = _db.GetList<OrganizationDetail>("sp_company_get", false);
            OrganizationDetail result = companyGrp.Find(x => x.CompanyName == companyGroup.CompanyName);
            if (result != null)
            {
                throw new HiringBellException("Company Already exist.");
            }
            else
            {
                result = companyGroup;
            }

            var value = _db.Execute<OrganizationDetail>("sp_company_intupd", result, true);
            if (string.IsNullOrEmpty(value))
                throw new HiringBellException("Fail to insert company group.");

            companyGrp = this.GetAllCompany();
            return companyGrp;
        }

        public dynamic GetCompanyById(int CompanyId)
        {
            OrganizationDetail result = _db.Get<OrganizationDetail>("sp_company_getById", new { CompanyId });
            List<Files> files = _db.GetList<Files>("sp_Files_GetBy_OwnerId", new { FileOwnerId = CompanyId, UserTypeId = (int)UserType.Compnay });
            return new { OrganizationDetail = result, Files = files };
        }

        public dynamic GetOrganizationDetailService()
        {
            var ResultSet = _db.GetDataset("sp_organization_detail_get");
            if (ResultSet.Tables.Count != 2)
                throw new HiringBellException("Unable to get organization detail.");

            OrganizationDetail organizationDetail = Converter.ToType<OrganizationDetail>(ResultSet.Tables[0]);
            FileDetail fileDetail = Converter.ToType<FileDetail>(ResultSet.Tables[1]);

            return new { OrganizationDetail = organizationDetail, Files = fileDetail };
        }

        public OrganizationDetail InsertUpdateOrganizationDetailService(OrganizationDetail companyInfo, IFormFileCollection fileCollection)
        {
            OrganizationDetail company = new OrganizationDetail();
            if (string.IsNullOrEmpty(companyInfo.Email))
                throw new HiringBellException("Invalid organization email.");

            if (string.IsNullOrEmpty(companyInfo.CompanyName))
                throw new HiringBellException("Invalid company name.");

            if (companyInfo.OrganizationName == null)
                throw new HiringBellException("Invalid Orgznization Name");
            if (companyInfo.InCorporationDate != null)
            {
                var date = companyInfo.InCorporationDate?.ToString("yyyy/MM/dd");
                companyInfo.InCorporationDate = Convert.ToDateTime(date);

            }

            var ResultSet = _db.GetDataset("sp_organization_detail_get");
            if (ResultSet.Tables.Count != 2)
                throw new HiringBellException("Unable to get organization detail.");
            
            company = Converter.ToType<OrganizationDetail>(ResultSet.Tables[0]);
            if (company != null)
            {
                company.OrganizationName = companyInfo.OrganizationName;
                company.OrgEmail = companyInfo.OrgEmail;
                company.OrgFax = companyInfo.OrgFax;
                company.OrgMobileNo = companyInfo.OrgMobileNo;
                company.OrgPrimaryPhoneNo = companyInfo.OrgPrimaryPhoneNo;
                company.OrgSecondaryPhoneNo = companyInfo.OrgSecondaryPhoneNo;
                company.CompanyName = companyInfo.CompanyName;
                company.CompanyDetail = companyInfo.CompanyDetail;
                company.FirstAddress = companyInfo.FirstAddress;
                company.SecondAddress = companyInfo.SecondAddress;
                company.ThirdAddress = companyInfo.ThirdAddress;
                company.ForthAddress = companyInfo.ForthAddress;
                company.Email = companyInfo.Email;
                company.PrimaryPhoneNo = companyInfo.PrimaryPhoneNo;
                company.SecondaryPhoneNo = companyInfo.SecondaryPhoneNo;
                company.Fax = companyInfo.Fax;
                company.FirstEmail = companyInfo.FirstEmail;
                company.SecondEmail = companyInfo.SecondEmail;
                company.ThirdEmail = companyInfo.ThirdEmail;
                company.ForthEmail = companyInfo.ForthEmail;
                company.Pincode = companyInfo.Pincode;
                company.FileId = companyInfo.FileId;
                company.MobileNo = companyInfo.MobileNo;
                company.City = companyInfo.City;
                company.Country = companyInfo.Country;
                company.FullAddress = companyInfo.FullAddress;
                company.GSTNo = companyInfo.GSTNo;
                company.InCorporationDate = companyInfo.InCorporationDate;
                company.LegalDocumentPath = companyInfo.LegalDocumentPath;
                company.LegalEntity = companyInfo.LegalEntity;
                company.LegalNameOfCompany = companyInfo.LegalNameOfCompany;
                company.PANNo = companyInfo.PANNo;
                company.SectorType = companyInfo.SectorType;
                company.State = companyInfo.State;
                company.TradeLicenseNo = companyInfo.TradeLicenseNo;
                company.TypeOfBusiness = companyInfo.TypeOfBusiness;
                company.AccountNo = companyInfo.AccountNo;
                company.BankName = companyInfo.BankName;
                company.Branch = companyInfo.Branch;
                company.IFSC = companyInfo.IFSC;
                company.IsPrimaryCompany = companyInfo.IsPrimaryCompany;
                company.FixedComponentsId = companyInfo.FixedComponentsId;
                company.BranchCode=companyInfo.BranchCode;
                company.OpeningDate= companyInfo.OpeningDate;
                company.ClosingDate= companyInfo.ClosingDate;
                company.AdminId = _currentSession.CurrentUserDetail.UserId;
            }
            else
                company = companyInfo;


            company.IsPrimaryCompany = true;
            company.FixedComponentsId = "[]";

            var status = _db.Execute<OrganizationDetail>("sp_organization_intupd", company, true);

            if (string.IsNullOrEmpty(status))
                throw new HiringBellException("Fail to insert or update.");

            if (fileCollection.Count == 1)
            {
                UpdateOrganizationLogo(companyInfo, fileCollection);
            }

            return company;
        }

        private void UpdateOrganizationLogo(OrganizationDetail companyInfo, IFormFileCollection fileCollection)
        {
            string companyLogo = String.Empty;
            try
            {
                if (fileCollection.Count == 1)
                    companyLogo = Path.Combine(_fileLocationDetail.RootPath, _fileLocationDetail.LogoPath, fileCollection[0].Name);

                if (File.Exists(companyLogo))
                    File.Delete(companyLogo);
                else
                {
                    FileDetail fileDetailWSig = new FileDetail();
                    fileDetailWSig.DiskFilePath = Path.Combine(_fileLocationDetail.RootPath, companyLogo);
                }

                var files = fileCollection.Select(x => new Files
                {
                    FileUid = companyInfo.FileId,
                    FileName = x.Name,
                    Email = companyInfo.Email,
                    FileExtension = string.Empty
                }).ToList<Files>();
                _fileService.SaveFile(_fileLocationDetail.LogoPath, files, fileCollection, (companyInfo.OrganizationId).ToString());

                var fileInfo = (from n in files
                                select new
                                {
                                    FileId = n.FileUid,
                                    FileOwnerId = companyInfo.CompanyId,
                                    FilePath = n.FilePath,
                                    FileName = n.FileName,
                                    FileExtension = n.FileExtension,
                                    ItemStatusId = 0,
                                    PaidOn = DateTime.Now,
                                    UserTypeId = (int)UserType.Compnay,
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
            catch
            {
                _db.RollBack();
                if (File.Exists(companyLogo))
                    File.Delete(companyLogo);

                throw;
            }
        }

        public OrganizationDetail InsertUpdateCompanyDetails(OrganizationDetail companyInfo, IFormFileCollection fileCollection)
        {
            OrganizationDetail company = new OrganizationDetail();
            if (string.IsNullOrEmpty(companyInfo.Email))
                throw new HiringBellException("Invalid organization email.");

            if (string.IsNullOrEmpty(companyInfo.CompanyName))
                throw new HiringBellException("Invalid company name.");

            if (companyInfo.OrganizationName == null)
                throw new HiringBellException("Invalid Orgznization Name");

            company = _db.Get<OrganizationDetail>("sp_company_getById", new { companyInfo.CompanyId });

            if (company != null)
            {
                company.OrganizationName = companyInfo.OrganizationName;
                company.CompanyName = companyInfo.CompanyName;
                company.CompanyDetail = companyInfo.CompanyDetail;
                company.FirstAddress = companyInfo.FirstAddress;
                company.SecondAddress = companyInfo.SecondAddress;
                company.ThirdAddress = companyInfo.ThirdAddress;
                company.ForthAddress = companyInfo.ForthAddress;
                company.Email = companyInfo.Email;
                company.PrimaryPhoneNo = companyInfo.PrimaryPhoneNo;
                company.SecondaryPhoneNo = companyInfo.SecondaryPhoneNo;
                company.Fax = companyInfo.Fax;
                company.FirstEmail = companyInfo.FirstEmail;
                company.SecondEmail = companyInfo.SecondEmail;
                company.ThirdEmail = companyInfo.ThirdEmail;
                company.ForthEmail = companyInfo.ForthEmail;
                company.Pincode = companyInfo.Pincode;
                company.FileId = companyInfo.FileId;
                company.MobileNo = companyInfo.MobileNo;
                company.City = companyInfo.City;
                company.Country = companyInfo.Country;
                company.FullAddress = companyInfo.FullAddress;
                company.GSTNo = companyInfo.GSTNo;
                company.InCorporationDate = companyInfo.InCorporationDate;
                company.LegalDocumentPath = companyInfo.LegalDocumentPath;
                company.LegalEntity = companyInfo.LegalEntity;
                company.LegalNameOfCompany = companyInfo.LegalNameOfCompany;
                company.PANNo = companyInfo.PANNo;
                company.SectorType = companyInfo.SectorType;
                company.State = companyInfo.State;
                company.TradeLicenseNo = companyInfo.TradeLicenseNo;
                company.TypeOfBusiness = companyInfo.TypeOfBusiness;
                company.AccountNo = companyInfo.AccountNo;
                company.BankName = companyInfo.BankName;
                company.Branch = companyInfo.Branch;
                company.IFSC = companyInfo.IFSC;
            }
            else
                company = companyInfo;

            int i = 0;
            string signatureWithStamp = String.Empty;
            string signatureWithoutStamp = String.Empty;
            string companyLogo = String.Empty;

            while (i < fileCollection.Count)
            {
                if (fileCollection[i].Name == "signwithStamp")
                {
                    signatureWithStamp = Path.Combine(_fileLocationDetail.RootPath, _fileLocationDetail.LogoPath, fileCollection[i].Name);
                }
                else if (fileCollection[i].Name == "signwithoutStamp")
                {
                    signatureWithoutStamp = Path.Combine(_fileLocationDetail.RootPath, _fileLocationDetail.LogoPath, fileCollection[i].Name);
                }
                else
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

            company.IsPrimaryCompany = true;
            company.FixedComponentsId = "[]";
            var status = _db.Execute<OrganizationDetail>("sp_organization_intupd", company, true);

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
                        FileUid = companyInfo.FileId,
                        FileName = x.Name,
                        Email = companyInfo.Email,
                        FileExtension = string.Empty
                    }).ToList<Files>();
                    _fileService.SaveFile(_fileLocationDetail.LogoPath, files, fileCollection, (companyInfo.OrganizationId).ToString());

                    var fileInfo = (from n in files
                                    select new
                                    {
                                        FileId = n.FileUid,
                                        FileOwnerId = companyInfo.CompanyId,
                                        FilePath = n.FilePath,
                                        FileName = n.FileName,
                                        FileExtension = n.FileExtension,
                                        ItemStatusId = 0,
                                        PaidOn = DateTime.Now,
                                        UserTypeId = (int)UserType.Compnay,
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

            return company;
        }

        public BankDetail InsertUpdateCompanyAccounts(BankDetail bankDetail)
        {
            BankDetail bank = null;

            if (bankDetail.CompanyId <= 0)
                throw new HiringBellException("Invalid organization detail submitted. Please login again.");
            if (bankDetail.CompanyId <= 0)
                throw new HiringBellException("Invalid company detail submitted. Please login again.");
            if (string.IsNullOrEmpty(bankDetail.AccountNo))
                throw new HiringBellException("Invalid account number submitted.");

            bank = _db.Get<BankDetail>("sp_bank_accounts_getby_cmpId", new { CompanyId = bankDetail.CompanyId });

            if (bank == null)
                bank = bankDetail;
            else
            {
                bank.BankAccountId = bankDetail.BankAccountId;
                bank.CompanyId = bankDetail.CompanyId;
                bank.AccountNo = bankDetail.AccountNo;
                bank.BankName = bankDetail.BankName;
                bank.Branch = bankDetail.Branch;
                bank.IFSC = bankDetail.IFSC;
                bank.OpeningDate = bankDetail.OpeningDate;
                bank.BranchCode = bankDetail.BranchCode;
                bank.PANNo = bankDetail.PANNo;
                bank.GSTNo = bankDetail.GSTNo;
                bank.TradeLicenseNo = bankDetail.TradeLicenseNo;
            }

            var status = _db.Execute<BankDetail>("sp_bank_accounts_intupd", bank, true);

            if (string.IsNullOrEmpty(status))
            {
                throw new HiringBellException("Fail to insert or update.");
            }

            return bank;
        }

        public BankDetail GetCompanyBankDetail(int OrganizationId, int CompanyId)
        {
            BankDetail result = _db.Get<BankDetail>("sp_bank_accounts_getby_cmpId", new { OrganizationId, CompanyId });
            return result;
        }
    }
}
