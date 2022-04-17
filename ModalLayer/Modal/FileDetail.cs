using ModalLayer.Modal;
using System;

public class FileDetail : GenerateBillFileDetail
{
    public string LogoPath { set; get; }
    public string DiskFilePath { get; set; }
    public long StatusId { get; set; }
    public DateTime? PaidOn { get; set; }
    public int Status { set; get; }
    public string GeneratedBillNo { set; get; }
    public DateTime UpdatedOn { set; get; }
    public string Notes { set; get; }
    public long FileOwnerId { set; get; }
}