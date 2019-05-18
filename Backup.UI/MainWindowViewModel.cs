using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Backup.Library;
using Backup.Library.Models;
using GalaSoft.MvvmLight.CommandWpf;

namespace Backup.UI
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public string CurrentFolder { get; set; } = "testing";
        public int TotalFolderFiles { get; set; } = 0;
        public int MaxFilesCurrentFolder { get; set; } = 100;
        public int CurrentCountCurrentFolder { get; set; } = 0;
        public int MaxFolderCount { get; set; }
        public int ProcessedCount { get; set; } = 0;
        public int CopiedCount { get; set; } = 0;
        private BackupDetail previousDetails { get; set; } = new BackupDetail();
        private BackupDetail DetailsTotals { get; set; } = new BackupDetail();

        private List<Config> _configList = null;
        private List<string> _ignoreList = null;
        private FileManager _fileManager = null;
        private bool _cancelFlag { get; set; } = false;
        private bool _pauseFlag { get; set; } = false;


        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            _cancelFlag = false;
            _pauseFlag = false;
            var uiContext = SynchronizationContext.Current;
            LoadSettings();
            _fileManager = new FileManager();
            _fileManager.Callback += new FileManager.CallbackEventHandler(FM_Callback);
            Task t = Task.Run(() =>
            {
                int i = 0;
                foreach (var item in _configList)
                {
                    i++;
                    try
                    {
                        _fileManager.ProcessFolder(item, _ignoreList);
                    }
                    catch (Exception ex)
                    {
                        uiContext.Send(x => UpdateError(ex.Message), null);
                        //throw;
                    }
                    uiContext.Send(X => UpdateUI(currentFolderNumber:i), null);
                    if (_cancelFlag)
                    {
                        break;
                    }
                }
                if (_exitOnEnd)
                {
                    uiContext.Send(x => AppClose(), null);
                }
            });
        }

        private void UpdateError(string message)
        {
            ErrorMessage = message;
        }

        private RelayCommand _cancelCommand { get; set; }

        public RelayCommand CancelCommand
        {
            get
            { return _cancelCommand ?? (
                    _cancelCommand = new RelayCommand(
                   () =>
                   {
                       _exitOnEnd = false;
                       _cancelFlag = true;
                       _fileManager.Cancel();
                       ErrorMessage = "Cancelled";
                   }
                ));
            }
        }

        private RelayCommand _pauseCommand { get; set; }

        public RelayCommand PauseCommand
        {
            get
            {
                return _pauseCommand ?? (
                    _pauseCommand = new RelayCommand(
                     () =>
                     {
                         _pauseFlag = !_pauseFlag;
                         _fileManager.Pause(_pauseFlag);
                         OnPropertyChanged("PauseLabel");
                     }
                  ));
            }
        }

        public string PauseLabel
        {
            get
            {
                return _pauseFlag ? "Resume" : "Pause";
            }
        }

        private bool _exitOnEnd { get; set; } = true;

        private StringBuilder _errorMessage { get; set; } = new StringBuilder();

        public string ErrorMessage
        {
            get
            {
                return _errorMessage.ToString();
            }
            set
            {
                _errorMessage.AppendLine(value);
                OnPropertyChanged("ErrorMessage");
            }
        }
        public void AppClose()
        {
            System.Windows.Application.Current.Shutdown();
        }
        private int _currentFolderNumber { get; set; } = 0;

        public int CurrentFolderNumber {
            get {
                return _currentFolderNumber;
            }
            set
            {
                _currentFolderNumber = value;
                 OnPropertyChanged("CurrentFolderNumber");
            }
        } 

        public void UpdateUI(int currentFolderNumber)
        {
            CurrentFolderNumber = currentFolderNumber;
            // Add the totals to the totals object
            
            DetailsTotals.ProcessedCount += previousDetails.ProcessedCount;
            DetailsTotals.CopiedCount += previousDetails.CopiedCount;
        }

        public void FM_Callback(BackupDetail details)
        {
            switch (details.Type)
            {
                case Enums.BackupDetailType.ProcessFile:
                    previousDetails = details;
                    CurrentFolder = details.CurrentFolder;
                    MaxFilesCurrentFolder = details.MaxFilesFound;
                    CurrentCountCurrentFolder = details.CurrentFileNumber;

                    // Add the overall previous totals to the current counts
                    ProcessedCount = details.ProcessedCount + DetailsTotals.ProcessedCount;
                    CopiedCount = details.CopiedCount + DetailsTotals.CopiedCount;

                    OnPropertyChanged("CurrentFolder");
                    OnPropertyChanged("MaxFilesCurrentFolder");
                    OnPropertyChanged("CurrentCountCurrentFolder");
                    OnPropertyChanged("ProcessedCount");
                    OnPropertyChanged("CopiedCount");
                    break;
                case Enums.BackupDetailType.ResultMessage:
                    ErrorMessage = details.ResultMessage;
                    _exitOnEnd = false;
                    break;
                default:
                    break;
        }
        }

        public void LoadSettings()
        {
            ConfigurationResult result = ConfigurationManager.Read();
            if (result.Success)
            {
                ErrorMessage = result.ResultMessage;
                _exitOnEnd = false;
            }
            _configList = result.ConfigurationFolders;
            _ignoreList = result.IgnoreFolders;
            MaxFolderCount = _configList.Count();
            //MessageBox.Show($"max folder count is {MaxFolderCount}", "main window", MessageBoxButton.OK);

        }

        /// <summary>
        /// Raises the PropertyChanged event for the supplied property.
        /// </summary>
        /// <param name="name">The property name.</param>
        internal void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
