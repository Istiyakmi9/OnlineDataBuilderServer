﻿using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObjectiveController : BaseController
    {
        private readonly IObjectiveService _objectiveService;

        public ObjectiveController(IObjectiveService objectiveService)
        {
            _objectiveService = objectiveService;
        }

        [HttpPost("ObjectiveInsertUpdate")]
        public IResponse<ApiResponse> ObjectiveInsertUpdate([FromBody] ObjectiveDetail objectiveDetail)
        {
            var result = _objectiveService.ObjectiveInsertUpdateService(objectiveDetail);
            return BuildResponse(result);   
        }

        [HttpPost("GetPerformanceObjective")]
        public IResponse<ApiResponse> GetPerformanceObjective([FromBody] FilterModel filterModel)
        {
            var result = _objectiveService.GetPerformanceObjectiveService(filterModel);
            return BuildResponse(result);
        }
    }
}
