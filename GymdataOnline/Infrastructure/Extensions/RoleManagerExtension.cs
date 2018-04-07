using AccreditationMS.Core.Repositories.Interfaces;
using AccreditationMS.Models.Domain;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccreditationMS.Infrastructure.Extensions
{
    public static class RoleManagerExtension
    {
        public static async Task<RoleDependency> GetBiggestRoleAsync(this RoleManager<IdentityRole> _roleManager,UserManager<AppUser> _userManager, AppUser user,IRoleDependencyRepository roleDependencyRepository)
        {
            return (from str in (await _userManager.GetRolesAsync(user))
                    join t in _roleManager.Roles
                    on str equals t.Name
                    join f in (await roleDependencyRepository.GetAllAsync())
                    on t.Id equals f.RoleId
                    orderby f.RoleType descending
                    select f).FirstOrDefault();
        }
    }
}
