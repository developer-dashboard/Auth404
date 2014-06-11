using System;
using Auth_404.Model.Data;
using Auth_404.Model.Operations;
using RESTServiceUtilities.Implementations.Db;

namespace Auth_404.DataLayer.Repositories
{
    public class CurrencyTypeRepository : DbTypeRepository<CurrencyType, GetCurrencyTypes>
    {
        public override void ValidateInsertData(CurrencyType currencyType)
        {
            base.ValidateInsertData(currencyType);

            if (string.IsNullOrEmpty(currencyType.Code))
                throw new ArgumentException("CurrencyType: Code must not be null");

            if (string.IsNullOrEmpty(currencyType.Symbol))
                throw new ArgumentException("CurrencyType: Symbol must not be null");
        }
    }
}