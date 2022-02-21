using System;
using System.Collections.Generic;
using System.Text;

namespace MultiTypeDocumentConverter.Service
{
    public interface IDocumentConverter
    {
        string PdfToHtml();
        string DocToHtml();
    }
}
