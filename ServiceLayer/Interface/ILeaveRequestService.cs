using ModalLayer.Modal;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface ILeaveRequestService
    {
        dynamic FetchPendingRequestService(long employeeId, int requestTypeId);
        List<LeaveRequestNotification> ApprovalOrRejectActionService(LeaveRequestNotification approvalRequest, ItemStatus status, int RequestId);
        List<LeaveRequestNotification> ReAssigneToOtherManagerService(LeaveRequestNotification approvalRequest);
    }
}
