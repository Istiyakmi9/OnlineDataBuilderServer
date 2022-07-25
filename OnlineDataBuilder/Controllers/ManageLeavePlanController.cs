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
    }
}
