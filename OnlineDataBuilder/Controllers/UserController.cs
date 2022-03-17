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
        public IResponse<ApiResponse> ManagePersonalDetail(PersonalDetail userDetail)
        {
            return null;
        }

        [HttpPost("EmploymentDetail")]
        public IResponse<ApiResponse> ManageEmploymentDetail(EmploymentDetail employmentDetail)
        {
            var result = _userService.ManageEmploymentDetail(employmentDetail);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("EducationDetail")]
        public IResponse<ApiResponse> ManageEducationDetail(List<EducationDetail> educationDetails)
        {
            var result = _userService.ManageEducationDetail(educationDetails);
            return BuildResponse(result, HttpStatusCode.OK);
        }

        [HttpPost("SkillsDetail")]
        public IResponse<ApiResponse> ManageSkillsDetail(List<SkillDetail> skillDetails)
        {
            return null;
        }

        [HttpPost("ProjectDetail")]
        public IResponse<ApiResponse> ManageProjectDetail(List<ProjectDetail> projectDetails)
        {
            return null;
        }

        [HttpPost("AccomplishmentDetail")]
        public IResponse<ApiResponse> ManageAccomplishmentDetail(AccomplishmentsDetail accomplishmentsDetail)
        {
            return null;
        }

        [HttpPost("CarrerProfileDetail")]
        public IResponse<ApiResponse> ManageCarrerProfileDetail(List<CarrerDetail> carrerDetails)
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
