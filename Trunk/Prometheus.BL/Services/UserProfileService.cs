using Prometheus.BL.Interfaces;
using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Dal;
using Prometheus.Dal.Entities;
using Prometheus.Model.Models;
using Prometheus.Model.Models.AuthorizationModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IPrometheusEntities _entity;
        private readonly ILogger _logger;

        public UserProfileService(IPrometheusEntities entity, ILogger logger)
        {
            _entity = entity;
            _logger = logger;
        }

        public void DeleteUserProfile(long Id)
        {            
            try
            {
                var user = _entity.UserProfile.Find(Id);
                _entity.UserProfile.Remove(user);
                _entity.SaveChanges();
            }
            catch(Exception ex)
            {
                _logger.Information($"UserProfileService.DeleteUserProfile(Id: {Id}");
                _logger.Error(ex.Message);
            }
        }

        public IResponse<long> SaveUserProfile(RegisterModel model)
        {            
            var result = new Response<long>();

            try
            {
                var userProfile = new UserProfile();
                if (model.CompanyName != null)
                {
                    _entity.Company.Add(new Company
                    {
                        Name = model.CompanyName,
                        Address = model.CompanyAddress,
                        Email = model.CompanyEmail,
                        Phone = model.CompanyPhone,
                        Mobile = model.CompanyMobile,
                        ContactPerson = model.CompanyContactPerson,
                        Description = model.Descrition
                    });
                    userProfile.CompanyId = model.CompanyId;
                }
                else
                {
                    userProfile.CompanyId = null;
                }

                userProfile.FirstName = model.FirstName;
                userProfile.LastName = model.LastName;
                userProfile.RegistrationDate = DateTime.UtcNow;

                _entity.UserProfile.Add(userProfile);
                _entity.SaveChanges();

                result.Status = Common.Enums.StatusEnum.Success;
                result.Value = userProfile.Id;
            }
            catch (Exception ex)
            {
                result.Status = Common.Enums.StatusEnum.Error;
                _logger.Information($"UserProfileService.SaveUserProfile(model: {model}");
                _logger.Error(ex.Message);
            }

            return result;
        }
    }
}
