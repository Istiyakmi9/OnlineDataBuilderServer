using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ModalLayer.Modal;
using ModalLayer.Modal.Profile;
using Newtonsoft.Json;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Net;

namespace OnlineDataBuilder.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : BaseController
    {
        private readonly HttpContext _httpContext;
        private readonly IUserService _userService;
        public UserController(IHttpContextAccessor httpContext, IUserService userService)
        {
            _httpContext = httpContext.HttpContext;
            _userService = userService;
        }

        [HttpPost("PersonalDetail")]
        public IResponse<ApiResponse> ManageUserDetail(PersonalDetail userDetail)
        {
            return null;
        }

        [HttpPost("CreateUser")]
        public IResponse<ApiResponse> CreateUser(UserDetail userDetail)
        {
            return null;
        }

        [HttpPost("UploadProfileDetailFile/{userId}")]
        public IResponse<ApiResponse> UploadProfileDetailFile(string userId)
        {
            StringValues UserInfoData = default(string);
            _httpContext.Request.Form.TryGetValue("userInfo", out UserInfoData);
            if (UserInfoData.Count > 0)
            {
                var userInfo = JsonConvert.DeserializeObject<UserInfo>(UserInfoData);
                IFormFileCollection files = _httpContext.Request.Form.Files;
                var Result = this._userService.UploadUserInfo(userId, userInfo, files);
                return BuildResponse(Result, HttpStatusCode.OK);
            }
            return BuildResponse("No files found", HttpStatusCode.OK);
        }
    }
}
