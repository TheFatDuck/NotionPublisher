using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Common.Exceptions
{
    public class MissingRequiredFieldException : MissingFieldException
    {
        public int GetErrorCode() { return ExceptionCodes.MissingRequiredField; }
        public MissingRequiredFieldException()
        {
        }

        public MissingRequiredFieldException(string message)
            : base(message)
        {
        }

        public MissingRequiredFieldException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
