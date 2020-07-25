using System.ComponentModel.DataAnnotations;

namespace Prometheus.Model.Models.AuthorizationModel
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
