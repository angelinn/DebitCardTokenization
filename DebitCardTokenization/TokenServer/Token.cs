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
        private BankCard owner;

        public string ID
        {
            get
            {
                return ID;
            }
            set
            {
                if (value != null)
                    ID = value;
                else
                    ID = String.Empty;
            }
        }

        public BankCard Owner
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
    }
}
