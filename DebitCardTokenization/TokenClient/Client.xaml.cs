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

namespace TokenClient
{
    public partial class Client : Window
    {
        public Client()
        {
            InitializeComponent();
        }

        private void DisplayMessage(object[] messageControlPair)
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke(new Action<object[]>(DisplayMessage), messageControlPair);
            else
            {
                TextBox box = messageControlPair[1] as TextBox;
                string message = messageControlPair[0] as string;
                box.Text += message;
            }
        }
        private void mainWindow_Closed(object sender, EventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);
        }
    }
}
