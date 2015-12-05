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

                    if (EnteredSpan <= SM.ExcercisePeriod)
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

                        SM.OverallPeriod = EnteredSpan;

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

                    if (EnteredSpan >= SM.OverallPeriod)
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

                        SM.ExcercisePeriod = EnteredSpan;

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
