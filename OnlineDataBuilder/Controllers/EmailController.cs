using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ModalLayer.Modal;
using Newtonsoft.Json;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Code;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Net;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : BaseController
    {
        private readonly HttpContext _httpContext;
        private readonly IEmailService _emailService;
        public EmailController(IEmailService emailService, IHttpContextAccessor httpContext)
        {
            _emailService = emailService;
            _httpContext = httpContext.HttpContext;
        }

        [HttpPost("SendEmailRequest")]
        public ApiResponse SendEmailRequest()
        {
            StringValues emailDetail = default(string);
            _httpContext.Request.Form.TryGetValue("mailDetail", out emailDetail);
            if (emailDetail.Count == 0)
                throw new HiringBellException("No detail found. Please pass all detail.");

            EmailSenderModal emailSenderModal = JsonConvert.DeserializeObject<EmailSenderModal>(emailDetail);
            IFormFileCollection files = _httpContext.Request.Form.Files;
            var Result = _emailService.SendEmailRequestService(emailSenderModal, files);
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpGet("GetMyMails")]
        public ApiResponse GetMyMails()
        {
            var Result = _emailService.GetMyMailService();
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpGet("GetEmailSettingByCompId/{CompanyId}")]
        public IResponse<ApiResponse> GetEmailSettingByCompId(int CompanyId)
        {
            var result = _emailService.GetEmailSettingByCompIdService(CompanyId);
            return BuildResponse(result);
        }

        [HttpPost("InsertUpdateEmailSetting")]
        public IResponse<ApiResponse> InsertUpdateEmailSetting(EmailSettingDetail emailSettingDetail)
        {
            var result = _emailService.InsertUpdateEmailSettingService(emailSettingDetail);
            return BuildResponse(result);
        }
    }
}
