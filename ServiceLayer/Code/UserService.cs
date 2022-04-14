using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.Profile;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
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
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly CurrentSession _currentSession;

        public UserService(IDb db, IFileService fileService, FileLocationDetail fileLocationDetail, IHostingEnvironment hostingEnvironment, CurrentSession currentSession)
        {
            _db = db;
            _fileService = fileService;
            _fileLocationDetail = fileLocationDetail;
            _hostingEnvironment = hostingEnvironment;
            _currentSession = currentSession;
        }

        public ProfileDetail UpdateProfile(ProfessionalUser professionalUser, int UserTypeId, int IsProfileImageRequest = 0)
        {
            ProfileDetail profileDetail = new ProfileDetail();
            var professionalUserDetail = JsonConvert.SerializeObject(professionalUser);
            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(professionalUser.UserId, typeof(long), "_UserId"),
                new DbParam(professionalUser.FileId, typeof(long), "_FileId"),
                new DbParam(professionalUser.Mobile_Number, typeof(string), "_Mobile"),
                new DbParam(professionalUser.Email, typeof(string), "_Email"),
                new DbParam(professionalUser.FirstName, typeof(string), "_FirstName"),
                new DbParam(professionalUser.LastName, typeof(string), "_LastName"),
                new DbParam(professionalUser.Date_Of_Application, typeof(DateTime), "_Date_Of_Application"),
                new DbParam(professionalUser.Total_Experience_In_Months, typeof(int), "_Total_Experience_In_Months"),
                new DbParam(professionalUser.Salary_Package, typeof(double), "_Salary_Package"),
                new DbParam(professionalUser.Notice_Period, typeof(int), "_Notice_Period"),
                new DbParam(professionalUser.Expeceted_CTC, typeof(double), "_Expeceted_CTC"),
                new DbParam(professionalUser.Current_Location, typeof(string), "_Current_Location"),
                new DbParam(JsonConvert.SerializeObject(professionalUser.Preferred_Locations), typeof(string), "_Preferred_Location"),
                new DbParam(professionalUserDetail, typeof(string), "_ProfessionalDetail_Json"),
                new DbParam(IsProfileImageRequest, typeof(int), "_IsProfileImageRequest")
            };

            var msg = _db.ExecuteNonQuery("sp_professionaldetail_insetupdate", dbParams, true);
            profileDetail = this.GetUserDetail(professionalUser.UserId, UserTypeId);
            return profileDetail;
        }

        public string UploadUserInfo(string userId, ProfessionalUser professionalUser, IFormFileCollection FileCollection)
        {
            var result = string.Empty;
            if (string.IsNullOrEmpty(professionalUser.Email))
            {
                throw new HiringBellException("Email id is required field.");
            }

            int IsProfileImageRequest = 0;
            Files file = new Files();
            if (FileCollection.Count > 0)
            {
                var files = FileCollection.Select(x => new Files
                {
                    FileUid = professionalUser.FileId,
                    FileName = x.Name,
                    Email = professionalUser.Email,
                    FileExtension = string.Empty
                }).ToList<Files>();
                _fileService.SaveFile(_fileLocationDetail.UserFolder, files, FileCollection, userId);

                var fileInfo = (from n in files
                                select new
                                {
                                    FileId = n.FileUid,
                                    FileOwnerId = professionalUser.UserId,
                                    FileName = n.FileName,
                                    FilePath = n.FilePath,
                                    FileExtension = n.FileExtension,
                                    UserTypeId = (int)UserType.Candidate,
                                    AdminId = _currentSession.CurrentUserDetail.UserId
                                });

                DataTable table = Converter.ToDataTable(fileInfo);
                var dataSet = new DataSet();
                dataSet.Tables.Add(table);
                _db.StartTransaction(IsolationLevel.ReadUncommitted);
                int insertedCount = _db.BatchInsert("sp_candidatefiledetail_InsUpd", dataSet, true);
                _db.Commit();
            }

            var value = this.UpdateProfile(professionalUser, (int)UserType.Candidate, IsProfileImageRequest);
            return result;
        }

        public string UploadResume(string userId, ProfessionalUser professionalUser, IFormFileCollection FileCollection)
        {
            var result = string.Empty;
            if (Int32.Parse(userId) <= 0)
            {
                throw new HiringBellException("");
            }
            
            Files file = new Files();
            if (FileCollection.Count > 0)
            {
                var files = FileCollection.Select(x => new Files
                {
                    FileUid = professionalUser.FileId,
                    FileName = x.Name,
                    Email = professionalUser.Email,
                    FileExtension = string.Empty
                }).ToList<Files>();
                _fileService.SaveFile(_fileLocationDetail.UserFolder, files, FileCollection, userId);

                var fileInfo = (from n in files
                                select new
                                {
                                    FileId = n.FileUid,
                                    FileOwnerId = professionalUser.UserId,
                                    FileName = n.FileName,
                                    FilePath = n.FilePath,
                                    FileExtension = n.FileExtension,
                                    UserTypeId = (int)UserType.Candidate,
                                    AdminId = _currentSession.CurrentUserDetail.UserId
                                });

                DataTable table = Converter.ToDataTable(fileInfo);
                var dataSet = new DataSet();
                dataSet.Tables.Add(table);
                _db.StartTransaction(IsolationLevel.ReadUncommitted);
                int insertedCount = _db.BatchInsert("sp_candidatefiledetail_InsUpd", dataSet, true);
                _db.Commit();
                if (insertedCount == 1)
                    result = "Resume Uploaded Successfully.";
            }

            return result;
        }


        public ProfileDetail GetUserDetail(long userId, int UserTypeId)
        {
            if (UserTypeId <= 0)
                throw new HiringBellException { UserMessage = "Invalid UserTypeId", FieldName = nameof(UserTypeId), FieldValue = UserTypeId.ToString() };

            if (userId <= 0)
                throw new HiringBellException { UserMessage = "Invalid User Id", FieldName = nameof(userId), FieldValue = userId.ToString() };

            ProfileDetail profileDetail = new ProfileDetail();
            //if (_currentSession.CurrentUserDetail.RoleId == 1)
            //{
            //    profileDetail.userDetail = new UserDetail();
            //    profileDetail.RoleId = (int)RolesName.Admin;
            //    return profileDetail;
            //}


            DbParam[] param = new DbParam[]
            {
               new DbParam(userId, typeof(long), "_UserId"),
               new DbParam(null, typeof(string), "_Mobile"),
               new DbParam(null, typeof(string), "_Email")
            };

            if(UserTypeId == 3)
            {
                var Result = _db.GetDataset("sp_professionaldetail_filter", param);

                if (Result.Tables.Count == 0)
                {
                    throw new HiringBellException("Fail to get record.");
                }
                else
                {
                    profileDetail.profileDetail = Converter.ToList<FileDetail>(Result.Tables[1]);
                    string jsonData = Convert.ToString(Result.Tables[0].Rows[0][0]);
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        profileDetail.professionalUser = JsonConvert.DeserializeObject<ProfessionalUser>(jsonData);
                    }
                    else
                    {
                        throw new HiringBellException("Fail to get record.");
                    }
                }
            }
            else
            {
                var Result = _db.GetDataset("sp_employee_profile", param);

                if (Result.Tables.Count == 0)
                {
                    throw new HiringBellException("Fail to get record.");
                }
                else
                {
                    profileDetail.profileDetail = Converter.ToList<FileDetail>(Result.Tables[1]);

                    var employeeProfessionDetail = Converter.ToType<EmployeeProfessionDetail>(Result.Tables[0]);
                    if (!string.IsNullOrEmpty(employeeProfessionDetail.ProfessionalDetail_Json))
                    {
                        profileDetail.professionalUser = JsonConvert.DeserializeObject<ProfessionalUser>(employeeProfessionDetail.ProfessionalDetail_Json);
                    }
                    else
                    {
                        profileDetail.professionalUser = new ProfessionalUser();
                        profileDetail.professionalUser.Name = employeeProfessionDetail.FirstName +" "+ employeeProfessionDetail.LastName;
                        profileDetail.professionalUser.Email = employeeProfessionDetail.Email;
                        profileDetail.professionalUser.Mobile_Number = employeeProfessionDetail.Mobile;
                        profileDetail.professionalUser.UserId = employeeProfessionDetail.EmployeeUid;
                    }
                }
            }
            return profileDetail;
        }

        public string GenerateResume(long userId)
        {
            if (userId <= 0)
                throw new HiringBellException { UserMessage = "Invalid User Id", FieldName = nameof(userId), FieldValue = userId.ToString() };

            var value = string.Empty;
            ProfileDetail profileDetail = new ProfileDetail();
            DbParam[] param = new DbParam[]
            {
               new DbParam(userId, typeof(long), "_UserId"),
               new DbParam(null, typeof(string), "_Mobile"),
               new DbParam(null, typeof(string), "_Email")
            };

            var Result = _db.GetDataset("sp_professionaldetail_filter", param);

            if (Result.Tables.Count == 0)
            {
                throw new HiringBellException("Fail to get record.");
            }
            else
            {
                profileDetail.profileDetail = Converter.ToList<FileDetail>(Result.Tables[1]);
                string jsonData = Convert.ToString(Result.Tables[0].Rows[0][0]);
                if (!string.IsNullOrEmpty(jsonData))
                {
                    profileDetail.professionalUser = JsonConvert.DeserializeObject<ProfessionalUser>(jsonData);
                }
                else
                {
                    throw new HiringBellException("Fail to get record.");
                }

                string rootPath = _hostingEnvironment.ContentRootPath;
                string templatePath = Path.Combine(rootPath,
                    _fileLocationDetail.Location,
                    Path.Combine(_fileLocationDetail.resumePath.ToArray()),
                    _fileLocationDetail.resumeTemplate
                );
            }
                
            return value;
        }
    }
}
