using Aspose.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MultiTypeDocumentConverter.Service
{
    public class DocumentConverter : IDocumentConverter
    {
        private string basePath = @"E:\Workspace";
        public string PdfToHtml()
        {
            // Load source PDF file
            Document doc = new Document(Path.Combine(basePath, "ABHINEET_VERMA_Resume.pdf"));
            // Instantiate HTML Save options object
            HtmlSaveOptions newOptions = new HtmlSaveOptions();

            // Enable option to embed all resources inside the HTML
            newOptions.PartsEmbeddingMode = HtmlSaveOptions.PartsEmbeddingModes.EmbedAllIntoHtml;

            // This is just optimization for IE and can be omitted 
            newOptions.LettersPositioningMethod = HtmlSaveOptions.LettersPositioningMethods.UseEmUnitsAndCompensationOfRoundingErrorsInCss;
            newOptions.RasterImagesSavingMode = HtmlSaveOptions.RasterImagesSavingModes.AsEmbeddedPartsOfPngPageBackground;
            newOptions.FontSavingMode = HtmlSaveOptions.FontSavingModes.SaveInAllFormats;
            // Output file path 
            string outHtmlFile = Path.Combine(basePath, "output.html");
            doc.Save(outHtmlFile, newOptions);
            return null;
        }

        public string DocToHtml()
        {
            return null;
        }
    }
}
