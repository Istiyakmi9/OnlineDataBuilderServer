using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Data;
using System.Linq;

namespace ServiceLayer.Code
{
    public class LoginService : ILoginService
    {
        private readonly IDb db;
        private readonly IAuthenticationService _authenticationService;
        public LoginService(IDb db, IAuthenticationService authenticationService)
        {
            this.db = db;
            _authenticationService = authenticationService;
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

        public string SignUpUser(UserDetail userDetail)
        {
            DbParam[] param = new DbParam[]
            {
                new DbParam(userDetail.UserUid, typeof(long), "_UserId"),
                new DbParam(userDetail.FirstName, typeof(string), "_FirstName"),
                new DbParam(userDetail.LastName, typeof(string), "_LastName"),
                new DbParam(userDetail.MobileNo, typeof(string), "_MobileNo"),
                new DbParam(userDetail.EmailId, typeof(string), "_EmailId"),
                new DbParam(userDetail.Address, typeof(string), "_Address"),
                new DbParam(userDetail.Company, typeof(string), "_CompanyName"),
                new DbParam(null, typeof(string), "_AdminId")
            };

            var ResultSet = this.db.ExecuteNonQuery("sp_UserDetail_insupd", param, false);

            return ResultSet;
        }

        public UserDetail GetLoginUserObject(AuthUser authUser)
        {
            UserDetail userDetail = default;
            Boolean IsMobileNoFlag = false;
            if (!string.IsNullOrEmpty(authUser.UserId))
            {
                if (authUser.UserId.IndexOf('@') > 0)
                {
                    authUser.Email = authUser.UserId;
                }
                else
                {
                    IsMobileNoFlag = true;
                    authUser.MobileNo = authUser.UserId;
                }
            }
            else
                return null;
            DbParam[] param = new DbParam[]
            {
                new DbParam(authUser.Password, typeof(System.String), "@PASSWORD"),
                new DbParam(authUser.Email, typeof(System.String), "@EMAILID"),
                new DbParam(authUser.MobileNo, typeof(System.String), "@MOBILENO"),
                new DbParam(IsMobileNoFlag, typeof(System.Boolean), "@ISMOBILE")
            };
            DataSet ds = db.GetDataset("sp_Userlogin_Get", param);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                userDetail = new UserDetail();
                if (ds.Tables[0].Rows[0]["UserUid"] != DBNull.Value)
                    userDetail.UserUid = Convert.ToInt64(ds.Tables[0].Rows[0]["UserUid"]);
                else
                    userDetail.UserUid = 0;

                if (ds.Tables[0].Rows[0]["MobileNo"] != DBNull.Value)
                    userDetail.MobileNo = ds.Tables[0].Rows[0]["MobileNo"].ToString();
                else
                    userDetail.MobileNo = null;

                if (ds.Tables[0].Rows[0]["EmailId"] != DBNull.Value)
                    userDetail.EmailId = ds.Tables[0].Rows[0]["EmailId"].ToString();
                else
                    userDetail.EmailId = null;

                if (ds.Tables[0].Rows[0]["RoleUid"] != DBNull.Value)
                    userDetail.RoleUid = ds.Tables[0].Rows[0]["RoleUid"].ToString();
                else
                    userDetail.RoleUid = null;

                if (ds.Tables[0].Rows[0]["FirstName"] != DBNull.Value)
                    userDetail.FirstName = ds.Tables[0].Rows[0]["FirstName"].ToString();
                else
                    userDetail.FirstName = "UNKNOWN";

                if (ds.Tables[0].Rows[0]["LastName"] != DBNull.Value)
                    userDetail.LastName = ds.Tables[0].Rows[0]["LastName"].ToString();
                else
                    userDetail.LastName = "USER";

                if (ds.Tables[0].Rows[0]["Address"] != DBNull.Value)
                    userDetail.Address = ds.Tables[0].Rows[0]["Address"].ToString();
                else
                    userDetail.Address = null;

                if (ds.Tables[0].Rows[0]["Designation"] != DBNull.Value)
                    userDetail.Designation = ds.Tables[0].Rows[0]["Designation"].ToString();
                else
                    userDetail.Designation = "USER";

                if (ds.Tables[0].Rows[0]["Dob"] != DBNull.Value)
                    userDetail.Dob = Convert.ToDateTime(ds.Tables[0].Rows[0]["Dob"]);
                else
                    userDetail.Dob = DateTime.Now;

                var _token = _authenticationService.Authenticate(authUser.UserId);
                if (_token != null)
                {
                    userDetail.Token = _token.Token;
                    userDetail.RefreshToken = _token.RefreshToken;
                }
            }

            return userDetail;
        }
    }
}
