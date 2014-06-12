using ServiceStack;

namespace Auth_404.Model.Requests
{
    
    public class GetUserAuthRequest : IReturn<GetUserAuthResponse>
    {
        public string UserAuthId { get; set; }
    }
}
