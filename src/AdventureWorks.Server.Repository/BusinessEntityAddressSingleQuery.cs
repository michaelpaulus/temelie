using AdventureWorks.Entities;
using Cornerstone.Repository;

namespace AdventureWorks.Repository;
public class BusinessEntityAddressSingleQuery(BusinessEntityId businessEntityId, AddressId addressId, AddressTypeId addressTypeId) : IQuerySpec<BusinessEntityAddress>
{
    public IQueryable<BusinessEntityAddress> Apply(IQueryable<BusinessEntityAddress> query)
    {
        return query.Where(i => i.BusinessEntityId == businessEntityId && i.AddressId == addressId && i.AddressTypeId == addressTypeId);
    }
}
