﻿using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using DocMaker.ExcelMaker;
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
        private readonly IMediaService _mediaService;
        private readonly CurrentSession _currentSession;
        private readonly IAuthenticationService _authenticationService;
        private readonly ExcelWriter _excelWriter;

        public LoginService(IDb db, IOptions<JwtSetting> options,
            CurrentSession currentSession,
            IMediaService mediaService,
            IAuthenticationService authenticationService,
            ExcelWriter excelWriter)
        {
            this.db = db;
            _jwtSetting = options.Value;
            _currentSession = currentSession;
            _mediaService = mediaService;
            _authenticationService = authenticationService;
            _excelWriter = excelWriter;
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

        public async Task<LoginResponse> FetchAuthenticatedUserDetail(UserDetail authUser)
        {
            return await Task.Run(() =>
            {
                LoginResponse loginResponse = default;
                if ((!string.IsNullOrEmpty(authUser.EmailId) || !string.IsNullOrEmpty(authUser.Mobile)) && !string.IsNullOrEmpty(authUser.Password))
                {
                    loginResponse = FetchUserDetail(authUser, "sp_Candidatelogin_Auth");
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

        private LoginResponse FetchUserDetail(UserDetail authUser, string ProcedureName)
        {
            LoginResponse loginResponse = default;
            UserDetail userDetail = default;
            DbParam[] param = new DbParam[]
            {
                new DbParam(authUser.UserId, typeof(System.Int64), "_UserId"),
                new DbParam(authUser.Mobile, typeof(System.String), "_MobileNo"),
                new DbParam(authUser.EmailId, typeof(System.String), "_EmailId"),
                new DbParam(authUser.UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(authUser.Password, typeof(System.String), "_Password")
            };
            DataSet ds = db.GetDataset(ProcedureName, param);
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
                            CompanyName = loginDetail.CompanyName,
                            UserTypeId = loginDetail.UserTypeId
                        };

                        var _token = _authenticationService.Authenticate(userDetail.UserId, loginDetail.RoleId);
                        if (_token != null)
                        {
                            userDetail.Token = _token.Token;
                            userDetail.TokenExpiryDuration = DateTime.Now.AddHours(_jwtSetting.AccessTokenExpiryTimeInSeconds);
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
