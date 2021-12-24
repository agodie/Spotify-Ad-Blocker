using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace EZBlocker
{
    public partial class App : Application
    {
        private static string APP_GUID =
            ((GuidAttribute)System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
        private static readonly System.Threading.Mutex APP_MUTEX = new System.Threading.Mutex(true, APP_GUID);

        public App()
        {
            if (!APP_MUTEX.WaitOne(TimeSpan.Zero, false))
            {
                // another instance is already running
                Application.Current.Shutdown();
            }
        }
    }
}