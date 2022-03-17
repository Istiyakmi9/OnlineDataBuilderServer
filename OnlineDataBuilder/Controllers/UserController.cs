using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ModalLayer.Modal;
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

        [HttpPost("UploadResume")]
        public IResponse<ApiResponse> UploadResumeFile()
        {
            StringValues RegistrationData = default(string);
            StringValues FileData = default(string);
            _httpContext.Request.Form.TryGetValue("fileDetail", out FileData);
            if (FileData.Count > 0)
            {
                Files fileDetail = JsonConvert.DeserializeObject<Files>(FileData);
                IFormFileCollection files = _httpContext.Request.Form.Files;
                var Result = this._userService.UploadResume(fileDetail, files);
                return BuildResponse(Result, HttpStatusCode.OK);
            }
            return BuildResponse("No files found", HttpStatusCode.OK);
        }
    }
}
