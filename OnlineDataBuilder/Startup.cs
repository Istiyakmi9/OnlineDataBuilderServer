using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.DatabaseLayer.MsSql.Code;
using CoreServiceLayer.Implementation;
using DocMaker.PdfService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using ModalLayer.Modal;
using Newtonsoft.Json.Serialization;
using OnlineDataBuilder.Model;
using SchoolInMindServer.MiddlewareServices;
using ServiceLayer.Code;
using ServiceLayer.Interface;
using System;
using System.IO;
using System.Text;

namespace OnlineDataBuilder
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", false, false);
                //AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: false);

                this.Configuration = config.Build();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IConfiguration Configuration { get; }
        public string CorsPolicy = "BottomhalfCORS";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
               .AddJwtBearer(x =>
               {
                   x.SaveToken = true;
                   x.RequireHttpsMetadata = false;
                   x.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = false,
                       ValidateAudience = false,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["jwtSetting:Key"])),
                       ClockSkew = TimeSpan.Zero
                   };
               });

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            string connectionString = Configuration.GetConnectionString("OnlinedatabuilderDb");
            services.AddScoped<IDb, Db>(x => new Db(connectionString));
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IOnlineDocumentService, OnlineDocumentService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ILiveUrlService, LiveUrlService>();

            services.Configure<JwtSetting>(o => Configuration.GetSection("jwtSetting").Bind(o));
            services.AddHttpContextAccessor();
            services.AddSingleton<CurrentSession>();
            services.AddScoped<IFileMake, CreatePDFFile>();

            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicy, policy =>
                {
                    policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("Authorization");
                });
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.Admin, Policies.AdminPolicy());
                options.AddPolicy(Policies.User, Policies.UserPolicy());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                   Path.Combine(Directory.GetCurrentDirectory())),
                RequestPath = "/Files"
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors(CorsPolicy);
            app.UseMiddleware<RequestMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
