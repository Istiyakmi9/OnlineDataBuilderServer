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
                        userId = securityToken.Claims.FirstOrDefault(x => x.Type == "unique_name").Value;
                        var roleName = securityToken.Claims.FirstOrDefault(x => x.Type == "role").Value;
                        switch (roleName)
                        {
                            case nameof(RolesName.Admin):
                                currentSession.CurrentUserDetail.RoleId = (int)RolesName.Admin;
                                break;
                            case nameof(RolesName.User):
                                currentSession.CurrentUserDetail.RoleId = (int)RolesName.User;
                                break;
                            default:
                                currentSession.CurrentUserDetail.RoleId = (int)RolesName.Other;
                                break;
                        }

                        currentSession.TimeZone = TZConvert.GetTimeZoneInfo("India Standard Time");
                        currentSession.CurrentUserDetail.UserId = Convert.ToInt32(userId);
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
