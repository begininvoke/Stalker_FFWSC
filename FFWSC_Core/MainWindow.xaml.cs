using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading;
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

namespace FFWSC_Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
   
        
        public MainWindow()
        {
 
            InitializeComponent();
            SetValue(MainWindow.TitleProperty, " FFWSC Antivirus " + Antivirus_name);
            if (Autoscan == "true")
            {
                BTNscan.IsEnabled = false;
                BTNclean.IsEnabled = false;
                Start();

            }
            if (Startup == "true")
            {
                AddToRegistry();
            }
        }




        public  string hash = "{hash}";
        //public string hash = "fafe9c2c410aed31084c98892ad05237f709de81";
        //public string Lentgh { get; set; } = "1179024";
        public string Lentgh { get; set; } = "{Len}";
        public string Custom_directory { get; set; } = "{customdirectory}";
        //public string Custom_directory { get; set; } = "C:\\Users";
        public string Startup = "{startup}";
        //public string Startup = "false";
        public string Autoscan = "{autoscan}";
        //public string Autoscan = "false";
        public string Antivirus_name = "{name}";
        //public string Antivirus_name = "putty.exe_Stalker_Cleaner";
        public int Max { get; set; }

        Queue<string> Allfiles = new Queue<string>();
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
                    if (xx.Length == long.Parse(Lentgh))
                    {
                        if (Gethash(xx.FullName) == hash)
                        {
                            Addlistfile(xx.FullName, 1);
                            continue;
                        }
                    }
                    continue;
                }
                catch
                { }

            }
            await Task.Run(() => Thread.Sleep(10000)).ConfigureAwait(true);
            retry++;
            if (retry < 7)
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
                    await Task.Run(() => Thread.Sleep(3000)).ConfigureAwait(true);
                    _ = Traverse(item);
                }
            }
            catch (Exception)
            {


            }


        }

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
        public static void AddToRegistry()
        {
            try
            {
                //System.IO.File.Copy(System.Reflection.Assembly.GetEntryAssembly().Location, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\FFWCS\" + "FFWCS_Core.exe");
                RegistryKey RegStartUp = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                RegStartUp.SetValue("FFWCS_Core", System.Reflection.Assembly.GetEntryAssembly().Location) ;
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message);}
        }

        int PRvalue = 0;
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
                    int newvalue = this.Max - max;
                    if (newvalue > PRvalue)
                    {
                        PRvalue = newvalue;
                   PRmain.Maximum = this.Max;
                        PRmain.Value = PRvalue;
                    }
                   

                }));
            }
            else
            {
                if (this.Max < max)
                {
                    this.Max = max;
                }
                int newvalue = this.Max - max;
                if (newvalue > PRvalue)
                {
                    PRvalue = newvalue;
                    PRmain.Maximum = this.Max;
                    PRmain.Value = PRvalue;
                }
            }

        }
        private async Task Time_Cheker()
        {
            await Task.Run(() => Thread.Sleep(5000)).ConfigureAwait(true);
            try
            {
                while (Allfiles.Count > 0)
                {
                    
                    await Task.Run(() => Thread.Sleep(2000)).ConfigureAwait(true);
                    UpdatePrograssBar(Allfiles.Count);
                    if (!CheckAccess())
                    {
                        Dispatcher.Invoke(new Action(() => { LBLque.Content = Allfiles.Count; }));
                    }

                };
            }
            catch (Exception)
            {
                
            }
            if (!CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => {
                    if (Autoscan == "true")
                    {
                        Clean();
                        LBLque.Content = "Scan Complate.....";
                        System.Media.SystemSounds.Beep.Play();
                    }

                }));
            }
           

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
                        fileData = binaryReader.ReadBytes(int.Parse(Lentgh));


                        return BitConverter.ToString(sha1.ComputeHash(fileData))
                                           .Replace("-", "")
                                           .ToLowerInvariant();
                    }
                    ;

                }
            }
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
        private async Task TaskOne(string dir)
        {
            _ = Task.Run(() => Traverse(@dir));
        }
        public static void LocalProcessKill(string processName)
        {
            foreach (Process p in Process.GetProcessesByName(processName))
            {
                p.Kill();
            }
        }

        private void Start()
        {
            ListView.Items.Clear();
            //DisableControler();
            if (!string.IsNullOrEmpty(Custom_directory))
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


            _ = Task.Run(() => Time_Cheker());
            _ = Task.Run(() => FullDirList());

        }
        private void BTNscan_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }


        private void Clean()
        {
            BTNclean.IsEnabled = true;
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
        private void BTNclean_Click(object sender, RoutedEventArgs e)
        {
            Clean();

        }

        private void Activeform_Activated(object sender, EventArgs e)
        {
           
        }

        private void BTNabout_Click(object sender, RoutedEventArgs e)
        {
            about about = new about();
            about.ShowDialog();
        }
    }
}
