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
