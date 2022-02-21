using ModalLayer.Modal;
using Newtonsoft.Json;

namespace DocMaker.PdfService
{
    public class PdfGenerateHelper
    {
        public BuildPdfTable MapUserDetail(BuildPdfTable _buildPdfTable, PdfModal pdfModal)
        {
            string strScript = JsonConvert.SerializeObject(_buildPdfTable);
            strScript = strScript.Replace("{{developer_name}}", pdfModal.developerName.ToUpper())
                                .Replace("{{month}}", pdfModal.billingMonth.ToString("MMMM"))
                                .Replace("{{package_amount}}", pdfModal.packageAmount.ToString())
                                .Replace("{{package_amount_total}}", pdfModal.packageAmount.ToString())
                                .Replace("{{cgst_percent}}", pdfModal.cGST.ToString())
                                .Replace("{{cgst_amount}}", pdfModal.cGstAmount.ToString())
                                .Replace("{{sgst_percent}}", pdfModal.sGST.ToString())
                                .Replace("{{sgst_amount}}", pdfModal.sGstAmount.ToString())
                                .Replace("{{igst_percent}}", pdfModal.iGST.ToString())
                                .Replace("{{igst_amount}}", pdfModal.iGstAmount.ToString())
                                .Replace("{{sub_total}}", pdfModal.packageAmount.ToString())
                                .Replace("{{grand_total}}", pdfModal.grandTotalAmount.ToString())
                                .Replace("{{receiverCompanyName}}", pdfModal.receiverCompanyName.ToString())
                                .Replace("{{receiverGstInNo}}", pdfModal.receiverGSTNo.ToString())
                                .Replace("{{receiverFirstAddress}}", pdfModal.receiverFirstAddress.ToString())
                                .Replace("{{receiverSecondAddress}}", pdfModal.receiverSecondAddress.ToString())
                                .Replace("{{receiverThirdAddress}}", pdfModal.receiverThirdAddress.ToString())
                                .Replace("{{receiverPrimaryContactNo}}", pdfModal.receiverPrimaryContactNo.ToString())
                                .Replace("{{receiverEmailId}}", pdfModal.receiverEmail == null ? "" : pdfModal.receiverEmail)
                                .Replace("{{senderCompanyName}}", pdfModal.senderCompanyName.ToString())
                                .Replace("{{senderGstInNo}}", pdfModal.senderGSTNo.ToString())
                                .Replace("{{senderFirstAddress}}", pdfModal.senderFirstAddress.ToString())
                                .Replace("{{senderSecondAddress}}", pdfModal.senderSecondAddress.ToString())
                                .Replace("{{senderPrimaryContactNo}}", pdfModal.senderPrimaryContactNo.ToString())                                                               
                                .Replace("{{senderEmailId}}", pdfModal.senderEmail.ToString())
                                .Replace("{{billNo}}", pdfModal.billNo.ToString())
                                .Replace("{{dateOfBilling}}", pdfModal.dateOfBilling.ToString("dd MMM yyyy"))
                                ;

            _buildPdfTable = JsonConvert.DeserializeObject<BuildPdfTable>(strScript);
            return _buildPdfTable;
        }
    }
}
