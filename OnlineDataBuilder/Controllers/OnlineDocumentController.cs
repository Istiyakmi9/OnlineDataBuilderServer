using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ModalLayer.Modal;
using Newtonsoft.Json;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Net;
using ServiceLayer.Code;

namespace OnlineDataBuilder.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class OnlineDocumentController : BaseController
    {
        private readonly IOnlineDocumentService _ionlineDocumentService;
        private readonly CommonFilterService _commonFilterService;
        private readonly HttpContext _httpContext;

        public OnlineDocumentController(IOnlineDocumentService ionlineDocumentService, 
            IHttpContextAccessor httpContext, 
            CommonFilterService commonFilterService)
        {
            _ionlineDocumentService = ionlineDocumentService;
            _httpContext = httpContext.HttpContext;
            _commonFilterService = commonFilterService;
        }

        [HttpPost]
        [Route("GetOnlineDocuments")]
        public IResponse<ApiResponse> GetOnlineDocuments([FromBody] FilterModel filterModel)
        {
            var Result = _commonFilterService.GetResult<OnlineDocumentModel>(filterModel, "SP_OnlineDocument_Get");
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("GetOnlineDocumentsWithFiles")]
        public IResponse<ApiResponse> GetOnlineDocumentsWithFiles([FromBody] FilterModel filterModel)
        {
            var Result = _ionlineDocumentService.GetOnlineDocumentsWithFiles(filterModel);
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("CreateDocument")]
        public IResponse<ApiResponse> CreateDocument([FromBody] CreatePageModel createPageModel)
        {
            var Result = _ionlineDocumentService.CreateDocument(createPageModel);
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpPost("DeleteFiles")]
        public IResponse<ApiResponse> DeleteFiles([FromBody] List<Files> fileDetails)
        {
            var Result = _ionlineDocumentService.DeleteFilesService(fileDetails);
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpPost("EditCurrentFile")]
        public IResponse<ApiResponse> EditCurrentFile([FromBody] Files fileDetail)
        {
            var Result = _ionlineDocumentService.EditCurrentFileService(fileDetail);
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpPost("UploadDocumentDetail")]
        public IResponse<ApiResponse> UploadDocumentDetail()
        {
            _httpContext.Request.Form.TryGetValue("facultObject", out StringValues RegistrationData);
            _httpContext.Request.Form.TryGetValue("fileDetail", out StringValues FileData);
            if (RegistrationData.Count > 0)
            {
                CreatePageModel createPageModel = JsonConvert.DeserializeObject<CreatePageModel>(RegistrationData[0]);
                List<Files> fileDetail = JsonConvert.DeserializeObject<List<Files>>(FileData);
                if (createPageModel != null)
                {
                    IFormFileCollection files = _httpContext.Request.Form.Files;
                    var Result = _ionlineDocumentService.UploadDocumentDetail(createPageModel, files, fileDetail);
                    BuildResponse(Result, HttpStatusCode.OK);
                }
            }
            return apiResponse;
        }

        [HttpGet("LoadApplicationData")]
        public ApiResponse LoadApplicationData()
        {
            var Result = _ionlineDocumentService.LoadApplicationData();
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpGet("GetFilesAndFolderById/{Type}/{Uid}")]
        public ApiResponse GetFilesAndFolderById(string Type, string Uid)
        {
            var Result = _ionlineDocumentService.GetFilesAndFolderByIdService(Type, Uid);
            return BuildResponse(Result, HttpStatusCode.OK);
        }
    }
}
