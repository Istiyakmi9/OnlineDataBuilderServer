using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.DatabaseLayer.MySql.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using CoreServiceLayer.Implementation;
using DocMaker.ExcelMaker;
using DocMaker.HtmlToDocx;
using DocMaker.PdfService;
using EMailService.Service;
using HtmlService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using ModalLayer.Modal;
using MultiTypeDocumentConverter.Service;
using Newtonsoft.Json.Serialization;
using OnlineDataBuilder.Model;
using SchoolInMindServer.MiddlewareServices;
using ServiceLayer.Caching;
using ServiceLayer.Code;
using ServiceLayer.Interface;
using SocialMediaServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OnlineDataBuilder
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", false, false)
                    .AddJsonFile("staffingbill.json", false, false)
                    .AddEnvironmentVariables();
                //AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: false);

                this.Configuration = config.Build();
                this.Env = env;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IWebHostEnvironment Env { set; get; }
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
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSetting:Key"])),
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
            services.AddScoped<IEvaluationPostfixExpression, EvaluationPostfixExpression>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IRolesAndMenuService, RolesAndMenuService>();
            services.AddScoped<IOnlineDocumentService, OnlineDocumentService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ILiveUrlService, LiveUrlService>();
            services.AddScoped<IUserService, UserService>();

            services.Configure<JwtSetting>(o => Configuration.GetSection(nameof(JwtSetting)).Bind(o));
            services.Configure<BuildPdfTable>(o => Configuration.GetSection("StaffingBill").Bind(o));
            services.Configure<Dictionary<string, List<string>>>(o => Configuration.GetSection("TaxSection").Bind(o));

            services.AddHttpContextAccessor();
            services.AddScoped<CurrentSession>();
            services.AddScoped<IFileMaker, CreatePDFFile>();
            services.AddScoped<IHtmlMaker, ToHtml>();
            services.AddScoped<PdfGenerateHelper>();
            services.AddScoped<IEMailManager, EMailManager>();
            services.AddScoped<IManageUserCommentService, ManageUserCommentService>();
            services.AddScoped<IMediaService, GooogleService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<CommonFilterService>();
            services.AddScoped<IDocumentConverter, DocumentConverter>();
            services.AddScoped<IClientsService, ClientsService>();
            services.AddScoped<IBillService, BillService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<IHTMLConverter, HTMLConverter>();
            services.AddScoped<ITemplateService, TemplateService>();
            services.AddScoped<HtmlConverterService>();
            services.AddScoped<IDOCXToHTMLConverter, DOCXToHTMLConverter>();
            services.AddScoped<ExcelWriter>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddSingleton<FileLocationDetail>(service =>
            {
                var fileLocationDetail = Configuration.GetSection("BillingFolders").Get<FileLocationDetail>();
                var locationDetail = new FileLocationDetail
                {
                    RootPath = this.Env.ContentRootPath,
                    BillsPath = fileLocationDetail.BillsPath,
                    Location = fileLocationDetail.Location,
                    HtmlTemplaePath = fileLocationDetail.HtmlTemplaePath,
                    StaffingBillPdfTemplate = fileLocationDetail.StaffingBillPdfTemplate,
                    StaffingBillTemplate = fileLocationDetail.StaffingBillTemplate,
                    DocumentFolder = fileLocationDetail.Location,
                    UserFolder = Path.Combine(fileLocationDetail.Location, fileLocationDetail.User),
                    BillFolder = Path.Combine(fileLocationDetail.Location, fileLocationDetail.BillsPath),
                    LogoPath = Path.Combine(fileLocationDetail.Location, fileLocationDetail.LogoPath)
                };

                return locationDetail;
            });
            services.AddSingleton<ITimezoneConverter, TimezoneConverter>();
            services.AddScoped<IDocumentProcessing, DocumentProcessing>();
            services.AddScoped<HtmlToPdfConverter>();
            services.AddSingleton<ICacheManager, CacheManager>();
            services.AddScoped<IRequestService, RequestService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<ISalaryComponentService, SalaryComponentService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IDeclarationService, DeclarationService>();
            services.AddScoped<ILeaveService, LeaveService>();
            services.AddScoped<IManageLeavePlanService, ManageLeavePlanService>();

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

            app.UseMiddleware<ExceptionHandlerMiddleware>();
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
