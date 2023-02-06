using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModalLayer.Modal;
using OnlineDataBuilder.ContextHandler;
using ServiceLayer.Interface;

namespace OnlineDataBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftController : BaseController
    {
        private readonly IShiftService _shiftService;

        public ShiftController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        [HttpPost("GetAllWorkShift")]
        public IResponse<ApiResponse> GetAllWorkShift(FilterModel filterModel)
        {
            var result = _shiftService.GetAllShiftService(filterModel);
            return BuildResponse(result);
        }

        [HttpPost("WorkShiftInsertUpdate")]
        public IResponse<ApiResponse> WorkShiftInsertUpdate(ShiftDetail shiftDetail)
        {
            var result = _shiftService.WorkShiftInsertUpdateService(shiftDetail);
            return BuildResponse(result);
        }
    }
}
