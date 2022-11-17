using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class CommonService : ICommonService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly ITimezoneConverter _timezoneConverter;

        public CommonService(IDb db, ITimezoneConverter timezoneConverter, CurrentSession currentSession)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
        }

        public List<Employee> LoadEmployeeData()
        {
            FilterModel filterModel = new FilterModel();
            List<Employee> employeeTable = _db.GetList<Employee>("SP_Employees_Get", new
            {
                filterModel.SearchString,
                filterModel.SortBy,
                filterModel.PageIndex,
                filterModel.PageSize
            });

            return employeeTable;
        }

        public EmailTemplate GetTemplate(int EmailTemplateId)
        {
            (EmailTemplate emailTemplate, EmailSettingDetail emailSetting) = 
                _db.GetMulti<EmailTemplate, EmailSettingDetail>("sp_email_template_by_id", new { EmailTemplateId });

            if (emailSetting == null)
                throw new HiringBellException("Email setting detail not found. Please contact to admin.");

            if (emailTemplate == null)
                throw new HiringBellException("Email template not found. Please contact to admin.");

            return emailTemplate;
        }

        public bool IsEmptyJson(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                if (json == "" || json == "{}" || json == "[]")
                    return true;
            }
            else
                return true;

            return false;
        }

        public async Task<EmailSenderModal> ReplaceActualData(TemplateReplaceModal templateReplaceModal, EmailTemplate template)
        {
            EmailSenderModal emailSenderModal = null;
            var fromDate = _timezoneConverter.ToTimeZoneDateTime(templateReplaceModal.FromDate, _currentSession.TimeZone);
            var toDate = _timezoneConverter.ToTimeZoneDateTime(templateReplaceModal.ToDate, _currentSession.TimeZone);
            if (templateReplaceModal != null)
            {
                var totalDays = templateReplaceModal.ToDate.Date.Subtract(templateReplaceModal.FromDate.Date).TotalDays + 1;
                string subject = templateReplaceModal.Subject
                                 .Replace("[[REQUEST-TYPE]]", templateReplaceModal.RequestType)
                                 .Replace("[[ACTION-TYPE]]", templateReplaceModal.ActionType);

                string body = JsonConvert.DeserializeObject<string>(templateReplaceModal.BodyContent)
                                .Replace("[[DEVELOPER-NAME]]", templateReplaceModal.DeveloperName)
                                .Replace("[[DAYS-COUNT]]", $"{totalDays}")
                                .Replace("[[REQUEST-TYPE]]", templateReplaceModal.RequestType)
                                .Replace("[[TO-DATE]]", fromDate.ToString("dd MMM, yyyy"))
                                .Replace("[[FROM-DATE]]", toDate.ToString("dd MMM, yyyy"))
                                .Replace("[[ACTION-TYPE]]", templateReplaceModal.ActionType)
                                .Replace("[[MANAGER-NAME]]", templateReplaceModal.ManagerName)
                                .Replace("[[USER-MESSAGE]]", templateReplaceModal.Message);

                StringBuilder builder = new StringBuilder();
                builder.Append("<div style=\"border-bottom:1px solid black; margin-top: 14px; margin-bottom:5px\">" + "" + "</div>");
                builder.AppendLine();
                builder.AppendLine();
                builder.Append("<div>" + template.EmailClosingStatement + "</div>");
                builder.Append("<div>" + template.SignatureDetail + "</div>");
                builder.Append("<div>" + template.ContactNo + "</div>");
                emailSenderModal = new EmailSenderModal
                {
                    To = templateReplaceModal.ToAddress,
                    Subject = subject,
                    Body = string.Concat(body, builder.ToString()),
                };
            }

            emailSenderModal.Title = templateReplaceModal.Title;

            return await Task.FromResult(emailSenderModal);
        }
    }
}
