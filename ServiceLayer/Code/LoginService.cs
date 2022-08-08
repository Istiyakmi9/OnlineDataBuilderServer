using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using DocMaker.ExcelMaker;
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
using Table = ServiceLayer.Caching.Table;

namespace ServiceLayer.Code
{
    public class LoginService : ILoginService
    {
        private readonly IDb db;
        private readonly JwtSetting _jwtSetting;
        private readonly IMediaService _mediaService;
        private readonly CurrentSession _currentSession;
        private readonly IAuthenticationService _authenticationService;
        private readonly ExcelWriter _excelWriter;
        private readonly ICacheManager _cacheManager;
        private readonly IConfiguration _configuration;

        public LoginService(IDb db, IOptions<JwtSetting> options,
            CurrentSession currentSession,
            IMediaService mediaService,
            IAuthenticationService authenticationService,
            ICacheManager cacheManager,
            IConfiguration configuration,
            ExcelWriter excelWriter)
        {
            this.db = db;
            _configuration = configuration;
            _jwtSetting = options.Value;
            _currentSession = currentSession;
            _mediaService = mediaService;
            _authenticationService = authenticationService;
            _excelWriter = excelWriter;
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


        public string FetchPasswordByRoleType(UserDetail authUser, string role)
        {
            string encryptedPassword = string.Empty;

            if (!string.IsNullOrEmpty(authUser.EmailId))
                authUser.EmailId = authUser.EmailId.Trim().ToLower();

            DbParam[] param = new DbParam[]
            {
                new DbParam(authUser.UserId, typeof(System.Int64), "_UserId"),
                new DbParam(authUser.Mobile, typeof(System.String), "_MobileNo"),
                new DbParam(authUser.EmailId, typeof(System.String), "_EmailId"),
                new DbParam(authUser.UserTypeId, typeof(int), "_UserTypeId"),
            };

            DataSet ds = db.GetDataset("sp_Password_GetByRole", param);
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                encryptedPassword = ds.Tables[0].Rows[0]["Password"].ToString();
                int userTypeId = Convert.ToInt32(ds.Tables[0].Rows[0]["UserTypeId"]);

                switch (userTypeId)
                {
                    case 1:
                        {
                            if (role != Role.Admin)
                                throw new HiringBellException("Invalid username or password for the current role.");
                        }
                        break;
                    case 2:
                        {
                            if (role != Role.Employee)
                                throw new HiringBellException("Invalid username or password for the current role.");
                        }
                        break;
                    default:
                        throw new HiringBellException("Invalid username or password for the current role.");
                }
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
            this.BuildApplicationCache();
            LoginResponse loginResponse = default;
            UserDetail userDetail = default;

            var encryptedPassword = this.FetchPasswordByRoleType(authUser, role);
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
            if (ds != null && ds.Tables.Count == 2)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    loginResponse = new LoginResponse();
                    var loginDetail = Converter.ToType<LoginDetail>(ds.Tables[0]);

                    if (loginDetail != null)
                    {
                        userDetail = new UserDetail
                        {
                            FirstName = loginDetail.FirstName,
                            LastName = loginDetail.LastName,
                            Address = loginDetail.Address,
                            Mobile = loginDetail.Mobile,
                            EmailId = loginDetail.EmailId,
                            UserId = loginDetail.UserId,
                            CompanyName = loginDetail.CompanyName,
                            UserTypeId = loginDetail.UserTypeId,
                            ReportingManagerId = loginDetail.ReportingManagerId
                        };

                        var _token = _authenticationService.Authenticate(userDetail.UserId, userDetail.ReportingManagerId, loginDetail.UserTypeId);
                        if (_token != null)
                        {
                            userDetail.Token = _token.Token;
                            userDetail.TokenExpiryDuration = DateTime.Now.AddHours(_jwtSetting.AccessTokenExpiryTimeInSeconds);
                            userDetail.RefreshToken = _token.RefreshToken;
                        }

                        loginResponse.Menu = ds.Tables[1];
                        loginResponse.UserDetail = userDetail;
                    }
                }
            }

            return loginResponse;
        }

        public void BuildApplicationCache(bool isRelead = false)
        {
            string ProcedureName = "SP_ApplicationData_Get";
            if (_cacheManager.IsEmpty() || isRelead)
            {
                DataSet ds = db.GetDataset(ProcedureName, null);

                if (ds.Tables.Count == 5)
                {
                    if (isRelead)
                        _cacheManager.Clean();

                    _cacheManager.Add(Table.Client, ds.Tables[0]);
                    _cacheManager.Add(Table.Employee, ds.Tables[1]);
                    _cacheManager.Add(Table.EmployeeRoles, ds.Tables[2]);
                    _cacheManager.Add(Table.Companies, ds.Tables[3]);
                }
                else
                {
                    throw new HiringBellException("Unable to load application data");
                }
            }
        }

        public string ResetEmployeePassword(UserDetail authUser, string role)
        {
            string Status = string.Empty;
            var encryptedPassword = this.FetchPasswordByRoleType(authUser, role);
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
    }
}
