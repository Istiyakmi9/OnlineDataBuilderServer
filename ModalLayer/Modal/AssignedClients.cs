using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public class AssignedClients
    {
        public long EmployeeUid { set; get; }
        public long EmployeeMappedClientsUid { get; set; }
        public long ClientUid { set; get; }
        public string ClientName { set; get; }
        public float ActualPackage { set; get; }
        public float FinalPackage { set; get; }
        public float TakeHomeByCandidate { set; get; }
        public bool IsPermanent { set; get; }
        public long FileId { set; get; }
        public int BillingHours { set; get; } = 0;
        public int WorkingDaysPerWeek { set; get; } = 0;
    }
}
