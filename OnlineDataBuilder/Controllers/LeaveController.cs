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
    public class LeaveController : BaseController
    {
        private readonly ILeaveService _leaveService;
        public LeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        [HttpGet("GetLeavePlans")]
        public IResponse<ApiResponse> GetLeavePlans()
        {
            var result = _leaveService.GetLeavePlansService();
            return BuildResponse(result);
        }

        [HttpPost("AddLeavePlanType")]
        public IResponse<ApiResponse> AddLeavePlanType(LeavePlanType leavePlanType)
        {
            var result = _leaveService.AddLeavePlanTypeService(leavePlanType);
            return BuildResponse(result);
        }

        [HttpPost("AddLeavePlan")]
        public IResponse<ApiResponse> AddLeavePlans(LeavePlan leavePlan)
        {
            var result = _leaveService.AddLeavePlansService(leavePlan);
            return BuildResponse(result);
        }

        [HttpPost("LeavePlanUpdateTypes/{leavePlanId}")]
        public IResponse<ApiResponse> LeavePlanUpdateTypes([FromRoute] int leavePlanId, [FromBody] List<LeavePlanType> leavePlanTypes)
        {
            var result = _leaveService.LeavePlanUpdateTypes(leavePlanId, leavePlanTypes);
            return BuildResponse(result);
        }

        [HttpPut("UpdateLeavePlanType/{leavePlanTypeId}")]
        public IResponse<ApiResponse> UpdateLeavePlanType([FromRoute] int leavePlanTypeId, [FromBody] LeavePlanType leavePlanType)
        {
            var result = _leaveService.UpdateLeavePlanTypeService(leavePlanTypeId, leavePlanType);
            return BuildResponse(result);
        }

        [HttpPost("AddUpdateLeaveQuota")]
        public IResponse<ApiResponse> AddUpdateLeaveQuota([FromBody] LeaveDetail leaveDetail)
        {
            var result = _leaveService.AddUpdateLeaveQuotaService(leaveDetail);
            return BuildResponse(result);
        }

        [HttpGet("GetLeaveTypeDetailById/{leavePlanTypeId}")]
        public IResponse<ApiResponse> GetLeaveTypeDetailById(int leavePlanTypeId)
        {
            var result = _leaveService.GetLeaveTypeDetailByIdService(leavePlanTypeId);
            return BuildResponse(result);
        }

        [HttpGet("GetLeaveTypeFilter")]
        public IResponse<ApiResponse> GetLeaveTypeFilter()
        {
            var result = _leaveService.GetLeaveTypeFilterService();
            return BuildResponse(result);
        }
    }
}
