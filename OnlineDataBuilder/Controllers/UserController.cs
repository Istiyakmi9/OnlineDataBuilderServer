﻿using Microsoft.AspNetCore.Authorization;
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
    public class UserController : ControllerBase
    {
        [Authorize(Role.Admin)]
        [HttpPost("GetEmployeeDeatils")]
        public IResponse<ApiResponse> InternalUser(UserDetail userDetail)
        {
            return null;
        }
    }
}
