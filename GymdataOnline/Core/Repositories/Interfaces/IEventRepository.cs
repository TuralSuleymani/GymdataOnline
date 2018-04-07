using AccreditationMS.Models.Domain;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccreditationMS.Core.Repositories.Interfaces
{
   public interface IEventRepository : IRepository<Event,int>,IDbDependency<Event,int>
    {
        Task<Event> GetActiveEventWithRelatedDataByIdAsync(int id);
        Task<IEnumerable<Event>> GetActiveEventsWithRelatedDataAsync();
        Task<Event> GetActiveEventIdAsync(int id);
        Event GetActiveEventByIdAsNoTracked(int id);
        Task<IEnumerable<Event>> GetActiveEventsAsync();
        Task<IEnumerable<User_Events>> GetActiveEventsByUserAsync(string userId);
        Task<IEnumerable<Event>> GetActiveNestedEventsByUserAsync(string userId, UserManager<AppUser> _userManager, IUserDependencyRepository dependencyRepository);
        IEnumerable<Event> GetActiveNestedEventsByUser(string userId, UserManager<AppUser> _userManager, IUserDependencyRepository dependencyRepository);
        Task<IEnumerable<Event>> GetAllEventsInRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Event>> GetAllEventsAsync();
    }
}
