using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;

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

        [HttpPost("AnnexureOfferLetterInsertUpdate/{LetterType}")]
        public IResponse<ApiResponse> AnnexureOfferLetterInsertUpdate(AnnexureOfferLetter annexureOfferLetter, [FromRoute] int LetterType)
        {
            var result = _templateService.AnnexureOfferLetterInsertUpdateService(annexureOfferLetter, LetterType);
            return BuildResponse(result);
        }

        [HttpGet("GetAnnexureOfferLetter/{CompanyId}/{LetterType}")]
        public IResponse<ApiResponse> GetAnnexureOfferLetter([FromRoute] int CompanyId, [FromRoute] int LetterType)
        {
            var result = _templateService.GetOfferLetterService(CompanyId, LetterType);
            return BuildResponse(result);
        }
    }
}
