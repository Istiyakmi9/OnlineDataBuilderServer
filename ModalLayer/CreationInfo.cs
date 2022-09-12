using System;

namespace ModalLayer
{
    public class CreationInfo
    {
        public long CreatedBy { set; get; }
        public long? UpdatedBy { set; get; }
        public DateTime CreatedOn { set; get; }
        public DateTime? UpdatedOn { set; get; }
        public long AdminId { get; set; }
    }
}