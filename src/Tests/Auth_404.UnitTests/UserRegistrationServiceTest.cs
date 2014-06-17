using Auth_404.Model.Requests;
using Auth_404.WebAPI.Services;
using Moq;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.FluentValidation;
using ServiceStack.Host;
using ServiceStack.Testing;



namespace Auth_404.UnitTests
{
    public class UserRegistrationServiceTest
    {
        static readonly AuthUserSession AuthUserSession = new AuthUserSession();
        private ServiceStackHost _appHost;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _appHost = new BasicAppHost
            {
                ConfigureContainer = c =>
                {
                    var authService = new AuthenticateService();
                    c.Register(authService);
                    c.Register<IAuthSession>(AuthUserSession);
                    AuthenticateService.Init(() => AuthUserSession, new CredentialsAuthProvider());
                }
            }.Init();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            _appHost.Dispose();
        }

        public static IUserAuthRepository GetStubRepo()
        {
            var mock = new Mock<IUserAuthRepository>();
            mock.Setup(x => x.GetUserAuthByUserName(It.IsAny<string>()))
                .Returns((UserAuth)null);
            mock.Setup(x => x.CreateUserAuth(It.IsAny<UserAuth>(), It.IsAny<string>()))
                .Returns(new UserAuth { Id = 1 });

            return mock.Object;
        }

        public static UserRegistrationService GetRegistrationService(
            AbstractValidator<UserRegistrationRequest> validator = null,
            IUserAuthRepository authRepo = null,
            string contentType = null)
        {
            var requestContext = new BasicRequest();
            if (contentType != null)
            {
                requestContext.ResponseContentType = contentType;
            }
            var userAuthRepository = authRepo ?? GetStubRepo();
            var service = new UserRegistrationService
            {
                UserRegistrationRequestValidator = validator ?? new UserRegistrationRequestValidator { UserAuthRepo = userAuthRepository },
                AuthRepo = userAuthRepository,
                Request = requestContext,
            };

            HostContext.Container.Register(userAuthRepository);

            return service;
        }

        [Test]
        public void Empty_Registration_is_invalid()
        {
            var service = GetRegistrationService();

            var response = PostRegistrationError(service, new UserRegistrationRequest());
            var errors = response.GetFieldErrors();

            Assert.That(errors.Count, Is.EqualTo(3));
            Assert.That(errors[0].ErrorCode, Is.EqualTo("NotEmpty"));
            Assert.That(errors[0].FieldName, Is.EqualTo("Password"));
            Assert.That(errors[1].ErrorCode, Is.EqualTo("NotEmpty"));
            Assert.That(errors[1].FieldName, Is.EqualTo("UserName"));
            Assert.That(errors[2].ErrorCode, Is.EqualTo("NotEmpty"));
            Assert.That(errors[2].FieldName, Is.EqualTo("Email"));
        }

        private static HttpError PostRegistrationError(UserRegistrationService service, UserRegistrationRequest register)
        {
            var response = (HttpError)service.RunAction(register, (svc, req) => svc.Post(req));
            return response;
        }

        [Test]
        public void Empty_Registration_is_invalid_with_FullRegistrationValidator()
        {
            var service = GetRegistrationService(new FullUserRegistrationRequestValidator());

            var response = PostRegistrationError(service, new UserRegistrationRequest());
            var errors = response.GetFieldErrors();

            Assert.That(errors.Count, Is.EqualTo(4));
            Assert.That(errors[0].ErrorCode, Is.EqualTo("NotEmpty"));
            Assert.That(errors[0].FieldName, Is.EqualTo("Password"));
            Assert.That(errors[1].ErrorCode, Is.EqualTo("NotEmpty"));
            Assert.That(errors[1].FieldName, Is.EqualTo("UserName"));
            Assert.That(errors[2].ErrorCode, Is.EqualTo("NotEmpty"));
            Assert.That(errors[2].FieldName, Is.EqualTo("Email"));
            Assert.That(errors[3].ErrorCode, Is.EqualTo("NotEmpty"));
            Assert.That(errors[3].FieldName, Is.EqualTo("DisplayName"));
        }

        [Test]
        public void Accepts_valid_registration()
        {
            var service = GetRegistrationService();
            var request = GetValidRegistrationRequest();
            var response = service.Post(request);

            Assert.That(response as UserRegistrationResponse, Is.Not.Null);
        }

        /*
        [Test]
        public void Accepts_valid_registration_with_autologon()
        {
            var service = GetRegistrationService();
            var request = GetValidRegistrationRequest(true);
            var response = service.Post(request);

            Assert.That(response as UserRegistrationResponse, Is.Not.Null);
        }*/

        public static UserRegistrationRequest GetValidRegistrationRequest(bool autoLogin = false)
        {
            var request = new UserRegistrationRequest
            {
                DisplayName = "DisplayName",
                Email = "my@email.com",
                FirstName = "FirstName",
                LastName = "LastName",
                Password = "Password",
                UserName = "UserName",
                AutoLogin = autoLogin,
            };
            return request;
        }

        [Test]
        public void Requires_unique_UserName_and_Email()
        {
            var mockExistingUser = new UserAuth();

            var mock = new Mock<IUserAuthRepository>();
            mock.Setup(x => x.GetUserAuthByUserName(It.IsAny<string>()))
                .Returns(() => mockExistingUser).Verifiable();
            var mockUserAuth = mock.Object;

            var service = new UserRegistrationService
            {
                UserRegistrationRequestValidator = new UserRegistrationRequestValidator { UserAuthRepo = mockUserAuth },
                AuthRepo = mockUserAuth,
            };

            var request = new UserRegistrationRequest
            {
                DisplayName = "DisplayName",
                Email = "my@email.com",
                FirstName = "FirstName",
                LastName = "LastName",
                Password = "Password",
                UserName = "UserName",
            };

            var response = PostRegistrationError(service, request);
            var errors = response.GetFieldErrors();

            Assert.That(errors.Count, Is.EqualTo(2));
            Assert.That(errors[0].ErrorCode, Is.EqualTo("AlreadyExists"));
            Assert.That(errors[0].FieldName, Is.EqualTo("UserName"));
            Assert.That(errors[1].ErrorCode, Is.EqualTo("AlreadyExists"));
            Assert.That(errors[1].FieldName, Is.EqualTo("Email"));

            mock.Verify();
        }

        [Test]
        public void Registration_with_Html_ContentType_And_Continue_returns_302_with_Location()
        {
            var service = GetRegistrationService(null, null, MimeTypes.Html);

            var request = GetValidRegistrationRequest();
            request.Continue = "http://localhost/home";

            var response = service.Post(request) as HttpResult;

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Status);
            Assert.That(response.Status, Is.EqualTo(302));
            Assert.That(response.Headers[HttpHeaders.Location], Is.EqualTo("http://localhost/home"));
        }

        [Test]
        public void Registration_with_EmptyString_Continue_returns_RegistrationResponse()
        {
            var service = GetRegistrationService(null, null, MimeTypes.Html);

            var request = GetValidRegistrationRequest();
            request.Continue = string.Empty;

            var response = service.Post(request);

            Assert.That(response as HttpResult, Is.Null);
            Assert.That(response as UserRegistrationResponse, Is.Not.Null);
        }

        [Test]
        public void Registration_with_Json_ContentType_And_Continue_returns_RegistrationResponse_with_ReferrerUrl()
        {
            var service = GetRegistrationService(null, null, MimeTypes.Json);

            var request = GetValidRegistrationRequest();
            request.Continue = "http://localhost/home";

            var response = service.Post(request) as UserRegistrationResponse;
            Assert.IsNotNull(response);
            Assert.AreEqual("http://localhost/home", response.ReferrerUrl);
        }
    }
}
