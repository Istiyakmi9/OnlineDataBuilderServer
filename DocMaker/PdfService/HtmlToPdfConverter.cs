using iText.Html2pdf;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using ModalLayer.Modal.HtmlTagModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Location = ModalLayer.Modal;

namespace DocMaker.PdfService
{
    public class HtmlToPdfConverter
    {
        private readonly Location.FileLocationDetail _fileLocationDetail;

        public HtmlToPdfConverter(Location.FileLocationDetail fileLocationDetail)
        {
            _fileLocationDetail = fileLocationDetail;
        }

        public void ToPdf(HtmlNodeDetail parentNode, string filePath)
        {
            string finalPath = System.IO.Path.Combine(_fileLocationDetail.RootPath, _fileLocationDetail.UserFolder, filePath);
            if (File.Exists(finalPath)) File.Delete(finalPath);
            this.ConvertToPdf(null, finalPath);
        }

        public void ConvertToPdf(string html, string filePath)
        {
            try
            {
                var file = new FileStream(filePath, FileMode.Create);
                HtmlConverter.ConvertToPdf(html, file);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void BuildNodes(HtmlNodeDetail bodyNode, Document pdfDocument)
        {
            foreach (var node in bodyNode.ChildNodes)
            {
                if (node.TagType == "node")
                {
                    switch (node.TagName)
                    {
                        case "table":
                            pdfDocument.Add(this.CreateTable(node, pdfDocument));
                            break;
                        default:
                            pdfDocument.Add(this.CreateDivision(node, pdfDocument));
                            break;
                    }
                }
                else
                {
                    var para = this.CreateText(node);
                    Paragraph p = new Paragraph();
                    p.Add(para);
                    pdfDocument.Add(p);
                }
            }
        }

        private int CalculateTotalColumns(HtmlNodeDetail nodes)
        {
            HtmlNodeDetail counterNode = nodes;
            HtmlNodeDetail item = null;

            int i = 0;
            while (i < counterNode.ChildNodes.Count)
            {
                item = counterNode.ChildNodes.ElementAt(i);
                switch (item.TagName)
                {
                    case "td":
                    case "th":
                        return counterNode.ChildNodes.Count;
                    default:
                        counterNode = item;
                        i = 0;
                        continue;
                }
            }
            return 0;
        }

        private Table CreateTable(HtmlNodeDetail nodes, Document pdfDocument)
        {
            int TotalColumns = this.CalculateTotalColumns(nodes);
            if (TotalColumns <= 0)
                TotalColumns = 0;
            Table table = new Table(TotalColumns);
            int i = 0;
            HtmlNodeDetail item = null;
            while (i < nodes.ChildNodes.Count)
            {
                item = nodes.ChildNodes.ElementAt(i);
                if (item.TagType == "node")
                {
                    switch (item.TagName)
                    {
                        case "table":
                            CreateTable(item, pdfDocument);
                            break;
                        case "thead":
                        case "tbody":
                        case "tfoot":
                            i = 0;
                            nodes = item;
                            continue;
                        case "tr":
                            var cells = this.CreateTableRow(item.ChildNodes, pdfDocument);
                            break;
                        case "td":
                            break;
                        case "th":
                            break;
                        default:
                            var para = this.CreateDivision(item, pdfDocument);
                            table.AddCell(new Cell().Add(para));
                            break;
                    }
                }
                else
                {
                    var para = this.CreateText(item);
                    Paragraph p = new Paragraph();
                    p.Add(para);
                    table.AddCell(new Cell().Add(p));
                }
                i++;
            }
            return table;
        }

        private void TableStyling(Table table, Document doc, HtmlNodeDetail node)
        {
            if (node.Properties != null && node.Properties.Count > 0)
            {
                foreach (var prop in node.Properties)
                {
                    switch (prop.Key)
                    {
                        case "border":
                            table.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                            break;
                        case "width":
                            table.SetWidth(doc.GetPdfDocument().GetPage(1).GetPageSize().GetWidth());
                            break;
                    }
                }
            }
        }

        private List<Cell> CreateTableRow(List<HtmlNodeDetail> nodes, Document pdfDocument)
        {
            List<Cell> cells = new List<Cell>();
            int i = 0;
            HtmlNodeDetail item = null;
            while (i < nodes.Count)
            {
                item = nodes.ElementAt(i);
                if (item.TagType == "node")
                {
                    switch (item.TagName)
                    {
                        case "table":
                            this.CreateTable(item, pdfDocument);
                            break;
                        case "thead":
                        case "tbody":
                        case "tfoot":
                            break;
                        case "tr":
                            cells = this.CreateTableRow(item.ChildNodes, pdfDocument);
                            break;
                        case "td":
                            cells.Add(this.CreateCells(item.ChildNodes, false, pdfDocument));
                            break;
                        case "th":
                            cells.Add(this.CreateCells(item.ChildNodes, true, pdfDocument));
                            break;
                        default:
                            Cell cell = new Cell();
                            cell.Add(this.CreateDivision(item, pdfDocument));
                            cells.Add(cell);
                            break;
                    }
                }
                else
                {
                    Cell cell = new Cell();
                    var text = this.CreateText(item);
                    Paragraph p = new Paragraph();
                    p.Add(text);
                    cell.Add(p);
                }
                i++;
            }
            return cells;
        }

        private Cell CreateCells(List<HtmlNodeDetail> nodes, bool isHeader, Document pdfDocument)
        {
            int i = 0;
            HtmlNodeDetail item = null;
            Cell cell = new Cell();
            while (i < nodes.Count)
            {
                item = nodes.ElementAt(i);
                if (item.TagType == "node")
                {
                    switch (item.TagName)
                    {
                        case "table":
                            this.CreateTable(item, pdfDocument);
                            break;
                        case "thead":
                        case "tbody":
                        case "tfoot":
                            break;
                        case "tr":
                            this.CreateTableRow(item.ChildNodes, pdfDocument);
                            break;
                        case "td":
                            cell.Add(this.CreateCells(item.ChildNodes, false, pdfDocument));
                            break;
                        case "th":
                            cell.Add(this.CreateCells(item.ChildNodes, true, pdfDocument));
                            break;
                        default:
                            cell.Add(this.CreateDivision(item, pdfDocument));
                            break;
                    }
                }
                else
                {
                    if (item.ChildNodes.Count > 0)
                        cell.Add(this.CreateDivision(item, pdfDocument));
                    else
                    {
                        var text = this.CreateText(item);
                        Paragraph p = new Paragraph();
                        p.Add(text);
                        cell.Add(p);
                    }
                }
                i++;
            }
            return cell;
        }

        private Paragraph CreateDivision(HtmlNodeDetail nodes, Document pdfDocument)
        {
            Paragraph paragraph = new Paragraph();
            int i = 0;
            HtmlNodeDetail item = null;
            while (i < nodes.ChildNodes.Count)
            {
                item = nodes.ChildNodes.ElementAt(i);
                if (item.TagType == "node")
                {
                    switch (item.TagName)
                    {
                        case "table":
                            this.CreateTable(item, pdfDocument);
                            break;
                        case "thead":
                        case "tbody":
                        case "tfoot":
                            break;
                        case "tr":
                            this.CreateTableRow(item.ChildNodes, pdfDocument);
                            break;
                        case "td":
                            paragraph.Add(this.CreateCells(item.ChildNodes, false, pdfDocument));
                            break;
                        case "th":
                            paragraph.Add(this.CreateCells(item.ChildNodes, true, pdfDocument));
                            break;
                        default:
                            var innerParas = this.CreateDivision(item, pdfDocument);
                            paragraph.Add(innerParas);
                            break;
                    }
                }
                else
                {
                    paragraph.Add(this.CreateText(item));
                }
                i++;
            }

            this.ParagraphStyling(nodes, paragraph);
            return paragraph;
        }

        private void ParagraphStyling(HtmlNodeDetail parentNode, Paragraph para)
        {
            if (parentNode.Properties != null)
            {
                List<Pair> props = parentNode.Properties;
                var Styles = props.Where(x => x.InlineStyle != null).FirstOrDefault();
                List<Pair> InlineStyle = Styles.InlineStyle;
                foreach (Pair p in InlineStyle)
                {
                    switch (p.Key)
                    {
                        case "text-align":
                            if (p.Value == "center")
                                para.SetTextAlignment(TextAlignment.CENTER);
                            else if (p.Value == "left")
                                para.SetTextAlignment(TextAlignment.LEFT);
                            else if (p.Value == "right")
                                para.SetTextAlignment(TextAlignment.RIGHT);
                            break;
                        case "font-size":
                            para.SetFontSize(14);
                            break;
                        case "font-weight":
                            para.SetBold();
                            break;
                        case "color":
                            para.SetFontColor(ColorConstants.GRAY);
                            break;
                    }
                }
            }
        }

        private Text CreateText(HtmlNodeDetail node)
        {
            Text para = new Text(node.Value);
            return para;
        }
    }
}
