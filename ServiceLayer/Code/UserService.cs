using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.Profile;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ServiceLayer.Code
{
    public class UserService : IUserService
    {
        private readonly IDb _db;
        private readonly IFileService _fileService;


        public UserService(IDb db, IFileService fileService)
        {
            _db = db;
            _fileService = fileService;
        }

        public string ManageEmploymentDetail(EmploymentDetail employmentDetail)
        {
            var result = string.Empty;
            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(employmentDetail.CurrentCompany, typeof(string), ""),
                new DbParam(employmentDetail.CurrentSalary, typeof(string), ""),
                new DbParam(employmentDetail.CurrentSalaryLakh, typeof(int), ""),
                new DbParam(employmentDetail.CurrentSalaryThousand, typeof(int), ""),
                new DbParam(employmentDetail.Designation, typeof(string), ""),
                new DbParam(employmentDetail.Experties, typeof(string), ""),
                new DbParam(employmentDetail.YourOrganization, typeof(string), ""),
                new DbParam(employmentDetail.WorkingYear, typeof(int), ""),
                new DbParam(employmentDetail.WorkingMonth, typeof(int), ""),
                new DbParam(employmentDetail.JobProfile, typeof(string), ""),
                new DbParam(employmentDetail.NoticePeriod, typeof(string), ""),
                new DbParam(employmentDetail.WorkedYear, typeof(int), ""),
                new DbParam(employmentDetail.WorkedMonth, typeof(int), "")
            };

            return result;
        }

        public string ManageEducationDetail(List<EducationDetail> educationDetails)
        {
            var result = string.Empty;
            //var permissionMenu = (from n in educationDetails
            //                      select new EducationDetail
            //                      {
            //                          RoleAccessibilityMappingId = -1,
            //                          AccessLevelId = rolesAndMenus.AccessLevelId,
            //                          AccessCode = n.AccessCode,
            //                          AccessibilityId = n.Permission
            //                      }).ToList<EducationDetail>();

            //DataSet ds = Converter.ToDataSet<RoleAccessibilityMapping>(permissionMenu);
            //result = _db.BatchInsert("sp_role_accessibility_mapping_InsUpd", ds, false);
            return result;
        }

        public string UploadResume(Files fileDetail, IFormFileCollection FileCollection)
        {
            return null;
        }
    }
}
