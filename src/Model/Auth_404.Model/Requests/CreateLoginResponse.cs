
using ServiceStack;

namespace Auth_404.Model.Requests
{
    public class CreateLoginResponse : IResponseStatus
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public bool IsSuccess { get;  set; }
    }
}
