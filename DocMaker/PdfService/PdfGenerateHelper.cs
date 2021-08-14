using ModalLayer.Modal;
using Newtonsoft.Json;

namespace DocMaker.PdfService
{
    public class PdfGenerateHelper
    {
        public BuildPdfTable MapUserDetail(BuildPdfTable _buildPdfTable, PdfModal pdfModal)
        {
            string strScript = JsonConvert.SerializeObject(_buildPdfTable);
            strScript = strScript.Replace("{{developer_name}}", pdfModal.developerName);
            strScript = strScript.Replace("{{month}}", pdfModal.billForMonth);
            strScript = strScript.Replace("{{package_amount}}", pdfModal.packageAmount.ToString());
            strScript = strScript.Replace("{{package_amount_total}}", pdfModal.packageAmount.ToString());
            strScript = strScript.Replace("{{cgst_percent}}", pdfModal.cGST.ToString());
            strScript = strScript.Replace("{{cgst_amount}}", pdfModal.cGstAmount.ToString());
            strScript = strScript.Replace("{{sgst_percent}}", pdfModal.sGST.ToString());
            strScript = strScript.Replace("{{sgst_amount}}", pdfModal.sGstAmount.ToString());
            strScript = strScript.Replace("{{igst_percent}}", pdfModal.iGST.ToString());
            strScript = strScript.Replace("{{igst_amount}}", pdfModal.iGstAmount.ToString());
            strScript = strScript.Replace("{{sub_total}}", pdfModal.packageAmount.ToString());
            strScript = strScript.Replace("{{grand_total}}", pdfModal.grandTotalAmount.ToString());

            _buildPdfTable = JsonConvert.DeserializeObject<BuildPdfTable>(strScript);
            return _buildPdfTable;
        }
    }
}
