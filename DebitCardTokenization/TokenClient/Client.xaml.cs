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
using System.Net.Sockets;
using System.IO;

namespace TokenClient
{
    public partial class Client : Window
    {
        private const string LOCALHOST = "127.0.0.1";
        private const int PORT = 10000;

        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private BinaryReader reader;
        private BinaryWriter writer;

        public Client()
        {
            InitializeComponent();
            Connect();
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

        private void LogIn(string username, string password)
        {
            writer.Write((int)Activity.LOGIN);
            writer.Write(username);
            writer.Write(password);

            string returnedMessage = reader.ReadString();
            MessageBox.Show(returnedMessage);
        }
        public enum AccessLevel
        {
            NONE = 0,
            REGISTER = 1,
            REQUEST = 2,
            MASTER = 3
        };
        private void Register(string username, string password, AccessLevel access)
        {
            writer.Write((int)Activity.REGISTER);
            writer.Write(username);
            writer.Write(password);
            writer.Write((int)access);

            string returnedMessage = reader.ReadString();
            MessageBox.Show(returnedMessage);
        }

        private void Connect()
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(LOCALHOST, PORT);

                networkStream = tcpClient.GetStream();
                reader = new BinaryReader(networkStream);
                writer = new BinaryWriter(networkStream);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Fatal Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Environment.Exit(System.Environment.ExitCode);
            }
        }

        private void mainWindow_Closed(object sender, EventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);
        }

        private enum Activity
        {
            REGISTER = 12000,
            LOGIN = 14000,
            REGISTER_TOKEN = 15000,
            REQUEST_CARD = 16000
        };

        private void ucLogin_Login(object sender, LoginUserControl.LoginEventArgs args)
        {
            LogIn(args.Username, args.Password);
        }

        private void ucLogin_Register(object sender, LoginUserControl.LoginEventArgs args)
        {
            Register(args.Username, args.Password, (AccessLevel)args.ListBoxMarked);
        }


    }
}
