using Spire.Doc;
using Spire.Doc.Documents;
using System.IO;

namespace DocMaker.PdfService
{
    public class ToHtml : IHtmlMaker
    {
        public void DocxFileToHtml(string FilePath, string SaveLocation)
        {
            string FileName = Path.GetRandomFileName();

            string inpFile = Path.GetFullPath(FilePath);
            string outFile = Path.GetFullPath(Path.Combine(SaveLocation, FileName.Replace(".", "") + ".html"));

            //Create word document
            Document document = new Document();
            document.LoadFromFile(inpFile);

            //Save doc file to html
            document.SaveToFile(outFile, FileFormat.Html);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(outFile) { UseShellExecute = true });
        }

        public void HtmlToDocxFile(string FilePath, string SaveLocation)
        {
            string FileName = Path.GetRandomFileName();

            string inpFile = Path.GetFullPath(FilePath);
            string outFile = Path.GetFullPath(Path.Combine(SaveLocation, FileName.Replace(".", "") + ".docx"));

            //Create word document
            Document document = new Document();
            document.LoadFromFile(inpFile, FileFormat.Html, XHTMLValidationType.None);

            //Save doc file to html
            document.SaveToFile(outFile, FileFormat.Docx);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(outFile) { UseShellExecute = true });
        }
    }
}
