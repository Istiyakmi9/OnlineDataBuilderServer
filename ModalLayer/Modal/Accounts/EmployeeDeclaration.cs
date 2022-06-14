namespace ModalLayer.Modal.Accounts
{
    public class EmployeeDeclaration
    {
        public long EmployeeDeclarationId { set; get; }
        public long EmployeeId { set; get; }
        public string DocumentPath { set; get; }
        public string DeclarationDetail { set; get; }
        public string ComponentId { set; get; }
        public decimal Value { set; get; }
    }
}
