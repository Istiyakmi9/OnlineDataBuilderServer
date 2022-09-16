using ModalLayer.Modal;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface IRequestService
    {
        List<ApprovalRequest> FetchPendingRequestService(long employeeId, int requestTypeId);
        List<ApprovalRequest> ApprovalOrRejectActionService(ApprovalRequest approvalRequest, ItemStatus status, int RequestId);
        List<ApprovalRequest> ReAssigneToOtherManagerService(ApprovalRequest approvalRequest);
    }
}
