using ModalLayer.Modal;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface IRequestService
    {
        List<ApprovalRequest> FetchPendingRequestService(int employeeId, int requestTypeId);
        string ApprovalActionService(ApprovalRequest approvalRequest);
        string ReAssigneToOtherManagerService(ApprovalRequest approvalRequest);
    }
}
