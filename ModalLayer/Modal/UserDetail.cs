using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModalLayer.Modal
{
    public class UserDetail : LoginDetail
    {
        public DateTime? Dob { set; get; }
        public string State { set; get; }
        public string City { set; get; }
        public string Designation { set; get; }
        public DateTime? CreatedOn { set; get; }
        public string Token { set; get; }
        public DateTime? TokenExpiryDuration { set; get; }
        public string RefreshToken { set; get; }
        public string Password { get; set; }
        public string MediaName { set; get; }
        public string AccessToken { set; get; }
    }

    public class LoginDetail
    {
        public long UserId { set; get; }
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public string Mobile { set; get; }
        public string EmailId { set; get; }
        public string Address { set; get; }
        public string CompanyName { set; get; }
        public int RoleId { set; get; }
    }

    public class LoginResponse
    {
        public UserDetail UserDetail { set; get; }
        public DataTable Menu { set; get; }
        public DataTable ReportColumnMapping { set; get; }
    }
}
