using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnlineDataBuilder.MiddlewareServices.Src;

namespace OnlineDataBuilder
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(x => x.AddHostedService<EmailSchedulerJob>());
        // .ConfigureServices(x => x.AddHostedService<LeaveAccrualSchedular>());
    }
}
