using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System.Collections.Generic;

namespace ServiceLayer.Code
{
    public class CompanyNotificationService : ICompanyNotificationService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;

        public CompanyNotificationService(IDb db, CurrentSession currentSession)
        {
            _db = db;
            _currentSession = currentSession;
        }

        public List<CompanyNotification> GetNotificationRecordService(FilterModel filterModel)
        {
            var result = _db.GetList<CompanyNotification>("SP_company_notification_getby_filter", new
            {
                filterModel.SearchString,
                filterModel.PageIndex,
                filterModel.PageSize,
                filterModel.SortBy
            });
            return result;
        }

        public List<CompanyNotification> InsertUpdateNotificationService(CompanyNotification notification)
        {
            ValidateCompanyNotification(notification);
            var oldNotification = _db.Get<CompanyNotification>("SP_company_notification_getby_id", new { NotificationId = notification.NotificationId });
            if (oldNotification == null)
                oldNotification = notification;
            else
            {
                oldNotification.Topic = notification.Topic;
                oldNotification.BriefDetail = notification.BriefDetail;
                oldNotification.DepartmentId = notification.DepartmentId;
            }
            oldNotification.CompleteDetail = JsonConvert.SerializeObject(notification.CompleteDetail);
            oldNotification.AdminId = _currentSession.CurrentUserDetail.UserId;
            var result = _db.Execute<CompanyNotification>("sp_company_notification_insupd", oldNotification, true);
            if (string.IsNullOrEmpty(result))
                throw HiringBellException.ThrowBadRequest("Fail to insert or update company notification");

            FilterModel filterModel = new FilterModel
            {
                SearchString = $"1=1 and CompanyId={notification.CompanyId}"
            };
            return this.GetNotificationRecordService(filterModel);
        }

        private void ValidateCompanyNotification(CompanyNotification notification)
        {
            if (string.IsNullOrEmpty(notification.CompleteDetail))
                throw HiringBellException.ThrowBadRequest("Complete detail is null or empty");

            if (string.IsNullOrEmpty(notification.BriefDetail))
                throw HiringBellException.ThrowBadRequest("Brief detail is null or empty");

            if (string.IsNullOrEmpty(notification.Topic))
                throw HiringBellException.ThrowBadRequest("Topic is null or empty");

            if (notification.CompanyId <= 0)
                throw HiringBellException.ThrowBadRequest("Invalid company id");

            if (notification.DepartmentId <= 0)
                throw HiringBellException.ThrowBadRequest("Invalid department id");
        }
    }
}
