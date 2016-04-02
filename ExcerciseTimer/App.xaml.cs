using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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

        FlashWindowHelper FlashHelper { get; set; }

        SharedModel SM;

        ViewModel_App VM_App;


        Stopwatch_ThreadSafe ElapsedStopwatch = new Stopwatch_ThreadSafe();

        System.Timers.Timer EverySecondTimer = new System.Timers.Timer(1000);


        enum ProgramStates
        {
            WaitingForUserStart,
            Working,
            TimeToExcercise,
            Excercising
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

            MainWindow = main;

            TrayIconUnmanaged = new System.Windows.Forms.NotifyIcon();
            TrayIconUnmanaged.Icon = ExcerciseTimer.Properties.Resources.AppIcon;
            TrayIconUnmanaged.Visible = true;
            TrayIconUnmanaged.DoubleClick += (s, args) => { main.Show(); main.WindowState = WindowState.Normal; };

            FlashHelper = new FlashWindowHelper(System.Windows.Application.Current);

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

            lock (ProgramStateLock) { ProgramState = ProgramStates.WaitingForUserStart; }

            main.Show();
            
        }

        public void StartMainApplication()
        {
            lock (ProgramStateLock)
            {
                ProgramState = ProgramStates.Working;

                ElapsedStopwatch.Start();
                lock (EverySecondTimer) { EverySecondTimer.Enabled = true; }
            }
            
        }

        
        private void EverySecondTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            lock (ProgramStateLock)
            {
                lock (EverySecondTimer)
                {
                    if (EverySecondTimer.Enabled)
                    {
                        SM.ActiveSessionTime = ElapsedStopwatch.Elapsed;

                        TimeSpan TimeOwedNew;
                        lock (SM.ParameterLock)
                        {
                            TimeOwedNew = new TimeSpan((long)((((Decimal)SM.ExcercisePeriod.Ticks) / ((Decimal)SM.OverallPeriod.Ticks)) *
                                                                                ((Decimal)ElapsedStopwatch.Elapsed.Ticks)));
                            if(ProgramState == ProgramStates.Working)
                            {
                                if ((SM.TimeOwedPrevious + TimeOwedNew) > SM.ExcercisePeriod)
                                    ProgramState = ProgramStates.TimeToExcercise;
                            }
                        }

                        SM.TimeOwed = SM.TimeOwedPrevious + TimeOwedNew;

                        VM_App.UpdateUI();

                        if (ProgramState == ProgramStates.TimeToExcercise)
                        {
                            App.Current.Dispatcher.Invoke(new Action(() => { FlashHelper.FlashApplicationWindow(); MainWindow.Show(); }));
                            
                        }
                            

                    } 
                }
            }
           
        }


        private void SessionSwitchOccured(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            lock(ProgramStateLock)
            {
                switch (ProgramState)
                {
                    case ProgramStates.WaitingForUserStart:
                        break;
                    case ProgramStates.Working:
                    case ProgramStates.TimeToExcercise:
                        if (e.Reason == SessionSwitchReason.SessionLock)
                        {
                            //Final count of worktime till now.
                            ElapsedStopwatch.Stop();

                            //Stop updating interface
                            lock (EverySecondTimer) { EverySecondTimer.Enabled = false; }

                            lock (SM.ParameterLock)
                            {
                                SM.TimeOwed = SM.TimeOwedPrevious + new TimeSpan((long)((((Decimal)SM.ExcercisePeriod.Ticks) / ((Decimal)SM.OverallPeriod.Ticks)) *
                                                                                    ((Decimal)ElapsedStopwatch.Elapsed.Ticks)));
                            }

                            //Beging count of how long station is locked.
                            ElapsedStopwatch.Reset();
                            ElapsedStopwatch.Start();

                            ProgramState = ProgramStates.Excercising;
                        }                        
                        break;
                    case ProgramStates.Excercising:
                        if (e.Reason == SessionSwitchReason.SessionUnlock)
                        {
                            //ElapsedStopWatch now holds time away from desk.
                            ElapsedStopwatch.Stop();

                            ProgramState = ProgramStates.Working;

                            //Reduce timeowed by time away. The formula in the EverySecond method will calculate time owed properly now.
                            SM.TimeOwedPrevious = SM.TimeOwed > ElapsedStopwatch.Elapsed ? SM.TimeOwed - ElapsedStopwatch.Elapsed : new TimeSpan();
                            SM.TimeOwed = SM.TimeOwedPrevious;

                            //Beging count of how new session lasts
                            ElapsedStopwatch.Reset();
                            ElapsedStopwatch.Start();

                            //Update UI manually to correct post lock values.
                            VM_App.UpdateUI();

                            //Start updating interface again.
                            lock (EverySecondTimer) { EverySecondTimer.Enabled = true; }
                                                       
                        }
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
        readonly TimeSpan OverallPeriodDefault = new TimeSpan(hours: 1, minutes: 0, seconds: 0);
        readonly TimeSpan ExcercisePeriodDefault = new TimeSpan(hours: 0, minutes: 5, seconds: 0);

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

    public class FlashWindowHelper
    {
        private IntPtr mainWindowHWnd;
        private System.Windows.Application theApp;

        public FlashWindowHelper(System.Windows.Application app)
        {
            this.theApp = app;
        }

        public void FlashApplicationWindow()
        {
            InitializeHandle();
            Flash(this.mainWindowHWnd, 5);
        }

        public void StopFlashing()
        {
            InitializeHandle();

            if (Win2000OrLater)
            {
                FLASHWINFO fi = CreateFlashInfoStruct(this.mainWindowHWnd, FLASHW_STOP, uint.MaxValue, 0);
                FlashWindowEx(ref fi);
            }
        }

        private void InitializeHandle()
        {
            if (this.mainWindowHWnd == IntPtr.Zero)
            {
                // Delayed creation of Main Window IntPtr as Application.Current passed in to ctor does not have the MainWindow set at that time
                var mainWindow = this.theApp.MainWindow;
                this.mainWindowHWnd = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            /// <summary>
            /// The size of the structure in bytes.
            /// </summary>
            public uint cbSize;
            /// <summary>
            /// A Handle to the Window to be Flashed. The window can be either opened or minimized.
            /// </summary>
            public IntPtr hwnd;
            /// <summary>
            /// The Flash Status.
            /// </summary>
            public uint dwFlags;
            /// <summary>
            /// The number of times to Flash the window.
            /// </summary>
            public uint uCount;
            /// <summary>
            /// The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.
            /// </summary>
            public uint dwTimeout;
        }

        /// <summary>
        /// Stop flashing. The system restores the window to its original stae.
        /// </summary>
        public const uint FLASHW_STOP = 0;

        /// <summary>
        /// Flash the window caption.
        /// </summary>
        public const uint FLASHW_CAPTION = 1;

        /// <summary>
        /// Flash the taskbar button.
        /// </summary>
        public const uint FLASHW_TRAY = 2;

        /// <summary>
        /// Flash both the window caption and taskbar button.
        /// This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
        /// </summary>
        public const uint FLASHW_ALL = 3;

        /// <summary>
        /// Flash continuously, until the FLASHW_STOP flag is set.
        /// </summary>
        public const uint FLASHW_TIMER = 4;

        /// <summary>
        /// Flash continuously until the window comes to the foreground.
        /// </summary>
        public const uint FLASHW_TIMERNOFG = 12;

        /// <summary>
        /// Flash the spacified Window (Form) until it recieves focus.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static bool Flash(IntPtr hwnd)
        {
            // Make sure we're running under Windows 2000 or later
            if (Win2000OrLater)
            {
                FLASHWINFO fi = CreateFlashInfoStruct(hwnd, FLASHW_ALL | FLASHW_TIMERNOFG, uint.MaxValue, 0);

                return FlashWindowEx(ref fi);
            }
            return false;
        }

        private static FLASHWINFO CreateFlashInfoStruct(IntPtr handle, uint flags, uint count, uint timeout)
        {
            FLASHWINFO fi = new FLASHWINFO();
            fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));
            fi.hwnd = handle;
            fi.dwFlags = flags;
            fi.uCount = count;
            fi.dwTimeout = timeout;
            return fi;
        }

        /// <summary>
        /// Flash the specified Window (form) for the specified number of times
        /// </summary>
        /// <param name="hwnd">The handle of the Window to Flash.</param>
        /// <param name="count">The number of times to Flash.</param>
        /// <returns></returns>
        public static bool Flash(IntPtr hwnd, uint count)
        {
            if (Win2000OrLater)
            {
                FLASHWINFO fi = CreateFlashInfoStruct(hwnd, FLASHW_ALL | FLASHW_TIMERNOFG, count, 0);

                return FlashWindowEx(ref fi);
            }

            return false;
        }

        /// <summary>
        /// A boolean value indicating whether the application is running on Windows 2000 or later.
        /// </summary>
        private static bool Win2000OrLater
        {
            get { return Environment.OSVersion.Version.Major >= 5; }
        }
    }
}
