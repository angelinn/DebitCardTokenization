using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenServer
{
    public class Client
    {
        private string username;
        private string password;
        private string cardID;
        private string cardToken;

        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                if (value != null)
                    username = value;
            }
        }
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                if (value != null)
                    password = value;
            }
        }

        public string CardID
        {
            get
            {
                return cardID;
            }
            set
            {
                if (value != null)
                    cardID = value;
            }
        }

        public string CardToken
        {
            get
            {
                return cardToken;
            }
            set
            {
                if (value != null)
                    cardToken = value;
            }
        }

        public Client(string un, string pw, string id, string token)
        {
            Username = un;
            Password = pw;
            CardID = id;
            CardToken = token;
        }

        public Client() : this(String.Empty, String.Empty, String.Empty, String.Empty)
        {   }

        public Client(Client other) : this(other.Username, other.Password, other.CardID, other.CardToken)
        {   }

    }
}
