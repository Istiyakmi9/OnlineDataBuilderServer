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
using System.Threading.Tasks;

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
                            currentFile.DocumentId = Convert.ToInt64(ProfileUid);
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

        public DataSet DeleteFiles(long userId, List<string> fileIds)
        {
            this.DeleteFilesEntry(fileIds, ApplicationConstants.GetUserFileById);
            var resultSet = GetUserFilesById(userId);
            return resultSet;
        }

        public DataSet GetUserFilesById(long userId)
        {
            DataSet Result = null;
            if (userId > 0)
            {
                DbParam[] dbParams = new DbParam[]
                {
                        new DbParam(userId, typeof(long), "_OwnerId")
                };

                Result = _db.GetDataset("sp_document_filedetail_get", dbParams);
            }

            return Result;
        }

        public string CreateFolder(Files fileDetail)
        {
            bool isLocationFound = false;
            string actualFolderPath = string.Empty;
            string resultStatus = string.Empty;
            if (fileDetail != null)
            {
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

                        var result = this.InsertFileDetails(files, ApplicationConstants.InserUserFileDetail);
                        resultStatus = result.Item1;
                    }
                }
            }
            return resultStatus;
        }

        public Tuple<string, bool> InsertFileDetails(List<Files> fileDetail, string procedure)
        {
            var Items = fileDetail.Where(x => x.UserId <= 0);
            if (Items.Count() > 0)
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
