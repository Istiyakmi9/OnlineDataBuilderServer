using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ModalLayer.Modal.Profile;
using Newtonsoft.Json;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
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

        [HttpPost("UpdateUserProfile/{UserTypeId}")]
        public IResponse<ApiResponse> UpdateUserProfile(ProfessionalUser professionalUser, int UserTypeId)
        {
            var result = _userService.UpdateProfile(professionalUser, UserTypeId);
            return BuildResponse(result);
        }

      
        [HttpGet("GetUserDetail/{userId}/{UserTypeId}")]
        public IResponse<ApiResponse> GetUserDetail(long userId, int UserTypeId)
        {
            var result = _userService.GetUserDetail(userId, UserTypeId);
            return BuildResponse(result);
        }

        [HttpPost("UploadProfileDetailFile/{userId}")]
        public IResponse<ApiResponse> UploadProfileDetailFile(string userId)
        {
            StringValues UserInfoData = default(string);
            _httpContext.Request.Form.TryGetValue("userInfo", out UserInfoData);
            if (UserInfoData.Count > 0)
            {
                var userInfo = JsonConvert.DeserializeObject<ProfessionalUser>(UserInfoData);
                IFormFileCollection files = _httpContext.Request.Form.Files;
                var Result = _userService.UploadUserInfo(userId, userInfo, files);
                return BuildResponse(Result, HttpStatusCode.OK);
            }
            return BuildResponse("No files found", HttpStatusCode.OK);
        }

        [HttpPost("UploadResume/{userId}")]
        public IResponse<ApiResponse> UploadResume(string userId)
        {
            StringValues UserInfoData = default(string);
            _httpContext.Request.Form.TryGetValue("userInfo", out UserInfoData);
            if (UserInfoData.Count > 0)
            {
                var userInfo = JsonConvert.DeserializeObject<ProfessionalUser>(UserInfoData);
                IFormFileCollection files = _httpContext.Request.Form.Files;
                var Result = _userService.UploadResume(userId, userInfo, files);
                return BuildResponse(Result, HttpStatusCode.OK);
            }
            return BuildResponse("No files found", HttpStatusCode.OK);
        }

        [HttpGet("GenerateResume/{userId}")]
        public IResponse<ApiResponse> GenerateResume(long userId)
        {
            var result = _userService.GenerateResume(userId);
            return BuildResponse(result);
        }
    }
}
