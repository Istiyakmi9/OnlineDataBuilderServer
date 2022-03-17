using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public class ManageUserDetail
    {
        
        public string Designation { get; set; }
        public string YourOrganization { get; set; }
        public string CurrentCompany { get; set; }
        public int WorkingYear { get; set; }
        public int WorkingMonth { get; set; }
        public int WorkedYear { get; set; }
        public string CurrentSalary { get; set; }
        public int CurrentSalaryLakh { get; set; }
        public int Experties { get; set; }
        public string JobProfile { get; set; }
        public string NoticePeriod { get; set; }
        public int CurrentSalaryThousand { get; set; }
        public string ITSkill { get; set; }
        public int Version { get; set; }
        public string LastUsed { get; set; }
        public int ExperienceYear { get; set; }
        public int ExperienceMonth { get; set; }
        public string KeySkill { get; set; }
        public string Education { get; set; }
        public string Course { get; set; }
        public string Specialization { get; set; }
        public string University { get; set; }
        public string CourseType { get; set; }
        public int PassingYear { get; set; }
        public string GradingSystem { get; set; }
        public string ProjectTitle { get; set; }
        public string ProjectTag { get; set; }
        public int ProjectWorkingYear { get; set; }
        public int ProjectWorkingMonth { get; set; }
        public int ProjectWorkedYear { get; set; }
        public string ProjectStatus { get; set; }
        public string ClientName { get; set; }
        public string ProjectDetail { get; set; }
        public string CurrentIndustry { get; set; }
        public string Department { get; set; }
        public string RoleCategory { get; set; }
        public string JobRole { get; set; }
        public string DesiredJob { get; set; }
        public string EmploymentType { get; set; }
        public string PreferredShift { get; set; }
        public string PreferredWorkLocation { get; set; }
        public string ExpectedSalary { get; set; }
        public int ExpectedSalaryLakh { get; set; }
        public int ExpectedSalaryThousand { get; set; }
        public string ProfileSummary { get; set; }
        public string OnlineProfile { get; set; }
        public string WorkSample { get; set; }
        public string Research { get; set; }
        public string Presentation { get; set; }
        public string Patent { get; set; }
        public string Certification { get; set; }
        public int UserId { get; set; }
    }


    public class PersonalDetail
    {
        public DateTime DOB { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string HomeTown { get; set; }
        public int PinCode { get; set; }
        public string MaritalStatus { get; set; }
        public string Category { get; set; }
        public string DifferentlyAbled { get; set; }
        public string PermitUSA { get; set; }
        public string PermitOtherCountry { get; set; }
        public List<LanguageDetail> LanguageDetails { get; set; }
    }

    public class LanguageDetail
    {
        public string Language { get; set; }
        public bool LanguageRead { get; set; }
        public bool LanguageWrite { get; set; }
        public string ProficiencyLanguage { get; set; }
        public bool LanguageSpeak { get; set; }
    }
}
