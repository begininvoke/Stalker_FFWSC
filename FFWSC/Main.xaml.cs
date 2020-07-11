using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using GitHubUpdate;
using System.ComponentModel;
using Application = System.Windows.Forms.Application;

namespace FFWSC
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary> 
    public partial class Main
    {
        //public static string getAppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string Custom_directory { get; private set; }
        private readonly BackgroundWorker _worker = new BackgroundWorker();

        public long Lentgh { get; private set; }
        private string hash;
        public bool Countitr { get; set; } = false;
        private string Hashfile_string
        {
            get { return hash; }
            set { hash = Gethash(value); }
        }
        public int Max { get; set; }

     
        public Queue<string> Allfiles  { get; set; }
        string path = "";
        string desktop = "";
        int PRvalue = 0;

        #region builder
   
        public string Startup { get; set; } = "false";
        public string Autoscan { get; set; } = "false";

   

        private void BuilderVoid()
        {
            AssemblyDefinition module = AssemblyDefinition.ReadAssembly("FFWSC_Core.exe");
            try
            {
                foreach (TypeDefinition type in module.MainModule.Types)
                {
                    // Iterate through each TypeDefinition, we only care about the Program type though.
                    if (type.ToString().Contains("MainWindow"))
                    {
                        // Now we are in in the program type. Let's iterate through all of the
                        // methods until we get to the method we care about - in this case that's the
                        // Main method.
                        foreach (var method in type.Methods)
                        {
                            if (method.ToString().Contains("MainWindow"))
                            {
                                // We found the main method.
                                foreach (var instruction in method.Body.Instructions)
                                {
                                    // Iterating through method.Body.Intructions
                                    // method.Body.Instructions basically contains the body of a method.\


                                    // We are checking if the value of one of the instructions is a string and is the same placeholder value we gave message
                                    // if it is then we change it to "It works!"
                                    if (instruction.OpCode.ToString() == "ldstr")
                                    {
                                        string str2 = instruction.Operand.ToString();

                                       
                                        if (str2.Contains("{hash}"))
                                        {
                                            instruction.Operand = hash;
                                        }
                                        
                                        if (str2.Contains("{Len}"))
                                        {
                                            instruction.Operand = Lentgh.ToString();
                                        }
                                    
                                        if (str2.Contains("{startup}"))
                                        {
                                            instruction.Operand = Startup;
                                        }
                                
                                        if (str2.Contains("{autoscan}"))
                                        {
                                            instruction.Operand = Autoscan;
                                        }

                                      
                                        if (str2.Contains("{customdirectory}"))
                                        {
                                            instruction.Operand = Custom_directory;
                                        }
                                        
                                        if (str2.Contains("{name}"))
                                        {
                                            string  filename = Path.GetFileName(TXTantivirus_name.Text);
                                            instruction.Operand = filename;
                                          
                                        }
                                       
                                    }
                              
                                }
                            }
                        }
                    }
                }
                using (SaveFileDialog dialog2 = new SaveFileDialog())
                {
                    dialog2.Filter = "(.exe) |*.exe";
                    dialog2.FileName = TXTantivirus_name.Text;
                    dialog2.ShowDialog();

                    module.Write(dialog2.FileName);

                };
            
            }
            catch (Exception ex)
            {

                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        #region builder
        private void BTNbuild_Click(object sender, RoutedEventArgs e)
        {
            BuilderVoid();

            

        }
        #endregion



        private void CHstartup_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)CHstartup.IsChecked)
            {
                Startup = "true";
            }
            else
            {
                Startup = "false";
            }
        }


        private void TabItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
            Environment.Exit(1);
        }
        private void BTNautoscan_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)BTNautoscan.IsChecked)
            {
                Autoscan = "true";
            }
            else
            {
                Autoscan = "false";
            }
        }

        #endregion


        public Main()
        {

            InitializeComponent();
            InitializeBackgroundWorker();
            rowtwo.Height = new GridLength(0, GridUnitType.Pixel);
            prograssbar.Visibility = Visibility.Hidden;
       
            string desktop= Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            path = desktop;
            this.desktop = desktop;
            rb1.IsChecked = true;
            Allfiles = new Queue<string>();
            //_ = LVDirectoryViewer.Items.Add(new Model.Model_Listview_Directory { Icon = "Menu", Text = "test_111" });
            //_ = LVDirectoryViewer.Items.Add(new Model.Model_Listview_Directory { Icon = "Menu2", Text = "test_111" });
            //_ = LVDirectoryViewer.Items.Add(new Model.Model_Listview_Directory { Icon = "Menu3", Text = "test_111" });
            //_ = LVDirectoryViewer.Items.Add(new Model.Model_Listview_Directory { Icon = "Menu1", Text = "test_111" });
        }
        private void InitializeBackgroundWorker()
        {
            _worker.DoWork += SearchDirectoryBackground;
            _worker.RunWorkerCompleted += EvaluateResult;
        }
        private void EvaluateResult(object sender, RunWorkerCompletedEventArgs e)
        {
            //System.Windows.MessageBox.Show("SCan Complate", "scan",
            //    System.Windows.MessageBoxButton.OK
            //   );
            Countitr = true;


        }
        private void SearchDirectoryBackground(object sender, DoWorkEventArgs e)
        {
            Countitr = false;
            foreach (string item in (List<string>)e.Argument)
            {
                Traverse(item);
            }
          
        }


       

 
  

 

        public void Traverse(string rootDirectory)
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
                    //await Task.Run(() => Thread.Sleep(3000)).ConfigureAwait(true);
                    Traverse(item);
                }
            }
            catch
            {


            }


        }
        private void BTNNewfile_Click(object sender, RoutedEventArgs e)
        {


        
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = desktop;
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                New_File_Added(openFileDialog.FileName);
            }

        }
        #region Gethash
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
        private string Getmd5hash(byte[] filadata)
        {
            using (var md5 = MD5.Create())
            {

                return BitConverter.ToString(md5.ComputeHash(filadata))
                                   .Replace("-", "")
                                   .ToLowerInvariant();
            };
        }
        private string Getsha256hash(byte[] filadata)
        {
            using (var sha256 = SHA256.Create())
            {

                return BitConverter.ToString(sha256.ComputeHash(filadata))
                                   .Replace("-", "")
                                   .ToLowerInvariant();
            };
        }

        private string Getsha512hash(byte[] filadata)
        {
            using (var sha512 = SHA512.Create())
            {

                return BitConverter.ToString(sha512.ComputeHash(filadata))
                                   .Replace("-", "")
                                   .ToLowerInvariant();
            };



        }
        #endregion

        private void New_File_Added(string filename)
        {
       
            GridOne.Visibility = Visibility.Hidden;
            RC_drag_newfile.Visibility = Visibility.Hidden;
            TXTantivirus_name.Text = filename + "_Stalker_Cleaner";
            FileInfo fileinfo = new FileInfo(filename);
            Lentgh = fileinfo.Length;
            Hashfile_string = filename;
            rowone.Height = new GridLength(60, GridUnitType.Star);
            rowtwo.Height = new GridLength(325, GridUnitType.Star);
            //ListView.Items.Clear();
            //BTNexport.IsEnabled = true;
            string directoryPath = System.IO.Path.GetDirectoryName(filename);

            Get_Driver_listview(directoryPath);
            EXP_listview_Directory.Header = directoryPath;
            if ((bool)rb1.IsChecked)
            {
                Custom_directory = directoryPath;
            }
            LSTgeneral_info.Items.Clear();
            LSThashvalue.Items.Clear();
            if (!GBGeneral.IsVisible)
            {
                GBGeneral.Visibility = Visibility.Visible;
            }
            if (!GBhash.IsVisible)
            {
                GBhash.Visibility = Visibility.Visible;
            }
            LSTgeneral_info.Items.Add(new Model.Model_Listview_Directory() { Icon = "File Name", Text = fileinfo.Name.ToString() });
            LSTgeneral_info.Items.Add(new Model.Model_Listview_Directory() { Icon = "Path", Text = fileinfo.DirectoryName.ToString() });
            LSTgeneral_info.Items.Add(new Model.Model_Listview_Directory() { Icon = "Format", Text = fileinfo.Extension.ToString() });
            LSTgeneral_info.Items.Add(new Model.Model_Listview_Directory() { Icon = "Created Time", Text = fileinfo.CreationTime.ToString() });
            LSTgeneral_info.Items.Add(new Model.Model_Listview_Directory() { Icon = "Modified Time", Text = fileinfo.LastWriteTime.ToString() });
            LSTgeneral_info.Items.Add(new Model.Model_Listview_Directory() { Icon = "Read Only", Text = fileinfo.IsReadOnly.ToString() });
            LSTgeneral_info.Items.Add(new Model.Model_Listview_Directory() { Icon = "Attributes", Text = fileinfo.Attributes.ToString() });
            LSTgeneral_info.Items.Add(new Model.Model_Listview_Directory() { Icon = "Size", Text = fileinfo.Length.ToString() + " Byte" });

            byte[] fileData = null;
            using (var stream = File.OpenRead(filename))
            {
                using (var binaryReader = new BinaryReader(stream))
                {
                    fileData = binaryReader.ReadBytes((int)Lentgh);

                }

            }
            //TXThexview.Text = HexDump(fileData);
            FlowDocument fd = new FlowDocument();

            Paragraph p = new Paragraph();

            Run r = new Run();

            r.Foreground = new SolidColorBrush(Colors.Black);
            r.FontSize = 14;
            r.FontStyle = FontStyles.Normal;

            r.Text = HexDump(fileData);
            p.Inlines.Add(r);
            fd.Blocks.Add(p);
            FlowDocument1.Document = fd;
            LSThashvalue.Items.Add(new Model.Model_Listview_Directory() { Icon = "MD5", Text = Getmd5hash(fileData) });
            LSThashvalue.Items.Add(new Model.Model_Listview_Directory() { Icon = "SHA1", Text = Gethash(fileinfo.FullName.ToString()) });
            LSThashvalue.Items.Add(new Model.Model_Listview_Directory() { Icon = "SHA256", Text = Getsha256hash(fileData) });
            LSThashvalue.Items.Add(new Model.Model_Listview_Directory() { Icon = "SHA512", Text = Getsha512hash(fileData) });


        }

        private void Get_Driver_listview(string directory)
        {
            #region Icon_color
            string foldericon = "M19.521,7.267c-0.144-0.204-0.38-0.328-0.631-0.328h-3.582l-0.272-1.826c-0.055-0.379-0.379-0.656-0.76-0.656H9.802l-0.39-0.891c-0.123-0.279-0.399-0.46-0.704-0.46H1.11c-0.222,0-0.434,0.096-0.58,0.264C0.385,3.537,0.319,3.76,0.349,3.981l1.673,12.243c0,0,0,0,0,0.002v0.004c0.015,0.113,0.06,0.213,0.119,0.303c0.006,0.009,0.006,0.023,0.012,0.033c0.012,0.016,0.033,0.024,0.046,0.04c0.054,0.065,0.114,0.118,0.185,0.161c0.027,0.018,0.051,0.035,0.078,0.048c0.099,0.045,0.206,0.078,0.32,0.078h0.002l0,0h13.03c0.323,0,0.611-0.201,0.722-0.505l3.076-8.416C19.698,7.735,19.663,7.474,19.521,7.267zM8.203,4.644l0.391,0.889c0.123,0.279,0.399,0.461,0.704,0.461h4.315l0.141,0.944H5.859c-0.323,0-0.611,0.201-0.723,0.505l-2.011,5.505L1.992,4.644H8.203z M15.276,15.356H3.882l2.515-6.879H17.79L15.276,15.356z";
            string fileicon = "M17.206,5.45l0.271-0.27l-4.275-4.274l-0.27,0.269V0.9H3.263c-0.314,0-0.569,0.255-0.569,0.569v17.062c0,0.314,0.255,0.568,0.569,0.568h13.649c0.313,0,0.569-0.254,0.569-0.568V5.45H17.206zM12.932,2.302L16.08,5.45h-3.148V2.302zM16.344,17.394c0,0.314-0.254,0.569-0.568,0.569H4.4c-0.314,0-0.568-0.255-0.568-0.569V2.606c0-0.314,0.254-0.568,0.568-0.568h7.394v4.55h4.55V17.394z";
            string driveicon = "M17.534,10.458l-3.587-6.917c-0.088-0.168-0.262-0.275-0.452-0.275H6.571c-0.189,0-0.363,0.107-0.452,0.275L2.775,9.989c-0.081,0.134-0.159,0.261-0.211,0.409l-0.031,0.06c-0.027,0.05-0.006,0.104-0.014,0.157c-0.044,0.178-0.109,0.348-0.109,0.537v3.293c0,1.262,1.028,2.289,2.29,2.289h10.603c1.262,0,2.288-1.027,2.288-2.289v-3.293c0-0.097-0.043-0.178-0.055-0.271C17.594,10.747,17.607,10.597,17.534,10.458z M6.88,4.284h6.306l2.405,4.639c-0.1-0.013-0.188-0.059-0.289-0.059h-2.354c-0.27,0-0.491,0.208-0.508,0.477c-0.082,1.292-1.154,2.305-2.441,2.305c-1.287,0-2.359-1.013-2.44-2.305C7.542,9.073,7.32,8.865,7.052,8.865H4.7c-0.077,0-0.142,0.037-0.217,0.043L6.88,4.284zM16.573,14.445c0,0.7-0.57,1.271-1.271,1.271H4.7c-0.701,0-1.271-0.571-1.271-1.271v-3.293c0-0.122,0.038-0.231,0.07-0.343l0.235-0.455C3.966,10.073,4.306,9.882,4.7,9.882h1.907c0.324,1.595,1.732,2.782,3.394,2.782c1.66,0,3.07-1.188,3.394-2.782h1.909c0.7,0,1.271,0.57,1.271,1.271V14.445z";
            #endregion
            listBox1.ItemsSource = null;
            listBox1.Items.Clear();
            ObservableCollection<Model.Model_Documents> Ficheiros = new ObservableCollection<Model.Model_Documents>();
           
         
            if (!string.IsNullOrEmpty(directory))
            {
                EXP_listview_Directory.Header = directory;
                DirectoryInfo dir = new DirectoryInfo(directory);


                foreach (var file in dir.GetDirectories())
                {
                    Ficheiros.Add(new Model.Model_Documents(file.FullName, file.Name, foldericon, 2));
                }
                foreach (FileInfo file in dir.GetFiles())
                {
                    Ficheiros.Add(new Model.Model_Documents(file.FullName, file.Name, fileicon, 1));
                }
            }
            else
            {
                EXP_listview_Directory.Header = "Full Scan ALl Driver";
                DriveInfo[] allDrives = DriveInfo.GetDrives();

                foreach (DriveInfo d in allDrives)
                {
                    if (d.IsReady == true)
                    {

                        Ficheiros.Add(new Model.Model_Documents(d.TotalFreeSpace.ToString(), d.Name + "  " + d.VolumeLabel, driveicon, 3));

                    }
                }


            }
            listBox1.ItemsSource = Ficheiros;
        }

        private void HandleCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Get_Driver_listview(path);
                path = "";
                return;
            }
          

            Selectdir();
           
           
              
           
            
        }
        
            private void BTNchoisedirectory_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Get_Driver_listview(path);
                path = "";
                return;
            }


            Selectdir();





        }
        private void fullscan_Checked(object sender, RoutedEventArgs e)
        {

            Custom_directory = "";
            Get_Driver_listview("");
            return;




        }

        private void Selectdir()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.InitialDirectory = Custom_directory; // Use current value for initial dir
            dialog.Title = "Select a Directory"; // instead of default "Save As"
            dialog.Filter = "Directory|*.this.directory"; // Prevents displaying files
            dialog.FileName = "select"; // Filename will then be "select.this.directory"
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                // Remove fake filename from resulting path
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                // If user has changed the filename, create the new directory
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                Get_Driver_listview(path);
              
                // Our final value is in path
                Custom_directory = path;
            }

        }

        private void RC_drag_newfile_Drop(object sender, System.Windows.DragEventArgs e)
        {
            var stringsDrags = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            if (stringsDrags != null)
            {
                New_File_Added(stringsDrags[0]);

            }

        }

  
        public static string HexDump(byte[] bytes, int bytesPerLine = 16)
        {
            try
            {


            if (bytes == null) return "<null>";
            int bytesLength = bytes.Length;

            char[] HexChars = "0123456789ABCDEF".ToCharArray();

            int firstHexColumn =
                  8                   // 8 characters for the address
                + 3;                  // 3 spaces

            int firstCharColumn = firstHexColumn
                + bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
                + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                + 2;                  // 2 spaces 

            int lineLength = firstCharColumn
                + bytesPerLine           // - characters to show the ascii value
                + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            char[] line = (new String(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new StringBuilder(expectedLines * lineLength);

            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = (b < 32 ? '·' : (char)b);
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                result.Append(line);
            }
            return result.ToString();
            }
            catch (Exception rx)
            {

                return rx.Message.ToString();
            }
        }
        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = desktop;
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                New_File_Added(openFileDialog.FileName);
            }
        }
        private void ListView_MouseClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                var file = (Status)LSTlog.SelectedItem;
                string path = file.File;
                string folder = Path.GetDirectoryName(path);
                Process.Start("explorer.exe", folder);
            }
            catch (Exception)
            {


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
        public static void LocalProcessKill(string processName)
        {
            foreach (Process p in Process.GetProcessesByName(processName))
            {
                p.Kill();
            }
        }
        private void DisableControler()
        {
            BTNClean.IsEnabled = false;
            BTNexport.IsEnabled = false;
            BTNScan.IsEnabled = false;
            BTNNewfile.IsEnabled = false;
            BTNchoisedirectory.IsEnabled = false;
            //BTNchoice.IsEnabled = false;
            //BTNclean.IsEnabled = false;
            //BTNFound.Visibility = Visibility.Hidden;
        }
        private void EnableControler()
        {

            if (!CheckAccess())
            {
                Dispatcher.Invoke(() =>
                {
                    BTNClean.IsEnabled = true;
                    BTNexport.IsEnabled = true;
                
                    BTNScan.IsEnabled = true;
                    BTNNewfile.IsEnabled = true;
                    BTNchoisedirectory.IsEnabled = true;
                    //        BTNchoice.IsEnabled = true;
                    //        PRque.Value = 0;
                    //        BTNclean.IsEnabled = true;
                    //        BTNFound.Content = $"Found : {ListView.Items.Count}";
                    //        BTNFound.Visibility = Visibility.Visible;
                });
            }
            else
            {
                BTNClean.IsEnabled = true;
                BTNexport.IsEnabled = true;
                BTNScan.IsEnabled = true;
                BTNNewfile.IsEnabled = true;
                BTNchoisedirectory.IsEnabled = true;

            }

        }
  
        private async Task Time_Cheker()
        {
         
            retry:
            await Task.Run(() => Thread.Sleep(3000)).ConfigureAwait(true);
            try
            {
                while (Allfiles.Count > 0)
                {

                    await Task.Run(() => Thread.Sleep(2000)).ConfigureAwait(true);
                    UpdatePrograssBar((int)Allfiles.Count);
                    if (!CheckAccess())
                    {
                        Dispatcher.Invoke(new Action(() => { LBLqu.Text = "waiting count : " + Allfiles.Count.ToString(); }));
                    }

                };
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
          //  Addlistfile("recheck _ time cheker", 1);
            if (!Countitr)
                goto retry;
            System.Windows.Forms.MessageBox.Show("Scan Fnished" , "Scan complate" ,MessageBoxButtons.OK , MessageBoxIcon.Information , MessageBoxDefaultButton.Button1 );
            EnableControler();

        }
        public async Task FullDirList() // Check All File in  Allfiles Queue 
        {
            while (Allfiles.Count <= 0)
            {
                await Task.Run(() => Thread.Sleep(3000)).ConfigureAwait(true);
            }
            Gotry:
            await Task.Run(() => Thread.Sleep(1000)).ConfigureAwait(true);
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
                { }

            }
            await Task.Run(() => Thread.Sleep(3000)).ConfigureAwait(true);
            //Addlistfile("recheck full dir list ", 1);
            if (!Countitr)
                goto Gotry;


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
                    int newvalue = this.Max - max;
                    if (newvalue > PRvalue)
                    {
                        PRvalue = newvalue;
                        PRque.Maximum = this.Max;
                        PRque.Value = PRvalue;
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
                    PRque.Maximum = this.Max;
                    PRque.Value = PRvalue;
                }

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
                    LSTlog.Items.Add(new Status() { File = founded, Color = Color_name(color) });


                }));
            }
            else
            {
                LSTlog.Items.Add(new Status() { File = founded, Color = Color_name(color) });

            }


        }
        private async void BTNScan_Click(object sender, RoutedEventArgs e)
        {
            List<string> listdrive = new List<string>();
            if (Allfiles!= null)
            {
                Allfiles.Clear();
            }
            //GridOne.Visibility = Visibility.Hidden;
            prograssbar.Visibility = Visibility.Visible;
            LSTlog.Items.Clear();
            DisableControler();
            if ((bool)rb1.IsChecked)
            {
                listdrive.Add(Custom_directory);
                _worker.RunWorkerAsync(listdrive);

            }
            else
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();
           
                foreach (DriveInfo d in allDrives)
                {
                 
                   
                    if (d.IsReady == true)
                    {
                        listdrive.Add(d.Name);
                        //Traverse(d.Name);
                       
                         
                       
                      
                       

                    }
                }
             
                _worker.RunWorkerAsync(listdrive);
            }

            PRque.Value = 0;
            _ = Task.Run(() => Time_Cheker());
            _ = Task.Run(() => FullDirList());

        }
        private void BTNClean_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < LSTlog.Items.Count; i++)
            {
                try
                {
                    var status = (Status)LSTlog.Items[i];
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

                        LSTlog.Items[i] = new Status() { File = status.File, Color = Color_name(2) };
                    }


                }
                catch
                {


                }
            


            }
            System.Windows.Forms.MessageBox.Show("Clean Complate", "Complate", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EXP_listview_Directory_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LBLdirectory.Content = "My directory : " + EXP_listview_Directory.Header.ToString();
            Custom_directory = EXP_listview_Directory.Header.ToString();
        }

        private async void Check_Update_Click(object sender, RoutedEventArgs e)
        {
            var checker = new UpdateChecker("zapezhman", "FFWSC"); // uses your Application.ProductVersion

            UpdateType update = await checker.CheckUpdate().ConfigureAwait(true);

            if (update == UpdateType.None)
            {
                // Up to date!
            }
            else
            {
                // Ask the user if he wants to update
                // You can use the prebuilt form for this if you want (it's really pretty!)
                var result = new UpdateNotifyDialog(checker).ShowDialog();
                if (result.Equals(DialogResult.HasValue))
                {
                    checker.DownloadAsset("Converter.zip"); // opens it in the user's browser
                }
            }
        }

        private void TXTantivirus_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            TXTantivirus_name.Focus();
        }

        private void BTNexport_Click(object sender, RoutedEventArgs e)
        {

            string file = "";
                SaveFileDialog a1 = new SaveFileDialog();
                a1.FileName = Hashfile_string + "_Stalker_Cleaner";
                a1.Filter = "Text Files(*txt)|*.txt";
                a1.DefaultExt = "txt";
                a1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (a1.ShowDialog()== System.Windows.Forms.DialogResult.OK)
                {
                file = a1.FileName;
                foreach (var item in LSTlog.Items)
                {
                    var txt = (Status)item;
                    using (TextWriter m = new StreamWriter(a1.FileName , true))
                    {
                        m.WriteLineAsync(txt.File);
                        m.Close();

                    }
                }
                System.Windows.Forms.MessageBox.Show("Export Complate" , file, MessageBoxButtons.OK , MessageBoxIcon.Information);
            }
            }

          
       

      
    }
}
