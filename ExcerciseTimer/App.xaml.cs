using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ExcerciseTimer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Main application method. Called at some point after App was constructed in the generated static Main() method.
        /// Linked to by App.xaml.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ApplicationEntryPoint(object sender, StartupEventArgs e)
        {
            Console.WriteLine("Hello world.");
        }
    }
}
