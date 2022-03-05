using FolderCleaner.Worker.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FolderCleaner.Worker.Models
{
    public class CleaningHistory
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; private set; } = DateTime.Now;
        public DateTime? UpdatedOn { get; set; }
        public DateTime RunsAt { get; set; }

        private CleaningStatus cleaningStatus;
        public CleaningStatus CleaningStatus
        {
            get => cleaningStatus;
            set
            {
                cleaningStatus = value;
                CleaningStatusDescription = CleaningStatusDescription == null ? value.ToString() : $"{CleaningStatusDescription}→{value}";
            }
        }
        public string CleaningStatusDescription { get; private set; }
        public TriggeredBy TriggeredBy { get; set; }
        public int? TotalFiles { get; set; }
        public long? CleaningSize { get; set; }
        public int CleaningRecordId { get; set; }
        [ForeignKey(nameof(CleaningRecordId))]
        public CleaningRecord CleaningRecord { get; set; }

        public void AppendExtraMessageToStatusDescription(string message)
        {
            CleaningStatusDescription += $"! {message}";
        }
    }
}