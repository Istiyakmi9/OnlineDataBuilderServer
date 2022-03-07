using HtmlService;
using System;
using System.IO;

namespace DocMaker.HtmlToDocx
{
    public class DOCXToHTMLConverter : IDOCXToHTMLConverter
    {
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
                html = _htmlConverterService.ToHtml(fileDetail.FilePath);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return html;
        }
    }
}
