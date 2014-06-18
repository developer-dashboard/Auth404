using System;
using System.Collections.Generic;
using Auth_404.DatabaseSetup;
using Auth_404.Model.Constants;
using Auth_404.Model.Data;
using Auth_404.WebAPI;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Data;
using ServiceStack.Logging;
using ServiceStack.Logging.Log4Net;
using ServiceStack.OrmLite;


namespace Auth_404.UnitTests
{
    [TestFixture]
    public class WebAuthenticationTests
    {
        private Auth_404AppHost _appHost;

        protected virtual string ListeningOn
        {
            get { return "http://localhost:50334/"; }
        }

        private IDbConnectionFactory _appDbConnectionFactory;
        private IDbConnectionFactory _authDbConnectionFactory;

        [TestFixtureSetUp]
        public void on_set_up()
        {
            LogManager.LogFactory = new Log4NetFactory(true);
            _appDbConnectionFactory = new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider);
            _authDbConnectionFactory = new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider);

            DataBaseHelper.Setup_Test_Database(_appDbConnectionFactory, _authDbConnectionFactory);

            _appHost = new Auth_404AppHost(_appDbConnectionFactory, _authDbConnectionFactory);
            _appHost.Init();
            _appHost.Start(ListeningOn);
        }

        [TestFixtureTearDown]
        public void on_tear_down()
        {
            _appHost.Dispose();
        }

        private IServiceClient GetJsonClient()
        {
            return new JsonServiceClient(ListeningOn);
        }

        [Test]
        public void get_currency_types_ok_with_authentication()
        {
            var client = GetJsonClient();
            client.SetCredentials(DefaultAdmin.Email, DefaultAdmin.Password);
            var response = client.Get<List<CurrencyType>>("/currencytypes");
            Assert.IsNotNull(response);
        }
        
        [Test]
        public void get_currency_types_ok_without_authentication()
        {
            var client = GetJsonClient();
            var response = client.Get<List<CurrencyType>>("/currencytypes");
            Assert.IsNotNull(response);
        }

        [Test]
        public void post_transaction_fails_without_authentication()
        {
            var transaction = new Transaction
            {
                Amount = 10.00m,
                Card = "XXXXXXXXXX124",
                CreateDate = DateTime.UtcNow,
                SubscriptionId = 101,
                GatewayTransactionId = "123456",
                TransactionTypeId = (long) TRANSACTION_TYPE.AuthorizeAndCapture,
                TransactionStatusId = (long) TRANSACTION_STATUS.Pending,
                GatewayResponse = "ok"
            };
            var client = GetJsonClient();
            var error = Assert.Throws<WebServiceException>(() => client.Post(transaction));
            Assert.AreEqual("Unauthorized", error.Message);
        }

        [Test]
        public void post_transaction_success_when_authenticated()
        {
            var transaction = new Transaction
            {
                Amount = 10.00m,
                Card = "XXXXXXXXXX124",
                CreateDate = DateTime.UtcNow,
                SubscriptionId = 101,
                GatewayTransactionId = "123456",
                TransactionTypeId = (long) TRANSACTION_TYPE.AuthorizeAndCapture,
                TransactionStatusId = (long) TRANSACTION_STATUS.Pending,
                GatewayResponse = "ok"
            };
            var client = GetJsonClient();
            client.SetCredentials(DefaultAdmin.Email, DefaultAdmin.Password);
            var results = client.Post(transaction);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Id > 0);
        }

        [Test]
        public void test_logout()
        {
            var client = GetJsonClient();
            var authResponse = client.Send(new Authenticate
            {
                provider = CredentialsAuthProvider.Name,
                UserName = DefaultAdmin.Email,
                Password = DefaultAdmin.Password,
                RememberMe = true,
            });

            Assert.IsNotNull(authResponse);
            Assert.IsNotNull(authResponse.SessionId);
            Assert.AreEqual(DefaultAdmin.Email, authResponse.UserName);

            var response = client.Get<AuthenticateResponse>("/auth");
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.SessionId);
            Assert.AreEqual(DefaultAdmin.Email, response.UserName);

            var transactions = client.Get<List<Transaction>>("/transactions");
            Assert.IsNotNull(transactions);


            var logoutResponse = client.Get<AuthenticateResponse>("/auth/logout");

            Assert.That(logoutResponse.ResponseStatus.ErrorCode, Is.Null);

            logoutResponse = client.Send(new Authenticate
            {
                provider = AuthenticateService.LogoutAction,
            });

            Assert.That(logoutResponse.ResponseStatus.ErrorCode, Is.Null);

            //finally check to ensure logout
            var error1 = Assert.Throws<WebServiceException>(() =>client.Get<AuthenticateResponse>("/auth"));
            Assert.AreEqual("Not Authenticated", error1.Message);

            var error = Assert.Throws<WebServiceException>(() =>client.Get<List<Transaction>>("/transactions"));
            Assert.AreEqual("Unauthorized", error.Message);
        }
    }
}