using BottomhalfCore.DatabaseLayer.Common.Code;
//using iTextSharp.text;
//using iTextSharp.text.html;
//using iTextSharp.text.html.simpleparser;
//using iTextSharp.text.pdf;
//using iTextSharp.tool.xml;
using ModalLayer.Modal;
using System;
using System.IO;

namespace DocMaker.PdfService
{
    public class CreatePDFFile : IFileMaker
    {
        private readonly PdfGenerateHelper _pdfGenerateHelper;
        private readonly float DefaultFontSize;
        private readonly IDb _db;
        public CreatePDFFile(PdfGenerateHelper pdfGenerateHelper, IDb db)
        {
            _pdfGenerateHelper = pdfGenerateHelper;
            DefaultFontSize = 9;
            _db = db;
        }

        public FileDetail _fileDetail { set; get; }
        public FileDetail SetFileDetail
        {
            set { _fileDetail = value; }
        }

        //private PdfPTable CreatePdfTable(Table table)
        //{
        //    int TotalColumns = table.columnCount;
        //    if (TotalColumns <= 0)
        //        TotalColumns = 0;
        //    PdfPTable pdfTable = new PdfPTable(TotalColumns);
        //    pdfTable.WidthPercentage = 85;
        //    pdfTable.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
        //    if (table.isBorder)
        //    {
        //        pdfTable.DefaultCell.BorderWidth = 1;
        //        pdfTable.DefaultCell.Border = 1;
        //        pdfTable.DefaultCell.BorderColor = WebColors.GetRGBColor("#d9d9d9");
        //    }
        //    else
        //    {
        //        pdfTable.DefaultCell.BorderWidth = 1;
        //        pdfTable.DefaultCell.Border = 1;
        //    }

        //    if (table.spaceAfter > 0)
        //        pdfTable.SpacingAfter = table.spaceAfter;

        //    if (table.spaceBefore > 0)
        //        pdfTable.SpacingAfter = table.spaceBefore;

        //    PdfPCell cell = null;
        //    foreach (var rows in table.rows)
        //    {
        //        foreach (var col in rows.columns)
        //        {
        //            if (!string.IsNullOrEmpty(col.img))
        //            {
        //                iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(_fileDetail.LogoPath);
        //                png.ScaleToFit(200f, 150f);
        //                png.SpacingBefore = 10f;
        //                png.SpacingAfter = 15f;
        //                png.Alignment = Element.ALIGN_CENTER;

        //                cell = new PdfPCell(png);
        //                cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //                if (col.colSpan > 0)
        //                    cell.Colspan = col.colSpan;
        //                if (col.rowSpan > 0)
        //                    cell.Rowspan = col.rowSpan;
        //                if (table.isBorder)
        //                    cell.Border = 1;
        //                else
        //                    cell.Border = 0;

        //                pdfTable.AddCell(cell);
        //            }
        //            else
        //            {
        //                Phrase phrase = new Phrase();
        //                foreach (var elem in col.text)
        //                {
        //                    Chunk chunk = new Chunk(elem.text + "\n", FontFactory.GetFont("Time New Roman"));
        //                    chunk.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
        //                    if (elem.isBold)
        //                        chunk.Font.SetStyle(1);
        //                    if (elem.fontSize > 0)
        //                        chunk.Font.Size = elem.fontSize;
        //                    if (!string.IsNullOrEmpty(elem.color))
        //                        chunk.Font.Color = WebColors.GetRGBColor(elem.color);

        //                    if (elem.fontSize > 0)
        //                        chunk.Font.Size = elem.fontSize;
        //                    else
        //                        chunk.Font.Size = DefaultFontSize;
        //                    phrase.Add(chunk);
        //                }

        //                cell = new PdfPCell(phrase);
        //                if (col.colSpan > 0)
        //                    cell.Colspan = col.colSpan;
        //                if (col.rowSpan > 0)
        //                    cell.Rowspan = col.rowSpan;

        //                if (!table.isBorder)
        //                    cell.Border = 0;
        //                else
        //                {
        //                    cell.BorderColor = WebColors.GetRGBColor("#b8b2b2");
        //                }

        //                if (col.removeBorder)
        //                    cell.Border = 0;

        //                if (col.paddingLeft > 0)
        //                    cell.PaddingLeft = col.paddingLeft;
        //                if (col.paddingTop > 0)
        //                    cell.PaddingTop = col.paddingTop;
        //                if (col.paddingRight > 0)
        //                    cell.PaddingRight = col.paddingRight;
        //                if (col.paddingBottom > 0)
        //                    cell.PaddingBottom = col.paddingBottom;

        //                switch (col.align)
        //                {
        //                    case "c":
        //                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //                        break;
        //                    case "r":
        //                        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //                        break;
        //                    default:
        //                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //                        break;
        //                }
        //                pdfTable.AddCell(cell);
        //            }
        //        }
        //    }

        //    return pdfTable;
        //}

        public void ConvertToPDF(string html, string path)
        {
            path = @"E:\projects\OnlineDataBuilderServer\OnlineDataBuilder\Documents\User\test.pdf";
            //StringReader sr = new StringReader(html);
            //Document pdfDoc = new Document(PageSize.A4, 20f, 20f, 40f, 0f);
            //using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
            //{
            //    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
            //    pdfDoc.Open();

            //    XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
            //    pdfDoc.Close();

            //    pdfDoc.NewPage();
            //    pdfDoc.Close();
            //    stream.Close();
            //}
        }

        public void BuildPdfBill(BuildPdfTable _buildPdfTable, PdfModal pdfModal, Organization sender)
        {
            //string physicalPath = Path.Combine(_fileDetail.DiskFilePath, _fileDetail.FileName + $".{ApplicationConstants.Pdf}");
            //_fileDetail.Status = 0;
            //if (pdfModal.ClientId > 0)
            //{
            //    try
            //    {
            //        _buildPdfTable = _pdfGenerateHelper.MapUserDetail(_buildPdfTable, pdfModal, sender);
            //        using (FileStream stream = new FileStream(physicalPath, FileMode.Create))
            //        {
            //            Document pdfDoc = new Document(PageSize.A4, 5f, 5f, 60f, 0f);
            //            PdfWriter.GetInstance(pdfDoc, stream);
            //            pdfDoc.Open();

            //            PTables _pTable = null;
            //            _buildPdfTable.tables.TryGetValue("headerIcon", out _pTable);
            //            pdfDoc.Add(CreatePdfTable(_pTable.table));

            //            _buildPdfTable.tables.TryGetValue("fromAddress", out _pTable);
            //            pdfDoc.Add(CreatePdfTable(_pTable.table));

            //            //_buildPdfTable.tables.TryGetValue("fromBriefAccountDetail", out _pTable);
            //            //pdfDoc.Add(CreatePdfTable(_pTable.table));

            //            _buildPdfTable.tables.TryGetValue("toAddress", out _pTable);
            //            pdfDoc.Add(CreatePdfTable(_pTable.table));

            //            _buildPdfTable.tables.TryGetValue("instruction", out _pTable);
            //            pdfDoc.Add(CreatePdfTable(_pTable.table));

            //            _buildPdfTable.tables.TryGetValue("billingTable", out _pTable);
            //            pdfDoc.Add(CreatePdfTable(_pTable.table));

            //            _buildPdfTable.tables.TryGetValue("completeBankDetail", out _pTable);
            //            pdfDoc.Add(CreatePdfTable(_pTable.table));

            //            _buildPdfTable.tables.TryGetValue("finalInstruction", out _pTable);
            //            pdfDoc.Add(CreatePdfTable(_pTable.table));

            //            _buildPdfTable.tables.TryGetValue("thanking", out _pTable);
            //            pdfDoc.Add(CreatePdfTable(_pTable.table));

            //            //foreach (var item in _buildPdfTable.tables)
            //            //    pdfDoc.Add(CreatePdfTable(item.Value.table));

            //            pdfDoc.NewPage();
            //            pdfDoc.Close();
            //            stream.Close();
            //            _fileDetail.Status = 1;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        _fileDetail.Status = -1;
            //        throw ex;
            //    }
            //}
        }

        public bool GeneratePdfUsingHtml(string htmlCode, string filePath)
        {
            throw new NotImplementedException();
        }

        #region HTML_TO_PDF_SAMPLE_CODE

        //public bool GeneratePdfUsingHtml(string htmlCode, string filePath)
        //{
        //    bool statusFlag = false;
        //    //Dummy data for Invoice (Bill).
        //    using (FileStream stream = new FileStream(filePath, FileMode.Create))
        //    {
        //        //Export HTML String as PDF.
        //        StringReader sr = new StringReader(htmlCode);
        //        Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);

        //        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
        //        pdfDoc.Open();

        //        XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);

        //        pdfDoc.Close();

        //        //Response.ContentType = "application/pdf";
        //        //Response.AddHeader("content-disposition", "attachment;filename=Invoice_" + orderNo + ".pdf");
        //        //Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //        //Response.Write(pdfDoc);
        //        //Response.End();
        //    }
        //    return statusFlag;
        //}

        #endregion
    }
}