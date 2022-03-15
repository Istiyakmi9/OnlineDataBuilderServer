using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Net;

namespace OnlineDataBuilder.Controllers
{
    [Authorize(Role.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : BaseController
    {
        private readonly IAttendanceService _attendanceService;

        public RolesController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }
        

        [HttpPost("AddUpdateRole")]
        public IResponse<ApiResponse> AddUpdateRole(List<RolesAndMenu> rolesAndMenus)
        {
            string result = null;
            return BuildResponse(result, HttpStatusCode.OK);
        }
    }
}
