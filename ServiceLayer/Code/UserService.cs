using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ModalLayer.Modal;
using ModalLayer.Modal.Profile;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class UserService : IUserService
    {
        private readonly IDb _db;
        private readonly IFileService _fileService;
        private readonly FileLocationDetail _fileLocationDetail;

        public UserService(IDb db, IFileService fileService, FileLocationDetail fileLocationDetail)
        {
            _db = db;
            _fileService = fileService;
            _fileLocationDetail = fileLocationDetail;
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

        public string UploadUserInfo(string userId, UserInfo userInfo, IFormFileCollection FileCollection)
        {
            if (string.IsNullOrEmpty(userInfo.Email))
            {
                throw new HiringBellException("Email id is required field.");
            }

            int IsProfileImageRequest = 0;
            List<Files> files = new List<Files>();
            Files file = new Files();
            if(FileCollection.Count > 0)
            {
                IsProfileImageRequest = 1;
                Parallel.ForEach(FileCollection, x =>
                {
                    files.Add(new Files
                    {
                        FileName = x.Name,
                        FilePath = string.Empty,
                        Email = userInfo.Email
                    });
                });

                _fileService.SaveFile(_fileLocationDetail.UserFolder, files, FileCollection, userId);
                file = files.Single();
            }

            DbParam[] param = new DbParam[]
            {
                new DbParam(userId, typeof(long), "_UserId"),
                new DbParam(userInfo.FirstName + " " + userInfo.LastName, typeof(string), "_Name"),
                new DbParam(null, typeof(string), "_Alternet_Numbers"),
                new DbParam(userInfo.ResumeHeadline, typeof(string), "_Resume_Headline"),
                new DbParam(userInfo.FileId, typeof(long), "_FileId"),
                new DbParam(file.FilePath, typeof(string), "_FilePath"),
                new DbParam(file.FileName, typeof(string), "_FileName"),
                new DbParam(file.FileExtension, typeof(string), "_FileExtension"),
                new DbParam(0, typeof(int), "_UserTypeId"),
                new DbParam(IsProfileImageRequest, typeof(int), "_IsProfileImageRequest")
            };

            var result = _db.ExecuteNonQuery("sp_ProfessionalCandidates_UpdInfo", param, true);
            if (string.IsNullOrEmpty(result))
            {
                _fileService.DeleteFiles(files);
                throw new HiringBellException("Fail to update user info.");
            }
            return result;
        }

        public DataSet GetUserDetail(long userId)
        {
            DbParam[] param = new DbParam[]
            {
               new DbParam(userId, typeof(long), "_UserId"),
               new DbParam(null, typeof(string), "_MobileNo"),
               new DbParam(null, typeof(string), "_EmailId")
            };

            var Result = _db.GetDataset("sp_professionalcandidates_Get", param);
            if (Result.Tables.Count == 0)
                throw new HiringBellException("Fail to get record.");
            return Result;
        }
    }
}
