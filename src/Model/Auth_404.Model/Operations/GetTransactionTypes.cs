using System.Collections.Generic;
using Auth_404.Model.Data;
using RESTServiceUtilities.Interfaces;
using ServiceStack;

namespace Auth_404.Model.Operations
{
    [Api("Return a List of Transaction Type Resources")]
    [Route("/TransactionTypes", "GET")]
    [Route("/TransactionTypes/{Ids}", "GET")]
    public class GetTransactionTypes : IReturn<List<TransactionStatusType>>, IDatas
    {
        public List<long> Ids { get; set; }
    }
}