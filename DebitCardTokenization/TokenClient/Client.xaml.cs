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
using System.Text.RegularExpressions;
using Tokenization.Consts;

namespace TokenClient
{
    public partial class Client : Window
    {
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
                ucRequest.Result = (string)message;
        }

        private bool LogIn(string username, string password)
        {
            writer.Write((int)Activity.LOGIN);
            writer.Write(username);
            writer.Write(password);

            string returnedMessage = reader.ReadString();
            MessageBox.Show(returnedMessage);

            if (returnedMessage.StartsWith(Constants.LOGIN_SUCCESSFUL))
                return true;

            return false;
        }

        private bool IsUsernameValid(string username)
        {
            return Regex.Match(username, "^(?=.{6,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$").Success;
        }

        private bool Register(string username, string password, AccessLevel access)
        {
            writer.Write((int)Activity.REGISTER);
            writer.Write(username);
            writer.Write(password);
            writer.Write((int)access);

            string returnedMessage = reader.ReadString();
            MessageBox.Show(returnedMessage);

            if (returnedMessage == Constants.REGISTER_SUCCESSFUL)
                return true;

            return false;
        }

        private void Connect()
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(Constants.LOCALHOST, Constants.PORT);

                networkStream = tcpClient.GetStream();
                reader = new BinaryReader(networkStream);
                writer = new BinaryWriter(networkStream);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, Constants.FATAL_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (!IsUsernameValid(args.Username))
                MessageBox.Show(Constants.USERNAME_INCORRECT, Constants.INCORRECT_TITLE,
                                MessageBoxButton.OK, MessageBoxImage.Information);
            if(args.ListBoxMarked == -1)
                MessageBox.Show(Constants.ACCESS_NOT_SELECTED, Constants.INCORRECT_TITLE,
                                MessageBoxButton.OK, MessageBoxImage.Information);  

            else if(Register(args.Username, args.Password, (AccessLevel)args.ListBoxMarked))
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
