using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using System;
using System.Threading.Tasks;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadPayrollDataController : BaseController
    {
        private readonly HttpContext _httpContext;
        public UploadPayrollDataController(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext.HttpContext;
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPost("UploadPayrollExcel")]
        public async Task<ApiResponse> UploadPayrollExcel()
        {
            try
            {
                IFormFileCollection files = _httpContext.Request.Form.Files;
                //var result = await _employeeService.UpdateEmployeeService(files);
                return BuildResponse("file found");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
