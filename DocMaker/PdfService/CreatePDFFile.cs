namespace DocMaker.PdfService
{
    public class CreatePDFFile : IFileMaker
    {
        public FileDetail _fileDetail { set; get; }
        public FileDetail SetFileDetail
        {
            set { _fileDetail = value; }
        }
    }
}