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
using System.Threading;
using System.Net;

namespace TokenServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Server : Window
    {
        private Thread readThread;
        private List<Client> clients;
        private List<BankCard> bankCards;
        //private List<Token> tokens; Needed in Sorted token output
        private Tokenizer tokenizer;


        public Server()
        {
            InitializeComponent();
            Load();
        }

        public void Load()
        {
            readThread = new Thread(new ThreadStart(RunServer));
            readThread.Start();
            clients = new List<Client>();
            bankCards = new List<BankCard>();
            // deserialize data - Cards and Clients
        }

        private void RunServer()
        {
            TcpListener listener;
            try
            {
                IPAddress localhost = IPAddress.Parse("127.0.0.1"); 
                listener = new TcpListener(localhost, 10000);

                listener.Start();

                while(true)
                {
                    DisplayMessage("Server loaded. Waiting for connection.");
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessClient), listener.AcceptSocket());
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Fatal error.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayMessage(object message)
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke(new Action<string>(DisplayMessage), message);
            else
                txtDisplay.Text += (string)message;
        }

        private void ProcessClient(object socket)
        {
            Socket currentSocket = socket as Socket;
            if (currentSocket == null)
                throw new InvalidDataException();

            NetworkStream networkStream = new NetworkStream(currentSocket);
            BinaryReader reader = new BinaryReader(networkStream);
            BinaryWriter writer = new BinaryWriter(networkStream);

            Activity response;
            
            response = (Activity)reader.ReadInt32();

            if (response == Activity.REGISTER)
                RegisterClient(reader, writer, currentSocket);
            else if (response == Activity.LOGIN)
                LogClientIn(reader, writer, currentSocket);
            else
                throw new InvalidDataException();

            do
            {
                response = (Activity)reader.ReadInt32();

                if (response == Activity.REQUEST_CARD)
                {

                }
                else if (response == Activity.REQUEST_TOKEN)
                {

                }
            } while (currentSocket.Connected);




            reader.Close();
            writer.Close();
            networkStream.Close();
            currentSocket.Close();
        }

        private void RegisterClient(BinaryReader reader, BinaryWriter writer, Socket socket)
        {
            string username = String.Empty;
            string password = String.Empty;
            AccessLevel access = AccessLevel.NONE;

            while (socket.Connected)
            {
                username = reader.ReadString();
                password = reader.ReadString();
                access = (AccessLevel)reader.ReadInt32();

                if (clients.Any(cl => cl.Username == username))
                    writer.Write("Username already exists!");
                else
                    break;
            }

            clients.Add(new Client(username.Trim(), password.Trim(), (AccessLevel)Convert.ToInt32(access)));
            DisplayMessage(String.Format("{0} has registered successfully.", username));
        }

        private void LogClientIn(BinaryReader reader, BinaryWriter writer, Socket socket)
        {
            string username = String.Empty;
            string password = String.Empty;

            while (socket.Connected)
            {
                try
                {
                    clients.Single(cl => cl.Username == username && cl.Password == password);
                }
                catch (InvalidOperationException)
                {
                    writer.Write("Username or Password was incorrect.");
                    continue;
                }
                break;
            }
            DisplayMessage(String.Format("{0} has logged in.", username));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);
        }

        private enum Activity
        {
            REGISTER = 12000,
            LOGIN = 14000,
            REQUEST_TOKEN = 15000,
            REQUEST_CARD = 16000
        };
    }
}
