using Prometheus.Common;
using Prometheus.Model.Models.AuthorizationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Interfaces
{
    public interface IUserProfileService
    {
        IResponse<long> SaveUserProfile(RegisterModel model);
        void DeleteUserProfile(long Id);
    }
}
