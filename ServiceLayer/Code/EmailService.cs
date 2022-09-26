using BottomhalfCore.DatabaseLayer.Common.Code;
using EMailService.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace ServiceLayer.Code
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IEMailManager _eMailManager;
        private readonly IDb _db;
        public EmailService(IDb db, ILogger<EmailService> logger, IEMailManager eMailManager)
        {
            _db = db;
            _logger = logger;
            _eMailManager = eMailManager;
        }

        public List<string> GetMyMailService()
        {
            EmailSettingDetail emailSettingDetail = GetSettingDetail();
            _eMailManager.ReadMails(emailSettingDetail);
            return null;
        }

        public string SendEmailRequestService(EmailSenderModal mailRequest, IFormFileCollection files)
        {
            string result = null;
            EmailSenderModal emailSenderModal = new EmailSenderModal
            {
                To = mailRequest.To, //receiver.Email,
                CC = mailRequest.CC,    //new List<string>(),
                BCC = mailRequest.BCC,  //new List<string>(),
                Subject = mailRequest.Subject,
                Body = mailRequest.Body,
                FileDetails = new List<FileDetail>() //Converter.ToList<FileDetail>(mailRequest.FileDetails)
            };

            result = SendMail(emailSenderModal, files);
            return result;
        }

        private EmailSettingDetail GetSettingDetail()
        {
            var emailSettingDetail = _db.Get<EmailSettingDetail>("sp_email_setting_detail_get", new { EmailSettingDetailId = 0 });
            if (emailSettingDetail == null)
                throw new HiringBellException("Fail to get emaill detail. Please contact to admin.");

            return emailSettingDetail;
        }

        private string SendMail(EmailSenderModal emailSenderModal, IFormFileCollection files)
        {
            string status = string.Empty;
            var emailSettingDetail = GetSettingDetail();

            if (emailSenderModal.To == null || emailSenderModal.To.Count == 0)
                throw new HiringBellException("To send email receiver address is mandatory. Receiver address not found.");

            //var fromEmail = new
            //{
            //    Id = emailSettingDetail.EmailAddress,
            //    Pwd = emailSettingDetail.Credentials, //"bottomhalf@mi9",
            //    Host = emailSettingDetail.EmailHost, //"smtpout.asia.secureserver.net",
            //    Port = emailSettingDetail.PortNo // 587  // 25	                 
            //};

            var fromAddress = new System.Net.Mail.MailAddress(emailSettingDetail.EmailAddress, emailSenderModal.Subject);

            var smtp = new SmtpClient
            {
                Host = emailSettingDetail.EmailHost,
                Port = emailSettingDetail.PortNo,
                EnableSsl = emailSettingDetail.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = emailSettingDetail.UserDefaultCredentials,
                Credentials = new NetworkCredential(emailSettingDetail.EmailAddress, emailSettingDetail.Credentials)
            };

            var mailMessage = new MailMessage();
            mailMessage.Subject = emailSenderModal.Subject;
            mailMessage.Body = emailSenderModal.Body;
            mailMessage.IsBodyHtml = false;
            mailMessage.From = fromAddress;

            foreach (var emailAddress in emailSenderModal.To)
                mailMessage.To.Add(emailAddress);

            if (emailSenderModal.CC != null && emailSenderModal.CC.Count > 0)
                foreach (var emailAddress in emailSenderModal.CC)
                    mailMessage.CC.Add(emailAddress);

            if (emailSenderModal.BCC != null && emailSenderModal.BCC.Count > 0)
                foreach (var emailAddress in emailSenderModal.BCC)
                    mailMessage.Bcc.Add(emailAddress);

            try
            {
                if (files != null && files.Count > 0)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        foreach (var file in files)
                        {
                            file.CopyTo(ms);
                            ms.Position = 0;
                            mailMessage.Attachments.Add(
                                new System.Net.Mail.Attachment(ms, file.Name)
                            );
                            ms.Flush();
                        }
                        smtp.Send(mailMessage);
                    }
                }
                else
                {
                    smtp.Send(mailMessage);
                }

                status = "success";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return status;
        }
    }
}
