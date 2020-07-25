using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models.ScheduleModel
{
    public class ScheduleModel
    {
        public long Id { get; set; }

        [Required]
        public long JobDefinitionId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        //end date needs to be removed
        public DateTime EndDate { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
        public string Description { get; set; }
        public string RecurrenceRule { get; set; }
        public long UserProfileId { get; set; }
        public string User { get; set; }

        public long JobId { get; set; }
        public bool JobsExecuted { get; set; }
        public List<JobDefinitionSelectListModel> JobDefinitions { get; set; }
    }
}
