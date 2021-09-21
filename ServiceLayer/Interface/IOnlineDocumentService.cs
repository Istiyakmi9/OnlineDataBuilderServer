using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface IOnlineDocumentService
    {
        List<OnlineDocumentModel> CreateDocument(CreatePageModel createPageModel);
        string DeleteFilesService(List<Files> fileDetails);
        string EditCurrentFileService(Files editFile);
        string UploadDocumentDetail(CreatePageModel createPageModel, IFormFileCollection files, List<Files> fileDetail);
        DropdownItems LoadApplicationData();
        List<Files> GetFilesAndFolderByIdService(string Type, string Uid);
        DocumentWithFileModel GetOnlineDocumentsWithFiles(FilterModel filterModel);
        FileDetail InsertGeneratedBillRecord(BuildPdfTable _buildPdfTable, PdfModal pdfModal);
    }
}
