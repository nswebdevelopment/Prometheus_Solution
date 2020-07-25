using Prometheus.Model.Models.EnterpriseAdapterModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Prometheus.Common.Enums;

namespace Prometheus.Model.Models
{
    public class JobDefinitionModel
    {
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }
        public bool Retry { get; set; }

        [Range(1,10)]
        [DisplayName("Number of retry")]
        public int NumberOfRetry { get; set; }
        
        [Required]
        public long From { get; set; }

        [Required]
        public long To { get; set; }

        public AdapterType FromCategory { get; set; }
        
        [DisplayName("Enterprise adapter")]
        public long EnterpriseAdapterId { get; set; }

        [DisplayName("Crypto adapter")]
        public long CryptoAdapterId { get; set; }

        [DisplayName("Business adapter")]
        public long BusinessAdapterId { get; set; }

        [DisplayName("Enterprise adapter")]
        public List<EnterpriseAdapterSelectListModel> EnterpriseAdapters { get; set; }

        [DisplayName("Crypto adapter")]
        public List<CryptoAdapterSelectListModel> CryptoAdapters { get; set; }
        
        [DisplayName("Business adapter")]
        public List<BusinessAdapterSelectListModel> BusinessAdapters { get; set; }

        public List<AdapterSelectListModel> Adapters { get; set; }

        public long UserProfileId { get; set; }
        
        public List<JobDefinitionPropertyModel> Properties { get; set; }
        
        public List<PropertyModel> PropertiesGet { get; set; }

    }

    public class JobDefinitionSelectListModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
