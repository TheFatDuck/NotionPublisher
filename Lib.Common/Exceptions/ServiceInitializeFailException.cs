namespace Lib.Common.Exceptions
{
    public class ServiceInitializeFailException : Exception
    {
        public int GetErrorCode() { return ExceptionCodes.ServiceInitializeFailed; }
        public ServiceInitializeFailException()
        {
        }

        public ServiceInitializeFailException(string message)
            : base(message)
        {
        }

        public ServiceInitializeFailException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}