using System;
using System.Net;

namespace ModalLayer.Modal
{
    [Serializable]
    public class HiringBellException : Exception
    {
        public string UserMessage { set; get; }
        public HttpStatusCode HttpStatusCode { set; get; } = HttpStatusCode.BadRequest;
        public string FieldName { set; get; } = null;
        public string FieldValue { set; get; } = null;

        public HiringBellException() { }
        public HiringBellException(string Message, Exception InnerException) : base(Message, InnerException) { }
        public HiringBellException(string Message, string FieldName = null, string FieldValue = null, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        {
            this.UserMessage = Message;
            this.FieldName = FieldName;
            this.FieldValue = FieldValue;
            HttpStatusCode = httpStatusCode;
        }

        public HiringBellException BuildBadRequest(string Message, string Field = null, string Value = null)
        {
            HttpStatusCode = HttpStatusCode.BadRequest;
            UserMessage = $"{Message} Field: {Field}, Value: {Value}";
            FieldName = Field;
            FieldValue = Value;
            return this;
        }

        public HiringBellException BuildNotFound(string Message, string Filed = null, string Value = null)
        {
            HttpStatusCode = HttpStatusCode.NotFound;
            UserMessage = Message;
            FieldName = Filed;
            FieldValue = Value;
            return this;
        }
    }
}
