using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backup.Library.Models
{
    public class ConfigurationResult
    {
        public bool Success { get; set; } = false;
        public string ResultMessage { get; set; }
        public List<Config> ConfigurationFolders { get; set; } = new List<Config>();
        public List<string> IgnoreFolders { get; set; } = new List<string>();
    }
}
