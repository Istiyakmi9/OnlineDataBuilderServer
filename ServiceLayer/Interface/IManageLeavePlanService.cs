using ModalLayer.Modal.Leaves;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface IManageLeavePlanService
    {
        LeavePlanConfiguration UpdateLeaveAccrual(int leavePlanTypeId, LeaveAccrual leaveAccrual);
        LeavePlanConfiguration UpdateLeaveDetail(int leavePlanTypeId, LeaveDetail leaveDetail);
        LeavePlanConfiguration GetLeaveConfigurationDetail(int leavePlanTypeId);
    }
}
