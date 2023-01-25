using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyNotificationController : BaseController
    {
        private readonly ICompanyNotificationService _companyNotificationService;

        public CompanyNotificationController(ICompanyNotificationService companyNotificationService)
        {
            _companyNotificationService = companyNotificationService;
        }

        [HttpPost("InsertUpdateNotification")]
        public IResponse<ApiResponse> InsertUpdateNotification(CompanyNotification notification)
        {
            var result = _companyNotificationService.InsertUpdateNotificationService(notification);
            return BuildResponse(result);
        }

        [HttpPost("GetNotificationRecord")]
        public IResponse<ApiResponse> GetNotificationRecord(FilterModel filterModel)
        {
            var result = _companyNotificationService.GetNotificationRecordService(filterModel);
            return BuildResponse(result);
        }
    }
}
