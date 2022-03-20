using Microsoft.AspNetCore.Http;
using ModalLayer.Modal.Profile;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface IUserService
    {
        string UploadUserInfo(string userId, UserInfo userInfo, IFormFileCollection FileCollection);
        string ManageEmploymentDetail(EmploymentDetail employmentDetail);
        string ManageEducationDetail(List<EducationDetail> educationDetails);
        DataSet GetUserDetail(long userId);
    }
}
