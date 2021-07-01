using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ServiceLayer.Code
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly JwtSetting _jwtSetting;
        private readonly IDb _db;
        private readonly IConfiguration _configuration;
        private readonly CurrentSession _currentSession;
        public AuthenticationService(IOptions<JwtSetting> options, IDb db, IConfiguration configuration, CurrentSession currentSession)
        {
            _jwtSetting = options.Value;
            _db = db;
            _configuration = configuration;
            _currentSession = currentSession;
        }

        private string ReadJwtToken()
        {
            string userId = string.Empty;
            if (!string.IsNullOrEmpty(_currentSession.Authorization))
            {
                string token = _currentSession.Authorization.Replace("Bearer", "").Trim();
                if (!string.IsNullOrEmpty(token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    handler.ValidateToken(token, new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _configuration["jwtSetting:Issuer"],
                        ValidAudience = _configuration["jwtSetting:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwtSetting:Key"]))
                    }, out SecurityToken validatedToken);

                    var securityToken = handler.ReadToken(token) as JwtSecurityToken;
                    userId = securityToken.Claims.FirstOrDefault(x => x.Type == "unique_name").Value;
                }
            }
            return userId;
        }

        public RefreshTokenModal Authenticate(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;
            string generatedToken = GenerateAccessToken(userId);
            var refreshToken = GenerateRefreshToken(null);
            refreshToken.Token = generatedToken;
            SaveRefreshToken(refreshToken, userId);
            return refreshToken;
        }

        private string GenerateAccessToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new Claim[] {
                    new Claim(JwtRegisteredClaimNames.Sub, userId),
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim(ClaimTypes.Name, userId),
                    new Claim(ClaimTypes.Version, "1.0.0"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(
                                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key)),
                                            SecurityAlgorithms.HmacSha256Signature
                                     )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var generatedToken = tokenHandler.WriteToken(token);
            return generatedToken;
        }

        public RefreshTokenModal RenewAndGenerateNewToken()
        {
            string UserId = ReadJwtToken();
            RefreshTokenModal refreshTokenModal = default;
            DbParam[] param = new DbParam[]
            {
                new DbParam(UserId, typeof(string), "@UserId")
            };
            var ResultSet = _db.GetDataset("SP_AuthenticationToken_VerifyAndGet", param);
            if (ResultSet.Tables.Count > 0)
            {
                var Result = Converter.ToList<TokenModal>(ResultSet.Tables[0]);
                if (Result.Count > 0)
                {
                    var currentModal = Result.FirstOrDefault();
                    refreshTokenModal = new RefreshTokenModal
                    {
                        Token = GenerateAccessToken(UserId),
                        Expires = currentModal.ExpiryTime
                    };
                }
            }
            return refreshTokenModal;
        }

        private void SaveRefreshToken(RefreshTokenModal refreshToken, string userId)
        {
            DbParam[] param = new DbParam[]
            {
                new DbParam(userId, typeof(string), "@UserId"),
                new DbParam(refreshToken.RefreshToken, typeof(string), "@RefreshToken"),
                new DbParam(refreshToken.Expires, typeof(DateTime), "@ExpiryTime")
            };

            _db.ExecuteNonQuery("sp_UpdateRefreshToken", param, false);
        }

        public RefreshTokenModal GenerateRefreshToken(string ipAddress)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshTokenModal
                {
                    RefreshToken = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
            }
        }
    }
}
