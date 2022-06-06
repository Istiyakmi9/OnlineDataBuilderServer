using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;

namespace OnlineDataBuilder.Controllers
{
    [Authorize(Roles = Role.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class SalaryComponentController : BaseController
    {
        private readonly ISalaryComponentService _salaryComponentService;

        public SalaryComponentController(ISalaryComponentService salaryComponentService)
        {
            _salaryComponentService = salaryComponentService;
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

        [HttpPost("UpdateSalaryComponents")]
        public IResponse<ApiResponse> UpdateSalaryComponents(List<SalaryComponents> salaryComponents)
        {
            var result = _salaryComponentService.UpdateSalaryComponentService(salaryComponents);
            return BuildResponse(result);
        }

        [HttpPost("AddSalaryGroup")]
        public IResponse<ApiResponse> AddSalaryGroup(SalaryGroup salaryGroup)
        {
            var result = _salaryComponentService.AddSalaryGroup(salaryGroup);
            return BuildResponse(result);
        }
    }
}
