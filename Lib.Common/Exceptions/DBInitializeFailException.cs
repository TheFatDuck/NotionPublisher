using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Common.Exceptions
{
    public class DBInitializeFailException : Exception
    {
        public int GetErrorCode() { return ExceptionCodes.DBInitializeFailed; }
        public DBInitializeFailException()
        {
        }

        public DBInitializeFailException(string message)
            : base(message)
        {
        }

        public DBInitializeFailException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
