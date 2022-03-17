using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface IUserService
    {
        string UploadResume(Files fileDetail, IFormFileCollection FileCollection);
        string ManageEmploymentDetail(EmploymentDetail employmentDetail);
        string ManageEducationDetail(List<EducationDetail> educationDetails);
    }
}
