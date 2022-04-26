﻿using System;

namespace ModalLayer.Modal
{
    public enum UserType
    {
        Admin = 1,
        Employee = 2,
        Candidate = 3,
        Client = 4,
        Other = 5
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
        public string ParentFolder { set; get; } = null;
        public FileSystemType SystemFileType { set; get; }
    }
}
