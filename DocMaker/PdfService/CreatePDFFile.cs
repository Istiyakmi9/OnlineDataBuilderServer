using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using ModalLayer.Modal;
using System;
using System.IO;
using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using System.Data;

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

        private PdfPTable CreatePdfTable(Table table)
        {
            int TotalColumns = table.columnCount;
            if (TotalColumns <= 0)
                TotalColumns = 0;
            PdfPTable pdfTable = new PdfPTable(TotalColumns);
            pdfTable.WidthPercentage = 85;
            pdfTable.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
            if (table.isBorder)
            {
                pdfTable.DefaultCell.BorderWidth = 1;
                pdfTable.DefaultCell.Border = 1;
                pdfTable.DefaultCell.BorderColor = WebColors.GetRGBColor("#d9d9d9");
            }
            else
            {
                pdfTable.DefaultCell.BorderWidth = 1;
                pdfTable.DefaultCell.Border = 1;
            }

            if (table.spaceAfter > 0)
                pdfTable.SpacingAfter = table.spaceAfter;

            if (table.spaceBefore > 0)
                pdfTable.SpacingAfter = table.spaceBefore;

            PdfPCell cell = null;
            foreach (var rows in table.rows)
            {
                foreach (var col in rows.columns)
                {
                    if (!string.IsNullOrEmpty(col.img))
                    {
                        iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(
                                                        Path.Combine(Directory.GetCurrentDirectory(), "Documents", col.img)
                                                    );
                        png.ScaleToFit(200f, 150f);
                        png.SpacingBefore = 10f;
                        png.SpacingAfter = 15f;
                        png.Alignment = Element.ALIGN_CENTER;

                        cell = new PdfPCell(png);
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        if (col.colSpan > 0)
                            cell.Colspan = col.colSpan;
                        if (col.rowSpan > 0)
                            cell.Rowspan = col.rowSpan;
                        if (table.isBorder)
                            cell.Border = 1;
                        else
                            cell.Border = 0;

                        pdfTable.AddCell(cell);
                    }
                    else
                    {
                        Phrase phrase = new Phrase();
                        foreach (var elem in col.text)
                        {
                            Chunk chunk = new Chunk(elem.text + "\n", FontFactory.GetFont("Time New Roman"));
                            chunk.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
                            if (elem.isBold)
                                chunk.Font.SetStyle(1);
                            if (elem.fontSize > 0)
                                chunk.Font.Size = elem.fontSize;
                            if (!string.IsNullOrEmpty(elem.color))
                                chunk.Font.Color = WebColors.GetRGBColor(elem.color);

                            if (elem.fontSize > 0)
                                chunk.Font.Size = elem.fontSize;
                            else
                                chunk.Font.Size = DefaultFontSize;
                            phrase.Add(chunk);
                        }

                        cell = new PdfPCell(phrase);
                        if (col.colSpan > 0)
                            cell.Colspan = col.colSpan;
                        if (col.rowSpan > 0)
                            cell.Rowspan = col.rowSpan;

                        if (!table.isBorder)
                            cell.Border = 0;
                        else
                        {
                            cell.BorderColor = WebColors.GetRGBColor("#b8b2b2");
                        }

                        if (col.removeBorder)
                            cell.Border = 0;

                        if (col.paddingLeft > 0)
                            cell.PaddingLeft = col.paddingLeft;
                        if (col.paddingTop > 0)
                            cell.PaddingTop = col.paddingTop;
                        if (col.paddingRight > 0)
                            cell.PaddingRight = col.paddingRight;
                        if (col.paddingBottom > 0)
                            cell.PaddingBottom = col.paddingBottom;

                        switch (col.align)
                        {
                            case "c":
                                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                break;
                            case "r":
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                break;
                            default:
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                break;
                        }
                        pdfTable.AddCell(cell);
                    }
                }
            }

            return pdfTable;
        }

        public FileDetail BuildPdfBill(BuildPdfTable _buildPdfTable, PdfModal pdfModal)
        {
            FileDetail fileDetail = new FileDetail();
            fileDetail.Status = "Client not selected";
            if (pdfModal.ClientId > 0)
            {
                try
                {
                    _buildPdfTable = _pdfGenerateHelper.MapUserDetail(_buildPdfTable, pdfModal);
                    string MonthName = pdfModal.billingMonth.ToString("MMMM_yyyy");

                    string FolderLocation = Path.Combine("Documents", "Bills", MonthName);
                    string OldFileName = pdfModal.developerName.Replace(" ", "_") + "_" +
                                      MonthName + "_" +
                                      pdfModal.billNo.Replace("#", "") + "_" + pdfModal.UpdateSeqNo + ".pdf";

                    pdfModal.UpdateSeqNo++;
                    string FileName = pdfModal.developerName.Replace(" ", "_") + "_" +
                                      MonthName + "_" +
                                      pdfModal.billNo.Replace("#", "") + "_" + pdfModal.UpdateSeqNo + ".pdf";
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), FolderLocation);
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    string OldphysicalPath = Path.Combine(
                                            folderPath,
                                            OldFileName
                                    );

                    string physicalPath = Path.Combine(
                                            folderPath,
                                            FileName
                                    );

                    if (File.Exists(OldphysicalPath))
                        File.Delete(OldphysicalPath);

                    fileDetail.FilePath = FolderLocation;
                    fileDetail.FileName = FileName;
                    fileDetail.FileExtension = "pdf";
                    if (pdfModal.FileId > 0)
                        fileDetail.FileId = pdfModal.FileId;
                    else
                        fileDetail.FileId = -1;
                    fileDetail.ClientId = pdfModal.ClientId;
                    fileDetail.StatusId = 2;
                    fileDetail.PaidOn = null;
                    using (FileStream stream = new FileStream(physicalPath, FileMode.Create))
                    {
                        Document pdfDoc = new Document(PageSize.A4, 5f, 5f, 60f, 0f);
                        PdfWriter.GetInstance(pdfDoc, stream);
                        pdfDoc.Open();

                        PTables _pTable = null;
                        _buildPdfTable.tables.TryGetValue("headerIcon", out _pTable);
                        pdfDoc.Add(CreatePdfTable(_pTable.table));

                        _buildPdfTable.tables.TryGetValue("fromAddress", out _pTable);
                        pdfDoc.Add(CreatePdfTable(_pTable.table));

                        _buildPdfTable.tables.TryGetValue("fromBriefAccountDetail", out _pTable);
                        pdfDoc.Add(CreatePdfTable(_pTable.table));

                        _buildPdfTable.tables.TryGetValue("toAddress", out _pTable);
                        pdfDoc.Add(CreatePdfTable(_pTable.table));

                        _buildPdfTable.tables.TryGetValue("instruction", out _pTable);
                        pdfDoc.Add(CreatePdfTable(_pTable.table));

                        _buildPdfTable.tables.TryGetValue("billingTable", out _pTable);
                        pdfDoc.Add(CreatePdfTable(_pTable.table));

                        _buildPdfTable.tables.TryGetValue("completeBankDetail", out _pTable);
                        pdfDoc.Add(CreatePdfTable(_pTable.table));

                        _buildPdfTable.tables.TryGetValue("finalInstruction", out _pTable);
                        pdfDoc.Add(CreatePdfTable(_pTable.table));

                        _buildPdfTable.tables.TryGetValue("thanking", out _pTable);
                        pdfDoc.Add(CreatePdfTable(_pTable.table));

                        //foreach (var item in _buildPdfTable.tables)
                        //    pdfDoc.Add(CreatePdfTable(item.Value.table));

                        pdfDoc.NewPage();
                        pdfDoc.Close();
                        stream.Close();
                        fileDetail.Status = "Generated";
                    }
                }
                catch (Exception)
                {
                    fileDetail.Status = "Got error while create PDF file.";
                }
            }
            return fileDetail;
        }

        #region HTML_TO_PDF_SAMPLE_CODE

        //public MemoryStream GeneratePdf()
        //{
        //    //Dummy data for Invoice (Bill).
        //    MemoryStream memoryStream = new MemoryStream();
        //    //string companyName = "ASPSnippets";
        //    //int orderNo = 2303;
        //    //DataTable dt = new DataTable();
        //    //dt.Columns.AddRange(new DataColumn[5] {
        //    //                new DataColumn("ProductId", typeof(string)),
        //    //                new DataColumn("Product", typeof(string)),
        //    //                new DataColumn("Price", typeof(int)),
        //    //                new DataColumn("Quantity", typeof(int)),
        //    //                new DataColumn("Total", typeof(int))});
        //    //dt.Rows.Add(101, "Sun Glasses", 200, 5, 1000);
        //    //dt.Rows.Add(102, "Jeans", 400, 2, 800);
        //    //dt.Rows.Add(103, "Trousers", 300, 3, 900);
        //    //dt.Rows.Add(104, "Shirts", 550, 2, 1100);

        //    //using (StringWriter sw = new StringWriter())
        //    //{
        //    //    using (HtmlTextWriter hw = new HtmlTextWriter(sw))
        //    //    {
        //    //        StringBuilder sb = new StringBuilder();

        //    //        //Generate Invoice (Bill) Header.
        //    //        sb.Append("<table width='100%' cellspacing='0' cellpadding='2'>");
        //    //        sb.Append("<tr><td align='center' style='background-color: #18B5F0' colspan = '2'><b>Order Sheet</b></td></tr>");
        //    //        sb.Append("<tr><td colspan = '2'></td></tr>");
        //    //        sb.Append("<tr><td><b>Order No: </b>");
        //    //        sb.Append(orderNo);
        //    //        sb.Append("</td><td align = 'right'><b>Date: </b>");
        //    //        sb.Append(DateTime.Now);
        //    //        sb.Append(" </td></tr>");
        //    //        sb.Append("<tr><td colspan = '2'><b>Company Name: </b>");
        //    //        sb.Append(companyName);
        //    //        sb.Append("</td></tr>");
        //    //        sb.Append("</table>");
        //    //        sb.Append("<br />");

        //    //        //Generate Invoice (Bill) Items Grid.
        //    //        sb.Append("<table border = '1'>");
        //    //        sb.Append("<tr>");
        //    //        foreach (DataColumn column in dt.Columns)
        //    //        {
        //    //            sb.Append("<th style = 'background-color: #D20B0C;color:#ffffff'>");
        //    //            sb.Append(column.ColumnName);
        //    //            sb.Append("</th>");
        //    //        }
        //    //        sb.Append("</tr>");
        //    //        foreach (DataRow row in dt.Rows)
        //    //        {
        //    //            sb.Append("<tr>");
        //    //            foreach (DataColumn column in dt.Columns)
        //    //            {
        //    //                sb.Append("<td>");
        //    //                sb.Append(row[column]);
        //    //                sb.Append("</td>");
        //    //            }
        //    //            sb.Append("</tr>");
        //    //        }
        //    //        sb.Append("<tr><td align = 'right' colspan = '");
        //    //        sb.Append(dt.Columns.Count - 1);
        //    //        sb.Append("'>Total</td>");
        //    //        sb.Append("<td>");
        //    //        sb.Append(dt.Compute("sum(Total)", ""));
        //    //        sb.Append("</td>");
        //    //        sb.Append("</tr></table>");

        //    //        //Export HTML String as PDF.
        //    //        StringReader sr = new StringReader(sb.ToString());
        //    //        Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);


        //    //        //HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
        //    //        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
        //    //        pdfDoc.Open();

        //    //        XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);


        //    //        //htmlparser.Parse(sr);

        //    //        pdfDoc.Close();

        //    //        //Response.ContentType = "application/pdf";
        //    //        //Response.AddHeader("content-disposition", "attachment;filename=Invoice_" + orderNo + ".pdf");
        //    //        //Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //    //        //Response.Write(pdfDoc);
        //    //        //Response.End();
        //    //    }
        //    //}
        //    return memoryStream;
        //}

        #endregion
    }
}