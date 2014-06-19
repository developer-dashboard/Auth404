using System.Collections.Generic;
using System.Configuration;
using Auth_404.BusinessLogic.BusinessLogic;
using Auth_404.DataLayer.Repositories;
using Auth_404.Model.Constants;
using Auth_404.Model.Data;
using Auth_404.Model.Operations;
using Auth_404.Model.Requests;
using Auth_404.WebAPI.Services;
using Funq;
using RESTServiceUtilities.Interfaces;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Data;
using ServiceStack.FluentValidation;
using ServiceStack.Logging;
using ServiceStack.Logging.Log4Net;
using ServiceStack.OrmLite;
//using ServiceStack.Redis;
using WebServiceUtilities.Utilities;

namespace Auth_404.WebAPI
{ 
    public class Auth_404AppHost : AppHostHttpListenerBase
    {

        private readonly IDbConnectionFactory _appDbConnectionFactory;
        private readonly IDbConnectionFactory _authDbConnectionFactory;

        public Auth_404AppHost() : base("Auth 404 Services", typeof(Auth_404AppHost).Assembly)
        {
            _appDbConnectionFactory = new OrmLiteConnectionFactory(ConfigurationManager.ConnectionStrings["AppDb"].ConnectionString, SqlServerDialect.Provider);
            _authDbConnectionFactory = new OrmLiteConnectionFactory(ConfigurationManager.ConnectionStrings["AuthDb"].ConnectionString, SqlServerDialect.Provider);
        }

        public Auth_404AppHost(IDbConnectionFactory appDbConnectionFactory, IDbConnectionFactory authDbConnectionFactory) : base("Auth 404 Services", typeof(Auth_404AppHost).Assembly)
        {
            _appDbConnectionFactory = appDbConnectionFactory;
            _authDbConnectionFactory = authDbConnectionFactory;
        }

        public override void Configure(Container container)
        {
            LogManager.LogFactory = new Log4NetFactory(true);
            container.Register(_appDbConnectionFactory);

            Plugins.Add(new AuthFeature(() => new AuthUserSession(),
                new IAuthProvider[] {new BasicAuthProvider(), new CredentialsAuthProvider(),}) {HtmlRedirect = null});

            //No use a custom one
            //Plugins.Add(new RegistrationFeature());
           
            var userRepo = new OrmLiteAuthRepository(_authDbConnectionFactory);
           
            container.Register<IUserAuthRepository>(userRepo);

            //wire-up a validator for the UserRegistrationService
            var userRegistrationValidator = new UserRegistrationRequestValidator {UserAuthRepo = userRepo};
            container.Register<IValidator<UserRegistrationRequest>>(userRegistrationValidator);
            
            var currencyTypeRepository = new CurrencyTypeRepository { DbConnectionFactory = _appDbConnectionFactory };
            var transactionTypeRepository = new TransactionTypeRepository { DbConnectionFactory = _appDbConnectionFactory };
            var transactionStatusTypeRepository = new TransactionStatusTypeRepository { DbConnectionFactory = _appDbConnectionFactory };
            var transactionNotificationStatusTypeRepository = new TransactionNotificationStatusTypeRepository { DbConnectionFactory = _appDbConnectionFactory };
            var transactionRepository = new TransactionRepository { DbConnectionFactory = _appDbConnectionFactory };

            var currencyTypeLogic = new CurrencyTypeLogic { Repository = currencyTypeRepository };
            var transactionTypeLogic = new TransactionTypeLogic { Repository = transactionTypeRepository };
            var transactionStatusTypeLogic = new TransactionStatusTypeLogic { Repository = transactionStatusTypeRepository };
            var transactionNotificationStatusTypeLogic = new TransactionNotificationStatusTypeLogic { Repository = transactionNotificationStatusTypeRepository };
            var transactionLogic = new TransactionLogic {Repository = transactionRepository};

            container.Register<IRest<CurrencyType, GetCurrencyTypes>>(currencyTypeLogic);
            container.Register<IRest<TransactionType, GetTransactionTypes>>(transactionTypeLogic);
            container.Register<IRest<TransactionStatusType, GetTransactionStatusTypes>>(transactionStatusTypeLogic);
            container.Register<IRest<TransactionNotificationStatusType, GetTransactionNotificationStatusTypes>>(transactionNotificationStatusTypeLogic);
            container.Register<IRest<Transaction, GetTransactions>>(transactionLogic);
           
            CatchAllHandlers.Add((httpMethod, pathInfo, filePath) => pathInfo.StartsWith("/favicon.ico") ? new FavIconHandler() : null);
        }
    }
   
}