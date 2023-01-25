using ModalLayer.Modal;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface ICompanyNotificationService
    {
        List<CompanyNotification> InsertUpdateNotificationService(CompanyNotification notification);
        List<CompanyNotification> GetNotificationRecordService(FilterModel filterModel);
    }
}
