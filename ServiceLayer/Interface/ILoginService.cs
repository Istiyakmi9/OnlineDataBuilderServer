using ModalLayer.Modal;
using System;

namespace ServiceLayer.Interface
{
    public interface ILoginService
    {
        UserDetail GetLoginUserObject(AuthUser outhUser);
        Boolean RemoveUserDetailService(string Token);
        UserDetail GetUserDetail(AuthUser authUser);
    }
}
