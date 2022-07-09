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
    [Authorize(Roles = Role.Admin)]
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

        [HttpGet("GetSalaryGroups")]
        public IResponse<ApiResponse> GetSalaryGroups()
        {
            var result = _salaryComponentService.GetSalaryGroupService();
            return BuildResponse(result);
        }

        [HttpGet("GetSalaryGroupsById/{SalaryGroupId}")]
        public IResponse<ApiResponse> GetSalaryGroupsById(int SalaryGroupId)
        {
            var result = _salaryComponentService.GetSalaryGroupsByIdService(SalaryGroupId);
            return BuildResponse(result);
        }

        [HttpPost("UpdateSalaryComponents")]
        public IResponse<ApiResponse> UpdateSalaryComponents(List<SalaryComponents> salaryComponents)
        {
            var result = _salaryComponentService.UpdateSalaryComponentService(salaryComponents);
            return BuildResponse(result);
        }

        [HttpPost("InsertUpdateSalaryComponentsByExcel")]
        public IResponse<ApiResponse> InsertUpdateSalaryComponentsByExcel(List<SalaryComponents> salaryComponents)
        {
            var result = _salaryComponentService.InsertUpdateSalaryComponentsByExcelService(salaryComponents);
            return BuildResponse(result);
        }

        [HttpPost("AddSalaryGroup")]
        public IResponse<ApiResponse> AddSalaryGroup(SalaryGroup salaryGroup)
        {
            var result = _salaryComponentService.AddSalaryGroup(salaryGroup);
            return BuildResponse(result);
        }

        [HttpPost("UpdateSalaryGroup")]
        public IResponse<ApiResponse> UpdateSalaryGroup(SalaryGroup salaryGroup)
        {
            var result = _salaryComponentService.UpdateSalaryGroup(salaryGroup);
            return BuildResponse(result);
        }

        [HttpPost("UpdateSalaryGroupComponents")]
        public IResponse<ApiResponse> UpdateSalaryGroupComponents(SalaryGroup salaryGroup)
        {
            var result = _salaryComponentService.UpdateSalaryGroupComponentService(salaryGroup);
            return BuildResponse(result);
        }

        [HttpPost("AddUpdateRecurringComponents")]
        public IResponse<ApiResponse> AddUpdateRecurringComponents(SalaryStructure salaryStructure)
        {
            var result = _salaryComponentService.AddUpdateRecurringComponents(salaryStructure);
            return BuildResponse(result);
         }

        [HttpPost("AddAdhocComponents")]
        public IResponse<ApiResponse> AddAdhocComponents(SalaryStructure salaryStructure)
        {
            var result = _salaryComponentService.AddAdhocComponents(salaryStructure);
            return BuildResponse(result);
        }

        [HttpPost("AddDeductionComponents")]
        public IResponse<ApiResponse> AddDeductionComponents(SalaryStructure salaryStructure)
        {
            var result = _salaryComponentService.AddDeductionComponents(salaryStructure);
            return BuildResponse(result);
        }

        [HttpPost("AddBonusComponents")]
        public IResponse<ApiResponse> AddBonusComponents(SalaryStructure salaryStructure)
        {
            var result = _salaryComponentService.AddBonusComponents(salaryStructure);
            return BuildResponse(result);
        }

        [HttpGet("GetSalaryGroupComponents/{CompanyId}")]
        public IResponse<ApiResponse> GetSalaryGroupComponents(int CompanyId)
        {
            var result = _salaryComponentService.GetSalaryGroupComponents(CompanyId);
            return BuildResponse(result);
        }

        [HttpPost("InsertUpdateSalaryBreakUp/{EmployeeId}")]
        public IResponse<ApiResponse> SalaryDetail(long EmployeeId)
        {
            _httpContext.Request.Form.TryGetValue("completesalarydetail", out StringValues compSalaryDetail);
            _httpContext.Request.Form.TryGetValue("salarydeatil", out StringValues salaryDetail);
            if (compSalaryDetail.Count > 0 && salaryDetail.Count > 0)
            {
                var fullSalaryDetail = JsonConvert.DeserializeObject<CompleteSalaryBreakup>(compSalaryDetail);
                var SalaryDetail = JsonConvert.DeserializeObject<SalaryBreakup>(salaryDetail);
                var result = _salaryComponentService.SalaryDetailService(EmployeeId, SalaryDetail, fullSalaryDetail);
                return BuildResponse(result);
            }
            return BuildResponse("No files found", HttpStatusCode.OK);
        }

        [HttpPost("SalaryBreakupCalc/{EmployeeId}/{SalaryGroupId}")]
        public IResponse<ApiResponse> SalaryBreakupCalc(long EmployeeId, int SalaryGroupId, [FromBody] int CTCAnnually)
        {
            var result = _salaryComponentService.SalaryBreakupCalcService(EmployeeId, SalaryGroupId, CTCAnnually);
            return BuildResponse(result);
        }
    }
}
