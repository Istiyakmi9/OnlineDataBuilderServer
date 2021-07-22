using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocMaker.PdfService
{
    public interface IFileMake
    {
        MemoryStream GeneratePdf();
    }
}
