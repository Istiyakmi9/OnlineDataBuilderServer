using HtmlService;
using Microsoft.Extensions.Logging;
using System;

namespace DocMaker.HtmlToDocx
{
    public class DOCXToHTMLConverter : IDOCXToHTMLConverter
    {
        private readonly HtmlConverterService _htmlConverterService;
        private readonly ILogger<DOCXToHTMLConverter> _logger;

        public DOCXToHTMLConverter(HtmlConverterService htmlConverterService, ILogger<DOCXToHTMLConverter> logger)
        {
            _htmlConverterService = htmlConverterService;
            _logger = logger;
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
                _logger.LogError($"[DOCXToHTMLConverter] Error message {ex.Message}. Inner exception: {ex.InnerException?.Message}");
                throw ex;
            }

            return html;
        }
    }
}
