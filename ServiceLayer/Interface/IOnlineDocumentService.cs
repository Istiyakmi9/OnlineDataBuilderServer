using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface IOnlineDocumentService
    {
        List<OnlineDocumentModel> GetOnlineDocuments(FilterModel filterModel);
        List<OnlineDocumentModel> CreateDocument(CreatePageModel createPageModel);
        string DeleteFilesService(List<Files> fileDetails);
        string UploadDocumentDetail(CreatePageModel createPageModel, IFormFileCollection files, List<Files> fileDetail);
        DocumentWithFileModel GetOnlineDocumentsWithFiles(FilterModel filterModel);
    }
}
