namespace Sandbox.Game.GameSystems.IntergridCommunication
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.SessionComponents;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    internal class MyMessageListener : IMyMessageProvider
    {
        private string m_callback;
        public bool m_hasPendingCallback;
        private Queue<MyIGCMessage> m_pendingMessages;
        private static Action<MyProgrammableBlock, string> m_invokeOverride;

        public MyMessageListener(MyIntergridCommunicationContext context)
        {
            this.Context = context;
        }

        public virtual MyIGCMessage AcceptMessage()
        {
            if (this.HasPendingMessage)
            {
                return this.m_pendingMessages.Dequeue();
            }
            return new MyIGCMessage();
        }

        public void DisableMessageCallback()
        {
            this.UnregisterCallback();
            this.m_callback = null;
        }

        public void EnqueueMessage(MyIGCMessage message)
        {
            if (this.m_pendingMessages == null)
            {
                this.m_pendingMessages = new Queue<MyIGCMessage>();
            }
            else if (this.m_pendingMessages.Count >= this.MaxWaitingMessages)
            {
                this.m_pendingMessages.Dequeue();
            }
            this.m_pendingMessages.Enqueue(message);
            this.RegisterForCallback();
            if (MyDebugDrawSettings.DEBUG_DRAW_IGC)
            {
                Vector3D to = this.Context.ProgrammableBlock.WorldMatrix.Translation;
                Vector3D from = MyEntities.GetEntityById(message.Source, false).WorldMatrix.Translation;
                Color color = (this is IMyBroadcastListener) ? Color.Blue : Color.Green;
                MyIGCSystemSessionComponent.Static.AddDebugDraw(delegate {
                    Color? colorTo = null;
                    MyRenderProxy.DebugDrawArrow3D(from, to, color, colorTo, false, 0.1, null, 0.5f, false);
                });
            }
        }

        public void InvokeCallback()
        {
            this.UnregisterCallback();
            MyProgrammableBlock programmableBlock = this.Context.ProgrammableBlock;
            if (m_invokeOverride != null)
            {
                m_invokeOverride(programmableBlock, this.m_callback);
            }
            else
            {
                programmableBlock.Run(this.m_callback, UpdateType.IGC);
            }
        }

        private void RegisterForCallback()
        {
            if ((this.m_callback != null) && !this.m_hasPendingCallback)
            {
                this.m_hasPendingCallback = true;
                this.Context.RegisterForCallback(this);
            }
        }

        public virtual void SetMessageCallback(string argument)
        {
            if (argument == null)
            {
                throw new ArgumentNullException("argument");
            }
            this.m_callback = argument;
        }

        private void UnregisterCallback()
        {
            if (this.m_hasPendingCallback)
            {
                this.m_hasPendingCallback = false;
                this.Context.UnregisterFromCallback(this);
            }
        }

        public MyIntergridCommunicationContext Context { get; private set; }

        public int MaxWaitingMessages =>
            0x19;

        public bool HasPendingMessage =>
            ((this.m_pendingMessages != null) && (this.m_pendingMessages.Count > 0));
    }
}

