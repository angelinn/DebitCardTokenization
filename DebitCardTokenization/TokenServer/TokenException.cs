using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenServer
{
    public class DebitCardException : Exception
    {
        public DebitCardException(string message) : base(message)
        {   }
    }

    public class InvalidCardID : DebitCardException
    {
        public InvalidCardID(string message) : base(message)
        {   }
    }
}
