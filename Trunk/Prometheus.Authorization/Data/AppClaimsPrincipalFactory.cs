using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Prometheus.Dal;
using Prometheus.Dal.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Authorization.Data
{
    public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole<long>>
    {
        private readonly IPrometheusEntities _entity;

        public AppClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole<long>> roleManager, 
            IOptions<IdentityOptions> options,
            IPrometheusEntities entity) : base(userManager, roleManager, options)
        {
            _entity = entity;
        }

        public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);

            UserProfile userProfile = _entity.UserProfile.Find(user.UserProfileId);

            if (userProfile != null)
            {
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim("FirstName", userProfile.FirstName));
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim("UserProfileId", user.UserProfileId.ToString()));
            }
            ((ClaimsIdentity)principal.Identity).AddClaim(new Claim("UserId", user.Id.ToString()));
       
            return principal;
        }
    }
}
