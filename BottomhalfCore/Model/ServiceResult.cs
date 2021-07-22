using System.Collections.Generic;

namespace BottomhalfCore.BottomhalfModel
{
    public class ServiceResult
    {
        public bool IsValidModal { set; get; }
        public string ErrorMessage { set; get; }
        public string SuccessMessage { set; get; }
        public int StatusCode { set; get; }
        public IList<string> ErrorResultedList { set; get; }
    }
}
