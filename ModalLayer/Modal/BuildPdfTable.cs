using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public class BuildPdfTable
    {
        public Dictionary<string, PTables> tables { set; get; }
    }

    public class PTables
    {
        public Table table { set; get; }
    }

    public class Table
    {
        public float spaceAfter { set; get; }
        public float spaceBefore { set; get; }
        public bool isBorder { set; get; }
        public int columnCount { set; get; }
        public List<PdfRows> rows { set; get; }
    }

    public class PdfRows
    {
        public List<PdfColumn> columns { set; get; }
    }

    public class PdfColumn
    {
        public List<Text> text { set; get; }
        public string img { set; get; }
        public int rowSpan { set; get; }
        public int colSpan { set; get; }
        public string align { set; get; }
        public bool removeBorder { set; get; }
        public float paddingTop { set; get; }
        public float paddingLeft { set; get; }
        public float paddingBottom { set; get; }
        public float paddingRight { set; get; }
    }

    public class Text
    {
        public string text { set; get; }
        public int fontSize { set; get; }
        public bool isBold { set; get; }
        public string color { set; get; }
    }

    public class FileLocationDetail
    {
        public string RootPath { set; get; }
        public string Location { set; get; }
        public string AppLocation { set; get; }
        public string DocumentFolder { set; get; }
        public string UserFolder { set; get; }
        public string BillFolder { set; get; }
        public List<string> HtmlTemplaePath { set; get; }
        public string User { set; get; }
        public string BillsPath { set; get; }
        public string StaffingBillTemplate { set; get; }
        public string StaffingBillPdfTemplate { set; get; }
        public string resumeTemplate { set; get; }
        public List<string> resumePath { set; get; }
        public string LogoPath { set; get; }
    }
}
