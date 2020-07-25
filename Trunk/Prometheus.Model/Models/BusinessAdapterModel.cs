using Prometheus.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Model.Models
{
    public class BusinessAdapterModel
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Adapter Type")]
        public AdapterTypeItemEnum BusinessAdapterType { get; set; }
        [Required]
        public DirectionEnum Direction { get; set; }
        [Display(Name = "File Name")]
        public string FileName { get; set; }

    }

    public class BusinessAdapterSelectListModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
