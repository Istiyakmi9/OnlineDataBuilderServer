using ModalLayer.Modal;
using System;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface ILoginService
    {
        Task<LoginResponse> FetchAuthenticatedUserDetail(UserDetail authUser);
        Task<LoginResponse> FetchAuthenticatedAdminDetail(UserDetail authUser);
        Task<LoginResponse> FetchAuthenticatedProviderDetail(UserDetail authUser);
        Task<bool> RegisterNewCompany(RegistrationForm registrationForm);
        Boolean RemoveUserDetailService(string Token);
        UserDetail GetUserDetail(AuthUser authUser);
        Task<LoginResponse> SignUpUser(UserDetail userDetail);
        string ResetEmployeePassword (UserDetail authUser);
        string ForgotPasswordService(string email);
    }
}
