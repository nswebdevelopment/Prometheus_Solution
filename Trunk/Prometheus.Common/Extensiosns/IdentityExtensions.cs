using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Common.Extensiosns
{
    public static class IdentityExtensions
    {
        public static long? GetUserProfileId(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("UserProfileId");
            return (claim != null) ? Convert.ToInt64(claim.Value) : default(long?);
        }

        public static long? GetUserId(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("UserId");
            return (claim != null) ? Convert.ToInt64(claim.Value) : default(long?);
        }

        public static string FirstName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("FirstName");
            return (claim != null) ? claim.Value : string.Empty;
        }
    }
}
