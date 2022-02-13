using DocMaker.PdfService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Linq;

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
        public FileMakerController(IFileMaker iFileMaker, IConfiguration configuration,
            IOnlineDocumentService onlineDocumentService,
            IFileService fileService,
            IOptions<BuildPdfTable> options)
        {
            _iFileMaker = iFileMaker;
            _buildPdfTable = options.Value;
            _onlineDocumentService = onlineDocumentService;
            _fileService = fileService;
        }

        [HttpPost]
        [Route("GeneratePdf")]
        public IResponse<ApiResponse> GeneratePdf([FromBody] PdfModal pdfModal)
        {
            var fileDetail = _onlineDocumentService.InsertGeneratedBillRecord(_buildPdfTable, pdfModal);
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
        [Route("CreateFolder")]
        public IResponse<ApiResponse> CreateFolder(Files file)
        {
            var result = _fileService.CreateFolder(file);
            return BuildResponse(result, System.Net.HttpStatusCode.OK);
        }

        [HttpDelete]
        [Route("DeleteFile/{userId}")]
        public IResponse<ApiResponse> DeleteFiles(long userId, List<string> fileIds)
        {
            var result = _fileService.DeleteFiles(userId, fileIds);
            return BuildResponse(result, System.Net.HttpStatusCode.OK);
        }
    }
}
