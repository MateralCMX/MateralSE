namespace Sandbox.Game.GameSystems.IntergridCommunication
{
    using Sandbox.ModAPI.Ingame;
    using System;

    internal class UnicastListener : MyMessageListener, IMyUnicastListener, IMyMessageProvider
    {
        public UnicastListener(MyIntergridCommunicationContext context) : base(context)
        {
        }
    }
}

