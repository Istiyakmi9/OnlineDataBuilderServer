using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ModalLayer.Modal;
using ServiceLayer.Caching;
using ServiceLayer.Interface;
using SocialMediaServices;
using SocialMediaServices.Modal;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CacheTable = ServiceLayer.Caching.CacheTable;

namespace ServiceLayer.Code
{
    public class LoginService : ILoginService
    {
        private readonly IDb db;
        private readonly JwtSetting _jwtSetting;
        private readonly IMediaService _mediaService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICacheManager _cacheManager;
        private readonly IConfiguration _configuration;

        public LoginService(IDb db, IOptions<JwtSetting> options,
            IMediaService mediaService,
            IAuthenticationService authenticationService,
            ICacheManager cacheManager,
            IConfiguration configuration)
        {
            this.db = db;
            _configuration = configuration;
            _jwtSetting = options.Value;
            _mediaService = mediaService;
            _authenticationService = authenticationService;
            _cacheManager = cacheManager;
        }

        public Boolean RemoveUserDetailService(string Token)
        {
            Boolean Flag = false;
            return Flag;
        }
        public UserDetail GetUserDetail(AuthUser authUser)
        {
            UserDetail userDetail = default;
            DbParam[] param = new DbParam[]
            {
                new DbParam(authUser.Email, typeof(string), "_email"),
                new DbParam(authUser.MobileNo, typeof(string), "_mobile")
            };

            var ResultSet = this.db.GetDataset("sp_UserDetail_GetByMobileOrEmail", param);
            var Data = Converter.ToList<UserDetail>(ResultSet.Tables[0]);
            if (Data != null & Data.Count > 0)
                userDetail = Data.FirstOrDefault();
            return userDetail;
        }

        public async Task<LoginResponse> SignUpUser(UserDetail userDetail)
        {
            LoginResponse loginResponse = null;
            GoogleResponseModal googleResponseModal = await _mediaService.FetchUserProfileByAccessToken(userDetail.AccessToken);
            if (googleResponseModal != null)
            {
                userDetail.EmailId = googleResponseModal.email;
                DbParam[] param = new DbParam[]
                {
                    new DbParam(userDetail.UserId, typeof(long), "_UserId"),
                    new DbParam(userDetail.FirstName, typeof(string), "_FirstName"),
                    new DbParam(userDetail.LastName, typeof(string), "_LastName"),
                    new DbParam(userDetail.Mobile, typeof(string), "_MobileNo"),
                    new DbParam(userDetail.EmailId, typeof(string), "_EmailId"),
                    new DbParam(userDetail.Address, typeof(string), "_Address"),
                    new DbParam(userDetail.CompanyName, typeof(string), "_CompanyName"),
                    new DbParam(null, typeof(string), "_AdminId")
                };

                var ResultSet = this.db.ExecuteNonQuery("sp_UserDetail_Ins", param, true);
                if (!string.IsNullOrEmpty(ResultSet))
                    loginResponse = FetchUserDetail(userDetail, "sp_Userlogin_Auth");
            }
            return loginResponse;
        }


        public string FetchUserLoginDetail(UserDetail authUser, string role)
        {
            string encryptedPassword = string.Empty;

            if (!string.IsNullOrEmpty(authUser.EmailId))
                authUser.EmailId = authUser.EmailId.Trim().ToLower();

            var loginDetail = db.Get<UserDetail>("sp_Password_GetByRole", new
            {
                authUser.UserId,
                MobileNo = authUser.Mobile,
                authUser.EmailId
            });

            if (loginDetail != null)
            {
                encryptedPassword = loginDetail.Password;
                authUser.OrganizationId = loginDetail.OrganizationId;
                authUser.CompanyId = loginDetail.CompanyId;
            }
            else
            {
                throw new HiringBellException("Incorrect user detail provided.");
            }

            return encryptedPassword;
        }

        public async Task<LoginResponse> FetchAuthenticatedUserDetail(UserDetail authUser, string role)
        {
            return await Task.Run(() =>
            {
                LoginResponse loginResponse = default;
                if ((!string.IsNullOrEmpty(authUser.EmailId) || !string.IsNullOrEmpty(authUser.Mobile)) && !string.IsNullOrEmpty(authUser.Password))
                {
                    loginResponse = FetchUserDetail(authUser, "sp_Employeelogin_Auth", role);
                }

                return loginResponse;
            });
        }

        public async Task<LoginResponse> FetchAuthenticatedProviderDetail(UserDetail authUser)
        {
            string ProcedureName = string.Empty;
            if (authUser.UserTypeId == (int)UserType.Admin)
                ProcedureName = "sp_Userlogin_Auth";
            else if (authUser.UserTypeId == (int)UserType.Employee)
                ProcedureName = "sp_Employeelogin_Auth";
            else
                throw new HiringBellException("UserType is invalid. Only system user allowed");
            return await Task.Run(() =>
            {
                LoginResponse loginResponse = default;
                if ((!string.IsNullOrEmpty(authUser.EmailId) || !string.IsNullOrEmpty(authUser.Mobile)) && !string.IsNullOrEmpty(authUser.Password))
                {
                    loginResponse = FetchUserDetail(authUser, ProcedureName);
                }

                return loginResponse;
            });
        }

        private LoginResponse FetchUserDetail(UserDetail authUser, string ProcedureName, string role = Role.Other)
        {
            LoginResponse loginResponse = default;
            var encryptedPassword = this.FetchUserLoginDetail(authUser, role);
            encryptedPassword = _authenticationService.Decrypt(encryptedPassword, _configuration.GetSection("EncryptSecret").Value);
            if (encryptedPassword.CompareTo(authUser.Password) != 0)
            {
                throw new HiringBellException("Invalid userId or password.");
            }

            DbParam[] param = new DbParam[]
            {
                new DbParam(authUser.UserId, typeof(System.Int64), "_UserId"),
                new DbParam(authUser.Mobile, typeof(System.String), "_MobileNo"),
                new DbParam(authUser.EmailId, typeof(System.String), "_EmailId"),
                new DbParam(authUser.UserTypeId, typeof(int), "_UserTypeId")
            };

            DataSet ds = db.GetDataset(ProcedureName, param);
            if (ds != null && ds.Tables.Count == 3)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    loginResponse = new LoginResponse();
                    var loginDetail = Converter.ToType<LoginDetail>(ds.Tables[0]);
                    var employee = Converter.ToType<Employee>(ds.Tables[1]);
                    if (loginDetail != null)
                    {
                        var userDetail = new UserDetail
                        {
                            FirstName = loginDetail.FirstName,
                            LastName = loginDetail.LastName,
                            Address = loginDetail.Address,
                            Mobile = loginDetail.Mobile,
                            EmailId = loginDetail.EmailId,
                            UserId = loginDetail.UserId,
                            CompanyName = loginDetail.CompanyName,
                            UserTypeId = loginDetail.UserTypeId,
                            OrganizationId = loginDetail.OrganizationId,
                            CompanyId = loginDetail.CompanyId,
                            ReportingManagerId = loginDetail.ReportingManagerId,
                            CreatedOn = employee.UpdatedOn
                        };

                        var _token = _authenticationService.Authenticate(userDetail);
                        if (_token != null)
                        {
                            userDetail.Token = _token.Token;
                            userDetail.TokenExpiryDuration = DateTime.Now.AddHours(_jwtSetting.AccessTokenExpiryTimeInSeconds);
                            userDetail.RefreshToken = _token.RefreshToken;
                        }

                        loginResponse.Menu = ds.Tables[2];
                        loginResponse.UserDetail = userDetail;
                        loginResponse.Companies = _cacheManager.Get(CacheTable.Company);
                    }
                }
            }

            return loginResponse;
        }

        public string ResetEmployeePassword(UserDetail authUser, string role)
        {
            string Status = string.Empty;
            var encryptedPassword = this.FetchUserLoginDetail(authUser, role);
            encryptedPassword = _authenticationService.Decrypt(encryptedPassword, _configuration.GetSection("EncryptSecret").Value);
            if (encryptedPassword != authUser.Password)
                throw new HiringBellException("Incorrect old password");

            string newEncryptedPassword = _authenticationService.Encrypt(authUser.NewPassword, _configuration.GetSection("EncryptSecret").Value);
            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(authUser.EmailId, typeof(System.String), "_EmailId"),
                new DbParam(authUser.Mobile, typeof(System.String), "_MobileNo"),
                new DbParam(newEncryptedPassword, typeof(System.String), "_NewPassword")
            };
            var result = db.ExecuteNonQuery("sp_Reset_Password", dbParams, true);
            if (result == "Update")
            {
                Status = "Password changed successfully, Please logout and login again";
            }
            else
            {
                throw new HiringBellException("Unable to update your password");
            }

            return Status;
        }

        public async Task<bool> RegisterNewCompany(RegistrationForm registrationForm)
        {
            return await Task.Run(() =>
            {
                bool statusFlag = false;
                if (string.IsNullOrEmpty(registrationForm.OrganizationName))
                    throw new HiringBellException { UserMessage = $"Invalid Organization name passed: {registrationForm.OrganizationName}" };

                if (string.IsNullOrEmpty(registrationForm.CompanyName))
                    throw new HiringBellException { UserMessage = $"Invalid Company name passed: {registrationForm.CompanyName}" };

                if (string.IsNullOrEmpty(registrationForm.Mobile))
                    throw new HiringBellException { UserMessage = $"Invalid Mobile number: {registrationForm.Mobile}" };

                if (string.IsNullOrEmpty(registrationForm.EmailId))
                    throw new HiringBellException { UserMessage = $"Invalid Email address passed: {registrationForm.EmailId}" };

                if (string.IsNullOrEmpty(registrationForm.AuthenticationCode))
                    throw new HiringBellException { UserMessage = $"Invalid Authentication Code passed: {registrationForm.AuthenticationCode}" };

                registrationForm.FirstName = "Admin";
                registrationForm.LastName = "User";
                string EncreptedPassword = _authenticationService.Encrypt(
                    _configuration.GetSection("DefaultNewEmployeePassword").Value,
                    _configuration.GetSection("EncryptSecret").Value
                );
                registrationForm.Password = EncreptedPassword;

                var status = this.db.Execute<string>("sp_new_registration", new
                {
                    registrationForm.OrganizationName,
                    registrationForm.CompanyName,
                    registrationForm.Mobile,
                    registrationForm.EmailId,
                    registrationForm.FirstName,
                    registrationForm.LastName,
                    registrationForm.Password
                }, true);

                statusFlag = true;
                return statusFlag;
            });
        }
    }
}
