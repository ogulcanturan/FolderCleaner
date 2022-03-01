using FolderCleaner.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FolderCleaner.Services.Interfaces
{
    public interface ICleanerHistoryService
    {
        Task<IEnumerable<CleanerHistoryModel>> GetAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<CleanerHistoryModel>> GetActiveAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<CleanerHistoryModel>> GetActiveStatusReadyRecordsAsync(CancellationToken cancellationToken);
        Task<CleanerHistoryModel> CreateAsync(CleanerHistoryModel cleanerHistoryModel, CancellationToken cancellationToken);
        Task<CleanerHistoryModel> UpdateAsync(CleanerHistoryModel cleanerHistoryModel, CancellationToken cancellationToken);
    }
}