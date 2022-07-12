namespace ModalLayer.Modal.Accounts
{
    public class TaxDetails
    {
        public long EmployeeId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TaxDeducted { get; set; }
        public decimal TaxPaid { get; set; }
    }
}
