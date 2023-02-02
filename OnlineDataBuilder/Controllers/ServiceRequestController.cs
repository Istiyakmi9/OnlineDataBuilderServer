using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Threading.Tasks;

namespace OnlineDataBuilder.Controllers
{
    [Authorize(Roles = Role.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestService _serviceRequestService;

        public ServiceRequestController(IServiceRequestService serviceRequestService)
        {
            _serviceRequestService = serviceRequestService;
        }

        [HttpPost("GetServiceRequest")]
        public async Task<ApiResponse> GetServiceRequest(FilterModel filter)
        {
            var result = await _serviceRequestService.GetServiceRequestService(filter);
            return BuildResponse(result);
        }
    }
}
