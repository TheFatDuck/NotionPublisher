using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Common.Exceptions
{
    public class ExceptionCodes
    {
        public const int ServiceInitializeFailed = 90000;
        public const int DBInitializeFailed = 90001;
        public const int MissingRequiredField = 90002;
        public const int DataNotFound = 90003;
    }
}
