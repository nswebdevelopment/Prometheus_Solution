using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    public class CompanyModel
    {
        public long CompanyId { get; set; }
        [Required]
        [Display(Name = "Company name")]
        public string CompanyName { get; set; }

        [Required]
        public string CompanyAddress { get; set; }

        public string CompanyPhone { get; set; }

        public string CompanyMobile { get; set; }

        [Display(Name = "Contact person")]
        public string CompanyContactPerson { get; set; }

        [Required]
        [EmailAddress]
        public string CompanyEmail { get; set; }

        [StringLength(500, ErrorMessage = "Description is too long.")]
        public string Descrition { get; set; }
    }
}
