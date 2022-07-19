using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal.Leaves;
using ServiceLayer.Interface;
using System.Collections.Generic;

namespace ServiceLayer.Code
{
    public  class LeaveService : ILeaveService
    {
        private readonly IDb _db;

        public LeaveService(IDb db)
        {
            _db = db;
        }

        public string AddLeavePlansService(LeavePlan leavePlan)
        {
            throw new System.NotImplementedException();
        }

        public List<LeavePlan> GetLeavePlansService()
        {
            List<LeavePlan> LeavePlans = _db.GetList<LeavePlan>("sp_leave_plans_get");
            return LeavePlans;
        }

        public string UpdateLeavePlansService(int leavePlanId, LeavePlan leavePlan)
        {
            throw new System.NotImplementedException();
        }
    }
}
