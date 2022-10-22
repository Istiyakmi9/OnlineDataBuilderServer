using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface ILeaveRequestService
    {
        dynamic ApprovalLeaveService(LeaveRequestDetail leaveRequestDetail);
        dynamic RejectLeaveService(LeaveRequestDetail leaveRequestDetail);
        List<LeaveRequestNotification> ReAssigneToOtherManagerService(LeaveRequestNotification approvalRequest);
    }
}
