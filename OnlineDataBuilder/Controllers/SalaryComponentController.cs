using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;

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
    }
}
