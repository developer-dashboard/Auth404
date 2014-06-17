using ServiceStack;

namespace Auth_404.Model.Requests
{
    [Route("/UserRegistrationRequest", "POST,PUT")]
    public class UserRegistrationRequest : IReturn<UserRegistrationResponse>
    {
        public bool? AutoLogin { get; set; }
        public string Continue { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
    }
}
