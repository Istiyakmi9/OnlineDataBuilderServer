using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using DocMaker.ExcelMaker;
using DocMaker.PdfService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Caching;
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
    public class OnlineDocumentService : IOnlineDocumentService
    {
        private readonly IDb db;
        private readonly IFileService _fileService;
        private readonly CommonFilterService _commonFilterService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IFileMaker _iFileMaker;
        private readonly ICacheManager _cacheManager;
        private readonly CurrentSession _currentSession;
        private readonly IBillService _billService;
        private readonly FileLocationDetail _fileLocationDetail;
        private readonly ILogger<OnlineDocumentService> _logger;
        private readonly ExcelWriter _excelWriter;

        public OnlineDocumentService(IDb db, IFileService fileService,
            IFileMaker iFileMaker,
            ExcelWriter excelWriter,
            ILogger<OnlineDocumentService> logger,
            CommonFilterService commonFilterService,
            IAuthenticationService authenticationService,
            CurrentSession currentSession,
            ICacheManager cacheManager,
            FileLocationDetail fileLocationDetail,
            IBillService billService)
        {
            this.db = db;
            _excelWriter = excelWriter;
            _logger = logger;
            _cacheManager = cacheManager;
            _currentSession = currentSession;
            _fileService = fileService;
            _commonFilterService = commonFilterService;
            _authenticationService = authenticationService;
            _iFileMaker = iFileMaker;
            _billService = billService;
            _fileLocationDetail = fileLocationDetail;
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

            return _commonFilterService.GetResult<OnlineDocumentModel>(new FilterModel
            {
                SearchString = createPageModel.SearchString,
                PageIndex = createPageModel.PageIndex,
                PageSize = createPageModel.PageSize,
                SortBy = createPageModel.SortBy
            }, "SP_OnlineDocument_Get");
        }

        public DocumentWithFileModel GetOnlineDocumentsWithFiles(FilterModel filterModel)
        {
            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(filterModel.SearchString, typeof(string), "_SearchString"),
                new DbParam(filterModel.PageIndex, typeof(int), "_PageIndex"),
                new DbParam(filterModel.PageSize, typeof(int), "_PageSize"),
                new DbParam(filterModel.SortBy, typeof(string), "_SortBy"),
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

        public DataSet LoadApplicationData()
        {
            return _cacheManager.LoadApplicationData();
        }

        public Bills GetBillData()
        {
            Bills bill = default;
            DbParam[] param = new DbParam[]
            {
                new DbParam(1, typeof(long), "_BillTypeUid")
            };
            DataSet ds = db.GetDataset("sp_billdata_get", param);
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var bills = Converter.ToList<Bills>(ds.Tables[0]);
                if (bills != null && bills.Count > 0)
                {
                    bill = bills[0];
                }
            }
            return bill;
        }

        public ResponseModel<FileDetail> InsertGeneratedBillRecord(BuildPdfTable _buildPdfTable, PdfModal pdfModal)
        {
            ResponseModel<FileDetail> responseModel = new ResponseModel<FileDetail>();
            try
            {
                TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
                pdfModal.billingMonth = TimeZoneInfo.ConvertTimeFromUtc(pdfModal.billingMonth, istTimeZome);
                pdfModal.dateOfBilling = TimeZoneInfo.ConvertTimeFromUtc(pdfModal.dateOfBilling, istTimeZome);
                FileDetail fileDetail = new FileDetail();
                Bills bill = GetBillData();
                if (string.IsNullOrEmpty(pdfModal.billNo))
                {
                    if (bill == null || string.IsNullOrEmpty(bill.GeneratedBillNo))
                    {
                        responseModel.ErroMessage = "Fail to generate bill no. Please contact admin.";
                        return responseModel;
                    }
                    pdfModal.billNo = bill.GeneratedBillNo;
                }
                else
                {
                    string GeneratedBillNo = "";
                    int len = pdfModal.billNo.Length;
                    int i = 0;
                    while (i < bill.BillNoLength)
                    {
                        if (i < len)
                        {
                            GeneratedBillNo += pdfModal.billNo[i];
                        }
                        else
                        {
                            GeneratedBillNo = '0' + GeneratedBillNo;
                        }
                        i++;
                    }
                    pdfModal.billNo = GeneratedBillNo;
                }

                _iFileMaker.BuildPdfBill(_buildPdfTable, pdfModal, new Organization());
                if (fileDetail.Status == 1)
                {
                    int Year = Convert.ToInt32(pdfModal.billingMonth.ToString("yyyy"));
                    DbParam[] dbParams = new DbParam[]
                    {
                        new DbParam(fileDetail.FileId, typeof(long), "_FileId"),
                        new DbParam(fileDetail.ClientId, typeof(long), "_ClientId"),
                        new DbParam(fileDetail.FileName, typeof(string), "_FileName"),
                        new DbParam(fileDetail.FilePath, typeof(string), "_FilePath"),
                        new DbParam(fileDetail.FileExtension, typeof(string), "_FileExtension"),
                        new DbParam(pdfModal.StatusId, typeof(long), "_StatusId"),
                        new DbParam(bill.NextBillNo, typeof(int), "_GeneratedBillNo"),
                        new DbParam(bill.BillUid, typeof(int), "_BillUid"),
                        new DbParam(pdfModal.billId, typeof(long), "_BillDetailId"),
                        new DbParam(pdfModal.billNo, typeof(string), "_BillNo"),
                        new DbParam(pdfModal.packageAmount, typeof(double), "_PaidAmount"),
                        new DbParam(pdfModal.billingMonth.Month, typeof(int), "_BillForMonth"),
                        new DbParam(Year, typeof(int), "_BillYear"),
                        new DbParam(pdfModal.workingDay, typeof(int), "_NoOfDays"),
                        new DbParam(pdfModal.daysAbsent, typeof(double), "_NoOfDaysAbsent"),
                        new DbParam(pdfModal.iGST, typeof(float), "_IGST"),
                        new DbParam(pdfModal.sGST, typeof(float), "_SGST"),
                        new DbParam(pdfModal.cGST, typeof(float), "_CGST"),
                        new DbParam(ApplicationConstants.TDS, typeof(float), "_TDS"),
                        new DbParam(ApplicationConstants.Pending, typeof(int), "_BillStatusId"),
                        new DbParam(pdfModal.PaidOn, typeof(DateTime), "_PaidOn"),
                        new DbParam(pdfModal.FileId, typeof(int), "_FileDetailId"),
                        new DbParam(pdfModal.UpdateSeqNo, typeof(int), "_UpdateSeqNo"),
                        new DbParam(pdfModal.EmployeeId, typeof(int), "_EmployeeUid"),
                        new DbParam(pdfModal.dateOfBilling, typeof(DateTime), "_BillUpdatedOn"),
                        new DbParam(pdfModal.IsCustomBill, typeof(bool), "_isCustomBill"),
                        new DbParam(UserType.Employee, typeof(int), "_UserTypeId"),
                        new DbParam(_currentSession.CurrentUserDetail.UserId, typeof(long), "_AdminId")
                    };

                    var fileId = this.db.ExecuteNonQuery("sp_filedetail_insupd", dbParams, true);
                    if (string.IsNullOrEmpty(fileId))
                    {
                        List<Files> files = new List<Files>();
                        files.Add(new Files
                        {
                            FilePath = fileDetail.FilePath,
                            FileName = fileDetail.FileName
                        });
                        _fileService.DeleteFiles(files);
                    }
                    else
                    {
                        fileDetail.FileId = Convert.ToInt32(fileId);
                        fileDetail.DiskFilePath = null;
                    }
                }

                responseModel.Result = fileDetail;
            }
            catch (Exception ex)
            {
                responseModel.ErroMessage = ex.Message;
            }

            return responseModel;
        }

        public DataSet GetFilesAndFolderByIdService(string Type, string Uid, FilterModel filterModel)
        {
            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(Type, typeof(string), "_Type"),
                new DbParam(Uid, typeof(string), "_Uid"),
                new DbParam(filterModel.SearchString, typeof(string), "_searchString"),
                new DbParam(filterModel.SortBy, typeof(string), "_sortBy"),
                new DbParam(filterModel.PageIndex, typeof(int), "_pageIndex"),
                new DbParam(filterModel.PageSize, typeof(int), "_pageSize")
            };

            var Result = this.db.GetDataset("sp_billdetail_filter", dbParams);
            if (Result.Tables.Count == 3)
            {
                Result.Tables[0].TableName = "Files";
                Result.Tables[1].TableName = "Employee";
                Result.Tables[2].TableName = "EmployeesList";
            }
            return Result;
        }

        public List<Files> EditFileService(Files files)
        {
            //List<Files> filses = new List<Files>();
            //FileDetail fileDetail = new FileDetail();
            //DbParam[] dbParams = new DbParam[]
            //{
            //    new DbParam(fileDetail.FileExtension, typeof(string), "_FileExtension"),
            //    new DbParam(fileDetail.FileName, typeof(string), "_FileName"),
            //    new DbParam(fileDetail.FileId, typeof(int), "_FileDetailId")
            //};

            //var Result = this.db.ExecuteNonQuery("sp_Files_GetById", dbParams, true);
            //files = Converter.ToList<Files>(Result.Tables[0]);
            //return files;
            return null;
        }

        public DataSet EditEmployeeBillDetailService(GenerateBillFileDetail fileDetail)
        {
            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(_currentSession.CurrentUserDetail.UserId, typeof(long), "_AdminId"),
                new DbParam(fileDetail.EmployeeId, typeof(long), "_EmployeeId"),
                new DbParam(fileDetail.ClientId, typeof(long), "_ClientId"),
                new DbParam(fileDetail.FileId, typeof(long), "_FileId"),
            };

            var Result = this.db.GetDataset("sp_EmployeeBillDetail_ById", dbParams);
            if (Result.Tables.Count == 6)
            {
                Result.Tables[0].TableName = "fileDetail";
                Result.Tables[1].TableName = "clients";
                Result.Tables[2].TableName = "employees";
                Result.Tables[3].TableName = "roles";
                Result.Tables[4].TableName = "companys";
                Result.Tables[5].TableName = "allocatedClients";
            }
            return Result;
        }

        public FileDetail ReGenerateService(BuildPdfTable _buildPdfTable, GenerateBillFileDetail generateBillFileDetail)
        {
            FileDetail fileDetail = new FileDetail
            {
                ClientId = generateBillFileDetail.ClientId,
                EmployeeId = generateBillFileDetail.EmployeeId,
                FileExtension = generateBillFileDetail.FileExtension,
                FileId = generateBillFileDetail.FileId,
                FileName = generateBillFileDetail.FileName,
                FilePath = generateBillFileDetail.FilePath
            };

            try
            {
                BillDetail billDetail = null;
                FileDetail currentFileDetail = null;
                Organization receiverOrganization = null;
                Organization organization = null;
                List<AttendenceDetail> attendanceSet = new List<AttendenceDetail>();
                DbParam[] dbParams = new DbParam[]
                {
                    new DbParam(_currentSession.CurrentUserDetail.UserId, typeof(long), "_AdminId"),
                    new DbParam(fileDetail.EmployeeId, typeof(long), "_EmployeeId"),
                    new DbParam(fileDetail.ClientId, typeof(long), "_ClientId"),
                    new DbParam(fileDetail.FileId, typeof(long), "_FileId"),
                    new DbParam(UserType.Employee, typeof(int), "_UserTypeId")
                };

                var Result = this.db.GetDataset("sp_ExistingBill_GetById", dbParams);
                if (Result.Tables.Count == 5)
                {
                    billDetail = Converter.ToType<BillDetail>(Result.Tables[0]);
                    currentFileDetail = Converter.ToType<FileDetail>(Result.Tables[1]);
                    receiverOrganization = Converter.ToType<Organization>(Result.Tables[2]);
                    organization = Converter.ToType<Organization>(Result.Tables[3]);

                    if (Result.Tables[4].Rows.Count > 0)
                    {
                        var currentAttendance = Converter.ToType<Attendance>(Result.Tables[4]);
                        attendanceSet = JsonConvert.DeserializeObject<List<AttendenceDetail>>(currentAttendance.AttendanceDetail);
                    }
                }
                else
                {
                    throw new HiringBellException("Unable to get file detail.");
                }

                string Extension = Utility.GetExtension(currentFileDetail.FileExtension, "pdf");
                if (Extension == null)
                {
                    fileDetail.FileExtension = "pdf,docx";
                    Extension = "pdf";

                }

                string filePath = Path.Combine(Directory.GetCurrentDirectory(), currentFileDetail.FilePath, $"{currentFileDetail.FileName}.{Extension}");
                if (!File.Exists(filePath))
                {
                    var billmonth = billDetail.BillYear.ToString() + billDetail.BillForMonth.ToString().PadLeft(2, '0') + billDetail.BillUpdatedOn.ToString("dd");
                    DateTime billingForMonth = DateTime.ParseExact(billmonth, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

                    PdfModal pdfModal = new PdfModal
                    {
                        header = null,
                        billingMonth = billingForMonth,
                        billNo = billDetail.BillNo,
                        billId = billDetail.BillDetailUid,
                        dateOfBilling = billDetail.BillUpdatedOn,
                        cGST = billDetail.CGST,
                        sGST = billDetail.SGST,
                        iGST = billDetail.IGST,
                        cGstAmount = Converter.TwoDecimalValue((decimal)(billDetail.SGST * billDetail.PaidAmount) / 100),
                        sGstAmount = Converter.TwoDecimalValue((decimal)(billDetail.CGST * billDetail.PaidAmount) / 100),
                        iGstAmount = Converter.TwoDecimalValue((decimal)(billDetail.IGST * billDetail.PaidAmount) / 100),
                        workingDay = billDetail.NoOfDays - (int)billDetail.NoOfDaysAbsent,
                        packageAmount = billDetail.PaidAmount,
                        grandTotalAmount = Converter.TwoDecimalValue(billDetail.PaidAmount + (billDetail.PaidAmount * (billDetail.CGST + billDetail.SGST + billDetail.IGST)) / 100),
                        senderCompanyName = organization.ClientName,
                        receiverFirstAddress = receiverOrganization.FirstAddress,
                        receiverCompanyId = receiverOrganization.ClientId,
                        receiverCompanyName = receiverOrganization.ClientName,
                        senderClientId = organization.ClientId,
                        developerName = billDetail.DeveloperName,
                        receiverSecondAddress = receiverOrganization.SecondAddress,
                        receiverThirdAddress = receiverOrganization.ThirdAddress,
                        senderFirstAddress = receiverOrganization.FirstAddress,
                        daysAbsent = billDetail.NoOfDaysAbsent,
                        senderSecondAddress = organization.SecondAddress,
                        senderPrimaryContactNo = organization.PrimaryPhoneNo,
                        senderEmail = organization.Email,
                        senderGSTNo = organization.GSTNO,
                        receiverGSTNo = receiverOrganization.GSTNO,
                        receiverPrimaryContactNo = receiverOrganization.PrimaryPhoneNo,
                        receiverEmail = receiverOrganization.Email,
                        UpdateSeqNo = billDetail.UpdateSeqNo,
                        ClientId = receiverOrganization.ClientId,
                        EmployeeId = billDetail.EmployeeUid,
                        FileId = currentFileDetail.FileId,
                        FileName = currentFileDetail.FileName,
                        FilePath = currentFileDetail.FilePath,
                        LogoPath = currentFileDetail.LogoPath,
                        DiskFilePath = currentFileDetail.DiskFilePath,
                        FileExtension = currentFileDetail.FileExtension,
                        StatusId = billDetail.BillStatusId,
                        PaidOn = billDetail.PaidOn,
                        Status = (int)currentFileDetail.StatusId,
                        GeneratedBillNo = billDetail.BillNo,
                        UpdatedOn = currentFileDetail.UpdatedOn,
                        Notes = null
                    };

                    string MonthName = pdfModal.billingMonth.ToString("MMM_yyyy");
                    string FolderLocation = Path.Combine(_fileLocationDetail.Location, _fileLocationDetail.BillsPath, MonthName);
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), FolderLocation);
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    string destinationFilePath = Path.Combine(
                        folderPath,
                        pdfModal.developerName.Replace(" ", "_") + "_" +
                        pdfModal.billingMonth.ToString("MMM_yyyy") + "_" +
                        pdfModal.billNo + $".{ApplicationConstants.Excel}");

                    if (File.Exists(destinationFilePath))
                        File.Delete(destinationFilePath);

                    var timesheetData = (from n in attendanceSet
                                         orderby n.AttendanceDay ascending
                                         select new TimesheetModel
                                         {
                                             Date = n.AttendanceDay.ToString("dd MMM yyyy"),
                                             ResourceName = pdfModal.developerName,
                                             StartTime = "10:00 AM",
                                             EndTime = "06:00 PM",
                                             TotalHrs = 9,
                                             Comments = n.UserComments,
                                             Status = "Approved"
                                         }
                    ).ToList<TimesheetModel>();

                    var timeSheetDataSet = Converter.ToDataSet<TimesheetModel>(timesheetData);
                    _excelWriter.ToExcel(timeSheetDataSet.Tables[0], destinationFilePath, pdfModal.billingMonth.ToString("MMM_yyyy"));

                    _billService.CreateFiles(_buildPdfTable, pdfModal, organization, receiverOrganization);
                }

                return fileDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public string DeleteDataService(string Uid)
        {
            throw new NotImplementedException();
        }

        public string UpdateRecord(FileDetail fileDetail, long Uid)
        {
            string status = string.Empty;
            TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
            fileDetail.UpdatedOn = TimeZoneInfo.ConvertTimeFromUtc(fileDetail.UpdatedOn, istTimeZome);
            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(Uid, typeof(long), "_FileId"),
                new DbParam(fileDetail.StatusId, typeof(long), "_StatusId"),
                new DbParam(fileDetail.UpdatedOn, typeof(DateTime), "_UpdatedOn"),
                new DbParam(_currentSession.CurrentUserDetail.UserId, typeof(long), "_AdminId"),
                new DbParam(fileDetail.Notes, typeof(string), "_Notes"),
            };

            status = this.db.ExecuteNonQuery("sp_FileDetail_PatchRecord", dbParams, true);
            return status;
        }

        public string UploadDocumentRecord(List<ProfessionalUserDetail> uploadDocuments)
        {
            DataSet ds = Converter.ToDataSet<ProfessionalUserDetail>(uploadDocuments);
            var status = this.db.BatchInsert("sp_ProfessionalCandidates_InsUpdate", ds.Tables[0], true);
            return "Uploaded success";

        }

        public DataSet GetProfessionalCandidatesRecords(FilterModel filterModel)
        {
            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(filterModel.SearchString, typeof(string), "_SearchString"),
                new DbParam(filterModel.PageIndex, typeof(int), "_PageIndex"),
                new DbParam(filterModel.PageSize, typeof(int), "_PageSize"),
                new DbParam(filterModel.SortBy, typeof(string), "_SortBy")
            };
            DataSet Result = this.db.GetDataset("SP_professionalcandidates_filter", dbParams);
            return Result;
        }

        public string UploadDocumentDetail(CreatePageModel createPageModel, IFormFileCollection FileCollection, List<Files> fileDetail)
        {
            string Result = "Fail";
            var NewDocId = InsertOnlineDocument(createPageModel);
            if (!string.IsNullOrEmpty(NewDocId))
            {
                if (FileCollection.Count > 0 && fileDetail.Count > 0)
                {
                    string FolderPath = Path.Combine(_fileLocationDetail.Location,
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

        public async Task<DataSet> UploadFilesOrDocuments(List<Files> fileDetail, IFormFileCollection FileCollection)
        {
            DataSet Result = null;
            Files file = fileDetail.FirstOrDefault();
            try
            {
                await Task.Run(() =>
                {
                    if (FileCollection.Count > 0 && fileDetail.Count > 0)
                    {
                        string userEmail = null;
                        if (file.UserTypeId == UserType.Employee)
                        {
                            DbParam[] dbParams = new DbParam[]
                            {
                            new DbParam(file.UserId, typeof(long), "_EmployeeId"),
                            new DbParam(-1, typeof(int), "_IsActive")
                            };

                            DataSet ResultSet = this.db.GetDataset("SP_Employees_ById", dbParams);
                            if (ResultSet != null && ResultSet.Tables.Count == 1)
                            {
                                var employee = Converter.ToList<Employee>(ResultSet.Tables[0]).Single();
                                userEmail = employee.Email;
                            }
                        }
                        else if (file.UserTypeId == UserType.Client)
                        {
                            DbParam[] dbParams = new DbParam[]
                            {
                            new DbParam(file.UserId, typeof(string), "_userId"),
                            };

                            DataSet ResultSet = this.db.GetDataset("sp_UserDetail_ById", dbParams);
                            if (ResultSet != null && ResultSet.Tables.Count == 1)
                            {
                                var userDetail = Converter.ToList<UserDetail>(ResultSet.Tables[0]).Single();
                                userEmail = userDetail.EmailId;
                            }
                        }

                        if (!string.IsNullOrEmpty(userEmail))
                        {
                            fileDetail.ForEach(item =>
                            {
                                if (string.IsNullOrEmpty(item.ParentFolder))
                                {
                                    item.ParentFolder = string.Empty;  // Path.Combine(ApplicationConstants.DocumentRootPath, ApplicationConstants.User);
                                }
                                else
                                {
                                    item.ParentFolder = Path.Combine(_fileLocationDetail.Location, item.ParentFolder);
                                    item.ParentFolder = item.ParentFolder;
                                    item.Email = userEmail;
                                }
                            });


                            string FolderPath = _fileLocationDetail.UserFolder;
                            List<Files> files = _fileService.SaveFile(FolderPath, fileDetail, FileCollection, file.UserId.ToString());
                            if (files != null && files.Count > 0)
                            {
                                Result = InsertFileDetails(fileDetail);
                            }
                        }
                        else
                        {
                            throw new Exception("Invalid user detail.");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Result;
        }

        public DataSet GetDocumentResultById(Files fileDetail)
        {
            DataSet Result = null;
            if (fileDetail != null)
            {
                DbParam[] dbParams = new DbParam[]
                {
                        new DbParam(fileDetail.UserId, typeof(long), "_OwnerId"),
                        new DbParam(fileDetail.UserTypeId, typeof(long), "_UserTypeId")
                };

                Result = this.db.GetDataset("sp_document_filedetail_get", dbParams);
            }

            return Result;
        }

        public DataSet InsertFileDetails(List<Files> fileDetail)
        {
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

            //DataTable table = Converter.ToDataTable(fileInfo);
            //var dataSet = new DataSet();
            //dataSet.Tables.Add(table);

            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(JsonConvert.SerializeObject(fileInfo), typeof(string), "_InsertFileJsonData")
            };

            return this.db.GetDataset(ApplicationConstants.InserUserFileDetail, dbParams);
        }
    }
}
