using ModalLayer.Modal;
using System.IO;

namespace DocMaker.PdfService
{
    public interface IFileMaker
    {
        FileDetail _fileDetail { set; get; }
        void BuildPdfBill(BuildPdfTable _buildPdfTable, PdfModal pdfModal, Organization sender);
        bool GeneratePdfUsingHtml(string htmlCode, string filePath);
        void ConvertToPDF(string html, string path);
    }
}
