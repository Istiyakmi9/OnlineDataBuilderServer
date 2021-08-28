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

        [HttpPost("GetEmployeeById/{EmployeeId}")]
        public ApiResponse GetEmployeeById(int EmployeeId)
        {
            var Result = _employeeService.GetEmployeeByIdService(EmployeeId);
            return BuildResponse(Result, HttpStatusCode.OK);
        }
    }
}
