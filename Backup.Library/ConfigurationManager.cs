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
        public static ConfigurationResult Read()
        {
            string dataFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            ConfigurationResult retVal = new ConfigurationResult();

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

            string[] lines = File.ReadAllLines(configFilename);
            bool onIgnore = false;
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    
                    if (line.Equals("<IGNORE>"))
                    {
                        onIgnore = true;
                        continue;
                    }
                    string[] data = line.Split(',');
                    if (!onIgnore && data.Length < 2)
                    {
                        retVal.Success = false;
                        throw new System.ArgumentException($"Invalid row in configuration file. line is '{line}'", "original");
                    }
                    if (onIgnore)
                    {
                        retVal.IgnoreFolders.Add(data[0].Trim());
                    }
                    else
                    {
                        retVal.ConfigurationFolders.Add(new Config() { Source = data[0].Trim(), Destination = data[1].Trim() });
                    }
                }
            }
            return retVal;
        }
    }
}
