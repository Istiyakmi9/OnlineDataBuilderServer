using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using Newtonsoft.Json;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : BaseController
    {
        private readonly ICompanyService _companyService;
        private readonly HttpContext _httpContext;

        public CompanyController(ICompanyService companyService, IHttpContextAccessor httpContext)
        {
            _companyService = companyService;
            _httpContext = httpContext.HttpContext;
        }

        [HttpGet("GetAllCompany")]
        public IResponse<ApiResponse> GetAllCompany()
        {
            var result = _companyService.GetAllCompany();
            return BuildResponse(result);
        }

        [HttpPost("AddCompanyGroup")]
        public IResponse<ApiResponse> AddCompanyGroup(OrganizationSettings companyGroup)
        {
            var result = _companyService.AddCompanyGroup(companyGroup);
            return BuildResponse(result);
        }

        [HttpPost("UpdateCompanyDetails")]
        public IResponse<ApiResponse> UpdateCompanyDetails()
        {
            StringValues compnyinfo = default(string);
            OrganizationSettings org = null;
            _httpContext.Request.Form.TryGetValue("CompanyInfo", out compnyinfo);
            if (compnyinfo.Count > 0)
            {
                OrganizationSettings organizationSettings = JsonConvert.DeserializeObject<OrganizationSettings>(compnyinfo);
                IFormFileCollection files = _httpContext.Request.Form.Files;
                org = _companyService.UpdateCompanyDetails(organizationSettings, files);
            }
            return BuildResponse(org);
        }

        [HttpGet("GetCompanyById/{companyId}")]
        public IResponse<ApiResponse> GetCompanyById(int companyId)
        {
            var result = _companyService.GetCompanyById(companyId);
            return BuildResponse(result);
        }
    }
}
