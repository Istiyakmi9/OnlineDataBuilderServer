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
using System.Globalization;
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

        public FileDetail GenerateDocument(BuildPdfTable _buildPdfTable, PdfModal pdfModal)
        {
            //this.iHTMLConverter.ToHtml();
            FileDetail fileDetail = new FileDetail();
            try
            {
                TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
                pdfModal.billingMonth = TimeZoneInfo.ConvertTimeFromUtc(pdfModal.billingMonth, istTimeZome);
                pdfModal.dateOfBilling = TimeZoneInfo.ConvertTimeFromUtc(pdfModal.dateOfBilling, istTimeZome);
                this.ValidateBillModal(pdfModal);

                Bills bill = this.GetBillData();
                if (string.IsNullOrEmpty(pdfModal.billNo))
                {
                    if (bill == null || string.IsNullOrEmpty(bill.GeneratedBillNo))
                    {
                        throw new HiringBellException("Fail to generate bill no.");
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

                pdfModal.billNo = pdfModal.billNo.Replace("#", "");

                Organization sender = null;
                Organization receiver = null;
                Employee employeeMappedClient = null;
                DataSet ds = new DataSet();
                DbParam[] dbParams = new DbParam[]
                {
                            new DbParam(pdfModal.ClientId, typeof(long), "_receiver"),
                            new DbParam(pdfModal.senderClientId, typeof(long), "_sender"),
                            new DbParam(pdfModal.billNo, typeof(long), "_billNo"),
                            new DbParam(pdfModal.EmployeeId, typeof(long), "_employeeId")
                };

                ds = this.db.GetDataset("sp_Billing_detail", dbParams);
                if (ds.Tables.Count == 4)
                {
                    List<Organization> SenderDetails = Converter.ToList<Organization>(ds.Tables[0]);
                    List<Organization> ReceiverDetails = Converter.ToList<Organization>(ds.Tables[1]);
                    List<Employee> EmployeeMappedClient = Converter.ToList<Employee>(ds.Tables[2]);
                    fileDetail = Converter.ToList<FileDetail>(ds.Tables[3]).FirstOrDefault();

                    if (fileDetail == null)
                    {
                        fileDetail = new FileDetail
                        {
                            ClientId = pdfModal.ClientId
                        };
                    }

                    sender = SenderDetails.Single();
                    receiver = ReceiverDetails.Single();
                    employeeMappedClient = EmployeeMappedClient.Single();

                    string rootPath = _hostingEnvironment.ContentRootPath;
                    string templatePath = Path.Combine(rootPath,
                        _fileLocationDetail.Location,
                        Path.Combine(_fileLocationDetail.HtmlTemplaePath.ToArray()),
                        _fileLocationDetail.StaffingBillTemplate
                    );

                    string headerLogo = Path.Combine(rootPath, _fileLocationDetail.Location, "Logos", "logo.png");
                    if (File.Exists(templatePath) && File.Exists(headerLogo))
                    {
                        pdfModal.UpdateSeqNo++;
                        GetFileDetail(pdfModal, fileDetail, ApplicationConstants.Docx);
                        fileDetail.LogoPath = headerLogo;

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

                            string destinationFilePath = Path.Combine(fileDetail.DiskFilePath, fileDetail.FileName + $".{ApplicationConstants.Docx}");
                            this.iHTMLConverter.ToDocx(html, destinationFilePath, headerLogo);
                        }

                        GetFileDetail(pdfModal, fileDetail, ApplicationConstants.Pdf);
                        _fileMaker._fileDetail = fileDetail;
                        _fileMaker.BuildPdfBill(_buildPdfTable, pdfModal, sender);

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

                        var status = this.db.ExecuteNonQuery("sp_filedetail_insupd", dbParams, true);
                        if (string.IsNullOrEmpty(status))
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
                    else
                    {
                        throw new HiringBellException("HTML template or Logo file path is invalid");
                    }
                }
                else
                {
                    throw new HiringBellException("Amount calculation is not matching", nameof(pdfModal.grandTotalAmount), pdfModal.grandTotalAmount.ToString());
                }
            }
            catch (HiringBellException e)
            {
                throw e.BuildBadRequest(e.UserMessage, e.FieldName, e.FieldValue);
            }
            catch (Exception ex)
            {
                throw new HiringBellException(ex.Message, ex);
            }

            return fileDetail;
        }

        private void ValidateBillModal(PdfModal pdfModal)
        {
            double grandTotalAmount = 0.00;
            double sGSTAmount = 0.00;
            double cGSTAmount = 0.00;
            double iGSTAmount = 0.00;
            int days = DateTime.DaysInMonth(pdfModal.billingMonth.Year, pdfModal.billingMonth.Month);

            if (pdfModal.ClientId <= 0)
                throw new HiringBellException { UserMessage = "Invald Client.", FieldName = nameof(pdfModal.ClientId), FieldValue = pdfModal.ClientId.ToString() };

            if (pdfModal.EmployeeId <= 0)
                throw new HiringBellException { UserMessage = "Invalid Employee", FieldName = nameof(pdfModal.EmployeeId), FieldValue = pdfModal.EmployeeId.ToString() };

            if (pdfModal.senderClientId <= 0)
                throw new HiringBellException { UserMessage = "Invalid Sender", FieldName = nameof(pdfModal.senderClientId), FieldValue = pdfModal.senderClientId.ToString() };

            if (pdfModal.packageAmount <= 0)
                throw new HiringBellException { UserMessage = "Invalid Package Amount", FieldName = nameof(pdfModal.packageAmount), FieldValue = pdfModal.packageAmount.ToString() };

            if (pdfModal.cGST < 0)
                throw new HiringBellException { UserMessage = "Invalid CGST", FieldName = nameof(pdfModal.cGST), FieldValue = pdfModal.cGST.ToString() };

            if (pdfModal.iGST < 0)
                throw new HiringBellException { UserMessage = "Invalid IGST", FieldName = nameof(pdfModal.iGST), FieldValue = pdfModal.iGST.ToString() };

            if (pdfModal.sGST < 0)
                throw new HiringBellException { UserMessage = "Invalid SGST", FieldName = nameof(pdfModal.sGST), FieldValue = pdfModal.sGST.ToString() };

            if (pdfModal.cGstAmount < 0)
                throw new HiringBellException { UserMessage = "Invalid CGST Amount", FieldName = nameof(pdfModal.cGstAmount), FieldValue = pdfModal.cGstAmount.ToString() };

            if (pdfModal.iGstAmount < 0)
                throw new HiringBellException { UserMessage = "Invalid IGST Amount", FieldName = nameof(pdfModal.iGstAmount), FieldValue = pdfModal.iGstAmount.ToString() };

            if (pdfModal.sGstAmount < 0)
                throw new HiringBellException { UserMessage = "Invalid CGST Amount", FieldName = nameof(pdfModal.cGstAmount), FieldValue = pdfModal.cGstAmount.ToString() };

            if (pdfModal.cGST > 0 || pdfModal.sGST > 0 || pdfModal.iGST > 0)
            {
                sGSTAmount = Converter.TwoDecimalValue((pdfModal.packageAmount * pdfModal.sGST) / 100);
                cGSTAmount = Converter.TwoDecimalValue((pdfModal.packageAmount * pdfModal.cGST) / 100);
                iGSTAmount = Converter.TwoDecimalValue((pdfModal.packageAmount * pdfModal.iGST) / 100);
                grandTotalAmount = Converter.TwoDecimalValue(pdfModal.packageAmount + (cGSTAmount + sGSTAmount + iGSTAmount));
            }
            else
            {
                grandTotalAmount = pdfModal.packageAmount;
            }

            if (pdfModal.grandTotalAmount != grandTotalAmount)
                throw new HiringBellException { UserMessage = "Total Amount calculation is not matching", FieldName = nameof(pdfModal.grandTotalAmount), FieldValue = pdfModal.grandTotalAmount.ToString() };

            if (pdfModal.sGstAmount != sGSTAmount)
                throw new HiringBellException { UserMessage = "SGST Amount invalid calculation", FieldName = nameof(pdfModal.sGstAmount), FieldValue = pdfModal.sGstAmount.ToString() };

            if (pdfModal.iGstAmount != iGSTAmount)
                throw new HiringBellException { UserMessage = "IGST Amount invalid calculation", FieldName = nameof(pdfModal.iGstAmount), FieldValue = pdfModal.iGstAmount.ToString() };

            if (pdfModal.cGstAmount != cGSTAmount)
                throw new HiringBellException { UserMessage = "CGST Amount invalid calculation", FieldName = nameof(pdfModal.cGstAmount), FieldValue = pdfModal.cGstAmount.ToString() };

            if (pdfModal.workingDay < 0 || pdfModal.workingDay > days)
                throw new HiringBellException { UserMessage = "Invalid Working days", FieldName = nameof(pdfModal.workingDay), FieldValue = pdfModal.workingDay.ToString() };

            if (pdfModal.billingMonth.Month < 0 || pdfModal.billingMonth.Month > 12)
                throw new HiringBellException { UserMessage = "Invalid billing month", FieldName = nameof(pdfModal.billingMonth), FieldValue = pdfModal.billingMonth.ToString() };

            if (pdfModal.daysAbsent < 0 || pdfModal.daysAbsent > days)
                throw new HiringBellException { UserMessage = "Invalid No of days absent", FieldName = nameof(pdfModal.daysAbsent), FieldValue = pdfModal.daysAbsent.ToString() };

            if (pdfModal.dateOfBilling == null)
            {
                throw new HiringBellException { UserMessage = "Invalid date of Billing", FieldName = nameof(pdfModal.dateOfBilling), FieldValue = pdfModal.dateOfBilling.ToString() };
            }
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

        private void GetFileDetail(PdfModal pdfModal, FileDetail fileDetail, string fileExtension)
        {
            fileDetail.Status = 0;
            if (pdfModal.ClientId > 0)
            {
                try
                {
                    // Old file name and path
                    if (!string.IsNullOrEmpty(fileDetail.FilePath) && fileExtension == fileDetail.FileExtension)
                    {
                        string ExistingFolder = Path.Combine(Directory.GetCurrentDirectory(), fileDetail.FilePath);
                        if (Directory.Exists(ExistingFolder))
                        {
                            if (Directory.GetFiles(ExistingFolder).Length == 0)
                            {
                                Directory.Delete(ExistingFolder);
                            }
                            else
                            {
                                string ExistingFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileDetail.FilePath, fileDetail.FileName);
                                if (File.Exists(ExistingFilePath))
                                    File.Delete(ExistingFilePath);
                            }
                        }
                    }

                    string MonthName = pdfModal.billingMonth.ToString("MMM_yyyy");
                    string FolderLocation = Path.Combine(_fileLocationDetail.Location, _fileLocationDetail.BillsPath, MonthName);
                    string FileName = pdfModal.developerName.Replace(" ", "_") + "_" +
                                      MonthName + "_" +
                                      pdfModal.billNo.Replace("#", "") + "_" + pdfModal.UpdateSeqNo;

                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), FolderLocation);
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    fileDetail.FilePath = FolderLocation;
                    fileDetail.DiskFilePath = folderPath;
                    fileDetail.FileName = FileName;
                    if (string.IsNullOrEmpty(fileDetail.FileExtension))
                        fileDetail.FileExtension = fileExtension;
                    else
                        fileDetail.FileExtension += $",{fileExtension}";
                    if (pdfModal.FileId > 0)
                        fileDetail.FileId = pdfModal.FileId;
                    else
                        fileDetail.FileId = -1;
                    fileDetail.StatusId = 2;
                    fileDetail.PaidOn = null;
                    fileDetail.Status = 1;
                }
                catch (Exception ex)
                {
                    fileDetail.Status = -1;
                    throw ex;
                }
            }
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
