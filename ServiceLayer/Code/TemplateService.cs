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
                detail.BodyContentDetail = JsonConvert.DeserializeObject<BodyContentDetail>(detail.BodyContent);

            return detail;
        }
        public string GetStaffingTemplateService()
        {
            string htmlBody = @"<!DOCTYPE html>            
                    <html style='background: white;'>
                        <head>
                            <title>STAFFING BILL</title> 
                         </head> 
                         <body>
                            <h4>Hi Sir/Madam, </h4> 
                            <p>PFA bill for the month of July.</p> 
                            <p>Developer detail as follows:</p>
                            <div style='margin-left:10px;'>1. FAHIM SHAIKH  [ROLE: SOFTWARE DEVELOPER]</div> 
                            <div style='margin-left:10px; padding:15px 0px;'>2. VANHAR BASHA  [ROLE: SOFTWARE DEVELOPER]</div> 
                            
                            <p style='margin-top: 2rem;'>Thanks & Regards,</p>
                            <div>Team BottomHalf</div>
                            <div>Mob: +91-9100544384</div>
                        </body> 
                    </html>";

            return htmlBody;
        }
    }
}
