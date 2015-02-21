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
        private List<Client> clientsRef;
        private List<BankCard> bankCardsRef;
        private Action<object> DisplayMethod;
        

        public ClientProcessor(Action<object> message, List<Client> clients, List<BankCard> cards)
        {
            DisplayMethod = message;
            clientsRef = clients;
            bankCardsRef = cards;
        }

        private void AddToCards(string cardID, string token)
        {
            lock (this)
            {
                BankCard current = null;
                try
                {
                    current = bankCardsRef.Single(card => card.ID == cardID);
                    current.Tokens.Add(new Token(token, current.ID));
                }
                catch(InvalidOperationException)
                {
                    bankCardsRef.Add(new BankCard(cardID, new Token(token, cardID)));
                }
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

        private void RequestToken()
        {
            string cardID = reader.ReadString();
            string token = String.Empty;
            if ((token = Tokenizer.MakeToken(cardID)) == null)
            {
                writer.Write("The ID of the card is not valid.");
                return;
            }

            int attempts = 0;
            while(!IsTokenAvailable(token))
            {
                if (attempts++ == 100000)
                {
                    writer.Write("Could not create token.");
                    return;
                }
                token = Tokenizer.MakeToken(cardID);
            }

            AddToCards(cardID, token);
            writer.Write(token);
            DisplayMethod(String.Format("{0} created Token {1}", client.Username, token));
        }

        private void ProcessRequest()
        {
            Activity response = (Activity)reader.ReadInt32();

            if (response == Activity.REQUEST_CARD && client.Access >= AccessLevel.REQUEST)
                RequestCardID();
            else if (response == Activity.REGISTER_TOKEN && client.Access >= AccessLevel.REGISTER)
                RequestToken();
            else
                writer.Write("Your access level is not high enough");
        }

        private bool IsTokenAvailable(string token)
        {
            foreach(BankCard card in bankCardsRef)
            {
                if (card.Tokens.Any(tk => tk.ID == token))
                    return false;
            }
            return true;
        }

        private void RequestCardID()
        {
            string token = reader.ReadString();
            string cardID = "There's no ID associated to this token.";

            foreach (BankCard card in bankCardsRef)
            {
                if (card.Tokens.Any(tk => tk.ID == token))
                {
                    cardID = card.ID;
                    break;
                }
            }

            writer.Write(cardID);
            DisplayMethod(String.Format("{0} requested {1}", client.Username, cardID));
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
            lock(this)
            {
                clientsRef.Add(current);
            }
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

                writer.Write(String.Format("Welcome back, {0}!", current.Username));
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
