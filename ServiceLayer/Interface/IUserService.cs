using Microsoft.AspNetCore.Http;
using ModalLayer.Modal.Profile;

namespace ServiceLayer.Interface
{
    public interface IUserService
    {
        string UploadUserInfo(string userId, ProfessionalUser userInfo, IFormFileCollection FileCollection);
        ProfileDetail GetUserDetail(long userId, int UserTypeId);
        string GenerateResume(long userId);
        ProfileDetail UpdateProfile(ProfessionalUser professionalUser, int UserTypeId, int IsProfileImageRequest = 0);
        string UploadResume(string userId, ProfessionalUser professionalUser, IFormFileCollection FileCollection);
    }
}
