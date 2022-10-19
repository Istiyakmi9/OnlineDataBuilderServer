using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;

namespace OnlineDataBuilder.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class TimesheetRequestController : BaseController
    {
        private readonly ITimesheetRequestService _requestService;
        public TimesheetRequestController(ITimesheetRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpPut("ApproveTimesheet")]
        public IResponse<ApiResponse> ApproveTimesheet(List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            var result = _requestService.ApprovalTimesheetService(dailyTimesheetDetails);
            return BuildResponse(result);
        }

        [HttpPut("RejectAction")]
        public IResponse<ApiResponse> RejectAction(List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            var result = _requestService.RejectTimesheetService(dailyTimesheetDetails);
            return BuildResponse(result);
        }

        [HttpPut("ReAssigneToOtherManager")]
        public IResponse<ApiResponse> ReAssigneToOtherManager(List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            var result = _requestService.ReAssigneTimesheetService(dailyTimesheetDetails);
            return BuildResponse(result);
        }
    }
}
