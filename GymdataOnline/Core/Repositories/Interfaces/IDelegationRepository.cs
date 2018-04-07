using AccreditationMS.Models.Domain;
using AccreditationMS.Models.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccreditationMS.Core.Repositories.Interfaces
{
    public interface IDelegationRepository : IRepository<Delegation, int>, IDbDependency<Delegation,int>,IEventDepend<Delegation>,ICurrentData<Delegation,string,int>
    {
        //TODO: add Event methods
        void Add(Delegation delegation);
        Delegation GetDelegationByEventId(int eventId,string userId);
        IEnumerable<CountryDelegation> GetDependentUsers(int eventId, string userId);
    }
}
