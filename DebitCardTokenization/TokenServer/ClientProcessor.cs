using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Net;
using Tokenization.Access;
using Tokenization.Activities;

namespace TokenServer
{
    public class ClientProcessor
    {
        private Socket currentSocket;
        private BinaryReader reader;
        private BinaryWriter writer;
        private NetworkStream networkStream;
        private Client client;
        private Tokenizer tokenizer;
        private List<Client> clientsRef;
        private List<BankCard> bankCardsRef;
        private Action<object> DisplayMethod;
        

        public ClientProcessor(Action<object> message, List<Client> clients, List<BankCard> cards)
        {
            DisplayMethod = message;
            clientsRef = clients;
            bankCardsRef = cards;
        }

        private void AddToList<T>(List<T> list, T item)
        {
            lock (this)
            {
                list.Add(item);
            }
        }

        public void Process(object socket)
        {
            currentSocket = socket as Socket;
            if (currentSocket == null)
                throw new InvalidDataException();

            networkStream = new NetworkStream(currentSocket);
            reader = new BinaryReader(networkStream);
            writer = new BinaryWriter(networkStream);
            tokenizer = new Tokenizer();

            try
            {
                while (client == null && currentSocket.Connected)
                    client = DetermineLogin();
            }
            catch (Exception e)
            {
                DisplayMethod(e.Message);
            }

            while(currentSocket.Connected)
            {
                try
                {
                    ProcessRequest();
                }
                catch(Exception e)
                {
                    DisplayMethod(e.Message);
                }
            }

            reader.Close();
            writer.Close();
            networkStream.Close();
            currentSocket.Close();
        }

        private Client DetermineLogin()
        {
            Activity response = (Activity)reader.ReadInt32();

            if (response == Activity.REGISTER)
                return RegisterClient();

            if (response == Activity.LOGIN)
                return LogClientIn();

            throw new InvalidDataException();
        }

        private void ProcessRequest()
        {
            Activity response = (Activity)reader.ReadInt32();
            string ID = null;
            string token = null;

            if (response == Activity.REQUEST_CARD)
            {
                if (client.Access < AccessLevel.REQUEST)
                {
                    writer.Write("You cannot request ID of a card.");
                    return;
                }

                token = reader.ReadString();
                ID = RequestCardID(token);

                string returnMessage = ID == null ? "There's no ID associated to this token." : ID;
                writer.Write(returnMessage);
                DisplayMethod(String.Format("{0} requested {1}", client.Username, returnMessage));
            }
            else if (response == Activity.REGISTER_TOKEN)
            {
                if (client.Access < AccessLevel.REGISTER)
                {
                    writer.Write("You cannot register tokens for a card.");
                    return;
                }

                ID = reader.ReadString();
                if ((token = tokenizer.MakeToken(ID)) == null)
                {
                    writer.Write("The ID of the card is not valid.");
                    return;
                }

                AddToList<BankCard>(bankCardsRef, new BankCard(ID, new Token(token, ID)));
                writer.Write(token);
                DisplayMethod(String.Format("{0} created Token {1}", client.Username, token));
            }
        }

        private string RequestCardID(string token)
        {
            foreach (BankCard card in bankCardsRef)
            {
                if (card.Tokens.Any(tk => tk.ID == token))
                    return card.ID;
            }
            return null;
        }

        private Client RegisterClient()
        {
            string username = String.Empty;
            string password = String.Empty;
            AccessLevel access = AccessLevel.NONE;

            username = reader.ReadString();
            password = reader.ReadString();
            access = (AccessLevel)reader.ReadInt32();

            if (clientsRef.Any(cl => cl.Username == username))
            {
                writer.Write("Username already exists!");
                return null;
            }
            
            writer.Write("200");
            Client current = new Client(username.Trim(), password.Trim(), (AccessLevel)Convert.ToInt32(access));
            AddToList<Client>(clientsRef, current);
            DisplayMethod(String.Format("{0} has registered successfully.", username));

            return current;
        }

        private Client LogClientIn()
        {
            string username = String.Empty;
            string password = String.Empty;
            Client current = null;

            try
            {
                username = reader.ReadString();
                password = reader.ReadString();
                current = clientsRef.Single(cl => cl.Username == username && cl.Password == password);

                writer.Write(String.Format("Welcome back, {0}!", client.Username));
                DisplayMethod(String.Format("{0} has logged in.", username));
            }
            catch (InvalidOperationException)
            {
                writer.Write("Username or Password was incorrect.");
            }

            return current;
        }
    }
}
