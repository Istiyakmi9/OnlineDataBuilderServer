using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal.Leaves;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageLeavePlanController : BaseController
    {
        private readonly IManageLeavePlanService _manageLeavePlanService;

        public ManageLeavePlanController(IManageLeavePlanService manageLeavePlanService)
        {
            _manageLeavePlanService = manageLeavePlanService;
        }

        [HttpGet("GetLeavePlanTypeConfiguration/{leavePlanTypeId}")]
        public IResponse<ApiResponse> GetLeavePlanTypeConfigurationDetail(int leavePlanTypeId)
        {
            var result = _manageLeavePlanService.GetLeaveConfigurationDetail(leavePlanTypeId);
            return BuildResponse(result);
        }

        [HttpPut("UpdateLeaveDetail/{leavePlanTypeId}")]
        public IResponse<ApiResponse> UpdateLeaveDetail([FromRoute] int leavePlanTypeId, [FromBody] LeaveDetail leaveDetail)
        {
            var result = _manageLeavePlanService.UpdateLeaveDetail(leavePlanTypeId, leaveDetail);
            return BuildResponse(result);
        }

        [HttpPut("UpdateLeaveAccrual/{leavePlanTypeId}")]
        public IResponse<ApiResponse> UpdateLeaveAccrual([FromRoute] int leavePlanTypeId, [FromBody] LeaveAccrual leaveAccrual)
        {
            var result = _manageLeavePlanService.UpdateLeaveAccrualService(leavePlanTypeId, leaveAccrual);
            return BuildResponse(result);
        }

        [HttpPut("UpdateApplyForLeave/{leavePlanTypeId}")]
        public IResponse<ApiResponse> UpdateApplyForLeave([FromRoute] int leavePlanTypeId, [FromBody] LeaveApplyDetail leaveApplyDetail)
        {
            var result = _manageLeavePlanService.UpdateApplyForLeaveService(leavePlanTypeId, leaveApplyDetail);
            return BuildResponse(result);
        }

        [HttpPut("UpdateLeaveRestriction/{leavePlanTypeId}")]
        public IResponse<ApiResponse> UpdateLeaveRestriction([FromRoute] int leavePlanTypeId, [FromBody] LeavePlanRestriction leavePlanRestriction)
        {
            var result = _manageLeavePlanService.UpdateLeaveRestrictionService(leavePlanTypeId, leavePlanRestriction);
            return BuildResponse(result);
        }

        [HttpPut("UpdateHolidayNWeekOffPlan/{leavePlanTypeId}")]
        public IResponse<ApiResponse> UpdateHolidayNWeekOffPlan([FromRoute] int leavePlanTypeId, [FromBody] LeaveHolidaysAndWeekoff leaveHolidaysAndWeekoff)
        {
            var result = _manageLeavePlanService.UpdateHolidayNWeekOffPlanService(leavePlanTypeId, leaveHolidaysAndWeekoff);
            return BuildResponse(result);
        }
    }
}
