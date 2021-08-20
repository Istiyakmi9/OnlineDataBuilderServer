using Microsoft.AspNetCore.Mvc;
using OnlineDataBuilder.ContextHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
    }
}