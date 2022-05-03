using FolderCleaner.Worker.Enums;
using FolderCleaner.Worker.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FolderCleaner.Worker.Services.Interfaces
{
    public interface ICleaningHistoryService
    {
        Task StartCleaningAsync(CleaningHistory activeHistory, TriggeredBy triggeredBy, CancellationToken cancellationToken);
        Task StartCleaningAsync(TriggeredBy triggeredBy, CancellationToken cancellationToken);
        Task<IEnumerable<CleaningHistory>> GetAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<CleaningHistory>> GetActiveTriggeredByWorkerAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<CleaningHistory>> GetActiveStatusReadyRecordsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<CleaningHistory>> GetActivePendingRecordsAsync(CancellationToken cancellationToken);
        Task<CleaningHistory> CreateAsync(CleaningHistory cleaningHistory, CancellationToken cancellationToken);
        Task<CleaningHistory> UpdateAsync(CleaningHistory cleaningHistory, CancellationToken cancellationToken);
        Task ClearTheHistoryAsync(CancellationToken cancellationToken);
    }
}