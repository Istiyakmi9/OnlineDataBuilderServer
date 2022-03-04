using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace OnlineDataBuilder.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        //private readonly IEMailManager _eMailManager;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IBillService _billService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IBillService billService)
        {
            _logger = logger;
            _billService = billService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<WeatherForecast> Get()
        {
            // _billService.GenerateDocument(null);
            // SendMail();
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        private void SendMail()
        {
            try
            {
                MailMessage msgs = new MailMessage();
                msgs.To.Add("istiyaq.4game@gmail.com");
                MailAddress address = new MailAddress("info@bottomhalf.in");
                msgs.From = address;
                msgs.Subject = "Contact";
                string htmlBody = @"<!DOCTYPE html>
                    <html>            
                    <head>
                    <title> Email </title> 
                        </ head> 
                            <body>            
                                <h1> Hi welcome </h1> 
                                <p> Thank you for register </p> 
                            </body> 
                        </html>";
                msgs.Body = htmlBody;
                msgs.IsBodyHtml = true;
                SmtpClient client = new SmtpClient();
                client.Host = "relay-hosting.secureserver.net";
                client.Port = 25;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential("info@bottomhalf.in", "bottomhalf@mi9");
                client.Send(msgs);
            }
            catch (Exception ex) { }
        }
    }
}
