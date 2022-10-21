using ModalLayer.Modal;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface ILeaveRequestService
    {
        List<LeaveRequestNotification> ApprovalLeaveService(LeaveRequestNotification approvalRequest);
        List<LeaveRequestNotification> RejectLeaveService(LeaveRequestNotification approvalRequest);
        List<LeaveRequestNotification> ReAssigneToOtherManagerService(LeaveRequestNotification approvalRequest);
    }
}
