using System.Collections.Generic;
using ServiceStack;

namespace Auth_404.Model.Requests 
{
    public class UserRegistrationResponse : IMeta
    {
        public Dictionary<string, string> Meta { get; set; }
        public string ReferrerUrl { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
    }
}
