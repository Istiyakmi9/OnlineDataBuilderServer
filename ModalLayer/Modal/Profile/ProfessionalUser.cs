using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Profile
{
    public class ProfileDetail
    {
        public ProfessionalUser professionalUser { set; get; }
        public List<FileDetail> profileDetail { set; get; }
        public UserDetail userDetail { set; get; }
        public int RoleId { set; get; }
    }

    public class ProfessionalUser
    {
        public long UserId { get; set; }
        public long FileId { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ResumeHeadline { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Job_Title { get; set; }
        public double Expeceted_CTC { get; set; }
        public string Mobile_Number { get; set; }
        public int Notice_Period { get; set; }
        public double Salary_Package { get; set; }
        public long? Alternate_Number { get; set; }
        public string Current_Location { get; set; }
        public DateTime Date_Of_Application { get; set; }
        public double Total_Experience_In_Months { get; set; }

        public ActivityStatus Activity_Status { get; set; }
        public OtherDetail Other_Detail { get; set; }
        public List<string> Preferred_Locations { get; set; } = new List<string>();
        public List<SkillDetail> Skills { get; set; } = new List<SkillDetail>();
        public List<Company> Companies { get; set; } = new List<Company>();
        public List<EducationalDetail> Educational_Detail { set; get; } = new List<EducationalDetail>();
        public List<ProjectDetail> Projects { get; set; } = new List<ProjectDetail>();
        public AccomplishmentsDetail Accomplishments { get; set; } = new AccomplishmentsDetail();
        public PersonalDetail PersonalDetail { get; set; } = new PersonalDetail();
        public List<EmploymentDetail> Employments { set; get; } = new List<EmploymentDetail>();
    }
}
