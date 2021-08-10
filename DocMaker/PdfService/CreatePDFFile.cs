using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace DocMaker.PdfService
{
    public class CreatePDFFile : IFileMaker
    {
        public bool TextSharpGeneratePdf()
        {
            try
            {
                //region Common Part
                PdfPTable pdfTableBlank = new PdfPTable(1);

                //Footer Section
                PdfPTable pdfTableFooter = new PdfPTable(1);
                pdfTableFooter.DefaultCell.BorderWidth = 0;
                pdfTableFooter.WidthPercentage = 100;
                pdfTableFooter.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;

                Chunk cnkFooter = new Chunk("@ Marghub", FontFactory.GetFont("Arial", 14));
                //cnkFooter.Font.Size = 12;
                pdfTableFooter.AddCell(new Phrase(cnkFooter));
                //End of Footer Section

                pdfTableBlank.AddCell(new Phrase(" "));
                pdfTableBlank.DefaultCell.Border = 0;
                //end region

                //region Page
                //region for Header
                PdfPTable pdfPTable1 = new PdfPTable(1);
                PdfPTable pdfPTable2 = new PdfPTable(1);
                PdfPTable pdfPTable3 = new PdfPTable(2);
                PdfPTable pdfPTable5 = new PdfPTable(1);
                PdfPTable pdfPTable6 = new PdfPTable(1);
                PdfPTable pdfPTable7 = new PdfPTable(1);

                //Font style
                System.Drawing.Font fontH1 = new System.Drawing.Font("Currier", 16);

                //PdfTable
                pdfPTable1.WidthPercentage = 80;
                pdfPTable1.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfPTable1.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfPTable1.DefaultCell.BorderWidth = 0;

                pdfPTable2.WidthPercentage = 80;
                pdfPTable2.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfPTable2.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfPTable2.DefaultCell.BorderWidth = 0;

                pdfPTable5.WidthPercentage = 80;
                pdfPTable5.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfPTable5.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfPTable5.DefaultCell.BorderWidth = 0;

                pdfPTable3.DefaultCell.Padding = 5;
                pdfPTable3.WidthPercentage = 80;
                pdfPTable3.DefaultCell.BorderWidth = 0.5f;

                pdfPTable6.WidthPercentage = 80;
                pdfPTable6.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
                pdfPTable6.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfPTable6.DefaultCell.BorderWidth = 0;
                pdfPTable6.DefaultCell.PaddingBottom = 5;

                pdfPTable7.WidthPercentage = 80;
                pdfPTable7.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
                pdfPTable7.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfPTable7.DefaultCell.BorderWidth = 0;
                pdfPTable7.DefaultCell.PaddingBottom = 5;

                Chunk c1 = new Chunk("DEMO Enterprise", FontFactory.GetFont("Time New Roman"));
                c1.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
                c1.Font.SetStyle(0);
                c1.Font.Size = 14;
                Phrase p1 = new Phrase();
                p1.Add(c1);
                pdfPTable1.AddCell(p1);

                Chunk c2 = new Chunk("18/3, ABC Narayana XYZ, Asansol - 713302", FontFactory.GetFont("Time New Roman"));
                c2.Font.Size = 14;
                c2.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
                c2.Font.SetStyle(0);
                Phrase p2 = new Phrase();
                p2.Add(c2);
                pdfPTable2.AddCell(p2);

                Chunk c3 = new Chunk("Customer Care: 0341-12345, 8798000000", FontFactory.GetFont("Time New Roman"));
                c3.Font.Size = 14;
                c3.Font.SetStyle(0);
                c3.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
                Phrase p3 = new Phrase();
                p3.Add(c3);
                pdfPTable5.AddCell(p3);

                Chunk c6 = new Chunk("Company Details", FontFactory.GetFont("Time New Roman", 14));
                c6.Font.SetStyle(0);
                c6.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
                Phrase p6 = new Phrase();
                p6.Add(c6);
                pdfPTable6.AddCell(p6);

                Chunk c7 = new Chunk("Bill Details", FontFactory.GetFont("Time New Roman", 14));
                c7.Font.SetStyle(0);
                c7.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
                Phrase p7 = new Phrase();
                p7.Add(c7);
                pdfPTable7.AddCell(p7);

                //End Region
                // Section-1 Upper Part
                PdfPTable pdfPTable4 = new PdfPTable(2);
                pdfPTable4.DefaultCell.Padding = 5;
                pdfPTable4.WidthPercentage = 80;
                pdfPTable4.DefaultCell.BorderWidth = 0.5f;

                pdfPTable4.AddCell(new Phrase("Bill No."));
                pdfPTable4.AddCell(new Phrase("B001"));
                pdfPTable4.AddCell(new Phrase("Date"));
                pdfPTable4.AddCell(new Phrase("01-01-2021"));
                pdfPTable4.AddCell(new Phrase("Vendor"));
                pdfPTable4.AddCell(new Phrase("Demo Vendor"));
                pdfPTable4.AddCell(new Phrase("Address"));
                pdfPTable4.AddCell(new Phrase("Asansol"));
                //End Region

                //Image Section
                string imageURL = @"C:\Users\botto\Downloads\logo.png";
                iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(imageURL);

                //imgae  size
                png.ScaleToFit(140f, 120f);
                //Give space before image
                png.SpacingBefore = 10f;
                //Give Space after image
                png.SpacingAfter = 1f;

                png.Alignment = Element.ALIGN_CENTER;
                //End Region

                //Section Table Region
                pdfPTable3.AddCell(new Phrase("COMPANY NAME"));
                pdfPTable3.AddCell(new Phrase(""));
                pdfPTable3.AddCell(new Phrase("JOB TITLE"));
                pdfPTable3.AddCell(new Phrase(""));

                pdfPTable3.AddCell(new Phrase("ADDRESS"));
                pdfPTable3.AddCell(new Phrase(""));
                pdfPTable3.AddCell(new Phrase("CONTACT NO"));
                pdfPTable3.AddCell(new Phrase(""));
                //End Region

                //For PDF Generation
                string folderPath = "E:\\Workspace\\";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                //File Name
                int fileCount = Directory.GetFiles(@"E:\\Workspace\\").Length;
                string strFileName = "JobDescriptionForm" + (fileCount + 1) + ".pdf";

                using (FileStream stream = new FileStream(folderPath + strFileName, FileMode.Create))
                {
                    Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                    PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();

                    //region Page-1
                    pdfDoc.Add(pdfPTable1);
                    pdfDoc.Add(pdfPTable2);
                    pdfDoc.Add(pdfPTable5);
                    pdfDoc.Add(pdfTableBlank);
                    pdfDoc.Add(png);
                    pdfDoc.Add(pdfPTable6);
                    pdfDoc.Add(pdfPTable3);
                    pdfDoc.Add(pdfPTable7);
                    pdfDoc.Add(pdfPTable4);
                    pdfDoc.Add(pdfTableFooter);
                    pdfDoc.NewPage();
                    //End Region

                    //pdfDoc.Add(pdfPTable2);
                    pdfDoc.Close();
                    stream.Close();
                }

                //Display PDF
                System.Diagnostics.Process.Start(folderPath + "\\" + strFileName);


            }
            catch (Exception ex)
            {
                throw;
            }
            return true;
        }
        public MemoryStream GeneratePdf()
        {
            //Dummy data for Invoice (Bill).
            MemoryStream memoryStream = new MemoryStream();
            //string companyName = "ASPSnippets";
            //int orderNo = 2303;
            //DataTable dt = new DataTable();
            //dt.Columns.AddRange(new DataColumn[5] {
            //                new DataColumn("ProductId", typeof(string)),
            //                new DataColumn("Product", typeof(string)),
            //                new DataColumn("Price", typeof(int)),
            //                new DataColumn("Quantity", typeof(int)),
            //                new DataColumn("Total", typeof(int))});
            //dt.Rows.Add(101, "Sun Glasses", 200, 5, 1000);
            //dt.Rows.Add(102, "Jeans", 400, 2, 800);
            //dt.Rows.Add(103, "Trousers", 300, 3, 900);
            //dt.Rows.Add(104, "Shirts", 550, 2, 1100);

            //using (StringWriter sw = new StringWriter())
            //{
            //    using (HtmlTextWriter hw = new HtmlTextWriter(sw))
            //    {
            //        StringBuilder sb = new StringBuilder();

            //        //Generate Invoice (Bill) Header.
            //        sb.Append("<table width='100%' cellspacing='0' cellpadding='2'>");
            //        sb.Append("<tr><td align='center' style='background-color: #18B5F0' colspan = '2'><b>Order Sheet</b></td></tr>");
            //        sb.Append("<tr><td colspan = '2'></td></tr>");
            //        sb.Append("<tr><td><b>Order No: </b>");
            //        sb.Append(orderNo);
            //        sb.Append("</td><td align = 'right'><b>Date: </b>");
            //        sb.Append(DateTime.Now);
            //        sb.Append(" </td></tr>");
            //        sb.Append("<tr><td colspan = '2'><b>Company Name: </b>");
            //        sb.Append(companyName);
            //        sb.Append("</td></tr>");
            //        sb.Append("</table>");
            //        sb.Append("<br />");

            //        //Generate Invoice (Bill) Items Grid.
            //        sb.Append("<table border = '1'>");
            //        sb.Append("<tr>");
            //        foreach (DataColumn column in dt.Columns)
            //        {
            //            sb.Append("<th style = 'background-color: #D20B0C;color:#ffffff'>");
            //            sb.Append(column.ColumnName);
            //            sb.Append("</th>");
            //        }
            //        sb.Append("</tr>");
            //        foreach (DataRow row in dt.Rows)
            //        {
            //            sb.Append("<tr>");
            //            foreach (DataColumn column in dt.Columns)
            //            {
            //                sb.Append("<td>");
            //                sb.Append(row[column]);
            //                sb.Append("</td>");
            //            }
            //            sb.Append("</tr>");
            //        }
            //        sb.Append("<tr><td align = 'right' colspan = '");
            //        sb.Append(dt.Columns.Count - 1);
            //        sb.Append("'>Total</td>");
            //        sb.Append("<td>");
            //        sb.Append(dt.Compute("sum(Total)", ""));
            //        sb.Append("</td>");
            //        sb.Append("</tr></table>");

            //        //Export HTML String as PDF.
            //        StringReader sr = new StringReader(sb.ToString());
            //        Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);


            //        //HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
            //        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
            //        pdfDoc.Open();

            //        XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);


            //        //htmlparser.Parse(sr);

            //        pdfDoc.Close();

            //        //Response.ContentType = "application/pdf";
            //        //Response.AddHeader("content-disposition", "attachment;filename=Invoice_" + orderNo + ".pdf");
            //        //Response.Cache.SetCacheability(HttpCacheability.NoCache);
            //        //Response.Write(pdfDoc);
            //        //Response.End();
            //    }
            //}
            return memoryStream;
        }

        

     
    }
}