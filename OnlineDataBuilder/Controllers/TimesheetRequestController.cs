using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public async Task<ApiResponse> ApproveTimesheet(List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            var result = await _requestService.ApprovalTimesheetService(dailyTimesheetDetails);
            return BuildResponse(result);
        }

        [HttpPut("RejectAction")]
        public async Task<ApiResponse> RejectAction(List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            var result = await _requestService.RejectTimesheetService(dailyTimesheetDetails);
            return BuildResponse(result);
        }

        [HttpPut("ReAssigneTimesheet")]
        public IResponse<ApiResponse> ReAssigneToOtherManager(List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            var result = _requestService.ReAssigneTimesheetService(dailyTimesheetDetails);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPut("ApproveTimesheetRequest/{filterId}")]
        public async Task<ApiResponse> ApproveTimesheetRequest([FromRoute]int filterId, [FromBody]List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            var result = await _requestService.ApprovalTimesheetService(dailyTimesheetDetails, filterId);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPut("RejectTimesheetRequest/{filterId}")]
        public async Task<ApiResponse> RejectTimesheetRequest([FromRoute] int filterId, [FromBody] List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            var result = await _requestService.RejectTimesheetService(dailyTimesheetDetails, filterId);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPut("ReAssigneTimesheetRequest/{filterId}")]
        public IResponse<ApiResponse> ReAssigneTimesheetRequest([FromRoute] int filterId, [FromBody]List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            var result = _requestService.ReAssigneTimesheetService(dailyTimesheetDetails, filterId);
            return BuildResponse(result);
        }
    }
}
