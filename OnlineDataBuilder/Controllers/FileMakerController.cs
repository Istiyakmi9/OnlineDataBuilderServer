using DocMaker.HtmlToDocx;
using DocMaker.PdfService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ModalLayer.Modal;
using ModalLayer.Modal.HtmlTagModel;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileMakerController : BaseController
    {
        private readonly BuildPdfTable _buildPdfTable;
        private readonly IOnlineDocumentService _onlineDocumentService;
        private readonly IFileMaker _iFileMaker;
        private readonly IFileService _fileService;
        private readonly IBillService _billService;
        private readonly IDOCXToHTMLConverter _iDOCXToHTMLConverter;

        public FileMakerController(IFileMaker iFileMaker, IConfiguration configuration,
            IOnlineDocumentService onlineDocumentService,
            IFileService fileService, IBillService billService,
            IOptions<BuildPdfTable> options,
            IDOCXToHTMLConverter iDOCXToHTMLConverter)
        {
            _iFileMaker = iFileMaker;
            _buildPdfTable = options.Value;
            _onlineDocumentService = onlineDocumentService;
            _fileService = fileService;
            _billService  = billService;
            _iDOCXToHTMLConverter = iDOCXToHTMLConverter;
        }

        [HttpPost]
        [Route("GeneratePdf")]
        public IResponse<ApiResponse> GeneratePdf([FromBody] PdfModal pdfModal)
        {
            var fileDetail = _onlineDocumentService.InsertGeneratedBillRecord(_buildPdfTable, pdfModal);
            return BuildResponse(fileDetail, System.Net.HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("GenerateBill")]
        public IResponse<ApiResponse> GenerateBill([FromBody] PdfModal pdfModal)
        {
            var fileDetail = _billService.GenerateDocument(_buildPdfTable, pdfModal);
            return BuildResponse(fileDetail, System.Net.HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("EditEmployeeBillDetail")]
        public IResponse<ApiResponse> EditEmployeeBillDetail([FromBody] FileDetail fileDetail)
        {
            var result = _onlineDocumentService.EditEmployeeBillDetailService(fileDetail);
            return BuildResponse(result, System.Net.HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("ReGenerateBill")]
        public IResponse<ApiResponse> ReGenerateBill([FromBody] FileDetail fileDetail)
        {
            var Result = _onlineDocumentService.ReGenerateService(_buildPdfTable, fileDetail);
            return BuildResponse(Result, System.Net.HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("CreateFolder")]
        public IResponse<ApiResponse> CreateFolder(Files file)
        {
            var result = _fileService.CreateFolder(file);
            return BuildResponse(result, System.Net.HttpStatusCode.OK);
        }

        [HttpDelete]
        [Route("DeleteFile/{userId}/{UserTypeId}")]
        public IResponse<ApiResponse> DeleteFiles(long userId, int userTypeId, List<string> fileIds)
        {
            var result = _fileService.DeleteFiles(userId, fileIds, userTypeId);
            return BuildResponse(result, System.Net.HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("GetDocxHtml")]
        public IResponse<ApiResponse> GetDocxHtml(FileDetail fileDetail)
        {
            var result = _iDOCXToHTMLConverter.ToHtml(fileDetail);
            return BuildResponse(result, System.Net.HttpStatusCode.OK);
        }
    }
}
