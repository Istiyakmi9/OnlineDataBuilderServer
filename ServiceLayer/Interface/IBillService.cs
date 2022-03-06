using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface IBillService
    {
        string UpdateGstStatus(GstStatusModel createPageModel, IFormFileCollection FileCollection, List<Files> fileDetail);
        ResponseModel<FileDetail> GenerateDocument(BuildPdfTable _buildPdfTable, PdfModal pdfmodal);
    }
}
