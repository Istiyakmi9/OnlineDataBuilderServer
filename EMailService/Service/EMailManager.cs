using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace EMailService.Service
{
    public class EMailManager : IEMailManager
    {
        public bool Send()
        {
            bool flag = false;
            try
            {
                MailMessage msgs = new MailMessage();
                msgs.To.Add("istiyaq.mi9@gmail.com");
                MailAddress address = new MailAddress("info@bottomhalf.in");
                msgs.From = address;
                msgs.Subject = "Testing";
                string htmlBody = @"< !DOCTYPE html >            
                    <html>
                        <head>
                            <title>Email</title> 
                         </head> 
                         <body>
                            <h1> Hi welcome </h1> 
                            <p> Thank you for register</p> 
                        </body> 
                    </html>";
                msgs.Body = htmlBody;
                msgs.IsBodyHtml = true;
                SmtpClient client = new SmtpClient();
                client.Host = "relay-hosting.secureserver.net";
                client.Port = 25;
                client.EnableSsl = false;
                client.UseDefaultCredentials = true;
                client.Credentials = new System.Net.NetworkCredential("info@bottomhalf.in", "bottomhalf@mi9");
                client.Send(msgs);
                return flag;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SendMail(string emailId, string userName, string subject, string body)
        {
            var fromEmail = new
            {
                Id = "info@bottomhalf.in",
                Pwd = "bottomhalf@mi9",
                Host = "smtpout.asia.secureserver.net",
                Port = 587  // 25	                 
            };

            var fromAddress = new MailAddress(fromEmail.Id, "Testing - godaddy ");
            var toAddress = new MailAddress(emailId, userName);

            var smtp = new SmtpClient
            {
                Host = fromEmail.Host,
                Port = fromEmail.Port,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromEmail.Pwd)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = this.GetClientBillingBody(),
                IsBodyHtml = true
            })

                try
                {
                    using (FileStream stream = new FileStream(@"C:\Users\istiy\Downloads\Appointment_Letter.pdf", FileMode.Open))
                    {
                        Attachment attachment = new Attachment(stream, "Letter_Of_Appointment.pdf");
                        message.Attachments.Add(attachment);
                        smtp.Send(message);
                    }
                }
                catch (Exception ex)
                {
                    var _e = ex;
                    throw;
                }
        }

        private string GetClientBillingBody()
        {
            string htmlBody = @"<!DOCTYPE html>            
                    <html>
                        <head>
                            <title>STAFFING BILL</title> 
                         </head> 
                         <body>
                            <h4>Hi Sir/Madam, </h4> 
                            <p>PFA bill for the month of July.</p> 
                            <p>Developer detail as follows:</p>
                            <span style='margin-left:10px;'>1. FAHIM SHAIKH  [ROLE: SOFTWARE DEVELOPER]</span> 
                            <span style='margin-left:10px;'>2. VANHAR BASHA  [ROLE: SOFTWARE DEVELOPER]</span> 
                            
                            <p style='margin-top: 2rem;'>Thanks & Regards,</p>
                            <div>Team BottomHalf</div>
                            <div>Mob: +91-9100544384</div>
                        </body> 
                    </html>";

            return htmlBody;
        }
    }
}
