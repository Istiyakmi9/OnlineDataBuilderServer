using System;
using System.Collections.Generic;
using System.Text;

namespace DocMaker.HtmlToDocx
{
    public interface IHTMLConverter
    {
        string ToDocx(string templatePath, string destinationFolder, string headerLogoPath);
        string ToHtml();
    }
}
