﻿using Gma.System.MouseKeyHook;
using Newtonsoft.Json;
using RestSharp;
using SharpCompress.Archives.Rar;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;


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
                    if (waitingforkey)
                    {
                        if (e.KeyData.ToString().Equals("LControlKey"))
                        {
                            HotKeyBox.Text = e.KeyData.ToString().Replace("LControlKey", "Control +");
                        }
                        if (e.KeyData.Equals(System.Windows.Forms.Keys.Control) || e.KeyData.Equals(System.Windows.Forms.Keys.ControlKey) || e.KeyData.Equals(System.Windows.Forms.Keys.LControlKey))
                        {
                            HotKeyBox.Text = "Control +";
                        }
                        HotKeyBox.Text = e.KeyData.ToString().Replace(",", " +");
                        if (HotKeyBox.Text.Contains("Control") && !HotKeyBox.Text.Contains("+") && !HotKeyBox.Text.Contains("LControlKey") || HotKeyBox.Text.Contains("LControlKey"))
                        {
                            HotKeyBox.Text = Properties.Settings.Default.HotKeyKeyData.ToString().Replace(",", " +");
                        }
                        if (e.KeyData.ToString().Contains("LControlKey, Control") || e.KeyData.ToString().Contains("Control") && !e.KeyData.ToString().Contains(" ") || e.KeyData.ToString().Contains("LControlKey") && !e.KeyData.ToString().Contains(" "))
                        {
                            return;
                        }
                        else
                        {
                            hotkeyset = e.KeyData;
                        }
                        if (hotkeyset.Equals(System.Windows.Forms.Keys.Control) || hotkeyset.Equals(System.Windows.Forms.Keys.ControlKey) || hotkeyset.Equals(System.Windows.Forms.Keys.LControlKey) || hotkeyset.ToString().Equals("LControlKey, Control"))
                        {
                            waitingforkey = false;
                            return;
                        }
                        else
                        {
                            Properties.Settings.Default.HotKeyKeyData = hotkeyset;
                            Properties.Settings.Default.Save();
                            HotKeyBox.Text = Properties.Settings.Default.HotKeyKeyData.ToString().Replace(",", " +");
                            waitingforkey = false;
                        }
                    }
                    else
                    {
                        if (e.KeyData == Properties.Settings.Default.HotKeyKeyData)
                        {
                            DoAsyncConversion();



                        }
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
        public static bool endreached = false;
        private async void MainWindow_Load(object sender, EventArgs e)
        {
          OpenDirBox.Checked = Properties.Settings.Default.OpenDir;
            disableNotiesBox.Checked = Properties.Settings.Default.DisableNotifies;
            Close2Tray.Checked = Properties.Settings.Default.Close2Tray;
            RemDL.Checked = Properties.Settings.Default.RemDL;
            AutoOverwrite.Checked = Properties.Settings.Default.AutoOverwrite;
            AutoDLBox.Checked = Properties.Settings.Default.AutoDL;
            autoDelZips.Checked = Properties.Settings.Default.DelZips;
            HotKeyBox.Text = Properties.Settings.Default.HotKeyKeyData.ToString().Replace(",", " +");
            AutoExtract.Checked = Properties.Settings.Default.AutoExtract;
            if (!String.IsNullOrEmpty(Properties.Settings.Default.ZipPWS))
            {
                PWBox.Text = Properties.Settings.Default.ZipPWS;
                PWLIST = PWBox.Text;
            }
            StayOnTopCheckbox.Checked = Properties.Settings.Default.TopMost;
            this.Invoke(() =>
            {
                DownloadingText.Text = $"";
            });
            RemDL.Checked = Properties.Settings.Default.RemDL;
            if (Properties.Settings.Default.DownloadDir == null || Properties.Settings.Default.DownloadDir.Length < 3)
            {
                Properties.Settings.Default.DownloadDir = GetDownloadsPath() + "\\All Legit Downloads";
                Properties.Settings.Default.Save();
            }
            if (!Directory.Exists(Properties.Settings.Default.DownloadDir))
            {
                Directory.CreateDirectory(Properties.Settings.Default.DownloadDir);
            }
            DownloadDir.Text = Properties.Settings.Default.DownloadDir;
            await ConnectViaAPIAsync();
            while (!loginsuccess)
            {
                this.Invoke(() =>
                {
                    this.Text = "All-Legit: Not Connected";
                });


            }
            if (loginsuccess)
            {
                this.Invoke(() =>
                {
                    this.Text = "All-Legit: Connected";
                });
            }
        }

        public static dynamic getJson(string requestURL)
        {
            try
            {
                var client = new RestClient($"https://api.alldebrid.com/v4/");
                var request = new RestRequest(requestURL, Method.Get);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                var queryResult = client.Execute(request);
                var obj = JsonConvert.DeserializeObject<dynamic>(queryResult.Content);
                return obj;
            }
            catch
            {
                if (!cancel)
                {
                    MessageBox.Show(new Form { TopMost = true }, "Not able to access the internet, so cannot login... Exiting program.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                    return null;
                }
                return null;
            }
        }
        public static bool loginsuccess = false;

        public async Task ConnectViaAPIAsync()
        {
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

                        this.Invoke(() =>
                        {
                            Form WebFormForm = new WebFormForm();
                            WebFormForm.ShowDialog();
                        });
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
        public static int CurrentCount = 1;
        public static int cancelGroupRuns = 0;
        public WebClient webClient = new WebClient();
        string currentGroup = "";
        private async Task downloadFiles(string URL, string FILENAME, string MagnetNAME)
        {
            if (webClient.IsBusy)
            {
                webClient.Dispose(); ;
            }

            currGroup = MagnetNAME;
            CancelButton.Visible = true;
            if (!String.IsNullOrEmpty(cancelledGroup))
            {

                cancelGroupRuns++;
                if (MagnetNAME.Equals(cancelledGroup))
                {
                    webClient.CancelAsync();
                    webClient.Dispose();
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

            string DIR = Properties.Settings.Default.DownloadDir + "\\" + Utilities.RemoveEverythingAfterLast(MagnetNAME, ".");
            if (!Directory.Exists(DIR))
            {
                Directory.CreateDirectory(DIR);
            }
            string DL = Properties.Settings.Default.DownloadDir + "\\" + Utilities.RemoveEverythingAfterLast(MagnetNAME, ".") + "\\" + FILENAME;

            if (File.Exists(DL))
            {
                if (Properties.Settings.Default.AutoOverwrite)
                {
                    overwrite = true;
                }
                if (!currentGroup.Contains(MagnetNAME) && !Properties.Settings.Default.AutoOverwrite)
                {
                    currentGroup = MagnetNAME;
                    DialogResult Overwrite1 = MessageBox.Show(new Form { TopMost = true }, $"Files found, do you want to overwrite all files from this batch of links({Utilities.RemoveEverythingAfterLast(MagnetNAME, ".")})", "Overwrite?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (Overwrite1 == DialogResult.Yes)
                    {
                        overwrite = true;
                    }
                    else
                    {
                        cancelledGroup = "";
                        fileDownloading = "";
                        isDownloading = false;
                        cancel = false;
                        overwrite = false;
                    }

                }
                if (overwrite)
                {
                    try
                    {

                        if (File.Exists(DL))
                        {
                            File.Delete(DL);
                        }

                    }
                    catch { }
                }
                else if (!overwrite)
                {
                    try
                    {
                        if (Properties.Settings.Default.RemDL)
                        {
                            listView1.BeginUpdate();
                            foreach (ListViewItem itemmm in listView1.Items)
                            {
                                if (itemmm.SubItems[2].Text.Contains(currGroup) && itemmm.Checked)
                                {
                                    listView1.Items.Remove(itemmm);
                                }
                            }
                            listView1.EndUpdate();
                            listView1.Refresh();
                        }
                        webClient.Dispose();
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


            webClient.DownloadProgressChanged += (s, e) =>
            {
                sw.Start();
                string DLS;
                DLS = String.Format("{0:0.00}", (e.BytesReceived / 1024 / 1024 / sw.Elapsed.TotalSeconds).ToString("0.00"));
                var name = FILENAME;
                const int MaxLength = 35;
                if (name.Length > MaxLength)
                {
                    FILENAME = name.Substring(0, MaxLength) + "..."; //
                }

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
                torrentDLING = false;
                cancel = false;
                webClient.Dispose();
                if (e.Cancelled)
                {
                    if (File.Exists(DL))
                    {
                        File.Delete(DL);
                    }
                    if (webClient.IsBusy)



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
                    return;
                }
                this.Invoke(() =>
                {
                    DownloadingText.Text = $"Download finished...";
                });
                sw.Stop();
                if (Properties.Settings.Default.RemDL)
                {
                    this.Invoke(() =>
                    {
                        listView1.BeginUpdate();

                        foreach (ListViewItem item in listView1.Items)
                        {
                            if (item.SubItems[1].Text.Equals(URL))
                            {
                                listView1.Items.Remove(item);

                            }
                        }
                        listView1.EndUpdate();
                        listView1.Refresh();
                    });


                }



                this.Invoke(() =>
                {
                    dlProg.Value = 0;
                });


            };
            await webClient.DownloadFileTaskAsync(new Uri(URL), $"{DL}");
            if (webClient.IsBusy)
                webClient.Dispose();
            isDownloading = false;
            fileDownloading = "";
            torrentDLING = false;
            cancel = false;
            listView1.Refresh();

        }

        public static bool notdone = true;
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
        public static string currentLink = "";
        public static bool showingconversion = false;
        public async void DoAsyncConversion()
        {


            SplashText.Visible = false;
            string pasted = Clipboard.GetText();

            bool convertingMag = false;

            fileDownloading += $"{pasted};";
            if (pasted.ToLower().StartsWith("magnet".ToLower()))
            {
                convertingMag = true;
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
                    int a = 0;
                    int b = 0;
                    int c = 0;
                    string a1 = "";
                    string b1 = "";
                    string c1 = "";

                    foreach (string s in output)
                    {
                        convertingMag = true;
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
                            string magnetName = obj.data.magnets[0].name.ToString();
                            string magnetID = obj.data.magnets[0].id.ToString();
                            string MagnetPoll = $"magnet/status?agent={apiNAME}&apikey={APIKEY}&id={magnetID}";

                            //WHILE LOOP START


                            obj = getJson(MagnetPoll);

                            var key = obj.data.magnets;
                            magnetName = key.filename.ToString();

                            bool ready = false;
                            notdone = true;
                            bool secondalert = false;
                            while (notdone)
                            {
                                obj = getJson(MagnetPoll);
                                key = obj.data.magnets;
                                if (key.id.ToString().Equals(magnetID))
                                {
                                    string MagnetStatus = key.status.ToString();
                                    if (MagnetStatus.Equals("Ready"))
                                    {
                                        notdone = false;
                                        ready = true;
                                    }
                                    else
                                    {
                                        if (cancel)
                                        {
                                            if (torrentDLING)
                                            {
                                                getJson($"magnet/delete?agent={apiNAME}&apikey={APIKEY}&id={magnetID}");
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
                                                cancel = false;
                                                fileDownloading = "";
                                                CurrentlyShowingID = 0;
                                                convertingMag = false;

                                                return;
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

                                                string seeders = key.seeders.ToString();
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


                                                this.Invoke(() =>
                                                {
                                                    foreach (ListViewItem item in listView1.Items)
                                                    {
                                                        if (item.SubItems[1].Text.EndsWith(key2.filename.ToString()) && item.SubItems[2].Text.Equals(magnetName))
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
                                                        string[] row = { key2.filename.ToString(), unlockedLink, magnetName, FileSizeInt };
                                                        ListViewItem item = new ListViewItem(row);
                                                        listView1.Items.Add(item);
                                                        item.Checked = true;
                                                    }

                                                    if (listView1.Items.Count == 0)
                                                    {
                                                        this.Invoke(() =>
                                                        {
                                                            listView1.Items.Add(new ListViewItem(new string[] { key2.filename.ToString(), unlockedLink, magnetName, FileSizeInt }));
                                                        });
                                                    }

                                                    skip = false;

                                                });
                                            }
                                            catch (Exception ex)
                                            {
                                                MessageBox.Show(ex.ToString());
                                            }
                                        }
                                        this.Invoke(() =>
                                        {
                                            dlProg.Value = 0;
                                        });
                                        convertingMag = false;
                                        torrentDLING = false;
                                        notdone = false;
                                        this.Invoke(() =>
                                        {
                                            Program.form.DownloadingText.Text = "";
                                        });

                                    }
                                }
                            }
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
                });
                t1.Start();
                t1.IsBackground = true;
                while (t1.IsAlive || convertingMag)
                {
                    if (cancel)
                    {
                        t1.Abort();
                    }
                    await Task.Delay(100);
                }
         
                
            }
            cancel = false;



            if (pasted.ToLower().StartsWith("https://filecrypt.".ToLower()) || pasted.ToLower().StartsWith("https://www.filecrypt.".ToLower()))
            {
                this.Invoke(() =>
                {
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

                    FilecryptURL = s;

                    Form WebFormForm = new WebFormForm();
                    WebFormForm.ShowDialog();
                    filecryptinprog = true;
                    Utilities.DecryptDLC();

                }
            }
            else if (pasted.ToLower().StartsWith("https://".ToLower()))
            {

                try
                {
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
                                cancel = false;
                                return;
                            }
                            var obj = getJson($"link/unlock?agent={apiNAME}&apikey={APIKEY}&link={s}");
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
                                    linkpass++;
                                    linkpasss += s + "\n";
                                }
                                if (obj.error.code.ToString().Equals("LINK_HOST_NOT_SUPPORTED"))
                                {
                                    linknotsup++;
                                    linknotsups += s + "\n";
                                }
                            }
                            else
                            {
                                int addedlinks = 0;
                                string unlockedLink = obj.data.link.ToString();
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
                                    foreach (ListViewItem item in listView1.Items)
                                    {
                                        if (item.SubItems[0].Text.Equals(obj.data.filename.ToString()) && item.SubItems[2].Text.Equals(FileNameNoExt))
                                        {
                                            skip = true;
                                        }
                                    }
                                    if (!skip)
                                    {
                                        addedlinks++;
                                        listView1.Items.Add(new ListViewItem(new string[] { obj.data.filename.ToString(), unlockedLink, FileNameNoExt, FileSizeInt }));

                                    }
                                    if (listView1.Items.Count == 0)
                                    {
                                        addedlinks++;
                                        listView1.Items.Add(new ListViewItem(new string[] { obj.data.filename.ToString(), unlockedLink, FileNameNoExt, FileSizeInt }));

                                    }
                                    foreach (ListViewItem item in listView1.Items)
                                    {
                                        if (item.SubItems[0].Text.Equals(obj.data.filename.ToString()) && item.SubItems[1].Text.Equals(unlockedLink))
                                        {
                                            item.Checked = true;
                                        }
                                    }
                                });
                            }

                        }
                        if (isErrors)
                        {
                            this.Invoke(() =>
                            {
                                if (linkdown > 0)
                                {
                                    if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                                    {
                                        ALTrayIcon.ShowBalloonTip(2000, "", $"Link(s) have been taken down from the filehoster's website.\n\nNot adding!", ToolTipIcon.None);
                                    }
                                    else
                                        DownloadingText.Text = "Link(s) have been taken down from the filehoster's website.\n\nNot adding!";
                                }
                                if (linkmissing > 0)
                                {
                                    if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                                    {
                                        ALTrayIcon.ShowBalloonTip(2000, "", $"Links missing.\n\nNot adding!", ToolTipIcon.None);

                                    }
                                    else
                                        DownloadingText.Text = "Links missing.\n\nNot adding!";
                                }
                                if (linkpass > 0)
                                {
                                    if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                                    {
                                        ALTrayIcon.ShowBalloonTip(2000, "", $"Link(s) are password protected on the filehost's website.\n\nNot adding!", ToolTipIcon.None);
                                    }
                                    else
                                        DownloadingText.Text = "Link(s) are password protected on the file hoster site.\n\nNot adding!";
                                }
                                if (linknotsup > 0)
                                {
                                    if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                                    {
                                        ALTrayIcon.ShowBalloonTip(2000, "", $"Link(s) are from an unsupported host.\n\nPlease check this link for a list of supported filehosters:\nhttps://alldebrid.com/hosts/", ToolTipIcon.None);
                                    }
                                    else
                                        DownloadingText.Text = "Link(s) are not supported by All Debrid!";
                                }
                            });
                        }
                    });
                    t1.Start();
                    t1.IsBackground = true;
                    while (t1.IsAlive)
                    {
                        if (cancel)
                        {
                            t1.Abort();
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

            this.Invoke(() =>
            {
                listView1.Update();
            });



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
                        File.Delete(file);
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
            if (listView1.CheckedItems.Count > 0 && !isDownloading)
            {
                if (Properties.Settings.Default.AutoDL)
                {
                    object sender = null;
                    EventArgs e = new EventArgs();
                    startDownloads_Click(sender, e);
                }
            }
            this.Invoke(() =>
            {
                if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                {
                    DownloadingText.Text = "";
                }

            });
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
        public static bool isDownloading = false;
        public async void startDownloads_Click(object sender, EventArgs e)
        {

            endreached = false;

            if (listView1.CheckedItems.Count == 0)
            {
                endreached = true;
                return;
            }

            string DLList = "";
            CancelButton.Visible = true;
            if (listView1.CheckedItems.Count > 0)
            {
                if (Properties.Settings.Default.AutoOverwrite)
                {
                    overwrite = true;

                }
                foreach (ListViewItem item in listView1.CheckedItems)
                {
                    if (!String.IsNullOrEmpty(currentGroup))
                    {
                        currentGroup = item.SubItems[2].Text;
                    }

                    this.Invoke(() =>
                    {
                        this.Text = $"Downloading {item.SubItems[0].Text}...";
                        DownloadingText.Text = $"Downloading {item.SubItems[0].Text}...";
                    });
                    try
                    {
                        isDownloading = true;
                        item.BackColor = Color.MediumSpringGreen;
                        //
                        //HERES WHERE IT ADDS THEM TO THE LIST
                        //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
                        await downloadFiles(item.SubItems[1].Text, item.SubItems[0].Text, item.SubItems[2].Text);
                        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                        //HERES WHERE IT ADDS THEM TO THE LIST
                        //

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


                    if (AutoExtract.Checked)
                    {
                        dlsPara += $"{Properties.Settings.Default.DownloadDir}\\{Utilities.RemoveEverythingAfterLast(item.SubItems[2].Text, ".")}\\{item.SubItems[0].Text}";
                        DLList += $"{item.SubItems[0].Text};{Utilities.RemoveEverythingAfterLast(item.SubItems[2].Text, ".")}\n";
                    }
                }
                if (!muteoutputcancelled)
                {
                    if (AutoExtract.Checked)
                    {
                        this.Invoke(() =>
                        {
                            if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                            {
                                ALTrayIcon.ShowBalloonTip(2000, "", $"Downloads finished! Checking for and extracting archives...", ToolTipIcon.None);
                            }
                            else
                                DownloadingText.Text = $"Downloads finished! Checking for and extracting archives...";
                        });
                    }
                    else
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
                    }
                    isDownloading = false;

                    if (AutoExtract.Checked)
                    {
                        this.Invoke(() =>
                        {
                            string[] SplitDLList = DLList.Split('\n');
                            foreach (string FullDL in SplitDLList)
                            {

                                if (!String.IsNullOrWhiteSpace(FullDL))
                                {

                                    string[] DLS = FullDL.Split(';');
                                    bool Extract = false;
                                    if (DLS[0].ToString().ToLower().EndsWith(".rar"))
                                    {
                                        try
                                        {

                                            using (var archive = RarArchive.Open(Properties.Settings.Default.DownloadDir + "\\" + DLS[1].ToString() + "\\" + DLS[0].ToString()))
                                            {
                                                if (archive.IsMultipartVolume())
                                                {
                                                    if (!archive.IsFirstVolume())
                                                    {
                                                        Extract = false;
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        Extract = true;
                                                    }
                                                }
                                                else
                                                {
                                                    Extract = true;

                                                }

                                            }
                                        }
                                        catch
                                        {

                                            Extract = true;

                                        }
                                    }



                                    else if (DLS[0].ToString().Contains(".001") && DLS[0].ToString().Contains("7z") || DLS[0].ToString().Contains(".01") && DLS[0].ToString().Contains("7z")
                                        || DLS[0].ToString().EndsWith(".7z") || DLS[0].ToString().EndsWith(".zip") || DLS[0].ToString().EndsWith(".rar"))
                                    {
                                        Extract = true;
                                    }
                                    if (Extract && !muteoutputcancelled)
                                    {
                                        this.Invoke(() =>
                                        {
                                            DownloadingText.Text = $"Extracting archives...";
                                        });

                                        string DLDir = Properties.Settings.Default.DownloadDir + "\\" + DLS[1].ToString();
                                        string DL = DLDir + "\\" + DLS[0].ToString();
                                        if (!Directory.Exists(DLDir))
                                        {
                                            Directory.CreateDirectory(DLDir);
                                        }

                                        string ArchiveDIR = DLDir;
                                        if (!Directory.Exists(ArchiveDIR))
                                        {
                                            Directory.CreateDirectory(ArchiveDIR);
                                        }
                                        Utilities.ExtractFile(DL, ArchiveDIR);
                                        Extract = false;

                                    }


                                }
                            }
                        });


                    }
                    this.Invoke(() =>
                    {
                        this.Text = "";
                        dlProg.Value = 0;
                        DownloadingText.Text = "";
                    });


                    CancelButton.Visible = false;
                }

            }
            fileDownloading = "";
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            muteoutputcancelled = false;
            isDownloading = false;
            fileDownloading = "";
            currentGroup = "";
            cancelledGroup = "";
            endreached = true;
            cancel = false;
            startDownloads.Enabled = true;
            if (listView1.CheckedItems.Count == 0)
            {
                this.Invoke(() =>
                {
                    if (!Program.form.Focused && TrayNotify && !Properties.Settings.Default.DisableNotifies)
                    {
                        ALTrayIcon.ShowBalloonTip(2000, "", $"All jobs complete!", ToolTipIcon.None);
                    }
                    else
                        DownloadingText.Text = $"All jobs complete!";
                });
                if (Properties.Settings.Default.OpenDir)
                {
                    Process.Start(Properties.Settings.Default.DownloadDir);
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
                        if (Properties.Settings.Default.RemDL)
                        {
                            this.Invoke(() =>
                            {
                                if (item.SubItems[2].Text.Equals(currGroup))
                                {
                                    listView1.Items.Remove(item);

                                }
                            });
                        }
                    }
                }
                listView1.EndUpdate();
                listView1.Refresh();
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
            FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
            folderSelectDialog.Title = "Select All Legit download directory...";
            if (Properties.Settings.Default.DownloadDir != null)
                folderSelectDialog.InitialDirectory = Properties.Settings.Default.DownloadDir;
            if (folderSelectDialog.Show(Handle))
            {
                Properties.Settings.Default.DownloadDir = folderSelectDialog.FileName;
                Properties.Settings.Default.Save();
            }
            DownloadDir.Text = Properties.Settings.Default.DownloadDir;
        }
        public static bool torrentDLING = false;

        private void RemDL_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.RemDL = RemDL.Checked;
            Properties.Settings.Default.Save();
            if (RemDL.Checked)
            {
                RemDL.ForeColor = Color.FromArgb(192, 255, 192);
            }
            else
            {
                RemDL.ForeColor = Color.FromArgb(0, 100, 80);
            }
        }
        public static bool muteoutputcancelled = Properties.Settings.Default.DisableNotifies;
        public static bool cancel = false;
        public static bool iscancelling = false;
        public static string cancelledGroup = "";
        public async void CancelButton_Click(object sender, EventArgs e)
        {
            startDownloads.Enabled = false;
            CancelButton.Visible = false;
            cancel = true;
            if (isDownloading)
            {
                isDownloading = false;
                cancelledGroup = currGroup;
                muteoutputcancelled = true;
                webClient.CancelAsync();
                if (cancel)
                {
                    if (Properties.Settings.Default.RemDL)
                    {
                        listView1.BeginUpdate();
                        foreach (ListViewItem itemstogo in listView1.CheckedItems)
                        {
                            if (itemstogo.SubItems[2].Text.Equals(currGroup))
                            {
                                listView1.Items.Remove(itemstogo);

                            }

                        }
                        listView1.EndUpdate();
                        listView1.Update();
                        listView1.Refresh();
                    }

                }
            }
            this.Invoke(() =>
            {
                CancelButton.Visible = false;
                Program.form.DownloadingText.Text = $"";
                dlProg.Value = 0;
            });
            startDownloads.Enabled = true;

            await Task.Delay(5000);
            muteoutputcancelled = false;
            cancel = false;
        }

        private void PasteButton_Click(object sender, EventArgs e)
        {
            string cb = Clipboard.GetText();
            if (cb.ToLower().StartsWith("https://") || cb.ToLower().StartsWith("magnet:?"))
            {
                DoAsyncConversion();
            }
        }



        public void listView1_MouseDoubleClick(object sender, EventArgs e)
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

        private void PWBox_Enter(object sender, EventArgs e)
        {

            if (PWBox.Text == "your;common;zip;passwords")
            {
                PWBox.Text = "";
            }
            else
            {
                PWBox.Text = Properties.Settings.Default.ZipPWS;
            }
        }

        private void PWBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape)
            {
                settingsP.Focus();
            }
            if (e.KeyChar == (char)System.Windows.Forms.Keys.Enter && PWBox.Text.Length > 0 && !PWBox.Text.Equals("your;commonly;used;zip;passwords"))
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

            if (PWBox.Text == "")
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
            else if (!PWBox.Text.Equals("your;commonly;used;zip;passwords") && PWBox.Text.Length > 0)
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
                    Properties.Settings.Default.DownloadDir = DownloadDir.Text;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    DownloadDir.Text = Properties.Settings.Default.DownloadDir;
                }
            }
        }

        private void HotKeyBtn_Click(object sender, EventArgs e)
        {
            waitingforkey = true;
            HotKeyBox.Text = "Press desired hotkey...";
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



        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (Properties.Settings.Default.Close2Tray)
            {
                if (TrayExit)
                {
                    ALTrayIcon.Visible = false;
                    ALTrayIcon.Dispose();
                    Program.mutex.Close();
                    Application.ExitThread();
                    this.Close();
                }
                else if (e.CloseReason == CloseReason.UserClosing)
                {
                    ALTrayIcon.BalloonTipText = "All Legit! is still running in your system tray.";
                    ALTrayIcon.BalloonTipIcon = ToolTipIcon.None;
                    if (!Properties.Settings.Default.DisableNotifies)
                    {
                        ALTrayIcon.ShowBalloonTip(2000);

                    }
                    this.Hide();
                    e.Cancel = true;
                }
                else
                {
                    ALTrayIcon.Visible = false;
                    ALTrayIcon.Dispose();
                    Program.mutex.Close();
                    Application.ExitThread();
                    this.Close();
                }
            }
            else
            {
                ALTrayIcon.Dispose();
                Program.mutex.Close();
                Application.ExitThread();
                this.Close();
            }

        }


        private void ALTrayIcon_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {

            this.Show();
            this.WindowState = FormWindowState.Normal;


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
        public async void settingsP_MouseLeave(object sender, EventArgs e)
        {
            TrayNotify = true;
            if (PWBox.Focused) return;
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
                Close2Tray.ForeColor = Color.FromArgb(192, 255, 192);
            }
            else
            {
                Close2Tray.ForeColor = Color.FromArgb(0, 100, 80);
            }
            Properties.Settings.Default.Close2Tray = Close2Tray.Checked;
            Properties.Settings.Default.Save();
        }

        private void AutoExtract_CheckedChanged_1(object sender, EventArgs e)
        {
            if (AutoExtract.Checked)
            {
                AutoExtract.ForeColor = Color.FromArgb(192, 255, 192);
                PWBox.Enabled = true;
            }
            else
            {
                AutoExtract.ForeColor = Color.FromArgb(0, 100, 80);
                PWBox.Enabled = false;

            }
            Properties.Settings.Default.AutoExtract = AutoExtract.Checked;
            Properties.Settings.Default.Save();
        }

        private void autoDelZips_CheckedChanged_1(object sender, EventArgs e)
        {
            if (autoDelZips.Checked)
            {
                autoDelZips.ForeColor = Color.FromArgb(192, 255, 192);
            }
            else
            {
                autoDelZips.ForeColor = Color.FromArgb(0, 100, 80);
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
                AutoDLBox.ForeColor = Color.FromArgb(192, 255, 192);
            }
            else
            {
                AutoDLBox.ForeColor = Color.FromArgb(0, 100, 80);
            }
        }

        private void AutoDelete_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoOverwrite = AutoOverwrite.Checked;
            Properties.Settings.Default.Save();
            if (AutoOverwrite.Checked)
            {
                AutoOverwrite.ForeColor = Color.FromArgb(192, 255, 192);
            }
            else
            {
                AutoOverwrite.ForeColor = Color.FromArgb(0, 100, 80);
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

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (listView1.CheckedItems.Count == 0)
            {
                ClearButton.Enabled = false;
            }
            else
            {
                ClearButton.Enabled = true;
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

                disableNotiesBox.ForeColor = Color.FromArgb(192, 255, 192);
            }
            else
            {

                disableNotiesBox.ForeColor = Color.FromArgb(0, 100, 80);
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
