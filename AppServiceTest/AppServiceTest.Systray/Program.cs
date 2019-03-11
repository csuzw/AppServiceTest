using System;
using System.Threading;
using System.Windows.Forms;

namespace AppServiceTest.Systray
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            const string mutexName = "AxpPhoneIntegrationMutex";

            if (!Mutex.TryOpenExisting(mutexName, out var mutex))
            {
                mutex = new Mutex(false, mutexName);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var context = new SystrayApplicationContext();
                context.Initialise();
                Application.Run(context);
                mutex.Close();
            }
        }
    }
}
