﻿
using ALL_LEGIT.Download;
using Gma.System.MouseKeyHook;
using Microsoft.Graph.TermStore;
using Microsoft.VisualStudio.Services.CircuitBreaker;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.Web.Helpers;
using Newtonsoft.Json;
using RestSharp;
using SharpCompress.Archives.Rar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static Azure.Core.HttpHeader;
using Directory = System.IO.Directory;
using Timer = System.Windows.Forms.Timer;
using ToolTip = System.Windows.Forms.ToolTip;
using System.Drawing.Text; 

namespace ALL_LEGIT
{
    public partial class MainWindow : Form
    {// 1. Import InteropServices


        /// 2. Declare DownloadsFolder KNOWNFOLDERID
        private static Guid FolderDownloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");

        /// 3. Import SHGetKnownFolderPath method
        /// <summary>
        /// Retrieves the full path of a known folder identified by the folder's KnownFolderID.
        /// </summary>
        /// <param name="id">A KnownFolderID that identifies the folder.</param>
        /// <param name="flags">Flags that specify special retrieval options. This value can be
        ///     0; otherwise, one or more of the KnownFolderFlag values.</param>
        /// <param name="token">An access token that represents a particular user. If this
        ///     parameter is NULL, which is the most common usage, the function requests the known
        ///     folder for the current user. Assigning a value of -1 indicates the Default User.
        ///     The default user profile is duplicated when any new user account is created.
        ///     Note that access to the Default User folders requires administrator privileges.
        ///     </param>
        /// <param name="path">When this method returns, contains the address of a string that
        ///     specifies the path of the known folder. The returned path does not include a
        ///     trailing backslash.</param>
        /// <returns>Returns S_OK if successful, or an error value otherwise.</returns>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);
        /// 4. Declare method that returns the Downloads Path as string
        /// Returns the absolute downloads directory specified on the system.
        /// </summary>
        /// <returns></returns>
        /// 
        public static string FilecryptURL = "";
        public static Dictionary<string, string> Dict = new Dictionary<string, string>();
        public static string GetDownloadsPath()
        {
            if (Environment.OSVersion.Version.Major < 6) throw new NotSupportedException();

            IntPtr pathPtr = IntPtr.Zero;

            try
            {
                SHGetKnownFolderPath(ref FolderDownloads, 0, IntPtr.Zero, out pathPtr);
                return Marshal.PtrToStringUni(pathPtr);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pathPtr);
            }

        }
        public static bool TrayExit = false;
        private void menuItem1_Click(object Sender, EventArgs e)
        {
            // Close the form, which closes the application.
            TrayExit = true;
            this.Close();
        }
        void intToolTips()
        {
            try
            {
                ToolTip AutoDLBox = new ToolTip();
                AutoDLBox.SetToolTip(this.AutoDLBox, "Automatically download added links.");

                ToolTip HKTip = new ToolTip();
                HKTip.SetToolTip(this.HotKeyBox, "Change global shortcut, global shortcut can be used from anywhere, even if All Legit! is minimized or closed to tray.\n" +
                    "Simply copy a link and press the global shortcut and All Legit! will parse the links.");

                ToolTip listView1 = new ToolTip();
                listView1.SetToolTip(this.listView1, "Right click or middle click to remove item.");

                ToolTip StayOnTop = new ToolTip();
                StayOnTop.SetToolTip(this.StayOnTopCheckbox, "Keep All Legit on top of other programs.");

                ToolTip disableNotiesBox = new ToolTip();
                disableNotiesBox.SetToolTip(this.disableNotiesBox, "Disable windows notifications.");

                ToolTip OpenDirBox = new ToolTip();
                OpenDirBox.SetToolTip(this.OpenDirBox, "Open download directory after downloads/extractions finish.");

                ToolTip AutoUpdate = new ToolTip();
                AutoUpdate.SetToolTip(this.autoUpdateBox, "If update is available install it autommatically at launch.");

                ToolTip Close2Tray = new ToolTip();
                Close2Tray.SetToolTip(this.Close2Tray, "Minimize All Legit to taskbar instead of exiting when you close the app.");

                ToolTip RemDL = new ToolTip();
                RemDL.SetToolTip(this.RemDL, "Remove downloaded/extracted/copied links.");

                ToolTip AutoOverwrite = new ToolTip();
                AutoOverwrite.SetToolTip(this.AutoOverwrite, "If file exists automatically overwrite it without asking.");

                ToolTip autoDelZips = new ToolTip();
                autoDelZips.SetToolTip(this.autoDelZips, "Delete zips after they have been successfully extracted.");

                ToolTip AutoExtract = new ToolTip();
                AutoExtract.SetToolTip(this.AutoExtract, "Automatically extract downloaded archives.");

                ToolTip extractNested = new ToolTip();
                extractNested.SetToolTip(this.extractNested, "If archives exist within extracted archives, extract them to a folder named after the archive.");


            }
            catch { }
        }
        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem2;
        public static string APIKEY;
        public static string apiNAME;
        public static bool waitingforkey = false;
        public static Timer a = new Timer();
        public static System.Windows.Forms.Keys hotkeyset = Properties.Settings.Default.HotKeyKeyData;
        public MainWindow()
        {
            InitializeComponent();
            intToolTips();
            Utilities.GetMissingFiles();
            this.components = new System.ComponentModel.Container();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            // Initialize contextMenu1
            this.contextMenu1.MenuItems.AddRange(
                       new System.Windows.Forms.MenuItem[] { this.menuItem1, this.menuItem2 });

            // Initialize menuItem1
            this.menuItem1.Index = 0;
            this.menuItem1.Text = "E&xit";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
            // Initialize menuItem1
            this.menuItem2.Index = 1;
            this.menuItem2.Text = "Diable notifications";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);



            // Create the NotifyIcon.
            // The Icon property sets the icon that will appear
            // in the systray for this application.
            ALTrayIcon.Icon = new Icon("_bin\\AL.ico");

            // The ContextMenu property sets the menu that will
            // appear when the systray icon is right clicked.
            ALTrayIcon.ContextMenu = this.contextMenu1;


            // The Text property sets the text that will be displayed,
            // in a tooltip, when the mouse hovers over the systray icon.
            ALTrayIcon.Text = "All Legit";
            ALTrayIcon.Visible = true;
            this.Invoke(() =>
            {
                menuItem2.Checked = Properties.Settings.Default.DisableNotifies;
            });
            // Handle the DoubleClick event to activate the form.

            var appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION",
                  appName, 11000, Microsoft.Win32.RegistryValueKind.DWord);

            try
            {
                Hook.GlobalEvents().KeyDown += (sender, e) =>
               {
                   if (e.KeyData == Properties.Settings.Default.HotKeyKeyData)
                   {
                       DoAsyncConversion();
                   }
               };
            }
            catch { }

        }
        private void menuItem2_Click(object Sender, EventArgs e)
        {
            if (menuItem2.Checked)
            {
                disableNotiesBox.Checked = false;
                menuItem2.Checked = false;
                Properties.Settings.Default.DisableNotifies = false;
            }
            else
            {
                menuItem2.Checked = true;
                disableNotiesBox.Checked = true;
                Properties.Settings.Default.DisableNotifies = true;
            }

            Properties.Settings.Default.Save();

        }
    
        public static string patchNotes =
        " • Added BULK UPLOAD option! If a site gives you 20 rapid gator links or whatever (unless its filecrypt), use this mode, it will count it all as one object and make auto extraction (usually) work better and download faster!\n" +
        " • Fixed some instances of nested deletions, still a WIP so be careful with using delete extracted on massive downloads if you're worried about potentially needing to download the files again. Though please DO NOTE, when this " +
        "happens it will go to your RECYCLE BIN, not full deletion, the vast majority of the time!\n\n\n";

        public static bool endreached = false;



        private async void MainWindow_Load(object sender, EventArgs e)

        {
            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddFontFile($"{Environment.CurrentDirectory}\\_bin\\ka1.ttf");
            foreach (Control c in this.Controls)
            {
                if (c.Name.StartsWith("KA"))
                {
                    c.Font = new Font(pfc.Families[0], 12, FontStyle.Regular);
                }
  
            }
            extractNested.Checked = Properties.Settings.Default.extractNested;
            autoUpdateBox.Checked = Properties.Settings.Default.AutoUpdate;
            Updater.Update();

            var cme = new System.Net.Configuration.ConnectionManagementElement();
            cme.MaxConnection = 99999;
            System.Net.ServicePointManager.DefaultConnectionLimit = 99999;


            KA_changeLog_Label.Text = $"{Updater.LocalVersion} Change log:\n\n";
            tipsText.Text = " • Click settings cog in top-right corner for auto downloads, auto extraction and more.\n" +
                $" • Shortcut key works everywhere, even when app is minimized/closed to tray.";
            SplashText.Text = $"{patchNotes}";
            StayOnTopCheckbox.Checked = Properties.Settings.Default.TopMost;
            var converter = new KeysConverter();
            removeURLs.Checked = Properties.Settings.Default.ExcludeURLS;
            HotKeyBox.Text = converter.ConvertToString(Properties.Settings.Default.HotKeyKeyData);
            OpenDirBox.Checked = Properties.Settings.Default.OpenDir;
            disableNotiesBox.Checked = Properties.Settings.Default.DisableNotifies;
            Close2Tray.Checked = Properties.Settings.Default.Close2Tray;
            RemDL.Checked = Properties.Settings.Default.RemDL;
            AutoOverwrite.Checked = Properties.Settings.Default.AutoOverwrite;
            AutoDLBox.Checked = Properties.Settings.Default.AutoDL;
            autoDelZips.Checked = Properties.Settings.Default.DelZips;
            AutoExtract.Checked = Properties.Settings.Default.AutoExtract;
            if (!String.IsNullOrEmpty(Properties.Settings.Default.ZipPWS))
            {
                PWBox.Text = Properties.Settings.Default.ZipPWS;
                PWLIST = PWBox.Text;
            }
            this.Invoke(() =>
            {
                DownloadingText.Text = $"";
            });
            KA_tipsHeader.Text = "Tips:\n\n";
            RemDL.Checked = Properties.Settings.Default.RemDL;
            if (!Properties.Settings.Default.RegistrySet || Properties.Settings.Default.DownloadDir == null || !Properties.Settings.Default.NoNag)
            {
                if (Properties.Settings.Default.DownloadDir.Length < 3 || String.IsNullOrWhiteSpace(Properties.Settings.Default.DownloadDir))
                {
                    Properties.Settings.Default.NoNag = false;
                    Properties.Settings.Default.RegistrySet = false;
                    Properties.Settings.Default.DownloadDir = GetDownloadsPath() + "\\All Legit Downloads";
                    Properties.Settings.Default.Save();
                }
                DownloadDir.Text = Properties.Settings.Default.DownloadDir;
                Process proc = new Process();

                if (!Properties.Settings.Default.RegistrySet)
                {
                    try
                    {
                        //check if we are running as administrator currently
                        if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                        {
                            DialogResult yesnoQ = MessageBox.Show("Administrator rights are required to properly register your downloads path. Once it is registered then it is set until you change it. Please click YES to elevate All Legit and register your download directory.\n\nNOTE: If you click NO we will not ask you again and it might cause download conflicts in the future. For best results click yes or press cancel to be reminded on next launch.", "Relaunch as administrator?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                            if (yesnoQ == DialogResult.Yes)
                            {

                                proc.StartInfo.UseShellExecute = true;
                                proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                                proc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
                                //Start new process as administrator
                                proc.StartInfo.FileName = "All.Legit.exe";
                                proc.StartInfo.Verb = "runas";
                                proc.StartInfo.Arguments = "";
                                proc.Start();
                                //Exit current process
                                Environment.Exit(0);

                            }
                            else if (yesnoQ == DialogResult.No)
                            {
                                MessageBox.Show("OK we will not ask again.");
                                Properties.Settings.Default.RegistrySet = true;
                                Properties.Settings.Default.NoNag = true;
                                Properties.Settings.Default.Save();
                            }
                            else
                            {
                                Properties.Settings.Default.RegistrySet = false;
                                Properties.Settings.Default.Save();
                            }

                        }
                        else
                        {
                            Environment.SetEnvironmentVariable("ALDOWNLOADS", Properties.Settings.Default.DownloadDir,
                            EnvironmentVariableTarget.Machine);
                            Properties.Settings.Default.RegistrySet = true;
                            Properties.Settings.Default.Save();
                            try
                            {
                                Process.Start($"\"C:\\Windows\\explorer.exe\" \"{Environment.CurrentDirectory}\\All.Legit.exe\"");
                                await Task.Delay(3000);
                                File.WriteAllText(Environment.CurrentDirectory + "\\js", "");
                                Application.Exit();
                            }
                            catch { }
                        }
                    }
                    //if user selects "no" from adminstrator request.
                    catch
                    {
                        MessageBox.Show("Administrator access was denied, please allow All Legit to store your downloads directory by accepting the admin prompt.");
                    }
                }
            }
            if (!Directory.Exists(Properties.Settings.Default.DownloadDir))
            {
                Directory.CreateDirectory(Properties.Settings.Default.DownloadDir);
            }
            if (File.Exists(Environment.CurrentDirectory + "\\js"))
            {
                File.Delete(Environment.CurrentDirectory + "\\js");
                MessageBox.Show("All set!");
            }
            await ConnectViaAPIAsync();
            while (!loginsuccess)
            {
                this.Invoke(() =>
                {
                    this.Text = $"All-Legit {Updater.LocalVersion}: Not Connected";
                });


            }
            if (loginsuccess)
            {
                this.Invoke(() =>
                {
                    isloggingin = false;
                    this.Text = $"All-Legit {Updater.LocalVersion}: Connected";
                });
            }

        }

        public static dynamic getJson(string requestURL)
        {
            try
            {
                string BaseURL = "https://api.alldebrid.com/v4/";
                var client = new RestClient(BaseURL);
                Console.WriteLine($"Requesting: {BaseURL}{requestURL}");
                var request = new RestRequest(requestURL, Method.Get);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                var queryResult = client.Execute(request);
                var obj = JsonConvert.DeserializeObject<dynamic>(queryResult.Content);
                return obj;
            }
            catch
            {

                return null;
            }
        }
        public static bool loginsuccess = false;
        public static bool isloggingin = false;
        public async Task ConnectViaAPIAsync(bool wasManual = false)
        {
            isloggingin = true;
            Thread t1 = new Thread(() =>
            {
                try
                {
                    apiNAME = Properties.Settings.Default.ApiNAME;
                    APIKEY = Properties.Settings.Default.ApiKEY;
                    if (!String.IsNullOrWhiteSpace(apiNAME) || !String.IsNullOrWhiteSpace(APIKEY))
                    {
                        apiNAME = Properties.Settings.Default.ApiNAME;
                        APIKEY = Properties.Settings.Default.ApiKEY;
                        var auth = getJson($"user?agent={apiNAME}&apikey={APIKEY}");
                        if (!auth.ToString().Contains("AUTH"))
                        {
                            loginsuccess = true;
                        }
                        else
                        {
                            if (!wasManual)
                                MessageBox.Show(new Form { TopMost = true }, "Previously set API key no longer working... you must reconnect!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    if (!loginsuccess)
                    {
                        Random random = new Random();
                        int randomNumber = random.Next(0, 1000);
                        apiNAME = $"AllLegit{randomNumber}";
                        Properties.Settings.Default.ApiNAME = apiNAME;
                        Properties.Settings.Default.Save();
                        var obj = getJson($"pin/get?agent={apiNAME}");
                        FilecryptURL = obj.data.user_url.ToString();
                        if (!wasManual)
                        {
                            this.Invoke(() =>
                            {
                                Form WebFormForm = new WebFormForm();
                                WebFormForm.ShowDialog();
                                this.Show();
                            });
                        }
                        string CheckURL = obj.data.check_url.ToString();
                        obj = getJson(CheckURL);
                        bool Activated;
                        Activated = bool.Parse(obj.data.activated.ToString());
                        while (!Activated)
                        {
                            obj = getJson(CheckURL);
                            Activated = bool.Parse(obj.data.activated.ToString());
                            Task.Delay(1000);
                        }

                        if (Activated)
                        {
                            obj = getJson(CheckURL);
                            APIKEY = obj.data.apikey.ToString();
                            Properties.Settings.Default.ApiKEY = APIKEY;
                            Properties.Settings.Default.Save();
                        }

                        var auth = getJson($"user?agent={apiNAME}&apikey={APIKEY}");
                        if (!auth.ToString().Contains("AUTH"))
                        {
                            loginsuccess = true;
                        }
                        else
                        {
                            MessageBox.Show(new Form { TopMost = true }, "Previously set API key no longer working... you must reconnect!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch
                {
                    Program.mutex.Close();
                    Application.Exit();
                }
            });
            t1.Start();
            while (t1.IsAlive)
            {
                await Task.Delay(100);
            }


        }
        public static string fileDownloading = ";";
        public static bool warnedthisbatch = false;
        public static bool overwrite = false;
        public static string MagnetSubName = "";
        public static bool NoNamesPresent = false;
        public static int CheckedCount = 0;
        public static int cancelledCount = 0;
        public static int CurrentCount = 0;
        public static int cancelGroupRuns = 0;
        public static string FinishedDL = "";
        public static string DIR = "";
        public static string DLList = "";
        public static string CurrentDLFileName = "";
        public WebClient webClient = new WebClient();
        string currentGroup = "";
        public async Task downloadFiles(string URL, string FILENAME, string MagnetNAME)
        {
            bool hasgonethru = false;
            if (dlsGoin.Contains($"{FILENAME};{MagnetNAME}"))
            {
                return;
            }
            dlsGoin += $"{FILENAME};{MagnetNAME}\n";
            while (webClient.IsBusy)
            {
                await (Task.Delay(100));
            }
            currGroup = MagnetNAME;
            CancelButton.Visible = true;
            if (!String.IsNullOrEmpty(cancelledGroup))
            {

                cancelGroupRuns++;
                if (MagnetNAME.Equals(cancelledGroup))
                {
                    webClient.CancelAsync();
                    if (cancelGroupRuns == cancelledCount)
                    {
                        cancelledGroup = "";
                        cancelledCount = 0;
                        cancelGroupRuns = 1;
                    }
                    return;
                }
                else
                {
                    cancelledGroup = "";
                }
            }
            else
            {
                cancelledGroup = "";
            }
            webClient = new WebClient();
            warnedthisbatch = false;
            Stopwatch sw = new Stopwatch(); // The stopwatch which we will be using to calculate the download speed

            DIR = Properties.Settings.Default.DownloadDir + "\\" + MagnetNAME;
            DLList += $"{FILENAME};{DIR}\n";
            if (!Directory.Exists(DIR))
            {
                Directory.CreateDirectory(DIR);
            }
            string DL = Properties.Settings.Default.DownloadDir + "\\" + MagnetNAME + "\\" + FILENAME;

            if (File.Exists(DL))
            {
                if (!currentGroup.Contains(MagnetNAME) && !Properties.Settings.Default.AutoOverwrite)
                {
                    currentGroup = MagnetNAME;
                    DialogResult Overwrite1 = MessageBox.Show(new Form { TopMost = true }, $"Files found, do you want to overwrite all files from this batch of links({MagnetNAME})", "Overwrite?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (Overwrite1 == DialogResult.Yes)
                    {
                        overwrite = true;
                    }
                    else
                    {
                        cancelledGroup = "";
                        fileDownloading = "";
                        isDownloading = false;
                    }
                }
                if (Properties.Settings.Default.AutoOverwrite)
                {
                    try
                    {

                        if (File.Exists(DL))
                        {
                            DL.FileRecycle();
                        }
                    }
                    catch { }
                }
                else if (!Properties.Settings.Default.AutoOverwrite)
                {
                    try
                    {
                        listView1.BeginUpdate();
                        foreach (ListViewItem itemmm in listView1.Items)
                        {
                            if (itemmm.SubItems[2].Text.Contains(currGroup) && itemmm.Checked)
                            {
                                if (Properties.Settings.Default.RemDL)
                                {
                                    listView1.Items.Remove(itemmm);
                                }
                                else
                                {
                                    itemmm.Checked = false;
                                }
                            }
                        }
                        listView1.EndUpdate();
                    }
                    catch { }
                    return;
                }
            }
            //
            //
            //
            //
            //DOWNLOAD AREA
            //
            //
            //
            //
            string DLS = "";
            sw.Start();
            webClient.DownloadProgressChanged += (s, e) =>
            {
                isDownloading = true;
                if (!hasgonethru)
                {
                    this.Invoke(() =>
                    {
                        CancelButton.Visible = true;
                    });

                    if (listView1.Items.Count > 0)
                    {
                        CurrentDLFileName = listView1.TopItem.SubItems[1].Text;
                    }
                    var name = FILENAME;
                    const int MaxLength = 35;
                    if (name.Length > MaxLength)
                    {
                        FILENAME = name.Substring(0, MaxLength) + "..."; //
                    }
                    hasgonethru = true;
                }
                DLS = String.Format("{0:0.00}", (e.BytesReceived / 1024 / 1024 / sw.Elapsed.TotalSeconds).ToString("0.00"));
                this.Invoke(() =>
                {
                    DownloadingText.Text = $"{FILENAME} - {e.ProgressPercentage}% - {DLS}MB\\s";
                    dlProg.Value = e.ProgressPercentage;
                });
            };

            webClient.DownloadFileCompleted += (s, e) =>
            {
                isDownloading = false;
                fileDownloading = "";
                FinishedDL = DL;
                torrentDLING = false;
                webClient.Dispose();
                if (e.Cancelled)
                {
                    if (File.Exists(DL))
                    {
                        DL.FileRecycle();
                    }
                    if (Directory.Exists(DIR))
                    {
                        string[] files = Directory.GetFiles(DIR, "*.*", SearchOption.AllDirectories);
                        if (files.Length == 0)
                        {
                            DIR.DirectoryRecycle();
                        }
                    }
                    if (webClient.IsBusy)
                    {
                    }
                    foreach (ListViewItem item in listView1.CheckedItems)
                    {
                        if (MagnetNAME == cancelledGroup)
                        {
                            cancelledCount++;
                        }
                        if (item.BackColor == Color.MediumSpringGreen)
                        {
                            item.BackColor = Color.FromArgb(0, 50, 42);
                        }
                    }
                }
                this.Invoke(() =>
                {
                    CancelButton.Visible = false;
                    DownloadingText.Text = $"Download finished...";
                });
                sw.Stop();

                this.Invoke(() =>
                {
                    listView1.BeginUpdate();

                    foreach (ListViewItem item in listView1.Items)
                    {
                        if (item.SubItems[1].Text.Equals(URL))
                        {
                            if (Properties.Settings.Default.RemDL)
                            {
                                listView1.Items.Remove(item);
                            }
                            else
                            {
                                item.Checked = false;
                            }
                        }
                    }
                    listView1.EndUpdate();
                });
                this.Invoke(() =>
                {
                    dlProg.Value = 0;
                });
                fileDownloading = "";
                torrentDLING = false;
                cancel = false;
                webClient.Dispose();
            };
            await webClient.DownloadFileTaskAsync(new Uri(URL), $"{DL}");


        }

        public static bool notdone = false;
        public static long total = 0;
        public static long DLsofar = 0;
        public static long ULsofar = 0;
        public static double DownloadSpeed = 0;
        public static double UploadSpeed = 0;
        public static bool alertedonce = false;
        public static string PWLIST = "";
        public static string filecryptlist = "";
        public static bool filecryptinprog = false;

        /// <summary>
        /// The URLMON library contains this function, URLDownloadToFile, which is a way
        /// to download files without user prompts.  The ExecWB( _SAVEAS ) function always
        /// prompts the user, even if _DONTPROMPTUSER parameter is specified, for "internet
        /// security reasons".  This function gets around those reasons.
        /// </summary>
        /// <param name="callerPointer">Pointer to caller object (AX).</param>
        /// <param name="url">String of the URL.</param>
        /// <param name="filePathWithName">String of the destination filename/path.</param>
        /// <param name="reserved">[reserved].</param>
        /// <param name="callBack">A callback function to monitor progress or abort.</param>
        /// <returns>0 for okay.</returns>
        /// source: http://www.pinvoke.net/default.aspx/urlmon/URLDownloadToFile%20.html
        [DllImport("urlmon.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern Int32 URLDownloadToFile(
            [MarshalAs(UnmanagedType.IUnknown)] object callerPointer,
            [MarshalAs(UnmanagedType.LPWStr)] string url,
            [MarshalAs(UnmanagedType.LPWStr)] string filePathWithName,
            Int32 reserved,
            IntPtr callBack);

        public static string currGroup = "";
        /// <summary>
        public static int CurrentlyShowingID = 0;
        /// Download a file from the webpage and save it to the destination without promting the user
        /// </summary>
        /// <param name="url">the url with the file</param>
        /// <param name="destinationFullPathWithName">the absolut full path with the filename as destination</param>
        /// <returns></returns>
        public FileInfo DownloadFile(string url, string destinationFullPathWithName)
        {
            URLDownloadToFile(null, url, destinationFullPathWithName, 0, IntPtr.Zero);
            return new FileInfo(destinationFullPathWithName);
        }
        private Dictionary<string, string> CurrentPaste { get; set; } = new Dictionary<string, string>();
        private List<string> pastedList { get; set; } = new List<string>();
        public static string LinkType = "";
        public static string currentLink = "";
        public static bool convertingMag = false;
        public static bool showingconversion = false;
        public bool workingOnPaste { get; set; } = false;
        public static Dictionary<string, Dictionary<string, string>> magnetList { get; set; }
        public async void DoAsyncConversion(string pasted = "")
        {
            List<string> _pastedList = pastedList;
            pasted = pasted.Trim();
            if (pasted.Length == 0)
            {
                if (Clipboard.GetText().Length > 0)
                {
                    pasted = Clipboard.GetText();
                }
                else
                {
                    return;
                }
            }
            int DLCCountLocal = 0;
            fileDownloading += $"{pasted};";
            bool _workingOnPaste = workingOnPaste;

            while (_workingOnPaste)
            {
                await Task.Delay(100);
            }    
            if (pasted.ToLower().StartsWith("magnet:?".ToLower()))
            {
                _workingOnPaste = true;
                this.Invoke(() =>
                {
                    splashPanel.Visible = false;
                });
                if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                {
                    this.Invoke(() =>
                    {
                        ALTrayIcon.ShowBalloonTip(2000, "", "Adding magnet links...", ToolTipIcon.None);
                    });
                }
                else
                {
                    this.Invoke(() =>
                    {
                        DownloadingText.Text = $"Adding magnet links...";
                    });
                }

                Thread t1 = new Thread(async () =>
                {
                    pasted = pasted.Trim();

                    string[] output = new string[] { pasted };
                    output = pasted.Replace("\r\n", "\n").Replace(";", "\n").Replace("\n\n", "\n").Replace("\r", "\n").Trim().Split('\n');
                    bool isErrors = false;
                    int a = 0;
                    int b = 0;
                    int c = 0;
                    string a1 = "";
                    string b1 = "";
                    string c1 = "";
                    foreach (string link in output)
                    {
                        _pastedList.Add(link);
                    }
                    try
                    {

                        foreach (string s in _pastedList)
                        {
                            while (_workingOnPaste)
                            {
                                await Task.Delay(100);
                            }
                            _workingOnPaste = true;
                            var obj = getJson($"magnet/upload?agent={apiNAME}&apikey={APIKEY}&magnets[]={s}");
                            string status = obj.status.ToString();
                            if (obj.data.magnets[0].ToString().Contains("error"))
                            {
                                isErrors = true;
                                string error = obj.data.magnets[0].error.code.ToString();
                                if (error.Contains("MAGNET_INVALID_URI"))
                                {
                                    a++;
                                    a1 += $"{s}\n";
                                }
                                if (error.Contains("MAGNET_TOO_MANY_ACTIVE"))
                                {
                                    b++;
                                    b1 += $"{s}\n";
                                }
                                if (error.Contains("MAGNET_NO_SERVER"))
                                {
                                    c++;
                                    c1 += $"{s}\n";
                                }
                            }
                            if (isErrors)
                            {
                                continue;
                            }
                            else
                            {
                                LinkType = "M";
                                string magnetName = obj.data.magnets[0].name.ToString();
                                Dictionary<string, string> magnetDict = new Dictionary<string, string>();
                                magnetDict.Add("Name", magnetName);
                                string magnetID = obj.data.magnets[0].id.ToString();
                                magnetDict.Add("ID", magnetID);
                                string MagnetPoll = $"magnet/status?agent={apiNAME}&apikey={APIKEY}&id={magnetID}";
                                magnetDict.Add("StatusLink", MagnetPoll);
                                Dictionary<string, Dictionary<string, string>> magnetList = new Dictionary<string, Dictionary<string, string>>();
                                magnetList.Add(magnetName, magnetDict);

                                //WHILE LOOP START
                                if (notdone)
                                {
                                    this.Invoke(() =>
                                    {
                                        Program.form.DownloadingText.Text = "Magnet already converting! Cannot add torrent until this finshes or it it cancelled first!";
                                    });
                                    _workingOnPaste = false;
                                    return;
                                }
                                obj = getJson(MagnetPoll);
                                var key = obj.data.magnets;
                                magnetName = key.filename.ToString();
                                bool ready = false;
                                notdone = true;
                                bool secondalert = false;
                                while (notdone)
                                {
                                    foreach (var MagnetList in magnetList)
                                    {

                                        magnetName = MagnetList.Key.ToString();
                                        magnetID = MagnetList.Value["ID"];
                                        obj = getJson(MagnetList.Value["StatusLink"]);
                                        key = obj.data.magnets;
                                        if (key.id.ToString().Equals(magnetID))
                                        {
                                            string seeders = key.seeders.ToString();
                                            string MagnetStatus = key.status.ToString();
                                            if (MagnetStatus.Equals("Ready"))
                                            {
                                                torrentDLING = false;
                                                convertingMag = false;
                                                notdone = false;
                                                ready = true;
                                            }
                                            else
                                            {
                                                convertingMag = true;
                                                if (cancel)
                                                {
                                                    if (torrentDLING)
                                                    {
                                                        getJson($"magnet/delete?agent={apiNAME}&apikey={APIKEY}&id={magnetID}");
                                                        if (int.Parse(seeders) == 0)
                                                        {

                                                            if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                                                            {
                                                                ALTrayIcon.ShowBalloonTip(2000, "", $"Magnet removed from profile!", ToolTipIcon.None);

                                                            }
                                                            this.Invoke(() =>
                                                            {
                                                                Program.form.DownloadingText.Text = "Magnet removed from profile!";
                                                            });
                                                            this.Invoke(() =>
                                                            {
                                                                Program.form.DownloadingText.Text = $"";
                                                            });
                                                            this.Invoke(() =>
                                                            {
                                                                CancelButton.Visible = false;
                                                                DownloadingText.Text = $"";
                                                                dlProg.Value = 0;
                                                            });
                                                            torrentDLING = false;
                                                            fileDownloading = "";
                                                            CurrentlyShowingID = 0;
                                                        }
                                                        else
                                                        {
                                                            if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                                                            {
                                                                ALTrayIcon.ShowBalloonTip(2000, "", $"Cannot remove magnet that has already started downloading!", ToolTipIcon.None);

                                                            }
                                                            this.Invoke(() =>
                                                            {
                                                                Program.form.DownloadingText.Text = "Cannot remove magnet that has already started downloading!";
                                                            });
                                                        }
                                                    }
                                                    await Task.Delay(5000);
                                                }
                                                if (!alertedonce)
                                                {
                                                    alertedonce = true;
                                                    this.Invoke(() =>
                                                    {
                                                        if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                                                        {
                                                            ALTrayIcon.ShowBalloonTip(2000, "", $"Torrent not cached, now converting!", ToolTipIcon.None);

                                                        }

                                                        DownloadingText.Text = $"Torrent not cached, now converting!";


                                                    });
                                                }

                                                MagnetStatus = key.status.ToString();
                                                if (MagnetStatus.Equals("In Queue") && !secondalert)
                                                {
                                                    if (!secondalert)
                                                    {
                                                        secondalert = true;
                                                        await Task.Delay(1000);
                                                        this.Invoke(() =>
                                                        {
                                                            if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                                                            {
                                                                ALTrayIcon.ShowBalloonTip(2000, "", $"Gathering information on torrent...", ToolTipIcon.None);
                                                            }

                                                        });

                                                    }
                                                }
                                                if (MagnetStatus.Equals("Downloading"))
                                                {
                                                    await Task.Delay(3000);
                                                    if (!isDownloading)
                                                    {
                                                        CurrentlyShowingID = int.Parse(magnetID);
                                                        torrentDLING = true;
                                                        this.Invoke(() =>
                                                        {
                                                            CancelButton.Visible = true;

                                                        });

                                                        seeders = key.seeders.ToString();
                                                        DownloadSpeed = double.Parse(key.downloadSpeed.ToString());
                                                        double DownloadSpeed2 = (DownloadSpeed / 1024) / 1024;
                                                        string DLS;
                                                        DLS = "Speed:" + String.Format("{0:0.00}", DownloadSpeed2) + $" MB/s";


                                                        total = long.Parse(Utilities.KeepOnlyNumbers(key.size.ToString()));
                                                        DLsofar = long.Parse(Utilities.KeepOnlyNumbers(key.downloaded.ToString()));

                                                        int percentComplete = (int)Math.Round((double)(100 * DLsofar) / total);
                                                        if (percentComplete < 0)
                                                        {
                                                            percentComplete = 0;
                                                        }
                                                        this.Invoke(() =>
                                                        {

                                                            dlProg.Value = percentComplete;
                                                        });
                                                        const int MaxLength = 20;
                                                        var name = magnetName;
                                                        if (name.Length > MaxLength)
                                                            name = name.Substring(0, MaxLength);
                                                        this.Invoke(() =>
                                                        {
                                                            DownloadingText.Text = $"Downloading torrent: {name}. {percentComplete}% Seeds:{seeders}. {DLS}";
                                                        });

                                                    }
                                                }
                                                else if (MagnetStatus.Equals("Ready"))
                                                {
                                                    ready = true;
                                                    torrentDLING = false;
                                                    if (CurrentlyShowingID == int.Parse(magnetID)) CurrentlyShowingID = 0;
                                                    this.Invoke(() =>
                                                    {
                                                        if (torrentDLING && !isDownloading)
                                                        {
                                                            CancelButton.Visible = false;

                                                        }
                                                        if (torrentDLING)
                                                        {
                                                            torrentDLING = false;
                                                        }


                                                        if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                                                        {

                                                            ALTrayIcon.ShowBalloonTip(2000, "", $"{magnetName} finished downloading, now uploading to DDL links...", ToolTipIcon.None);

                                                        }
                                                        else
                                                        {

                                                            DownloadingText.Text = $"{magnetName} finshed downloading!";
                                                        }
                                                    });

                                                }
                                                else if (MagnetStatus.Equals("Uploading"))
                                                {
                                                    if (!isDownloading)
                                                    {
                                                        UploadSpeed = double.Parse(key.uploadSpeed.ToString());
                                                        double UploadSpeed2 = (UploadSpeed / 1024) / 1024;
                                                        string ULS;
                                                        ULS = "Speed: " + String.Format("{0:0.00}", UploadSpeed2) + $" MB/s";


                                                        total = long.Parse(Utilities.KeepOnlyNumbers(key.size.ToString()));
                                                        ULsofar = long.Parse(Utilities.KeepOnlyNumbers(key.uploaded.ToString()));

                                                        int percentComplete = (int)Math.Round((double)(100 * ULsofar) / total);
                                                        if (percentComplete < 0)
                                                        {
                                                            percentComplete = 0;
                                                        }
                                                        this.Invoke(() =>
                                                        {
                                                            dlProg.Value = percentComplete;
                                                        });
                                                        this.Invoke(() =>
                                                        {
                                                            DownloadingText.Text = $"Uploading to DDL links. {percentComplete}% complete. {ULS}";
                                                        });
                                                    }
                                                }
                                                await Task.Delay(10);
                                            }
                                            if (ready)
                                            {
                                                torrentDLING = false;
                                                convertingMag = false;
                                                this.Invoke(() =>
                                                {
                                                    DownloadingText.Text = "Magnet conversion finished!";
                                                });
                                                foreach (var key2 in key.links)
                                                {
                                                    bool skip = false;
                                                    var result = getJson($"link/unlock?agent={apiNAME}&apikey={APIKEY}&link={key2.link.ToString()}");
                                                    try
                                                    {
                                                        string unlockedLink = result.data.link.ToString();
                                                        if (Properties.Settings.Default.ExcludeURLS)
                                                        {
                                                            if (unlockedLink.ToLower().EndsWith("url".ToLower()))
                                                            {
                                                                skip = true;
                                                            }
                                                        }
                                                        string Size = result.data.filesize.ToString();
                                                        double FileSize = double.Parse(result.data.filesize.ToString());
                                                        string FileSizeInt = "";
                                                        if ((FileSize / 1024 / 1024) > 1000)
                                                        {
                                                            FileSizeInt = String.Format("{0:0.00}", (FileSize / 1024 / 1024 / 1024).ToString("0.00")) + "GB";

                                                        }
                                                        else
                                                        {
                                                            FileSizeInt = (String.Format("{0:0.00}", (FileSize / 1024 / 1024).ToString("0.00"))) + "MB";
                                                        }

                                                        string filename = key2.filename.ToString();

                                                        this.Invoke(() =>
                                                        {
                                                            foreach (ListViewItem item in listView1.Items)
                                                            {

                                                                if (item.SubItems[1].Text.Equals(unlockedLink) && item.SubItems[2].Text.Equals(magnetName))
                                                                {
                                                                    skip = true;

                                                                    if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)

                                                                        this.Invoke(() =>
                                                                        {
                                                                            Program.form.DownloadingText.Text = "Already in the queue! Skipping...";
                                                                        });
                                                                }
                                                            }
                                                            if (!skip)
                                                            {
                                                                if (Dict.ContainsKey(magnetName))
                                                                {
                                                                    if (Dict[magnetName] == Size)
                                                                    {
                                                                        this.Invoke(() =>
                                                                        {
                                                                            Program.form.DownloadingText.Text = "Files already in list... skipping!";
                                                                        });
                                                                        convertingMag = false;
                                                                        Dict.Clear();
                                                                        return;
                                                                    }
                                                                }
                                                                else if (!Dict.ContainsKey(magnetName))
                                                                {
                                                                    Dict.Add(magnetName, Size);
                                                                }
                                                                string[] row = { Utilities.RemoveEverythingBeforeLast(HttpUtility.UrlDecode(unlockedLink), "/").Replace("/", ""), unlockedLink, magnetName, FileSizeInt };
                                                                ListViewItem item = new ListViewItem(row);
                                                                listView1.Items.Add(item);
                                                                item.Checked = true;
                                                            }

                                                            if (listView1.Items.Count == 0 && !skip)
                                                            {
                                                                if (Dict.ContainsKey(magnetName))
                                                                {
                                                                    if (Dict[magnetName] == Size)
                                                                    {
                                                                        this.Invoke(() =>
                                                                        {
                                                                            Program.form.DownloadingText.Text = "Files already in list... skipping!";
                                                                        });
                                                                        convertingMag = false;
                                                                        Dict.Clear();
                                                                        return;
                                                                    }
                                                                }
                                                                else if (!Dict.ContainsKey(magnetName))
                                                                {
                                                                    Dict.Add(magnetName, Size);
                                                                }
                                                                this.Invoke(() =>
                                                                {
                                                                    listView1.Items.Add(new ListViewItem(new string[] { Utilities.RemoveEverythingBeforeLast(HttpUtility.UrlDecode(unlockedLink), "/").Replace("/", ""),
                                                                    unlockedLink, magnetName, FileSizeInt }));
                                                                });

                                                            }
                                                            skip = false;
                                                        });
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Console.WriteLine(ex.Message);
                                                    }
                                                }
                                                this.Invoke(() =>
                                                {
                                                    dlProg.Value = 0;
                                                });
                                                notdone = false;
                                                this.Invoke(() =>
                                                {
                                                    Program.form.DownloadingText.Text = "";
                                                });
                                            }
                                        }
                                    }
                                    notdone = false;
                                    convertingMag = false;
                                    if (cancel)
                                    {
                                        this.Invoke(() =>
                                        {
                                            CancelButton.Visible = false;

                                        });
                                    }


                                }
                                if (isErrors)
                                {
                                    this.Invoke(() =>
                                    {
                                        if (a > 0)
                                        {
                                            if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                                            {
                                                ALTrayIcon.ShowBalloonTip(2000, "", $"Magnet(s) not valid.", ToolTipIcon.Error);
                                            }
                                            else
                                                MessageBox.Show(new Form { TopMost = true }, $"{a1}\nMagnet(s) not valid.", "Invalid magnet.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                        if (b > 0)
                                        {
                                            if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                                            {
                                                ALTrayIcon.ShowBalloonTip(2000, "", $"Already have maximum allowed active magnets (30)", ToolTipIcon.Error);
                                            }
                                            else
                                                MessageBox.Show(new Form { TopMost = true }, $"{b1}\nAlready have maximum allowed active magnets (30).", "Cannot add over 30 torrents.", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                        }
                                        if (c > 0)
                                        {
                                            if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                                            {
                                                ALTrayIcon.ShowBalloonTip(2000, "", $"Server are not allowed to use this feature. Visit https://alldebrid.com/vpn if you're using a VPN", ToolTipIcon.Error);
                                            }
                                            else
                                                MessageBox.Show(new Form { TopMost = true }, $"{c1}\nServer are not allowed to use this feature. Visit https://alldebrid.com/vpn if you're using a VPN.", "LServer not allowed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    });


                                }

                            }
                            convertingMag = false;
                            if (Properties.Settings.Default.AutoDL)
                            {
                                this.Invoke(() =>
                                {
                                    startDownloads_Click();
                                });
                            }
                        }

                    }
                    catch
                    {

                    }
                });
                t1.Start();
                t1.IsBackground = true;
                while (convertingMag)
                {
                    if (cancel)
                    {
                        t1.Abort();
                        convertingMag = false;
                    }
                    await Task.Delay(100);
                }
                _workingOnPaste = false;
            }




            if (pasted.ToLower().StartsWith("https://filecrypt.".ToLower()) || pasted.ToLower().StartsWith("https://www.filecrypt.".ToLower()))
            {
                this.Invoke(() =>
                {
                    splashPanel.Visible = false;
                    if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                        ALTrayIcon.ShowBalloonTip(2000, "", $"Adding FileCrypt links...", ToolTipIcon.None);

                    else
                        DownloadingText.Text = "Adding FileCrypt links...";
                });

                CurrentDLC = randomNumber;
                pasted = pasted.Trim();
                string[] output = new string[] { pasted };
                if (pasted.Contains(";"))
                {
                    string[] pastedsplit = pasted.Split(';');
                    output = pastedsplit;
                }
                else if (pasted.Contains("\r\n"))
                {
                    pasted = pasted.Replace("\r\n", "\n");
                    string[] pastedsplit = pasted.Split('\n');
                    output = pastedsplit;
                }
                else if (pasted.Contains('\r'))
                {
                    string[] pastedsplit = pasted.Split('\r');
                    output = pastedsplit;
                }
                else if (pasted.Contains('\n'))
                {
                    string[] pastedsplit = pasted.Split('\n');
                    output = pastedsplit;
                }
                foreach (string s in output)
                {

                    try
                    {
                        FilecryptURL = s;
                        filecryptinprog = true;
                        Form WebFormForm = new WebFormForm();
                        WebFormForm.ShowDialog();
                        this.Show();
                        this.TopMost = false;
                        Utilities.DecryptDLC();
                    }
                    catch { }
                }
            }
            else if (pasted.ToLower().StartsWith("https://".ToLower()))
            {
                try
                {
                    bool convertingLinks = true;
                    pasted = pasted.Trim();
                    string[] output = new string[] { pasted };
                    if (pasted.Contains(";"))
                    {
                        string[] pastedsplit = pasted.Split(';');
                        output = pastedsplit;

                    }
                    else if (pasted.Contains("\r\n"))
                    {
                        pasted = pasted.Replace("\r\n", "\n");
                        string[] pastedsplit = pasted.Split('\n');
                        output = pastedsplit;
                    }
                    else if (pasted.Contains('\r'))
                    {
                        string[] pastedsplit = pasted.Split('\r');
                        output = pastedsplit;
                    }
                    else if (pasted.Contains('\n'))
                    {
                        string[] pastedsplit = pasted.Split('\n');
                        output = pastedsplit;
                    }
                    bool isErrors = false;
                    int linkdown = 0;
                    string linkdowns = "";
                    int linkmissing = 0;
                    string linkmissings = "";
                    int linkpass = 0;
                    string linkpasss = "";
                    int linknotsup = 0;
                    string linknotsups = "";
                    Thread t1 = new Thread(() =>
                    {

                        foreach (string s in output)
                        {
                            if (s.ToLower().Contains(".debrid.it"))
                            {
                                convertingMag = false;
                                fileDownloading = "";
                                _workingOnPaste = false;
                                break;
                            }
                            string s2 = s; ;
                            if (s.ToLower().Contains("mega."))
                            {
                                s2 = Uri.EscapeDataString(s2);
                            }
                       
                            string unlockString = $"link/unlock?agent={apiNAME}&apikey={APIKEY}&link={s2}";
                            var obj = getJson(unlockString);
                            if (obj.status.ToString().Equals("error"))
                            {
                                isErrors = true;
                                if (obj.error.code.ToString().Equals("LINK_DOWN"))
                                {
                                    linkdown++;
                                    linkdowns += s + "\n";
                                }

                                if (obj.error.code.ToString().Equals("LINK_IS_MISSING"))
                                {
                                    linkmissing++;
                                    linkmissings += s + "\n";
                                }
                                if (obj.error.code.ToString().Equals("LINK_PASS_PROTECTED"))
                                {
                                    int tries = 0;
                                    while (isErrors)
                                    {
                                        string[] passwords = Properties.Settings.Default.ZipPWS.Split(';');
                                        try
                                        {
                                            foreach (string PW in passwords)
                                            {
                                                unlockString = $"link/unlock?agent={apiNAME}&apikey={APIKEY}&link={s2}&password={PW}";
                                                obj = getJson(unlockString);
                                                if (!obj.status.ToString().Equals("error"))
                                                {
                                                    isErrors = false;
                                                    _workingOnPaste = false;
                                                    break;
                                                }
                                                else
                                                    isErrors = true;
                                                tries++;
                                            }
                                        }
                                        catch
                                        {
                                            MessageBox.Show("Link is passworded!" +
                                                "\n\nIf you have any commonly used passwords set none of them were correct." +
                                                "\n\nTry finding the password for this link and adding it to commonly used passwords!");
                                        }
                                        if (tries == passwords.Length)
                                        {

                                            isErrors = true;
                                            _workingOnPaste = false;
                                            break;
                                        }
                                        if (!isErrors)
                                        {
                                            _workingOnPaste = false;
                                            break;
                                        }
             
                                    }
                                }
                                if (obj.error.code.ToString().Equals("LINK_HOST_NOT_SUPPORTED"))
                                {
                                    linknotsup++;
                                    linknotsups += s + "\n";
                                }
                            }
                            if (!isErrors)
                            {
                                this.Invoke(() =>
                                {
                                    splashPanel.Visible = false;
                                });
                                int addedlinks = 0;
                                string unlockedLink = obj.data.link.ToString();
                                string Size = obj.data.filesize.ToString();
                                double FileSize = double.Parse(obj.data.filesize.ToString());
                                string FullFileName = obj.data.filename.ToString();
                                string FileNameNoExt = Utilities.RemoveEverythingAfterLast(FullFileName, ".");
                                if (FileNameNoExt.Contains(".part") || FileNameNoExt.Contains(".7z") || FileNameNoExt.Contains(".zip"))
                                {
                                    FileNameNoExt = Utilities.RemoveEverythingAfterLast(FileNameNoExt, ".");
                                }
                                string FileSizeInt = "";
                                if ((FileSize / 1024 / 1024) > 1000)
                                {
                                    FileSizeInt = String.Format("{0:0.00}", (FileSize / 1024 / 1024 / 1024).ToString("0.00")) + "GB";

                                }
                                else
                                {
                                    FileSizeInt = (String.Format("{0:0.00}", (FileSize / 1024 / 1024).ToString("0.00"))) + "MB";
                                }

                                bool skip = false;
                                this.Invoke(() =>
                                {
                                    if (Properties.Settings.Default.ExcludeURLS && unlockedLink.ToLower().EndsWith(".url".ToLower()))
                                    {
                                        skip = true;
                                    }
                                    foreach (ListViewItem item in listView1.Items)
                                    {
                                        if (item.SubItems[0].Text.Equals(obj.data.filename.ToString()) && item.SubItems[2].Text.Equals(FileNameNoExt))
                                        {
                                            skip = true;
                                        }
                                    }
                                    if (!skip)
                                    {
                                        if (Dict.ContainsKey(FullFileName))
                                        {
                                            if (Dict[FullFileName] == Size)
                                            {
                                                skip = true;
                                                this.Invoke(() =>
                                                {
                                                    Program.form.DownloadingText.Text = "Files already in list... skipping!";
                                                });
                                                _workingOnPaste = false;
                                                return;
                                            }
                                        }
                                        else if (!Dict.ContainsKey(FullFileName))
                                        {
                                            Dict.Add(FullFileName, Size);
                                        }
                                        addedlinks++;
                                        if (!skip)
                                        {
                                            if (WebFormForm.isDLC && DLCCountLocal < Utilities.DLCCount)
                                            {
                                                DLCCountLocal++;
                                                FileNameNoExt = WebFormForm.dlcGroup;
                                            }
                                            if (WebFormForm.isDLC && DLCCountLocal == Utilities.DLCCount)
                                            {
                                                WebFormForm.isDLC = false;
                                                WebFormForm.dlcGroup = "";
                                                Utilities.DLCCount = 0;
                                            }
                                            listView1.Items.Add(new ListViewItem(new string[] { Utilities.RemoveEverythingBeforeLast(HttpUtility.UrlDecode(unlockedLink), "/").Replace("/", ""),
                                            unlockedLink, FileNameNoExt, FileSizeInt }));
                                            foreach (ListViewItem item in listView1.Items)
                                            {
                                                if (item.SubItems[1].Text.Equals(unlockedLink))
                                                {
                                                    item.Checked = true;
                                                }
                                            }
                                        }
                                        skip = false;
                                    }
                                });

                            }
                            if (isErrors)
                            {
                                this.Invoke(() =>
                                {
                                    if (linkdown > 0)
                                    {
                                        if (!Properties.Settings.Default.DisableNotifies)
                                        {
                                            ALTrayIcon.ShowBalloonTip(2000, "", $"Link(s) have been taken down from the filehoster's website.\n\nNot adding!", ToolTipIcon.None);
                                        }

                                        DownloadingText.Text = "Link(s) have been taken down from the filehoster's website.\n\nNot adding!";
                                    }
                                    if (linkmissing > 0)
                                    {
                                        if (!Properties.Settings.Default.DisableNotifies)
                                        {
                                            ALTrayIcon.ShowBalloonTip(2000, "", $"Links missing.\n\nNot adding!", ToolTipIcon.None);

                                        }

                                        DownloadingText.Text = "Links missing.\n\nNot adding!";
                                    }
                                    if (linkpass > 0)
                                    {
                                        if (!Properties.Settings.Default.DisableNotifies)
                                        {
                                            ALTrayIcon.ShowBalloonTip(2000, "", $"Link(s) are password protected on the filehost's website.\n\nNot adding!", ToolTipIcon.None);
                                        }

                                        DownloadingText.Text = "Link(s) are password protected on the file hoster site.\n\nNot adding!";
                                    }
                                    if (linknotsup > 0)
                                    {
                                        if (!Properties.Settings.Default.DisableNotifies)
                                        {
                                            ALTrayIcon.ShowBalloonTip(2000, "", $"Link(s) are from an unsupported host.\n\nPlease check this link for a list of supported filehosters:\nhttps://alldebrid.com/hosts/", ToolTipIcon.None);
                                        }
                                        DownloadingText.Text = "Link(s) are not supported by All Debrid!";
                                    }
                                });
                            }
                        }
                        convertingLinks = false;
                    });
                    t1.Start();
                    t1.IsBackground = true;
                    while (t1.IsAlive)
                    {
                        if (cancel)
                        {
                            t1.Abort();
                            convertingLinks = false;
                        }
                        await Task.Delay(100);
                    }
                }
                catch (Exception ex)
                {
                    this.Invoke(() =>
                    {

                        if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                        {
                            ALTrayIcon.ShowBalloonTip(2000, "", $"Pasted information is not suppported.", ToolTipIcon.None);
                        }
                        else
                            DownloadingText.Text = "Pasted information is not suppported.";
                    });
                }



            }


            if (filecryptinprog)
            {
                string[] files = Directory.GetFiles($"{Environment.CurrentDirectory}\\_bin", "*.txt", SearchOption.TopDirectoryOnly);
                string linkstoget = "";
                foreach (string file in files)
                {
                    if (file.Contains(".txt"))
                    {
                        string[] list = File.ReadAllLines(file);
                        foreach (string line in list)
                        {
                            if (!line.Contains("Urls stored"))
                            {
                                linkstoget += line + "\n";
                            }

                        }
                        file.FileRecycle();
                    }
                }
                if (!String.IsNullOrWhiteSpace(linkstoget))
                {
                    Clipboard.SetText(linkstoget);
                    DoAsyncConversion();
                    linkstoget = "";
                    filecryptinprog = false;
                }

            }
            CancelButton.Visible = false;
            DownloadingText.Text = "";
            if (Properties.Settings.Default.AutoDL)
            {
                object sender = null;
                EventArgs e = new EventArgs();
                startDownloads_Click(sender, e);
            }
            _workingOnPaste = false;
        }


        protected override bool ProcessCmdKey(ref Message msg, System.Windows.Forms.Keys keyData)
        {
            if (keyData == (System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V))
            {
                    DoAsyncConversion();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        public static string dlsPara = "";
        public static string dlsGoin = "";
        public static string DLSDir = "";
        public static bool isDownloading = false;
        public static bool isMultiPart = false;
        public static List<string> downloadedGroups = null;

        public async void startDownloads_Click(object sender = null, EventArgs e = null)
        {
            if (isDownloading)
            {
                return;
            }
            endreached = false;
            this.Invoke(() =>
            {
                if (listView1.CheckedItems.Count == 0)
                {
                    endreached = true;
                    return;
                }
                CancelButton.Visible = true;
            });

            if (listView1.CheckedItems.Count > 0)
            {
                foreach (ListViewItem item in listView1.CheckedItems)
                {
                    if (!String.IsNullOrEmpty(currentGroup))
                    {
                        currentGroup = item.SubItems[2].Text;
                    }


                    try
                    {
                        this.Invoke(() =>
                        {
                            if (item.SubItems.Count > 0)
                            {
                                this.Text = $"Downloading {item.SubItems[0].Text}...";
                                DownloadingText.Text = $"Downloading {item.SubItems[0].Text}...";
                            }
                        });
                        isDownloading = true;
                        item.BackColor = Color.MediumSpringGreen;
                        //
                        //HERES WHERE IT ADDS THEM TO THE LIST
                        //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
                        await downloadFiles(item.SubItems[1].Text, item.SubItems[0].Text, item.SubItems[2].Text.Replace(";", ""));
                        while (webClient.IsBusy)
                        {
                            await Task.Delay(100);
                        }
                        DLSDir = Properties.Settings.Default.DownloadDir + "\\" + item.SubItems[2].Text.Replace(";", "");

                        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                        //HERES WHERE IT ADDS THEM TO THE LIST
                        //

                        if (!webClient.IsBusy)
                        {
                            this.Invoke(() =>
                            {
                                CancelButton.Visible = false;
                                Program.form.DownloadingText.Text = $"";
                                dlProg.Value = 0;
                            });
     
                        }
                        item.BackColor = Color.FromArgb(0, 50, 42);
                    }
                    catch (System.Net.WebException Ex)
                    {
                        if (Ex.Message.Contains("The remote server returned"))
                        {
                            this.Invoke(() =>
                            {
                                this.Text = $"Issue with the server such as the file was removed or your internet is acting up.... try again later!";
                            });
                        }
                    }
                }
                while (isDownloading)
                {
                    await Task.Delay(100);
                }
                if (!isDownloading && !muteoutputcancelled)
                {
                    this.Invoke(() =>
                    {
                        if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                        {
                            ALTrayIcon.ShowBalloonTip(2000, "", $"Downloads finished!", ToolTipIcon.None);
                        }
                        else
                            DownloadingText.Text = $"Downloads finished!";
                    });
                    this.Invoke(() =>
                    {
                        this.Text = "";
                        dlProg.Value = 0;

                        DownloadingText.Text = "";
                    });


                    CancelButton.Visible = false;
                    if (listView1.CheckedItems.Count == 0)
                    {
                        Dict.Clear();
                        dlsGoin = "";
                        if (Properties.Settings.Default.AutoExtract)
                        {

                            this.Invoke(() =>
                            {
                                if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                                {
                                    ALTrayIcon.ShowBalloonTip(5000, "", $"Downloads complete, extracting zips...", ToolTipIcon.None);
                                }
                                else
                                    DownloadingText.Text = $"Downloads complete, extracting zips...";
                            });

                            AutoExtraction();
                            if (Properties.Settings.Default.extractNested)
                            {
                                ExtractNestedPath(MainDLDir);
                            }
                            if (Properties.Settings.Default.DelZips)
                            {
                                AutoDelete(IgnoreList);
                            }
                            IgnoreList.Clear();
                        }
                        else
                        {

                            this.Invoke(() =>
                            {
                                if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                                {
                                    ALTrayIcon.ShowBalloonTip(5000, "", $"Downloads complete!", ToolTipIcon.None);
                                }
                                else
                                    DownloadingText.Text = $"Downloads complete!";
                            });
                        }


                        if (Properties.Settings.Default.OpenDir)
                        {
                            if (tempNoOpen)
                            {
                                tempNoOpen = false;
                            }
                            else
                            {
                                if (Directory.Exists(MainDLDir))
                                {
                                    Process.Start(MainDLDir);
                                }
                                else
                                {
                                    Process.Start(Properties.Settings.Default.DownloadDir);
                                }
                            }
                        }
                        MainDLDir = "";
                    }
                }
                isMultiPart = false;
            }
            fileDownloading = "";
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            muteoutputcancelled = false;
            fileDownloading = "";
            currentGroup = "";
            cancelledGroup = "";
            endreached = true;
            Utilities.FailedExtract = "";
            startDownloads.Enabled = true;

        }

        public void AutoDelete(List<string> IgnoreList)
        {
            foreach (string item in IgnoreList)
            {
                item.FileRecycle();
            }
        }


        public static List<string> IgnoreList = new List<string>();
        public void AutoExtraction()
        {
            if (AutoExtract.Checked)
            {
                this.Invoke(() =>
                {
                    string DLDir = "";
                    string DLFile = "";
                    ExtractedZips = "";
                    string[] SplitDLList = DLList.Split('\n');
                    DLList = "";
                    foreach (string FullDL in SplitDLList)
                    {
                        if (!String.IsNullOrWhiteSpace(FullDL))
                        {
                            string[] DLS = FullDL.Split(';');
                            DLDir = DLS[1];
                            DLFile = DLS[0];
                            string DL = $"{DLS[1]}\\{DLFile}";
                            IgnoreList.Add(DL);
                            if (DLFile.ToLower().Contains(".7z.".ToLower()) || DLFile.ToLower().Contains(".part".ToLower()) && DLFile.ToLower().EndsWith(".rar".ToLower()))
                            {
                                isMultiPart = true;
                            }
                            bool Extract = false;
                            string DLSExtension = Path.GetExtension(DLFile);

                            if (!isMultiPart)
                            {
                                if (DLSExtension.Equals(".7z") || DLSExtension.Equals(".zip") || DLSExtension.Equals(".rar")
                                       || DLSExtension.Equals(".gz") || DLSExtension.Equals(".tar"))
                                {
                                    Extract = true;
                                }
                            }
                            if (isMultiPart && DLFile.EndsWith(".part1.rar".ToLower()) || DLFile.EndsWith(".part 1.rar".ToLower()) || DLFile.EndsWith(".7z.001"))
                            {
                                isMultiPart = true;
                                Extract = true;
                                string DLFileBase = DLFile.Replace(".part1.rar", "").Replace(".7z.001", "").Replace(".part 1.rar", "");
                                bool allPartsDone = false;
                                while (!allPartsDone)
                                {
                                    allPartsDone = true;
                                    foreach (ListViewItem item in listView1.CheckedItems)
                                    {
                                        if (item.SubItems[0].Text.ToLower().Contains(DLFileBase.Trim().ToLower()))
                                        {
                                            allPartsDone = false;
                                        }
                                    }
                                    if (allPartsDone)
                                    {
                                        if (!Directory.Exists(DLDir))
                                        {
                                            Directory.CreateDirectory(DLDir);
                                        }
                                        else
                                        {
                                            Utilities.ExtractFile(DL, DLDir);
                                            ExtractedZips += DL + ";";
                                            Extract = false;
                                        }
                                    }
                                }
                            }
                            else if (isMultiPart)
                            {
                                IgnoreList.Add(DLFile);
                                continue;
                            }
                            if (Extract && !isMultiPart && !muteoutputcancelled)
                            {
                                if (!Directory.Exists(DLDir))
                                {
                                    Directory.CreateDirectory(DLDir);
                                }
                                Utilities.ExtractFile(DL, DLDir);
                                ExtractedZips += DL + ";";
                                Extract = false;
                            }

                        }
                    }
                    DLList = "";
                    dlsPara = "";
                });
                this.Invoke(() =>
                {
                    DownloadingText.Text = $"";
                });
            }
        }

        public void ExtractNestedPath(string MainDLDir)
        {
            if (Properties.Settings.Default.extractNested && !String.IsNullOrEmpty(MainDLDir))
            {
                string[] directories = Directory.GetDirectories(MainDLDir);
                foreach (string directory in directories)
                {
                    string[] files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        bool IsArchive = Utilities.IsArchive(file);
                        if (IsArchive)
                        {
                            if (IgnoreList.Contains(file))
                            {
                                continue;
                            }
                            else
                            {
                                IgnoreList.Add(file);
                                if (!Directory.Exists(MainDLDir + "\\" + Path.GetFileNameWithoutExtension(file)))
                                {
                                    Directory.CreateDirectory(MainDLDir + "\\" + Path.GetFileNameWithoutExtension(file));
                                }
                                Utilities.ExtractFile(file, MainDLDir + "\\" + Path.GetFileNameWithoutExtension(file));
                                ExtractedZips += $"{file};";
                            }
                        }
                    }
                }
            }
        }
        private void CopyLinks_Click(object sender, EventArgs e)
        {
            if (listView1.CheckedItems.Count > 0)
            {
                string forclip = "";
                listView1.BeginUpdate();
                foreach (ListViewItem item in listView1.Items)
                {
                    if (item.Checked)
                    {
                        forclip += item.SubItems[1].Text + "\n";

                        this.Invoke(() =>
                        {
                            if (item.SubItems[2].Text.Equals(currGroup))
                            {
                                if (Properties.Settings.Default.RemDL)
                                {
                                    listView1.Items.Remove(item);
                                }
                                else
                                {
                                    item.Checked = false;
                                }
                            }
                        });
                    }
                }
                listView1.EndUpdate();
                forclip = forclip.Trim('\r', '\n');
                try
                {
                    Clipboard.SetText(forclip);
                }
                catch
                {
                    MessageBox.Show(new Form { TopMost = true }, "Clipboard was in use and could not be set!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show(new Form { TopMost = true }, "Please select items to download or hit clear.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void ClearButton_Click(object sender, EventArgs e)
        {
            listView1.BeginUpdate();
            foreach (ListViewItem item in listView1.CheckedItems)
            {
                listView1.Items.Remove(item);
            }
            listView1.EndUpdate();
            listView1.Refresh();
        }

        private void SetDLDIR_Click(object sender, EventArgs e)
        {
            string DLDir = "";
            FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
            folderSelectDialog.Title = "Select All Legit download directory...";
            if (folderSelectDialog.Show(Handle))
            {
                DLDir = folderSelectDialog.FileName;
            }
            else
            {
                return;
            }
            Properties.Settings.Default.RegistrySet = false;
            Properties.Settings.Default.NoNag = false;
            if (!DLDir.EndsWith("All Legit Downloads"))
            {
                DLDir += "\\All Legit Downloads";
                DLDir = DLDir.Replace("\\\\", "\\");
            }
            Properties.Settings.Default.DownloadDir = DLDir;
            Properties.Settings.Default.Save();
            DownloadDir.Text = DLDir;
            Process proc = new Process();

            try
            {
                //check if we are running as administrator currently

                DialogResult yesnoQ = MessageBox.Show("Administrator rights are required to properly register your downloads path. Once it is registered then it is set until you change it. Please click YES to elevate All Legit and register your download directory.\n\nNOTE: If you click NO we will not ask you again and it might cause download conflicts in the future. For best results click yes or press cancel to be reminded on next launch.", "Relaunch as administrator?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (yesnoQ == DialogResult.Yes)
                {

                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    proc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
                    //Start new process as administrator
                    proc.StartInfo.FileName = "All.Legit.exe";
                    proc.StartInfo.Verb = "runas";
                    proc.StartInfo.Arguments = "";
                    proc.Start();
                    //Exit current process
                    Environment.Exit(0);

                }
                else if (yesnoQ == DialogResult.No)
                {
                    MessageBox.Show("OK we will not ask again.");
                    Properties.Settings.Default.RegistrySet = true;
                    Properties.Settings.Default.NoNag = true;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    Properties.Settings.Default.RegistrySet = false;
                    Properties.Settings.Default.Save();
                }
            }
            //if user selects "no" from adminstrator request.
            catch
            {
                MessageBox.Show("Administrator access was denied, please allow All Legit to store your downloads directory by accepting the admin prompt.");
            }

            if (!Directory.Exists(DLDir))
            {
                Directory.CreateDirectory(DLDir);
            }
        }
        public static bool torrentDLING = false;

        private void RemDL_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.RemDL = RemDL.Checked;
            Properties.Settings.Default.Save();
            if (RemDL.Checked)
            {
                RemDL.ForeColor = Color.PaleTurquoise;
            }
            else
            {
                RemDL.ForeColor = Color.SteelBlue;
            }
        }
        public static bool muteoutputcancelled = Properties.Settings.Default.DisableNotifies;
        public static bool cancel = false;
        public static bool iscancelling = false;
        public static bool tempNoOpen = false;
        public static string cancelledGroup = "";
        public void CancelButton_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.OpenDir)
            {
                tempNoOpen = true;
            }
            startDownloads.Enabled = false;
            CancelButton.Visible = false;
            cancel = true;
            this.Invoke(() =>
            {
                CancelButton.Visible = false;
                Program.form.DownloadingText.Text = $"";
                dlProg.Value = 0;
            });
            isDownloading = false;
            cancelledGroup = currGroup;
            muteoutputcancelled = true;
            webClient.CancelAsync();
            if (cancel)
            {
                listView1.BeginUpdate();
                foreach (ListViewItem itemstogo in listView1.CheckedItems)
                {
                    if (itemstogo.SubItems[2].Text.Equals(currGroup))
                    {
                        if (Properties.Settings.Default.RemDL)
                        {
                            listView1.Items.Remove(itemstogo);
                        }
                        else
                        {
                            itemstogo.Checked = false;
                        }
                    }
                }
                listView1.EndUpdate();
            }
            startDownloads.Enabled = true;

        }

        private void PasteButton_Click(object sender, EventArgs e)
        {
            string cb = Clipboard.GetText();
            if (cb.ToLower().StartsWith("https://") || cb.ToLower().StartsWith("magnet:?"))
            {
                DoAsyncConversion();
            }
        }

        private void DownloadDir_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)System.Windows.Forms.Keys.Enter)
            {
                if (!DownloadDir.Text.Equals(Properties.Settings.Default.DownloadDir))
                {
                    DialogResult answer = MessageBox.Show(new Form { TopMost = true }, $"Apply current text as download directory?\n\n{DownloadDir.Text}", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (answer == DialogResult.OK)
                    {
                        Properties.Settings.Default.DownloadDir = DownloadDir.Text;
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }

        public bool INPWBOX = false;
        private void PWBox_Enter(object sender, EventArgs e)
        {
            INPWBOX = true;
            if (PWBox.Text == "your;common;zip;passwords")
            {
                PWBox.Text = "";
            }

        }

        private void PWBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape)
            {
                settingsP.Focus();
            }
            if (e.KeyChar == (char)System.Windows.Forms.Keys.Enter && PWBox.Text.Length > 0 && !PWBox.Text.Equals("your;common;zip;passwords"))
            {
                if (PWBox.Text.Length > 0)
                {
                    Properties.Settings.Default.ZipPWS = PWBox.Text;
                    Properties.Settings.Default.Save();
                    PWLIST = PWBox.Text;
                }
                else
                {
                    PWBox.Text = "your;common;zip;passwords";
                    listView1.Focus();
                }
                if (PWBox.Text == "")
                {
                    if (String.IsNullOrWhiteSpace(Properties.Settings.Default.ZipPWS))
                    {


                        PWBox.Text = "your;common;zip;passwords";
                    }
                }
            }
        }

        private void PWBox_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(PWBox.Text))
            {
                if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.ZipPWS))
                {
                    PWBox.Text = Properties.Settings.Default.ZipPWS;
                }
                else
                {
                    PWBox.Text = "your;common;zip;passwords";
                }

            }
            else if (!PWBox.Text.Equals("your;common;zip;passwords") && PWBox.Text.Length > 0)
            {
                Properties.Settings.Default.ZipPWS = PWBox.Text;
                Properties.Settings.Default.Save();
                PWLIST = PWBox.Text;
            }
            PWBox.Update();
            listView1.Focus();
        }

        private void DownloadDir_Leave(object sender, EventArgs e)
        {
            if (!DownloadDir.Text.Equals(Properties.Settings.Default.DownloadDir))
            {
                DialogResult answer = MessageBox.Show(new Form { TopMost = true }, $"Apply current text as download directory?\n\n{DownloadDir.Text}", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (answer == DialogResult.OK)
                {
                    if (!String.IsNullOrWhiteSpace(DownloadDir.Text))
                    {
                        Properties.Settings.Default.DownloadDir = DownloadDir.Text;
                        Properties.Settings.Default.Save();
                    }

                }
            }
        }

        private void HotKeyBtn_Click(object sender, EventArgs e)
        {
            if (HotKeyBtn.Text != "Hold Ctrl/Shift/Alt + press letter key...")
            {
                HotKeyBox.Text = "Hold Ctrl/Shift/Alt + press letter key...";
                HotKeyBox.Focus();
                return;
            }
        }

        public static Random random = new Random();
        public static int randomNumber = random.Next(0, 99999);
        public static int CurrentDLC = 0;

        private void MainWindow_Resize_1(object sender, EventArgs e)
        {
            //if the form is minimized  
            //hide it from the task bar  
            //and show the system tray icon (represented by the NotifyIcon control)  
            if (WindowState == FormWindowState.Minimized)
            {
                TrayNotify = true;
            }
            else
            {
                TrayNotify = false;
            }
            if (listView1.Items.Count == 0)
            {
                return;
            }
            listView1.BeginUpdate();
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.EndUpdate();
            listView1.Refresh();
        }


        public static bool NotifiedTray = false;
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Properties.Settings.Default.Close2Tray)
            {
                if (TrayExit)
                {
                    ALTrayIcon.Visible = false;
                    ALTrayIcon.Dispose();
                    Program.mutex.Close();
                    try
                    {
                        Application.ExitThread();
                    }
                    catch
                    {
                    }
                    this.Close();
                }
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    if (!NotifiedTray)
                    {
                        ALTrayIcon.BalloonTipText = "All Legit! is still running in your system tray!";
                    }
                    NotifiedTray = true;
                    ALTrayIcon.BalloonTipIcon = ToolTipIcon.None;
                    if (!Properties.Settings.Default.DisableNotifies)
                    {
                        ALTrayIcon.ShowBalloonTip(2000);

                    }
                    this.Hide();
                    e.Cancel = true;
                }
            }
            else
            {
                ALTrayIcon.Visible = false;

                ALTrayIcon.Icon.Dispose();
                ALTrayIcon.Dispose();
                Program.mutex.Close();
                if (Application.OpenForms[0].InvokeRequired)
                {
                    Application.Exit();
                }
            }
        }

        private void ALTrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void showSettings_MouseEnter(object sender, EventArgs e)
        {
            stopwatch.Stop();
            settingsP.Visible = true;
            settingsP.BringToFront();
        }

        public static bool InSettings = false;
        public static bool InSettingsP = false;
        public static bool TrayNotify = false;
        public Stopwatch stopwatch = Stopwatch.StartNew();
        private EventArgs e;
        private object sender;

        public string ExtractedZips { get; private set; }
        public static string MainDLDir { get; set; }

        public async void settingsP_MouseLeave(object sender, EventArgs e)
        {
            if (settingsP.Visible)
            {


                TrayNotify = true;
                if (PWBox.Focused || ApiNameMan.Focused || ApiKeyMan.Focused) return;
                ApiManPanel.Visible = false;
                if (stopwatch.IsRunning) return;
                stopwatch.Restart();
                while (stopwatch.ElapsedMilliseconds < 500)
                {
                    await Task.Delay(100);
                }
                stopwatch.Stop();
                stopwatch.Reset();
                settingsP.Visible = false;
            }
            }

        private void showSettings_MouseHover(object sender, EventArgs e)
        {
            settingsP.Visible = true;
        }

        private void settingsP_MouseHover(object sender, EventArgs e)
        {
            settingsP.Visible = true;
        }

        private void StayOnTopCheckbox_CheckedChanged_1(object sender, EventArgs e)
        {
            Properties.Settings.Default.TopMost = StayOnTopCheckbox.Checked;
            Properties.Settings.Default.Save();
            if (StayOnTopCheckbox.Checked)
            {
                this.TopMost = true;
            }
            else
            {
                this.TopMost = false;
            }

        }

        private void Close2Tray_CheckedChanged_1(object sender, EventArgs e)
        {
            if (Close2Tray.Checked)
            {
                Close2Tray.ForeColor = Color.PaleTurquoise;
            }
            else
            {
                Close2Tray.ForeColor = Color.SteelBlue;
            }
            Properties.Settings.Default.Close2Tray = Close2Tray.Checked;
            Properties.Settings.Default.Save();
        }

        private void AutoExtract_CheckedChanged_1(object sender, EventArgs e)
        {
            if (AutoExtract.Checked)
            {
                extractNested.Enabled = true;
                AutoExtract.ForeColor = Color.PaleTurquoise;
                PWBox.Enabled = true;
            }
            else
            {
                if (extractNested.Checked)
                {
                    extractNested.Checked = false;
                }
                AutoExtract.ForeColor = Color.SteelBlue;
                PWBox.Enabled = false;
                extractNested.Enabled = false;

            }
            Properties.Settings.Default.AutoExtract = AutoExtract.Checked;
            Properties.Settings.Default.Save();
        }

        private void autoDelZips_CheckedChanged_1(object sender, EventArgs e)
        {
            if (autoDelZips.Checked)
            {
                autoDelZips.ForeColor = Color.PaleTurquoise;
            }
            else
            {
                autoDelZips.ForeColor = Color.SteelBlue;
            }
            Properties.Settings.Default.DelZips = autoDelZips.Checked;
            Properties.Settings.Default.Save();
        }
        private void AutoDLBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoDL = AutoDLBox.Checked;
            Properties.Settings.Default.Save();
            if (AutoDLBox.Checked)
            {
                AutoDLBox.ForeColor = Color.PaleTurquoise;
            }
            else
            {
                AutoDLBox.ForeColor = Color.SteelBlue;
            }
        }

        private void AutoDelete_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoOverwrite = AutoOverwrite.Checked;
            Properties.Settings.Default.Save();
            if (AutoOverwrite.Checked)
            {
                AutoOverwrite.ForeColor = Color.PaleTurquoise;
            }
            else
            {
                AutoOverwrite.ForeColor = Color.SteelBlue;
            }
        }

        private void UncheckAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Checked)
                {
                    item.Checked = false;
                }
            }
            if (listView1.Items.Count == listView1.CheckedItems.Count)
            {
                CheckAll.Visible = false;
            }
            else
            {
                CheckAll.Visible = true;
            }
        }

        private void CheckAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                if (!item.Checked)
                {
                    item.Checked = true;
                }
            }
            if (listView1.Items.Count == listView1.CheckedItems.Count)
            {
                CheckAll.Visible = false;
            }
            else
            {
                CheckAll.Visible = true;
            }
        }


        private void EnableButtons()
        {
            ClearButton.Enabled = true;
            CopyLinks.Enabled = true;
            linksToTextButton.Enabled = true;
        }       
        
        private void DisableButtons()
        {
            ClearButton.Enabled = false;
            CopyLinks.Enabled = false;
            linksToTextButton.Enabled = false;
        }
        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (listView1.CheckedItems.Count == 0)
            {
                DisableButtons();
            }
            else
            {
                EnableButtons();
            }
            if (listView1.Items.Count == listView1.CheckedItems.Count)
            {
                CheckAll.Visible = false;
            }
            else
            {
                CheckAll.Visible = true;
            }
        }

        private void disableNotiesBox_CheckedChanged(object sender, EventArgs e)
        {
            menuItem2.Checked = disableNotiesBox.Checked;
            Properties.Settings.Default.DisableNotifies = disableNotiesBox.Checked;
            Properties.Settings.Default.Save();

            if (disableNotiesBox.Checked)
            {

                disableNotiesBox.ForeColor = Color.PaleTurquoise;
            }
            else
            {

                disableNotiesBox.ForeColor = Color.SteelBlue;
            }
        }

        private void ALTrayIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void ALTrayIcon_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void OpenDirBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.OpenDir = OpenDirBox.Checked;
            Properties.Settings.Default.Save();
        }

        public void HotKeyBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.None)
            {
                return;
            }
            var converter = new KeysConverter();
            if (e.KeyCode == Keys.Enter)
            {

            }
            if (e.KeyCode == Keys.Escape)
            {
                listView1.Focus();
                HotKeyBox.Text = converter.ConvertToString(Properties.Settings.Default.HotKeyKeyData);
            }
            else if (e.KeyCode != Keys.Back)
            {
                Keys modifierKeys = e.Modifiers;
                Keys pressedKey = e.KeyData ^ modifierKeys; //remove modifier keys

                if (modifierKeys != Keys.None && pressedKey != Keys.None)
                {
                    HotKeyBox.Text = converter.ConvertToString(e.KeyData);
                    //At this point, we know a one or more modifiers and another key were pressed
                    //modifierKeys contains the modifiers
                    //pressedKey contains the other pressed key
                    //Do stuff with results here
                    hotkeyset = e.KeyData;
                    KeyUp += new KeyEventHandler(HotKeyBox_KeyUp);
                    KeyPreview = true;
                }
            }
            else
            {
                e.Handled = false;
                e.SuppressKeyPress = true;
                HotKeyBox.Text = converter.ConvertToString(Properties.Settings.Default.HotKeyKeyData);
            }

        }

        private void HotKeyBox_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(HotKeyBox.Text))
            {
                var converter = new KeysConverter();
                HotKeyBox.Text = converter.ConvertToString(Properties.Settings.Default.HotKeyKeyData);
            }
            listView1.Focus();

        }


        private void HotKeyBox_KeyUp(object sender, KeyEventArgs e)
        {
            var converter = new KeysConverter();

            if (e.KeyData == hotkeyset)
            {
                Properties.Settings.Default.HotKeyKeyData = e.KeyData;
                Properties.Settings.Default.Save();
                listView1.Focus();
            }
            else
            {
                HotKeyBox.Text = converter.ConvertToString(Properties.Settings.Default.HotKeyKeyData);
                listView1.Focus();
            }
        }

        private void autoUpdateBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoUpdate = autoUpdateBox.Checked;
            Properties.Settings.Default.Save();
            if (autoUpdateBox.Checked)
            {
                autoUpdateBox.ForeColor = Color.PaleTurquoise;
            }
            else
            {
                autoUpdateBox.ForeColor = Color.SteelBlue;
            }

        }

        private async void updateNow_Click(object sender, EventArgs e)
        {
            Updater.Update();
            if (Updater.UpdateNotAvailable)
            {
                updateNow.ForeColor = Color.PaleTurquoise;
                updateNow.FlatAppearance.BorderColor = Color.PaleTurquoise;
                updateNow.Text = "No update available";
                Thread t1 = new Thread(() =>
                {
                    Thread.Sleep(3000);
                });
                t1.IsBackground = true;
                t1.Start();
                while (t1.IsAlive)
                {
                    await Task.Delay(100);
                }
                if (!t1.IsAlive)
                {
                    updateNow.Text = "Check for update now";
                    updateNow.ForeColor = Color.SteelBlue;
                    updateNow.FlatAppearance.BorderColor = Color.SteelBlue;
                }
            }
        }

        private void extractNested_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.extractNested = extractNested.Checked;
            Properties.Settings.Default.Save();
            if (extractNested.Checked)
            {
                extractNested.ForeColor = Color.PaleTurquoise;
            }
            else
            {
                extractNested.ForeColor = Color.SteelBlue;
            }
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Right)
            {
                var info = listView1.HitTest(e.X, e.Y);
                if (info.Item == null)
                {
                    return;
                }
                var row = info.Item.Index;
                if (listView1.Items[row] != null)
                {
                    listView1.BeginUpdate();
                    if (listView1.Items[row].SubItems[1].Text.Equals(CurrentDLFileName))
                    {
                        webClient.CancelAsync();
                        CurrentDLFileName = "";
                    }
                    if (listView1.Items[row] != null)
                    {
                        listView1.Items.RemoveAt(row);
                    }
                    listView1.EndUpdate();
                }
            }
        }

        private void removeURLs_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ExcludeURLS = removeURLs.Checked;
            Properties.Settings.Default.Save();
            if (removeURLs.Checked)
            {
                removeURLs.ForeColor = Color.PaleTurquoise;
            }
            else
            {
                removeURLs.ForeColor = Color.SteelBlue;
            }
        }

        private void selectTorrent_Click(object sender, EventArgs e)
        {
            string Error = "";
            string Output = "";
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Select A Torrent File";
            openDialog.Filter = "Torrent Files (*.torrent)|*.torrent";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                string file = openDialog.FileName;

                System.Diagnostics.Process x2 = new System.Diagnostics.Process();
                ProcessStartInfo pro = new ProcessStartInfo();
                pro.WindowStyle = ProcessWindowStyle.Hidden;
                pro.UseShellExecute = false;
                pro.CreateNoWindow = true;
                pro.RedirectStandardError = true;
                pro.RedirectStandardOutput = true;
                pro.FileName = $"{Environment.CurrentDirectory}\\_bin\\transmission-show.exe";
                pro.Arguments = $"-m \"{file}\"";
                x2.StartInfo = pro;
                x2.Start();
                Error = x2.StandardError.ReadToEnd();
                Output = x2.StandardOutput.ReadToEnd();
                if (!x2.HasExited)
                    x2.WaitForExit();
                if (String.IsNullOrEmpty(Error))
                {
                    Clipboard.SetText(Output);
                    DoAsyncConversion();
                }

            }
            else
            {
                return;
            }
        }



        private async void ApiManConnect_Click(object sender, EventArgs e)
        {
            if (ApiNameMan.Text.Length < 1 && ApiKeyMan.Text.Length < 1)
            {
                MessageBox.Show("You must enter APP name AND api key in the boxes!");
                return;
            }
            ApiManPanel.Visible = false;
            Properties.Settings.Default.ApiNAME = ApiNameMan.Text;
            Properties.Settings.Default.ApiKEY = ApiKeyMan.Text;
            Properties.Settings.Default.Save();
            await ConnectViaAPIAsync(true);
            while (!loginsuccess)
            {
                this.Invoke(() =>
                {
                    this.Text = $"All-Legit {Updater.LocalVersion}: Not Connected";
                });


            }
            if (loginsuccess)
            {
                this.Invoke(() =>
                {
                    isloggingin = false;
                    this.Text = $"All-Legit {Updater.LocalVersion}: Connected";
                });
            }

        }

        private void ApiNameMan_TextChanged(object sender, EventArgs e)
        {
            if (ApiKeyMan.Text.Trim().Length > 0 && ApiKeyMan.Text.Trim().Length > 0)
            {
                ApiManConnect.Enabled = true;
            }
        }

        private void ApiKeyMan_TextChanged(object sender, EventArgs e)
        {
            if (ApiKeyMan.Text.Trim().Length > 0 && ApiKeyMan.Text.Trim().Length > 0)
            {
                ApiManConnect.Enabled = true;
            }
        }

        private void ShowManualPanel_Click(object sender, EventArgs e)
        {
            ApiManPanel.Visible = true;
            ApiManPanel.BringToFront();
            ApiNameMan.Focus();
        }


        private void CancelAPIMan_Click(object sender, EventArgs e)
        {
            ApiManPanel.Visible = false;
            settingsP.Visible = false;
        }






        private void linksToTextButton_Click(object sender, EventArgs e)
        {

            if (listView1.CheckedItems.Count > 0)
            {
                string forclip = "";
                listView1.BeginUpdate();
                foreach (ListViewItem item in listView1.Items)
                {
                    if (item.Checked)
                    {
                        forclip += item.SubItems[1].Text + "\n";

                        this.Invoke(() =>
                        {
                            if (item.SubItems[2].Text.Equals(currGroup))
                            {
                                if (Properties.Settings.Default.RemDL)
                                {
                                    listView1.Items.Remove(item);
                                }
                                else
                                {
                                    item.Checked = false;
                                }
                            }
                        });
                    }
                }
                listView1.EndUpdate();
                forclip = forclip.Trim('\r', '\n');
                try
                {
                    SaveFileDialog sf = new SaveFileDialog();
                    string _date = DateTime.Now.ToString("dd.MM.yy");
                    // Feed the dummy name to the save dialog
                    sf.FileName = $"Debrid_links_{_date}.txt";

                    if (sf.ShowDialog() == DialogResult.OK)
                    {
                        // Now here's our save folder
                        string savePath = Path.GetDirectoryName(sf.FileName);
                        File.WriteAllText(savePath, forclip);
                        MessageBox.Show($"File saved successfully at:\n{savePath}");
                    }
                }
                catch(Exception er)
                {
                    MessageBox.Show(new Form { TopMost = true }, $"Something went wrong!\n\nError message:\n{er.Message}", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(new Form { TopMost = true }, "Please select items first.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void bulkAddBtn_Click(object sender, EventArgs e)
        {
            Form form = new bulkAdd();
            form.ShowDialog();
        }





        private void bulkAddBox_TextChanged(object sender, EventArgs e)
        { 
        }


    }
}

public static class ControlExtensions
{
    public static void Invoke(this Control control, Action action)
    {
        if (control.InvokeRequired)
        {
            control.Invoke(new MethodInvoker(action), null);
        }
        else
        {
            action.Invoke();
        }
    }
}
