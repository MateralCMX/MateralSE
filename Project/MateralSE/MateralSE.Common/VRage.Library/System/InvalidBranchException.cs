namespace System
{
    public class InvalidBranchException : Exception
    {
        public InvalidBranchException()
        {
        }

        public InvalidBranchException(string message) : base(message)
        {
        }
    }
}

