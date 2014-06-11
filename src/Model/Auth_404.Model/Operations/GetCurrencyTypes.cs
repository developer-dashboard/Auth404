using System.Collections.Generic;
using Auth_404.Model.Data;
using RESTServiceUtilities.Interfaces;
using ServiceStack;

namespace Auth_404.Model.Operations
{
    [Api("Return a List of Currency Type Resources")]
    [Route("/CurrencyTypes", "GET")]
    [Route("/CurrencyTypes/{Ids}", "GET")]
    public class GetCurrencyTypes : IReturn<List<CurrencyType>>, IDatas
    {
        public List<long> Ids { get; set; }
    }
}