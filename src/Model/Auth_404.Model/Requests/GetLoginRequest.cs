using ServiceStack;

namespace Auth_404.Model.Requests
{
    
    [Route("/login/{Email}")]
    public class GetLoginRequest : IReturn<GetLoginResponse>
    {
        public string Email { get; set; }
    }
}
