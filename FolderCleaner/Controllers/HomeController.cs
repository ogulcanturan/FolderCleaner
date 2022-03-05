using FolderCleaner.Worker.Enums;
using FolderCleaner.Worker.Models;
using FolderCleaner.Worker.Models.ViewModels;
using FolderCleaner.Worker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FolderCleaner.Worker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICleaningRecordService _cleaningRecordService;
        private readonly ICleaningHistoryService _cleaningHistoryService;
        private readonly IFileService _fileService;
        public HomeController(ICleaningRecordService cleaningRecordService, ICleaningHistoryService cleaningHistoryService, IFileService fileService)
        {
            _cleaningRecordService = cleaningRecordService;
            _cleaningHistoryService = cleaningHistoryService;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var list = await _cleaningRecordService.GetAllAsync(cancellationToken);
            return View(new IndexPageViewModel
            {
                CollectionOfCleaningRecords = list,
                CleaningRecord = new CleaningRecord
                {
                    IsActive = true,
                    RunsAt = DateTime.Now,
                    Repeat = true,
                    Time = Time.Day,
                }
            });
        }

        public async Task<IActionResult> History(CancellationToken cancellationToken)
        {
            var allHistory = await _cleaningHistoryService.GetAllAsync(cancellationToken);
            var statusReadyHistories = await _cleaningHistoryService.GetActiveStatusReadyRecordsAsync(cancellationToken);
            return View(new List<IEnumerable<CleaningHistory>> { statusReadyHistories, allHistory });
        }

        [HttpPost]
        public async Task<IActionResult> AddCleaningRecord(IndexPageViewModel indexPageViewModel, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                await _cleaningRecordService.CreateAsync(indexPageViewModel.CleaningRecord, TriggeredBy.Worker, cancellationToken);
                TempData["SuccessMessage"] = "Success!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCleaningRecord(int id, CancellationToken cancellationToken)
        {
            await _cleaningRecordService.DeleteByIdAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Success!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SwitchCleaningRecordActivity(int id, CancellationToken cancellationToken)
        {
            await _cleaningRecordService.SwitchActivityAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Success!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ManualCleanUp(int id, CancellationToken cancellationToken)
        {
            var cleaningRecord = await _cleaningRecordService.GetByIdAsync(id, cancellationToken);
            if (cleaningRecord != null)
            {
                try
                {
                    int? totalFiles = cleaningRecord.TotalFiles;
                    long? cleaningSize = cleaningRecord.CleaningSize;
                    _fileService.Delete(cleaningRecord.Path);

                    await _cleaningHistoryService.CreateAsync(new CleaningHistory
                    {
                        RunsAt = DateTime.Now,
                        CleaningSize = cleaningSize,
                        TotalFiles = totalFiles,
                        CleaningStatus = CleaningStatus.Success,
                        CleaningRecordId = id,
                        TriggeredBy = TriggeredBy.User,
                    }, cancellationToken);

                    TempData["SuccessMessage"] = "Success!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> ClearTheHistory(CancellationToken cancellationToken)
        {
            await _cleaningHistoryService.ClearTheHistoryAsync(cancellationToken);
            return RedirectToAction(nameof(History));
        }
    }
}