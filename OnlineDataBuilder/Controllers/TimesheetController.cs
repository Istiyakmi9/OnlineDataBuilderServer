using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ModalLayer.Modal;
using Newtonsoft.Json;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Code;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class TimesheetController : BaseController
    {
        private readonly ITimesheetService _timesheetService;
        private readonly HttpContext _httpContext;
        public TimesheetController(ITimesheetService timesheetService, IHttpContextAccessor httpContext)
        {
            _timesheetService = timesheetService;
            _httpContext = httpContext.HttpContext;
        }

        [HttpPost("GetTimesheetByUserId")]
        public IResponse<ApiResponse> GetTimesheetByUserId(TimesheetDetail attendenceDetail)
        {
            var result = _timesheetService.GetTimesheetByUserIdService(attendenceDetail);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("GetWeekTimesheetData")]
        public async Task<ApiResponse> GetWeekTimesheetData(TimesheetDetail attendenceDetail)
        {
            var result = await _timesheetService.GetWeekTimesheetDataService(attendenceDetail);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("InsertUpdateTimesheet")]
        public async Task<ApiResponse> InsertUpdateTimesheet(List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            var result = await _timesheetService.InsertUpdateTimesheet(dailyTimesheetDetails);
            return BuildResponse(result, HttpStatusCode.OK);
        }


        [HttpGet("GetPendingTimesheetById/{EmployeeId}/{clientId}")]
        public IResponse<ApiResponse> GetPendingTimesheetById(long employeeId, long clientId)
        {
            var result = _timesheetService.GetPendingTimesheetByIdService(employeeId, clientId);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("GetEmployeeTimeSheet")]
        public IResponse<ApiResponse> GetEmployeeTimeSheet(TimesheetDetail timesheetDetail)
        {
            var result = _timesheetService.GetEmployeeTimeSheetService(timesheetDetail);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        //[Authorize(Roles = Role.Admin)]
        //[HttpPost("UpdateTimesheet")]
        //public IResponse<ApiResponse> UpdateTimesheet()
        //{
        //    _httpContext.Request.Form.TryGetValue("comment", out StringValues comment);
        //    _httpContext.Request.Form.TryGetValue("dailyTimesheetDetail", out StringValues timeSheetDetail);
        //    _httpContext.Request.Form.TryGetValue("timesheet", out StringValues timesheet);
        //    string Comment = JsonConvert.DeserializeObject<string>(comment);
            
        //    List<DailyTimesheetDetail> dailyTimesheetDetails = JsonConvert.DeserializeObject<List<DailyTimesheetDetail>>(timeSheetDetail);
        //    TimesheetDetail timesheetDetail = JsonConvert.DeserializeObject<TimesheetDetail>(timesheet);
            
        //    var result = _timesheetService.UpdateTimesheetService(dailyTimesheetDetails, timesheetDetail, Comment);
        //    return BuildResponse(result, HttpStatusCode.OK);
        //}

        [HttpPost]
        [Route("EditEmployeeBillDetail")]
        public IResponse<ApiResponse> EditEmployeeBillDetail([FromBody] GenerateBillFileDetail fileDetail)
        {
            var result = _timesheetService.EditEmployeeBillDetailService(fileDetail);
            return BuildResponse(result, System.Net.HttpStatusCode.OK);
        }
    }
}
