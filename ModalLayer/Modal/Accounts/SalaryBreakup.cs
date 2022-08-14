namespace ModalLayer.Modal.Accounts
{
    public class CompleteSalaryBreakup
    {
        public decimal BasicAnnually { get; set; }
        public decimal ConveyanceAnnually { get; set; }
        public decimal HRAAnnually { get; set; }
        public decimal MedicalAnnually { get; set; }
        public decimal CarRunningAnnually { get; set; }
        public decimal InternetAnnually { get; set; }
        public decimal TravelAnnually { get; set; }
        public decimal ShiftAnnually { get; set; }
        public decimal SpecialAnnually { get; set; }
        public decimal GrossAnnually { get; set; }
        public decimal InsuranceAnnually { get; set; }
        public decimal PFAnnually { get; set; }
        public decimal GratuityAnnually { get; set; }
        public decimal CTCAnnually { get; set; }
        public decimal FoodAnnually { get; set; }
    }

    public class CalculatedSalaryBreakupDetail
    {
        public string ComponentId { set; get; }
        public string ComponentName { set; get; }
        public string Formula { set; get; }
        public decimal FinalAmount { set; get; }
        public decimal ComponentTypeId { set; get; }
    }
}
