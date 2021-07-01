using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface IFileService
    {
        List<Files> SaveFile(string FolderPath, List<Files> fileDetail, IFormFileCollection formFiles, string ProfileUid);
        int DeleteFiles(List<Files> files);
    }
}
