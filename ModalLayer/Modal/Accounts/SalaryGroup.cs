using System.Collections.Generic;

namespace ModalLayer.Modal.Accounts
{
    public class SalaryGroup : CreationInfo
    {
        public int CompanyId { get; set; }
        public int SalaryGroupId { get; set; }
        public string SalaryComponents { get; set; }
        public List<SalaryComponents> GroupComponents { get; set; }
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        public decimal MinAmount { set; get; }
        public decimal MaxAmount { set; get; }
        public decimal? CTC { get; set; }
    }
}
