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
            clients = new List<Client>();
            bankCards = new List<BankCard>();
            tokenizer = new Tokenizer();

            readThread = new Thread(new ThreadStart(RunServer));
            readThread.Start();
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
                DisplayMessage("Server loaded. Waiting for connection.");

                while(true)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessClient), listener.AcceptSocket());
                    DisplayMessage("Connection received.");
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
                txtDisplay.Text += (string)message + '\n';
        }

        private void ProcessClient(object socket)
        {
            Socket currentSocket = socket as Socket;
            if (currentSocket == null)
                throw new InvalidDataException();

            NetworkStream networkStream = new NetworkStream(currentSocket);
            BinaryReader reader = new BinaryReader(networkStream);
            BinaryWriter writer = new BinaryWriter(networkStream);

            try
            {
                Client client = DetermineLogin(reader, writer, currentSocket);

                while(currentSocket.Connected)
                {
                    try
                    {
                        ProcessRequest(client, reader, writer, currentSocket);
                    }
                    catch()
                }
            }
            catch (Exception e)
            {
                DisplayMessage(e.Message);
            }

            reader.Close();
            writer.Close();
            networkStream.Close();
            currentSocket.Close();
        }

        private class ClientProcessor
        {
            private Socket currentSocket;
            private BinaryReader reader;
            private BinaryWriter writer;
            private Client client;

            public void Process(object socket)
            {

            }


        }

        private Client DetermineLogin(BinaryReader reader, BinaryWriter writer, Socket socket)
        {
            Activity response = (Activity)reader.ReadInt32();

            if (response == Activity.REGISTER)
                return RegisterClient(reader, writer, socket);

            if (response == Activity.LOGIN)
                return LogClientIn(reader, writer, socket);
      
            throw new InvalidDataException();
        }

        private void ProcessRequest(Client client, BinaryReader reader, BinaryWriter writer, Socket socket)
        {
            Activity response = (Activity)reader.ReadInt32();
            string ID = null;
            string token = null;

            if (response == Activity.REQUEST_CARD)
            {
                if (client.Access != AccessLevel.REQUEST)
                {
                    writer.Write("You cannot request ID of a card.");
                    return;
                }

                token = reader.ReadString();
                ID = RequestCardID(token);

                string returnMessage = ID == null ? "There's no ID associated to this token." : ID;
                writer.Write(returnMessage);
            }
            else if (response == Activity.REGISTER_TOKEN)
            {
                if (client.Access != AccessLevel.REGISTER)
                {
                    writer.Write("You cannot register tokens for a card.");
                    return;
                }

                ID = reader.ReadString();
                if((token = tokenizer.MakeToken(ID)) == null)
                {
                    writer.Write("The ID of the card is not valid.");
                    return;
                }

                writer.Write(token);
            }
        }

        private string RequestCardID(string token)
        {
            foreach(BankCard card in bankCards)
            {
                if (card.Tokens.Any(tk => tk.ID == token))
                    return card.ID;
            }
            return null;
        }

        private Client RegisterClient(BinaryReader reader, BinaryWriter writer, Socket socket)
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
                {
                    writer.Write("200");
                    break;
                }
            }
            Client current = new Client(username.Trim(), password.Trim(), (AccessLevel)Convert.ToInt32(access));
            clients.Add(current);
            DisplayMessage(String.Format("{0} has registered successfully.", username));

            return current;
        }

        private Client LogClientIn(BinaryReader reader, BinaryWriter writer, Socket socket)
        {
            string username = String.Empty;
            string password = String.Empty;
            Client current = null;

            while (socket.Connected)
            {
                try
                {
                    username = reader.ReadString();
                    password = reader.ReadString();
                    current = clients.Single(cl => cl.Username == username && cl.Password == password);
                }
                catch (InvalidOperationException)
                {
                    writer.Write("Username or Password was incorrect.");
                    continue;
                }

                writer.Write("200");
                break;
            }
            DisplayMessage(String.Format("{0} has logged in.", username));
            return current;
        }

        private void Window_Closed(object sender, EventArgs e)
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
    }
}
