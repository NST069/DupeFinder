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

        public ICommand Open {
            get {
                return new Models.DelegateCommand((obj)=> {
                    using (var fbd = new FolderBrowserDialog())
                    {
                        DialogResult result = fbd.ShowDialog();

                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                        {
                            //string[] files = Directory.GetFiles(fbd.SelectedPath);

                            //.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");
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
                    String[] fileEntries = Directory.GetFiles(Path, "*", SearchOption.AllDirectories);
                    foreach (String x in fileEntries)
                    {
                        Models.FileInfo file = await System.Threading.Tasks.Task.Run(() => new Models.FileInfo(new System.IO.FileInfo(x)));
                        Files.Add(file);
                        Res += file.checksum + '\n';
                    }
                    //var dupes = Files.GroupBy(x => new { x.checksum, x.name })
                    //    .Where(x => x.Skip(1).Any());
                    //if (dupes.Any())
                    //{
                    //    Res += "\n\n=====DUPES=====\n";
                    //    foreach (var x in dupes)
                    //    {
                    //        Res += "checksum = " + x.Key.checksum + " name = " + x.Key.name + " has " + (x.Count() - 1) + " duplicates";
                    //    }
                    //}
                    var duplicates = Files.GroupBy(s => s.checksum)
                                                 .Where(g => g.Count() > 1)
                                                 .SelectMany(g => g);
                    if (duplicates.Count() > 0)
                    {
                        Res += "\n=====Duplicates with same content: ";
                        foreach (var x in duplicates.ToArray())
                        {
                            Res += "\n" + x.parent + "\\" + x.name;
                        }
                    }
                    duplicates = Files.GroupBy(s => s.name)
                                                 .Where(g => g.Count() > 1)
                                                 .SelectMany(g => g);
                    if (duplicates.Count() > 0)
                    {
                        Res += "\n=====Duplicates with same name: ";
                        foreach (var x in duplicates.ToArray()) {
                            Res += "\n" + x.parent + "\\" + x.name;
                        }
                    }
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] String info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
