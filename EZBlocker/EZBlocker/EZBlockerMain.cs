using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
namespace EZBlocker
{
    public partial class Main : Form
    {
        private bool muted = false;
        private string lastMessage = "";
        private ToolTip artistTooltip = new ToolTip();

        private readonly string volumeMixerPath = Environment.GetEnvironmentVariable("WINDIR") + @"\System32\SndVol.exe";

        private DateTime lastRequest;
        private string lastAction = "";
        private MediaHook hook;

        public Main()
        {
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            InitializeComponent();
        }

        /**
         * Contains the logic for when to mute Spotify
         **/
        private void MainTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (hook.IsRunning())
                {
                    Debug.WriteLine("Is running");
                    if (hook.IsAdPlaying)
                    {
                        if (MainTimer.Interval != 1000) MainTimer.Interval = 1000;
                        if (!muted) Mute(true);
                        if (!hook.IsPlaying)
                        {
                            hook.SendNextTrack();
                            Thread.Sleep(500);
                        }

                        string artist = hook.CurrentArtist;
                        string message = Properties.strings.StatusMuting + " " + Truncate(artist);
                        if (lastMessage != message)
                        {
                            lastMessage = message;
                            StatusLabel.Text = message;
                            artistTooltip.SetToolTip(StatusLabel, artist);
                            LogAction("/mute/" + artist);
                        }
                    }
                    else if (hook.IsPlaying) // Normal music
                    {
                        Debug.WriteLine("Playing");
                        if (muted)
                        {
                            Thread.Sleep(200); // Give extra time for ad to change out
                            Mute(false);
                        }
                        if (MainTimer.Interval != 200) MainTimer.Interval = 200;

                        string artist = hook.CurrentArtist;
                        string message = Properties.strings.StatusPlaying + " " + Truncate(artist);
                        if (lastMessage != message)
                        {
                            lastMessage = message;
                            StatusLabel.Text = message;
                            artistTooltip.SetToolTip(StatusLabel, artist);
                            LogAction("/play/" + artist);
                        }
                    }
                    else
                    {
                        string message = Properties.strings.StatusPaused;
                        if (lastMessage != message)
                        {
                            lastMessage = message;
                            StatusLabel.Text = message;
                            artistTooltip.SetToolTip(StatusLabel, "");
                        }
                    }
                }
                else
                {
                    if (MainTimer.Interval != 1000) MainTimer.Interval = 1000;
                    string message = Properties.strings.StatusNotFound;
                    if (lastMessage != message)
                    {
                        lastMessage = message;
                        StatusLabel.Text = message;
                        artistTooltip.SetToolTip(StatusLabel, "");
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /**
         * Mutes/Unmutes Spotify.
         
         * i: false = unmute, true = mute
         **/
        private void Mute(bool mute)
        {
            AudioUtils.SetSpotifyMute(mute);
            muted = mute;
        }

        private string Truncate(string name)
        {
            if (name.Length > 9)
            {
                return name.Substring(0, 9) + "...";
            }
            return name;
        }

        private void LogAction(string action)
        {
            if (lastAction.Equals(action) && DateTime.Now - lastRequest < TimeSpan.FromMinutes(5)) return;
            lastAction = action;
            lastRequest = DateTime.Now;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.UpdateSettings) // If true, then first launch of latest EZBlocker
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpdateSettings = false;
                Properties.Settings.Default.Save();
            }

            string spotifyPath = GetSpotifyPath();
            if (spotifyPath != "")
            {
                Properties.Settings.Default.SpotifyPath = spotifyPath;
                Properties.Settings.Default.Save();
            }
            else
            {
                spotifyPath = Environment.GetEnvironmentVariable("APPDATA") + @"\Spotify\spotify.exe";
            }

            // Start Spotify and give EZBlocker higher priority
            try
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High; // Windows throttles down when minimized to task tray, so make sure EZBlocker runs smoothly
                if (Properties.Settings.Default.StartSpotify && File.Exists(Properties.Settings.Default.SpotifyPath) && Process.GetProcessesByName("spotify").Length < 1)
                {
                    Process.Start(Properties.Settings.Default.SpotifyPath);
                }
            }
            catch (Exception) { }

            // Set up Analytics
            if (String.IsNullOrEmpty(Properties.Settings.Default.CID))
            {
                Properties.Settings.Default.CID = "0";
                Properties.Settings.Default.Save();
            }

            // Start Spotify hook
            hook = new MediaHook();

            MainTimer.Enabled = true;

            LogAction("/launch");

        }

        private string GetSpotifyPath()
        {
            foreach (Process p in Process.GetProcessesByName("spotify"))
            {
                if (p.MainWindowTitle.Length > 1)
                {
                    return p.MainModule.FileName;
                }
            }
            return "";
        }

        private void RestoreFromTray()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void Notify(String message)
        {
            NotifyIcon.ShowBalloonTip(5000, "EZBlocker", message, ToolTipIcon.None);
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!this.ShowInTaskbar && e.Button == MouseButtons.Left)
            {
                RestoreFromTray();
            }
        }

        private void NotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            RestoreFromTray();
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                Notify(Properties.strings.HiddenNotify);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestoreFromTray();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!MainTimer.Enabled) return; // Still setting up UI
            var result = MessageBox.Show(Properties.strings.OnExitMessageBox, "EZBlocker",
                                 MessageBoxButtons.YesNo,
                                 MessageBoxIcon.Warning);

            e.Cancel = (result == DialogResult.No);

            if (result == DialogResult.Yes)
            {
                Properties.Settings.Default.UserEducated = true;
                Properties.Settings.Default.Save();
            }
        }
    }
}
