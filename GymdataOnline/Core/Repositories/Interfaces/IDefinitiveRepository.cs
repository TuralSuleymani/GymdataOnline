using AccreditationMS.Models.Domain;
using AccreditationMS.Models.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccreditationMS.Core.Repositories.Interfaces
{
    public interface IDefinitiveRepository : IRepository<Definitive, int>, IDbDependency<Definitive, int>, IEventDepend<Definitive>
    {
        Task<Definitive> GetWithRelatedDataByParamsAsync(int eventid, int functionId);
        Task<IEnumerable<DefinitiveCompact>> GetCompactDataByEventIdAsync(int eventId);
        Task<Definitive> GetWithRelatedDataByParamsAsync(int eventId, int functionId, int subgroupId);
    }
}
