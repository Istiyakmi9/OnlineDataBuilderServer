using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public enum UserType
    {
        Employee = 1,
        Client = 2,
        Candidate = 3,
        Other = 4
    }

    public class DocumentFile
    {
        public long FileUid { set; get; }
        public long DocumentId { set; get; }
        public string ProfileUid { set; get; }
        public UserType UserTypeId { set; get; }
    }

    public class Files : DocumentFile
    {
        public string FilePath { set; get; }
        public string FileName { set; get; }
        public string FileExtension { set; get; }
        public string Status { set; get; }
        public DateTime? PaidOn { set; get; }
        public long BillTypeId { set; get; }
        public long UserId { set; get; }
        public string Mobile { set; get; }
        public string Email { set; get; }
        public string FileType { set; get; }
        public string FileSize { set; get; }
        public string? LocalImgPath { get; set; }
        public FileSystemType SystemFileType { set; get; }
    }
}
