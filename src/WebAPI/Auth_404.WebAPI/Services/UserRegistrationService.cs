using System;
using System.Collections.Generic;
using System.Globalization;
using Auth_404.Model.Requests;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.FluentValidation;
using ServiceStack.Validation;
using ServiceStack.Web;

namespace Auth_404.WebAPI.Services
{
    public class FullUserRegistrationRequestValidator : UserRegistrationRequestValidator
    {
        public FullUserRegistrationRequestValidator()
        {
            RuleSet(ApplyTo.Post, () => RuleFor(x => x.DisplayName).NotEmpty());
        }
    }

    public class UserRegistrationRequestValidator : AbstractValidator<UserRegistrationRequest>
    {
        public IUserAuthRepository UserAuthRepo { get; set; }

        public UserRegistrationRequestValidator()
        {
            RuleSet(
                ApplyTo.Post,
                () =>
                {
                    RuleFor(x => x.Password).NotEmpty();
                    RuleFor(x => x.UserName).NotEmpty().When(x => x.Email.IsNullOrEmpty());
                    RuleFor(x => x.Email).NotEmpty().EmailAddress().When(x => x.UserName.IsNullOrEmpty());
                    RuleFor(x => x.UserName)
                        .Must(x => UserAuthRepo.GetUserAuthByUserName(x) == null)
                        .WithErrorCode("AlreadyExists")
                        .WithMessage("UserName already exists")
                        .When(x => !x.UserName.IsNullOrEmpty());
                    RuleFor(x => x.Email)
                        .Must(x => x.IsNullOrEmpty() || UserAuthRepo.GetUserAuthByUserName(x) == null)
                        .WithErrorCode("AlreadyExists")
                        .WithMessage("Email already exists")
                        .When(x => !x.Email.IsNullOrEmpty());
                });
            RuleSet(ApplyTo.Put, () => RuleFor(x => x.Email).NotEmpty());
        }
    }
    
    public class UserRegistrationService : Service
    {
        public IUserAuthRepository AuthRepo { get; set; }
        public static ValidateFn ValidateFn { get; set; }
        public IValidator<UserRegistrationRequest> UserRegistrationRequestValidator { get; set; }


        /// <summary>
        ///     Create new Registration
        /// </summary>
        public object Post(UserRegistrationRequest request)
        {
            if (HostContext.GlobalRequestFilters == null
                || !HostContext.GlobalRequestFilters.Contains(ValidationFilters.RequestFilter)) //Already gets run
            {
                if (UserRegistrationRequestValidator != null)
                {
                    UserRegistrationRequestValidator.ValidateAndThrow(request, ApplyTo.Post);
                }
            }

            var userAuthRepo = AuthRepo.AsUserAuthRepository(GetResolver());

            if (ValidateFn != null)
            {
                var validateResponse = ValidateFn(this, HttpMethods.Post, request);
                if (validateResponse != null)
                    return validateResponse;
            }
            
            if (string.IsNullOrEmpty(request.DisplayName)) request.DisplayName = request.Email;

            UserRegistrationResponse response = null;
            var session = this.GetSession();
            var newUserAuth = ToUserAuth(request);
            var existingUser = userAuthRepo.GetUserAuth(session, null);

            var registerNewUser = existingUser == null;
            var user = registerNewUser
                ? userAuthRepo.CreateUserAuth(newUserAuth, request.Password)
                : userAuthRepo.UpdateUserAuth(existingUser, newUserAuth, request.Password);

            if (request.AutoLogin.GetValueOrDefault())
            {
                using (var authService = base.ResolveService<AuthenticateService>())
                {
                    var authResponse = authService.Post(
                        new Authenticate
                        {
                            provider = "credentials",
                            UserName = request.UserName ?? request.Email,
                            Password = request.Password,
                            Continue = request.Continue
                        });

                    if (authResponse is IHttpError)
                        throw (Exception) authResponse;

                    var typedResponse = authResponse as AuthenticateResponse;
                    if (typedResponse != null)
                    {
                        response = new UserRegistrationResponse
                        {
                            SessionId = typedResponse.SessionId,
                            DisplayName = typedResponse.DisplayName,
                            ReferrerUrl = typedResponse.ReferrerUrl,
                            UserId = user.Id.ToString(CultureInfo.InvariantCulture),
                        };
                    }
                }
            }

            if (registerNewUser)
            {
                session = this.GetSession();
                session.OnRegistered(this);
            }

            if (response == null)
            {
                response = new UserRegistrationResponse
                {
                    UserId = user.Id.ToString(CultureInfo.InvariantCulture),
                    ReferrerUrl = request.Continue
                };
            }

            var isHtml = Request.ResponseContentType.MatchesContentType(MimeTypes.Html);
            if (isHtml)
            {
                if (string.IsNullOrEmpty(request.Continue))
                    return response;

                return new HttpResult(response)
                {
                    Location = request.Continue
                };
            }

            return response;
        }

        /// <summary>
        /// Update an existing registraiton
        /// </summary>
        [Authenticate]
        public object Put(UserRegistrationRequest request)
        {
            /*
            if (HostContext.GlobalRequestFilters == null
                || !HostContext.GlobalRequestFilters.Contains(ValidationFilters.RequestFilter))
            {
                UserRegistrationRequestValidator.ValidateAndThrow(request, ApplyTo.Put);
            }

            if (ValidateFn != null)
            {
                var response = ValidateFn(this, HttpMethods.Put, request);
                if (response != null)
                    return response;
            }*/

            var userAuthRepo = AuthRepo.AsUserAuthRepository(GetResolver());
            var session = this.GetSession();

            var existingUser = userAuthRepo.GetUserAuth(session, null);
            if (existingUser == null)
                throw HttpError.NotFound("User does not exist");

            var newUserAuth = ToUserAuth(request);

            if (string.IsNullOrEmpty(newUserAuth.Email)) newUserAuth.Email = existingUser.Email;
            if (string.IsNullOrEmpty(newUserAuth.DisplayName)) newUserAuth.DisplayName = newUserAuth.Email;
            
            userAuthRepo.UpdateUserAuth(existingUser, newUserAuth, request.Password);

            return new UserRegistrationResponse
            {
                UserId = existingUser.Id.ToString(CultureInfo.InvariantCulture),
            };
        }

        /// <summary>
        ///     Let's an Admin update a Registration's Email Address
        /// </summary>
        [Authenticate]
        [RequiredRole("Admin")]
        public object Post(UpdateUserRegistrationEmailRequest request)
        {
            var userAuthRepo = AuthRepo.AsUserAuthRepository(GetResolver());
            var existingUser = userAuthRepo.GetUserAuthByUserName(request.OldEmail);
            if (existingUser == null)
            {
                var rs = new ResponseStatus { Message = request.OldEmail + " Not Found", ErrorCode = "404" };
                return new UpdateUserRegistrationEmailResponse { ResponseStatus = rs };
            }

            var newUserAuth = existingUser;
            newUserAuth.DisplayName = request.NewEmail;
            newUserAuth.Email = request.NewEmail;
            
            var updatedUser = userAuthRepo.UpdateUserAuth(existingUser, newUserAuth, null);

            return new UpdateUserRegistrationEmailResponse
            {
                DisplayName = updatedUser.DisplayName,
                UserId = updatedUser.Id.ToString(CultureInfo.InvariantCulture),
                ResponseStatus = new ResponseStatus { Message = "200" }
            };
        }

        /// <summary>
        ///     Let's an Admin update a Registration's Password
        /// </summary>
        [Authenticate]
        [RequiredRole("Admin")]
        public object Post(UpdateUserRegistrationPasswordRequest request)
        {
            var userAuthRepo = AuthRepo.AsUserAuthRepository(GetResolver());
            var existingUser = userAuthRepo.GetUserAuthByUserName(request.Email);
            if (existingUser == null)
            {
                var rs = new ResponseStatus {Message = request.Email + " Not Found", ErrorCode = "404"};
                return new UpdateUserRegistrationPasswordResponse {ResponseStatus = rs};
            }
                
            var newUserAuth = existingUser;
            var updatedUser = userAuthRepo.UpdateUserAuth(existingUser, newUserAuth, request.NewPassword);
            
            return new UpdateUserRegistrationPasswordResponse
            {
                DisplayName = updatedUser.DisplayName,
                UserId = updatedUser.Id.ToString(CultureInfo.InvariantCulture),
                ResponseStatus = new ResponseStatus {Message = "200"}
            };
        }


        
        // Helper Functions
        
        public UserAuth ToUserAuth(UserRegistrationRequest request)
        {
            var to = request.ConvertTo<UserAuth>();
            to.PrimaryEmail = request.Email;
            return to;
        }
    }
}