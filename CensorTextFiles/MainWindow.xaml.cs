using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace CensorTextFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, List<string>> _filesInFolder;
        private List<string> _censorFile;
        public string FolderName { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var folder = new FolderBrowserDialog();
            var result = folder.ShowDialog();

            this.TextBoxLocationFolder.Text = folder.SelectedPath;

            FolderName = folder.SelectedPath;
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var file = new OpenFileDialog();
            var result = file.ShowDialog();

            this.TextBoxLocationFile.Text = System.IO.Path.GetFullPath(file.FileName);

            _censorFile = ReadFileLines(file.FileName);
        }

        private void ButtonCensorFiles_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                
                _filesInFolder = GetAllFiles(FolderName);
                _filesInFolder = CensorFiles(_filesInFolder);
                WriteChangesToFiles(_filesInFolder);
                
            }
            catch (Exception)
            {
                throw new Exception();
            }
            finally
            {
                stopwatch.Stop();
                string result = string.Format("Done\nEditing {0} files in {1}", _filesInFolder.Count, stopwatch.Elapsed);
                MessageBoxResult message = System.Windows.MessageBox.Show(result, "Result", MessageBoxButton.OK, MessageBoxImage.Information);
            }


        }

        private Dictionary<string, List<string>> GetAllFiles(string folder)
        {
            try
            {
                _filesInFolder = new Dictionary<string, List<string>>();
                string[] files = Directory.GetFiles(folder, "*.txt").ToArray();

                foreach (var file in files)
                {
                    lock (file)
                    {
                        var fileLines = ReadFileLines(file);
                        _filesInFolder.Add(file, fileLines);
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception();
            }

            return _filesInFolder;
        }

        private List<string> ReadFileLines(string file)
        {
            List<string> fileLines = new List<string>();

            try
            {
                StreamReader reader = new StreamReader(file);
                string line = null;

                using (reader)
                {
                    line = reader.ReadLine();

                    while (line != null)
                    {
                        fileLines.Add(line);
                        line = reader.ReadLine();
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception();
            }

            return fileLines;
        }

        private Dictionary<string, List<string>> CensorFiles(Dictionary<string, List<string>> filesInFolder)
        {
            var edit = new Dictionary<string, List<string>>();

            foreach (var file in filesInFolder)
            {
                List<string> censor = file.Value;


                CensorFileFromDirectory(ref censor);

                edit.Add(file.Key, censor);
            }

            return edit;
        }

        private void CensorFileFromDirectory(ref List<string> fileLines)
        {
            try
            {
                //{
                //    foreach (var censor in _censorFile)
                //    {
                //        var replace = from line in fileLines
                //                      where line.Contains(censor)
                //                      select line.Replace(censor, new string('*', censor.Length - 1));



                //        fileLines = replace.ToList();
                //    }

                int fileLinesLength = fileLines.Count;
                int censorFileLength = _censorFile.Count;

                for (int i = 0; i < fileLinesLength; i++)
                {
                    for (int j = 0; j < censorFileLength; j++)
                    {
                        if (fileLines[i].Contains(_censorFile[j]))
                        {
                            fileLines[i] = fileLines[i].Replace(_censorFile[j], new string('*', _censorFile[j].Length));
                        }
                    }
                }

            }
            catch (Exception)
            {
                throw new Exception();
            }
        }

        private void WriteChangesToFiles(Dictionary<string, List<string>> filesInFolder)
        {
            try
            {
                foreach (var file in filesInFolder)
                {
                    lock (file.Value)
                    {
                        WriteToFile(file.Key, file.Value);
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }

        private async void WriteToFile(string fileName, List<string> fileLines)
        {
            try
            {
                StreamWriter writer = new StreamWriter(fileName);

                using (writer)
                {
                    foreach (var item in fileLines)
                    {
                        await writer.WriteLineAsync(item);
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }
    }
}
