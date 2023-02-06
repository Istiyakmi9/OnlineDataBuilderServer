using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using EMailService.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Caching;
using ServiceLayer.Code.SendEmail;
using ServiceLayer.Interface;
using SocialMediaServices;
using SocialMediaServices.Modal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text;
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
        private readonly IEMailManager _emailManager;
        private readonly ICommonService _commonService;
        private readonly ForgotPasswordEmailService _forgotPasswordEmailService;

        public LoginService(IDb db, IOptions<JwtSetting> options,
            IMediaService mediaService,
            IAuthenticationService authenticationService,
            ICacheManager cacheManager,
            IEMailManager emailManager,
            ICommonService commonService,
            IConfiguration configuration, ForgotPasswordEmailService forgotPasswordEmailService)
        {
            this.db = db;
            _configuration = configuration;
            _jwtSetting = options.Value;
            _mediaService = mediaService;
            _authenticationService = authenticationService;
            _cacheManager = cacheManager;
            _emailManager = emailManager;
            _commonService = commonService;
            _forgotPasswordEmailService = forgotPasswordEmailService;
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
                var ResultSet = this.db.Execute<string>("sp_UserDetail_Ins", new
                {
                    UserId = userDetail.UserId,
                    FirstName = userDetail.FirstName,
                    LastName = userDetail.LastName,
                    MobileNo = userDetail.Mobile,
                    EmailId = userDetail.EmailId,
                    Address = userDetail.Address,
                    CompanyName = userDetail.CompanyName,
                    AdminId = 0
                }, true);
                if (!string.IsNullOrEmpty(ResultSet))
                    loginResponse = await FetchUserDetail(userDetail, "sp_Userlogin_Auth");
            }
            return loginResponse;
        }


        public string GetUserLoginDetail(UserDetail authUser)
        {
            string encryptedPassword = string.Empty;

            if (!string.IsNullOrEmpty(authUser.EmailId))
                authUser.EmailId = authUser.EmailId.Trim().ToLower();

            var loginDetail = db.Get<UserDetail>("sp_password_get_by_email_mobile", new
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
                authUser.UserTypeId = loginDetail.UserTypeId;
            }
            else
            {
                throw new HiringBellException("Fail to get user detail. UserDetail");
            }

            return encryptedPassword;
        }

        public string FetchUserLoginDetail(UserDetail authUser)
        {
            string encryptedPassword = string.Empty;

            if (!string.IsNullOrEmpty(authUser.EmailId))
                authUser.EmailId = authUser.EmailId.Trim().ToLower();

            var loginDetail = db.Get<UserDetail>("sp_password_get", new
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
                throw new HiringBellException("Fail to retrieve user detail.", "UserDetail", JsonConvert.SerializeObject(authUser));
            }

            return encryptedPassword;
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

            LoginResponse loginResponse = default;
            if ((!string.IsNullOrEmpty(authUser.EmailId) || !string.IsNullOrEmpty(authUser.Mobile)) && !string.IsNullOrEmpty(authUser.Password))
            {
                loginResponse = await FetchUserDetail(authUser, ProcedureName);
            }

            return loginResponse;
        }

        public async Task<LoginResponse> AuthenticateUser(UserDetail authUser)
        {
            LoginResponse loginResponse = default;
            if ((!string.IsNullOrEmpty(authUser.EmailId) || !string.IsNullOrEmpty(authUser.Mobile)) && !string.IsNullOrEmpty(authUser.Password))
            {
                var encryptedPassword = this.GetUserLoginDetail(authUser);
                encryptedPassword = _authenticationService.Decrypt(encryptedPassword, _configuration.GetSection("EncryptSecret").Value);
                if (encryptedPassword.CompareTo(authUser.Password) != 0)
                {
                    throw new HiringBellException("Invalid userId or password.");
                }

                loginResponse = await FetchUserDetail(authUser, "sp_Employeelogin_Auth");
            }

            return await Task.FromResult(loginResponse);
        }

        private async Task<LoginResponse> FetchUserDetail(UserDetail authUser, string ProcedureName)
        {
            LoginResponse loginResponse = default;
            DataSet ds = await db.GetDataSet(ProcedureName, new
            {
                UserId = authUser.UserId,
                MobileNo = authUser.Mobile,
                EmailId = authUser.EmailId,
                UserTypeId = authUser.UserTypeId,
                PageSize = 1000
            });

            if (ds != null && ds.Tables.Count == 4)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    loginResponse = new LoginResponse();
                    var loginDetail = Converter.ToType<LoginDetail>(ds.Tables[0]);
                    if (loginDetail != null)
                    {
                        var userDetail = new UserDetail
                        {
                            FirstName = loginDetail.FirstName,
                            LastName = loginDetail.LastName,
                            Address = loginDetail.Address,
                            Mobile = loginDetail.Mobile,
                            Email = loginDetail.Email,
                            UserId = loginDetail.UserId,
                            CompanyName = loginDetail.CompanyName,
                            UserTypeId = loginDetail.UserTypeId,
                            OrganizationId = loginDetail.OrganizationId,
                            CompanyId = loginDetail.CompanyId,
                            ManagerName = loginDetail.ManagerName,
                            ReportingManagerId = loginDetail.ReportingManagerId,
                            UpdatedOn = loginDetail.UpdatedOn,
                            EmployeeCurrentRegime = loginDetail.EmployeeCurrentRegime,
                            DOB = loginDetail.DOB,
                            CreatedOn = loginDetail.CreatedOn
                        };

                        var _token = _authenticationService.Authenticate(userDetail);
                        if (_token != null)
                        {
                            userDetail.Token = _token.Token;
                            userDetail.TokenExpiryDuration = DateTime.Now.AddHours(_jwtSetting.AccessTokenExpiryTimeInSeconds);
                            userDetail.RefreshToken = _token.RefreshToken;
                        }

                        loginResponse.Menu = ds.Tables[1];
                        loginResponse.Department = ds.Tables[3];
                        loginResponse.UserDetail = userDetail;
                        loginResponse.UserTypeId = authUser.UserTypeId;
                        loginResponse.Companies = _cacheManager.Get(CacheTable.Company);
                        loginResponse.EmployeeList = ds.Tables[2].AsEnumerable()
                                                       .Select(x => new AutoCompleteEmployees
                                                       {
                                                           value = x.Field<long>("EmployeeUid"),
                                                           text = x.Field<string>("Name"),
                                                           email = x.Field<string>("Email")
                                                       }).ToList<AutoCompleteEmployees>();
                    }
                }
            }

            return loginResponse;
        }

        public string ResetEmployeePassword(UserDetail authUser)
        {
            string Status = string.Empty;
            var encryptedPassword = this.FetchUserLoginDetail(authUser);
            encryptedPassword = _authenticationService.Decrypt(encryptedPassword, _configuration.GetSection("EncryptSecret").Value);
            if (encryptedPassword != authUser.Password)
                throw new HiringBellException("Incorrect old password");

            string newEncryptedPassword = _authenticationService.Encrypt(authUser.NewPassword, _configuration.GetSection("EncryptSecret").Value);
            var result = db.Execute<string>("sp_Reset_Password", new
            {
                EmailId = authUser.EmailId,
                MobileNo = authUser.Mobile,
                NewPassword = newEncryptedPassword,
            }, true);

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

        public async Task<string> ForgotPasswordService(string email)
        {
            try
            {
                string Status = string.Empty;
                if (string.IsNullOrEmpty(email))
                    throw new HiringBellException("Email is null or empty");

                ValidateEmailId(email);
                UserDetail authUser = new UserDetail();
                authUser.EmailId = email;
                var encryptedPassword = this.FetchUserLoginDetail(authUser);

                if (string.IsNullOrEmpty(encryptedPassword))
                    throw new HiringBellException("Email id is not registered. Please contact to admin");

                var password = _authenticationService.Decrypt(encryptedPassword, _configuration.GetSection("EncryptSecret").Value);

                await _forgotPasswordEmailService.SendForgotPasswordEmail(password, email);
                Status = ApplicationConstants.Successfull;
                return Status;
            }
            catch (Exception)
            {
                throw new HiringBellException("Getting some server error. Please contact to admin.");
            }
        }

        private void BuildEmailBody(EmailTemplate emailTemplate, string password)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<div>" + emailTemplate.Salutation + "</div>");
            string body = JsonConvert.DeserializeObject<string>(emailTemplate.BodyContent)
                          .Replace("[[NEW-PASSWORD]]", password);

            stringBuilder.Append("<div>" + emailTemplate.EmailClosingStatement + "</div>");
            stringBuilder.Append("<div>" + emailTemplate.SignatureDetail + "</div>");
            stringBuilder.Append("<div>" + emailTemplate.ContactNo + "</div>");

            emailTemplate.BodyContent = body + stringBuilder.ToString();
        }

        private void ValidateEmailId(string email)
        {
            var mail = new MailAddress(email);
            bool isValidEmail = mail.Host.Contains(".");
            if (!isValidEmail)
                throw new HiringBellException("The email is invalid");
        }
    }
}
