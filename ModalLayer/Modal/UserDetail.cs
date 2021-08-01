using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModalLayer.Modal
{
    public class UserDetail
    {
        public long UserUid { set; get; }
        public string RoleUid { set; get; }
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public string MobileNo { set; get; }
        public string EmailId { set; get; }
        public string Address { set; get; }
        public DateTime Dob { set; get; }
        public string State { set; get; }
        public string City { set; get; }
        public string Designation { set; get; }
        public DateTime CreatedOn { set; get; }
        public string Company { set; get; }
        public string Token { set; get; }
        public string RefreshToken { set; get; }
        public string Password { get; set; }
    }
}
