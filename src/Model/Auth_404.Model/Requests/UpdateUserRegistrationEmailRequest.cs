using ServiceStack;

namespace Auth_404.Model.Requests
{
    [Route("/UpdateUserRegistrationEmailRequest", "POST")]
    public class UpdateUserRegistrationEmailRequest : IReturn<UpdateUserRegistrationEmailResponse>
    {
        public string OldEmail { get; set; }
        public string NewEmail { get; set; }
    }
}