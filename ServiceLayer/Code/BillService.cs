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
using TimeZoneConverter;

namespace ServiceLayer.Code
{
    public class BillService : IBillService
    {
        private readonly IDb db;
        private readonly IFileService fileService;

        public BillService(IDb db, IFileService fileService)
        {
            this.db = db;
            this.fileService = fileService;
        }

        public string UpdateGstStatus(GstStatusModel createPageModel, IFormFileCollection FileCollection, List<Files> fileDetail)
        {
            string result = string.Empty;
            TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
            createPageModel.Paidon = TimeZoneInfo.ConvertTimeFromUtc(createPageModel.Paidon, istTimeZome);

            if (fileDetail.Count > 0)
            {
                string FolderPath = Path.Combine("Documents", $"GSTFile_{createPageModel.Billno}");
                List<Files> files = fileService.SaveFile(FolderPath, fileDetail, FileCollection, "0");
                if (files != null && files.Count > 0)
                {
                    if (files != null && files.Count > 0)
                    {
                        var fileInfo = (from n in fileDetail
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
                        this.db.BatchInsert("sp_document_filedetail_insupd", dataSet, true);
                    }
                }
            }

            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(createPageModel.GstId, typeof(long), "_gstId"),
                new DbParam(createPageModel.Billno, typeof(string), "_billno"),
                new DbParam(createPageModel.Gststatus, typeof(int), "_gststatus"),
                new DbParam(createPageModel.Paidon, typeof(DateTime), "_paidon"),
                new DbParam(createPageModel.Paidby, typeof(long), "_paidby"),
                new DbParam(createPageModel.Amount, typeof(double), "_amount"),
                new DbParam(createPageModel.FileId, typeof(long), "_fileId")
            };

            result = this.db.ExecuteNonQuery("sp_gstdetail_insupd", dbParams, true);
            return result;
        }
    }
}
