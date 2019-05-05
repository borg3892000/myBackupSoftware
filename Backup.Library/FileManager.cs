using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backup.Library.Models;

namespace Backup.Library
{
    public class FileManager
    {
        public delegate void CallbackEventHandler(BackupDetail details);
        public event CallbackEventHandler Callback;
        private bool _cancelFlag { get; set; } = false;
        private bool _pauseFlag { get; set; } = false;

        public void Cancel()
        {
            _cancelFlag = true;
        }

        public void Pause(bool pause)
        {
            _pauseFlag = pause;
        }

        public void ProcessFolder(Config config)
        {
            if (!Directory.Exists(config.Source))
            {
                throw new System.ArgumentException($"Error.  Source folder does not exist.  {config.Source}", "original");
            }
            List<string> folders = new List<string>();
            folders.Add(config.Source);
            folders.AddRange(Directory.GetDirectories(config.Source, "*.*", SearchOption.AllDirectories));
            foreach (string folder in folders)
            {
                Debug.WriteLine($"Scanning folder {folder} for files");
                string[] files = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly);
                Debug.WriteLine($"Done scanning folder {folder} for files.  Calling ProcessFolder.");
                try
                {
                    ProcessFolder(config, files);
                }
                catch (Exception ex)
                {
                    BackupDetail details = new BackupDetail();
                    details.Type = Enums.BackupDetailType.ResultMessage;
                    details.ResultMessage = $"Error: {ex.Message}\r\n{ex.InnerException}";
                    if (Callback != null)
                        Callback(details);
                }
                if (_cancelFlag)
                {
                    break;
                }
                while (_pauseFlag)
                {
                    // Pause the thread for a second until the user releases it
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// Process the found files
        /// </summary>
        /// <param name="config"></param>
        /// <param name="files"></param>
        public void ProcessFolder(Config config, string[] files)
        { 
            int cc = files.Count();
            //Console.WriteLine($"Comparing {cc} files");
            Debug.WriteLine($"Processing {cc} files");
            //int copied = 0;
            BackupDetail details = new BackupDetail();
            details.MaxFilesFound = files.Count();
            details.CurrentFileNumber = 0;
            foreach (var item in files)
            {
                string itemFolder = Path.GetDirectoryName(item);
                details.CurrentFileNumber++;
                details.CurrentFolder = itemFolder;
                details.Type = Enums.BackupDetailType.ProcessFile;
                if (Callback != null)
                    Callback(details);


                // In order to correctly do subfolders, we need to copy it to the matching subfolder on the destination
                // If the file is "sourcefolder\foo\myfile.txt", we need to copy it to "destinationfolder\foo\myfile.txt"
                //   so we need to take the directory name and remove the sourcefolder part
                int sourceFolderLength = config.Source.Length;
                if (itemFolder.Length < sourceFolderLength)
                {
                    details.Type = Enums.BackupDetailType.ResultMessage;
                    details.ResultMessage = $"Error.Length of found file is too short!";
                    if (Callback != null)
                        Callback(details);
                    //throw new System.ArgumentException($""Error.  Length of found file is too short!"", "original");
                }
                // If the length is identical, it's not a subfolder
                // If the length is greater, it is a subfolder, and we only want everything after the first X characters (x being the length variable)
                int itemFolderLength = itemFolder.Length;
                string itemName = item;
                if (itemFolder.Length > sourceFolderLength)
                {
                    // Just grab everything from "\foo" and after from "sourcefolder\foo\myfile.txt"
                    itemName = item.Substring(sourceFolderLength+1);
                } else
                {
                    itemName = Path.GetFileName(item);
                }

                string filename = Path.GetFileName(item);
                string destinationName = $"{config.Destination}\\{itemName}";
                string destinationFolder = Path.GetDirectoryName(destinationName);
                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }
                if (!FileMatch(item, destinationName))
                {
                    details.LastFileCopied = destinationName;
                    FileCopy(item, destinationName);
                    details.CopiedCount++;
                }
                details.ProcessedCount++;
                details.Type = Enums.BackupDetailType.ProcessFile;
                if (Callback != null)
                    Callback(details);

            }
            //Console.WriteLine($"Updated {copied} items");
        }

        public void FileCopy(string sourceFile, string destinationFile)
        {
            //Console.WriteLine($"copying file {sourceFile} to {destinationFile}");
            File.Copy(sourceFile, destinationFile, true);
        }

        public bool FileMatch(string sourceFile, string destinationFile)
        {
            //if the file doesn't even exist on the target machine, it's not a match
            if (!File.Exists(destinationFile))
            {
                return false;
            }

            FileInfo sourceFileinfo = new System.IO.FileInfo(sourceFile);
            FileInfo destinationFileinfo = new System.IO.FileInfo(destinationFile);
            //if the file lengths are different, it's not a match
            if (sourceFileinfo.Length != destinationFileinfo.Length)
            {
                return false;
            }

            if (sourceFileinfo.LastWriteTime != destinationFileinfo.LastWriteTime)
            {
                return false;
            }
            //at this point, we're assuming it's identical if the filename and length is the same
            return true;
        }
    }
}
