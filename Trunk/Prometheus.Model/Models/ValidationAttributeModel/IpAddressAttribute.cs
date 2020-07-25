using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;

namespace Prometheus.Model
{

    public class IpAddressAttribute : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                string IpAddressOrHost = value.ToString();
                try
                {
                    var IpAddress = Dns.GetHostEntry(IpAddressOrHost);
                    return ValidationResult.Success;
                }
                catch
                {
                    return new ValidationResult("No such host is known.");
                }
            }
            return new ValidationResult("The ServerIP field is required.");
        }
    }
}


