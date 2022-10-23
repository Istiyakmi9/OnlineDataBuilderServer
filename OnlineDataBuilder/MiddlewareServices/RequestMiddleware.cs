using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ModalLayer.Modal;
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

        public RequestMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            this.configuration = configuration;
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

                        var securityToken = handler.ReadToken(token) as JwtSecurityToken;
                        userId = securityToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
                        currentSession.CurrentUserDetail.ReportingManagerId = Convert.ToInt32(
                            securityToken.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid").Value);
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

                        var orgId = securityToken.Claims
                            .FirstOrDefault(x => x.Type == ApplicationConstants.OrganizationId).Value;

                        var companyId = securityToken.Claims.
                            FirstOrDefault(x => x.Type == ApplicationConstants.CompanyId).Value;

                        if (string.IsNullOrEmpty(orgId) || string.IsNullOrEmpty(companyId))
                            throw new HiringBellException("Invalid Organization id or Company id. Please contact to admin.");
                            
                        currentSession.CurrentUserDetail.OrganizationId = Convert.ToInt32(orgId);
                        currentSession.CurrentUserDetail.CompanyId = Convert.ToInt32(companyId);
                        currentSession.TimeZone = TZConvert.GetTimeZoneInfo("India Standard Time");
                        currentSession.CurrentUserDetail.UserId = Convert.ToInt32(userId);
                        currentSession.CurrentUserDetail.FullName = securityToken.Claims.FirstOrDefault(x => x.Type == "unique_name").Value;
                    }
                }

                await _next(context);
            }
            catch (HiringBellException e)
            {
                throw e;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
