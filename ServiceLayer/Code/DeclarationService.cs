using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ServiceLayer.Code
{
    public class DeclarationService : IDeclarationService
    {
        private readonly IDb _db;
        private readonly IFileService _fileService;
        private readonly FileLocationDetail _fileLocationDetail;
        private readonly CurrentSession _currentSession;
        public DeclarationService(IDb db, IFileService fileService, FileLocationDetail fileLocationDetail, CurrentSession currentSession)
        {
            _db = db;
            _fileService = fileService;
            _fileLocationDetail = fileLocationDetail;
            _currentSession = currentSession;
        }

        public EmployeeDeclaration GetDeclarationByEmployee(long EmployeeId)
        {
            throw new NotImplementedException();
        }

        public string UpdateDeclarationDetail(long EmployeeDeclarationId, EmployeeDeclaration employeeDeclaration, IFormFileCollection FileCollection, List<Files> files)
        {
            EmployeeDeclaration declaration = this.GetDeclarationById(EmployeeDeclarationId);
            if (declaration != null)
            {
                List<SalaryComponents> salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(declaration.DeclarationDetail);
                SalaryComponents salaryComponent = salaryComponents.Find(x => x.ComponentId == employeeDeclaration.ComponentId);
                salaryComponent.DeclaredValue = employeeDeclaration.DeclaredValue;

                declaration.DeclarationDetail = JsonConvert.SerializeObject(salaryComponents);
            }

            string declarationDoc = String.Empty;
                if (FileCollection[0].Name == "declaration")
                {
                declarationDoc = Path.Combine(_fileLocationDetail.RootPath, _fileLocationDetail.LogoPath, FileCollection[0].Name);
                }
            

            if (File.Exists(declarationDoc))
            {
                File.Delete(declarationDoc);
            }
            else
            {
                FileDetail fileDetailWSig = new FileDetail();
                fileDetailWSig.DiskFilePath = Path.Combine(_fileLocationDetail.RootPath, declarationDoc);
                declaration.DocumentPath = fileDetailWSig.DiskFilePath;
            }



            if (FileCollection.Count > 0)
            {
                var file = FileCollection.Select(x => new Files
                {
                    FileUid = employeeDeclaration.EmployeeId,
                    FileName = x.Name,
                    Email = "",
                    FileExtension = string.Empty
                }).ToList<Files>();
                _fileService.SaveFile(_fileLocationDetail.LogoPath, file, FileCollection, (employeeDeclaration.EmployeeId).ToString());

                var fileInfo = (from n in files
                                select new
                                {
                                    FileId = n.FileUid,
                                    FileOwnerId = (employeeDeclaration.EmployeeId),
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

            var result = _db.Execute<EmployeeDeclaration>("sp_employee_declaration_insupd", new
            {
                EmployeeDeclarationId = declaration.EmployeeDeclarationId,
                EmployeeId = declaration.EmployeeId,
                DocumentPath = declaration.DocumentPath,
                DeclarationDetail = declaration.DeclarationDetail
            }, true);

            if(!ApplicationConstants.IsExecuted(result))
            {
                File.Delete(declarationDoc);
            }

            return result;
        }

        public EmployeeDeclaration GetDeclarationById(long EmployeeDeclarationId)
        {
            var employeeDeclaration = _db.Get<EmployeeDeclaration>("sp_employee_declaration_get_byId", new { EmployeeDeclarationId = EmployeeDeclarationId });
            return employeeDeclaration;
        }

        public EmployeeDeclaration GetEmployeeDeclarationDetailById(long EmployeeId)
        {
            EmployeeDeclaration employeeDeclaration = _db.Get<EmployeeDeclaration>("sp_employee_declaration_get_byEmployeeId", new { EmployeeId = EmployeeId });
            //var salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(employeeDeclaration.DeclarationDetail);
            return employeeDeclaration;
        }
    }
}