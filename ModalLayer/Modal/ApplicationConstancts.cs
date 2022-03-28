public class ApplicationConstants
{
    public static int TDS = 10;
    public static int Completed = 1;
    public static int Pending = 2;
    public static int Canceled = 3;
    public static int NotGenerated = 4;
    public static int Rejected = 5;

    public const string DocumentRootPath = "documents";
    public const string User = "user";
    public const string InserUserFileDetail = "sp_document_filedetail_insupd";
    public const string InserUpdateAttendance = "sp_attendance_insupd";
    public const string deleteUserFile = "sp_document_filedetail_delete";
    public const string GetUserFileById = "sp_document_filedetail_getById";

    public const string JWTBearer = "Bearer";
    public const string ProfileImage = "profile";
    public const string Pdf = "pdf";
    public const string Docx = "docx";
}

public enum FileSystemType
{
    User = 1,
    Bills = 2
}