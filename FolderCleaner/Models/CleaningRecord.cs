using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FolderCleaner.Worker.Models
{
    public class CleaningRecord
    {
        public int Id { get; set; }
        [Required]
        public string Path { get; set; }
        public DateTime RunsAt { get; set; }
        
        public bool IsActive { get; set; }
        public bool Repeat { get; set; }
        [NotMapped]
        public Enums.Time? Time { get; set; }

        public int? repeatRange;
        public int? RepeatRange
        {
            get
            {
                return repeatRange;
            }
            set
            {
                if(value != null && Time != null)
                {
                    repeatRange = value * TimeToSecond(Time);
                }
                else // If repeat enabled however, repeatRange is null then make sure repeat is disable.
                {
                    Repeat = false;
                }
            }
        }
        public virtual ICollection<CleaningHistory> CleaningHistory { get; set; }


        public int? TotalFiles => Services.FileService.TotalFiles(Path);
        public long? CleaningSize => Services.FileService.GetSize(Path);

        public static int? TimeToSecond(Enums.Time? time)
        {
            switch (time)
            {
                case Enums.Time.Second:
                    return 1;
                case Enums.Time.Minute:
                    return 60;
                case Enums.Time.Hour:
                    return 3600;
                case Enums.Time.Day:
                    return 86400;
            }
            return null;
        }
    }
}