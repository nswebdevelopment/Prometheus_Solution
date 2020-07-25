using Microsoft.AspNetCore.Identity;
using Prometheus.Dal;
using Prometheus.Dal.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Authorization.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser<long>
    {
        public long? UserProfileId { get; set; }
    }
}
