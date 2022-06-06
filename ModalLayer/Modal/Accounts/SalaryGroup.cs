namespace ModalLayer.Modal.Accounts
{
    public class SalaryGroup : CreationInfo
    {
        public int SalaryGroupId { get; set; }
        public string ComponentId { get; set; }
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        public decimal MinAmount { set; get; }
        public decimal MaxAmount { set; get; }
    }
}
