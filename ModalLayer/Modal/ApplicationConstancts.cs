using ModalLayer;
using System.Data;
using System.Dynamic;

public class ApplicationConstants
{
    public static int TDS = 10;
    public static int Completed = 1;
    public static int Pending = 2;
    public static int Canceled = 3;
    public static int NotGenerated = 4;
    public static int Rejected = 5;
    public static int DefaultTaxRegin = 1;
    public static int OldRegim = 1;
    public static int NewRegim = 2;

    public const int Regular = 0;
    public const int InNoticePeriod = 1;
    public const int InProbationPeriod = 2;

    public static int Limit_80C = 150000;
    public static string NPS_Section = "80CCD(1)";
    public static int NPS_Allowed_Limit = 50000;
    public static string AutoCalculation = "[AUTO]";
    public static string LastInsertedKey = "__PLACEHOLDER__";
    public static string LastInsertedNumericKey = "-999";
    public static string OrganizationId = "OrganizationId";
    public static string CompanyId = "CompanyId";
    public static string ManagerName = "ManagerName";
    public static string ManagerId = "ManagerId";
    public static string EmployeeName = "EmployeeName";
    public static string EmployeeId = "EmployeeId";
    public static string JBot = "JBot";
    public static string CompanyCode = "CompanyCode";
    public static string HiringBellTransparentLogo = "hb-logo.png";
    public static string HiringBellLogoSmall = "hb-logo-low.png";
    public static string HiringBellLogo = "hb-t-logo.png";
    public static string LogoContentId = "bottomhalf@me.com";
    public static string DeclarationDocumentPath = "declarated_documents";
    public static string LeaveAttachmentPath = "leave_attachment";
    public static string ProcessingResult = "_ProcessingResult";
    public static string CompanyPrimaryLogo = "Company Primary Logo";
    public static string NullValue = null;
    public static int EmptyId = 0;

    public const string InserUserFileDetail = "sp_document_filedetail_insupd";
    public const string InserUpdateAttendance = "sp_attendance_insupd";
    public const string InsertUpdateTimesheet = "sp_timesheet_insupd";
    public const string deleteUserFile = "sp_document_filedetail_delete";
    public const string GetUserFileById = "sp_document_filedetail_getById";

    public const string JWTBearer = "Bearer";
    public const string ProfileImage = "profile";
    public const string Pdf = "pdf";
    public const string Docx = "docx";
    public const string Excel = "xlsx";

    public const string Inserted = "inserted";
    public const string Deleted = "deleted";
    public const string Updated = "updated";
    public const string EmptyJsonObject = "{}";
    public const string EmptyJsonArray = "[]";

    public const string WorkFromHome = "Work From Home";
    public const string WorkFromOffice = "Work From Office";
    public const string Leave = "Leave";
    public const string LeaveRequest = "Leave Request";
    public const string Timesheet = "Timesheet";
    public const string DailyAttendance = "Attendance: ";
    public const int Only = 1;
    public const int All = 0;

    public const string Successfull = "Successfull";
    public const string Fail = "Fail";
    public const string Submitted = "Submitted";


    public const string ExemptionDeclaration = "ExemptionDeclaration";
    public const string OneAndHalfLakhsExemptions = "1.5 Lac Exemptions";
    public const string OtherDeclaration = "OtherDeclaration";
    public const string OtherDeclarationName = "Other Exemptions";
    public const string TaxSavingAlloance = "TaxSavingAlloance";
    public const string TaxSavingAlloanceName = "Tax Saving Allowance";
    public const string HouseProperty = "House Property";
    public const string IncomeFromOtherSources = "Income From Other Sources";
    public const string Section16TaxExemption = "Section16TaxExemption";

    public const int BillingEmailTemplate = 1;
    public const int AttendanceSubmittedEmailTemplate = 2;
    public const int TimesheetSubmittedEmailTemplate = 3;
    public const int NewRegistrationEmailTemplate = 4;
    public const int OfferLetterEmailTemplate = 5;
    public const int LeaveApplyEmailTemplate = 6;
    public const int LeaveApprovalStatusEmailTemplate = 7;
    public const int AttendanceApprovalStatusEmailTemplate = 8;
    public const int TimesheetApprovalStatusEmailTemplate = 9;
    public const int ForgotPasswordEmailTemplate = 10;
    //public const int NewProjectAssignEmailTemplate = 11;
    public const int MigrateApprovalToNewLevel = 11;
    public const int PayrollTemplate = 12;

    public const int ReportingManager = 1;
    public const int SeniorHRManager = 2;

    public class DbProcedure
    {
        public const string Test = "test";
        public const string ParentTest = "parent_test";

        public const string ApprovalChainDetail = "approval_chain_detail";
        public const string ApprovalWorkFlow = "approval_work_flow";
        public const string SalaryGroup = "salary_group";

        public static long getParentKey(long key)
        {
            if (key == 0)
                return -999;
            else
                return key;
        }

        public static string getKey(string table)
        {
            switch (table)
            {
                case DbProcedure.Test:
                    return "Id";
                case DbProcedure.ParentTest:
                    return "ParentId";
                case DbProcedure.ApprovalChainDetail:
                    return "ApprovalChainDetailId";
                case DbProcedure.ApprovalWorkFlow:
                    return "ApprovalWorkFlowId";
                case DbProcedure.SalaryGroup:
                    return "SalaryGroupId";
            }
            return null;
        }
    }


    public const string ApplicationJson = @"application/json";

    public static bool IsExecuted(string Result)
    {
        if (Result != null && (Result.ToLower() == Inserted || Result.ToLower() == Updated || Result.ToLower() == Deleted))
            return true;
        return false;
    }

    public static bool IsValidDataSet(DataSet dataSet)
    {
        if (dataSet == null || dataSet.Tables.Count == 0)
            return false;

        return true;
    }

    public static bool IsValidDataSet(DataSet dataSet, int tableCount)
    {
        if (dataSet == null || dataSet.Tables.Count == 0 || dataSet.Tables.Count != tableCount)
            return false;

        return true;
    }

    public static bool CheckRowsInDataSets(DataSet dataSet, int tableCount)
    {
        bool flag = true;
        if (dataSet == null || dataSet.Tables.Count == 0 || dataSet.Tables.Count != tableCount)
            return false;

        DataTable table = null;
        int i = 0;
        while (i < dataSet.Tables.Count)
        {
            table = dataSet.Tables[i];
            if (table.Rows.Count == 0)
            {
                flag = false;
                break;
            }

            i++;
        }

        return flag;
    }

    public static bool ContainSingleRow(DataSet dataSet)
    {
        bool flag = true;
        if (dataSet == null || dataSet.Tables.Count != 1)
            flag = false;

        if (dataSet.Tables[0].Rows.Count == 0)
            flag = false;

        return flag;
    }

    public static bool IsSingleRow(DataTable table)
    {
        bool flag = true;
        if (table == null || table.Rows.Count != 1)
            flag = false;
        return flag;
    }

    public static bool IsValidDataTable(DataTable table)
    {
        bool flag = true;
        if (table == null || table.Rows.Count == 0)
            flag = false;

        return flag;
    }

    public static bool ContainSingleRow(DataTable table)
    {
        bool flag = false;
        if (table != null && table.Rows.Count == 1)
            flag = true;

        return flag;
    }
}

public class ComponentNames
{
    public const string Gross = "GROSS";
    public const string GrossName = "[GROSS]";
    public const string GrossId = "Gross";
    public const string CTC = "ANNUALCTC";
    public const string CTCId = "CTC";
    public const string StandardDeduction = "STD";
    public const string ProfessionalTax = "PTAX";
    public const string EmployeePF = "EPF";
    public const string EmployerPF = "EPER-PF";
    public const string Special = "SPECIAL ALLOWANCE";
    public const string SpecialAllowanceId = "Special";
    public const string HRA = "HRA";
    public const string Basic = "BS";
    public const string BasicName = "[BASIC]";
    public const string CTCName = "[CTC]";
    public const string ESI = "ESI";
    public const string LabourWelfareFund = "LWF";
    public const string LabourWelfareFundEmployee = "LWFE";
    public const string IncomeTax = "IT";
}

public class FileRoleType
{
    public static string PrimaryLogo = "Company Primary Logo";
    public static string CompanyLogo = "Company Logo";
    public static string OtherFile = "Other File";
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
    NotSubmitted = 0,
    Completed = 1,
    Pending = 2,
    Canceled = 3,
    NotGenerated = 4,
    Rejected = 5,
    Generated = 6,
    Raised = 7,
    Submitted = 8,
    Approved = 9,
    Present = 10,
    Absent = 11,
    MissingAttendanceRequest = 12,
    Saved = 13,
    AutoPromoted = 14,
    FinalLevel = 15
}

public enum LeaveType
{
    SickLeave = 1,
    Compensatory = 2,
    PaidLeave = 3
}

public enum RequestType
{
    Product = 1,
    Service = 2,
    Booking = 3,
    Leave = 5,
    Attendance = 4
}

public enum CommonFlags
{
    FullDay = 1,
    FirstHalf = 2,
    SecondHalf = 3,
    HalfDay = 4
}

public enum EmployeesRole
{
    ProjectManager = 2,
    ProjectArchitect = 3,
    TeamLead = 19,
    TeamMember = 21
}

public enum AdhocType
{
    Allowance = 1,
    Bonus = 2,
    Deduction = 3
}

public static class Bot
{
    public static bool IsSuccess(string Result)
    {
        if (Result != null && (Result.ToLower() == ApplicationConstants.Inserted
            || Result.ToLower() == ApplicationConstants.Updated
            || Result.ToLower() == ApplicationConstants.Deleted))
            return true;
        return false;
    }

    public static bool IsSuccess(DbResult Result)
    {
        if (Result != null && (Result.rowsEffected > 0 || (
                Result.statusMessage == ApplicationConstants.Inserted ||
                Result.statusMessage == ApplicationConstants.Updated ||
                Result.statusMessage == ApplicationConstants.Deleted
            )))
            return true;
        return false;
    }
}