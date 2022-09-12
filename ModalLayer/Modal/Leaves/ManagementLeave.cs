using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Leaves
{
    public class ManagementLeave
    {
        public int LeaveManagementId { set; get; }
        public int LeavePlanTypeId { set; get; }
        public bool CanManagerAwardCausalLeave { set; get; }
    }
}
