﻿using Microsoft.AspNetCore.Mvc;
using OnlineDataBuilder.ContextHandler;
using System.Net;

namespace OnlineDataBuilder.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected ApiResponse apiResponse;
        public BaseController()
        {
            apiResponse = new ApiResponse();
        }
        public ApiResponse BuildResponse(dynamic Data, HttpStatusCode httpStatusCode, string Resion = null, string Token = null)
        {
            apiResponse.AuthenticationToken = Token;
            apiResponse.HttpStatusMessage = Resion;
            apiResponse.HttpStatusCode = httpStatusCode;
            apiResponse.ResponseBody = Data;
            return apiResponse;
        }

        public ApiResponse GenerateResponse(HttpStatusCode httpStatusCode, dynamic Data = null)
        {
            apiResponse.HttpStatusCode = httpStatusCode;
            apiResponse.ResponseBody = Data;
            return apiResponse;
        }
    }
}