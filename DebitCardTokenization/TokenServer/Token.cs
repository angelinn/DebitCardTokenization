using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenServer
{
    public class Token
    {
        private string id;
        private string owner;

        public string ID
        {
            get
            {
                return id;
            }
            set
            {
                if (value != null)
                    id = value;
                else
                    id = String.Empty;
            }
        }

        public string Owner
        {
            get
            {
                return owner;
            }
            set
            {
                if (value != null)
                    owner = value;
            }
        }

        public Token(string numbers, string own)
        {
            ID = numbers;
            Owner = own;
        }
    }
}
