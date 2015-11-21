using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace ExcerciseTimer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {

        System.Windows.Forms.NotifyIcon TrayIconUnmanaged { get; set; }



        /// <summary>
        /// Main application method. Called at some point after App was constructed in the generated static Main() method.
        /// Linked to by App.xaml.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ApplicationEntryPoint(object sender, StartupEventArgs e)
        { 
             
            MainWindow main = new MainWindow();

            TrayIconUnmanaged = new System.Windows.Forms.NotifyIcon();
            TrayIconUnmanaged.Icon = ExcerciseTimer.Properties.Resources.AppIcon;
            TrayIconUnmanaged.Visible = true;
            TrayIconUnmanaged.DoubleClick += (s, args) => { main.Show(); main.WindowState = WindowState.Normal; };



            main.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            TrayIconUnmanaged.Dispose();

            base.OnExit(e);
        }

    }

    class SharedModel
    {

        /// <summary>
        /// Default Overall Paramaters.
        /// This gets cast to int32 in milliseconds. 
        /// DO NOT SET this to 590 hours or more!
        /// </summary>
        readonly TimeSpan OverallPeriodDefault = new TimeSpan(hours: 0, minutes: 0, seconds: 10);
        readonly TimeSpan ExcercisePeriodDefault = new TimeSpan(hours: 0, minutes: 0, seconds: 5);

        //TO DO: insert setter control.
        public TimeSpan OverallPeriod { get; set; }
        public TimeSpan ExcercisePeriod { get; set; }


        Stopwatch_ThreadSafe ElapsedStopwatch;
        //public TimeSpan ActiveSessionElapsed() { return ElapsedStopwatch.Elapsed; }


        public SharedModel(Stopwatch_ThreadSafe elapsedStopwatch)
        {
            this.OverallPeriod = OverallPeriodDefault;
            this.ExcercisePeriod = ExcercisePeriodDefault;

            ElapsedStopwatch = elapsedStopwatch;
        }

    }

    class Stopwatch_ThreadSafe
    {
        private Stopwatch s;

        public Stopwatch_ThreadSafe() { s = new Stopwatch(); }

        public TimeSpan Elapsed { get { TimeSpan local; lock (s) { local = s.Elapsed; } return local; } }

        public void Start() { lock (s) { s.Start(); } }
        public void Stop() { lock (s) { s.Stop(); } }
        public void Reset() { lock (s) { s.Reset(); } }
    }
}
