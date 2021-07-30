using DocMaker.PdfService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineDataBuilder.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IFileMaker _fileMake;
        private readonly IHtmlMaker _htmlMaker;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfiguration _configuration;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration, IFileMaker fileMake, IHtmlMaker htmlMaker)
        {
            _logger = logger;
            _fileMake = fileMake;
            _htmlMaker = htmlMaker;
            _configuration = configuration;
        }

        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<WeatherForecast> Get()
        {
            //string path = PathLocator.GetProjectDirectoryLocation("Documents");
            //_htmlMaker.DocxFileToHtml(@"F:\DocAndResumes\Istiyak-bh.docx", path);


            //var stream = _fileMake.GeneratePdf();
            //_htmlMaker.HtmlToDocxFile(@"E:\HtmlWork\demo.html", path);
            //string path = Path.Combine(Assembly.GetExecutingAssembly().Location, "a.pdf");
            //System.IO.File.WriteAllBytes(path, stream.ToArray());
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
