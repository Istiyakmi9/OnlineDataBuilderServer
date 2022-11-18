using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxRegimeController : BaseController
    {
        private readonly ITaxRegimeService _taxRegimeService;

        public TaxRegimeController(ITaxRegimeService taxRegimeService)
        {
            _taxRegimeService = taxRegimeService;
        }

        [HttpPost("AddUpdateTaxRegimeDesc")]
        public IResponse<ApiResponse> AddUpdateTaxRegimeDesc(TaxRegimeDesc taxRegimeDesc)
        {
            var result = _taxRegimeService.AddUpdateTaxRegimeDescService(taxRegimeDesc);
            return BuildResponse(result);
        }

        [HttpGet("GetAllRegime")]
        public IResponse<ApiResponse> GetAllRegime()
        {
            var result = _taxRegimeService.GetAllRegimeService();
            return BuildResponse(result);
        }

        [HttpPost("AddUpdateTaxRegime")]
        public async Task<ApiResponse> AddUpdateTaxRegime(List<TaxRegime> taxRegimes)
        {
            var result = await _taxRegimeService.AddUpdateTaxRegimeService(taxRegimes);
            return BuildResponse(result);
        }

        [HttpPost("AddUpdateAgeGroup")]
        public IResponse<ApiResponse> AddUpdateAgeGroup(TaxAgeGroup taxAgeGroup)
        {
            var result = _taxRegimeService.AddUpdateAgeGroupService(taxAgeGroup);
            return BuildResponse(result);
        }
    }
}
