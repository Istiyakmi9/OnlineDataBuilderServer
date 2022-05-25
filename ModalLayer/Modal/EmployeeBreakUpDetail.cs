using System;

namespace ModalLayer.Modal
{
    public class EmployeeBreakUpDetail
    {
        public long EmployeeId { set; get; }
        public string BreakUpDetail { set; get; }
        public int BreakUpHeaderCount { set; get; }
        public string DeductionDetail { set; get; }
        public int DeductionHeaderCount { set; get; }
        public DateTime UpdatedOn { set; get; }
    }
}
