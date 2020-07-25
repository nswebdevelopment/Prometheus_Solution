using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model
{
    public class MustHaveFirst3ColumnsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var type = value as ColumnTypeModel;

            if (type.ParentColumn.ColumnName != null)
            {
                return ValidationResult.Success;
            } else
            {
                return new ValidationResult("This field is required!");
            }

        }
    }
}
