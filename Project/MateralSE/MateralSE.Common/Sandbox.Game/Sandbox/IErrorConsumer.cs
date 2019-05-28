namespace Sandbox
{
    using System;

    public interface IErrorConsumer
    {
        void OnError(string header, string message, string callstack);
    }
}

