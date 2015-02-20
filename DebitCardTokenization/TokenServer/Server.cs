using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace TokenServer
{
    public class Server
    {
        private Thread readThread;
        private List<Client> clients;
        private List<BankCard> bankCards;
        private Tokenizer tokenizer;
        private Action<object> DisplayMethod;
        private Action<object> DisplayError;

        //private List<Token> tokens; Needed in Sorted token output

        public Server(Action<object> message, Action<object> error)
        {
            DisplayMethod = message;
            DisplayError = error;
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
                DisplayMethod("Server loaded. Waiting for connection.");

                while (true)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(new ClientProcessor(
                                                 DisplayMethod, clients, bankCards).Process),
                                                 listener.AcceptSocket());

                    DisplayMethod("Connection received.");
                }
            }
            catch (Exception e)
            {
                DisplayError(e.Message);
            }
        }
    }
}
