using Auth_404.BusinessLogic.BusinessLogic;
using Auth_404.Model.Data;
using Auth_404.Model.Operations;
using WebServiceUtilities.Implementations;

namespace Auth_404.WebAPI.Services
{
    public class TransactionNotificationStatusTypeWebService :
        StandardWebService<TransactionNotificationStatusType, GetTransactionNotificationStatusTypes, TransactionNotificationStatusTypeLogic>
    {
    }
}