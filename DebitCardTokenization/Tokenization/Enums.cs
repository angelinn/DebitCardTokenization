using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tokenization
{
    namespace Access
    {
        public enum AccessLevel
        {
            NONE = 0,
            REGISTER = 1,
            REQUEST = 2,
            MASTER = 3
        };
    }

    namespace Activities
    {
        public enum Activity
        {
            REGISTER = 12000,
            LOGIN = 14000,
            REGISTER_TOKEN = 15000,
            REQUEST_CARD = 16000
        };
    }
}
