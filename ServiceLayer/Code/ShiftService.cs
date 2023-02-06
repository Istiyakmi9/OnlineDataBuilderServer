using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System.Collections.Generic;

namespace ServiceLayer.Code
{
    public class ShiftService : IShiftService
    {
        private readonly IDb _db;
        private readonly CurrentSession _session;

        public ShiftService(IDb db, CurrentSession session)
        {
            _db = db;
            _session = session;
        }

        public List<ShiftDetail> GetAllShiftService(FilterModel filterModel)
        {
            var result = _db.GetList<ShiftDetail>("sp_work_shifts_filter", new
            {
                filterModel.SearchString,
                filterModel.PageIndex,
                filterModel.PageSize,
                filterModel.SortBy
            });
            return result;
        }

        public List<ShiftDetail> WorkShiftInsertUpdateService(ShiftDetail shiftDetail)
        {
            ValidateWorkShift(shiftDetail);
            var oldshift = _db.Get<ShiftDetail>("sp_work_shifts_getby_id", new { WorkShiftId = shiftDetail.WorkShiftId });
            if (oldshift == null)
                oldshift = shiftDetail;
            else
            {
                oldshift.Department = shiftDetail.Department;
                oldshift.WorkFlowCode = shiftDetail.WorkFlowCode;
                oldshift.ShiftTitle = shiftDetail.ShiftTitle;
                oldshift.Description = shiftDetail.Description;
                oldshift.IsMon = shiftDetail.IsMon;
                oldshift.IsTue = shiftDetail.IsTue;
                oldshift.IsWed = shiftDetail.IsWed;
                oldshift.IsThu = shiftDetail.IsThu;
                oldshift.IsFri = shiftDetail.IsFri;
                oldshift.IsSat = shiftDetail.IsSat;
                oldshift.IsSun = shiftDetail.IsSun;
                oldshift.TotalWorkingDays = shiftDetail.TotalWorkingDays;
                oldshift.StartDate = shiftDetail.StartDate;
                oldshift.EndDate = shiftDetail.EndDate;
                oldshift.OfficeTime = shiftDetail.OfficeTime;
                oldshift.Duration = shiftDetail.Duration;
                oldshift.Status = shiftDetail.Status;
            }
            oldshift.AdminId = _session.CurrentUserDetail.UserId;
            var result = _db.Execute<ShiftDetail>("sp_work_shifts_insupd", oldshift, true);
            if (string.IsNullOrEmpty(result))
                throw HiringBellException.ThrowBadRequest("Fail to insert or update shift detail");

            FilterModel filterModel = new FilterModel
            {
                SearchString = $"1=1 and CompanyId={shiftDetail.CompanyId}"
            };
            return this.GetAllShiftService(filterModel);
        }

        private void ValidateWorkShift(ShiftDetail shiftDetail)
        {
            if (shiftDetail.CompanyId <= 0)
                throw HiringBellException.ThrowBadRequest("Invalid company selected. Please login again");

            if (shiftDetail.Department <= 0)
                throw HiringBellException.ThrowBadRequest("Invalid department selected");

            if (string.IsNullOrEmpty(shiftDetail.WorkFlowCode))
                throw HiringBellException.ThrowBadRequest("Work flow code is null or empty");

            if (string.IsNullOrEmpty(shiftDetail.ShiftTitle))
                throw HiringBellException.ThrowBadRequest("Shift title is null or empty");

            if (shiftDetail.TotalWorkingDays <= 0)
                throw HiringBellException.ThrowBadRequest("Working days is zero or invalid");

            if (shiftDetail.StartDate == null)
                throw HiringBellException.ThrowBadRequest("Start date is null or empty");

            if (shiftDetail.EndDate == null)
                throw HiringBellException.ThrowBadRequest("End date is null or empty");

            if (string.IsNullOrEmpty(shiftDetail.OfficeTime))
                throw HiringBellException.ThrowBadRequest("Office time is null or empty");

            if (shiftDetail.Duration <= 0)
                throw HiringBellException.ThrowBadRequest("Department is zero or invalid");

            if (shiftDetail.LunchDuration <= 0)
                throw HiringBellException.ThrowBadRequest("Lunch duration is zero or invalid");
        }
    }
}
