using Microsoft.Win32;
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


        SharedModel SM;

        ViewModel_App VM_App;


        Stopwatch_ThreadSafe ElapsedStopwatch = new Stopwatch_ThreadSafe();

        System.Timers.Timer EverySecondTimer = new System.Timers.Timer(1000);


        enum ProgramStates
        {
            PeriodStarted,
            LockedPrematurely,
            PeriodFinished,
            Excercising,
        }

        object ProgramStateLock = new object();
        ProgramStates ProgramState { get; set; }

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


            SM = new SharedModel(ElapsedStopwatch);

            ViewModel_MainWindow VM_MainWindow = new ViewModel_MainWindow();
            VM_MainWindow.Initialize(App.Current.Dispatcher, SM);

            ViewModel_OverallParameters VM_OverallParameters = new ViewModel_OverallParameters();
            VM_OverallParameters.Initialize(App.Current.Dispatcher, SM);

            VM_App = new ViewModel_App();
            VM_App.Initialize(App.Current.Dispatcher, SM, this);

            main.DataContext = new {    MainWindow = VM_MainWindow,
                                        OverallParameters = VM_OverallParameters,
                                        App = VM_App};

            //Every second timer hooked up to a method. Not starting it yet.
            lock (EverySecondTimer) { EverySecondTimer.Elapsed += EverySecondTimer_Elapsed; }

            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SessionSwitchOccured);


            main.Show();
            
        }

        public void StartMainApplication()
        {
            ElapsedStopwatch.Start();
            lock (EverySecondTimer) { EverySecondTimer.Enabled = true; }
            lock (ProgramStateLock) { ProgramState = ProgramStates.PeriodStarted; }
            
        }

        private void EverySecondTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            lock(EverySecondTimer)
            {
                if(EverySecondTimer.Enabled)
                {
                    SM.ActiveSessionTime = ElapsedStopwatch.Elapsed;

                    TimeSpan TimeOwedIncrement;
                    lock (SM.ParameterLock)
                    {
                        TimeOwedIncrement = new TimeSpan((long)((((Decimal)SM.ExcercisePeriod.Ticks) / ((Decimal)SM.OverallPeriod.Ticks)) *
                                                                            ((Decimal)ElapsedStopwatch.Elapsed.Ticks))); 
                    }

                    SM.TimeOwed = SM.TimeOwedPrevious + TimeOwedIncrement;

                    VM_App.UpdateUI();
                }
            }
            
        }

        private void SessionSwitchOccured(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            lock(ProgramStateLock)
            {
                switch (ProgramState)
                {
                    case ProgramStates.PeriodStarted:

                        break;
                    case ProgramStates.LockedPrematurely:

                        break;
                    case ProgramStates.PeriodFinished:

                        break;
                    case ProgramStates.Excercising:

                        break;

                    default:
                        break;

                }
            }
            
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
        /// </summary>
        readonly TimeSpan OverallPeriodDefault = new TimeSpan(hours: 0, minutes: 0, seconds: 10);
        readonly TimeSpan ExcercisePeriodDefault = new TimeSpan(hours: 0, minutes: 0, seconds: 5);

        //TO DO: insert setter control.
        public object ParameterLock = new object();
        public TimeSpan OverallPeriod { get; set; }
        public TimeSpan ExcercisePeriod { get; set; }


        //Stopwatch_ThreadSafe ElapsedStopwatch;
        //public TimeSpan ActiveSessionElapsed() { return ElapsedStopwatch.Elapsed; }
       
        public TimeSpan ActiveSessionTime { get; set; } = new TimeSpan();

        public TimeSpan TimeOwedPrevious { get; set; } = new TimeSpan();
        public TimeSpan TimeOwed { get; set; } = new TimeSpan();



        public SharedModel(Stopwatch_ThreadSafe elapsedStopwatch)
        {
            lock(ParameterLock)
            {
                this.OverallPeriod = OverallPeriodDefault;
                this.ExcercisePeriod = ExcercisePeriodDefault;
            }
            

            //ElapsedStopwatch = elapsedStopwatch;
        }

        public void UpdateOverallParameters(TimeSpan EnteredOverallPeriod, TimeSpan EneteredExcercisePeriod)
        {
            lock (ParameterLock)
            {
                this.OverallPeriod = EnteredOverallPeriod;
                this.ExcercisePeriod = EneteredExcercisePeriod;
            }
        }


    }

    class Stopwatch_ThreadSafe
    {
        Stopwatch s;

        public Stopwatch_ThreadSafe() { s = new Stopwatch(); }

        public TimeSpan Elapsed { get { TimeSpan local; lock (s) { local = s.Elapsed; } return local; } }

        public void Start() { lock (s) { s.Start(); } }
        public void Stop() { lock (s) { s.Stop(); } }
        public void Reset() { lock (s) { s.Reset(); } }
    }
}
