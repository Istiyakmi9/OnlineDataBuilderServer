using System.IO;

namespace DocMaker.PdfService
{
    public interface IFileMaker
    {
        MemoryStream GeneratePdf();
        bool TextSharpGeneratePdf();
    }
    
}
