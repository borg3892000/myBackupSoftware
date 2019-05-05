using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Backup.Library.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Backup.Library
{
    public class ConfigurationManager
    {

        //public const string DATAFOLDER = @"c:\Program Files\MyBackupApp";
        public static ConfigurationResult Read()
        {
            //string DATAFOLDER = Application.StartupPath;
            string dataFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            ConfigurationResult retVal = new ConfigurationResult();

            //List<Config> retVal = new List<Config>();
            if (!Directory.Exists(dataFolder))
            {
                throw new System.ArgumentException($"Program Folder {dataFolder} doesn't exist.", "original");
            }
            string configFilename = $"{dataFolder}\\backup.cfg";
            if (!File.Exists(configFilename))
            {
               // MessageBox.Show($"Unable to read config file", "Startup Folder", MessageBoxButton.OK);
                throw new System.ArgumentException($"Configuration file {configFilename} doesn't exist.", "original");
            }
            //MessageBox.Show($"About to read the config file", "configuration", MessageBoxButton.OK);
            string[] lines = File.ReadAllLines(configFilename);
            foreach (var line in lines)
            {
                //MessageBox.Show($"Read a line", "configuration", MessageBoxButton.OK);
                if (!string.IsNullOrWhiteSpace(line))
                {
                    string[] data = line.Split(',');
                    if (data.Length < 2)
                    {
                        //MessageBox.Show($"invalid row in file.  line is '{line}'", "configuration", MessageBoxButton.OK);
                        retVal.Success = false;
                        throw new System.ArgumentException($"Invalid row in configuration file. line is '{line}'", "original");
                    }
                    retVal.ConfigurationFolders.Add(new Config() { Source = data[0].Trim(), Destination = data[1].Trim() });
                }
            }
            //int cc = retVal.ConfigurationFolders.Count();
            //MessageBoxResult result = MessageBox.Show($"{dataFolder} has {cc} folders.", "Startup Folder", MessageBoxButton.OK);
            return retVal;
        }
    }
}
