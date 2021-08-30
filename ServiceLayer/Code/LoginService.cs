using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using Microsoft.Extensions.Options;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using SocialMediaServices;
using SocialMediaServices.Modal;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class LoginService : ILoginService
    {
        private readonly IDb db;
        private readonly JwtSetting _jwtSetting;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMediaService _mediaService;
        public LoginService(IDb db, IOptions<JwtSetting> options, IAuthenticationService authenticationService, IMediaService mediaService)
        {
            this.db = db;
            _jwtSetting = options.Value;
            _authenticationService = authenticationService;
            _mediaService = mediaService;
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
                new DbParam(authUser.MobileNo, typeof(string), "@Mobile"),
                new DbParam(authUser.Email, typeof(string), "@Email")
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
                    loginResponse = FetchUserDetail(userDetail, true);
            }
            return loginResponse;
        }

        public async Task<string> RegisterEmployee(Employee employee)
        {
            return await Task.Run(() =>
            {
                string status = "expired";
                long AdminId = 0;
                string AdminUid = _authenticationService.ReadJwtToken();
                if (!string.IsNullOrEmpty(AdminUid))
                {
                    AdminId = Convert.ToInt64(AdminUid);
                    if(AdminId > 0) {
                        DbParam[] param = new DbParam[]
                        {
                            new DbParam(employee.EmployeeUid, typeof(long), "_EmployeeUid"),
                            new DbParam(employee.FirstName, typeof(string), "_FirstName"),
                            new DbParam(employee.LastName, typeof(string), "_LastName"),
                            new DbParam(employee.Mobile, typeof(string), "_Mobile"),
                            new DbParam(employee.Email, typeof(string), "_Email"),
                            new DbParam(employee.SecondaryMobile, typeof(string), "_SecondaryMobile"),
                            new DbParam(employee.FatherName, typeof(string), "_FatherName"),
                            new DbParam(employee.MotherName, typeof(string), "_MotherName"),
                            new DbParam(employee.SpouseName, typeof(string), "_SpouseName"),
                            new DbParam(employee.Gender, typeof(bool), "_Gender"),
                            new DbParam(employee.State, typeof(string), "_State"),
                            new DbParam(employee.City, typeof(string), "_City"),
                            new DbParam(employee.Pincode, typeof(int), "_Pincode"),
                            new DbParam(employee.Address, typeof(string), "_Address"),
                            new DbParam(employee.PANNo, typeof(string), "_PANNo"),
                            new DbParam(employee.AadharNo, typeof(string), "_AadharNo"),
                            new DbParam(employee.AccountNumber, typeof(string), "_AccountNumber"),
                            new DbParam(employee.BankName, typeof(string), "_BankName"),
                            new DbParam(employee.BranchName, typeof(string), "_BranchName"),
                            new DbParam(employee.IFSCCode, typeof(string), "_IFSCCode"),
                            new DbParam(employee.Domain, typeof(string), "_Domain"),
                            new DbParam(employee.Specification, typeof(string), "_Specification"),
                            new DbParam(employee.ExprienceInYear, typeof(float), "_ExprienceInYear"),
                            new DbParam(employee.LastCompanyName, typeof(string), "_LastCompanyName"),
                            new DbParam(employee.IsPermanent, typeof(bool), "_IsPermanent"),
                            new DbParam(employee.AllocatedClientId, typeof(long), "_AllocatedClientId"),
                            new DbParam(employee.AllocatedClientName, typeof(string), "_AllocatedClientName"),
                            new DbParam(employee.ActualPackage, typeof(float), "_ActualPackage"),
                            new DbParam(employee.FinalPackage, typeof(float), "_FinalPackage"),
                            new DbParam(employee.TakeHomeByCandidate, typeof(float), "_TakeHomeByCandidate"),
                            new DbParam(AdminId, typeof(long), "_AdminId")
                        };

                        status = this.db.ExecuteNonQuery("sp_Employees_InsUpdate", param, true);
                    }
                }
                return status;
            });
        }

        public async Task<LoginResponse> FetchAuthenticatedUserDetail(UserDetail authUser)
        {
            return await Task.Run(() =>
            {
                LoginResponse loginResponse = default;
                RefreshTokenModal refreshTokenModal = _authenticationService.RenewAndGenerateNewToken(authUser.Mobile, authUser.EmailId);
                if (refreshTokenModal != null)
                {
                    if (!string.IsNullOrEmpty(authUser.EmailId))
                    {
                        loginResponse = FetchUserDetail(authUser, true);
                    }
                    else if (!string.IsNullOrEmpty(authUser.EmailId) && !string.IsNullOrEmpty(authUser.Mobile))
                    {
                        loginResponse = FetchUserDetail(authUser, false);
                    }
                }
                return loginResponse;
            });
        }

        private LoginResponse FetchUserDetail(UserDetail authUser, bool flag)
        {
            LoginResponse loginResponse = default;
            UserDetail userDetail = default;
            DbParam[] param = new DbParam[]
            {
                new DbParam(authUser.UserId, typeof(System.Int64), "_UserId"),
                new DbParam(authUser.Mobile, typeof(System.String), "_MobileNo"),
                new DbParam(authUser.EmailId, typeof(System.String), "_EmailId"),
                new DbParam(flag, typeof(System.Boolean), "_isAccessTokenAvailable"),
                new DbParam(authUser.Password, typeof(System.String), "_Password")
            };
            DataSet ds = db.GetDataset("sp_Userlogin_Auth", param);
            if (ds != null && ds.Tables.Count == 3)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    loginResponse = new LoginResponse();
                    var LoginDetailList = Converter.ToList<LoginDetail>(ds.Tables[0]);
                    var loginDetail = LoginDetailList.FirstOrDefault();
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
                            CompanyName = loginDetail.CompanyName
                        };

                        var _token = _authenticationService.Authenticate(userDetail.UserId);
                        if (_token != null)
                        {
                            userDetail.Token = _token.Token;
                            userDetail.TokenExpiryDuration = DateTime.Now.AddHours(_jwtSetting.AccessTokenExpiryTimeInHours);
                            userDetail.RefreshToken = _token.RefreshToken;
                        }

                        loginResponse.Menu = ds.Tables[1];
                        loginResponse.ReportColumnMapping = ds.Tables[2];
                        loginResponse.UserDetail = userDetail;
                    }
                }
            }

            return loginResponse;
        }
    }
}
