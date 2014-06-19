using System.Collections.Generic;
using System.Configuration;
using Auth_404.DatabaseSetup;
using Auth_404.Model.Constants;
using Auth_404.Model.Data;
using Auth_404.Model.Operations;
using Auth_404.Model.Requests;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Auth;

namespace Auth404.IntegrationTests
{
    [TestFixture]
    public class UserRegistrationTests
    {
        public IRestClient RestClient;
        public string WebServiceHostUrl;

        [TestFixtureSetUp]
        public void set_up()
        {
           
            WebServiceHostUrl = ConfigurationManager.AppSettings["ServerUrl"];
            RestClient = new JsonServiceClient(WebServiceHostUrl);

            DataBaseHelper.Setup_Test_Database();
        }

        [TestFixtureTearDown]
        public void close_down()
        {
            Logout();
        }



        public void Logout()
        {
            // ReSharper disable RedundantTypeArgumentsOfMethod
            //logout
            RestClient.Post(new Authenticate {provider = "logout"});

            // Check to see if we are logged out.
            var error = Assert.Throws<WebServiceException>(() => RestClient.Post<AuthenticateResponse>(new Authenticate()));
            Assert.AreEqual("Not Authenticated", error.Message);
            // ReSharper restore RedundantTypeArgumentsOfMethod
        }

        [Test]
        public void can_register_a_new_user_and_log_them_on()
        {
            // ReSharper disable RedundantTypeArgumentsOfMethod
            var createRequest = new UserRegistrationRequest
            {
                Email = "user1@gmail.com",
                Password = "user1",
                AutoLogin = true
            };

            var createResponse = RestClient.Post<UserRegistrationResponse>(createRequest);
            Assert.IsNotNull(createResponse);
            Assert.IsTrue(createResponse.UserId.Length > 0);

            var checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate());
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual(createRequest.Email, checkLoginStatus.UserName);

            //logout
            Logout();
            // ReSharper restore RedundantTypeArgumentsOfMethod
        }

        [Test]
        public void test_login_logout_using_credential_auth()
        {
            // ReSharper disable RedundantTypeArgumentsOfMethod
            var createRequest = new UserRegistrationRequest
            {
                Email = "user2@gmail.com",
                Password = "user2",
                AutoLogin = false
            };

            var createResponse = RestClient.Post<UserRegistrationResponse>(createRequest);
            Assert.IsNotNull(createResponse);
            Assert.IsTrue(createResponse.UserId.Length > 0);

            //logout - in case we had an open session
            RestClient.Post(new Authenticate {provider = "logout"});
            var error = Assert.Throws<WebServiceException>(() => RestClient.Post<AuthenticateResponse>(new Authenticate()));
            Assert.AreEqual("Not Authenticated", error.Message);


            // login the user in
            var checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate {provider = "credentials", UserName = "user2@gmail.com", Password = "user2"});
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual("user2@gmail.com", checkLoginStatus.UserName);

            // we should stay logged in
            checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate());
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual("user2@gmail.com", checkLoginStatus.UserName);

            //logout
            Logout();
            // ReSharper restore RedundantTypeArgumentsOfMethod
        }

        [Test]
        public void test_login_logout_using_basic_auth()
        {
            // ReSharper disable RedundantTypeArgumentsOfMethod
            var createRequest = new UserRegistrationRequest
            {
                Email = "user3@gmail.com",
                Password = "user3",
                AutoLogin = false
            };

            var createResponse = RestClient.Post<UserRegistrationResponse>(createRequest);
            Assert.IsNotNull(createResponse);
            Assert.IsTrue(createResponse.UserId.Length > 0);

            //logout - in case we had an open session
            Logout();

            // login the user in by making a request with a Basic Auth Header
            var client = new JsonServiceClient {AlwaysSendBasicAuthHeader = true, BaseUri = WebServiceHostUrl, UserName = "user3@gmail.com", Password = "user3"};
            var response = client.Get<List<Transaction>>(new GetTransactions());
            Assert.IsNotNull(response);


            var checkLoginStatus = client.Post<AuthenticateResponse>(new Authenticate());
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual("user3@gmail.com", checkLoginStatus.UserName);

            //logout
            Logout();
            // ReSharper restore RedundantTypeArgumentsOfMethod
        }

        [Test]
        public void user_can_update_their_password()
        {
            // ReSharper disable RedundantTypeArgumentsOfMethod
            var createRequest = new UserRegistrationRequest
            {
                Email = "user4@gmail.com",
                Password = "user4",
                AutoLogin = true
            };

            var createResponse = RestClient.Post<UserRegistrationResponse>(createRequest);
            Assert.IsNotNull(createResponse);
            Assert.IsTrue(createResponse.UserId.Length > 0);

            var checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate());
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual(createRequest.Email, checkLoginStatus.UserName);

            //update the password
            var updateRequest = new UserRegistrationRequest {Password = "UPDATED"};
            var updateResponse = RestClient.Put<UserRegistrationResponse>(updateRequest);
            Assert.IsNotNull(updateResponse);
            Assert.IsTrue(updateResponse.UserId.Length > 0);

            //logout
            Logout();

            //logon with the new password
            checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate {provider = "credentials", UserName = "user4@gmail.com", Password = "UPDATED"});
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual("user4@gmail.com", checkLoginStatus.UserName);

            //logout
            Logout();
            // ReSharper restore RedundantTypeArgumentsOfMethod-
        }

        [Test]
        public void user_can_update_their_email()
        {
            // ReSharper disable RedundantTypeArgumentsOfMethod
            var createRequest = new UserRegistrationRequest
            {
                Email = "user8@gmail.com",
                Password = "user8",
                AutoLogin = true
            };

            var createResponse = RestClient.Post<UserRegistrationResponse>(createRequest);
            Assert.IsNotNull(createResponse);
            Assert.IsTrue(createResponse.UserId.Length > 0);

            var checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate());
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual(createRequest.Email, checkLoginStatus.UserName);

            //update the password
            var updateRequest = new UserRegistrationRequest { Password = "user8", Email = "updated@hotmail.com"};
            var updateResponse = RestClient.Put<UserRegistrationResponse>(updateRequest);
            Assert.IsNotNull(updateResponse);
            Assert.IsTrue(updateResponse.UserId.Length > 0);

            //logout
            Logout();

            //logon with the new email
            checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate { provider = "credentials", UserName = "updated@hotmail.com", Password = "user8" });
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual("updated@hotmail.com", checkLoginStatus.UserName);

            //logout
            Logout();
            // ReSharper restore RedundantTypeArgumentsOfMethod-
        }

        [Test]
        public void user_can_not_update_if_not_authenticated()
        {
            // ReSharper disable RedundantTypeArgumentsOfMethod
            var createRequest = new UserRegistrationRequest
            {
                Email = "user5@gmail.com",
                Password = "user5",
                AutoLogin = true
            };

            var createResponse = RestClient.Post<UserRegistrationResponse>(createRequest);
            Assert.IsNotNull(createResponse);
            Assert.IsTrue(createResponse.UserId.Length > 0);


            var checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate());
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual(createRequest.Email, checkLoginStatus.UserName);

            //logout
            Logout();

            // try and fail update the password
            var updateRequest = new UserRegistrationRequest {Email = "user5@gmail.com", Password = "UPDATED"};
            var error = Assert.Throws<WebServiceException>(() => RestClient.Put<UserRegistrationResponse>(updateRequest));
            Assert.AreEqual("Unauthorized", error.Message);
            // ReSharper restore RedundantTypeArgumentsOfMethod
        }
        
        [Test]
        public void user_an_admin_update_registration_password()
        {
            // ReSharper disable RedundantTypeArgumentsOfMethod
            var createRequest = new UserRegistrationRequest
            {
                Email = "user6@gmail.com",
                Password = "user6",
                AutoLogin = true
            };

            var createResponse = RestClient.Post<UserRegistrationResponse>(createRequest);
            Assert.IsNotNull(createResponse);
            Assert.IsTrue(createResponse.UserId.Length > 0);

            //login as admin
            var checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate { provider = "credentials", UserName = DefaultAdmin.Email, Password = DefaultAdmin.Password });
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual(DefaultAdmin.Email, checkLoginStatus.UserName);

            //update password
            var updateResponse = RestClient.Post<UpdateUserRegistrationPasswordResponse>(new UpdateUserRegistrationPasswordRequest{Email = "user6@gmail.com", NewPassword = "UPDATED"});
            Assert.IsNotNull(updateResponse);
            Assert.IsTrue(updateResponse.UserId.Length > 0);

            //logoff
            Logout();

            // login as the updated user
            checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate { provider = "credentials", UserName = "user6@gmail.com", Password = "UPDATED"});
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual("user6@gmail.com", checkLoginStatus.UserName);

            //logoff
            Logout();
           
            // ReSharper restore RedundantTypeArgumentsOfMethod-
        }

        [Test]
        public void non_admins_can_not_update_other_passwords()
        {
            // ReSharper disable RedundantTypeArgumentsOfMethod
            var createRequest = new UserRegistrationRequest
            {
                Email = "user7@gmail.com",
                Password = "user7",
                AutoLogin = true
            };

            var createResponse = RestClient.Post<UserRegistrationResponse>(createRequest);
            Assert.IsNotNull(createResponse);
            Assert.IsTrue(createResponse.UserId.Length > 0);


            var checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate());
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual(createRequest.Email, checkLoginStatus.UserName);

            //login as a non-admin
            checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate { provider = "credentials", UserName = TestUser.Email, Password = TestUser.Password });
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual(TestUser.Email, checkLoginStatus.UserName);
            

           
            // try and fail update the password
            var updateRequest = new UpdateUserRegistrationPasswordRequest { Email = "user7@gmail.com", NewPassword = "UPDATED" };
            var error = Assert.Throws<WebServiceException>(() => RestClient.Post<UpdateUserRegistrationPasswordResponse>(updateRequest));
            Assert.AreEqual("Invalid Role", error.Message);

            //logout
            Logout();
            // ReSharper restore RedundantTypeArgumentsOfMethod
        }

        [Test]
        public void can_assign_roles()
        {
            var createRequest = new UserRegistrationRequest
            {
                Email = "user8@gmail.com",
                Password = "user8",
                AutoLogin = false
            };

            var createResponse = RestClient.Post<UserRegistrationResponse>(createRequest);
            Assert.IsNotNull(createResponse);
            Assert.IsTrue(createResponse.UserId.Length > 0);

            //login an admin
            var checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate { provider = "credentials", UserName = DefaultAdmin.Email, Password = DefaultAdmin.Password });
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual(DefaultAdmin.Email, checkLoginStatus.UserName);

            var response = RestClient.Post(
                new AssignRoles
                {
                    UserName = "user8@gmail.com",
                    Roles = { "Role1", "Role2" },
                    Permissions = { "Permission1", "Permission2" }
                });

            Assert.That(response.AllRoles, Is.EquivalentTo(new[] { "Role1", "Role2" }));
            Assert.That(response.AllPermissions, Is.EquivalentTo(new[] { "Permission1", "Permission2" }));
            
        }

    }
}