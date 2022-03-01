using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FolderCleaner.Models
{
    public class CleanerHistoryModel
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; private set; } = DateTime.Now;
        //public DateTime UpdatedOn { get; set; }
        private CleanerStatus cleanerStatus;
        public CleanerStatus CleanerStatus
        {
            get => cleanerStatus;
            set
            {
                cleanerStatus = value;
                CleanerStatusDescription = value.ToString();
            }
        }
        public string CleanerStatusDescription { get; private set; }
        public int CleanerId { get; set; }
        [ForeignKey("CleanerId")]
        public CleanerModel CleanerModel { get; set; }

    }
}