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
using Tokenization.Activities;
using Tokenization.Access;

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

        private void DisplayMessage(object message)
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke(new Action<string>(DisplayMessage), message);
            else
            {
                ucRequest.Result = (string)message;
            }
        }

        private bool LogIn(string username, string password)
        {
            writer.Write((int)Activity.LOGIN);
            writer.Write(username);
            writer.Write(password);

            string returnedMessage = reader.ReadString();

            if (returnedMessage == "200" || returnedMessage.StartsWith("Welcome"))
                return true;

            MessageBox.Show(returnedMessage);
            return false;
        }

        private bool Register(string username, string password, AccessLevel access)
        {
            writer.Write((int)Activity.REGISTER);
            writer.Write(username);
            writer.Write(password);
            writer.Write((int)access);

            string returnedMessage = reader.ReadString();
            if (returnedMessage == "200")
                return true;

            MessageBox.Show(returnedMessage);
            return false;
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

        private void ucLogin_Login(object sender, LoginUserControl.LoginEventArgs args)
        {
            if(LogIn(args.Username, args.Password))
                ShowRequestUC();
        }

        private void ucLogin_Register(object sender, LoginUserControl.LoginEventArgs args)
        {
            if(Register(args.Username, args.Password, (AccessLevel)args.ListBoxMarked))
                ShowRequestUC();
        }

        private void RequestToken(string from)
        {
            writer.Write((int)Activity.REGISTER_TOKEN);
            writer.Write(from);
            DisplayMessage(reader.ReadString());
        }

        private void RequestCardID(string from)
        {
            writer.Write((int)Activity.REQUEST_CARD);
            writer.Write(from);
            DisplayMessage(reader.ReadString());
        }

        private void ShowRequestUC()
        {
            ucLogin.Visibility = Visibility.Hidden;
            ucRequest.Visibility = Visibility.Visible;
        }

        private void ucRequest_TokenRequested(object sender, TokenProcessorUserControl.GenerateEventArgs args)
        {
            RequestToken(args.From);
        }

        private void ucRequest_CardIDRequested(object sender, TokenProcessorUserControl.GenerateEventArgs args)
        {
            RequestCardID(args.From);
        }

    }
}
