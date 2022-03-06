using ModalLayer.Modal;
using System.IO;

namespace DocMaker.PdfService
{
    public interface IFileMaker
    {
        FileDetail _fileDetail { set; get; }
        void BuildPdfBill(BuildPdfTable _buildPdfTable, PdfModal pdfModal, Organization sender);
        void ConvertToPDF(string html, string path);
    }
}
