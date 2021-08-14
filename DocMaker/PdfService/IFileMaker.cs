using ModalLayer.Modal;
using System.IO;

namespace DocMaker.PdfService
{
    public interface IFileMaker
    {
        bool BuildPdfBill(BuildPdfTable _buildPdfTable, PdfModal pdfModal);
    }

}
