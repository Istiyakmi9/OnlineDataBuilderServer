using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using DocMaker.ExcelMaker;
using DocMaker.HtmlToDocx;
using DocMaker.PdfService;
using EMailService.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace ServiceLayer.Code
{
    public class BillService : IBillService
    {
        private readonly IDb db;
        private readonly IFileService fileService;
        private readonly IHTMLConverter iHTMLConverter;
        private readonly FileLocationDetail _fileLocationDetail;
        private readonly CurrentSession _currentSession;
        private readonly IFileMaker _fileMaker;
        private readonly ILogger<BillService> _logger;
        private readonly ExcelWriter _excelWriter;
        private readonly HtmlToPdfConverter _htmlToPdfConverter;
        private readonly IEMailManager _eMailManager;
        private readonly ITemplateService _templateService;
        private readonly ITimesheetService _timesheetService;
        public BillService(IDb db, IFileService fileService, IHTMLConverter iHTMLConverter,
            FileLocationDetail fileLocationDetail,
            ILogger<BillService> logger,
            CurrentSession currentSession,
            ExcelWriter excelWriter,
            HtmlToPdfConverter htmlToPdfConverter,
            IEMailManager eMailManager,
            ITemplateService templateService,
            ITimesheetService timesheetService,
            IFileMaker fileMaker)
        {
            this.db = db;
            _logger = logger;
            _eMailManager = eMailManager;
            _htmlToPdfConverter = htmlToPdfConverter;
            this.fileService = fileService;
            this.iHTMLConverter = iHTMLConverter;
            _fileLocationDetail = fileLocationDetail;
            _currentSession = currentSession;
            _fileMaker = fileMaker;
            _excelWriter = excelWriter;
            _templateService = templateService;
            _timesheetService = timesheetService;
        }

        public FileDetail CreateFiles(BuildPdfTable _buildPdfTable, PdfModal pdfModal, Organization sender, Organization receiver)
        {
            FileDetail fileDetail = new FileDetail();
            string templatePath = Path.Combine(_fileLocationDetail.RootPath,
                _fileLocationDetail.Location,
                Path.Combine(_fileLocationDetail.HtmlTemplaePath.ToArray()),
                _fileLocationDetail.StaffingBillTemplate
            );

            string pdfTemplatePath = Path.Combine(_fileLocationDetail.RootPath,
                _fileLocationDetail.Location,
                Path.Combine(_fileLocationDetail.HtmlTemplaePath.ToArray()),
                _fileLocationDetail.StaffingBillPdfTemplate
            );

            string headerLogo = Path.Combine(_fileLocationDetail.RootPath, _fileLocationDetail.LogoPath, "logo.png");
            if (File.Exists(templatePath) && File.Exists(templatePath) && File.Exists(headerLogo))
            {
                fileDetail.LogoPath = headerLogo;
                string html = string.Empty;

                fileDetail.DiskFilePath = Path.Combine(_fileLocationDetail.RootPath, pdfModal.FilePath);
                if (!Directory.Exists(fileDetail.DiskFilePath)) Directory.CreateDirectory(fileDetail.DiskFilePath);
                fileDetail.FileName = pdfModal.FileName;
                string destinationFilePath = Path.Combine(fileDetail.DiskFilePath, fileDetail.FileName + $".{ApplicationConstants.Docx}");
                html = this.GetHtmlString(templatePath, pdfModal, sender, receiver);
                this.iHTMLConverter.ToDocx(html, destinationFilePath, headerLogo);

                _fileMaker._fileDetail = fileDetail;
                destinationFilePath = Path.Combine(fileDetail.DiskFilePath, fileDetail.FileName + $".{ApplicationConstants.Pdf}");
                html = this.GetHtmlString(pdfTemplatePath, pdfModal, sender, receiver, headerLogo);
                _htmlToPdfConverter.ConvertToPdf(html, destinationFilePath);
            }

            return fileDetail;
        }

        private string GetHtmlString(string templatePath, PdfModal pdfModal, Organization sender, Organization receiver, string logoPath = null)
        {
            string html = string.Empty;
            using (FileStream stream = File.Open(templatePath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(stream);
                html = reader.ReadToEnd();

                html = html.Replace("[[BILLNO]]", pdfModal.billNo).
                Replace("[[dateOfBilling]]", pdfModal.dateOfBilling.ToString("dd MMM, yyyy")).
                Replace("[[senderFirstAddress]]", sender.FirstAddress).
                Replace("[[senderCompanyName]]", sender.CompanyName).
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
                Replace("[[clientName]]", sender.CompanyName).
                Replace("[[accountNumber]]", sender.AccountNo).
                Replace("[[iFSCCode]]", sender.IFSC).
                Replace("[[city]]", sender.City).
                Replace("[[state]]", sender.State);
            }

            if (logoPath != null)
            {
                string extension = string.Empty;
                int lastPosition = logoPath.LastIndexOf(".");
                extension = logoPath.Substring(lastPosition + 1);
                ImageFormat imageFormat = null;
                if (extension == "png")
                    imageFormat = ImageFormat.Png;
                else if (extension == "gif")
                    imageFormat = ImageFormat.Gif;
                else if (extension == "bmp")
                    imageFormat = ImageFormat.Bmp;
                else if (extension == "jpeg")
                    imageFormat = ImageFormat.Jpeg;
                else if (extension == "tiff")
                {
                    // Convert tiff to gif.
                    extension = "gif";
                    imageFormat = ImageFormat.Gif;
                }
                else if (extension == "x-wmf")
                {
                    extension = "wmf";
                    imageFormat = ImageFormat.Wmf;
                }

                string encodeStart = $@"data:image/{imageFormat.ToString().ToLower()};base64";
                var fs = new FileStream(logoPath, FileMode.Open);
                using (BinaryReader br = new BinaryReader(fs))
                {
                    Byte[] bytes = br.ReadBytes((Int32)fs.Length);
                    string base64String = Convert.ToBase64String(bytes, 0, bytes.Length);
                    html = html.Replace("[[COMPANYLOGO_PATH]]", $"{encodeStart}, {base64String}");
                }
            }
            return html;
        }

        private void FillSenderDetail(Organization organization, PdfModal pdfModal)
        {
            pdfModal.senderCompanyName = organization.CompanyName;
            pdfModal.senderGSTNo = organization.GSTNO;
            pdfModal.senderEmail = organization.Email;
            pdfModal.senderFirstAddress = organization.FirstAddress;
            pdfModal.senderPrimaryContactNo = organization.PrimaryPhoneNo;
            pdfModal.senderSecondAddress = organization.SecondAddress;
        }

        private void FillReceiverDetail(Organization organization, PdfModal pdfModal)
        {
            pdfModal.receiverCompanyId = organization.ClientId;
            pdfModal.receiverCompanyName = organization.ClientName;
            pdfModal.receiverEmail = organization.Email;
            pdfModal.receiverFirstAddress = organization.FirstAddress;
            pdfModal.receiverPrimaryContactNo = organization.PrimaryPhoneNo;
            pdfModal.receiverSecondAddress = organization.SecondAddress;
            pdfModal.receiverThirdAddress = organization.ThirdAddress;
            pdfModal.receiverGSTNo = organization.GSTNO;
        }

        public dynamic UpdateGeneratedBillService(PdfModal pdfModal, List<DailyTimesheetDetail> dailyTimesheetDetails,
            TimesheetDetail timesheetDetail, string Comment)
        {
            if (timesheetDetail == null || timesheetDetail.TimesheetId == 0)
                throw new HiringBellException("Invalid timesheet submitted. Please check you detail.");

            return GenerateDocument(pdfModal, dailyTimesheetDetails, timesheetDetail, Comment);
        }

        public dynamic GenerateDocument(PdfModal pdfModal, List<DailyTimesheetDetail> dailyTimesheetDetails,
            TimesheetDetail timesheetDetail, string Comment)
        {
            List<string> emails = new List<string>();
            FileDetail fileDetail = new FileDetail();
            try
            {
                TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
                pdfModal.billingMonth = TimeZoneInfo.ConvertTimeFromUtc(pdfModal.billingMonth, istTimeZome);
                pdfModal.dateOfBilling = TimeZoneInfo.ConvertTimeFromUtc(pdfModal.dateOfBilling, istTimeZome);

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
                DataSet ds = new DataSet();
                DbParam[] dbParams = new DbParam[]
                {
                    new DbParam(pdfModal.receiverCompanyId, typeof(long), "_receiver"),
                    new DbParam(pdfModal.senderId, typeof(long), "_sender"),
                    new DbParam(pdfModal.billNo, typeof(string), "_billNo"),
                    new DbParam(pdfModal.EmployeeId, typeof(long), "_employeeId"),
                    new DbParam(UserType.Employee, typeof(int), "_userTypeId"),
                    new DbParam(pdfModal.billingMonth.Month, typeof(long), "_forMonth"),
                    new DbParam(pdfModal.billYear, typeof(long), "_forYear")
                };

                ds = this.db.GetDataset("sp_Billing_detail", dbParams);
                if (ds.Tables.Count == 5)
                {
                    sender = Converter.ToType<Organization>(ds.Tables[0]);
                    receiver = Converter.ToType<Organization>(ds.Tables[1]);

                    emails.Add(receiver.Email);
                    if (!string.IsNullOrEmpty(receiver.OtherEmail_1))
                        emails.Add(receiver.OtherEmail_1);
                    if (!string.IsNullOrEmpty(receiver.OtherEmail_2))
                        emails.Add(receiver.OtherEmail_2);
                    emails.Add(sender.Email);

                    fileDetail = Converter.ToType<FileDetail>(ds.Tables[2]);

                    this.FillReceiverDetail(receiver, pdfModal);
                    this.FillSenderDetail(sender, pdfModal);

                    this.ValidateBillModal(pdfModal);

                    if (fileDetail == null)
                    {
                        fileDetail = new FileDetail
                        {
                            ClientId = pdfModal.ClientId
                        };
                    }

                    List<AttendenceDetail> attendanceSet = new List<AttendenceDetail>();
                    if (ds.Tables[4].Rows.Count > 0)
                    {
                        var currentAttendance = Converter.ToType<Attendance>(ds.Tables[4]);
                        attendanceSet = JsonConvert.DeserializeObject<List<AttendenceDetail>>(currentAttendance.AttendanceDetail);
                    }

                    string templatePath = Path.Combine(_fileLocationDetail.RootPath,
                        _fileLocationDetail.Location,
                        Path.Combine(_fileLocationDetail.HtmlTemplaePath.ToArray()),
                        _fileLocationDetail.StaffingBillTemplate
                    );

                    string pdfTemplatePath = Path.Combine(_fileLocationDetail.RootPath,
                        _fileLocationDetail.Location,
                        Path.Combine(_fileLocationDetail.HtmlTemplaePath.ToArray()),
                        _fileLocationDetail.StaffingBillPdfTemplate
                    );

                    string headerLogo = Path.Combine(_fileLocationDetail.RootPath, _fileLocationDetail.LogoPath, "logo.png");
                    if (File.Exists(templatePath) && File.Exists(pdfTemplatePath) && File.Exists(headerLogo))
                    {
                        this.CleanOldFiles(fileDetail);
                        pdfModal.UpdateSeqNo++;
                        fileDetail.FileExtension = string.Empty;

                        string MonthName = pdfModal.billingMonth.ToString("MMM_yyyy");
                        string FolderLocation = Path.Combine(_fileLocationDetail.Location, _fileLocationDetail.BillsPath, MonthName);
                        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), FolderLocation);
                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        string destinationFilePath = Path.Combine(
                            folderPath,
                            pdfModal.developerName.Replace(" ", "_") + "_" +
                            pdfModal.billingMonth.ToString("MMM_yyyy") + "_" +
                            pdfModal.billNo + "_" +
                            pdfModal.UpdateSeqNo + $".{ApplicationConstants.Excel}");

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

                        // UpdateTimesheet
                        _timesheetService.UpdateTimesheetService(dailyTimesheetDetails, timesheetDetail, Comment);

                        var timeSheetDataSet = Converter.ToDataSet<TimesheetModel>(timesheetData);
                        _excelWriter.ToExcel(timeSheetDataSet.Tables[0], destinationFilePath, pdfModal.billingMonth.ToString("MMM_yyyy"));

                        GetFileDetail(pdfModal, fileDetail, ApplicationConstants.Docx);
                        fileDetail.LogoPath = headerLogo;
                        // Converting html context for docx conversion.
                        string html = this.GetHtmlString(templatePath, pdfModal, sender, receiver);
                        destinationFilePath = Path.Combine(fileDetail.DiskFilePath, fileDetail.FileName + $".{ApplicationConstants.Docx}");
                        this.iHTMLConverter.ToDocx(html, destinationFilePath, headerLogo);

                        GetFileDetail(pdfModal, fileDetail, ApplicationConstants.Pdf);
                        _fileMaker._fileDetail = fileDetail;
                        // Converting html context for pdf conversion.
                        html = this.GetHtmlString(pdfTemplatePath, pdfModal, sender, receiver, headerLogo);
                        destinationFilePath = Path.Combine(fileDetail.DiskFilePath, fileDetail.FileName + $".{ApplicationConstants.Pdf}");
                        _htmlToPdfConverter.ConvertToPdf(html, destinationFilePath);

                        dbParams = new DbParam[]
                        {
                            new DbParam(fileDetail.FileId, typeof(long), "_FileId"),
                            new DbParam(pdfModal.receiverCompanyId, typeof(long), "_ClientId"),
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
                            new DbParam(pdfModal.billYear, typeof(int), "_BillYear"),
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
                            new DbParam(pdfModal.IsCustomBill, typeof(bool), "_IsCustomBill"),
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
                            this.fileService.DeleteFiles(files);
                        }
                        else
                        {
                            fileDetail.FileId = Convert.ToInt32(fileId);
                            fileDetail.DiskFilePath = null;
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
                _logger.LogError($"{e.UserMessage} Field: {e.FieldName} Value: {e.FieldValue}");
                throw e.BuildBadRequest(e.UserMessage, e.FieldName, e.FieldValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new HiringBellException(ex.Message, ex);
            }

            EmailTemplate emailTemplate = _templateService.GetBillingTemplateDetailService();
            emailTemplate.Emails = emails;
            return new { FileDetail = fileDetail, EmailTemplate = emailTemplate };
        }

        private void ValidateBillModal(PdfModal pdfModal)
        {
            decimal grandTotalAmount = 0;
            decimal sGSTAmount = 0;
            decimal cGSTAmount = 0;
            decimal iGSTAmount = 0;
            int days = DateTime.DaysInMonth(pdfModal.billingMonth.Year, pdfModal.billingMonth.Month);

            if (pdfModal.ClientId <= 0)
                throw new HiringBellException { UserMessage = "Invald Client.", FieldName = nameof(pdfModal.ClientId), FieldValue = pdfModal.ClientId.ToString() };

            if (pdfModal.EmployeeId <= 0)
                throw new HiringBellException { UserMessage = "Invalid Employee", FieldName = nameof(pdfModal.EmployeeId), FieldValue = pdfModal.EmployeeId.ToString() };

            if (pdfModal.senderId <= 0)
                throw new HiringBellException { UserMessage = "Invalid Sender", FieldName = nameof(pdfModal.senderId), FieldValue = pdfModal.senderId.ToString() };

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

            if (!pdfModal.IsCustomBill && (pdfModal.workingDay < 0 || pdfModal.workingDay > days))
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
            Bills bill = db.Get<Bills>("sp_billdata_get", new { BillTypeUid = 1 });
            if (bill == null)
                throw new HiringBellException("Unable to get bill detail for current employee.");

            return bill;
        }

        private void CleanOldFiles(FileDetail fileDetail)
        {
            // Old file name and path
            if (!string.IsNullOrEmpty(fileDetail.FilePath))
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
                        string ExistingFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileDetail.FilePath, fileDetail.FileName + "." + ApplicationConstants.Docx);
                        if (File.Exists(ExistingFilePath))
                            File.Delete(ExistingFilePath);

                        ExistingFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileDetail.FilePath, fileDetail.FileName + "." + ApplicationConstants.Pdf);
                        if (File.Exists(ExistingFilePath))
                            File.Delete(ExistingFilePath);
                    }
                }
            }
        }

        private void GetFileDetail(PdfModal pdfModal, FileDetail fileDetail, string fileExtension)
        {
            fileDetail.Status = 0;
            if (pdfModal.ClientId > 0)
            {
                try
                {
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
                string FolderPath = Path.Combine(_fileLocationDetail.Location, $"GSTFile_{createPageModel.Billno}");
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
                        this.db.BatchInsert(ApplicationConstants.InserUserFileDetail, table, true);
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

        public string SendBillToClientService(GenerateBillFileDetail generateBillFileDetail)
        {
            var resultSet = db.FetchDataSet("sp_sendingbill_email_get_detail", new
            {
                generateBillFileDetail.SenderId,
                generateBillFileDetail.ClientId,
                generateBillFileDetail.FileId,
                generateBillFileDetail.EmployeeId
            });

            if (resultSet != null && resultSet.Tables.Count == 4)
            {
                var file = Converter.ToList<FileDetail>(resultSet.Tables[0]);
                var employee = Converter.ToType<Employee>(resultSet.Tables[1]);
                var emailSetting = Converter.ToType<EmailSettingDetail>(resultSet.Tables[2]);
                var emailTempalte = Converter.ToType<EmailTemplate>(resultSet.Tables[3]);

                List<string> emails = generateBillFileDetail.EmailTemplateDetail.Emails;
                if (emails.Count == 0)
                    throw new HiringBellException("No receiver address added. Please add atleast one email address.");

                UpdateTemplateData(emailTempalte, employee, generateBillFileDetail.MonthName, generateBillFileDetail.ForYear);
                Task.Run(() =>
                {
                    EmailSenderModal emailSenderModal = new EmailSenderModal
                    {
                        To = emails, //receiver.Email,
                        From = emailSetting.EmailAddress, //sender.Email,
                        UserName = emailSetting.EmailName,
                        CC = new List<string>(),
                        BCC = new List<string>(),
                        Subject = emailTempalte.SubjectLine,
                        FileDetails = file,
                        Body = emailTempalte.BodyContent,
                        EmailSettingDetails = emailSetting
                    };

                    _eMailManager.SendMail(emailSenderModal);
                });

            }

            return ApplicationConstants.Successfull;
        }

        private void UpdateTemplateData(EmailTemplate template, Employee employee, string month, int year)
        {
            if (template != null && !string.IsNullOrEmpty(template.BodyContent))
            {
                template.BodyContent = JsonConvert.DeserializeObject<string>(template.BodyContent
                    .Replace("[[DEVELOPER-NAME]]", employee.FirstName + " " + employee.LastName)
                    .Replace("[[YEAR]]", year.ToString())
                    .Replace("[[MONTH]]", month));
            }
        }
    }
}
