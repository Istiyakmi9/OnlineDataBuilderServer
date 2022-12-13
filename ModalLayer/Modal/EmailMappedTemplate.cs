namespace ModalLayer.Modal
{
    public class EmailMappedTemplate
    {
        public int EmailTempMappingId {set; get;}
        public int RequestType {set; get;}
        public int EmailTemplateId {set; get;}
        public int CompanyId { get; set; }
        public string Description { set; get; }
    }
}
