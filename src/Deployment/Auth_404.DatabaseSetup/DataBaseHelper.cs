using System.Configuration;
using Auth_404.Model.Constants;
using Auth_404.Model.Data;
using ServiceStack.Auth;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Auth_404.DatabaseSetup
{
    public class DataBaseHelper
    {
        private static void Main()
        {
            Setup_Test_Database();
        }

        public static void Setup_Test_Database(IDbConnectionFactory appDbFactory = null, IDbConnectionFactory authDbFactory = null)
        {
            var appDbConnectionFactory = appDbFactory;
            var authDbConnectionFactory = authDbFactory;
            
            if (appDbConnectionFactory == null)
            {
                var connectionString = ConfigurationManager.ConnectionStrings["AppDb"].ConnectionString;
                appDbConnectionFactory = new OrmLiteConnectionFactory(connectionString, SqlServerDialect.Provider);
            }

            if (authDbConnectionFactory == null)
            {
                var connectionString = ConfigurationManager.ConnectionStrings["AuthDb"].ConnectionString;
                authDbConnectionFactory = new OrmLiteConnectionFactory(connectionString, SqlServerDialect.Provider);
            }


            using (var db = appDbConnectionFactory.OpenDbConnection())
            {
                if (db.TableExists("Transaction")) db.DropTable<Transaction>();
                if (db.TableExists("CurrencyType")) db.DropTable<CurrencyType>();
                if (db.TableExists("TransactionStatusType")) db.DropTable<TransactionStatusType>();
                if (db.TableExists("TransactionNotificationStatusType")) db.DropTable<TransactionNotificationStatusType>();
                if (db.TableExists("TransactionType")) db.DropTable<TransactionType>();

                db.CreateTable<TransactionType>();
                db.CreateTable<TransactionNotificationStatusType>();
                db.CreateTable<TransactionStatusType>();
                db.CreateTable<CurrencyType>();
                db.CreateTable<Transaction>();

                db.Insert(new TransactionType {Id = (long) TRANSACTION_TYPE.AuthorizeAndCapture, Description = "Authorize and Capture"});
                db.Insert(new TransactionType {Id = (long) TRANSACTION_TYPE.AuthorizeOnly, Description = "Authorize Only"});
                db.Insert(new TransactionType {Id = (long) TRANSACTION_TYPE.CapturePrior, Description = "Capture Prior Authorization"});
                db.Insert(new TransactionType {Id = (long) TRANSACTION_TYPE.Refund, Description = "Refund"});
                db.Insert(new TransactionType {Id = (long) TRANSACTION_TYPE.Void, Description = "Void"});
                db.Insert(new TransactionType {Id = (long) TRANSACTION_TYPE.ZeroDollar, Description = "An internal zero dollar transaction"});
                db.Insert(new TransactionType {Id = (long) TRANSACTION_TYPE.Unknown, Description = "The transaction type is unknown"});

                db.Insert(new TransactionNotificationStatusType {Id = (long) TRANSACTION_NOTIFICATION_STATUS.None, Status = "Processing"});
                db.Insert(new TransactionNotificationStatusType {Id = (long) TRANSACTION_NOTIFICATION_STATUS.DeclinedNotification, Status = "Declined Notification Sent"});
                db.Insert(new TransactionNotificationStatusType {Id = (long) TRANSACTION_NOTIFICATION_STATUS.ErrorNotification, Status = "Error Notifications Sent"});
                db.Insert(new TransactionNotificationStatusType {Id = (long) TRANSACTION_NOTIFICATION_STATUS.SettledNotification, Status = "Settled Notifications Sent"});
                db.Insert(new TransactionNotificationStatusType {Id = (long) TRANSACTION_NOTIFICATION_STATUS.RefundedNotification, Status = "Refunded Notifications Sent"});
                db.Insert(new TransactionNotificationStatusType {Id = (long) TRANSACTION_NOTIFICATION_STATUS.VoidedNotification, Status = "Voided Notifications Sent"});

                db.Insert(new TransactionStatusType {Id = (long) TRANSACTION_STATUS.Pending, Status = "Transaction Approved but pending completion"});
                db.Insert(new TransactionStatusType {Id = (long) TRANSACTION_STATUS.Settled, Status = "Transaction completed.  Funds received."});
                db.Insert(new TransactionStatusType {Id = (long) TRANSACTION_STATUS.Refunded, Status = "Transaction completed.  Customer refunded."});
                db.Insert(new TransactionStatusType {Id = (long) TRANSACTION_STATUS.Voided, Status = "Transaction voided."});
                db.Insert(new TransactionStatusType {Id = (long) TRANSACTION_STATUS.Expired, Status = "Transaction has expired."});
                db.Insert(new TransactionStatusType {Id = (long) TRANSACTION_STATUS.Declined, Status = "Transaction Declinded.", IsErrorStatus = true});
                db.Insert(new TransactionStatusType {Id = (long) TRANSACTION_STATUS.Error, Status = "Transaction Error.", IsErrorStatus = true});

                db.Insert(new CurrencyType {Id = (long) CURRENCY_TYPE.USDollar, Description = "US Dollar", Code = "USD", Symbol = "$"});
                db.Insert(new CurrencyType {Id = (long) CURRENCY_TYPE.CandianDollar, Description = "Canadian Dollar", Code = "CAD", Symbol = "$"});
                db.Insert(new CurrencyType {Id = (long) CURRENCY_TYPE.Peso, Description = "Mexican Peso", Code = "MXN", Symbol = "$"});
            }

            
            
            var userRepo = new OrmLiteAuthRepository(authDbConnectionFactory);
            userRepo.DropAndReCreateTables();
            
            var defaultAdmin = new UserAuth
            {
                DisplayName = DefaultAdmin.Email,
                Email = DefaultAdmin.Email,
                Roles = DefaultAdmin.Roles,
                Permissions = DefaultAdmin.Permissions
            };

            CreateUser(userRepo, defaultAdmin, DefaultAdmin.Password);

            var testUser = new UserAuth
            {
                DisplayName = TestUser.FirstName + " " + TestUser.LastName,
                Email = TestUser.Email,
                FirstName = TestUser.FirstName,
                LastName = TestUser.LastName,
                Roles = TestUser.Roles,
                Permissions = TestUser.Permissions
            };

            CreateUser(userRepo, testUser, TestUser.Password);

        }

        private static void CreateUser(IUserAuthRepository userRepo, IUserAuth user, string password)
        {
            string hash;
            string salt;
            new SaltedHash().GetHashAndSaltString(password, out hash, out salt);
            user.Salt = salt;
            user.PasswordHash = hash;
            userRepo.CreateUserAuth(user, password);
        }
    }
}