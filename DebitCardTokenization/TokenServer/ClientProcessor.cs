using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Net;

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
        private Action<object> DisplayError;
        

        public ClientProcessor(Action<object> message, Action<object> error, List<Client> clients, List<BankCard> cards)
        {
            DisplayMethod = message;
            DisplayError = error;
            clientsRef = clients;
            bankCardsRef = cards;
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
                client = DetermineLogin();

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
            catch (Exception e)
            {
                DisplayError(e.Message);
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
                if ((token = tokenizer.MakeToken(ID)) == null)
                {
                    writer.Write("The ID of the card is not valid.");
                    return;
                }

                writer.Write(token);
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

            while (currentSocket.Connected)
            {
                username = reader.ReadString();
                password = reader.ReadString();
                access = (AccessLevel)reader.ReadInt32();

                if (clientsRef.Any(cl => cl.Username == username))
                    writer.Write("Username already exists!");
                else
                {
                    writer.Write("200");
                    break;
                }
            }
            Client current = new Client(username.Trim(), password.Trim(), (AccessLevel)Convert.ToInt32(access));
            clientsRef.Add(current);
            DisplayMethod(String.Format("{0} has registered successfully.", username));

            return current;
        }

        private Client LogClientIn()
        {
            string username = String.Empty;
            string password = String.Empty;
            Client current = null;

            while (currentSocket.Connected)
            {
                try
                {
                    username = reader.ReadString();
                    password = reader.ReadString();
                    current = clientsRef.Single(cl => cl.Username == username && cl.Password == password);
                }
                catch (InvalidOperationException)
                {
                    writer.Write("Username or Password was incorrect.");
                    continue;
                }

                writer.Write("200");
                break;
            }
            DisplayMethod(String.Format("{0} has logged in.", username));
            return current;
        }
    }

    public enum Activity
    {
        REGISTER = 12000,
        LOGIN = 14000,
        REGISTER_TOKEN = 15000,
        REQUEST_CARD = 16000
    };
}
