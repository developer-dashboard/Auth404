using Auth_404.Model.Data;
using Auth_404.Model.Operations;
using RESTServiceUtilities.Implementations.Db;

namespace Auth_404.DataLayer.Repositories
{
    public class TransactionTypeRepository : DbTypeRepository<TransactionType, GetTransactionTypes>
    {
    }
}