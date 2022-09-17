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
using System.Net;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalaryComponentController : BaseController
    {
        private readonly ISalaryComponentService _salaryComponentService;
        private readonly HttpContext _httpContext;

        public SalaryComponentController(ISalaryComponentService salaryComponentService, IHttpContextAccessor httpContext)
        {
            _salaryComponentService = salaryComponentService;
            _httpContext = httpContext.HttpContext;
        }


        [HttpGet("GetSalaryComponentsDetail")]
        public IResponse<ApiResponse> GetSalaryComponentsDetail()
        {
            var result = _salaryComponentService.GetSalaryComponentsDetailService();
            return BuildResponse(result);
        }

        [HttpGet("GetCustomSalryPageData/{CompanyId}")]
        public IResponse<ApiResponse> GetCustomSalryPageData(int CompanyId)
        {
            var result = _salaryComponentService.GetCustomSalryPageDataService(CompanyId);
            return BuildResponse(result);
        }

        [HttpGet("GetSalaryGroupsById/{SalaryGroupId}")]
        public IResponse<ApiResponse> GetSalaryGroupsById(int SalaryGroupId)
        {
            var result = _salaryComponentService.GetSalaryGroupsByIdService(SalaryGroupId);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("UpdateSalaryComponents")]
        public IResponse<ApiResponse> UpdateSalaryComponents(List<SalaryComponents> salaryComponents)
        {
            var result = _salaryComponentService.UpdateSalaryComponentService(salaryComponents);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("InsertUpdateSalaryComponentsByExcel")]
        public IResponse<ApiResponse> InsertUpdateSalaryComponentsByExcel(List<SalaryComponents> salaryComponents)
        {
            var result = _salaryComponentService.InsertUpdateSalaryComponentsByExcelService(salaryComponents);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("AddSalaryGroup")]
        public IResponse<ApiResponse> AddSalaryGroup(SalaryGroup salaryGroup)
        {
            var result = _salaryComponentService.AddSalaryGroup(salaryGroup);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("UpdateSalaryGroup")]
        public IResponse<ApiResponse> UpdateSalaryGroup(SalaryGroup salaryGroup)
        {
            var result = _salaryComponentService.UpdateSalaryGroup(salaryGroup);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("UpdateSalaryGroupComponents")]
        public IResponse<ApiResponse> UpdateSalaryGroupComponents(SalaryGroup salaryGroup)
        {
            var result = _salaryComponentService.UpdateSalaryGroupComponentService(salaryGroup);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("AddUpdateRecurringComponents")]
        public IResponse<ApiResponse> AddUpdateRecurringComponents(SalaryStructure salaryStructure)
        {
            var result = _salaryComponentService.AddUpdateRecurringComponents(salaryStructure);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("AddAdhocComponents")]
        public IResponse<ApiResponse> AddAdhocComponents(SalaryStructure salaryStructure)
        {
            var result = _salaryComponentService.AddAdhocComponents(salaryStructure);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("AddDeductionComponents")]
        public IResponse<ApiResponse> AddDeductionComponents(SalaryStructure salaryStructure)
        {
            var result = _salaryComponentService.AddDeductionComponents(salaryStructure);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("AddBonusComponents")]
        public IResponse<ApiResponse> AddBonusComponents(SalaryStructure salaryStructure)
        {
            var result = _salaryComponentService.AddBonusComponents(salaryStructure);
            return BuildResponse(result);
        }

        [HttpGet("GetSalaryGroupComponents/{SalaryGroupId}")]
        public IResponse<ApiResponse> GetSalaryGroupComponents(int SalaryGroupId)
        {
            var result = _salaryComponentService.GetSalaryGroupComponents(SalaryGroupId);
            return BuildResponse(result);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("InsertUpdateSalaryBreakUp/{EmployeeId}/{PresentMonth}/{PresentYear}")]
        public IResponse<ApiResponse> SalaryDetail(long EmployeeId, int PresentMonth, int PresentYear)
        {
            _httpContext.Request.Form.TryGetValue("completesalarydetail", out StringValues compSalaryDetail);
            if (compSalaryDetail.Count > 0)
            {
                var fullSalaryDetail = JsonConvert.DeserializeObject<List<CalculatedSalaryBreakupDetail>>(compSalaryDetail);
                var result = _salaryComponentService.SalaryDetailService(EmployeeId, fullSalaryDetail, PresentMonth, PresentYear);
                return BuildResponse(result);
            }
            return BuildResponse("No files found", HttpStatusCode.OK);
        }

        [HttpGet("SalaryBreakupCalc/{EmployeeId}/{CTCAnnually}")]
        public IResponse<ApiResponse> SalaryBreakupCalc(long EmployeeId, int CTCAnnually)
        {
            var result = _salaryComponentService.SalaryBreakupCalcService(EmployeeId, CTCAnnually);
            return BuildResponse(result);
        }

        [HttpGet("GetSalaryBreakupByEmpId/{EmployeeId}")]
        public IResponse<ApiResponse> GetSalaryBreakupByEmpId(long EmployeeId)
        {
            var result = _salaryComponentService.GetSalaryBreakupByEmpIdService(EmployeeId);
            return BuildResponse(result);
        }

        [HttpGet("GetSalaryGroupByCTC/{CTC}")]
        public IResponse<ApiResponse> GetSalaryGroupByCTC(decimal CTC)
        {
            var result = _salaryComponentService.GetSalaryGroupByCTC(CTC);
            return BuildResponse(result);
        }
    }
}
