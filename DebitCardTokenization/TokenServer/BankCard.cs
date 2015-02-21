using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenServer
{
    [Serializable]
    public class BankCard
    {
        private string id;
        private List<Token> tokens;

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

        public List<Token> Tokens
        {
            get
            {
                return tokens;
            }
            set
            {
                if (value != null)
                    tokens = value;
            }
        }

        public BankCard(string num, Token token)
        {
            Tokens = new List<Token>();
            ID = num;
            Tokens.Add(token);
        }
            
        public BankCard() : this(String.Empty, null)
        {   }
        
        private BankCard(BankCard other)
        {   }
    }
}
