using Auth_404.Model.Data;
using ServiceStack;

namespace Auth_404.Model.Requests
{
    [Route("/CreateLogin", "POST")]
    public class CreateLoginRequest : IReturn<CreateLoginResponse>
    {
        public Login LoginAccount { get; set; }
        public string Password { get; set; }
    }
}
