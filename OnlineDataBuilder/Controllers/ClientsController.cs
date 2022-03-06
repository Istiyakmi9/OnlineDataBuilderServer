using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ModalLayer.Modal;
using Newtonsoft.Json;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Code;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace OnlineDataBuilder.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : BaseController
    {
        private readonly IClientsService _clientsService;
        public ClientsController(IClientsService clientsService)
        {
            _clientsService = clientsService;
        }

        [HttpGet("GetClientById/{ClientId}/{IsActive}")]
        public ApiResponse GetClientById(long ClientId, bool IsActive)
        {
            var Result = _clientsService.GetClientDetailById(ClientId, IsActive);
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpPost("RegisterClient")]
        public async Task<ApiResponse> RegisterClient(Organization client)
        {
            var Result = await _clientsService.RegisterClient(client);
            return BuildResponse(Result, HttpStatusCode.OK);
        }

        [HttpPost("GetClients")]
        public ApiResponse GetClients(FilterModel filterModel)
        {
            var Result = _clientsService.GetClients(filterModel);
            return BuildResponse(Result, HttpStatusCode.OK);
        }
    }
}
