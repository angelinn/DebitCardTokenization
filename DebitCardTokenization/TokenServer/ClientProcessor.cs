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
using Tokenization.Consts;

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

        ~ClientProcessor()
        {
            reader.Close();
            writer.Close();
            networkStream.Close();
            currentSocket.Close();
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
                writer.Write(Constants.INVALID_CARD_ID);
                return;
            }

            int attempts = 0;
            while(!IsTokenAvailable(token))
            {
                if (attempts++ == 100000)
                {
                    writer.Write(Constants.TOKEN_CREATE_FAILED);
                    return;
                }
                token = Tokenizer.MakeToken(cardID);
            }

            AddToCards(cardID, token);
            writer.Write(token);
            DisplayMethod(String.Format(Constants.NAME_HAS_CREATED_TOKEN, client.Username, token));
        }

        private void ProcessRequest()
        {
            Activity response = (Activity)reader.ReadInt32();

            if (response == Activity.REQUEST_CARD && client.Access >= AccessLevel.REQUEST)
                RequestCardID();
            else if (response == Activity.REGISTER_TOKEN && client.Access >= AccessLevel.REGISTER)
                RequestToken();
            else
                writer.Write(Constants.ACCESS_DENIED);
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
            string cardID = Constants.ID_NOT_FOUND;

            foreach (BankCard card in bankCardsRef)
            {
                if (card.Tokens.Any(tk => tk.ID == token))
                {
                    cardID = card.ID;
                    break;
                }
            }

            writer.Write(cardID);
            DisplayMethod(String.Format(Constants.NAME_HAS_REQUESTED, client.Username, cardID));
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
                writer.Write(Constants.USERNAME_EXISTS);
                return null;
            }
            
            writer.Write(Constants.REGISTER_SUCCESSFUL);
            Client current = new Client(username.Trim(), password.Trim(), (AccessLevel)Convert.ToInt32(access));
            lock(this)
            {
                clientsRef.Add(current);
            }
            DisplayMethod(String.Format(Constants.NAME_HAS_REGISTERED, username));

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

                writer.Write(String.Format(Constants.WELCOME_BACK_NAME, current.Username));
                DisplayMethod(String.Format(Constants.NAME_HAS_LOGGED_IN, username));
            }
            catch (InvalidOperationException)
            {
                writer.Write(Constants.INCORRECT_INPUT);
            }

            return current;
        }
    }
}
