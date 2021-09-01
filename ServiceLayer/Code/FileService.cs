using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoreServiceLayer.Implementation
{
    public class FileService : IFileService
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public FileService(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public int DeleteFiles(List<Files> files)
        {
            int deleteCount = 0;
            if (files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (Directory.Exists(Path.Combine(_hostingEnvironment.ContentRootPath, file.FilePath)))
                    {
                        string ActualPath = Path.Combine(_hostingEnvironment.ContentRootPath, file.FilePath, file.FileName);
                        if (File.Exists(ActualPath))
                        {
                            File.Delete(ActualPath);
                            deleteCount++;
                        }
                    }
                }
            }
            return deleteCount;
        }

        public List<Files> SaveFile(string FolderPath, List<Files> fileDetail, IFormFileCollection formFiles, string ProfileUid)
        {
            string Extension = "";
            string NewFileName = string.Empty;
            if (!string.IsNullOrEmpty(FolderPath))
            {
                foreach (var file in formFiles)
                {
                    if (!string.IsNullOrEmpty(file.Name))
                    {
                        if (!Directory.Exists(Path.Combine(_hostingEnvironment.ContentRootPath, FolderPath)))
                            Directory.CreateDirectory(Path.Combine(_hostingEnvironment.ContentRootPath, FolderPath));

                        Extension = file.FileName.Substring(file.FileName.LastIndexOf('.') + 1, file.FileName.Length - file.FileName.LastIndexOf('.') - 1);
                        NewFileName = file.Name;

                        var currentFile = fileDetail.Where(x => x.FileName == file.Name).FirstOrDefault();

                        if (currentFile != null)
                        {
                            string ActualPath = Path.Combine(_hostingEnvironment.ContentRootPath, FolderPath, NewFileName);
                            if (File.Exists(ActualPath))
                            {
                                NewFileName = DateTime.Now.Ticks.ToString() + "_" + NewFileName;
                                ActualPath = Path.Combine(_hostingEnvironment.ContentRootPath, FolderPath, NewFileName);
                                currentFile.FileName = NewFileName;
                            }

                            currentFile.FileExtension = Extension;
                            currentFile.DocumentId = Convert.ToInt32(ProfileUid);
                            currentFile.FilePath = FolderPath;

                            using (FileStream fs = System.IO.File.Create(ActualPath))
                            {
                                file.CopyTo(fs);
                                fs.Flush();
                            }
                        }
                    }
                }
            }
            return fileDetail;
        }
    }
}
