using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
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

        public LoginController(ILoginService loginService, IAuthenticationService authenticationService)
        {
            this.loginService = loginService;
            _authenticationService = authenticationService;
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
    }
}
