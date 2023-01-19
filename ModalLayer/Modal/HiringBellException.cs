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
        public string StackTraceDetail { set; get; }

        public HiringBellException() { }
        public HiringBellException(string Message, Exception InnerException) : base(Message, InnerException)
        {
            this.UserMessage = Message;
            this.StackTraceDetail = InnerException != null ? InnerException.StackTrace : "";
            HttpStatusCode = HttpStatusCode.BadRequest;
        }

        public HiringBellException(string Message, HttpStatusCode httpStatusCode)
        {
            this.UserMessage = Message;
            HttpStatusCode = httpStatusCode;
        }

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

        public static HiringBellException ThrowBadRequest(string Message, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        {
            return new HiringBellException(Message, httpStatusCode);
        }
    }
}
