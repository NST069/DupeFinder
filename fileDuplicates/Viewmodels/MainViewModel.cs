using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.IO;
using System.Windows.Forms;

namespace fileDuplicates.Viewmodels
{
    class MainViewModel : INotifyPropertyChanged
    {
        ObservableCollection<Models.FileInfo> _files = new ObservableCollection<Models.FileInfo>();
        public ObservableCollection<Models.FileInfo> Files
        {
            get
            {
                return _files;
            }
            private set
            {
                _files = value;
                OnPropertyChanged(nameof(Files));
            }
        }

        String _path;
        public String Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }

        String _res;
        public String Res
        {
            get { return _res; }
            set
            {
                _res = value;
                OnPropertyChanged(nameof(Res));
            }
        }

        int _filesCount=1;
        public int FilesCount {
            get { return _filesCount; }
            set {
                _filesCount = value;
                OnPropertyChanged(nameof(FilesCount));
            }
        }

        int _progress;
        public int Progress {
            get { return _progress; }
            set {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        public ICommand Open {
            get {
                return new Models.DelegateCommand((obj)=> {
                    using (var fbd = new FolderBrowserDialog())
                    {
                        DialogResult result = fbd.ShowDialog();

                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                        {
                            Path = fbd.SelectedPath;
                        }
                    }
                });
            }
        }

        public ICommand Submit {
            get
            {
                return new Models.DelegateCommand(async (obj) =>
                {
                    Files.Clear();
                    Res = "";
                    FilesCount = 0;
                    Progress = 0;
                    await GetFiles(Path);
                    var duplicatesByContent = Files.GroupBy(s => s.checksum)
                                                 .Where(g => g.Count() > 1)
                                                 .SelectMany(g => g);
                    if (duplicatesByContent.Count() > 0)
                    {
                        Res += "\n=====Duplicates with same content: ";
                        foreach (var x in duplicatesByContent.ToArray())
                        {
                            Res += "\n" + x.parent + "\\" + x.name;
                        }
                    }
                    var duplicatesByName = Files.GroupBy(s => s.name)
                                                 .Where(g => g.Count() > 1)
                                                 .SelectMany(g => g);
                    if (duplicatesByName.Count() > 0)
                    {
                        Res += "\n=====Duplicates with same name: ";
                        foreach (var x in duplicatesByName.ToArray()) {
                            Res += "\n" + x.parent + "\\" + x.name;
                        }
                    }
                });
            }
        }

        async Task<int> GetFiles(String directory) {
            int filesInDir = Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly).Count();
            Res += "Folder: " + directory + "\tFiles: "+filesInDir+"\n";
            FilesCount += filesInDir;
            foreach (String x in Directory.GetFiles(directory)) {
                System.Threading.ThreadPool.QueueUserWorkItem(async (obj) =>
                {
                    Models.FileInfo file = await System.Threading.Tasks.Task.Run(() => new Models.FileInfo(new System.IO.FileInfo(x)));
                    Files.Add(file);
                    Progress++;
                    Res += "Progress: " + Progress + "/" + FilesCount + "\n";
                });

            }
            foreach (String x in Directory.GetDirectories(directory)) {
                try
                {
                    await GetFiles(x);
                    
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("Can\'t get access to " + x);
                    
                }
                catch (Exception e) {
                    MessageBox.Show(e.Message);
                }
            }
            return 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] String info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
