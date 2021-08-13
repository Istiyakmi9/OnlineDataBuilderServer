using DocMaker.PdfService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModalLayer.Modal;
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
        private readonly BuildPdfTable _buildPdfTable;
        private readonly IFileMaker _iFileMaker;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IOptions<BuildPdfTable> options, IFileMaker iFileMaker)
        {
            _logger = logger;
            _buildPdfTable = options.Value;
            _iFileMaker = iFileMaker;
        }

        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<WeatherForecast> Get()
        {
            _iFileMaker.BuildPdfBill(_buildPdfTable);
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
