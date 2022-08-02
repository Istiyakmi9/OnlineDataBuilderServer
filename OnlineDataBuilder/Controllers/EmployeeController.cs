using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using Newtonsoft.Json;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace OnlineDataBuilder.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : BaseController
    {
        private readonly IEmployeeService _employeeService;
        private readonly HttpContext _httpContext;

        public EmployeeController(IEmployeeService employeeService, IHttpContextAccessor httpContext)
        {
            _employeeService = employeeService;
            _httpContext = httpContext.HttpContext;
        }

        [HttpPost]
        [Route("GetEmployees")]
        public ApiResponse GetEmployees([FromBody] FilterModel filterModel)
        {
            var Result = _employeeService.GetEmployees(filterModel);
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpGet]
        // [Authorize(Roles = Role.Employee)]
        [Route("GetManageEmployeeDetail/{EmployeeId}")]
        public ApiResponse GetManageEmployeeDetail(long EmployeeId)
        {
            var Result = _employeeService.GetEmployeeLeaveDetailService(EmployeeId);
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        [Route("GetAllManageEmployeeDetail/{EmployeeId}")]
        public ApiResponse GetAllManageEmployeeDetail(long EmployeeId)
        {
            var Result = _employeeService.GetManageEmployeeDetailService(EmployeeId);
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("GetManageClient/{EmployeeId}")]
        public ApiResponse GetManageClient(long EmployeeId)
        {
            var Result = _employeeService.GetManageClientService(EmployeeId);
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
        public ApiResponse GetEmployeeById(int EmployeeId, int IsActive)
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

        [HttpPost("employeeregistration/{IsUpdating}")]
        public async Task<ApiResponse> EmployeeRegistration(bool IsUpdating)
        {
            StringValues UserInfoData = default(string);
            StringValues Clients = default(string);
            _httpContext.Request.Form.TryGetValue("employeeDetail", out UserInfoData);
            _httpContext.Request.Form.TryGetValue("allocatedClients", out Clients);
            if (UserInfoData.Count > 0)
            {
                Employee employee = JsonConvert.DeserializeObject<Employee>(UserInfoData);
                List<AssignedClients> assignedClients = JsonConvert.DeserializeObject<List<AssignedClients>>(Clients);
                IFormFileCollection files = _httpContext.Request.Form.Files;
                var resetSet = await _employeeService.RegisterEmployee(employee, assignedClients, files, IsUpdating);
                return BuildResponse(resetSet);
            }
            else
            {
                return BuildResponse(this.responseMessage, HttpStatusCode.BadRequest);
            }
        }

        
    }
}
