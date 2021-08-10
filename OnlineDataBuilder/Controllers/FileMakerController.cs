using DocMaker.PdfService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileMakerController : BaseController
    {
        private readonly IFileMaker _iFileMaker;
        public FileMakerController(IFileMaker iFileMaker)
        {
            _iFileMaker = iFileMaker;
        }

        [HttpPost]
        [Route("GeneratePdf")]
        public IResponse<ApiResponse> GeneratePdf([FromBody]PdfModal pdfModal)
        {
            _iFileMaker.TextSharpGeneratePdf();
            return BuildResponse(true, System.Net.HttpStatusCode.OK);
        } 
    }
}
