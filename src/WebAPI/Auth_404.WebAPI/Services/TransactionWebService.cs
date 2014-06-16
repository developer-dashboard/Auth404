using System;
using Auth_404.BusinessLogic.BusinessLogic;
using Auth_404.Model.Data;
using Auth_404.Model.Operations;
using ServiceStack;
using WebServiceUtilities.Implementations;

namespace Auth_404.WebAPI.Services
{
    public class TransactionWebService : StandardWebService<Transaction, GetTransactions, TransactionLogic>
    {
        [Authenticate]
        public override object Get(GetTransactions request)
        {
            var session = GetSession();
            var userName = session.UserAuthName;
            if (string.IsNullOrEmpty(userName))
                throw new ApplicationException("What the hell! No User?!?");
            return base.Get(request);
        }


        [Authenticate]
        public override object Post(Transaction data)
        {
            var session = GetSession();
            var userName = session.UserAuthName;
            if (string.IsNullOrEmpty(userName))
                throw new ApplicationException("What the hell! No User?!?");
            
            return base.Post(data);
        }
    }
}