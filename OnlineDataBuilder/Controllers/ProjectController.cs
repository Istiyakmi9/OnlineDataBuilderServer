using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : BaseController
    {
        private readonly IProjectService _projectService;
        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }
        [HttpPost("AddProject")]
        public IResponse<ApiResponse> AddProject(WikiDetail project)
        {
            var result = _projectService.AddProjectService(project);
            return BuildResponse(result);  
        }
        [HttpGet("GetAllProject")]
        public IResponse<ApiResponse> GetAllProject()
        {
            var result = _projectService.GetAllProjectService();
            return BuildResponse(result);
        }
    }
}
