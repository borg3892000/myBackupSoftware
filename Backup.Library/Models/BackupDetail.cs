using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backup.Library.Models
{
    public class BackupDetail
    {
        public int MaxFilesFound { get; set; } = 0;
        public int CurrentFileNumber { get; set; } = 0;
        public string LastFileCopied { get; set; } = "";
        public int ProcessedCount { get; set; } = 0;
        public int CopiedCount { get; set; } = 0;
        public string CurrentFolder { get; set; } = "";
        //public int CurrentFolderNumber { get; set; } = 0;
        public string ResultMessage { get; set; } = string.Empty;
        public Enums.BackupDetailType Type { get; set; } = Enums.BackupDetailType.None;
    }
}
