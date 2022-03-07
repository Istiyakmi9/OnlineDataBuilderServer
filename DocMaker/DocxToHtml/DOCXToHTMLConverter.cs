using HtmlService;
using System;
using System.IO;

namespace DocMaker.HtmlToDocx
{
    public class DOCXToHTMLConverter : IDOCXToHTMLConverter
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly HtmlConverterService _htmlConverterService;

        public DOCXToHTMLConverter(HtmlConverterService htmlConverterService)
        {
            _htmlConverterService = htmlConverterService;
        }

        public string ToHtml(FileDetail fileDetail)
        {
            string html = string.Empty;
            try
            {
                string from = fileDetail.FilePath;
                string to = "";
                html = _htmlConverterService.ToHtml(from, to);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return html;
        }
    }
}
