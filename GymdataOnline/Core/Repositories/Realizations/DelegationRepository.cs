using AccreditationMS.Core.Repositories.Interfaces;
using AccreditationMS.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AccreditationMS.Models.Views;

namespace AccreditationMS.Core.Repositories.Realizations
{
    public class DelegationRepository : Repository<Delegation, int>, IDelegationRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationRepository"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public DelegationRepository(AccreditationDbContext context) : base(context)
        {
        }

        public async Task<Delegation> GetWithRelatedDataByIdAsync(int id)
        {
            return await Context.Delegations.Include(x=>x.Event)
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Delegation>> GetWithRelatedDataAsync()
        {
            return await Context.Delegations.Include(x=>x.Event)
                .ToListAsync();
        }

        public AccreditationDbContext AccreditationContext
        {
            get { return Context as AccreditationDbContext; }
        }
        public void Add(Delegation delegation)
        {
            Context.Add(delegation);
        }

        public async Task<IEnumerable<Delegation>> GetByEventIdAsync(int eventId)
        {
            return await Context.Delegations
                                    .Where(x => x.EventId == eventId && x.IsMedia==false)
                                             .ToListAsync();
        }

        public async Task<Delegation> GetDataByUserAndEventIdAsync(string userId, int eventId)
        {
            return await Context.Delegations
                            .Where(x => x.EventId == eventId && x.AppUserId == userId && x.IsMedia==false)
                               .FirstOrDefaultAsync();
        }

        public Delegation GetDataByUserAndEventId(string userId, int eventId)
        {
            return Context.Delegations
                            .Where(x => x.EventId == eventId && x.AppUserId == userId)
                               .FirstOrDefault();
        }

        public Delegation GetDelegationByEventId(int eventId,string userId)
        {
            return Context.Delegations.Where(x => x.EventId == eventId && x.AppUserId == userId).SingleOrDefault();
        }

        public IEnumerable<CountryDelegation> GetDependentUsers(int eventId, string userId)
        {
            var ccc = (Context.Delegations.Include(x => x.AppUser).Include(x => x.AppUser.Country)
                 .Where(x => (x.AppUserId == userId || x.EventId == eventId) && x.IsMedia==false).Select(x => x));


             return (from c in ccc join y in Context.UserDependencies
                on c.AppUserId equals y.UserId
                  select new CountryDelegation
                  {
                      AppUserId = y.UserId,
                      Email = c.Email,
                      EventId = c.EventId,
                      FederationName = c.FederationName,
                      FirstName = c.FirstName,
                      LastName = c.LastName,
                      Id = c.Id,
                      MobilePhone = c.MobilePhone,
                      Phone = c.Phone,
                      AppUser = c.AppUser,
                      CountryName = c.AppUser.Country.A3,
                      CountryFlag = c.AppUser.Country.ImagePath

                  }).ToList();

        }
    }
}
