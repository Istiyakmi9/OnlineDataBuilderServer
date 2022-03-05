using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlService;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace DocMaker.HtmlToDocx
{
    public class HTMLConverter : IHTMLConverter
    {
        public string ToDocx(string html, string destinationFolder, string headerLogoPath)
        {
            string status = string.Empty;
            if (!string.IsNullOrEmpty(html))
            {
                //using (FileStream stream = File.Open(templatePath, FileMode.Open))
                //{
                //    StreamReader reader = new StreamReader(stream);
                //    string html = reader.ReadToEnd();

                if (File.Exists(destinationFolder)) File.Delete(destinationFolder);

                using (MemoryStream generatedDocument = new MemoryStream())
                {
                    using (WordprocessingDocument package = WordprocessingDocument.Create(generatedDocument, WordprocessingDocumentType.Document))
                    {
                        MainDocumentPart mainPart = package.MainDocumentPart;
                        if (mainPart == null)
                        {
                            mainPart = package.AddMainDocumentPart();
                            new Document(new Body()).Save(mainPart);
                        }

                        HtmlToOpenXml.HtmlConverter converter = new HtmlToOpenXml.HtmlConverter(mainPart);
                        if (File.Exists(headerLogoPath))
                            converter.ProcessHeaderImage(mainPart, headerLogoPath);
                        converter.ParseHtml(html);

                        mainPart.Document.Save();
                    }

                    File.WriteAllBytes(destinationFolder, generatedDocument.ToArray());
                }
            }
            else
                status = "Template path not found";

            return status;
        }


        public string ToHtml()
        {
            try
            {
                HtmlConverterService htmlConverterService = new HtmlConverterService();
                string from = @"E:\test\misba.docx";
                string to = @"E:\test";
                htmlConverterService.ToHtml(from, to);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }
    }
}
