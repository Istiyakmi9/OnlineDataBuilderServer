using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocMaker.PdfService
{
    public class PdfGenerateHelper
    {
        private List<dynamic> tableList;

        public PdfGenerateHelper()
        {
            tableList = new List<dynamic>();
        }

        public void CreateHeader()
        {
            PdfPTable table = new PdfPTable(2);
            table.DefaultCell.Border = Rectangle.NO_BORDER;
            PdfPCell cell = null;

            cell = new PdfPCell(new Phrase(@"
                                    BottomHalf Pvt. Ltd
                                    Bairagi Talab K.T Road
                                    Asansol Pin: 713302
                                    Mobile No# +91-9100544384
                                "));
            cell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase(@"INVOICE\nBILL NO# 000111"));
            cell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
            table.AddCell(cell);
            tableList.Add(table);
        }


        public PdfPTable CreateTable(List<Phrase> columnDetail)
        {
            PdfPCell cell = default(PdfPCell);
            PdfPTable pdfTable = new PdfPTable(columnDetail.Count);
            int i = 0;
            while (i < columnDetail.Count)
            {
                cell = new PdfPCell(columnDetail[i]);
                pdfTable.AddCell(cell);
                i++;
            }
            pdfTable.DefaultCell.Border = 0;
            pdfTable.DefaultCell.BorderWidth = 0;
            tableList.Add(pdfTable);
            return pdfTable;
        }

        public PdfPTable CreateTableWithBorder(List<Phrase> columnDetail, float borderWidth)
        {
            PdfPTable pdfTable = new PdfPTable(columnDetail.Count);
            int i = 0;
            while (i < columnDetail.Count)
            {
                pdfTable.AddCell(columnDetail[i]);
                i++;
            }
            pdfTable.DefaultCell.BorderWidth = borderWidth;
            tableList.Add(pdfTable);
            return pdfTable;
        }

        public iTextSharp.text.Image AddImage(string Url)
        {
            iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(Url);
            //imgae  size
            png.ScaleToFit(140f, 120f);
            //Give space before image
            png.SpacingBefore = 10f;
            //Give Space after image
            png.SpacingAfter = 1f;
            png.Alignment = Element.ALIGN_CENTER;
            //End Region
            tableList.Add(png);
            return png;
        }

        public void CreateFile(string Location)
        {
            if (!Directory.Exists(Location))
                Directory.CreateDirectory(Location);

            //File Name
            int fileCount = Directory.GetFiles(@"E:\\Workspace\\").Length;
            string strFileName = "JobDescriptionForm" + (fileCount + 1) + ".pdf";

            using (FileStream stream = new FileStream(Location + strFileName, FileMode.Create))
            {
                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                PdfWriter.GetInstance(pdfDoc, stream);
                if (!pdfDoc.IsOpen())
                    pdfDoc.Open();
                pdfDoc.NewPage();

                foreach (var item in tableList)
                    pdfDoc.Add(item);

                pdfDoc.Close();
                stream.Close();
            }
        }
    }
}
