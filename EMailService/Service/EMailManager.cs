using ModalLayer.Modal;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using EAGetMail;
using System.Globalization;
using System.Text;

namespace EMailService.Service
{
    public class EMailManager : IEMailManager
    {
        private readonly FileLocationDetail _fileLocationDetail;

        public EMailManager(FileLocationDetail fileLocationDetail)
        {
            _fileLocationDetail = fileLocationDetail;
        }

        public void ReadMails(EmailSettingDetail emailSettingDetail)
        {
            string localInbox = string.Format("{0}\\inbox", Directory.GetCurrentDirectory());
            MailServer oServer = new MailServer("pop.secureserver.net",
                emailSettingDetail.EmailAddress, // "info@bottomhalf.in",
                emailSettingDetail.Credentials, // "bottomhalf@mi9",
                ServerProtocol.Pop3);

            // Enable SSL/TLS connection, most modern email server require SSL/TLS by default
            oServer.SSLConnection = true;
            oServer.Port = 995;

            // if your server doesn't support SSL/TLS, please use the following codes
            // oServer.SSLConnection = false;
            // oServer.Port = 110;

            MailClient oClient = new MailClient("TryIt");
            oClient.Connect(oServer);

            MailInfo[] infos = oClient.GetMailInfos();
            Console.WriteLine("Total {0} email(s)\r\n", infos.Length);
            for (int i = 0; i < infos.Length; i++)
            {
                MailInfo info = infos[i];
                Console.WriteLine("Index: {0}; Size: {1}; UIDL: {2}",
                    info.Index, info.Size, info.UIDL);

                // Receive email from POP3 server
                Mail oMail = oClient.GetMail(info);

                Console.WriteLine("From: {0}", oMail.From.ToString());
                Console.WriteLine("Subject: {0}\r\n", oMail.Subject);

                // Generate an unqiue email file name based on date time.
                string fileName = _generateFileName(i + 1);
                string fullPath = string.Format("{0}\\{1}", localInbox, fileName);

                // Save email to local disk
                oMail.SaveAs(fullPath, true);

                // Mark email as deleted from POP3 server.
                oClient.Delete(info);
            }

            // Quit and expunge emails marked as deleted from POP3 server.
            oClient.Quit();
            Console.WriteLine("Completed!");
        }

        private string _generateFileName(int sequence)
        {
            DateTime currentDateTime = DateTime.Now;
            return string.Format("{0}-{1:000}-{2:000}.eml",
                currentDateTime.ToString("yyyyMMddHHmmss", new CultureInfo("en-US")),
                currentDateTime.Millisecond,
                sequence);
        }

        public string SendMail(EmailSenderModal emailSenderModal, EmailTemplate EmailTemplateDetail, Employee employee)
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

            var fromAddress = new System.Net.Mail.MailAddress(fromEmail.Id, emailSenderModal.Title);

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
            message.Subject = emailSenderModal.Title;
            message.Body = this.GetClientBillingBody(employee, EmailTemplateDetail);
            message.IsBodyHtml = true;
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
                if (emailSenderModal.FileDetails != null && emailSenderModal.FileDetails.Count > 0)
                    foreach (var files in emailSenderModal.FileDetails)
                    {
                        message.Attachments.Add(
                            new System.Net.Mail.Attachment(Path.Combine(_fileLocationDetail.RootPath, files.FilePath, files.FileName + ".pdf"))
                        );
                    }

                smtp.Send(message);
                status = "success";
            }
            catch (Exception ex)
            {
                var _e = ex;
                throw;
            }
            return status;
        }

        private string GetClientBillingBody(Employee employee, EmailTemplate emailTemplate)
        {
            StringBuilder firstPhase = new StringBuilder();
            StringBuilder body = new StringBuilder();
            StringBuilder endPhase = new StringBuilder();

            foreach (var first in emailTemplate.BodyContentDetail.FirstPhase)
                firstPhase.AppendLine(first);

            foreach (var first in emailTemplate.BodyContentDetail.Body)
                body.AppendLine(first);

            foreach (var first in emailTemplate.BodyContentDetail.EndPhase)
                endPhase.AppendLine(first);

            string style = @"
                    <style>
                        .mt-4 {
                            margin-top: 1.5rem!important;
                        }

                        .mb-1 {
                            margin-bottom: 0.25rem!important;
                        }

                        .border {
                            border: 1px solid #dee2e6!important;
                        }

                        .col-12 {
                            flex: 0 0 auto;
                            width: 100%;
                        }

                        .py-3 {
                            padding - top: 1rem!important;
                            padding-bottom: 1rem!important;
                        }

                        .px-2 {padding - right: 0.5rem!important;
                            padding-left: 0.5rem!important;
                        } 

                        .position-relative {
                            position: relative!important;
                        }
                        .d-flex {
                            display: flex!important;
                        }

                        .fw-bold {
                            font-weight: 700!important;
                        }

                        .ps-2 {
                            padding-left: 0.5rem!important;
                        }
                        .w-100 {
                            width: 100%!important;
                        }

                        .mt-3 {
                            margin-top: 1rem!important;
                        }
                    </style>
                ";

            string htmlBody = $@"<!DOCTYPE html>            
                    <html>
                        <head>
                            <title>STAFFING BILL</title> 
                            {style}
                         </head> 
                         <body>
                            <div>
                              <div class='mt-4'>
                                <div class='mt-1'>
                                  {emailTemplate.Salutation}
                                </div>
                                <div class='border py-3 px-2'>
                                  <div class='col-12'>
                                    <div>
                                      <div class='no-focus-line'>
                                        {firstPhase.ToString()}
                                      </div> 
                                    </div>
                                  </div>
                  
                                  <div class='col-12'>
                                    <div>
                                      <div class='no-focus-line'>
                                        {body.ToString()}
                                      </div> 
                                    </div>
                                  </div>

                                  <div class='col-12 my-4'>
                                    <div>
                                      <div>
                                        <div class='d-flex'>
                                          <span class='px-4 fw-bold'>CANDIDATE NAME: </span>
                                          <span>{employee.FirstName + ' ' + employee.LastName}</span>
                                        </div>

                                        <div class='d-flex'>
                                          <span class='px-4 fw-bold'>CANDIDATE ROLE: </span>
                                          <span>SOFTWARE DEVELOPER</span>
                                        </div>
                        
                                        <div class='d-flex'>
                                          <span class='px-4 fw-bold'>BILLING MONTH: </span>
                                          <span>{employee.CreatedOn.Month}, {employee.CreatedOn.Year}</span>
                                        </div>
                                      </div> 
                                    </div>
                                  </div>

                                  <div class='col-12'>
                                    <div>
                                      <div class='no-focus-line'>
                                        {endPhase.ToString()}
                                      </div>  
                                    </div>
                                  </div>
                                </div>
                                <div class='d-flex position-relative'>
                                  <div class='fw-bold'>Note: </div> 
                                  <div>
                                    <input type='text' class='w-100 ps-2 border-0' 
                                      placeholder='Enter your name' [value]='emailTemplate.EmailNote'>
                                  </div>
                                  <i class='edit fa fa-pencil position-absolute'></i>
                                </div>
                                <div class='mt-3'>
                                  {emailTemplate.EmailClosingStatement}
                                </div>
                                <div class='position-relative'>
                                  <input type='text' class='border-0' 
                                    placeholder='Enter your name' [value]='emailTemplate.SignatureDetail'>
                                  <i class='edit fa fa-pencil position-absolute'></i>
                                </div>
                                <div class='position-relative'>
                                  Contact No#:
                                  <div>
                                    +91-{emailTemplate.ContactNo}
                                  </div>
                                  <i class='edit fa fa-pencil position-absolute'></i>
                                </div>
                              </div>
                            </div>
                        </body> 
                    </html>";

            return htmlBody;
        }
    }
}
