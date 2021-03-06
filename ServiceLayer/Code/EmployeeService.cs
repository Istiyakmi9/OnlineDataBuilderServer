using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using Newtonsoft.Json;
using ServiceLayer.Caching;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IDb _db;
        private readonly CommonFilterService _commonFilterService;
        private readonly ICommonService _commonService;
        private readonly CurrentSession _currentSession;
        private readonly IFileService _fileService;
        private readonly FileLocationDetail _fileLocationDetail;
        private readonly IConfiguration _configuration;
        private readonly ICacheManager _cacheManager;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILoginService _loginService;
        private readonly IDeclarationService _declarationService;

        public EmployeeService(IDb db,
            CommonFilterService commonFilterService,
            CurrentSession currentSession,
            ICacheManager cacheManager,
            IFileService fileService,
            ICommonService commonService,
            IConfiguration configuration,
            ILoginService loginService,
            IDeclarationService declarationService,
            IAuthenticationService authenticationService,
            FileLocationDetail fileLocationDetail)
        {
            _db = db;
            _cacheManager = cacheManager;
            _loginService = loginService;
            _authenticationService = authenticationService;
            _configuration = configuration;
            _commonFilterService = commonFilterService;
            _currentSession = currentSession;
            _fileService = fileService;
            _fileLocationDetail = fileLocationDetail;
            _commonService = commonService;
            _declarationService = declarationService;
        }
        public List<Employee> GetEmployees(FilterModel filterModel)
        {
            int skipValue = (filterModel.PageIndex - 1) * 10;
            int takeValue = 10;
            var table = _commonService.LoadEmployeeData();
            List<Employee> employees = Converter.ToList<Employee>(table);
            // List<Employee> employees = _commonFilterService.GetResult<Employee>(filterModel, "SP_Employees_Get");

            if (filterModel.IsActive == true)
            {
                employees = employees.FindAll(x => x.IsActive == true);
            }
            else if (filterModel.IsActive == false)
            {
                //employees = employees.FindAll(x => x.IsActive == false);
                DataSet data = _db.GetDataset("SP_DeActivatedEmployee_Get", null);
                employees = Converter.ToList<Employee>(data.Tables[0]);
            }
            else
            {
                DataSet data = _db.GetDataset("SP_Employee_GetAll", null);
                employees = Converter.ToList<Employee>(data.Tables[0]);
            }

            List<Employee> emp = null;
            if (filterModel.SortBy == "FirstName Asc")
                emp = employees.OrderBy(x => x.FirstName).Skip(skipValue).Take(takeValue).ToList();
            else if (filterModel.SortBy == "FirstName Desc")
                emp = employees.OrderByDescending(x => x.FirstName).Skip(skipValue).Take(takeValue).ToList();
            if (filterModel.SortBy == "Mobile Asc")
                emp = employees.OrderBy(x => x.Mobile).Skip(skipValue).Take(takeValue).ToList();
            else if (filterModel.SortBy == "Mobile Desc")
                emp = employees.OrderByDescending(x => x.Mobile).Skip(skipValue).Take(takeValue).ToList();
            if (filterModel.SortBy == "Email Asc")
                emp = employees.OrderBy(x => x.Email).Skip(skipValue).Take(takeValue).ToList();
            else if (filterModel.SortBy == "Email Desc")
                emp = employees.OrderByDescending(x => x.Email).Skip(skipValue).Take(takeValue).ToList();

            int total = employees.Count;
            Parallel.For(0, total, i => employees[i].Total = total);

            if (emp == null)
                emp = employees;

            if (filterModel.SearchString.Contains("Global"))
            {
                int lastIndex = filterModel.SearchString.Length - 13;
                string value = (filterModel.SearchString.Substring(13, lastIndex)).ToLower();
                var result = emp.FindAll(x => x.Email.Contains(value));
                if (result.Count == 0)
                {
                    string saerchValue = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
                    result = emp.FindAll(x => x.FirstName.Contains(saerchValue));
                }
                if (result.Count == 0)
                {
                    result = emp.FindAll(x => x.Mobile.Contains(value));
                }
                emp = result;

                total = emp.Count;
                Parallel.For(0, total, i => emp[i].Total = total);
            }
            else
            {
                if (filterModel.SearchString.Contains("FirstName"))
                {
                    int lastIndex = filterModel.SearchString.Length - 16;
                    string searchValue = (filterModel.SearchString.Substring(16, lastIndex)).ToLower();
                    string value = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(searchValue);
                    emp = emp.FindAll(x => x.FirstName.Contains(value));
                    total = emp.Count;
                    Parallel.For(0, total, i => emp[i].Total = total);
                }
                else if (filterModel.SearchString.Contains("Email"))
                {
                    int lastIndex = filterModel.SearchString.Length - 11;
                    string value = (filterModel.SearchString.Substring(11, lastIndex)).ToLower();
                    emp = emp.FindAll(x => x.Email.Contains(value));
                    total = emp.Count;
                    Parallel.For(0, total, i => emp[i].Total = total);
                }
                else if (filterModel.SearchString.Contains("Mobile"))
                {
                    int lastIndex = filterModel.SearchString.Length - 12;
                    string value = filterModel.SearchString.Substring(12, lastIndex);
                    emp = emp.FindAll(x => x.Mobile.Contains(value));
                    total = emp.Count;
                    Parallel.For(0, total, i => emp[i].Total = total);
                }

            }

            if (string.IsNullOrEmpty(filterModel.SortBy))
                emp = emp.OrderBy(x => x.CreatedOn).Skip(skipValue).Take(takeValue).ToList();

            return emp;
        }

        public DataSet GetManageEmployeeDetailService(long EmployeeId)
        {
            DataSet finalResultSet = new DataSet();
            DbParam[] param = new DbParam[]
            {
                new DbParam(EmployeeId, typeof(long), "_employeeId")
            };
            var resultset = _db.GetDataset("SP_ManageEmployeeDetail_Get", param);
            if (resultset.Tables.Count == 6)
            {
                resultset.Tables[0].TableName = "Employee";
                resultset.Tables[1].TableName = "AllocatedClients";
                resultset.Tables[2].TableName = "FileDetail";
                resultset.Tables[3].TableName = "Roles";
                resultset.Tables[4].TableName = "SalaryDetail";
                resultset.Tables[5].TableName = "Companies";

                finalResultSet.Tables.Add(_cacheManager.Get(ServiceLayer.Caching.Table.Employee).Copy());
                finalResultSet.Tables[0].TableName = "EmployeesList";

                finalResultSet.Tables.Add(_cacheManager.Get(ServiceLayer.Caching.Table.Client).Copy());
                finalResultSet.Tables[1].TableName = "Clients";

                finalResultSet.Tables.Add(resultset.Tables[0].Copy());
                finalResultSet.Tables.Add(resultset.Tables[1].Copy());
                finalResultSet.Tables.Add(resultset.Tables[2].Copy());
                finalResultSet.Tables.Add(resultset.Tables[3].Copy());
                finalResultSet.Tables.Add(resultset.Tables[4].Copy());
                finalResultSet.Tables.Add(resultset.Tables[5].Copy());
            }
            return finalResultSet;
        }

        public DataSet GetManageClientService(long EmployeeId)
        {
            DbParam[] param = new DbParam[]
            {
                new DbParam(EmployeeId, typeof(long), "_employeeId")
            };
            var resultset = _db.GetDataset("SP_MappedClients_Get", param);
            if (resultset.Tables.Count == 1)
            {
                resultset.Tables[0].TableName = "AllocatedClients";
            }
            return resultset;
        }

        public DataSet UpdateEmployeeDetailService(Employee employee, bool IsUpdating)
        {
            if (employee.EmployeeUid <= 0)
                throw new HiringBellException { UserMessage = "Invalid EmployeeId.", FieldName = nameof(employee.EmployeeUid), FieldValue = employee.EmployeeUid.ToString() };

            if (employee.ClientUid <= 0)
                throw new HiringBellException { UserMessage = "Invalid ClientId.", FieldName = nameof(employee.ClientUid), FieldValue = employee.ClientUid.ToString() };

            if (IsUpdating == true)
            {
                if (employee.EmployeeMappedClientsUid <= 0)
                    throw new HiringBellException { UserMessage = "EmployeeMappedClientId is invalid.", FieldName = nameof(employee.EmployeeMappedClientsUid), FieldValue = employee.EmployeeMappedClientsUid.ToString() };
            }

            this.ValidateEmployeeDetails(employee);

            DbParam[] param = new DbParam[]
            {
                new DbParam(employee.EmployeeMappedClientsUid, typeof(long), "_employeeMappedClientsUid"),
                new DbParam(employee.EmployeeUid, typeof(long), "_employeeUid"),
                new DbParam(employee.ClientUid, typeof(long), "_clientUid"),
                new DbParam(employee.FinalPackage, typeof(float), "_finalPackage"),
                new DbParam(employee.ActualPackage, typeof(float), "_actualPackage"),
                new DbParam(employee.TakeHomeByCandidate, typeof(float), "_takeHome"),
                new DbParam(employee.IsPermanent, typeof(bool), "_isPermanent"),
                new DbParam(employee.BillingHours, typeof(int), "_BillingHours"),
                new DbParam(employee.WorkingDaysPerWeek, typeof(int), "_DaysPerWeek"),
                new DbParam(employee.DateOfLeaving, typeof(DateTime), "_DateOfLeaving")
            };
            var resultset = _db.GetDataset("SP_Employees_AddUpdateRemoteClient", param);
            return resultset;
        }

        public Employee GetEmployeeByIdService(int EmployeeId, bool? IsActive)
        {
            int statusValue = -1;
            switch (IsActive)
            {
                case true:
                    statusValue = 1;
                    break;
                case false:
                    statusValue = 0;
                    break;
            }

            Employee employee = default;
            DbParam[] param = new DbParam[]
            {
                new DbParam(EmployeeId, typeof(int), "_EmployeeId"),
                new DbParam(statusValue, typeof(int), "_IsActive")
            };

            var resultSet = _db.GetDataset("SP_Employees_ById", param);
            if (resultSet.Tables.Count > 0 && resultSet.Tables[0].Rows.Count > 0)
            {
                var emps = Converter.ToList<Employee>(resultSet.Tables[0]);
                if (emps != null && emps.Count > 0)
                    employee = emps[0];
            }
            return employee;
        }

        public List<Employee> DeleteEmployeeById(int EmployeeId, bool IsActive)
        {
            List<Employee> employees = null;
            var status = string.Empty;
            DbParam[] param = new DbParam[]
            {
                new DbParam(EmployeeId, typeof(int), "_employeeId"),
            };

            if (!IsActive)
                status = _db.ExecuteNonQuery("sp_Employee_DeActivate", param, false);
            else
                status = _db.ExecuteNonQuery("sp_Employee_Activate", param, false);

            if (!string.IsNullOrEmpty(status))
            {
                _loginService.BuildApplicationCache(true);
                var table = _commonService.LoadEmployeeData();
                employees = Converter.ToList<Employee>(table);
                if (IsActive == true)
                {
                    employees = employees.FindAll(x => x.IsActive == false);
                }
                else if (IsActive == false)
                {
                    employees = employees.FindAll(x => x.IsActive == true);
                }
                int total = employees.Count;
                Parallel.For(0, total, i => employees[i].Total = total);
                //employees = this.GetEmployees(new FilterModel
                //{
                //    PageIndex = 1,
                //    PageSize = 10,
                //    SearchString = "1=1",
                //    SortBy = string.Empty,
                //});
            }
            return employees;
        }

        public async Task<DataSet> RegisterEmployee(Employee employee, List<AssignedClients> assignedClients, IFormFileCollection fileCollection, bool IsUpdating)
        {
            if (IsUpdating == true)
            {
                if (employee.EmployeeUid <= 0)
                    throw new HiringBellException { UserMessage = "Invalid EmployeeId.", FieldName = nameof(employee.EmployeeUid), FieldValue = employee.EmployeeUid.ToString() };
            }
            this.ValidateEmployee(employee);
            this.ValidateEmployeeDetails(employee);

            //TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
            //employee.DOB = TimeZoneInfo.ConvertTimeFromUtc(employee.DOB, istTimeZome);
            //employee.DateOfJoining = TimeZoneInfo.ConvertTimeFromUtc(employee.DateOfJoining, istTimeZome);
            int empId = Convert.ToInt32(employee.EmployeeUid);

            Employee employeeDetail = this.GetEmployeeByIdService(empId, employee.IsActive);
            if (employeeDetail == null)
            {
                employeeDetail = new Employee
                {
                    EmpProfDetailUid = -1
                };
            }

            var professionalDetail = new EmployeeProfessionDetail
            {
                AadharNo = employee.AadharNo,
                AccountNumber = employee.AccountNumber,
                BankName = employee.BankName,
                BranchName = employee.BranchName,
                CreatedBy = employee.EmployeeUid,
                CreatedOn = employee.CreatedOn,
                Domain = employee.Domain,
                Email = employee.Email,
                EmployeeUid = employee.EmployeeUid,
                EmpProfDetailUid = employeeDetail.EmpProfDetailUid,
                ExperienceInYear = employee.ExperienceInYear,
                FirstName = employee.FirstName,
                IFSCCode = employee.IFSCCode,
                LastCompanyName = employee.LastCompanyName,
                LastName = employee.LastName,
                Mobile = employee.Mobile,
                PANNo = employee.PANNo,
                SecomdaryMobile = employee.SecondaryMobile,
                Specification = employee.Specification,
            };
            employeeDetail.ProfessionalDetail_Json = JsonConvert.SerializeObject(professionalDetail);


            EmployeeDeclaration employeeDeclaration = new EmployeeDeclaration
            {
                SalaryDetail = new EmployeeSalaryDetail
                {
                    CTC = employee.CTC
                }
            };
            EmployeeSalaryDetail employeeSalaryDetail = _declarationService.CalculateSalaryDetail(0, employeeDeclaration, employee.CTC);

            return await Task.Run(() =>
            {
                string EncreptedPassword = _authenticationService.Encrypt(
                    _configuration.GetSection("DefaultNewEmployeePassword").Value,
                    _configuration.GetSection("EncryptSecret").Value
                );

                employee.CompleteSalaryDetail = "{}";
                employee.TaxDetail = "[]";

                var employeeId = _db.Execute<Employee>("sp_Employees_InsUpdate", new
                {
                    employee.EmployeeUid,
                    employee.FirstName,
                    employee.LastName,
                    employee.Mobile,
                    employee.Email,
                    employee.SecondaryMobile,
                    employee.FatherName,
                    employee.MotherName,
                    employee.SpouseName,
                    employee.Gender,
                    employee.State,
                    employee.City,
                    employee.Pincode,
                    employee.Address,
                    employee.PANNo,
                    employee.AadharNo,
                    employee.AccountNumber,
                    employee.BankName,
                    employee.BranchName,
                    employee.IFSCCode,
                    employee.Domain,
                    employee.Specification,
                    employee.ExprienceInYear,
                    employee.LastCompanyName,
                    employee.IsPermanent,
                    employee.ActualPackage,
                    employee.FinalPackage,
                    employee.TakeHomeByCandidate,
                    employee.ReportingManagerId,
                    employee.DesignationId,
                    employeeDetail.ProfessionalDetail_Json,
                    Password = EncreptedPassword,
                    employee.AccessLevelId,
                    employee.UserTypeId,
                    employee.CompanyId,
                    employeeSalaryDetail.CTC,
                    employeeSalaryDetail.GrossIncome,
                    employeeSalaryDetail.NetSalary,
                    employeeSalaryDetail.CompleteSalaryDetail,
                    employeeSalaryDetail.TaxDetail,
                    AdminId = _currentSession.CurrentUserDetail.UserId,
                },
                    true
                );


                if (string.IsNullOrEmpty(employeeId) || employeeId == "0")
                {
                    throw new HiringBellException("Fail to insert or update record. Contact to admin.");
                }

                _loginService.BuildApplicationCache(true);
                long currentEmployeeId = Convert.ToInt64(employeeId);
                if (fileCollection.Count > 0)
                {
                    var files = fileCollection.Select(x => new Files
                    {
                        FileUid = employee.FileId,
                        FileName = ApplicationConstants.ProfileImage,
                        Email = employee.Email,
                        FileExtension = string.Empty
                    }).ToList<Files>();
                    _fileService.SaveFile(_fileLocationDetail.UserFolder, files, fileCollection, employeeId);

                    var fileInfo = (from n in files
                                    select new
                                    {
                                        FileId = n.FileUid,
                                        FileOwnerId = currentEmployeeId,
                                        FileName = n.FileName,
                                        FilePath = n.FilePath,
                                        FileExtension = n.FileExtension,
                                        UserTypeId = (int)UserType.Employee,
                                        AdminId = _currentSession.CurrentUserDetail.UserId
                                    });

                    DataTable table = Converter.ToDataTable(fileInfo);
                    _db.StartTransaction(IsolationLevel.ReadUncommitted);
                    int insertedCount = _db.BatchInsert("sp_userfiledetail_Upload", table, false);
                    _db.Commit();
                }

                var ResultSet = this.GetManageEmployeeDetailService(currentEmployeeId);
                return ResultSet;
            });
        }

        private void ValidateEmployeeDetails(Employee employee)
        {

            if (employee.ActualPackage < 0)
                throw new HiringBellException { UserMessage = "Invalid Actual Package.", FieldName = nameof(employee.ActualPackage), FieldValue = employee.ActualPackage.ToString() };

            if (employee.FinalPackage < 0)
                throw new HiringBellException { UserMessage = "Invalid Final Package.", FieldName = nameof(employee.FinalPackage), FieldValue = employee.FinalPackage.ToString() };

            if (employee.TakeHomeByCandidate < 0)
                throw new HiringBellException { UserMessage = "Invalid TakeHome By Candidate.", FieldName = nameof(employee.TakeHomeByCandidate), FieldValue = employee.TakeHomeByCandidate.ToString() };

            if (employee.FinalPackage < employee.ActualPackage)
                throw new HiringBellException { UserMessage = "Final package must be greater that or equal to Actual package.", FieldName = nameof(employee.FinalPackage), FieldValue = employee.FinalPackage.ToString() };

            if (employee.ActualPackage < employee.TakeHomeByCandidate)
                throw new HiringBellException { UserMessage = "Actual package must be greater that or equal to TakeHome package.", FieldName = nameof(employee.ActualPackage), FieldValue = employee.ActualPackage.ToString() };
        }

        private void ValidateEmployee(Employee employee)
        {
            if (string.IsNullOrEmpty(employee.Email))
                throw new HiringBellException { UserMessage = "Email id is a mandatory field.", FieldName = nameof(employee.Email), FieldValue = employee.Email.ToString() };

            if (string.IsNullOrEmpty(employee.FirstName))
                throw new HiringBellException { UserMessage = "First Name is a mandatory field.", FieldName = nameof(employee.FirstName), FieldValue = employee.FirstName.ToString() };

            if (string.IsNullOrEmpty(employee.LastName))
                throw new HiringBellException { UserMessage = "Last Name is a mandatory field.", FieldName = nameof(employee.LastName), FieldValue = employee.LastName.ToString() };

            if (string.IsNullOrEmpty(employee.Mobile))
                throw new HiringBellException { UserMessage = "Mobile number is a mandatory field.", FieldName = nameof(employee.Mobile), FieldValue = employee.Mobile.ToString() };

            if (employee.DesignationId <= 0)
                throw new HiringBellException { UserMessage = "Designation is a mandatory field.", FieldName = nameof(employee.DesignationId), FieldValue = employee.DesignationId.ToString() };

            if (employee.ReportingManagerId < 0)
                throw new HiringBellException { UserMessage = "Reporting Manager is a mandatory field.", FieldName = nameof(employee.ReportingManagerId), FieldValue = employee.ReportingManagerId.ToString() };

            if (employee.UserTypeId <= 0)
                throw new HiringBellException { UserMessage = "User Type is a mandatory field.", FieldName = nameof(employee.UserTypeId), FieldValue = employee.UserTypeId.ToString() };

            if (employee.AccessLevelId <= 0)
                throw new HiringBellException { UserMessage = "Role is a mandatory field.", FieldName = nameof(employee.AccessLevelId), FieldValue = employee.AccessLevelId.ToString() };


        }
    }
}
