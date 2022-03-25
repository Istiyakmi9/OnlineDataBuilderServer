using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ModalLayer.Modal;
using Newtonsoft.Json;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace OnlineDataBuilder.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : BaseController
    {
        private readonly ILoginService loginService;
        private readonly IAuthenticationService _authenticationService;
        private readonly HttpContext _httpContext;

        public LoginController(ILoginService loginService, IAuthenticationService authenticationService, IHttpContextAccessor httpContext)
        {
            this.loginService = loginService;
            _authenticationService = authenticationService;
            _httpContext = httpContext.HttpContext;
        }

        [HttpGet]
        [Route("LogoutUser")]
        public IResponse<ApiResponse> LogoutUser(string Token)
        {
            bool ResultFlag = this.loginService.RemoveUserDetailService(Token);
            return BuildResponse(ResultFlag, HttpStatusCode.OK);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("AuthenticateProvider")]
        public async Task<ApiResponse> AuthenticateProvider(UserDetail authUser)
        {
            var userDetail = await this.loginService.FetchAuthenticatedProviderDetail(authUser);
            return BuildResponse(userDetail, HttpStatusCode.OK);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("AuthenticateUser")]
        public async Task<ApiResponse> AuthenticateUser(UserDetail authUser)
        {
            var userDetail = await this.loginService.FetchAuthenticatedUserDetail(authUser);
            return BuildResponse(userDetail, HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("GetUserDetail")]
        public IResponse<ApiResponse> GetUserDetail(AuthUser authUser)
        {
            var userDetail = this.loginService.GetUserDetail(authUser);
            return BuildResponse(userDetail, HttpStatusCode.OK);
        }

        [HttpPost("SignUpViaSocialMedia")]
        [AllowAnonymous]
        public async Task<ApiResponse> SignUpViaSocialMedia(UserDetail userDetail)
        {
            var result = await loginService.SignUpUser(userDetail);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("employeeregistration")]
        public async Task<ApiResponse> EmployeeRegistration()
        {
            StringValues UserInfoData = default(string);
            StringValues Clients = default(string);
            _httpContext.Request.Form.TryGetValue("employeeDetail", out UserInfoData);
            _httpContext.Request.Form.TryGetValue("allocatedClients", out Clients);
            if (UserInfoData.Count > 0)
            {
                Employee employee = JsonConvert.DeserializeObject<Employee>(UserInfoData);
                List<AssignedClients> assignedClients = JsonConvert.DeserializeObject<List<AssignedClients>>(Clients);
                IFormFileCollection files = _httpContext.Request.Form.Files;
                this.responseMessage = await this.loginService.RegisterEmployee(employee, assignedClients, files);
            }
            else
            {
                return BuildResponse(this.responseMessage, HttpStatusCode.BadRequest);
            }

            return BuildResponse(this.responseMessage);
        }
    }
}
