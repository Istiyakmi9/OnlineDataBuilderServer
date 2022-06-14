using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal.Accounts;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceLayer.Code
{
    public class DeclarationService : IDeclarationService
    {
        private readonly IDb _db;
        public DeclarationService(IDb db)
        {
            _db = db;
        }

        public EmployeeDeclaration GetDeclarationByEmployee(long EmployeeId)
        {
            throw new NotImplementedException();
        }

        public string UpdateDeclarationDetail(long EmployeeDeclarationId, EmployeeDeclaration employeeDeclaration)
        {
            EmployeeDeclaration declaration = this.GetDeclarationById(EmployeeDeclarationId);
            if (declaration != null)
            {
                List<SalaryComponents> salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(declaration.DeclarationDetail);
                SalaryComponents salaryComponent = salaryComponents.Find(x => x.ComponentId == employeeDeclaration.ComponentId);
                // change value

                declaration.DeclarationDetail = JsonConvert.SerializeObject(salaryComponents);
            }

            // save file

            var result = _db.Execute<EmployeeDeclaration>("sp_employee_declaration_insupd", new
            {
                EmployeeDeclarationId = declaration.EmployeeDeclarationId,
                EmployeeId = declaration.EmployeeId,
                DocumentPath = declaration.DocumentPath,
                DeclarationDetail = declaration.DeclarationDetail
            }, true);

            if(!ApplicationConstants.IsExecuted(result))
            {
                // delete file
            }

            return result;
        }

        public EmployeeDeclaration GetDeclarationById(long EmployeeDeclarationId)
        {
            var employeeDeclaration = _db.Get<EmployeeDeclaration>("sp_employee_declaration_get_byId", new { EmployeeDeclarationId = EmployeeDeclarationId });
            return employeeDeclaration;
        }

        private EmployeeDeclaration GetEmployeeDeclarationDetailById(long EmployeeId)
        {
            var employeeDeclaration = _db.Get<EmployeeDeclaration>("sp_employee_declaration_get_byEmployeeId", new { EmployeeId = EmployeeId });
            return employeeDeclaration;
        }
    }
}
