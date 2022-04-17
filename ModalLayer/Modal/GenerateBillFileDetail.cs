namespace ModalLayer.Modal
{
    public class GenerateBillFileDetail
    {
        public long ClientId { get; set; }
        public long EmployeeId { set; get; }
        public string FileExtension { get; set; }
        public long FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
    }
}
