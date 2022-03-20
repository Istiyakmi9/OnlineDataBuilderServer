using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace CoreServiceLayer.Implementation
{
    public class FileService : IFileService
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IDb _db;
        public FileService(IHostingEnvironment hostingEnvironment, IDb db)
        {
            _hostingEnvironment = hostingEnvironment;
            _db = db;
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
            string Email = string.Empty;
            string NewFileName = string.Empty;
            string ActualPath = string.Empty;
            if (!string.IsNullOrEmpty(FolderPath))
            {
                foreach (var file in formFiles)
                {
                    if (!string.IsNullOrEmpty(file.Name))
                    {
                        var currentFile = fileDetail.Where(x => x.FileName == file.Name).FirstOrDefault();
                        Email = currentFile.Email.Replace("@", "_").Replace(".", "_");

                        if (currentFile.FilePath.IndexOf(Email) == -1 && FolderPath.IndexOf(Email) == -1)
                            FolderPath = Path.Combine(FolderPath, Email);

                        if (!string.IsNullOrEmpty(currentFile.FilePath))
                            ActualPath = Path.Combine(FolderPath, currentFile.FilePath);
                        else
                            ActualPath = FolderPath;

                        ActualPath = ActualPath.ToLower();
                        currentFile.FilePath = ActualPath;
                        if (!Directory.Exists(Path.Combine(_hostingEnvironment.ContentRootPath, ActualPath)))
                            Directory.CreateDirectory(Path.Combine(_hostingEnvironment.ContentRootPath, ActualPath));

                        Extension = file.FileName.Substring(file.FileName.LastIndexOf('.') + 1, file.FileName.Length - file.FileName.LastIndexOf('.') - 1);
                        currentFile.FileName = file.Name;
                        NewFileName = file.Name + "." + Extension;


                        if (currentFile != null)
                        {
                            string FilePath = Path.Combine(_hostingEnvironment.ContentRootPath, ActualPath, NewFileName);
                            if (File.Exists(FilePath))
                            {
                                File.Delete(FilePath);
                            }

                            currentFile.FileExtension = Extension;
                            currentFile.DocumentId = Convert.ToInt64(ProfileUid);
                            currentFile.FilePath = ActualPath;

                            using (FileStream fs = System.IO.File.Create(FilePath))
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

        public DataSet DeleteFiles(long userId, List<string> fileIds, int userTypeId)
        {
            this.DeleteFilesEntry(fileIds, ApplicationConstants.GetUserFileById);
            var resultSet = GetUserFilesById(userId, userTypeId);
            return resultSet;
        }

        public DataSet GetUserFilesById(long userId, int userTypeId)
        {
            DataSet Result = null;
            if (userId > 0)
            {
                DbParam[] dbParams = new DbParam[]
                {
                        new DbParam(userId, typeof(long), "_OwnerId"),
                        new DbParam(userTypeId, typeof(int), "_UserTypeId")
                };

                Result = _db.GetDataset("sp_document_filedetail_get", dbParams);
            }

            return Result;
        }

        public DataSet CreateFolder(Files fileDetail)
        {
            bool isLocationFound = false;
            string actualFolderPath = string.Empty;
            DataSet dataSet = null;
            if (fileDetail != null)
            {
                fileDetail.FilePath = fileDetail.FilePath.ToLower();
                if (string.IsNullOrEmpty(fileDetail.ParentFolder))
                    fileDetail.ParentFolder = Path.Combine(ApplicationConstants.DocumentRootPath, ApplicationConstants.User);
                else
                    fileDetail.ParentFolder = Path.Combine(Path.Combine(ApplicationConstants.DocumentRootPath, ApplicationConstants.User), fileDetail.ParentFolder);

                fileDetail.ParentFolder = fileDetail.ParentFolder.ToLower();
                if (!string.IsNullOrEmpty(fileDetail.FilePath))
                {
                    switch (fileDetail.SystemFileType)
                    {
                        case FileSystemType.User:
                            isLocationFound = true;
                            fileDetail.FilePath = Path.Combine(
                                    Path.Combine(
                                        ApplicationConstants.DocumentRootPath,
                                        ApplicationConstants.User
                                        ),
                                    fileDetail.FilePath
                                );

                            fileDetail.FilePath = fileDetail.FilePath.ToLower();
                            actualFolderPath = Path.Combine(
                                        _hostingEnvironment.ContentRootPath,
                                        fileDetail.FilePath
                                    );
                            break;
                        case FileSystemType.Bills:
                            break;
                    }

                    if (isLocationFound)
                    {
                        if (!Directory.Exists(actualFolderPath))
                            Directory.CreateDirectory(actualFolderPath);

                        List<Files> files = new List<Files>
                        {
                            fileDetail
                        };

                        this.InsertFileDetails(files, ApplicationConstants.InserUserFileDetail);
                        dataSet = this.GetUserFilesById(fileDetail.UserId, (int)fileDetail.UserTypeId);
                    }
                }
            }
            return dataSet;
        }

        public Tuple<string, bool> InsertFileDetails(List<Files> fileDetail, string procedure)
        {
            var Items = fileDetail.Where(x => x.UserId > 0);
            if (Items.Count() == 0)
            {
                return new Tuple<string, bool>("Incorrect userId provided.", false);
            }

            var fileInfo = (from n in fileDetail.AsEnumerable()
                            select new
                            {
                                FileId = n.FileUid,
                                FileOwnerId = n.UserId,
                                FileName = n.FileName,
                                FilePath = n.FilePath,
                                ParentFolder = n.ParentFolder,
                                FileExtension = n.FileExtension,
                                StatusId = 0,
                                UserTypeId = (int)n.UserTypeId,
                                AdminId = 1
                            });

            DataTable table = Converter.ToDataTable(fileInfo);
            var dataSet = new DataSet();
            dataSet.Tables.Add(table);
            var result = _db.BatchInsert(procedure, dataSet, true);
            return new Tuple<string, bool>("Total " + result + " inserted/updated.", true);
        }

        public string DeleteFilesEntry(List<string> fileIds, string GetFilesProcedure)
        {

            string Result = "Fail";
            if (fileIds != null && fileIds.Count > 0)
            {
                DbParam[] dbParams = new DbParam[]
                {
                    new DbParam(fileIds.Aggregate((x, y) => x + "," + y), typeof(string), "_FileIds")
                };

                DataSet FileSet = _db.GetDataset(GetFilesProcedure, dbParams);
                if (FileSet.Tables.Count > 0)
                {
                    List<Files> files = Converter.ToList<Files>(FileSet.Tables[0]);
                    Result = _db.ExecuteNonQuery(ApplicationConstants.deleteUserFile, dbParams, true);
                    if (Result == "Deleted successfully")
                        DeleteFiles(files);
                }
            }
            return Result;
        }
    }
}
