using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using Newtonsoft.Json;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Threading.Tasks;

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
        public IResponse<ApiResponse> AddCompanyGroup(OrganizationDetail companyGroup)
        {
            var result = _companyService.AddCompanyGroup(companyGroup);
            return BuildResponse(result);
        }

        [HttpPut("UpdateCompanyGroup/{companyId}")]
        public IResponse<ApiResponse> UpdateCompanyGroup([FromRoute] int companyId, [FromBody] OrganizationDetail companyGroup)
        {
            var result = _companyService.UpdateCompanyGroup(companyGroup, companyId);
            return BuildResponse(result);
        }

        [HttpPost("InsertUpdateOrganizationDetail")]
        public IResponse<ApiResponse> InsertUpdateOrganizationDetail()
        {
            StringValues compnyinfo = default(string);
            OrganizationDetail org = null;
            _httpContext.Request.Form.TryGetValue("OrganizationInfo", out compnyinfo);
            if (compnyinfo.Count > 0)
            {
                OrganizationDetail organizationSettings = JsonConvert.DeserializeObject<OrganizationDetail>(compnyinfo);
                IFormFileCollection files = _httpContext.Request.Form.Files;
                org = _companyService.InsertUpdateOrganizationDetailService(organizationSettings, files);
            }
            return BuildResponse(org);
        }

        [HttpPost("UpdateCompanyDetails")]
        public IResponse<ApiResponse> UpdateCompanyDetails()
        {
            StringValues compnyinfo = default(string);
            OrganizationDetail org = null;
            _httpContext.Request.Form.TryGetValue("CompanyInfo", out compnyinfo);
            if (compnyinfo.Count > 0)
            {
                OrganizationDetail organizationSettings = JsonConvert.DeserializeObject<OrganizationDetail>(compnyinfo);
                IFormFileCollection files = _httpContext.Request.Form.Files;
                org = _companyService.InsertUpdateCompanyDetailService(organizationSettings, files);
            }
            return BuildResponse(org);
        }

        [HttpGet("GetCompanyById/{companyId}")]
        public IResponse<ApiResponse> GetCompanyById(int companyId)
        {
            var result = _companyService.GetCompanyById(companyId);
            return BuildResponse(result);
        }

        [HttpGet("GetOrganizationDetail")]
        public IResponse<ApiResponse> GetOrganizationDetail()
        {
            var result = _companyService.GetOrganizationDetailService();
            return BuildResponse(result);
        }

        [HttpPost("InsertUpdateCompanyAccounts")]
        public IResponse<ApiResponse> InsertUpdateCompanyAccounts(BankDetail bankDetail)
        {
            var org = _companyService.InsertUpdateCompanyAccounts(bankDetail);
            return BuildResponse(org);
        }

        [HttpPost("GetCompanyBankDetail")]
        public IResponse<ApiResponse> GetCompanyBankDetail(FilterModel filterModel)
        {
            var bankDetail = _companyService.GetCompanyBankDetail(filterModel);
            return BuildResponse(bankDetail);
        }

        [HttpPut("UpdateSetting/{companyId}")]
        public async Task<ApiResponse> UpdateSetting([FromRoute] int companyId, [FromBody] CompanySetting companySetting)
        {
            var settingDetail = await _companyService.UpdateSettingService(companyId, companySetting);
            return BuildResponse(settingDetail);
        }

        [HttpGet("getcompanysettingdetail/{companyId}")]
        public async Task<ApiResponse> GetCompanySetting(int companyId)
        {
            var settingDetail = await _companyService.GetCompanySettingService(companyId);
            return BuildResponse(settingDetail);
        }
    }
}