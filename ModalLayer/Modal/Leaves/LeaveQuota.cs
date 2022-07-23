using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Leaves
{
    public class LeaveQuota
    {
        public bool IsQuotaLimit { get; set; }
        public int QuotaLimit { get; set; }
        public bool IsBeyondAnnualQuota { get; set; }
        public int BeyondAnnualQuota { get; set; }
        public bool IsLeaveQuotaAllocated { get; set; }
        public int LeaveQuotaAllocatedAfter { get; set; }
        public bool IsManagerAwardCasual { get; set; }
    }
}
