using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Net;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : BaseController
    {
        private IEmailService _emailService;
        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("SendEmailRequest")]
        public ApiResponse SendEmailRequest(MailRequest mailRequest)
        {
            var Result = _emailService.SendEmailRequestService(mailRequest);
            return BuildResponse(Result, HttpStatusCode.OK);
        }
    }
}
