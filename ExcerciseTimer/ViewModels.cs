using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ExcerciseTimer
{
    class ViewModel_OverallParameters : INotifyPropertyChanged
    {
        System.Windows.Threading.Dispatcher Dispatcher { get; set; }

        SharedModel SM { get; set; }

        public ViewModel_OverallParameters()
        {
            ToggleEntryDisplay = new RelayCommand(ToggleEntryDisplayMethod, o => true);
        }

        public void Initialize(System.Windows.Threading.Dispatcher d, SharedModel sm)
        {
            Dispatcher = d;
            SM = sm;

            OverallPeriod = SM.OverallPeriod.ToString("hh\\:mm\\:ss");
            ExcercisePeriod = SM.ExcercisePeriod.ToString("hh\\:mm\\:ss");
            EnteredOverallPeriod = SM.OverallPeriod;
            EnteredExcercisePeriod = SM.ExcercisePeriod;
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                this.Dispatcher.Invoke(new Action(() => { handler(this, new PropertyChangedEventArgs(propertyName)); }));
        }

       
        bool EntryMode { get; set; } = false;
        bool ParametersAreValid { get; set; } = true;
        public ICommand ToggleEntryDisplay { get; private set; }
        private void ToggleEntryDisplayMethod(object o)
        {
            if (ParametersAreValid)
            {
                if (EntryMode)
                {
                    ErrorOverallPeriodVisibility = "Collapsed";
                    ErrorExcercisePeriodVisibility = "Collapsed";

                    DisplayVisibility = "Visible";
                    EntryVisibility = "Hidden";

                    ButtonContent = "Change Parameters";

                    EntryMode = false;

                    //if enter values Are proper, update them here.
                    SM.UpdateOverallParameters(EnteredOverallPeriod, EnteredExcercisePeriod);

                }
                else
                {
                    DisplayVisibility = "Hidden";
                    EntryVisibility = "Visible";

                    ButtonContent = "Save";

                    EntryMode = true;
                }
            }
            else
                ParametersAreValid = true;
        }

        TimeSpan EnteredOverallPeriod;
        TimeSpan EnteredExcercisePeriod;
        

        Regex rgxFormat = new Regex(@"^\d{2}:\d{2}:\d{2}$");

        private string overallPeriod = "Not Initialized";
        public string OverallPeriod
        {
            get { return overallPeriod; }
            set
            {
                if (rgxFormat.IsMatch(value) == false)
                {
                    ParametersAreValid = false;
                    ErrorOverallPeriod = " Wrong Format.";
                    ErrorOverallPeriodVisibility = "Visible";

                    NotifyPropertyChanged();
                }
                else
                {
                    string[] input = value.Split(new[] { ':' });

                    TimeSpan EnteredSpan = new TimeSpan(int.Parse(input[0]), int.Parse(input[1]), int.Parse(input[2]));

                    if (EnteredSpan <= EnteredExcercisePeriod)
                    {
                        ParametersAreValid = false;
                        ErrorOverallPeriod = " Must be greater than Excercise Period.";
                        ErrorOverallPeriodVisibility = "Visible";

                        NotifyPropertyChanged();
                    }
                    else
                    {
                        ErrorOverallPeriodVisibility = "Collapsed";

                        overallPeriod = value;

                        NotifyPropertyChanged();

                        EnteredOverallPeriod = EnteredSpan;
                        

                    }
                }
            }
        }

        private string errorOverallPeriod = "Not Initialized";
        public string ErrorOverallPeriod
        {
            get { return errorOverallPeriod; }
            set
            {
                errorOverallPeriod = value;
                NotifyPropertyChanged();
            }
        }

        private string errorOverallPeriodVisibility = "Collapsed";
        public string ErrorOverallPeriodVisibility
        {
            get { return errorOverallPeriodVisibility; }
            set
            {
                errorOverallPeriodVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private string excercisePeriod = "Not Initialized";
        public string ExcercisePeriod
        {
            get { return excercisePeriod; }
            set
            {
                if (rgxFormat.IsMatch(value) == false)
                {
                    ParametersAreValid = false;
                    ErrorExcercisePeriod = " Wrong Format.";
                    ErrorExcercisePeriodVisibility = "Visible";

                    NotifyPropertyChanged();
                }
                else
                {
                    string[] input = value.Split(new[] { ':' });

                    TimeSpan EnteredSpan = new TimeSpan(int.Parse(input[0]), int.Parse(input[1]), int.Parse(input[2]));

                    if (EnteredSpan >= EnteredOverallPeriod)
                    {
                        ParametersAreValid = false;
                        ErrorExcercisePeriod = "  Must be less than Overall Period.";
                        ErrorExcercisePeriodVisibility = "Visible";

                        NotifyPropertyChanged();
                    }
                    else
                    {
                        ParametersAreValid = true;
                        ErrorExcercisePeriodVisibility = "Collapsed";

                        excercisePeriod = value;

                        NotifyPropertyChanged();

                        EnteredExcercisePeriod = EnteredSpan;
                         

                    }
                }
            }
        }

        private string errorExcercisePeriod = "Not Initialized";
        public string ErrorExcercisePeriod
        {
            get { return errorExcercisePeriod; }
            set
            {
                errorExcercisePeriod = value;
                NotifyPropertyChanged();
            }
        }

        private string errorExcercisePeriodVisibility = "Collapsed";
        public string ErrorExcercisePeriodVisibility
        {
            get { return errorExcercisePeriodVisibility; }
            set
            {
                errorExcercisePeriodVisibility = value;
                NotifyPropertyChanged();
            }
        }



        private string format = " hh:mm:ss";
        public string Format
        {
            get { return format; }
        }


        private string displayVisibility = "Visible";
        public string DisplayVisibility
        {
            get { return displayVisibility; }
            set
            {
                displayVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private string entryVisibility = "Hidden";
        public string EntryVisibility
        {
            get { return entryVisibility; }
            set
            {
                entryVisibility = value;
                NotifyPropertyChanged();
            }
        }


        private string buttonContent = "Change Parameters";
        public string ButtonContent
        {
            get { return buttonContent; }
            set
            {
                buttonContent = value;
                NotifyPropertyChanged();
            }
        }
    }

    class ViewModel_MainWindow : INotifyPropertyChanged
    {
        System.Windows.Threading.Dispatcher Dispatcher { get; set; }

        SharedModel SM { get; set; }

        public void Initialize(System.Windows.Threading.Dispatcher d, SharedModel sm)
        {
            Dispatcher = d;
            SM = sm;


            //20 title 14 normal default.
            FontSize = "14";
            TitleFontSize = "20";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                this.Dispatcher.Invoke(new Action(() => { handler(this, new PropertyChangedEventArgs(propertyName)); }));
        }

        private string fontSize = "11";
        public string FontSize
        {
            get { return fontSize; }
            set
            {
                fontSize = value;
                NotifyPropertyChanged();
            }
        }

        private string titleFontSize = "11";
        public string TitleFontSize
        {
            get { return titleFontSize; }
            set
            {
                titleFontSize = value;
                NotifyPropertyChanged();
            }
        }


    }

    class ViewModel_App : INotifyPropertyChanged
    {
        System.Windows.Threading.Dispatcher Dispatcher { get; set; }

        SharedModel SM { get; set; }

        App App { get; set; }

        public ViewModel_App()
        {
            Start = new RelayCommand(StartMainApplication, o => true);
        }
        
        public void Initialize(System.Windows.Threading.Dispatcher d, SharedModel sm, App app)
        {
            Dispatcher = d;
            SM = sm;

            App = app;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                this.Dispatcher.Invoke(new Action(() => { handler(this, new PropertyChangedEventArgs(propertyName)); }));
        }

        public ICommand Start { get; private set; }
        private void StartMainApplication(object o)
        {
            StartVisibility = "Collapsed";
            MainModeVisibility = "Visible";

            App.StartMainApplication();
        }

        private string startVisibility = "Visible";
        public string StartVisibility
        {
            get { return startVisibility; }
            set
            {
                startVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private string mainModeVisibility = "Collapsed";
        public string MainModeVisibility
        {
            get { return mainModeVisibility; }
            set
            {
                mainModeVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private string sessionTime = "00:00:00";
        public string SessionTime
        {
            get { return sessionTime; }
            set
            {
                sessionTime = value;
                NotifyPropertyChanged();
            }
        }

        private string timeOwed = "00:00:00";
        public string TimeOwed
        {
            get { return timeOwed; }
            set
            {
                timeOwed = value;
                NotifyPropertyChanged();
            }
        }

        public void UpdateUI()
        {
            SessionTime = SM.ActiveSessionTime.ToString("hh\\:mm\\:ss");
            TimeOwed = SM.TimeOwed.ToString("hh\\:mm\\:ss");
        }
    }

        public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }

    
}
