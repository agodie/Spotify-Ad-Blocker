using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace EZBlocker
{
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private DispatcherTimer mainTimer;

        private bool muted = false;
        private string lastMessage = "";

        private DateTime lastRequest;
        private string lastActionMessage = "";
        private MediaHook hook;

        public MainWindow()
        {
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            InitializeComponent();

            // Setup and start main timer
            mainTimer = new();
            mainTimer.Interval = TimeSpan.FromMilliseconds(600);
            mainTimer.Tick += MainTimerTick;

            // Set up NotifyIcon
            notifyIcon = new();
            notifyIcon.Text = "EZBlocker";
            notifyIcon.Visible = true;

            notifyIcon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("/resources/my_music.ico", UriKind.Relative)).Stream);
            notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIconMouseDoubleClick);

            // Set up contex menu for the taskbar icon
            var menu = new System.Windows.Forms.ContextMenu();
            menu.MenuItems.Add("Open", OnOpenClicked);
            menu.MenuItems.Add("Exit", OnExitClicked);
            notifyIcon.ContextMenu = menu;
        }

        /**
         * Contains the logic for when to mute Spotify
         **/
        private void MainTimerTick(object sender, EventArgs e)
        {
            try
            {
                if (hook.IsRunning())
                {
                    Debug.WriteLine("Is running");
                    if (hook.IsAdPlaying)
                    {
                        if (mainTimer.Interval != TimeSpan.FromMilliseconds(1000)) mainTimer.Interval = TimeSpan.FromMilliseconds(1000);
                        if (!muted) Mute(true);
                        if (!hook.IsPlaying)
                        {
                            hook.SendNextTrack();
                            Thread.Sleep(500);
                        }
                        UpdateLabels(Properties.strings.StatusMuting, hook.CurrentArtist, hook.CurrentTitle);
                        LogAction("/mute/", hook.CurrentArtist, hook.CurrentTitle);
                    }
                    else if (hook.IsPlaying) // Normal music
                    {
                        Debug.WriteLine("Playing");
                        if (muted)
                        {
                            Thread.Sleep(200); // Give extra time for ad to change out
                            Mute(false);
                        }
                        if (mainTimer.Interval != TimeSpan.FromMilliseconds(200)) mainTimer.Interval = TimeSpan.FromMilliseconds(200);
                        UpdateLabels(Properties.strings.StatusPlaying, hook.CurrentArtist, hook.CurrentTitle);
                        LogAction("/play/", hook.CurrentArtist, hook.CurrentTitle);
                    }
                    else
                    {
                        UpdateLabels(Properties.strings.StatusPaused);
                        LogAction("/pause/");
                    }
                }
                else
                {
                    if (mainTimer.Interval != TimeSpan.FromMilliseconds(1000)) mainTimer.Interval = TimeSpan.FromMilliseconds(1000);
                    UpdateLabels(Properties.strings.StatusNotFound);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /**
         * Mutes/Unmutes Spotify.
         *  false = unmute,
         *  true  = mute
         **/
        private void Mute(bool mute)
        {
            AudioUtils.SetSpotifyMute(mute);
            muted = mute;
        }

        private void UpdateLabels(string status, string currentArtist = "", string currentTitle = "")
        {
            var messageItems = new List<string> { status, currentArtist, currentTitle }
                .FindAll(item => item.Length > 0);
            var message = string.Join(" ", messageItems);
            if (lastMessage != message)
            {
                lastMessage = message;
                StatusValueTextBlock.Text = status;
                ArtistValueTextBlock.Text = currentArtist;
                TitleValueTextBlock.Text = currentTitle;
            };
        }

        /**
         * This logging does nothing useful at the moment.
         * Keep it as building block if someone wants to build
         * on top.
         */
        private void LogAction(string action, string currentArtist = "", string currentTitle = "")
        {
            var messageItems = new List<string> { action, currentArtist, currentTitle }
                .FindAll(item => item.Length > 0);
            var actionMessage = string.Join(";", messageItems);
            if (lastActionMessage.Equals(actionMessage) && DateTime.Now - lastRequest < TimeSpan.FromMinutes(5)) return;
            lastActionMessage = action;
            lastRequest = DateTime.Now;
        }

        private void MainLoad(object sender, EventArgs e)
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
            mainTimer.Start();
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
            this.WindowState = WindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void Notify(String message)
        {
            notifyIcon.ShowBalloonTip(5000, "EZBlocker", message, System.Windows.Forms.ToolTipIcon.None);
        }

        private void NotifyIconMouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!this.ShowInTaskbar && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                RestoreFromTray();
            }
        }

        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                ShowInTaskbar = false;
                Notify(Properties.strings.HiddenNotify);
            }
        }

        private void OnOpenClicked(object sender, EventArgs e)
        {
            RestoreFromTray();
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            Close();
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!mainTimer.IsEnabled) return; // Still setting up UI
            var result = MessageBox.Show(Properties.strings.OnExitMessageBox, "EZBlocker",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            e.Cancel = (result == MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.UserEducated = true;
                Properties.Settings.Default.Save();
            }
        }

    }
}