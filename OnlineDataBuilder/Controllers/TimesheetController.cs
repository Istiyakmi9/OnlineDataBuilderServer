using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Code;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Net;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class TimesheetController : BaseController
    {
        private readonly ITimesheetService _timesheetService;
        public TimesheetController(ITimesheetService timesheetService)
        {
            _timesheetService = timesheetService;
        }

        [HttpPost("GetTimesheetByUserId")]
        public IResponse<ApiResponse> GetTimesheetByUserId(TimesheetDetail attendenceDetail)
        {
            var result = _timesheetService.GetTimesheetByUserIdService(attendenceDetail);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("InsertUpdateTimesheet")]
        public IResponse<ApiResponse> InsertUpdateTimesheet(List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            var result = _timesheetService.InsertUpdateTimesheet(dailyTimesheetDetails);
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
    }
}
