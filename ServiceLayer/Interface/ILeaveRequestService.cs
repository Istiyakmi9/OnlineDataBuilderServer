using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface ILeaveRequestService
    {
        RequestModel ApprovalLeaveService(LeaveRequestDetail leaveRequestDetail, int filterId = ApplicationConstants.Only);
        RequestModel RejectLeaveService(LeaveRequestDetail leaveRequestDetail, int filterId = ApplicationConstants.Only);
        List<LeaveRequestNotification> ReAssigneToOtherManagerService(LeaveRequestNotification approvalRequest, int filterId = ApplicationConstants.Only);
    }
}
