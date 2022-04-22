using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.Profile;

namespace ServiceLayer.Interface
{
    public interface IUserService
    {
        string UploadUserInfo(string userId, ProfessionalUser userInfo, IFormFileCollection FileCollection, int UserTypeId);
        ProfileDetail GetUserDetail(long userId, int UserTypeId);
        string GenerateResume(long userId);
        ProfileDetail UpdateProfile(ProfessionalUser professionalUser, int UserTypeId, int IsProfileImageRequest = 0);
        string UploadResume(string userId, ProfessionalUser professionalUser, IFormFileCollection FileCollection, int UserTypeId);
        string UploadDeclaration(string UserId, int UserTypeId, UserDetail userDetail, IFormFileCollection FileCollection);
    }
}
