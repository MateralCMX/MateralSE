namespace Sandbox.Game.GameSystems.IntergridCommunication
{
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Runtime.CompilerServices;

    internal class BroadcastListener : MyMessageListener, IMyBroadcastListener, IMyMessageProvider
    {
        public BroadcastListener(MyIntergridCommunicationContext context, string tag) : base(context)
        {
            this.Tag = tag;
        }

        public override MyIGCMessage AcceptMessage()
        {
            if (!this.IsActive && !base.HasPendingMessage)
            {
                base.Context.DisposeBroadcastListener(this, false);
            }
            return base.AcceptMessage();
        }

        public override void SetMessageCallback(string argument)
        {
            if (!this.IsActive)
            {
                throw new Exception("Callbacks are not supported for disabled broadcast listeners!");
            }
            base.SetMessageCallback(argument);
        }

        public string Tag { get; private set; }

        public bool IsActive { get; set; }
    }
}

