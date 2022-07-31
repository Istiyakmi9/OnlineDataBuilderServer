using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal.Leaves;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;

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

        [HttpPut("UpdateLeaveApproval/{leavePlanTypeId}")]
        public IResponse<ApiResponse> UpdateLeaveApproval([FromRoute] int leavePlanTypeId, [FromBody] LeaveApproval leaveApproval)
        {
            var result = _manageLeavePlanService.UpdateLeaveApprovalService(leavePlanTypeId, leaveApproval);
            return BuildResponse(result);
        }

        [HttpPut("UpdateYearEndProcessing/{leavePlanTypeId}")]
        public IResponse<ApiResponse> UpdateYearEndProcessing([FromRoute] int leavePlanTypeId, [FromBody] LeaveEndYearProcessing leaveEndYearProcessing)
        {
            var result = _manageLeavePlanService.UpdateYearEndProcessingService(leavePlanTypeId, leaveEndYearProcessing);
            return BuildResponse(result);
        }

        [HttpPut("AddUpdateEmpLeavePlan/{leavePlanId}")]
        public IResponse<ApiResponse> AddUpdateEmpLeavePlan([FromRoute] int leavePlanId, [FromBody] List<EmpLeavePlanMapping> empLeavePlanMapping)
        {
            var result = _manageLeavePlanService.AddUpdateEmpLeavePlanService(leavePlanId, empLeavePlanMapping);
            return BuildResponse(result);
        }

        [HttpGet("GetEmpMappingByLeavePlanId/{leavePlanId}")]
        public IResponse<ApiResponse> GetEmpMappingByLeavePlanId([FromRoute] int leavePlanId)
        {
            var result = _manageLeavePlanService.GetEmpMappingByLeavePlanIdService(leavePlanId);
            return BuildResponse(result);
        }
    }
}
