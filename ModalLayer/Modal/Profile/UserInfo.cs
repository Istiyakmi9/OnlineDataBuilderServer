using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Profile
{
    public class UserInfo
    {
        public long UserId { get; set; }
        public long FileId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string ProfileImgPath { get; set; }
        public string ResumeHeadline { get; set; }
        public string ResumePath { get; set; }
    }
}
