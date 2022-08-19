using BottomhalfCore.Services.Code;
using EMailService.Service;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace ServiceLayer.Code
{
    public class EmailService: IEmailService
    {
        public string SendEmailRequestService(MailRequest mailRequest)
        {
            string result = null;
            MailRequest emailSenderModal = new MailRequest
            {
                To = mailRequest.To, //receiver.Email,
                From = "info@bottomhalf.in", //sender.Email,
                UserName = "BottomHalf",
                CC = mailRequest.CC,    //new List<string>(),
                BCC = mailRequest.BCC,  //new List<string>(),
                Subject = mailRequest.Subject,
                Body = mailRequest.Body,
                FileDetails = new List<FileDetail>() //Converter.ToList<FileDetail>(mailRequest.FileDetails)
            };

            result = SendMail(emailSenderModal);
            return result;
        }

        private string SendMail(MailRequest emailSenderModal)
        {
            string status = string.Empty;

            if (emailSenderModal.To == null || emailSenderModal.To.Count == 0)
                throw new HiringBellException("To send email receiver address is mandatory. Receiver address not found.");

            var fromEmail = new
            {
                Id = emailSenderModal.From,
                Pwd = "bottomhalf@mi9",
                Host = "smtpout.asia.secureserver.net",
                Port = 587  // 25	                 
            };

            var fromAddress = new System.Net.Mail.MailAddress(fromEmail.Id, emailSenderModal.Subject);

            var smtp = new SmtpClient
            {
                Host = fromEmail.Host,
                Port = fromEmail.Port,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromEmail.Pwd)
            };

            var message = new MailMessage();
            message.Subject = emailSenderModal.Subject;
            message.Body = emailSenderModal.Body;
            message.IsBodyHtml = false;
            message.From = fromAddress;

            foreach (var emailAddress in emailSenderModal.To)
                message.To.Add(emailAddress);

            if (emailSenderModal.CC != null && emailSenderModal.CC.Count > 0)
                foreach (var emailAddress in emailSenderModal.CC)
                    message.CC.Add(emailAddress);

            if (emailSenderModal.BCC != null && emailSenderModal.BCC.Count > 0)
                foreach (var emailAddress in emailSenderModal.BCC)
                    message.Bcc.Add(emailAddress);

            try
            {
                //if (emailSenderModal.FileDetails != null && emailSenderModal.FileDetails.Count > 0)
                //    foreach (var files in emailSenderModal.FileDetails)
                //    {
                //        message.Attachments.Add(
                //            new System.Net.Mail.Attachment(Path.Combine(_fileLocationDetail.RootPath, files.FilePath, files.FileName + ".pdf"))
                //        );
                //    }

                smtp.Send(message);
                status = "success";
            }
            catch (Exception ex)
            {
                var _e = ex;
                throw ex;
            }
            return status;
        }

    }
}
