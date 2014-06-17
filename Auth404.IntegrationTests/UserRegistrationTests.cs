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

        [Test]
        public void can_register_a_new_user_and_log_them_on()
        {
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
        }

        [Test]
        public void test_login_logout_using_credential_auth()
        {
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
            RestClient.Post(new Authenticate { provider = "logout" });
            var checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate());
            Assert.IsNotNull(checkLoginStatus);
            Assert.IsNullOrEmpty(checkLoginStatus.UserName);

            
            // login the user in
            checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate { provider = "credentials", UserName = "user2@gmail.com", Password = "user2"});
            Assert.IsNotNull(checkLoginStatus);
            Assert.AreEqual("user2@gmail.com", checkLoginStatus.UserName);

            //logout
            RestClient.Post(new Authenticate { provider = "logout" });
            checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate());
            Assert.IsNotNull(checkLoginStatus);
            Assert.IsNullOrEmpty(checkLoginStatus.UserName);

        }

        [Test]
        public void test_login_logout_using_basic_auth()
        {
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
            RestClient.Post(new Authenticate { provider = "logout" });
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
            RestClient.Post(new Authenticate { provider = "logout" });
            checkLoginStatus = RestClient.Post<AuthenticateResponse>(new Authenticate());
            Assert.IsNotNull(checkLoginStatus);
            Assert.IsNullOrEmpty(checkLoginStatus.UserName);

        }
    }
}
