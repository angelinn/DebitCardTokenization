using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using Tokenization.Activities;
using Tokenization.Access;
using Tokenization.Consts;
using System.Text.RegularExpressions;

namespace TokenClient
{
    public class Client
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private BinaryReader reader;
        private BinaryWriter writer;
        private Action<object> DisplayMessage;
        private Action<object> DisplayBox;

        public Client(Action<object> DisplayM, Action<object> Box)
        {
            DisplayMessage = DisplayM;
            DisplayBox = Box;
            Connect();
        }

        public void RequestToken(string from)
        {
            writer.Write((int)Activity.REGISTER_TOKEN);
            writer.Write(from);
            DisplayMessage(reader.ReadString());
        }

        public void RequestCardID(string from)
        {
            writer.Write((int)Activity.REQUEST_CARD);
            writer.Write(from);
            DisplayMessage(reader.ReadString());
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
            catch (Exception e)
            {
                DisplayBox(e.Message);
                System.Environment.Exit(System.Environment.ExitCode);
            }
        }

        public bool IsUsernameValid(string username)
        {
            return Regex.Match(username, "^(?=.{6,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$").Success;
        }

        public bool Register(string username, string password, AccessLevel access)
        {
            writer.Write((int)Activity.REGISTER);
            writer.Write(username);
            writer.Write(password);
            writer.Write((int)access);

            string returnedMessage = reader.ReadString();
            DisplayBox(returnedMessage);

            if (returnedMessage == Constants.REGISTER_SUCCESSFUL)
                return true;

            return false;
        }

        public bool LogIn(string username, string password)
        {
            writer.Write((int)Activity.LOGIN);
            writer.Write(username);
            writer.Write(password);

            string returnedMessage = reader.ReadString();
            DisplayBox(returnedMessage);

            if (returnedMessage.StartsWith(Constants.LOGIN_SUCCESSFUL))
                return true;

            return false;
        }
    }
}
