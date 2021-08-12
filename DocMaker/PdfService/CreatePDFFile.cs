using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace DocMaker.PdfService
{
    public class CreatePDFFile : IFileMaker
    {
        static void Main(string[] args)
        {
            PrintReceipt();
        }

        private static void PrintReceipt()
        {
            try
            {
                //Image Section
                string imageURL = @"C:\Users\botto\Downloads\logo.png";
                iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(imageURL);
                //imgae  size
                png.ScaleToFit(200f, 150f);
                //Give space before image
                png.SpacingBefore = 10f;
                //Give Space after image
                png.SpacingAfter = 15f;
                png.Alignment = Element.ALIGN_CENTER;
                //End Region

                //region Page
                //region for Header
                PdfPTable pdfPTable1 = new PdfPTable(3);
                pdfPTable1.WidthPercentage = 80;
                pdfPTable1.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfPTable1.DefaultCell.BorderWidth = 0;
                Chunk c1 = new Chunk("BottomHalf Pvt. Ltd.", FontFactory.GetFont("Time New Roman"));
                c1.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
                c1.Font.SetStyle(1);
                c1.Font.Size = 15;
                Phrase p1 = new Phrase();
                p1.Add(c1);
                pdfPTable1.AddCell(p1);
                pdfPTable1.AddCell("");
                Chunk c9 = new Chunk("INVOICE", FontFactory.GetFont("Franklin Gothic Demi"));
                c9.Font.Size = 18;
                c9.Font.SetStyle(1);
                Phrase p9 = new Phrase();
                p9.Add(c9);
                pdfPTable1.AddCell(p9);


                PdfPTable pdfPTable2 = new PdfPTable(3);
                pdfPTable2.WidthPercentage = 80;
                pdfPTable2.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfPTable2.DefaultCell.BorderWidth = 0;
                Chunk c2 = new Chunk("Bairagi Talab K.T Road", FontFactory.GetFont("Franklin Gothic Demi"));
                c2.Font.Size = 9;
                c2.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
                c2.Font.SetStyle(0);
                Phrase p2 = new Phrase();
                p2.Add(c2);
                pdfPTable2.AddCell(p2);
                pdfPTable2.AddCell("");
                Chunk c10 = new Chunk("Bill No. #000024", FontFactory.GetFont("Franklin Gothic Demi"));
                c10.Font.Size = 9;
                Phrase p10 = new Phrase();
                p10.Add(c10);
                pdfPTable2.AddCell(p10);


                PdfPTable pdfPTable3 = new PdfPTable(5);
                pdfPTable3.DefaultCell.Padding = 5;
                pdfPTable3.WidthPercentage = 80;
                pdfPTable3.DefaultCell.BorderWidth = 0.5f;
                pdfPTable3.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                Chunk ch1 = new Chunk("S.NO#");
                ch1.Font.SetStyle(1);
                ch1.Font.Size = 9;
                pdfPTable3.AddCell(new Phrase(ch1));
                Chunk ch2 = new Chunk("DEVELOPER");
                ch2.Font.SetStyle(1);
                ch2.Font.Size = 9;
                pdfPTable3.AddCell(new Phrase(ch2));
                Chunk ch3 = new Chunk("MONTH");
                ch3.Font.SetStyle(1);
                ch3.Font.Size = 9;
                pdfPTable3.AddCell(new Phrase(ch3));
                Chunk ch4 = new Chunk("PRICE");
                ch4.Font.SetStyle(1);
                ch4.Font.Size = 9;
                pdfPTable3.AddCell(new Phrase(ch4));
                Chunk ch5 = new Chunk("TOTAL");
                ch5.Font.SetStyle(1);
                ch5.Font.Size = 9;
                pdfPTable3.AddCell(new Phrase(ch5));
                Chunk ch6 = new Chunk("1");
                ch6.Font.Size = 9;
                pdfPTable3.AddCell(new Phrase(ch6));
                Chunk ch7 = new Chunk("ABC");
                ch7.Font.Size = 9;
                pdfPTable3.AddCell(new Phrase(ch7));
                Chunk ch8 = new Chunk("APRIL");
                ch8.Font.Size = 9;
                pdfPTable3.AddCell(new Phrase(ch8));
                Chunk ch9 = new Chunk("000000");
                ch9.Font.Size = 9;
                pdfPTable3.AddCell(new Phrase(ch9));
                Chunk ch10 = new Chunk("0000000");
                ch10.Font.Size = 9;
                pdfPTable3.AddCell(new Phrase(ch10));


                PdfPTable pdfPTable5 = new PdfPTable(1);
                pdfPTable5.WidthPercentage = 80;
                pdfPTable5.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfPTable5.DefaultCell.BorderWidth = 0;
                Chunk c3 = new Chunk("Asansol. Pin: 713302", FontFactory.GetFont("Time New Roman"));
                c3.Font.Size = 9;
                c3.Font.SetStyle(0);
                c3.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
                Phrase p3 = new Phrase();
                p3.Add(c3);
                pdfPTable5.AddCell(p3);


                PdfPTable pdfPTable11 = new PdfPTable(1);
                pdfPTable11.WidthPercentage = 80;
                pdfPTable11.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfPTable11.DefaultCell.BorderWidth = 0;
                Chunk c11 = new Chunk("Phone: +91-9100544384", FontFactory.GetFont("Time New Roman"));
                c11.Font.Size = 9;
                c11.Font.SetStyle(0);
                c11.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
                Phrase p11 = new Phrase();
                p11.Add(c11);
                pdfPTable11.AddCell(p11);

                PdfPTable blank = new PdfPTable(1);
                blank.WidthPercentage = 80;
                blank.DefaultCell.BorderWidth = 0.5f;
                blank.AddCell(new Phrase("HI"));

                //Chunk linebreak = new Chunk(new DottedLineSeparator());

                PdfPTable pdfPTable12 = new PdfPTable(1);
                pdfPTable12.WidthPercentage = 80;
                pdfPTable12.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfPTable12.DefaultCell.BorderWidth = 0;
                Chunk c12 = new Chunk("GSTIN No: - 19AAICB3816G1ZO", FontFactory.GetFont("Time New Roman"));
                c12.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
                c12.Font.SetStyle(1);
                c12.Font.Size = 11;
                Phrase p12 = new Phrase();
                p12.Add(c12);
                pdfPTable12.AddCell(p12);


                PdfPTable pdfPTable13 = new PdfPTable(1);
                pdfPTable13.WidthPercentage = 80;
                pdfPTable13.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfPTable13.DefaultCell.BorderWidth = 0;
                Chunk c13 = new Chunk("ICICI Bank", FontFactory.GetFont("Franklin Gothic Demi"));
                c13.Font.Size = 9;
                c13.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
                c13.Font.SetStyle(0);
                Phrase p13 = new Phrase();
                p13.Add(c13);
                pdfPTable13.AddCell(p13);

                PdfPTable pdfPTable14 = new PdfPTable(3);
                pdfPTable14.WidthPercentage = 80;
                pdfPTable14.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfPTable14.DefaultCell.BorderWidth = 0;
                Chunk c14 = new Chunk("BOTTOMHALF PRIVATE LIMITED", FontFactory.GetFont("Franklin Gothic Demi"));
                c14.Font.Size = 9;
                c14.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
                c14.Font.SetStyle(0);
                Phrase p14 = new Phrase();
                p14.Add(c14);
                pdfPTable14.AddCell(p14);
                pdfPTable14.AddCell("");
                Chunk c15 = new Chunk("INVOICE #000246", FontFactory.GetFont("Franklin Gothic Demi"));
                c15.Font.Size = 9;
                c15.Font.Color = new iTextSharp.text.BaseColor(0, 32, 255);
                Phrase p15 = new Phrase();
                p15.Add(c15);
                pdfPTable14.AddCell(p15);

                PdfPTable pdfPTable16 = new PdfPTable(3);
                pdfPTable16.WidthPercentage = 80;
                pdfPTable16.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfPTable16.DefaultCell.BorderWidth = 0;
                Chunk c16 = new Chunk("Account Number: 029105005572 ", FontFactory.GetFont("Franklin Gothic Demi"));
                c16.Font.Size = 9;
                c16.Font.Color = new iTextSharp.text.BaseColor(0, 0, 0);
                c16.Font.SetStyle(0);
                Phrase p16 = new Phrase();
                p16.Add(c16);
                pdfPTable16.AddCell(p16);
                pdfPTable16.AddCell("");
                Chunk c17 = new Chunk("DATE: 1st AUG, 2021", FontFactory.GetFont("Franklin Gothic Demi", 9));

                c17.Font.Color = new iTextSharp.text.BaseColor(0, 32, 255);
                Phrase p17 = new Phrase();
                p17.Add(c17);
                pdfPTable16.AddCell(p17);

                PdfPTable pdfPTable18 = new PdfPTable(1);
                pdfPTable18.WidthPercentage = 80;
                pdfPTable18.DefaultCell.VerticalAlignment = Element.ALIGN_CENTER;
                pdfPTable18.DefaultCell.BorderWidth = 0;
                Chunk c18 = new Chunk("IFSC: ICIC0000291", FontFactory.GetFont("Verdana"));
                c18.Font.Size = 9;
                c18.Font.Color = new iTextSharp.text.BaseColor(255, 0, 0);
                c18.Font.SetStyle(0);
                Phrase p18 = new Phrase();
                p18.Add(c18);
                pdfPTable18.AddCell(p18);

                PdfPTable pdfPTable19 = new PdfPTable(1);
                pdfPTable19.WidthPercentage = 80;
                pdfPTable19.DefaultCell.BorderWidth = 0;
                Chunk c19 = new Chunk("BILL TO:", FontFactory.GetFont("Arial", 9));
                c19.Font.Color = new iTextSharp.text.BaseColor(0, 32, 255);
                c19.Font.SetStyle(1);
                pdfPTable19.AddCell(new Phrase(c19));

                PdfPTable pdfPTable20 = new PdfPTable(1);
                pdfPTable20.DefaultCell.BorderWidth = 0;
                pdfPTable20.WidthPercentage = 80;
                Chunk c20 = new Chunk("Charter Global Pvt. Ltd", FontFactory.GetFont("Arial", 12));
                c20.Font.SetStyle(1);
                pdfPTable20.AddCell(new Phrase(c20));

                PdfPTable pdfPTable21 = new PdfPTable(1);
                pdfPTable21.DefaultCell.BorderWidth = 0;
                pdfPTable21.WidthPercentage = 80;
                Chunk c21 = new Chunk("GSTIN NO: 36AAICS8032K1Z0 3 - 6 - 770 / 2", FontFactory.GetFont("Arial", 10));
                c21.Font.SetStyle(0);
                pdfPTable21.AddCell(new Phrase(c21));

                PdfPTable pdfPTable22 = new PdfPTable(1);
                pdfPTable22.DefaultCell.BorderWidth = 0;
                pdfPTable22.WidthPercentage = 80;
                Chunk c22 = new Chunk("Himayatnagar, Hyderabad", FontFactory.GetFont("Arial", 9));
                c22.Font.SetStyle(0);
                pdfPTable22.AddCell(new Phrase(c22));

                PdfPTable pdfPTable23 = new PdfPTable(1);
                pdfPTable23.DefaultCell.BorderWidth = 0;
                pdfPTable23.WidthPercentage = 80;
                Chunk c23 = new Chunk("India – 500029", FontFactory.GetFont("Arial", 9));
                c23.Font.SetStyle(0);
                pdfPTable23.AddCell(new Phrase(c23));

                PdfPTable pdfPTable24 = new PdfPTable(1);
                pdfPTable24.DefaultCell.BorderWidth = 0;
                pdfPTable24.WidthPercentage = 80;
                Chunk c24 = new Chunk("Phone: (040) 66465977, 78, 79", FontFactory.GetFont("Arial", 9));
                c24.Font.SetStyle(0);
                pdfPTable24.AddCell(new Phrase(c24));

                PdfPTable pdfPTable25 = new PdfPTable(1);
                pdfPTable25.DefaultCell.BorderWidth = 0;
                pdfPTable25.WidthPercentage = 80;
                Chunk c25 = new Chunk("COMMENTS OR SPECIAL INSTRUCTIONS:", FontFactory.GetFont("Arial", 10));
                c25.Font.SetStyle(1);
                pdfPTable25.AddCell(new Phrase(c25));

                PdfPTable pdfPTable26 = new PdfPTable(1);
                pdfPTable26.DefaultCell.BorderWidth = 0;
                pdfPTable26.WidthPercentage = 80;
                Chunk c26 = new Chunk("[N/ A]", FontFactory.GetFont("Arial", 9));
                c26.Font.SetStyle(0);
                pdfPTable26.AddCell(new Phrase(c26));

                PdfPTable pdfPTable27 = new PdfPTable(1);
                pdfPTable27.DefaultCell.BorderWidth = 0;
                pdfPTable27.WidthPercentage = 80;
                Chunk c27 = new Chunk("Bank Detail:", FontFactory.GetFont("Arial", 11));
                c27.Font.SetStyle(1);
                pdfPTable27.AddCell(new Phrase(c27));

                PdfPTable pdfPTable28 = new PdfPTable(1);
                pdfPTable28.DefaultCell.BorderWidth = 0;
                pdfPTable28.WidthPercentage = 80;
                Chunk c28 = new Chunk("ICICI Bank", FontFactory.GetFont("Arial", 9));
                c28.Font.SetStyle(0);
                pdfPTable28.AddCell(new Phrase(c28));

                PdfPTable pdfPTable29 = new PdfPTable(1);
                pdfPTable29.DefaultCell.BorderWidth = 0;
                pdfPTable29.WidthPercentage = 80;
                Chunk c29 = new Chunk("BOTTOMHALF PRIVATE LIMITED", FontFactory.GetFont("Arial", 9));
                pdfPTable29.AddCell(new Phrase(c29));

                PdfPTable pdfPTable30 = new PdfPTable(1);
                pdfPTable30.DefaultCell.BorderWidth = 0;
                pdfPTable30.WidthPercentage = 80;
                Chunk c30 = new Chunk("Account Number: 029105005572", FontFactory.GetFont("Arial", 9));
                pdfPTable30.AddCell(new Phrase(c30));

                PdfPTable pdfPTable31 = new PdfPTable(1);
                pdfPTable31.DefaultCell.BorderWidth = 0;
                pdfPTable31.WidthPercentage = 80;
                Chunk c31 = new Chunk("IFSC: ICIC0000291", FontFactory.GetFont("Arial", 9));
                pdfPTable31.AddCell(new Phrase(c31));

                PdfPTable pdfPTable32 = new PdfPTable(1);
                pdfPTable32.DefaultCell.BorderWidth = 0;
                pdfPTable32.WidthPercentage = 80;
                Chunk c32 = new Chunk("Place: Asansol (West Bengal)", FontFactory.GetFont("Arial", 9));
                pdfPTable29.AddCell(new Phrase(c32));

                PdfPTable pdfPTable33 = new PdfPTable(1);
                pdfPTable33.DefaultCell.BorderWidth = 0;
                pdfPTable33.WidthPercentage = 80;
                Chunk c33 = new Chunk("Make all checks payable to BottomHalf Pvt. Ltd.", FontFactory.GetFont("Arial", 9));
                pdfPTable33.AddCell(new Phrase(c33));

                PdfPTable pdfPTable34 = new PdfPTable(1);
                pdfPTable34.DefaultCell.BorderWidth = 0;
                pdfPTable34.WidthPercentage = 80;
                Chunk c34 = new Chunk("If you have any questions concerning this invoice, contact:[ Miss. Ali Lubna, 9701633741, lubna@bottomhalf.in", FontFactory.GetFont("Arial", 9));
                pdfPTable34.AddCell(new Phrase(c34));

                //Footer Section
                PdfPTable pdfTableFooter = new PdfPTable(1);
                pdfTableFooter.DefaultCell.BorderWidth = 0;
                pdfTableFooter.WidthPercentage = 100;
                pdfTableFooter.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                Chunk cnkFooter = new Chunk("THANK YOU FOR YOUR BUSINESS!", FontFactory.GetFont("Arial", 10));
                cnkFooter.Font.SetStyle(1);
                cnkFooter.Font.Color = new iTextSharp.text.BaseColor(0, 32, 255);
                pdfTableFooter.AddCell(new Phrase(cnkFooter));
                //End of Footer Section



                //Font style
                //System.Drawing.Font fontH1 = new System.Drawing.Font("Currier", 9);

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
                    Document pdfDoc = new Document(PageSize.A4, 5f, 5f, 60f, 0f);
                    PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();

                    //region Page-1
                    pdfDoc.Add(png);
                    pdfDoc.Add(pdfPTable1);
                    pdfDoc.Add(pdfPTable2);
                    pdfDoc.Add(pdfPTable5);
                    pdfDoc.Add(pdfPTable11);
                    pdfDoc.Add(blank);
                    pdfDoc.Add(blank);
                    //pdfDoc.Add(linebreak);
                    pdfDoc.Add(pdfPTable12);
                    pdfDoc.Add(pdfPTable13);
                    pdfDoc.Add(pdfPTable14);
                    pdfDoc.Add(pdfPTable16);
                    pdfDoc.Add(pdfPTable18);
                    pdfDoc.Add(pdfPTable19);
                    pdfDoc.Add(blank);
                    pdfDoc.Add(pdfPTable20);
                    pdfDoc.Add(pdfPTable21);
                    pdfDoc.Add(pdfPTable22);
                    pdfDoc.Add(pdfPTable23);
                    pdfDoc.Add(pdfPTable24);
                    pdfDoc.Add(blank);
                    pdfDoc.Add(pdfPTable25);
                    pdfDoc.Add(pdfPTable26);
                    pdfDoc.Add(blank);
                    pdfDoc.Add(pdfPTable3);
                    pdfDoc.Add(blank);
                    pdfDoc.Add(pdfPTable27);
                    pdfDoc.Add(pdfPTable28);
                    pdfDoc.Add(pdfPTable29);
                    pdfDoc.Add(pdfPTable30);
                    pdfDoc.Add(pdfPTable31);
                    pdfDoc.Add(pdfPTable32);
                    pdfDoc.Add(blank);
                    pdfDoc.Add(blank);
                    pdfDoc.Add(pdfPTable33);
                    pdfDoc.Add(pdfPTable34);
                    pdfDoc.Add(blank);
                    pdfDoc.Add(blank);
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