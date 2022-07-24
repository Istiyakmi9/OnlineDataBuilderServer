using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal.Leaves;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;

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

        [HttpPut("UpdateLeavePlanType/{leavePlanId}")]
        public IResponse<ApiResponse> UpdateLeavePlanType([FromRoute] int leavePlanId, [FromBody] LeavePlanType leavePlanType)
        {
            var result = _leaveService.UpdateLeavePlanTypeService(leavePlanId, leavePlanType);
            return BuildResponse(result);
        }

        [HttpPost("AddUpdateLeaveQuota")]
        public IResponse<ApiResponse> AddUpdateLeaveQuota([FromBody] LeaveDetail leaveDetail)
        {
            var result = _leaveService.AddUpdateLeaveQuotaService(leaveDetail);
            return BuildResponse(result);
        }

        [HttpGet("GetLeaveTypeByPlanId/{leavePlanId}")]
        public IResponse<ApiResponse> GetLeaveTypeByPlanId(int leavePlanId)
        {
            var result = _leaveService.GetLeaveTypeDetailByPlan(leavePlanId);
            return BuildResponse(result);
        }

        [HttpGet("GetLeaveTypeDetailById/{leavePlanTypeId}")]
        public IResponse<ApiResponse> GetLeaveTypeDetailById(int leavePlanTypeId)
        {
            var result = _leaveService.GetLeaveTypeDetailByPlan(leavePlanTypeId);
            return BuildResponse(result);
        }
    }
}
