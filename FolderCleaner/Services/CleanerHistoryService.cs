using FolderCleaner.DataContext;
using FolderCleaner.Models;
using FolderCleaner.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FolderCleaner.Services
{
    public class CleanerHistoryService : ICleanerHistoryService
    {
        private readonly Context _context;

        public CleanerHistoryService(Context context)
        {
            _context = context;
        }

        public async Task<CleanerHistoryModel> CreateAsync(CleanerHistoryModel cleanerHistoryModel, CancellationToken cancellationToken)
        {
            var cleanerHistoryEntry = await _context.CleanerHistories.AddAsync(cleanerHistoryModel, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return cleanerHistoryEntry.Entity;
        }

        public async Task<IEnumerable<CleanerHistoryModel>> GetAllAsync(CancellationToken cancellationToken)
        {
            var cleanerHistories = await _context.CleanerHistories.AsNoTracking().Include("CleanerModel").OrderByDescending(x => x.CreatedOn).ToListAsync(cancellationToken);
            return cleanerHistories;
        }

        public async Task<IEnumerable<CleanerHistoryModel>> GetActiveAllAsync(CancellationToken cancellationToken)
        {
            var cleanerHistories = await _context.CleanerHistories.AsNoTracking()?.Include("CleanerModel").Where(x => x.CleanerModel.IsActive).OrderByDescending(x => x.CreatedOn).ToListAsync(cancellationToken);
            return cleanerHistories.GroupBy(x => x.CleanerId).Select(x => x.FirstOrDefault());
        }

        public async Task<IEnumerable<CleanerHistoryModel>> GetActiveStatusReadyRecordsAsync(CancellationToken cancellationToken)
        {
            var cleanerHistories = await _context.CleanerHistories.AsNoTracking()?.Include("CleanerModel").Where(x => x.CleanerModel.IsActive && x.CleanerStatus == CleanerStatus.Ready).OrderByDescending(x => x.CreatedOn).ToListAsync(cancellationToken);
            return cleanerHistories.GroupBy(x => x.CleanerId).Select(x => x.FirstOrDefault());
        }

        public async Task<CleanerHistoryModel> UpdateAsync(CleanerHistoryModel cleanerHistoryModel, CancellationToken cancellationToken)
        {
            var cleanerHistory = await _context.CleanerHistories.FindAsync(new object[] { cleanerHistoryModel.Id }, cancellationToken);
            cleanerHistory.CleanerStatus = cleanerHistoryModel.CleanerStatus;
            //cleanerHistory.UpdatedOn = DateTime.Now;
            await _context.SaveChangesAsync(cancellationToken);

            return cleanerHistory;
        }
    }
}
