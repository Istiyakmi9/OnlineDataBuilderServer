using System;

public class FileDetail
{
    public long ClientId { get; set; }
    public long EmployeeId { set; get; }
    public long FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string LogoPath { set; get; }
    public string DiskFilePath { get; set; }
    public string FileExtension { get; set; }
    public long StatusId { get; set; }
    public DateTime? PaidOn { get; set; }
    public int Status { set; get; }
    public string GeneratedBillNo { set; get; }
    public DateTime UpdatedOn { set; get; }
    public string Notes { set; get; }
}