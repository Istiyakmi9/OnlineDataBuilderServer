using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ModalLayer.Modal;
using Newtonsoft.Json;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Code;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Net;

namespace OnlineDataBuilder.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : BaseController
    {
        private readonly IEmployeeService _employeeService;
        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpPost]
        [Route("GetEmployees")]
        public ApiResponse GetEmployees([FromBody] FilterModel filterModel)
        {
            var Result = _employeeService.GetEmployees(filterModel);
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("GetManageEmployeeDetail/{EmployeeId}")]
        public ApiResponse GetManageEmployeeDetail(long EmployeeId)
        {
            var Result = _employeeService.GetManageEmployeeDetailService(EmployeeId);
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("UpdateEmployeeDetail/{IsUpdating}")]
        public ApiResponse UpdateEmployeeDetail([FromBody] Employee employee, bool IsUpdating)
        {
            var Result = _employeeService.UpdateEmployeeDetailService(employee, IsUpdating);
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpGet("GetEmployeeById/{EmployeeId}/{IsActive}")]
        public ApiResponse GetEmployeeById(int EmployeeId, bool IsActive)
        {
            var Result = _employeeService.GetEmployeeByIdService(EmployeeId, IsActive);
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpDelete("DeleteEmployeeById/{EmployeeId}/{IsActive}")]
        public ApiResponse DeleteEmployeeById(int EmployeeId, bool IsActive)
        {
            var Result = _employeeService.DeleteEmployeeById(EmployeeId, IsActive);
            return BuildResponse(Result, HttpStatusCode.OK);
        }
    }
}
