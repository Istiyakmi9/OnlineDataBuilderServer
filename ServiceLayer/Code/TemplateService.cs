using BottomhalfCore.DatabaseLayer.Common.Code;
using Microsoft.AspNetCore.Hosting;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.IO;

namespace ServiceLayer.Code
{
    public class TemplateService : ITemplateService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly FileLocationDetail _fileLocationDetail;
        private readonly IHostingEnvironment _hostingEnvironment;

        public TemplateService(IDb db, CurrentSession currentSession, FileLocationDetail fileLocationDetail, IHostingEnvironment hostingEnvironment)
        {
            _db = db;
            _currentSession = currentSession;
            _fileLocationDetail = fileLocationDetail;
            _hostingEnvironment = hostingEnvironment;
        }

        public AnnexureOfferLetter AnnexureOfferLetterInsertUpdateService(AnnexureOfferLetter annexureOfferLetter, int LetterType)
        {
            if (annexureOfferLetter.CompanyId <= 0)
                throw new HiringBellException("Invalid company selected. Please select a valid company");

            if (string.IsNullOrEmpty(annexureOfferLetter.TemplateName))
                throw new HiringBellException("Template name is null or empty");

            if (string.IsNullOrEmpty(annexureOfferLetter.BodyContent))
                throw new HiringBellException("Body content is null or empty");

            AnnexureOfferLetter letter = _db.Get<AnnexureOfferLetter>("sp_annexure_offer_letter_getby_lettertype", new { CompanyId = annexureOfferLetter.CompanyId, LetterType });
            if (letter == null)
                letter = annexureOfferLetter;
            else
            {
                letter.TemplateName = annexureOfferLetter.TemplateName;
                letter.FileId = annexureOfferLetter.FileId;
            }

            var folderPath = Path.Combine(_fileLocationDetail.DocumentFolder, _fileLocationDetail.CompanyFiles, "Offer_Letter");
            if (!Directory.Exists(Path.Combine(_hostingEnvironment.ContentRootPath, folderPath)))
                Directory.CreateDirectory(Path.Combine(_hostingEnvironment.ContentRootPath, folderPath));
            string filename = annexureOfferLetter.TemplateName.Replace(" ", "") + ".txt";
            var filepath = Path.Combine(folderPath, filename);
            if (File.Exists(filepath))
                File.Delete(filepath);

            var txt = new StreamWriter(filepath);
            txt.Write(annexureOfferLetter.BodyContent);
            txt.Close();

            letter.AdminId = _currentSession.CurrentUserDetail.UserId;
            letter.LetterType = LetterType;
            letter.FilePath = filepath;
            var result = _db.Execute<AnnexureOfferLetter>("sp_annexure_offer_letter_insupd", letter, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("fail to insert or update");
            var letterId = Convert.ToInt32(result);
            annexureOfferLetter.AnnexureOfferLetterId = letterId;
            return annexureOfferLetter;
        }

        public AnnexureOfferLetter GetOfferLetterService(int CompanyId, int LetterType)
        {
            var result = _db.Get<AnnexureOfferLetter>("sp_annexure_offer_letter_getby_lettertype", new { CompanyId, LetterType });
            if (result == null)
                throw new HiringBellException("Unable to find Offer letter data. Please contact to admin.");

            if (File.Exists(result.FilePath))
            {
                var txt = File.ReadAllText(result.FilePath);
                result.BodyContent = txt;
            }
            return result;
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
