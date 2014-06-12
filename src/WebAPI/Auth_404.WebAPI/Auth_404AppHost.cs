using System.Configuration;
using Auth_404.BusinessLogic.BusinessLogic;
using Auth_404.DataLayer.Repositories;
using Auth_404.Model.Constants;
using Auth_404.Model.Data;
using Auth_404.Model.Operations;
using Funq;
using RESTServiceUtilities.Interfaces;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Caching;
using ServiceStack.Data;
using ServiceStack.Logging;
using ServiceStack.Logging.Log4Net;
using ServiceStack.OrmLite;
using ServiceStack.Redis;
using WebServiceUtilities.Utilities;

namespace Auth_404.WebAPI
{
    public class Auth_404AppHost : AppHostHttpListenerBase
    {

        private readonly IDbConnectionFactory _dbConnectionFactory;

        public Auth_404AppHost() : base("Auth 404 Services", typeof(Auth_404AppHost).Assembly)
        {
            _dbConnectionFactory = new OrmLiteConnectionFactory(ConfigurationManager.ConnectionStrings["Auth404Db"].ConnectionString, SqlServerDialect.Provider);
        }

        public Auth_404AppHost(IDbConnectionFactory dbConnectionFactory) : base("Auth 404 Services", typeof(Auth_404AppHost).Assembly)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public override void Configure(Container container)
        {
            LogManager.LogFactory = new Log4NetFactory(true);
            
            container.Register(_dbConnectionFactory);
            var basicAuthProvider = new BasicAuthProvider();
            container.Register(basicAuthProvider);

            Plugins.Add(new AuthFeature( () => new AuthUserSession(), new IAuthProvider[] {basicAuthProvider, }, SystemConstants.LoginUrl ));
            
            var userRepo = new OrmLiteAuthRepository(_dbConnectionFactory);
            container.Register<IAuthRepository>(userRepo);

            var cacheClient = new MemoryCacheClient();
            container.Register(cacheClient);
           
            var currencyTypeRepository = new CurrencyTypeRepository { DbConnectionFactory = _dbConnectionFactory };
            var transactionTypeRepository = new TransactionTypeRepository { DbConnectionFactory = _dbConnectionFactory };
            var transactionStatusTypeRepository = new TransactionStatusTypeRepository { DbConnectionFactory = _dbConnectionFactory };
            var transactionNotificationStatusTypeRepository = new TransactionNotificationStatusTypeRepository { DbConnectionFactory = _dbConnectionFactory };
            var transactionRepository = new TransactionRepository { DbConnectionFactory = _dbConnectionFactory };

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

            var redisLocation = ConfigurationManager.AppSettings["ReddisService"];
            Container.Register<IRedisClientsManager>(new PooledRedisClientManager(redisLocation));
           

        }
    }
   
}