namespace VRage.Network
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyEventContext
    {
        [ThreadStatic]
        private static MyEventContext m_current;
        public readonly EndpointId Sender;
        public readonly MyClientStateBase ClientState;
        public static MyEventContext Current =>
            m_current;
        public static void ValidationFailed()
        {
            m_current.HasValidationFailed = true;
        }

        public bool IsLocallyInvoked { get; private set; }
        public bool HasValidationFailed { get; private set; }
        public bool IsValid { get; private set; }
        private MyEventContext(EndpointId sender, MyClientStateBase clientState, bool isInvokedLocally)
        {
            this = new MyEventContext();
            this.Sender = sender;
            this.ClientState = clientState;
            this.IsLocallyInvoked = isInvokedLocally;
            this.HasValidationFailed = false;
            this.IsValid = true;
        }

        public static Token Set(EndpointId endpoint, MyClientStateBase client, bool isInvokedLocally) => 
            new Token(new MyEventContext(endpoint, client, isInvokedLocally));
        [StructLayout(LayoutKind.Sequential)]
        public struct Token : IDisposable
        {
            private readonly MyEventContext m_oldContext;
            public Token(MyEventContext newContext)
            {
                this.m_oldContext = MyEventContext.m_current;
                MyEventContext.m_current = newContext;
            }

            void IDisposable.Dispose()
            {
                MyEventContext.m_current = this.m_oldContext;
            }
        }
    }
}

