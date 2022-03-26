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
    }
}
