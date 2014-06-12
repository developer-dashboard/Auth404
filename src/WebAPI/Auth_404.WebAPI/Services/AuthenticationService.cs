using Auth_404.Model.Requests;
using ServiceStack;
using ServiceStack.Auth;

namespace Auth_404.WebAPI.Services
{
    /*
    public class AuthenticationService : Service
    {
        public IUserAuthRepository UserRepo { get; set; } 

        [Authenticate]
        public object Get(GetUserAuthRequest request)
        {
            var session = GetSession();

            if (!session.HasRole("Admin"))
                request.UserAuthId = session.Id;

            var userInfo = UserRepo.GetUserAuth(request.UserAuthId) as UserAuth;
            return new GetUserAuthResponse {UserAuth = userInfo};
        }




        [Authenticate]
        [RequiredRole("Admin")]
        public object Post(UserAuth request)
        {
            /*
            request.p
            string hash;
            string salt;
            new SaltedHash().GetHashAndSaltString(password, out hash, out salt);
            user.Salt = salt;
            user.PasswordHash = hash;
            UserRepo.CreateUserAuth(user, password);
            UserRepo.UpdateUserAuth()
            return request;
        }
    }*/
}