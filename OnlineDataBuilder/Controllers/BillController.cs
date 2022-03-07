﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    [Route("api/[controller]")]
    [ApiController]
    public class BillController : BaseController
    {
        private readonly IBillService _billService;
        private readonly HttpContext _httpContext;

        public BillController(IBillService billService, IHttpContextAccessor httpContext)
        {
            _billService = billService;
            _httpContext = httpContext.HttpContext;
        }

        [HttpPost("UpdateGstStatus/{BillNo}")]
        public ApiResponse UpdateGstStatus(string BillNo)
        {
            _httpContext.Request.Form.TryGetValue("gstDetail", out StringValues GstDetail);
            _httpContext.Request.Form.TryGetValue("fileDetail", out StringValues FileData);
            if (GstDetail.Count > 0)
            {
                GstStatusModel gstStatusModel = JsonConvert.DeserializeObject<GstStatusModel>(GstDetail[0]);
                List<Files> fileDetail = JsonConvert.DeserializeObject<List<Files>>(FileData);
                if (gstStatusModel != null)
                {
                    IFormFileCollection files = _httpContext.Request.Form.Files;
                    var Result = _billService.UpdateGstStatus(gstStatusModel, files, fileDetail);
                    return BuildResponse(Result, HttpStatusCode.OK);
                }
            }
            
            return GenerateResponse(HttpStatusCode.BadRequest);
        }
    }
}