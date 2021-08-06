using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class OnlineDocumentService : IOnlineDocumentService
    {
        private readonly IDb db;
        private readonly IFileService _fileService;
        private readonly ILoginService _loginService;
        public OnlineDocumentService(IDb db, IFileService fileService, ILoginService loginService)
        {
            this.db = db;
            _fileService = fileService;
            _loginService = loginService;
        }

        public string InsertOnlineDocument(CreatePageModel createPageModel)
        {
            DbParam[] param = new DbParam[]
            {
                new DbParam(createPageModel.OnlineDocumentModel.Title, typeof(string), "@Title"),
                new DbParam(createPageModel.OnlineDocumentModel.Description, typeof(string), "@Description"),
                new DbParam(createPageModel.OnlineDocumentModel.DocumentId, typeof(int), "@DocumentId"),
                new DbParam(createPageModel.Mobile, typeof(string), "@Mobile"),
                new DbParam(createPageModel.Email, typeof(string), "@Email"),
                new DbParam(createPageModel.OnlineDocumentModel.DocPath, typeof(string), "@DocPath")
            };

            var result = this.db.ExecuteNonQuery("SP_OnlineDocument_InsUpd", param, true);
            return result;
        }

        public List<OnlineDocumentModel> CreateDocument(CreatePageModel createPageModel)
        {
            InsertOnlineDocument(createPageModel);

            return GetDocumentData(new FilterModel
            {
                SearchString = createPageModel.SearchString,
                PageIndex = createPageModel.PageIndex,
                PageSize = createPageModel.PageSize,
                SortBy = createPageModel.SortBy
            });
        }

        private List<OnlineDocumentModel> GetDocumentData(FilterModel filterModel)
        {
            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(filterModel.SearchString, typeof(string), "@SearchString"),
                new DbParam(filterModel.PageIndex, typeof(int), "@PageIndex"),
                new DbParam(filterModel.PageSize, typeof(int), "@PageSize")
            };

            List<OnlineDocumentModel> onlineDocumentModel = default;
            var Result = this.db.GetDataset("SP_OnlineDocument_Get", dbParams);
            if (Result.Tables.Count > 0 && Result.Tables[0].Rows.Count > 0)
            {
                onlineDocumentModel = Converter.ToList<OnlineDocumentModel>(Result.Tables[0]);
            }
            return onlineDocumentModel;
        }

        public List<OnlineDocumentModel> GetOnlineDocuments(FilterModel filterModel)
        {
            return GetDocumentData(filterModel);
        }

        public DocumentWithFileModel GetOnlineDocumentsWithFiles(FilterModel filterModel)
        {
            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(filterModel.SearchString, typeof(string), "@SearchString"),
                new DbParam(filterModel.PageIndex, typeof(int), "@PageIndex"),
                new DbParam(filterModel.PageSize, typeof(int), "@PageSize")
            };

            DocumentWithFileModel documentWithFileModel = new DocumentWithFileModel();
            var Result = this.db.GetDataset("SP_OnlineDocument_With_Files_Get", dbParams);
            if (Result.Tables.Count == 3)
            {
                documentWithFileModel.onlineDocumentModel = Converter.ToList<OnlineDocumentModel>(Result.Tables[0]);
                documentWithFileModel.files = Converter.ToList<Files>(Result.Tables[1]);
                documentWithFileModel.TotalRecord = Convert.ToInt64(Result.Tables[2].Rows[0]["TotalRecord"].ToString());
            }
            return documentWithFileModel;
        }

        public string UploadDocumentDetail(CreatePageModel createPageModel, IFormFileCollection FileCollection, List<Files> fileDetail)
        {
            string Result = "Fail";
            var NewDocId = InsertOnlineDocument(createPageModel);
            if (!string.IsNullOrEmpty(NewDocId))
            {
                if (FileCollection.Count > 0 && fileDetail.Count > 0)
                {
                    string FolderPath = Path.Combine("Documents",
                        createPageModel.OnlineDocumentModel.Title.Replace(" ", "_"));
                    List<Files> files = _fileService.SaveFile(FolderPath, fileDetail, FileCollection, NewDocId);
                    if (files != null && files.Count > 0)
                    {
                        Parallel.ForEach(files, item =>
                        {
                            item.Status = "Pending";
                            item.BillTypeId = 1;
                            item.UserId = 1;
                            item.PaidOn = null;
                        });
                        DataSet fileDs = Converter.ToDataSet<Files>(files);
                        if (fileDs != null && fileDs.Tables.Count > 0 && fileDs.Tables[0].Rows.Count > 0)
                        {
                            DataTable table = fileDs.Tables[0];
                            table.TableName = "Files";
                            db.InsertUpdateBatchRecord("sp_Files_InsUpd", table);
                            Result = "Success";
                        }
                    }
                }
            }
            return Result;
        }

        public string DeleteFilesService(List<Files> fileDetails)
        {

            string Result = "Fail";
            if (fileDetails != null && fileDetails.Count > 0)
            {
                var deletingFiles = new List<DocumentFile>();
                DocumentFile documentFile = default;
                Parallel.ForEach(fileDetails, item =>
                {
                    documentFile = new DocumentFile();
                    documentFile.DocumentId = item.DocumentId;
                    documentFile.FileUid = item.FileUid;
                    deletingFiles.Add(documentFile);
                });


                DataSet documentFileSet = Converter.ToDataSet<DocumentFile>(deletingFiles);

                DbParam[] dbParams = new DbParam[]
                {
                    new DbParam(fileDetails.FirstOrDefault().DocumentId, typeof(int), "@DocumentId"),
                    new DbParam(fileDetails.Select(x=>x.FileUid.ToString()).Aggregate((x,y) => x + "," + y), typeof(string), "@FileUid")
                };

                DataSet FileSet = db.GetDataset("sp_OnlieDocument_GetFiles", dbParams);
                if (FileSet.Tables.Count > 0)
                {
                    db.InsertUpdateBatchRecord("sp_OnlieDocument_Del_Multi", documentFileSet.Tables[0]);
                    List<Files> files = Converter.ToList<Files>(FileSet.Tables[0]);
                    _fileService.DeleteFiles(files);
                    Result = "Success";
                }
            }
            return Result;
        }

        public string EditCurrentFileService(Files editFile)
        {
            string Result = "Fail";
            if (editFile != null)
            {
                editFile.BillTypeId = 1;
                editFile.UserId = 1;

                DataSet fileDs = Converter.ToDataSet<Files>(new List<Files>() { editFile });
                if (fileDs != null && fileDs.Tables.Count > 0 && fileDs.Tables[0].Rows.Count > 0)
                {
                    DataTable table = fileDs.Tables[0];
                    table.TableName = "Files";
                    db.InsertUpdateBatchRecord("sp_Files_InsUpd", table);
                    Result = "Success";
                }
            }
            return Result;
        }
    }
}
