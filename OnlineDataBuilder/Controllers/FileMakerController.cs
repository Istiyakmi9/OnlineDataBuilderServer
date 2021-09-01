using DocMaker.PdfService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileMakerController : BaseController
    {
        private readonly BuildPdfTable _buildPdfTable;
        private readonly IOnlineDocumentService _onlineDocumentService;
        private readonly IFileMaker _iFileMaker;
        public FileMakerController(IFileMaker iFileMaker, IConfiguration configuration, 
            IOnlineDocumentService onlineDocumentService,
            IOptions<BuildPdfTable> options)
        {
            _iFileMaker = iFileMaker;
            _buildPdfTable = options.Value;
            _onlineDocumentService = onlineDocumentService;
        }

        [HttpPost]
        [Route("GeneratePdf")]
        public IResponse<ApiResponse> GeneratePdf([FromBody] PdfModal pdfModal)
        {
            FileDetail fileDetail = _onlineDocumentService.InsertGeneratedBillRecord(_buildPdfTable, pdfModal);
            return BuildResponse(fileDetail, System.Net.HttpStatusCode.OK);
        }
    }
}
