using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ITemplateService
    {
        EmailTemplate GetBillingTemplateDetailService();
        AnnexureOfferLetter AnnexureOfferLetterInsertUpdateService(AnnexureOfferLetter annexureOfferLetter, int LetterType);
        AnnexureOfferLetter GetOfferLetterService(int CompanyId, int LetterType);
    }
}
