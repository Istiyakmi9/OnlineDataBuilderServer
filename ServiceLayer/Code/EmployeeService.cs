﻿using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Code
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IDb _db;
        private readonly CommonFilterService _commonFilterService;
        public EmployeeService(IDb db, CommonFilterService commonFilterService)
        {
            _db = db;
            _commonFilterService = commonFilterService;
        }
        public List<Employee> GetEmployees(FilterModel filterModel)
        {
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
            if (resultset.Tables.Count == 3)
            {
                resultset.Tables[0].TableName = "Employee";
                resultset.Tables[1].TableName = "Clients";
                resultset.Tables[2].TableName = "AllocatedClients";
            }
            return resultset;
        }

        public DataSet UpdateEmployeeDetailService(Employee employee, bool IsUpdating)
        {
            DbParam[] param = new DbParam[]
            {
                new DbParam(employee.EmployeeUid, typeof(long), "_employeeUid"),
                new DbParam(employee.AllocatedClientId, typeof(long), "_clientUid"),
                new DbParam(employee.FinalPackage, typeof(float), "_finalPackage"),
                new DbParam(employee.ActualPackage, typeof(float), "_actualPackage"),
                new DbParam(employee.TakeHomeByCandidate, typeof(float), "_takeHome"),
                new DbParam(employee.IsPermanent, typeof(bool), "_isPermanent"),
                new DbParam(employee.IsActive, typeof(bool), "_isActive"),
                new DbParam(IsUpdating, typeof(bool), "_isUpdate"),
            };
            var resultset = _db.GetDataset("SP_Employees_AddUpdateRemoteClient", param);
            return resultset;
        }

        public Employee GetEmployeeByIdService(int EmployeeId, bool IsActive)
        {
            Employee employee = default;
            DbParam[] param = new DbParam[]
            {
                new DbParam(EmployeeId, typeof(int), "_EmployeeId"),
                new DbParam(IsActive, typeof(bool), "_IsActive")
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
    }
}