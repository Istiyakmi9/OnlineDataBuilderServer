using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using DocMaker.ExcelMaker;
using DocMaker.HtmlToDocx;
using DocMaker.PdfService;
using EMailService.Service;
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
        private readonly ITimezoneConverter _timezoneConverter;

        public BillService(IDb db, IFileService fileService, IHTMLConverter iHTMLConverter,
            FileLocationDetail fileLocationDetail,
            ILogger<BillService> logger,
            CurrentSession currentSession,
            ExcelWriter excelWriter,
            HtmlToPdfConverter htmlToPdfConverter,
            IEMailManager eMailManager,
            ITemplateService templateService,
            ITimezoneConverter timezoneConverter,
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
            _timezoneConverter = timezoneConverter;
        }

        public FileDetail CreateFiles(BuildPdfTable _buildPdfTable, BillGenerationModal billModal)
        {
            FileDetail fileDetail = new FileDetail();
            billModal.BillTemplatePath = Path.Combine(_fileLocationDetail.RootPath,
                _fileLocationDetail.Location,
                Path.Combine(_fileLocationDetail.HtmlTemplaePath.ToArray()),
                _fileLocationDetail.StaffingBillTemplate
            );

            billModal.PdfTemplatePath = Path.Combine(_fileLocationDetail.RootPath,
                _fileLocationDetail.Location,
                Path.Combine(_fileLocationDetail.HtmlTemplaePath.ToArray()),
                _fileLocationDetail.StaffingBillPdfTemplate
            );

            billModal.HeaderLogoPath = Path.Combine(_fileLocationDetail.RootPath, _fileLocationDetail.LogoPath, "logo.png");
            if (File.Exists(billModal.BillTemplatePath) && File.Exists(billModal.HeaderLogoPath))
            {
                fileDetail.LogoPath = billModal.HeaderLogoPath;
                string html = string.Empty;

                fileDetail.DiskFilePath = Path.Combine(_fileLocationDetail.RootPath, billModal.PdfModal.FilePath);
                if (!Directory.Exists(fileDetail.DiskFilePath)) Directory.CreateDirectory(fileDetail.DiskFilePath);
                fileDetail.FileName = billModal.PdfModal.FileName;
                string destinationFilePath = Path.Combine(fileDetail.DiskFilePath, fileDetail.FileName + $".{ApplicationConstants.Docx}");
                
                html = this.GetHtmlString(billModal.BillTemplatePath, billModal);
                this.iHTMLConverter.ToDocx(html, destinationFilePath, billModal.HeaderLogoPath);

                _fileMaker._fileDetail = fileDetail;
                destinationFilePath = Path.Combine(fileDetail.DiskFilePath, fileDetail.FileName + $".{ApplicationConstants.Pdf}");
                
                html = this.GetHtmlString(billModal.PdfTemplatePath, billModal, true);
                _htmlToPdfConverter.ConvertToPdf(html, destinationFilePath);
            }

            return fileDetail;
        }

        private string GetHtmlString(string templatePath, BillGenerationModal billModal, bool isHeaderLogoRequired = false)
        {
            string html = string.Empty;
            using (FileStream stream = File.Open(templatePath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(stream);
                html = reader.ReadToEnd();

                html = html.Replace("[[BILLNO]]", billModal.PdfModal.billNo).
                Replace("[[dateOfBilling]]", billModal.PdfModal.dateOfBilling.ToString("dd MMM, yyyy")).
                Replace("[[senderFirstAddress]]", billModal.Sender.FirstAddress).
                Replace("[[senderCompanyName]]", billModal.Sender.CompanyName).
                Replace("[[senderGSTNo]]", billModal.Sender.GSTNO).
                Replace("[[senderSecondAddress]]", billModal.Sender.SecondAddress).
                Replace("[[senderPrimaryContactNo]]", billModal.Sender.PrimaryPhoneNo).
                Replace("[[senderEmail]]", billModal.Sender.Email).
                Replace("[[receiverCompanyName]]", billModal.Receiver.ClientName).
                Replace("[[receiverGSTNo]]", billModal.Receiver.GSTNO).
                Replace("[[receiverFirstAddress]]", billModal.Receiver.FirstAddress).
                Replace("[[receiverSecondAddress]]", billModal.Receiver.SecondAddress).
                Replace("[[receiverPrimaryContactNo]]", billModal.Receiver.PrimaryPhoneNo).
                Replace("[[receiverEmail]]", billModal.Receiver.Email).
                Replace("[[developerName]]", billModal.PdfModal.developerName).
                Replace("[[billingMonth]]", billModal.PdfModal.billingMonth.ToString("MMMM")).
                Replace("[[packageAmount]]", billModal.PdfModal.packageAmount.ToString()).
                Replace("[[cGST]]", billModal.PdfModal.cGST.ToString()).
                Replace("[[cGSTAmount]]", billModal.PdfModal.cGstAmount.ToString()).
                Replace("[[sGST]]", billModal.PdfModal.sGST.ToString()).
                Replace("[[sGSTAmount]]", billModal.PdfModal.sGstAmount.ToString()).
                Replace("[[iGST]]", billModal.PdfModal.iGST.ToString()).
                Replace("[[iGSTAmount]]", billModal.PdfModal.iGstAmount.ToString()).
                Replace("[[grandTotalAmount]]", billModal.PdfModal.grandTotalAmount.ToString()).
                Replace("[[state]]", billModal.Sender.State).
                Replace("[[clientName]]", billModal.Sender.CompanyName).
                Replace("[[city]]", billModal.SenderBankDetail.Branch).
                Replace("[[bankName]]", billModal.SenderBankDetail.BankName).
                Replace("[[accountNumber]]", billModal.SenderBankDetail.AccountNo).
                Replace("[[iFSCCode]]", billModal.SenderBankDetail.IFSC);
            }

            if (!string.IsNullOrEmpty(billModal.HeaderLogoPath) && isHeaderLogoRequired)
            {
                string extension = string.Empty;
                int lastPosition = billModal.HeaderLogoPath.LastIndexOf(".");
                extension = billModal.HeaderLogoPath.Substring(lastPosition + 1);
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
                var fs = new FileStream(billModal.HeaderLogoPath, FileMode.Open);
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

        public async Task<dynamic> UpdateGeneratedBillService(BillGenerationModal billModal)
        {
            if (billModal.TimesheetDetail == null || billModal.TimesheetDetail.TimesheetId == 0)
                throw new HiringBellException("Invalid timesheet submitted. Please check you detail.");

            return await GenerateBillService(billModal);
        }

        private void ValidateRequestDetail(PdfModal pdfModal)
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


        private async Task GetBillNumber(PdfModal pdfModal, BillGenerationModal billGenerationModal)
        {
            Bills bill = Converter.ToType<Bills>(billGenerationModal.ResultSet.Tables[4]);
            if (bill == null || string.IsNullOrEmpty(bill.GeneratedBillNo))
                throw new HiringBellException("Bill sequence number not found. Please contact to admin.");

            billGenerationModal.BillSequence = bill;
            if (string.IsNullOrEmpty(pdfModal.billNo))
            {
                pdfModal.billNo = bill.GeneratedBillNo.Replace("#", "");
            }
            else
            {
                pdfModal.billNo = pdfModal.billNo.Replace("#", "");
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

            await Task.CompletedTask;
        }

        private async Task<DataSet> PrepareRequestForBillGeneration(BillGenerationModal billGenerationModal)
        {
            DataSet ds = this.db.FetchDataSet("sp_Billing_detail", new
            {
                Sender = billGenerationModal.PdfModal.senderId,
                Receiver = billGenerationModal.PdfModal.receiverCompanyId,
                BillNo = billGenerationModal.PdfModal.billNo,
                billGenerationModal.PdfModal.EmployeeId,
                UserTypeId = (int)UserType.Employee,
                ForMonth = billGenerationModal.PdfModal.billingMonth.Month,
                ForYear = billGenerationModal.PdfModal.billYear,
                BillTypeUid = 1,
                CompanyId = 0
            });

            if (ds == null || ds.Tables.Count != 6)
                throw new HiringBellException("Fail to get billing detail. Please contact to admin.");

            billGenerationModal.ResultSet = ds;
            await GetBillNumber(billGenerationModal.PdfModal, billGenerationModal);

            await BuildBackAccountDetail(billGenerationModal);

            return ds;
        }

        private async Task BuildBackAccountDetail(BillGenerationModal billModal)
        {
            if (billModal.ResultSet.Tables[5] == null || billModal.ResultSet.Tables[5].Rows.Count != 1)
                throw new HiringBellException("Unable to find sender back account detail.");

            billModal.SenderBankDetail = Converter.ToType<BankDetail>(billModal.ResultSet.Tables[5]);

            await Task.CompletedTask;
        }

        private async Task<Organization> GetSenderDetail(DataSet ds, List<string> emails)
        {
            if (ds.Tables[0].Rows.Count != 1)
                throw new HiringBellException("Fail to get company detail. Please contact to admin.");

            Organization sender = Converter.ToType<Organization>(ds.Tables[0]);
            if (sender == null)
                throw new HiringBellException("Fail to get company detail. Please contact to admin.");

            emails.Add(sender.Email);
            return await Task.FromResult(sender);
        }

        private async Task<Organization> GetReceiverDetail(DataSet ds, List<string> emails)
        {
            if (ds.Tables[1].Rows.Count != 1)
                throw new HiringBellException("Fail to get client detail. Please contact to admin.");

            Organization receiver = Converter.ToType<Organization>(ds.Tables[1]);
            if (receiver == null)
                throw new HiringBellException("Fail to get client detail. Please contact to admin.");

            emails.Add(receiver.Email);
            if (!string.IsNullOrEmpty(receiver.OtherEmail_1))
                emails.Add(receiver.OtherEmail_1);
            if (!string.IsNullOrEmpty(receiver.OtherEmail_2))
                emails.Add(receiver.OtherEmail_2);

            return await Task.FromResult(receiver);
        }

        private async Task<FileDetail> GetBillFileDetail(PdfModal pdfModal, DataSet ds)
        {
            FileDetail fileDetail = Converter.ToType<FileDetail>(ds.Tables[2]);
            if (fileDetail == null)
            {
                fileDetail = new FileDetail
                {
                    ClientId = pdfModal.ClientId
                };
            }

            return await Task.FromResult(fileDetail);
        }

        private async Task CaptureFileFolderLocations(BillGenerationModal billModal)
        {
            billModal.BillTemplatePath = Path.Combine(_fileLocationDetail.RootPath,
                        _fileLocationDetail.Location,
                        Path.Combine(_fileLocationDetail.HtmlTemplaePath.ToArray()),
                        _fileLocationDetail.StaffingBillTemplate
                    );

            if (!File.Exists(billModal.BillTemplatePath))
                throw new HiringBellException("Billing template not found. Please contact to admin.");



            billModal.PdfTemplatePath = Path.Combine(_fileLocationDetail.RootPath,
                _fileLocationDetail.Location,
                Path.Combine(_fileLocationDetail.HtmlTemplaePath.ToArray()),
                _fileLocationDetail.StaffingBillPdfTemplate
            );

            if (!File.Exists(billModal.PdfTemplatePath))
                throw new HiringBellException("PDF template not found. Please contact to admin.");

            billModal.HeaderLogoPath = Path.Combine(_fileLocationDetail.RootPath,
                _fileLocationDetail.LogoPath, "logo.png");

            if (!File.Exists(billModal.HeaderLogoPath))
                throw new HiringBellException("Logo image not found. Please contact to admin.");

            await Task.CompletedTask;
        }

        private async Task GenerateUpdateTimesheet(BillGenerationModal billModal)
        {

            List<AttendenceDetail> attendanceSet = new List<AttendenceDetail>();
            if (billModal.ResultSet.Tables[3].Rows.Count > 0)
            {
                var currentAttendance = Converter.ToType<Attendance>(billModal.ResultSet.Tables[3]);
                attendanceSet = JsonConvert.DeserializeObject<List<AttendenceDetail>>(currentAttendance.AttendanceDetail);
            }

            string FolderLocation = Path.Combine(
                _fileLocationDetail.Location,
                _fileLocationDetail.BillsPath,
                billModal.PdfModal.billingMonth.ToString("MMM_yyyy"));

            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), FolderLocation);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string destinationFilePath = Path.Combine(
                folderPath,
                billModal.PdfModal.developerName.Replace(" ", "_") + "_" +
                billModal.PdfModal.billingMonth.ToString("MMM_yyyy") + "_" +
                billModal.PdfModal.billNo + "_" +
                billModal.PdfModal.UpdateSeqNo + $".{ApplicationConstants.Excel}");

            if (File.Exists(destinationFilePath))
                File.Delete(destinationFilePath);

            var timesheetData = (from n in attendanceSet
                                 orderby n.AttendanceDay ascending
                                 select new TimesheetModel
                                 {
                                     Date = n.AttendanceDay.ToString("dd MMM yyyy"),
                                     ResourceName = billModal.PdfModal.developerName,
                                     StartTime = "10:00 AM",
                                     EndTime = "06:00 PM",
                                     TotalHrs = 9,
                                     Comments = n.UserComments,
                                     Status = ItemStatus.Approved.ToString()
                                 }
            ).ToList<TimesheetModel>();

            // UpdateTimesheet
            _timesheetService.UpdateTimesheetService(billModal.FullTimeSheet,
                billModal.TimesheetDetail, billModal.Comment);

            var timeSheetDataSet = Converter.ToDataSet<TimesheetModel>(timesheetData);
            _excelWriter.ToExcel(timeSheetDataSet.Tables[0],
                destinationFilePath,
                billModal.PdfModal.billingMonth.ToString("MMM_yyyy"));

            await Task.CompletedTask;
        }

        private async Task GeneratePdfFile(BillGenerationModal billModal)
        {
            GetFileDetail(billModal.PdfModal, billModal.FileDetail, ApplicationConstants.Pdf);
            _fileMaker._fileDetail = billModal.FileDetail;

            // Converting html context for pdf conversion.
            var html = this.GetHtmlString(billModal.PdfTemplatePath, billModal, true);

            var destinationFilePath = Path.Combine(billModal.FileDetail.DiskFilePath,
                billModal.FileDetail.FileName + $".{ApplicationConstants.Pdf}");
            _htmlToPdfConverter.ConvertToPdf(html, destinationFilePath);

            await Task.CompletedTask;
        }

        private async Task GenerateDocxFile(BillGenerationModal billModal)
        {
            GetFileDetail(billModal.PdfModal, billModal.FileDetail, ApplicationConstants.Docx);
            billModal.FileDetail.LogoPath = billModal.HeaderLogoPath;

            // Converting html context for docx conversion.
            string html = this.GetHtmlString(billModal.BillTemplatePath, billModal);

            var destinationFilePath = Path.Combine(billModal.FileDetail.DiskFilePath,
                billModal.FileDetail.FileName + $".{ApplicationConstants.Docx}");
            this.iHTMLConverter.ToDocx(html, destinationFilePath, billModal.HeaderLogoPath);

            await Task.CompletedTask;
        }

        private async Task SaveExecuteBill(BillGenerationModal billModal)
        {
            var dbResult = await this.db.ExecuteAsync("sp_filedetail_insupd", new
            {
                FileId = billModal.FileDetail.FileId,
                ClientId = billModal.PdfModal.receiverCompanyId,
                FileName = billModal.FileDetail.FileName,
                FilePath = billModal.FileDetail.FilePath,
                FileExtension = billModal.FileDetail.FileExtension,
                StatusId = billModal.PdfModal.StatusId,
                GeneratedBillNo = billModal.BillSequence.NextBillNo,
                BillUid = billModal.BillSequence.BillUid,
                BillDetailId = billModal.PdfModal.billId,
                BillNo = billModal.PdfModal.billNo,
                PaidAmount = billModal.PdfModal.packageAmount,
                BillForMonth = billModal.PdfModal.billingMonth.Month,
                BillYear = billModal.PdfModal.billYear,
                NoOfDays = billModal.PdfModal.workingDay,
                NoOfDaysAbsent = billModal.PdfModal.daysAbsent,
                IGST = billModal.PdfModal.iGST,
                SGST = billModal.PdfModal.sGST,
                CGST = billModal.PdfModal.cGST,
                TDS = ApplicationConstants.TDS,
                BillStatusId = ApplicationConstants.Pending,
                PaidOn = billModal.PdfModal.PaidOn,
                FileDetailId = billModal.PdfModal.FileId,
                UpdateSeqNo = billModal.PdfModal.UpdateSeqNo,
                EmployeeUid = billModal.PdfModal.EmployeeId,
                BillUpdatedOn = billModal.PdfModal.dateOfBilling,
                IsCustomBill = billModal.PdfModal.IsCustomBill,
                UserTypeId = (int)UserType.Employee,
                AdminId = _currentSession.CurrentUserDetail.UserId
            }, true);

            if (dbResult.rowsEffected != 0 || string.IsNullOrEmpty(dbResult.statusMessage))
            {
                List<Files> files = new List<Files>();
                files.Add(new Files
                {
                    FilePath = billModal.FileDetail.FilePath,
                    FileName = billModal.FileDetail.FileName
                });
                this.fileService.DeleteFiles(files);

                throw new HiringBellException("Bill generated task fail. Please contact to admin.");
            }

            billModal.FileDetail.FileId = Convert.ToInt32(dbResult.statusMessage);
            billModal.FileDetail.DiskFilePath = null;
        }

        public async Task<dynamic> GenerateBillService(BillGenerationModal billModal)
        {
            List<string> emails = new List<string>();
            try
            {
                billModal.PdfModal.billingMonth = _timezoneConverter.ToTimeZoneDateTime(
                    billModal.PdfModal.billingMonth,
                    _currentSession.TimeZone
                    );
                billModal.PdfModal.dateOfBilling = _timezoneConverter.ToTimeZoneDateTime(
                    billModal.PdfModal.dateOfBilling,
                    _currentSession.TimeZone
                    );


                // validate all the fields and request data.
                ValidateRequestDetail(billModal.PdfModal);

                // fetch and all the nessary data from database required to bill generation.
                var resultSet = await PrepareRequestForBillGeneration(billModal);

                Organization sender = await GetSenderDetail(resultSet, emails);
                billModal.Sender = sender;
                this.FillSenderDetail(sender, billModal.PdfModal);

                Organization receiver = await GetReceiverDetail(resultSet, emails);
                billModal.Receiver = receiver;
                this.FillReceiverDetail(receiver, billModal.PdfModal);

                FileDetail fileDetail = await GetBillFileDetail(billModal.PdfModal, resultSet);
                billModal.PdfModal.UpdateSeqNo++;
                fileDetail.FileExtension = string.Empty;
                billModal.FileDetail = fileDetail;

                // store template logo and file locations
                await CaptureFileFolderLocations(billModal);
                this.CleanOldFiles(fileDetail);

                // execute and build timesheet if missing
                await GenerateUpdateTimesheet(billModal);

                // generate pdf, docx and excel files
                await GeneratePdfFile(billModal);

                // execute billing data and store into database.
                await GenerateDocxFile(billModal);

                // save file in filesystem
                await SaveExecuteBill(billModal);

                // get email notification detail
                EmailTemplate emailTemplate = _templateService.GetBillingTemplateDetailService();
                emailTemplate.Emails = emails;

                // return result data
                return new { FileDetail = fileDetail, EmailTemplate = emailTemplate };
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

                Bills bill = null; //this.GetBillData();
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
                if (ds.Tables.Count == 4)
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

                    // this.ValidateBillModal(pdfModal);

                    if (fileDetail == null)
                    {
                        fileDetail = new FileDetail
                        {
                            ClientId = pdfModal.ClientId
                        };
                    }

                    List<AttendenceDetail> attendanceSet = new List<AttendenceDetail>();
                    if (ds.Tables[3].Rows.Count > 0)
                    {
                        var currentAttendance = Converter.ToType<Attendance>(ds.Tables[3]);
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
                        // string html = this.GetHtmlString(templatePath, pdfModal, sender, receiver);
                        string html = this.GetHtmlString(templatePath, null);
                        destinationFilePath = Path.Combine(fileDetail.DiskFilePath, fileDetail.FileName + $".{ApplicationConstants.Docx}");
                        this.iHTMLConverter.ToDocx(html, destinationFilePath, headerLogo);

                        GetFileDetail(pdfModal, fileDetail, ApplicationConstants.Pdf);
                        _fileMaker._fileDetail = fileDetail;

                        // Converting html context for pdf conversion.
                        // html = this.GetHtmlString(pdfTemplatePath, pdfModal, sender, receiver, headerLogo);
                        html = this.GetHtmlString(pdfTemplatePath, null, true);
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

            if (resultSet != null && resultSet.Tables.Count == 3)
            {
                var file = Converter.ToList<FileDetail>(resultSet.Tables[0]);
                var employee = Converter.ToType<Employee>(resultSet.Tables[1]);
                var emailTempalte = Converter.ToType<EmailTemplate>(resultSet.Tables[2]);

                List<string> emails = generateBillFileDetail.EmailTemplateDetail.Emails;
                if (emails.Count == 0)
                    throw new HiringBellException("No receiver address added. Please add atleast one email address.");

                UpdateTemplateData(emailTempalte, employee, generateBillFileDetail.MonthName, generateBillFileDetail.ForYear);

                EmailSenderModal emailSenderModal = new EmailSenderModal
                {
                    To = emails, //receiver.Email,
                    CC = new List<string>(),
                    BCC = new List<string>(),
                    Subject = emailTempalte.SubjectLine,
                    FileDetails = file,
                    Body = emailTempalte.BodyContent,
                    Title = emailTempalte.EmailTitle
                };

                _eMailManager.SendMailAsync(emailSenderModal);
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
