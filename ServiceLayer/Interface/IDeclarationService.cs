﻿using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface IDeclarationService
    {
        EmployeeDeclaration GetDeclarationByEmployee(long EmployeeId);
        EmployeeDeclaration GetDeclarationById(long EmployeeDeclarationId);
        EmployeeDeclaration UpdateDeclarationDetail(long EmployeeDeclarationId, EmployeeDeclaration employeeDeclaration, IFormFileCollection FileCollection, List<Files> fileDetail);
        EmployeeDeclaration GetEmployeeDeclarationDetailById(long EmployeeId);
    }
}
