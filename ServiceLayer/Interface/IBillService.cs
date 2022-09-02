using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.HtmlTagModel;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface IBillService
    {
        string UpdateGstStatus(GstStatusModel createPageModel, IFormFileCollection FileCollection, List<Files> fileDetail);
        FileDetail GenerateDocument(PdfModal pdfmodal);
        FileDetail CreateFiles(BuildPdfTable _buildPdfTable, PdfModal pdfmodal, Organization organization, Organization receiverOrganization);
        string SendBillToClientService(GenerateBillFileDetail generateBillFileDetail);
    }
}
