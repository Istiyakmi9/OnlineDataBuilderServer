using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.Profile;

namespace ServiceLayer.Interface
{
    public interface IUserService
    {
        string UploadUserInfo(string userId, UserInfo userInfo, IFormFileCollection FileCollection);
    }
}
