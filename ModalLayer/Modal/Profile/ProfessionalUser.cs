using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Profile
{
    public class ProfessionalUser
    {
        public string FirstName { set; get; }
        public List<EmploymentDetail> EmploymentDetails { set; get; }
        public List<EducationDetail> EducationDetails { set; get; }
    }
}
