using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.Profile;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
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

        public EmployeeService(IDb db,
            CommonFilterService commonFilterService,
            CurrentSession currentSession,
            IFileService fileService,
            ICommonService commonService,
            FileLocationDetail fileLocationDetail)
        {
            _db = db;
            _commonFilterService = commonFilterService;
            _currentSession = currentSession;
            _fileService = fileService;
            _fileLocationDetail = fileLocationDetail;
            _commonService = commonService;
        }
        public List<Employee> GetEmployees(FilterModel filterModel)
        {
            // var table = _commonService.LoadEmployeeData();
            // List<Employee> employees = Converter.ToList<Employee>(table);
            List<Employee> employees = _commonFilterService.GetResult<Employee>(filterModel, "SP_Employees_Get");
            return employees;
        }

        public DataSet GetManageEmployeeDetailService(long EmployeeId)
        {
            DbParam[] param = new DbParam[]
            {
                new DbParam(EmployeeId, typeof(long), "_employeeId")
            };
            var resultset = _db.GetDataset("SP_ManageEmployeeDetail_Get", param);
            if (resultset.Tables.Count == 5)
            {
                resultset.Tables[0].TableName = "Employee";
                resultset.Tables[1].TableName = "Clients";
                resultset.Tables[2].TableName = "AllocatedClients";
                resultset.Tables[3].TableName = "FileDetail";
                resultset.Tables[4].TableName = "EmployeesList";
            }
            return resultset;
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
                defaul:
                    statusValue = -1;
                    break;
            }

            Employee employee = default;
            DbParam[] param = new DbParam[]
            {
                new DbParam(EmployeeId, typeof(int), "_EmployeeId"),
                new DbParam(statusValue, typeof(int), "_IsActive")
                //new DbParam(IsActive ? 1 : 0, typeof(int), "_IsActive")

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
                new DbParam(IsActive, typeof(bool), "_active"),
                new DbParam(_currentSession.CurrentUserDetail.UserId, typeof(long), "_adminId")
            };

            status = _db.ExecuteNonQuery("SP_Employee_ToggleDelete", param, false);
            if (!string.IsNullOrEmpty(status))
            {
                employees = this.GetEmployees(new FilterModel
                {
                    PageIndex = 1,
                    PageSize = 10,
                    SearchString = "1=1",
                    SortBy = string.Empty,
                });
            }
            return employees;
        }

        public async Task<DataSet> RegisterEmployee(Employee employee, List<AssignedClients> assignedClients, IFormFileCollection fileCollection, bool IsUpdating)
        {
            if (string.IsNullOrEmpty(employee.Email))
                throw new HiringBellException { UserMessage = "Email id is a mandatory field.", FieldName = nameof(employee.Email), FieldValue = employee.Email.ToString() };

            if (IsUpdating == true)
            {
                if (employee.EmployeeUid <= 0)
                    throw new HiringBellException { UserMessage = "Invalid EmployeeId.", FieldName = nameof(employee.EmployeeUid), FieldValue = employee.EmployeeUid.ToString() };
            }

            this.ValidateEmployeeDetails(employee);

            //TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
            //employee.DOB = TimeZoneInfo.ConvertTimeFromUtc(employee.DOB, istTimeZome);
            //employee.DateOfJoining = TimeZoneInfo.ConvertTimeFromUtc(employee.DateOfJoining, istTimeZome);
            int empId = Convert.ToInt32(employee.EmployeeUid);

            Employee employeeDetail = this.GetEmployeeByIdService(empId, null);
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
                EmpProfDetailUid = employeeDetail.EmpProfDetailUid >= 0 ? employeeDetail.EmpProfDetailUid : -1,
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

            return await Task.Run(() =>
            {
                DataSet ResultSet = null;
                DbParam[] param = new DbParam[]
                {
                    new DbParam(employee.EmployeeUid, typeof(long), "_EmployeeUid"),
                    new DbParam(employee.FirstName, typeof(string), "_FirstName"),
                    new DbParam(employee.LastName, typeof(string), "_LastName"),
                    new DbParam(employee.Mobile, typeof(string), "_Mobile"),
                    new DbParam(employee.Email, typeof(string), "_Email"),
                    new DbParam(employee.SecondaryMobile, typeof(string), "_SecondaryMobile"),
                    new DbParam(employee.FatherName, typeof(string), "_FatherName"),
                    new DbParam(employee.MotherName, typeof(string), "_MotherName"),
                    new DbParam(employee.SpouseName, typeof(string), "_SpouseName"),
                    new DbParam(employee.Gender, typeof(bool), "_Gender"),
                    new DbParam(employee.State, typeof(string), "_State"),
                    new DbParam(employee.City, typeof(string), "_City"),
                    new DbParam(employee.Pincode, typeof(int), "_Pincode"),
                    new DbParam(employee.Address, typeof(string), "_Address"),
                    new DbParam(employee.PANNo, typeof(string), "_PANNo"),
                    new DbParam(employee.AadharNo, typeof(string), "_AadharNo"),
                    new DbParam(employee.AccountNumber, typeof(string), "_AccountNumber"),
                    new DbParam(employee.BankName, typeof(string), "_BankName"),
                    new DbParam(employee.BranchName, typeof(string), "_BranchName"),
                    new DbParam(employee.IFSCCode, typeof(string), "_IFSCCode"),
                    new DbParam(employee.Domain, typeof(string), "_Domain"),
                    new DbParam(employee.Specification, typeof(string), "_Specification"),
                    new DbParam(employee.ExprienceInYear, typeof(float), "_ExprienceInYear"),
                    new DbParam(employee.LastCompanyName, typeof(string), "_LastCompanyName"),
                    new DbParam(employee.IsPermanent, typeof(bool), "_IsPermanent"),
                    new DbParam(employee.ClientUid, typeof(long), "_AllocatedClientId"),
                    new DbParam(employee.ClientName, typeof(string), "_AllocatedClientName"),
                    new DbParam(employee.ActualPackage, typeof(float), "_ActualPackage"),
                    //new DbParam(employee.DOB, typeof(float), ""),
                    //new DbParam(employee.DateOfJoining, typeof(float), ""),
                    new DbParam(employee.FinalPackage, typeof(float), "_FinalPackage"),
                    new DbParam(employee.TakeHomeByCandidate, typeof(float), "_TakeHomeByCandidate"),
                    new DbParam(employee.ReportingManagerId, typeof(long), "_ReportingManagerId"),
                    new DbParam(employee.DesignationId, typeof(int), "_DesignationId"),
                    new DbParam(employeeDetail.ProfessionalDetail_Json, typeof(string), "_ProfessionalDetail_Json"),
                    new DbParam(_currentSession.CurrentUserDetail.UserId, typeof(long), "_AdminId")
                };

                var employeeId = _db.ExecuteNonQuery("sp_Employees_InsUpdate", param, true);
                if (string.IsNullOrEmpty(employeeId))
                {
                    throw new HiringBellException("Fail to insert or update record. Contact to admin.");
                }

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
                    var dataSet = new DataSet();
                    dataSet.Tables.Add(table);
                    _db.StartTransaction(IsolationLevel.ReadUncommitted);
                    int insertedCount = _db.BatchInsert("sp_candidatefiledetail_InsUpd", dataSet, true);
                    _db.Commit();
                }

                ResultSet = this.GetManageEmployeeDetailService(currentEmployeeId);
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
    }
}
