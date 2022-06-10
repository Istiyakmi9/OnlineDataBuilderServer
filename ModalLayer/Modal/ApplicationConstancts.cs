public class ApplicationConstants
{
    public static int TDS = 10;
    public static int Completed = 1;
    public static int Pending = 2;
    public static int Canceled = 3;
    public static int NotGenerated = 4;
    public static int Rejected = 5;

    public const string InserUserFileDetail = "sp_document_filedetail_insupd";
    public const string InserUpdateAttendance = "sp_attendance_insupd";
    public const string deleteUserFile = "sp_document_filedetail_delete";
    public const string GetUserFileById = "sp_document_filedetail_getById";

    public const string JWTBearer = "Bearer";
    public const string ProfileImage = "profile";
    public const string Pdf = "pdf";
    public const string Docx = "docx";
    public const string Excel = "xlsx";

    public const string Inserted = "inserted";
    public const string Updated = "updated";


    public static bool IsExecuted(string Result)
    {
        if (Result.ToLower() == Inserted || Result.ToLower() == Updated)
            return true;
        return false;
    }
}

public enum FileSystemType
{
    User = 1,
    Bills = 2
}

public enum RolesName
{
    Admin = 1,
    User = 2,
    Other = 3
}

public enum DayStatus
{
    Empty = 0,
    WorkFromOffice = 1,
    WorkFromHome = 2,
    Weekend = 3,
    Holiday = 4,
    OnLeave = 5
}

public enum ItemStatus
{
    Completed = 1,
    Pending = 2,
    Canceled = 3,
    NotGenerated = 4,
    Rejected = 5,
    Generated = 6,
    Raised = 7,
    Submitted = 8,
    Approved = 9
}