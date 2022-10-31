using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpPost("GetLeavePlans")]
        public IResponse<ApiResponse> GetLeavePlans(FilterModel filterModel)
        {
            var result = _leaveService.GetLeavePlansService(filterModel);
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
        public async Task<ApiResponse> LeavePlanUpdateTypes([FromRoute] int leavePlanId, [FromBody] List<LeavePlanType> leavePlanTypes)
        {
            var result = await _leaveService.LeavePlanUpdateTypes(leavePlanId, leavePlanTypes);
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

        [HttpPut("SetDefaultPlan/{leavePlanId}")]
        public IResponse<ApiResponse> SetDefaultPlan([FromRoute] int leavePlanId, [FromBody] LeavePlan leavePlan)
        {
            var result = _leaveService.SetDefaultPlanService(leavePlanId, leavePlan);
            return BuildResponse(result);
        }

        [HttpPut("LeaveRquestManagerAction/{RequestId}")]
        public IResponse<ApiResponse> LeaveRquestManagerAction([FromRoute] ItemStatus RequestId, LeaveRequestNotification approvalRequest)
        {
            var result = _leaveService.LeaveRquestManagerActionService(approvalRequest, RequestId);
            return BuildResponse(result);
        }

        [HttpPost("ApplyLeave")]
        public async Task<ApiResponse> ApplyLeave(LeaveRequestModal leaveRequestModal)
        {
            var result = await _leaveService.ApplyLeaveService(leaveRequestModal);
            return BuildResponse(result);
        }

        [HttpPost("GetAllLeavesByEmpId")]
        public async Task<ApiResponse> GetAllLeavesByEmpId(LeaveRequestModal leaveRequestModal)
        {
            var result = await _leaveService.GetEmployeeLeaveDetail(leaveRequestModal);
            return BuildResponse(result);
        }
    }
}
