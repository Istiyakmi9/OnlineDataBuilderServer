using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplateController : BaseController
    {
        private readonly ITemplateService _templateService;
        public TemplateController(ITemplateService templateService)
        {
            _templateService = templateService;
        }

        [HttpGet("GetBillingTemplateDetail")]
        public IResponse<ApiResponse> GetBillingTemplateDetail()
        {
            var result = _templateService.GetBillingTemplateDetailService();
            return BuildResponse(result);
        }
    }
}
