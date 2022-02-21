﻿using ModalLayer.Modal;
using System.IO;

namespace DocMaker.PdfService
{
    public interface IFileMaker
    {
        FileDetail BuildPdfBill(BuildPdfTable _buildPdfTable, PdfModal pdfModal);
    }
}
