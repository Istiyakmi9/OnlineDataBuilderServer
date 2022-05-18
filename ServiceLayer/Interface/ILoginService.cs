using ModalLayer.Modal;
using System;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface ILoginService
    {
        Task<LoginResponse> FetchAuthenticatedUserDetail(UserDetail authUser);
        Task<LoginResponse> FetchAuthenticatedProviderDetail(UserDetail authUser);
        Boolean RemoveUserDetailService(string Token);
        UserDetail GetUserDetail(AuthUser authUser);
        Task<LoginResponse> SignUpUser(UserDetail userDetail);
        void BuildApplicationCache(bool isRelead = false);
        string ResetEmployeePassword (UserDetail passwords);
    }
}
