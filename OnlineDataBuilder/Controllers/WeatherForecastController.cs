using BottomhalfCore.DatabaseLayer.Common.Code;
using EMailService.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModalLayer.Modal;
using ServiceLayer.Interface;
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
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IDb _db;
        private readonly IEMailManager _eMailManager;
        private readonly ILeaveCalculation _leaveCalculation;
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ITimesheetService _timesheetService;
        private readonly CurrentSession _currentSession;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            IEMailManager eMailManager,
            IDb db,
            ITimesheetService timesheetService,
            ILeaveCalculation leaveCalculation,
            CurrentSession currentSession
            )
        {
            _logger = logger;
            _eMailManager = eMailManager;
            _db = db;
            _timesheetService = timesheetService;
            _leaveCalculation = leaveCalculation;
            _currentSession = currentSession;
        }

        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<WeatherForecast> Get()
        {
            // _eMailManager.ReadMails(null);
            // _leaveCalculation.RunLeaveCalculationCycle();

            // Enable this section for test database connection in parallel execution
            //Parallel.For(0, 1000, async index =>
            //{
            //    // IDb database = new Db("server=192.168.0.101;port=3306;database=onlinedatabuilder;User Id=istiyak;password=live@Bottomhalf_001;Connection Timeout=30;Connection Lifetime=0;Min Pool Size=0;Max Pool Size=100;Pooling=true;");
            //    var result1 = await _db.GetDataSet("sp_attendance_get_byid", new { AttendanceId = 2 });
            //    Console.WriteLine($"[DataBase call] {index + 1} - Rcords: {result1.Tables.Count}");
            //});


            //Task.Run(async () =>
            //{
            //    DateTime? startDate = null;
            //    var testDatas = Enumerable.Range(1, 20).
            //        Select(i => new
            //        {
            //            Name = $"Test_{i}",
            //            Id = i,
            //            Code = i,
            //            LastUpdatedOn = DateTime.Now,
            //            Salary = 10 * i,
            //            IsEnable = true,
            //            AccountType = startDate,
            //        }).ToList<dynamic>();

            //    var result1 = await _db.ExecuteListAsync("sp_test_insupd", testDatas);
            //});

            //(EmailTemplate emailTemplate, EmailSettingDetail emailSetting) =
            //    _db.Get<EmailTemplate, EmailSettingDetail>("sp_email_template_by_id", new { EmailTemplateId = 1 });

            //var date = Convert.ToDateTime("2023-02-13");
            //_timesheetService.RunWeeklyTimesheetCreation(date);

            //_currentSession.CurrentUserDetail.CompanyId = 1;
            //_leaveCalculation.RunAccrualCycle();

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
