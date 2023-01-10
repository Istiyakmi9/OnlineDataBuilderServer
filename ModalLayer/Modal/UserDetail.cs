using ModalLayer.Modal.Accounts;
using System;
using System.Collections.Generic;
using System.Data;

namespace ModalLayer.Modal
{
    public class UserDetail : LoginDetail
    {
        public string State { set; get; }
        public string City { set; get; }
        public string Designation { set; get; }
        public string Token { set; get; }
        public DateTime? TokenExpiryDuration { set; get; }
        public string RefreshToken { set; get; }
        public string MediaName { set; get; }
        public string AccessToken { set; get; }
        public int AdminId { get; set; }
        public string NewPassword { get; set; }
    }

    public class LoginDetail
    {
        public long UserId { set; get; }
        public long EmployeeId { set; get; }
        public int OrganizationId { set; get; }
        public int CompanyId { set; get; }
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public string FullName { set; get; }
        public string ManagerName { set; get; }
        public string Mobile { set; get; }
        public string EmailId { set; get; }
        public string Email { set; get; }
        public string Address { set; get; }
        public string CompanyName { set; get; }
        public int RoleId { set; get; }
        public int AccessLevelId { set; get; }
        public int UserTypeId { set; get; }
        public string Password { set; get; }
        public long ReportingManagerId { set; get; }
        public DateTime UpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public int EmployeeCurrentRegime { get; set; }
        public DateTime? DOB { set; get; }
    }

    public class LoginResponse
    {
        public UserDetail UserDetail { set; get; }
        public DataTable Menu { set; get; }
        public DataTable Companies { set; get; }
        public int UserTypeId { set; get; }
        public List<AutoCompleteEmployees> EmployeeList { set; get; }
    }

    public class AutoCompleteEmployees
    {
        public long value { set; get; }
        public string text { set; get; }
    }
}
