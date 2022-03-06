using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using DocMaker.HtmlToDocx;
using DocMaker.PdfService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TimeZoneConverter;

namespace ServiceLayer.Code
{
    public class BillService : IBillService
    {
        private readonly IDb db;
        private readonly IFileService fileService;
        private readonly IHTMLConverter iHTMLConverter;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly FileLocationDetail _fileLocationDetail;
        private readonly CurrentSession _currentSession;
        private readonly IFileMaker _fileMaker;

        public BillService(IDb db, IFileService fileService, IHTMLConverter iHTMLConverter,
            IHostingEnvironment hostingEnvironment,
            IOptions<FileLocationDetail> options,
            CurrentSession currentSession,
            IFileMaker fileMaker)
        {
            this.db = db;
            this.fileService = fileService;
            this.iHTMLConverter = iHTMLConverter;
            _fileLocationDetail = options.Value;
            _hostingEnvironment = hostingEnvironment;
            _currentSession = currentSession;
            _fileMaker = fileMaker;
        }

        public ResponseModel<FileDetail> GenerateDocument(PdfModal pdfModal)
        {
            //this.iHTMLConverter.ToHtml();
            ResponseModel<FileDetail> responseModel = new ResponseModel<FileDetail>();
            try
            {
                TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
                pdfModal.billingMonth = TimeZoneInfo.ConvertTimeFromUtc(pdfModal.billingMonth, istTimeZome);
                pdfModal.dateOfBilling = TimeZoneInfo.ConvertTimeFromUtc(pdfModal.dateOfBilling, istTimeZome);
                FileDetail fileDetail = null;

                Bills bill = GetBillData();
                if (string.IsNullOrEmpty(pdfModal.billNo))
                {
                    if (bill == null || string.IsNullOrEmpty(bill.GeneratedBillNo))
                    {
                        responseModel.ErroMessage = "Fail to generate bill no. Please contact admin.";
                        return responseModel;
                    }
                    pdfModal.billNo = bill.GeneratedBillNo;

                    //pdfModal.billNo = Regex.Replace(bill.GeneratedBillNo, "[^a-zA-Z0-9_]+", " ");
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
                pdfModal.billNo = pdfModal.billNo.Replace("#", "");

                double grandTotalAmount = 0.00;
                double sGSTAmount = 0.00;
                double cGSTAmount = 0.00;
                double iGSTAmount = 0.00;

                if (pdfModal.cGST > 0 || pdfModal.sGST > 0 || pdfModal.iGST > 0)
                {
                    sGSTAmount = (pdfModal.packageAmount * pdfModal.sGST) / 100;
                    cGSTAmount = (pdfModal.packageAmount * pdfModal.cGST) / 100;
                    iGSTAmount = (pdfModal.packageAmount * pdfModal.iGST) / 100;
                    grandTotalAmount = Math.Round(pdfModal.packageAmount + (cGSTAmount + sGSTAmount + iGSTAmount), 2);
                }
                else
                {
                    grandTotalAmount = pdfModal.packageAmount;
                }

                if (pdfModal.grandTotalAmount == grandTotalAmount && pdfModal.cGstAmount == cGSTAmount && pdfModal.sGstAmount == sGSTAmount && pdfModal.iGstAmount == iGSTAmount)
                {
                    //fileDetail = _iFileMaker.BuildPdfBill(_buildPdfTable, pdfModal);
                    //&& fileDetail.Status == "Generated"
                    Client sender = null;
                    Client receiver = null;
                    Employee employeeMappedClient = null;
                    DataSet ds = new DataSet();
                    DbParam[] dbParams = new DbParam[]
                    {
                            new DbParam(pdfModal.ClientId, typeof(long), "_receiver"),
                            new DbParam(pdfModal.senderClientId, typeof(long), "_sender"),
                            new DbParam(pdfModal.EmployeeId, typeof(long), "_employeeId")
                    };

                    ds = this.db.GetDataset("sp_Billing_detail", dbParams);
                    if (ds.Tables.Count == 3)
                    {
                        List<Client> SenderDetails = Converter.ToList<Client>(ds.Tables[0]);
                        List<Client> ReceiverDetails = Converter.ToList<Client>(ds.Tables[1]);
                        List<Employee> EmployeeMappedClient = Converter.ToList<Employee>(ds.Tables[2]);

                        sender = SenderDetails.Single();
                        receiver = ReceiverDetails.Single();
                        employeeMappedClient = EmployeeMappedClient.Single();

                        fileDetail = GetFileDetail(pdfModal, ".docx");

                        string rootPath = _hostingEnvironment.ContentRootPath;
                        string templatePath = Path.Combine(rootPath,
                            _fileLocationDetail.Location,
                            Path.Combine(_fileLocationDetail.HtmlTemplaePath.ToArray()),
                            _fileLocationDetail.StaffingBillTemplate
                        );

                        string headerLogo = Path.Combine(rootPath, _fileLocationDetail.Location, "Logos", "logo.png");
                        if (File.Exists(templatePath))
                        {
                            using (FileStream stream = File.Open(templatePath, FileMode.Open))
                            {
                                StreamReader reader = new StreamReader(stream);
                                string html = reader.ReadToEnd();

                                html = html.Replace("[[BILLNO]]", pdfModal.billNo).
                                Replace("[[dateOfBilling]]", pdfModal.dateOfBilling.ToString("dd/MMM/yyyy")).
                                Replace("[[senderFirstAddress]]", sender.FirstAddress).
                                Replace("[[senderCompanyName]]", sender.ClientName).
                                Replace("[[senderGSTNo]]", sender.GSTNO).
                                Replace("[[senderSecondAddress]]", sender.SecondAddress).
                                Replace("[[senderPrimaryContactNo]]", sender.PrimaryPhoneNo).
                                Replace("[[senderEmail]]", sender.Email).
                                Replace("[[receiverCompanyName]]", receiver.ClientName).
                                Replace("[[receiverGSTNo]]", receiver.GSTNO).
                                Replace("[[receiverFirstAddress]]", receiver.FirstAddress).
                                Replace("[[receiverSecondAddress]]", receiver.SecondAddress).
                                Replace("[[receiverPrimaryContactNo]]", receiver.PrimaryPhoneNo).
                                Replace("[[receiverEmail]]", receiver.Email).
                                Replace("[[developerName]]", pdfModal.developerName).
                                Replace("[[billingMonth]]", pdfModal.billingMonth.ToString("MMMM")).
                                Replace("[[packageAmount]]", pdfModal.packageAmount.ToString()).
                                Replace("[[cGST]]", pdfModal.cGST.ToString()).
                                Replace("[[cGSTAmount]]", pdfModal.cGstAmount.ToString()).
                                Replace("[[sGST]]", pdfModal.sGST.ToString()).
                                Replace("[[sGSTAmount]]", pdfModal.sGstAmount.ToString()).
                                Replace("[[iGST]]", pdfModal.iGST.ToString()).
                                Replace("[[iGSTAmount]]", pdfModal.iGstAmount.ToString()).
                                Replace("[[grandTotalAmount]]", pdfModal.grandTotalAmount.ToString()).
                                Replace("[[bankName]]", sender.BankName).
                                Replace("[[clientName]]", sender.ClientName).
                                Replace("[[accountNumber]]", sender.AccountNo).
                                Replace("[[iFSCCode]]", sender.IFSC).
                                Replace("[[city]]", sender.City).
                                Replace("[[state]]", sender.State);

                                _fileMaker.ConvertToPDF(html, @"E:\ws\test.pdf");
                                this.iHTMLConverter.ToDocx(html, fileDetail.DiskFilePath, headerLogo);
                            }
                        }

                        int Year = Convert.ToInt32(pdfModal.billingMonth.ToString("yyyy"));
                        dbParams = new DbParam[]
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
                            new DbParam(UserType.Employee, typeof(int), "_UserTypeId"),
                            new DbParam(_currentSession.CurrentUserDetail.UserId, typeof(long), "_AdminId")
                        };

                        fileDetail.Status = this.db.ExecuteNonQuery("sp_filedetail_insupd", dbParams, true);
                        if (string.IsNullOrEmpty(fileDetail.Status))
                        {
                            List<Files> files = new List<Files>();
                            files.Add(new Files
                            {
                                FilePath = fileDetail.FilePath,
                                FileName = fileDetail.FileName
                            });
                            this.fileService.DeleteFiles(files);
                        }
                    }
                }

                responseModel.Result = fileDetail;

            }
            catch (Exception ex)
            {
                throw new Exception("Employee not found.");
            }
            return null;

        }

        private Bills GetBillData()
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

        private FileDetail GetFileDetail(PdfModal pdfModal, string fileExtension)
        {
            FileDetail fileDetail = new FileDetail();
            fileDetail.Status = "Client not selected";
            if (pdfModal.ClientId > 0)
            {
                try
                {
                    string MonthName = pdfModal.billingMonth.ToString("dd_MMM_yyyy");

                    string FolderLocation = Path.Combine(_fileLocationDetail.Location, _fileLocationDetail.BillsPath, MonthName);
                    string OldFileName = pdfModal.developerName.Replace(" ", "_") + "_" +
                                      MonthName + "_" +
                                      pdfModal.billNo.Replace("#", "") + "_" + pdfModal.UpdateSeqNo + fileExtension;

                    pdfModal.UpdateSeqNo++;
                    string FileName = pdfModal.developerName.Replace(" ", "_") + "_" +
                                      MonthName + "_" +
                                      pdfModal.billNo.Replace("#", "") + "_" + pdfModal.UpdateSeqNo + fileExtension;
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), FolderLocation);
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    string OldphysicalPath = Path.Combine(
                                            folderPath,
                                            OldFileName
                                    );

                    string physicalPath = Path.Combine(
                                            folderPath,
                                            FileName
                                    );

                    if (File.Exists(OldphysicalPath))
                        File.Delete(OldphysicalPath);

                    fileDetail.FilePath = FolderLocation;
                    fileDetail.DiskFilePath = physicalPath;
                    fileDetail.FileName = FileName;
                    fileDetail.FileExtension = fileExtension;
                    if (pdfModal.FileId > 0)
                        fileDetail.FileId = pdfModal.FileId;
                    else
                        fileDetail.FileId = -1;
                    fileDetail.ClientId = pdfModal.ClientId;
                    fileDetail.StatusId = 2;
                    fileDetail.PaidOn = null;
                    fileDetail.Status = "Generated";
                }
                catch (Exception)
                {
                    fileDetail.Status = "Got error while create PDF file.";
                }
            }
            return fileDetail;
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
                                            ParentFolder = n.ParentFolder,
                                            FileExtension = n.FileExtension,
                                            StatusId = 0,
                                            UserTypeId = (int)n.UserTypeId,
                                            AdminId = 1
                                        });

                        DataTable table = Converter.ToDataTable(fileInfo);
                        var dataSet = new DataSet();
                        dataSet.Tables.Add(table);
                        this.db.BatchInsert(ApplicationConstants.InserUserFileDetail, dataSet, true);
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
