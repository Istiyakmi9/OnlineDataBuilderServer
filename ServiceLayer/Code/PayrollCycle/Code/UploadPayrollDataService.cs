using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using Newtonsoft.Json;
using ServiceLayer.Code.PayrollCycle.Interface;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer.Code.PayrollCycle.Code
{
    public class UploadPayrollDataService : IUploadPayrollDataService
    {
        private readonly IDb _db;
        private readonly IEmployeeService _employeeService;
        public UploadPayrollDataService(IDb db, IEmployeeService employeeService)
        {
            _db = db;
            _employeeService = employeeService;
        }

        public async Task<List<UploadedPayrollData>> ReadPayrollDataService(IFormFileCollection files)
        {
            try
            {
                var uploadedPayrollData = await ReadPayrollExcelData(files);
                await UpdateEmployeeData(uploadedPayrollData);
                return uploadedPayrollData;
            }
            catch
            {
                throw;
            }
        }

        private async Task UpdateEmployeeData(List<UploadedPayrollData> uploadedPayrolls)
        {
            int i = 0;
            int skipIndex = 0;
            int chunkSize = 2;
            while (i < uploadedPayrolls.Count)
            {
                var emps = uploadedPayrolls.Skip(skipIndex++ * chunkSize).Take(chunkSize).ToList();

                var ids = JsonConvert.SerializeObject(emps.Select(x => x.EmployeeId).ToList());
                var employees = _db.GetList<Employee>("sp_active_employees_by_ids", new { EmployeeIds = ids });

                foreach (UploadedPayrollData e in emps)
                {
                    var em = employees.Find(x => x.EmployeeUid == e.EmployeeId);
                    if (em != null)
                    {
                        if (e.CTC > 0)
                        {
                            em.CTC = e.CTC;
                            em.IsCTCChanged = true;
                            await _employeeService.UpdateEmployeeService(em, null);
                        }
                    }
                    else
                    {
                        await RegisterNewEmployee(e);                        
                    }
                }

                i++;
            }
        }

        private async Task RegisterNewEmployee(UploadedPayrollData emp)
        {
            Employee employee = new Employee
            {
                AadharNo = "NA",
                AccountNumber = "NA",
                BankName = "NA",
                BranchName = "NA",
                EmployeeUid = emp.EmployeeId,
                CreatedOn = emp.DOJ,
                Domain = "NA",
                Email = emp.Email,
                Mobile = emp.Mobile,
                EmpProfDetailUid = 0,
                ExperienceInYear = 0,
                FirstName = emp.EmployeeName,
                IFSCCode = "NA",
                LastCompanyName = "NA",
                LastName = emp.EmployeeName,
                PANNo = emp.PAN,
                SecondaryMobile = "NA",
                Specification = "NA",
                AccessLevelId = (int)RolesName.User,
                OrganizationId = 1,
                LeavePlanId = 1,
                PayrollGroupId = 0,
                SalaryGroupId = 0,
                CompanyId = 1,
                NoticePeriodId = 0,
                FatherName = "NA",
                MotherName = "NA",
                SpouseName = "NA",
                Gender = true,
                State = "NA",
                City = "NA",
                Pincode = 000000,
                Address = emp.Address,
                ExprienceInYear = 0,
                IsPermanent = true,
                ActualPackage = 0,
                FinalPackage = 0,
                TakeHomeByCandidate = 0,
                ReportingManagerId = 1,
                DesignationId = 13,
                UserTypeId = 1,
                CTC = emp.CTC,
                DateOfJoining = emp.DOJ,
                DOB = new DateTime(1990, 5, 16),
                WorkShiftId = 1,
                IsCTCChanged = true
            };

            await _employeeService.RegisterEmployeeByExcelService(employee, null);
        }

        private async Task<List<UploadedPayrollData>> ReadPayrollExcelData(IFormFileCollection files)
        {
            DataTable dataTable = null;
            List<UploadedPayrollData> uploadedPayrollList = new List<UploadedPayrollData>();
            List<string> header = new List<string>();
            List<object> excelData = new List<object>();

            using (var ms = new MemoryStream())
            {
                foreach (IFormFile file in files)
                {
                    await file.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    FileInfo fileInfo = new FileInfo(file.FileName);
                    if (fileInfo.Extension == ".xlsx" || fileInfo.Extension == ".xls")
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(ms))
                        {
                            var result = reader.AsDataSet(new ExcelDataSetConfiguration
                            {
                                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                                {
                                    UseHeaderRow = true
                                }
                            });

                            dataTable = result.Tables[0];

                            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                            uploadedPayrollList = Converter.MapToList<UploadedPayrollData>(dataTable);
                        }
                    }
                    else
                    {
                        throw HiringBellException.ThrowBadRequest("Please select a valid excel file");
                    }
                }
            }

            return uploadedPayrollList;
        }
    }
}
