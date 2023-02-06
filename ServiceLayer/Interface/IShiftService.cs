using ModalLayer.Modal;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface IShiftService
    {
        List<ShiftDetail> GetAllShiftService(FilterModel filterModel);
        List<ShiftDetail> WorkShiftInsertUpdateService(ShiftDetail shiftDetail);
    }
}
