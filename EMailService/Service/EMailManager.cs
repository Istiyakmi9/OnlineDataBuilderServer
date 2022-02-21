using System;
using System.Collections.Generic;
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
    }
}
