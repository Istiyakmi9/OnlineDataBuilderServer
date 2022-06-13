using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using Newtonsoft.Json;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;

namespace OnlineDataBuilder.Controllers
{
    [Authorize(Roles = Role.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : BaseController
    {
        private readonly ISettingService _settingService;
        private readonly HttpContext _httpContext;
        public SettingsController(ISettingService settingService, IHttpContextAccessor httpContext)
        {
            _settingService = settingService;
            _httpContext = httpContext.HttpContext;
        }

        [HttpGet("GetSalaryComponents")]
        public IResponse<ApiResponse> GetSalaryComponents()
        {
            var result = _settingService.GetSalaryComponentService();
            return BuildResponse(result);
        }

        [HttpGet("GetOrganizationInfo")]
        public IResponse<ApiResponse> GetOrganizationInfo()
        {
            var result = _settingService.GetOrganizationInfo();
            return BuildResponse(result);
        }

        [HttpGet("GetOrganizationAccountsInfo/{OrganizationId}")]
        public IResponse<ApiResponse> GetOrganizationBankDetailInfo(int organizationId)
        {
            var result = _settingService.GetOrganizationBankDetailInfoService(organizationId);
            return BuildResponse(result);
        }

        [HttpPost("InsertUpdateCompanyDetail")]
        public IResponse<ApiResponse> InsertUpdateCompanyDetail()
        {
            StringValues compnyinfo = default(string);
            OrganizationSettings org = null;
            _httpContext.Request.Form.TryGetValue("CompanyInfo", out compnyinfo);
            if (compnyinfo.Count > 0)
            {
                OrganizationSettings organizationSettings = JsonConvert.DeserializeObject<OrganizationSettings>(compnyinfo);
                IFormFileCollection files = _httpContext.Request.Form.Files;
                org = _settingService.InsertUpdateCompanyDetailService(organizationSettings, files);
            }
            return BuildResponse(org);
        }

        [HttpPut("UpdateCompanyAccounts")]
        public IResponse<ApiResponse> UpdateCompanyAccounts(BankDetail bankDetail)
        {
            BankDetail org = _settingService.UpdateCompanyAccountsService(bankDetail);
            return BuildResponse(org);
        }

        [HttpPost("PfEsiSetting")]
        public IResponse<ApiResponse> PfEsiSetting()
        {
            StringValues pfSetting = default(string);
            StringValues esiSetting = default(string);
            StringValues pfesiSetting = default(string);
            _httpContext.Request.Form.TryGetValue("PFSetting", out pfSetting);
            _httpContext.Request.Form.TryGetValue("ESISetting", out esiSetting);
            _httpContext.Request.Form.TryGetValue("PFESISetting", out pfesiSetting);

            if (pfSetting.Count > 0 && esiSetting.Count > 0 && pfesiSetting.Count > 0)
            {
                var PfSetting = JsonConvert.DeserializeObject<SalaryComponents>(pfSetting);
                var EsiSetting = JsonConvert.DeserializeObject<SalaryComponents>(esiSetting);
                var PfesiSetting = JsonConvert.DeserializeObject<PfEsiSetting>(pfesiSetting);
                var result = _settingService.PfEsiSetting(PfSetting, EsiSetting, PfesiSetting);
                return BuildResponse(result);
            }
            return BuildResponse(null);
        }

        [HttpPost("InsertUpdatePayrollSetting")]
        public IResponse<ApiResponse> InsertUpdatePayrollSetting(Payroll payroll)
        {
            var result = _settingService.InsertUpdatePayrollSetting(payroll);
            return BuildResponse(result);
        }

        [HttpGet("GetPayrollSetting/{companyId}")]
        public IResponse<ApiResponse> GetPayrollSetting(int companyId)
        {
            var result = _settingService.GetPayrollSetting(companyId);
            return BuildResponse(result);
        }

        [HttpPost("InsertUpdateSalaryStructure")]
        public IResponse<ApiResponse> InsertUpdateSalaryStructure(List<SalaryStructure> salaryStructure)
        {
            var result = _settingService.InsertUpdateSalaryStructure(salaryStructure);
            return BuildResponse(result);
        }

        [HttpPut("UpdateSalaryComponentDetail/{componentId}")]
        public IResponse<ApiResponse> UpdateSalaryComponentDetail([FromRoute] string componentId, [FromBody] SalaryComponents component)
        {
            var result = _settingService.UpdateSalaryComponentDetailService(componentId, component);
            return BuildResponse(result);
        }

        [HttpPut("EnableSalaryComponentDetail/{componentId}")]
        public IResponse<ApiResponse> EnableSalaryComponentDetail([FromRoute] string componentId, [FromBody] SalaryComponents component)
        {
            var result = _settingService.EnableSalaryComponentDetailService(componentId, component);
            return BuildResponse(result);
        }

        [HttpGet("FetchComponentDetailById/{componentId}")]
        public IResponse<ApiResponse> FetchComponentDetailById(int componentTypeId)
        {
            var result = _settingService.FetchComponentDetailByIdService(componentTypeId);
            return BuildResponse(result);
        }
    }
}
