using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.HtmlTagModel;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface IBillService
    {
        string UpdateGstStatus(GstStatusModel createPageModel, IFormFileCollection FileCollection, List<Files> fileDetail);
        dynamic GenerateDocument(PdfModal pdfmodal, List<DailyTimesheetDetail> dailyTimesheetDetails,
            TimesheetDetail timesheetDetail, string Comment);
        FileDetail CreateFiles(BuildPdfTable _buildPdfTable, PdfModal pdfmodal, Organization organization, Organization receiverOrganization);
        string SendBillToClientService(GenerateBillFileDetail generateBillFileDetail);
    }
}
