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
        public List<OrganizationSettings> GetAllCompany()
        {
            var result = _db.GetList<OrganizationSettings>("sp_company_get");
            return result;
        }

        public List<OrganizationSettings> UpdateCompanyGroup(OrganizationSettings companyGroup, int companyId)
        {
            if (companyId <= 0)
            {
                throw new HiringBellException("Invalid compnay id. Unable to update detail.");
            }

            OrganizationSettings companyGrp = _db.Get<OrganizationSettings>("sp_company_getById", new { CompanyId = companyId });
            if (companyGrp == null)
                throw new HiringBellException("Compnay detail not found");

            companyGrp.Email = companyGroup.Email;
            companyGrp.InCorporationDate = companyGroup.InCorporationDate;
            companyGrp.CompanyDetail = companyGroup.CompanyDetail;


            var value = _db.Execute<OrganizationSettings>("sp_company_intupd", companyGrp, true);
            if (string.IsNullOrEmpty(value))
                throw new HiringBellException("Fail to insert company group.");

            return this.GetAllCompany();
        }

        public List<OrganizationSettings> AddCompanyGroup(OrganizationSettings companyGroup)
        {
            List<OrganizationSettings> companyGrp = null;
            companyGrp = _db.GetList<OrganizationSettings>("sp_company_get");
            OrganizationSettings result = companyGrp.Find(x => x.CompanyName == companyGroup.CompanyName);
            if (result != null)
            {
                throw new HiringBellException("Company Already exist.");
            }
            else
            {
                result = companyGroup;
            }

            var value = _db.Execute<OrganizationSettings>("sp_company_intupd", result, true);
            if (string.IsNullOrEmpty(value))
                throw new HiringBellException("Fail to insert company group.");

            companyGrp = this.GetAllCompany();
            return companyGrp;
        }



        public OrganizationSettings GetCompanyById(int CompanyId)
        {
            OrganizationSettings result = _db.Get<OrganizationSettings>("sp_company_getById", new { CompanyId });
            return result;
        }

        public OrganizationSettings UpdateCompanyDetails(OrganizationSettings companyInfo, IFormFileCollection fileCollection)
        {
            OrganizationSettings company = new OrganizationSettings();
            if (companyInfo.CompanyId <= 0)
                throw new HiringBellException("Invalid organization detail submitted. Please login again.");

            if (string.IsNullOrEmpty(companyInfo.Email))
                throw new HiringBellException("Invalid organization email.");

            if (companyInfo.OrganizationName == null)
                throw new HiringBellException("Invalid Orgznization Name");

            company = _db.Get<OrganizationSettings>("sp_company_getById", new { companyInfo.CompanyId });

            if (company == null)
                throw new HiringBellException("Company doesn't exist.");
            else
            {
                company.OrganizationName = companyInfo.OrganizationName;
                company.CompanyName = companyInfo.CompanyName;
                company.CompanyDetail = companyInfo.CompanyDetail;
                company.FirstAddress = companyInfo.FirstAddress;
                company.SecondAddress = companyInfo.SecondAddress;
                company.ThirdAddress = companyInfo.ThirdAddress;
                company.FourthAddress = companyInfo.FourthAddress;
                company.Email = companyInfo.Email;
                company.PrimaryPhoneNo = companyInfo.PrimaryPhoneNo;
                company.SecondaryPhoneNo = companyInfo.SecondaryPhoneNo;
                company.Fax = companyInfo.Fax;
                company.Pincode = companyInfo.Pincode;
                company.FileId = companyInfo.FileId;
                company.MobileNo = companyInfo.MobileNo;
                company.City = companyInfo.City;
                company.Country = companyInfo.Country;
                company.FullAddress = companyInfo.FullAddress;
                company.GSTINNumber = companyInfo.GSTINNumber;
                company.InCorporationDate = companyInfo.InCorporationDate;
                company.LegalDocumentPath = companyInfo.LegalDocumentPath;
                company.LegalEntity = companyInfo.LegalEntity;
                company.LegalNameOfCompany = companyInfo.LegalNameOfCompany;
                company.PANNumber = companyInfo.PANNumber;
                company.SectorType = companyInfo.SectorType;
                company.State = companyInfo.State;
                company.TradeLicenseNumber = companyInfo.TradeLicenseNumber;
                company.TypeOfBusiness = companyInfo.TypeOfBusiness;
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

            var status = _db.Execute<OrganizationSettings>("sp_company_intupd",
                new
                {
                    company.CompanyId,
                    company.OrganizationName,
                    company.CompanyName,
                    company.CompanyDetail,
                    company.SectorType,
                    company.Country,
                    company.State,
                    company.City,
                    company.FirstAddress,
                    company.SecondAddress,
                    company.ThirdAddress,
                    company.FourthAddress,
                    company.FullAddress,
                    company.MobileNo,
                    company.Email,
                    company.PrimaryPhoneNo,
                    company.SecondaryPhoneNo,
                    company.Fax,
                    company.Pincode,
                    company.FileId,
                    company.PANNumber,
                    company.TradeLicenseNumber,
                    company.GSTINNumber,
                    company.LegalDocumentPath,
                    company.LegalEntity,
                    company.LegalNameOfCompany,
                    company.TypeOfBusiness,
                    company.InCorporationDate
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
                                        FileOwnerId = (companyInfo.OrganizationId),
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

            return company;
        }
    }
}
