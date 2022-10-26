using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using Newtonsoft.Json;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeclarationController : BaseController
    {
        private readonly IDeclarationService _declarationService;
        private readonly HttpContext _httpContext;
        public DeclarationController(IDeclarationService declarationService, IHttpContextAccessor httpContext)
        {
            _declarationService = declarationService;
            _httpContext = httpContext.HttpContext;
        }

        [HttpGet("GetEmployeeDeclarationDetailById/{EmployeeId}")]
        public IResponse<ApiResponse> GetEmployeeDeclarationDetailById(long EmployeeId)
        {
            var result = _declarationService.GetEmployeeDeclarationDetailById(EmployeeId);
            return BuildResponse(result);
        }

        [HttpPost("UpdateDeclarationDetail/{EmployeeDeclarationId}")]
        public async Task<ApiResponse> UpdateDeclarationDetail([FromRoute]long EmployeeDeclarationId)
        {
            StringValues declaration = default(string);
            _httpContext.Request.Form.TryGetValue("declaration", out declaration);
            _httpContext.Request.Form.TryGetValue("fileDetail", out StringValues FileData);
            if (declaration.Count > 0)
            {
                var DeclarationDetail = JsonConvert.DeserializeObject<EmployeeDeclaration>(declaration);
                List<Files> files = JsonConvert.DeserializeObject<List<Files>>(FileData);
                IFormFileCollection fileDetail = _httpContext.Request.Form.Files;
                var result = await _declarationService.UpdateDeclarationDetail(EmployeeDeclarationId, DeclarationDetail, fileDetail, files);
                return BuildResponse(result, HttpStatusCode.OK);
            }
            return BuildResponse("No files found", HttpStatusCode.OK);
        }

        [HttpPost("HousingPropertyDeclaration/{EmployeeDeclarationId}")]
        public async Task<ApiResponse> HousingPropertyDeclaration([FromRoute] long EmployeeDeclarationId)
        {
            StringValues declaration = default(string);
            _httpContext.Request.Form.TryGetValue("declaration", out declaration);
            _httpContext.Request.Form.TryGetValue("fileDetail", out StringValues FileData);
            if (declaration.Count > 0)
            {
                var DeclarationDetail = JsonConvert.DeserializeObject<HousingDeclartion>(declaration);
                List<Files> files = JsonConvert.DeserializeObject<List<Files>>(FileData);
                IFormFileCollection fileDetail = _httpContext.Request.Form.Files;
                var result = await _declarationService.HousingPropertyDeclarationService(EmployeeDeclarationId, DeclarationDetail, fileDetail, files);
                return BuildResponse(result, HttpStatusCode.OK);
            }
            return BuildResponse("No files found", HttpStatusCode.OK);
        }
    }
}
