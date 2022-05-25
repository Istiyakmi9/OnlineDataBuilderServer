using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.Profile;
using Newtonsoft.Json;
using ServiceLayer.Caching;
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
        private readonly IEmployeeService _employeeService;
        private readonly ICacheManager _cacheManager;

        public UserService(
            IDb db,
            IFileService fileService,
            FileLocationDetail fileLocationDetail,
            IHostingEnvironment hostingEnvironment,
            CurrentSession currentSession,
            IEmployeeService employeeService,
            ICacheManager cacheManager)
        {
            _db = db;
            _cacheManager = cacheManager;
            _fileService = fileService;
            _fileLocationDetail = fileLocationDetail;
            _hostingEnvironment = hostingEnvironment;
            _currentSession = currentSession;
            _employeeService = employeeService;
        }

        public ProfileDetail UpdateProfile(ProfessionalUser professionalUser, int UserTypeId, int IsProfileImageRequest = 0)
        {
            long employeeId = 0;
            ProfileDetail profileDetail = new ProfileDetail();
            var professionalUserDetail = JsonConvert.SerializeObject(professionalUser);

            if (UserTypeId == 3)
            {
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
                employeeId = Convert.ToInt64(msg);
            }
            else if (UserTypeId == 1)
            {
                int empId = Convert.ToInt32(_currentSession.CurrentUserDetail.UserId);
                Employee employee = _employeeService.GetEmployeeByIdService(empId);
                DbParam[] dbParams = new DbParam[]
                {
                    new DbParam(employee.EmployeeUid, typeof(long), "_EmployeeUid"),
                    new DbParam(professionalUser.FirstName, typeof(string), "_FirstName"),
                    new DbParam(professionalUser.LastName, typeof(string), "_LastName"),
                    new DbParam(employee.FatherName, typeof(string), "_FatherName"),
                    new DbParam(professionalUser.Email, typeof(string), "_Email"),
                    new DbParam(employee.MotherName, typeof(string), "_MotherName"),
                    new DbParam(employee.SpouseName, typeof(string), "_SpouseName"),
                    new DbParam(employee.Gender, typeof(bool), "_Gender"),
                    new DbParam(employee.State, typeof(string), "_State"),
                    new DbParam(employee.City, typeof(string), "_City"),
                    new DbParam(employee.Pincode, typeof(int), "_Pincode"),
                    new DbParam(employee.Address, typeof(string), "_Address"),
                    new DbParam(employee.Mobile, typeof(string), "_Mobile"),
                    new DbParam(employee.SecondaryMobile, typeof(string), "_SecondaryMobile"),
                    new DbParam(employee.PANNo, typeof(string), "_PANNo"),
                    new DbParam(employee.AadharNo, typeof(string), "_AadharNo"),
                    new DbParam(employee.AccountNumber, typeof(string), "_AccountNumber"),
                    new DbParam(employee.BankName, typeof(string), "_BankName"),
                    new DbParam(employee.BranchName, typeof(string), "_BranchName"),
                    new DbParam(employee.IFSCCode, typeof(string), "_IFSCCode"),
                    new DbParam(employee.Domain, typeof(string), "_Domain"),
                    new DbParam(employee.IsPermanent, typeof(bool), "_IsPermanent"),
                    new DbParam(employee.ClientUid, typeof(long), "_AllocatedClientId"),
                    new DbParam(employee.ClientName, typeof(string), "_AllocatedClientName"),
                    new DbParam(employee.ActualPackage, typeof(float), "_ActualPackage"),
                    new DbParam(employee.FinalPackage, typeof(float), "_FinalPackage"),
                    new DbParam(employee.TakeHomeByCandidate, typeof(float), "_TakeHomeByCandidate"),
                    new DbParam(employee.Specification, typeof(string), "_Specification"),
                    new DbParam(employee.ExprienceInYear, typeof(float), "_ExprienceInYear"),
                    new DbParam(employee.LastCompanyName, typeof(string), "_LastCompanyName"),
                    new DbParam(employee.ReportingManagerId, typeof(long), "_ReportingManagerId"),
                    new DbParam(employee.DesignationId, typeof(int), "_DesignationId"),
                    new DbParam(null, typeof(long), "_Password"),
                    new DbParam(employee.AccessLevelId, typeof(int), "_AccessLevelId"),
                    new DbParam(employee.UserTypeId, typeof(int), "_UserTypeId"),
                    new DbParam(professionalUserDetail, typeof(string), "_ProfessionalDetail_Json"),
                    new DbParam(_currentSession.CurrentUserDetail.UserId, typeof(long), "_AdminId"),
                };

                var msg = _db.ExecuteNonQuery("sp_Employees_InsUpdate", dbParams, true);
                employeeId = Convert.ToInt64(msg);
            }
            profileDetail = this.GetUserDetail(employeeId, UserTypeId);
            return profileDetail;
        }

        public string UploadUserInfo(string userId, ProfessionalUser professionalUser, IFormFileCollection FileCollection, int UserTypeId)
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
                                    UserTypeId = UserTypeId,
                                    AdminId = _currentSession.CurrentUserDetail.UserId
                                });

                DataTable table = Converter.ToDataTable(fileInfo);
                var dataSet = new DataSet();
                dataSet.Tables.Add(table);
                _db.StartTransaction(IsolationLevel.ReadUncommitted);
                int insertedCount = _db.BatchInsert("sp_candidatefiledetail_InsUpd", dataSet, true);

                _db.Commit();
            }

            var value = this.UpdateProfile(professionalUser, UserTypeId, IsProfileImageRequest);
            return result;
        }

        public string UploadResume(string userId, ProfessionalUser professionalUser, IFormFileCollection FileCollection, int UserTypeId)
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
                                    UserTypeId = UserTypeId,
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

        public string UploadDeclaration(string UserId, int UserTypeId, UserDetail userDetail, IFormFileCollection FileCollection, List<Files> files)
        {
            string result = string.Empty;
            if (Int32.Parse(UserId) <= 0)
            {
                throw new HiringBellException("Invalid UserId");
            }

            if (UserTypeId <= 0)
            {
                throw new HiringBellException("Invalid UserTypeId");
            }

            // Files file = new Files();
            if (FileCollection.Count > 0)
            {
                _fileService.SaveFile(_fileLocationDetail.UserFolder, files, FileCollection, UserId);
                var fileInfo = (from n in files
                                select new
                                {
                                    FileId = n.FileUid,
                                    FileOwnerId = UserId,
                                    FileName = n.FileName,
                                    FilePath = n.FilePath,
                                    FileExtension = n.FileExtension,
                                    UserTypeId = UserTypeId,
                                    AdminId = _currentSession.CurrentUserDetail.UserId
                                });

                DataTable table = Converter.ToDataTable(fileInfo);
                var dataSet = new DataSet();
                dataSet.Tables.Add(table);
                _db.StartTransaction(IsolationLevel.ReadUncommitted);
                int insertedCount = _db.BatchInsert("", dataSet, true);
                _db.Commit();
                if (insertedCount == 1)
                    result = "Declaration Uploaded Successfully.";
            }
            return result;
        }



        public ProfileDetail GetUserDetail(long userId, int UserTypeId)
        {
            if (UserTypeId <= 0)
                throw new HiringBellException { UserMessage = "Invalid UserTypeId", FieldName = nameof(UserTypeId), FieldValue = UserTypeId.ToString() };

            if (userId <= 0)
                throw new HiringBellException { UserMessage = "Invalid Employee Id", FieldName = nameof(userId), FieldValue = userId.ToString() };

            ProfileDetail profileDetail = new ProfileDetail();

            if (UserTypeId == 3)
            {
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
                }
            }
            else
            {
                DbParam[] param = new DbParam[]
                {
                   new DbParam(userId, typeof(long), "_UserId"),
                   new DbParam(null, typeof(string), "_Mobile"),
                   new DbParam(null, typeof(string), "_Email"),
                   new DbParam(UserTypeId, typeof(int), "_UserTypeId")
                };
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
                    //profileDetail.professionalUser = new ProfessionalUser();
                    if (string.IsNullOrEmpty(profileDetail.professionalUser.FirstName) && string.IsNullOrEmpty(profileDetail.professionalUser.LastName))
                    {
                        profileDetail.professionalUser.FirstName = employeeProfessionDetail.FirstName;
                        profileDetail.professionalUser.LastName = employeeProfessionDetail.LastName;
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

        public async Task<DataSet> GetEmployeeAndChientListService()
        {
            DataSet ds = new DataSet();
            if (!_cacheManager.IsEmpty())
            {
                await Task.Run(() =>
                {
                    var emps = _cacheManager.Get(ServiceLayer.Caching.Table.Employee);
                    emps.TableName = "Employees";
                    var clients = _cacheManager.Get(ServiceLayer.Caching.Table.Client);
                    clients.TableName = "Clients";
                    ds.Tables.Add(emps.Copy());
                    ds.Tables.Add(clients.Copy());
                });
            }
            else
            {
                throw new HiringBellException("Application data not found.", "ApplicationCache", null, System.Net.HttpStatusCode.Unauthorized);
            }
            return ds;
        }
    }
}
