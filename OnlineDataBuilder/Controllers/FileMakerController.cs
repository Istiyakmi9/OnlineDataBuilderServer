using DocMaker.PdfService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileMakerController : BaseController
    {
        private readonly BuildPdfTable _buildPdfTable;
        private readonly IFileMaker _iFileMaker;
        public FileMakerController(IFileMaker iFileMaker, IConfiguration configuration, IOptions<BuildPdfTable> options)
        {
            _iFileMaker = iFileMaker;
            _buildPdfTable = options.Value;
        }

        [HttpPost]
        [Route("GeneratePdf")]
        public IResponse<ApiResponse> GeneratePdf([FromBody] PdfModal pdfModal)
        {
            _iFileMaker.BuildPdfBill(_buildPdfTable);
            //_iFileMaker.BuildPdfBill_Single();
            return BuildResponse(true, System.Net.HttpStatusCode.OK);
        }
    }
}
