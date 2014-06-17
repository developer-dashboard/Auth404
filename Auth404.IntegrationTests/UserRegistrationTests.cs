using System.Collections.Generic;
using System.Configuration;
using Auth_404.DatabaseSetup;
using Auth_404.Model.Data;
using Auth_404.Model.Operations;
using Auth_404.Model.Requests;
using NUnit.Framework;
using ServiceStack;

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

            DataBaseHelper.Settup_Test_Database();
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

            var checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate());
            Assert.IsNotNull(checkLoginStatus);
            Assert.IsNullOrEmpty(checkLoginStatus.UserName);
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
            var checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate());
            Assert.IsNotNull(checkLoginStatus);
            Assert.IsNullOrEmpty(checkLoginStatus.UserName);


            // login the user in
            checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate {provider = "credentials", UserName = "user2@gmail.com", Password = "user2"});
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
            RestClient.Post(new Authenticate {provider = "logout"});
            var checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate());
            Assert.IsNotNull(checkLoginStatus);
            Assert.IsNullOrEmpty(checkLoginStatus.UserName);


            // login the user in by making a request with a Basic Auth Header
            var client = new JsonServiceClient {AlwaysSendBasicAuthHeader = true, BaseUri = WebServiceHostUrl, UserName = "user3@gmail.com", Password = "user3"};
            var response = client.Get<List<Transaction>>(new GetTransactions());
            Assert.IsNotNull(response);


            checkLoginStatus = client.Post<AuthenticateResponse>(new Authenticate());
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual("user3@gmail.com", checkLoginStatus.UserName);

            //logout
            RestClient.Post(new Authenticate {provider = "logout"});
            checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate());
            Assert.IsNotNull(checkLoginStatus);
            Assert.IsNullOrEmpty(checkLoginStatus.UserName);
            // ReSharper restore RedundantTypeArgumentsOfMethod
        }

        [Test]
        public void user_can_update_their_registration()
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
    }
}