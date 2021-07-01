using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Net;

namespace OnlineDataBuilder.Controllers
{
    [Authorize]
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
        [Route("AuthenticateUser")]
        public IResponse<ApiResponse> AuthenticateUser(AuthUser authUser)
        {
            var userDetail = this.loginService.GetLoginUserObject(authUser);
            return BuildResponse(userDetail, HttpStatusCode.OK);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GenerateNewToken/{UserId}")]
        public IResponse<ApiResponse> GenerateNewToken(string UserId = null)
        {
            var userDetail = _authenticationService.RenewAndGenerateNewToken();
            return BuildResponse(userDetail, HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("GetUserDetail")]
        public IResponse<ApiResponse> GetUserDetail(AuthUser authUser)
        {
            var userDetail = this.loginService.GetUserDetail(authUser);
            return BuildResponse(userDetail, HttpStatusCode.OK);
        }
    }
}
