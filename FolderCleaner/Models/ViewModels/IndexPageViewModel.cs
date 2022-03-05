using System.Collections.Generic;

namespace FolderCleaner.Worker.Models.ViewModels
{
    public class IndexPageViewModel
    {
        public IEnumerable<CleaningRecord> CollectionOfCleaningRecords { get; set; }
        public CleaningRecord CleaningRecord { get; set; }
    }
}