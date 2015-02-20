using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TokenServer
{
    public partial class MainWindow : Window
    {
        private Server server;
        public MainWindow()
        {
            InitializeComponent();
            server = new Server(DisplayMessage, DisplayError);
        }

        public void DisplayError(object message)
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke(new Action<string>(DisplayMessage), message);
            else
                MessageBox.Show((string)message, "Fatal Error.", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void DisplayMessage(object message)
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke(new Action<string>(DisplayMessage), message);
            else
                txtDisplay.Text += (string)message + '\n';
        }       

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);
        }

    }
}
