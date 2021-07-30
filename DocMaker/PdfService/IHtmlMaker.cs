using System.IO;

namespace DocMaker.PdfService
{
    public interface IHtmlMaker
    {
        void DocxFileToHtml(string FilePath, string SaveLocation);
        void HtmlToDocxFile(string FilePath, string SaveLocation);
    }
}
