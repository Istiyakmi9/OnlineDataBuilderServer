using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ModalLayer.Modal;
using System;
using System.Linq;
using System.Threading.Tasks;

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

                //if (!string.IsNullOrEmpty(currentSession.Authorization))
                //{
                //    //string token = currentSession.Authorization.Replace("Bearer", "").Trim();
                //    //if (!string.IsNullOrEmpty(token))
                //    //{
                //    try
                //    {
                //        var handler = new JwtSecurityTokenHandler();
                //        handler.ValidateToken(currentSession.Authorization, new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                //        {
                //            ValidateIssuer = false,
                //            ValidateAudience = false,
                //            ValidateLifetime = true,
                //            ValidateIssuerSigningKey = true,
                //            ValidIssuer = configuration["jwtSetting:Issuer"],
                //            ValidAudience = configuration["jwtSetting:Issuer"],
                //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["jwtSetting:Key"]))
                //        }, out SecurityToken validatedToken);

                //        //var securityToken = handler.ReadToken(currentSession.Authorization) as JwtSecurityToken;
                //        //var jwtExpValue = long.Parse(securityToken.Claims.FirstOrDefault(x => x.Type == "exp").Value);
                //    }
                //    catch (Exception ex)
                //    {

                //    }
                //    //}
                //}

                //if (!ShortCircuitRequested)
                //{
                //context.Response.Headers["Authorization"] = currentSession.Authorization;
                await _next(context);
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
