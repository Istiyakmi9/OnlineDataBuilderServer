using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
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
    public class DeclarationService : IDeclarationService
    {
        private readonly IDb _db;
        private readonly IFileService _fileService;
        private readonly FileLocationDetail _fileLocationDetail;
        private readonly CurrentSession _currentSession;
        private readonly Dictionary<string, List<string>> _sections;
        public DeclarationService(IDb db, IFileService fileService, FileLocationDetail fileLocationDetail, CurrentSession currentSession, IOptions<Dictionary<string, List<string>>> options)
        {
            _db = db;
            _fileService = fileService;
            _fileLocationDetail = fileLocationDetail;
            _currentSession = currentSession;
            _sections = options.Value;
        }

        public EmployeeDeclaration GetDeclarationByEmployee(long EmployeeId)
        {
            throw new NotImplementedException();
        }

        public List<SalaryComponents> UpdateDeclarationDetail(long EmployeeDeclarationId, EmployeeDeclaration employeeDeclaration, IFormFileCollection FileCollection, List<Files> files)
        {
            EmployeeDeclaration declaration = this.GetDeclarationById(EmployeeDeclarationId);
            List<SalaryComponents> salaryComponents = new List<SalaryComponents>();
            if (declaration != null)
            {
                salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(declaration.DeclarationDetail);
                SalaryComponents salaryComponent = salaryComponents.Find(x => x.ComponentId == employeeDeclaration.ComponentId);
                if (salaryComponent == null)
                    throw new HiringBellException("Requested component not found. Please contact to admin.");
                salaryComponent.DeclaredValue = employeeDeclaration.DeclaredValue;

                declaration.DeclarationDetail = JsonConvert.SerializeObject(salaryComponents);
            }
            else
            {
                throw new HiringBellException("Requested component not found. Please contact to admin.");
            }

            string declarationDoc = String.Empty;
            if (FileCollection.Count > 0)
            {
                var email = employeeDeclaration.Email.Replace("@", "_").Replace(".", "_");
                declarationDoc = Path.Combine(
                    _fileLocationDetail.UserFolder,
                    email,
                    "declarated_documents"
                );
                var file = FileCollection.Select(x => new Files
                {
                    FileUid = employeeDeclaration.EmployeeId,
                    FileName = x.Name,
                    Email = "",
                    FileExtension = string.Empty
                }).ToList<Files>();
                _fileService.SaveFileToLocation(declarationDoc, file, FileCollection);

                int i = 0;
                Parallel.For(i, files.Count, x => files.ElementAt(i).FileName = file.ElementAt(i).FileName + "_"+ i);

                var fileInfo = (from n in files
                                select new
                                {
                                    FileId = n.FileUid,
                                    FileOwnerId = (employeeDeclaration.EmployeeId),
                                    FilePath = declarationDoc,
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

            var result = _db.Execute<EmployeeDeclaration>("sp_employee_declaration_insupd", new
            {
                EmployeeDeclarationId = declaration.EmployeeDeclarationId,
                EmployeeId = declaration.EmployeeId,
                DocumentPath = declarationDoc,
                DeclarationDetail = declaration.DeclarationDetail
            }, true);

            if (!ApplicationConstants.IsExecuted(result))
            {
                File.Delete(declarationDoc);
            }

            return salaryComponents;
        }

        public EmployeeDeclaration GetDeclarationById(long EmployeeDeclarationId)
        {
            var employeeDeclaration = _db.Get<EmployeeDeclaration>("sp_employee_declaration_get_byId", new { EmployeeDeclarationId = EmployeeDeclarationId });
            return employeeDeclaration;
        }

        public EmployeeDeclaration GetEmployeeDeclarationDetailById(long EmployeeId)
        {
            EmployeeDeclaration employeeDeclaration = _db.Get<EmployeeDeclaration>("sp_employee_declaration_get_byEmployeeId", new { EmployeeId = EmployeeId });
            employeeDeclaration.SalaryComponentItems = JsonConvert.DeserializeObject<List<SalaryComponents>>(employeeDeclaration.DeclarationDetail);
            employeeDeclaration.DeclarationDetail = null;
            employeeDeclaration.Sections = _sections;
            return employeeDeclaration;
        }
    }
}