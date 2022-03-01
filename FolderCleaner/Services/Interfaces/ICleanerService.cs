using FolderCleaner.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FolderCleaner.Services.Interfaces
{
    public interface ICleanerService
    {
        Task<IEnumerable<CleanerModel>> GetAllAsync(CancellationToken cancellationToken);
        Task<CleanerModel> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<CleanerModel> CreateAsync(CleanerModel cleanerModel, CancellationToken cancellationToken);
        Task<CleanerModel> UpdateAsync(CleanerModel cleanerModel, CancellationToken cancellationToken);
        Task<CleanerModel> SwitchActivityAsync(int id, CancellationToken cancellationToken);
        Task DeleteByIdAsync(int id, CancellationToken cancellationToken);
    }
}