using ServiceStack;

namespace Auth_404.Model.Requests
{
    public class UpdateUserRegistrationPasswordResponse
    {
        public ResponseStatus ResponseStatus { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
    }
}