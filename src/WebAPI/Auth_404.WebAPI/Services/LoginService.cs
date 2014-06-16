/*
using System;
using Auth_404.Model.Data;
using Auth_404.Model.Requests;
using CommonServiceUtilities;
using ServiceStack;
using ServiceStack.Auth;

namespace Auth_404.WebAPI.Services
{
    public class LoginService : Service
    {
        public IUserAuthRepository UserRepository { get; set; }
        public Logger Logger = new Logger(typeof (LoginService).Name);

        [Authenticate]
        public object Get(GetLoginRequest request)
        {
            try
            {
                var session = GetSession();

                //If the User isn't an Admin they can only get information about themselves
                if (!session.HasRole("Admin"))
                    request.Email = session.UserAuthName;

                var user = UserRepository.GetUserAuthByUserName(request.Email);
                return new Login {Email = user.Email, FirstName = user.FirstName, LastName = user.LastName, DisplayName = user.DisplayName};
            }
            catch (Exception error)
            {
                Logger.LogError(error);
                var errorCode = error is ArgumentException ? "400" : "500";
                return new CreateLoginResponse { ErrorMessage = error.Message, StackTrace = error.StackTrace, IsSuccess = false, ErrorCode = errorCode };
            }
        }
        
        [Authenticate]
        [RequiredRole("Admin")]
        public object Post(CreateLoginRequest request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentException("Null Request");

                if (string.IsNullOrEmpty(request.Password))
                    throw new ArgumentException("Invalid Password");

                string hash;
                string salt;
                new SaltedHash().GetHashAndSaltString(request.Password, out hash, out salt);

                var user = new UserAuth
                {
                    Email = request.LoginAccount.Email,
                    Salt = salt,
                    PasswordHash = hash,
                    DisplayName = request.LoginAccount.DisplayName,
                    FirstName = request.LoginAccount.FirstName,
                    LastName = request.LoginAccount.LastName
                };

                var result = UserRepository.CreateUserAuth(user, request.Password);

                if (result != null && result.Email == request.LoginAccount.Email)
                    return new CreateLoginResponse {IsSuccess = true};

                throw new ApplicationException("Problem creating new login");
            }
            catch (Exception error)
            {
                Logger.LogError(error);
                var errorCode = error is ArgumentException ? "400" : "500";
                return new CreateLoginResponse {ErrorMessage = error.Message, StackTrace = error.StackTrace, IsSuccess = false, ErrorCode = errorCode};
            }
        }
    }
}*/