using System;
using System.Collections.Generic;

namespace FolderCleaner.Models
{
    public class CleanerModel
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public DateTime WorksAt { get; set; }
        //public bool Repeat { get; set; }
        public int TotalFiles 
        {
            get
            {
                if (System.IO.Path.HasExtension(Path))
                {
                    if (System.IO.File.Exists(Path))
                        return 1;
                    return 0;
                }
                return System.IO.Directory.GetFileSystemEntries(Path).Length;
            }
        } 
        public bool IsActive { get; set; }
        public virtual ICollection<CleanerHistoryModel> CleanerHistories { get; set; }
    }
}