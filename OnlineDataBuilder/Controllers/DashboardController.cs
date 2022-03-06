using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Net;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [Authorize(Role.Admin)]
        [HttpPost("GetEmployeeDeatils")]
        public IResponse<ApiResponse> GetEmployeeDeatils(AttendenceDetail userDetail)
        {
            var result = _dashboardService.GetEmployeeDeatils(userDetail);
            return BuildResponse(result, HttpStatusCode.OK);
        }
    }
}
