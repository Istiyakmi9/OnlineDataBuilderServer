using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ModalLayer.Modal;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace SchoolInMindServer.MiddlewareServices
{
    public class RequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration configuration;
        private readonly string TokenName = "Authorization";
        private readonly ITimezoneConverter _timezoneConverter;

        public RequestMiddleware(RequestDelegate next, IConfiguration configuration, ITimezoneConverter timezoneConverter)
        {
            this.configuration = configuration;
            _timezoneConverter = timezoneConverter;
            _next = next;
        }

        public async Task Invoke(HttpContext context, CurrentSession currentSession)
        {
            try
            {
                Parallel.ForEach(context.Request.Headers, header =>
                {
                    if (header.Value.FirstOrDefault() != null)
                    {
                        if (header.Key == TokenName)
                            currentSession.Authorization = header.Value.FirstOrDefault();
                    }
                });

                string userId = string.Empty;
                if (!string.IsNullOrEmpty(currentSession.Authorization))
                {
                    string token = currentSession.Authorization.Replace(ApplicationConstants.JWTBearer, "").Trim();
                    if (!string.IsNullOrEmpty(token) && token != "null")
                    {
                        var handler = new JwtSecurityTokenHandler();
                        handler.ValidateToken(token, new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = false,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = configuration["jwtSetting:Issuer"],
                            ValidAudience = configuration["jwtSetting:Issuer"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["jwtSetting:Key"]))
                        }, out SecurityToken validatedToken);

                        JwtSecurityToken securityToken = handler.ReadToken(token) as JwtSecurityToken;
                        ReadToken(securityToken, currentSession);
                    }
                }

                await _next(context);
            }
            catch (HiringBellException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ReadToken(JwtSecurityToken securityToken, CurrentSession currentSession)
        {
            var userId = securityToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sid).Value;
            var userDetail = securityToken.Claims.FirstOrDefault(x => x.Type == ApplicationConstants.JBot).Value;
            currentSession.CurrentUserDetail = JsonConvert.DeserializeObject<UserDetail>(userDetail);
            var roleName = securityToken.Claims.FirstOrDefault(x => x.Type == "role").Value;
            switch (roleName)
            {
                case nameof(Role.Admin):
                    currentSession.CurrentUserDetail.RoleId = 1;
                    break;
                case nameof(Role.Employee):
                    currentSession.CurrentUserDetail.RoleId = 2;
                    break;
                case nameof(Role.Candidate):
                    currentSession.CurrentUserDetail.RoleId = 3;
                    break;
                case nameof(Role.Client):
                    currentSession.CurrentUserDetail.RoleId = 4;
                    break;
                default:
                    currentSession.CurrentUserDetail.RoleId = 5;
                    break;
            }

            if (currentSession.CurrentUserDetail == null)
                throw new HiringBellException("Invalid token found. Please contact to admin.");

            if (currentSession.CurrentUserDetail.OrganizationId <= 0
                || currentSession.CurrentUserDetail.CompanyId <= 0)
                throw new HiringBellException("Invalid Organization id or Company id. Please contact to admin.");

            if (string.IsNullOrEmpty(userId))
                throw new HiringBellException("Invalid employee id used. Please contact to admin.");

            currentSession.CurrentUserDetail.FullName = currentSession.CurrentUserDetail.FirstName
                                                        + " " +
                                                        currentSession.CurrentUserDetail.LastName;

            currentSession.TimeZone = TZConvert.GetTimeZoneInfo("India Standard Time");
            currentSession.TimeZoneNow = _timezoneConverter.ToTimeZoneDateTime(DateTime.UtcNow, currentSession.TimeZone);
            currentSession.CurrentUserDetail.UserId = Convert.ToInt32(userId);
        }
    }
}
