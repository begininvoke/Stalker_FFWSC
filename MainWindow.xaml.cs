using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using DataFormats = System.Windows.Forms.DataFormats;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace FFWSC
{
    /// <summary>
    /// github page https://github.com/zapezhman
    /// </summary>
    public partial class MainWindow : Window, System.Windows.Forms.IWin32Window
    {
        private string hash;
        private string Hashfile_string
        {
            get { return hash; }
            set { hash = Gethash(value); }
        }
        public long Lentgh { get; set; }
        public string Custom_directory { get; set; }

        public int Max { get; set; }
  
        Queue<string> Allfiles = new Queue<string>();
        public MainWindow()
        {
            this.DataContext = this;
            InitializeComponent();

        }
        public IntPtr Handle
        {
            get
            {
                return ((HwndSource)PresentationSource.FromVisual(this)).Handle;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                TXTaddress.Text = openFileDialog.FileName;
                FileInfo fileinfo = new FileInfo(openFileDialog.FileName);
                Lentgh = fileinfo.Length;
                Hashfile_string = openFileDialog.FileName;
                ListView.Items.Clear();

            }
        }

        public async Task FullDirList()
        {
            while (Allfiles.Count <= 0)
            {
                await Task.Run(() => Thread.Sleep(3000)).ConfigureAwait(true);
            }
            int retry = 0;
        Gotry:
            while (Allfiles.Count > 0)
            {
                try
                {
                    string f = Allfiles.Dequeue();
                    FileInfo xx = new FileInfo(f);
                    if (xx.Length == Lentgh)
                    {
                        if (Gethash(xx.FullName) == Hashfile_string)
                        {
                            Addlistfile(xx.FullName, 1);
                            continue;
                        }
                    }
                    continue;
                }
                catch
                {}

            }
            await Task.Run(() => Thread.Sleep(3000)).ConfigureAwait(true);
            retry++;
            if (retry < 2)
                goto Gotry;


        }


        public async Task Traverse(string rootDirectory)
        {
            IEnumerable<string> files = Enumerable.Empty<string>();
            IEnumerable<string> directories = Enumerable.Empty<string>();
            try
            {
                // The test for UnauthorizedAccessException.
                var permission = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, rootDirectory);
                permission.Demand();

                files = Directory.GetFiles(rootDirectory);
                directories = Directory.GetDirectories(rootDirectory);
            }
            catch
            {
                // Ignore folder (access denied).
                
            }

            try
            {
                foreach (var file in files)
                {
                    Allfiles.Enqueue(file);
                }
                foreach (var item in directories)
                {
                    _ = Traverse(item);
                }
            }
            catch (Exception)
            {


            }


        }



        private void UpdatePrograssBar(int max)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if (this.Max < max)
                    {
                        this.Max = max;

                    }

                    PRque.Maximum = this.Max; PRque.Value = max;
                }));
            }
            else
            {
                if (this.Max < max)
                {
                    this.Max = max;
                }
                PRque.Maximum = this.Max; PRque.Value = max;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"> 1 = red , 2 = Green ,3 = blue</param>
        /// <returns></returns>
        private static string Color_name(int color)
        {
            switch (color)
            {
                case (1):
                    return "Red";

                case (2):
                    return "Green";

                case (3):
                    return "Blue";

                default:
                    return "red";

            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="founded"> address </param>
        /// <param name="type_status"> 1 = detect || 2 = clean || 3 = info </param>
        /// <param name="color">  1 = red || 2 = green || 3 = Blue  </param>
        private void Addlistfile(string founded, int color = 1)
        {

            if (!CheckAccess())
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    ListView.Items.Add(new Status() { File = founded, Color = Color_name(color) });


                }));
            }
            else
            {
                ListView.Items.Add(new Status() { File = founded, Color = Color_name(color) });

            }


        }
        private async Task Time_Cheker()
        {
            await Task.Run(() => Thread.Sleep(5000)).ConfigureAwait(true);
            try
            {
                while (Allfiles.Count > 0)
                {
                    UpdatePrograssBar((int)Allfiles.Count);
                    await Task.Run(() => Thread.Sleep(2000)).ConfigureAwait(true);
                    if (!CheckAccess())
                    {
                        Dispatcher.Invoke(new Action(() => { LBLqu.Content = Allfiles.Count; }));
                    }

                };
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

            EnableControler();

        }

        //Get Hash String 
        private string Gethash(string filename)
        {
            using (var sha1 = SHA1.Create())
            {
                byte[] fileData = null;
                using (var stream = File.OpenRead(filename))
                {
                    using (var binaryReader = new BinaryReader(stream))
                    {
                        fileData = binaryReader.ReadBytes((int)Lentgh);


                        return BitConverter.ToString(sha1.ComputeHash(fileData))
                                           .Replace("-", "")
                                           .ToLowerInvariant();
                    }
                    ;

                }
            }
        }
        private void BTNselectdir_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                var result = fbd.ShowDialog(this);

                Custom_directory = fbd.SelectedPath;
                CHmydir.IsChecked = true;

            }
        }

        private async Task TaskOne(string dir)
        {
            _ = Task.Run(() => Traverse(@dir));
        }
        private void EnableControler()
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() =>
                {
                    BTNscan.IsEnabled = true;
                    BTNselectdir.IsEnabled = true;
                    BTNchoice.IsEnabled = true;
                    PRque.Value = 0;
                    BTNclean.IsEnabled = true;
                    BTNFound.Content = $"Found : {ListView.Items.Count}";
                    BTNFound.Visibility = Visibility.Visible;
                });
            }
            else
            {
                BTNscan.IsEnabled = true;
                BTNselectdir.IsEnabled = true;
                BTNchoice.IsEnabled = true;
                PRque.Value = 0;
                BTNclean.IsEnabled = true;
                BTNFound.Content = $"Found : {ListView.Items.Count}";
                BTNFound.Visibility = Visibility.Visible;
            }

        }
        private void DisableControler()
        {
            BTNscan.IsEnabled = false;
            BTNselectdir.IsEnabled = false;
            BTNchoice.IsEnabled = false;
            BTNclean.IsEnabled = false;
            BTNFound.Visibility = Visibility.Hidden;
        }
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ListView.Items.Clear();
            DisableControler();
            if ((bool)CHmydir.IsChecked)
            {
                _ = TaskOne(Custom_directory);

            }
            else
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();

                foreach (DriveInfo d in allDrives)
                {
                    if (d.IsReady == true)
                    {

                        _ = TaskOne(d.Name);

                    }
                }
            }

            PRque.Value = 0;
            _ = Task.Run(() => Time_Cheker());
            _ = Task.Run(() => FullDirList());
        }



        private void CHmydir_Checked(object sender, RoutedEventArgs e)
        {

        }
        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        public static void LocalProcessKill(string processName)
        {
            foreach (Process p in Process.GetProcessesByName(processName))
            {
                p.Kill();
            }
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < ListView.Items.Count; i++)
            {
                try
                {
                    var status = (Status)ListView.Items[i];
                    FileInfo file = new FileInfo(status.File);

                    if (file.Exists && status.Color == "Red")
                    {
                        if (IsFileLocked(file))
                        {

                            List<Process> lstProcs = new List<Process>();

                            lstProcs = FileUtil.WhoIsLocking(file.ToString());
                            if (lstProcs.Count > 0) // deal with the file lock
                            {
                                foreach (Process p in lstProcs)
                                {

                                    LocalProcessKill(p.ProcessName);
                                }
                                File.Delete(file.ToString());
                            }
                            else
                                File.Delete(file.ToString());
                        }
                        if (file.Exists)
                        {
                            file.Delete();
                        }

                        ListView.Items[i] = new Status() { File = status.File, Color = Color_name(2) };
                    }


                }
                catch (Exception)
                {

               
                }



            }



        }

        private void ListView_MouseClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                var file = (Status)ListView.SelectedItem;
                string path = file.File;
                string folder = Path.GetDirectoryName(path);
                Process.Start("explorer.exe", folder);
            }
            catch (Exception)
            {


            }
        }



        private void FFWSC_Activated(object sender, EventArgs e)
        {

        }

        private void FFWSC_Drop(object sender, System.Windows.DragEventArgs e)
        {
            var stringsDrags = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (stringsDrags != null)
            {
                TXTaddress.Text = stringsDrags[0];
                FileInfo fileinfo = new FileInfo(stringsDrags[0]);
                Lentgh = fileinfo.Length;
                Hashfile_string = fileinfo.FullName;
                ListView.Items.Clear();
            }
        }
    }
}