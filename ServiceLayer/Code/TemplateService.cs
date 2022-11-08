using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System.Collections.Generic;

namespace ServiceLayer.Code
{
    public class TemplateService: ITemplateService
    {
        private readonly IDb _db;
        public TemplateService(IDb db)
        {
            _db = db;
        }

        public EmailTemplate GetBillingTemplateDetailService()
        {
            var detail = _db.Get<EmailTemplate>("sp_email_template_get", new { EmailTemplateId = 1 });
            if (!string.IsNullOrEmpty(detail.BodyContent))
                detail.BodyContent = JsonConvert.DeserializeObject<string>(detail.BodyContent);

            return detail;
        }
    }
}
