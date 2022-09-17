using System.Collections.Generic;

namespace ModalLayer.Modal
{
    public class EmailSenderModal
    {
        public List<string> To { set; get; }
        public string From { set; get; }
        public string Body { set; get; }
        public string UserName { set; get; }
        public string Title { set; get; }
        public string Subject { set; get; }
        public List<string> CC { set; get; }
        public List<string> BCC { set; get; }
        public List<FileDetail> FileDetails { set; get; }
    }
}
