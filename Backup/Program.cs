using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backup.Library;
using Backup.Library.Models;

namespace Backup
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigurationResult result = ConfigurationManager.Read();
            FileManager fileManager = new FileManager();

            int cc = result.ConfigurationFolders.Count();
            int i = 0;
            foreach (var item in result.ConfigurationFolders)
            {
                Console.WriteLine($"Backing up {i} of {cc}: {item.Source}");
                fileManager.ProcessFolder(item, result.IgnoreFolders);
            }
            Debugger.Break();
        }
    }
}
