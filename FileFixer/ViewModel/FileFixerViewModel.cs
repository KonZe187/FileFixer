using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
//using System.Windows;
using System.Windows.Input;
using FileFixer.MVVM;
using System.Windows;
using System.ComponentModel;
using System.Threading;
using System.IO;

namespace FileFixer.ViewModel
{
    internal class FileFixerViewModel : BindableBase
    {

        private string _folderPath = "";

        public string FolderPath
        {
            get { return _folderPath; }
            set { SetProperty(ref _folderPath, value); }
        }

        private bool _useSubfolders;

        public bool UseSubfolders
        {
            get { return _useSubfolders; }
            set { SetProperty(ref _useSubfolders, value); }
        }

        private string _fileFilter;

        public string FileFilter
        {
            get { return _fileFilter; }
            set { SetProperty(ref _fileFilter, value); }
        }

        private int _currentProgress = 0;

        public int CurrentProgress
        {
            get { return _currentProgress; }
            set { SetProperty(ref _currentProgress, value); }
        }

        private int _maxProgress = 100;

        public int MaxProgress
        {
            get { return _maxProgress; }
            set { SetProperty(ref _maxProgress, value); }
        }

        private ICommand _startClick;
        public ICommand StartClick
        {
            get
            {
                if (_startClick == null)
                {
                    _startClick =
                        new RelayCommand(
                            save => FixFiles(), () => true);
                }
                return _startClick;
            }
        }

        private ICommand _chooseFolderClick;
        public ICommand ChooseFolderClick
        {
            get
            {
                if (_chooseFolderClick == null)
                {
                    _chooseFolderClick =
                        new RelayCommand(
                            save => ChooseFolder(), () => true);
                }
                return _chooseFolderClick;
            }
        }

        private BackgroundWorker worker;


        public FileFixerViewModel()
        {
            try
            {
                FileFilter = Properties.Settings.Default.FileFilter;
                FolderPath = Properties.Settings.Default.FolderPath;
                UseSubfolders = Properties.Settings.Default.UseSubfolders;

                worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += DoWork;
                worker.ProgressChanged += ProgressChanged;
                worker.RunWorkerCompleted += WorkerFinished;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            SearchOption searchOption;

            if (_useSubfolders)
            {
                searchOption = SearchOption.AllDirectories;
            }
            else
            {
                searchOption = SearchOption.TopDirectoryOnly;
            }

            var files = Directory.GetFiles(FolderPath, FileFilter, searchOption);

            MaxProgress = files.Count();
            int progress = 0;

            foreach(string path in files)
            {
                progress += 1;

                string readText = File.ReadAllText(path);
                File.WriteAllText(path, readText + " ");

                readText = File.ReadAllText(path);
                File.WriteAllText(path, readText.Remove(readText.Length - 1));

                (sender as BackgroundWorker).ReportProgress(progress);
            }
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            CurrentProgress = e.ProgressPercentage;
        }

        private void WorkerFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show($"Fertig! {MaxProgress} Dateien neu geschrieben.");
        }

        private void FixFiles()
        {
            Properties.Settings.Default.FileFilter = FileFilter;
            Properties.Settings.Default.FolderPath = FolderPath;
            Properties.Settings.Default.UseSubfolders = UseSubfolders;
            Properties.Settings.Default.Save();

            worker.RunWorkerAsync();
        }

        private void ChooseFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = string.IsNullOrEmpty(FolderPath) ? "C:\\" : FolderPath; 
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                FolderPath = dialog.SelectedPath;
            }
        }

    }
}
