using FolderCleaner.Worker.Enums;
using FolderCleaner.Worker.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FolderCleaner.Worker.Services.Interfaces
{
    public interface ICleaningRecordService
    {
        Task<IEnumerable<CleaningRecord>> GetAllAsync(CancellationToken cancellationToken);
        Task<CleaningRecord> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<CleaningRecord> CreateAsync(CleaningRecord cleaningRecord, TriggeredBy triggeredBy, CancellationToken cancellationToken);
        Task<CleaningRecord> UpdateAsync(CleaningRecord cleaningRecord, CancellationToken cancellationToken);
        Task<CleaningRecord> SwitchActivityAsync(int id, CancellationToken cancellationToken);
        Task DeleteByIdAsync(int id, CancellationToken cancellationToken);
    }
}