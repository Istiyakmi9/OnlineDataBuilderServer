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

namespace OnlineDataBuilder.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class OnlineDocumentController : BaseController
    {
        private readonly IOnlineDocumentService _ionlineDocumentService;
        private readonly HttpContext _httpContext;

        public OnlineDocumentController(IOnlineDocumentService ionlineDocumentService, IHttpContextAccessor httpContext)
        {
            _ionlineDocumentService = ionlineDocumentService;
            _httpContext = httpContext.HttpContext;
        }

        [HttpPost]
        [Route("GetOnlineDocuments")]
        public IResponse<ApiResponse> GetOnlineDocuments([FromBody] FilterModel filterModel)
        {
            var Result = _ionlineDocumentService.GetOnlineDocuments(filterModel);
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
    }
}
