namespace Sandbox.Game.World
{
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Game.ModAPI;

    public class MyControllerInfo : IMyControllerInfo
    {
        private MyEntityController m_controller;
        [CompilerGenerated]
        private Action<MyEntityController> ControlAcquired;
        [CompilerGenerated]
        private Action<MyEntityController> ControlReleased;

        public event Action<MyEntityController> ControlAcquired
        {
            [CompilerGenerated] add
            {
                Action<MyEntityController> controlAcquired = this.ControlAcquired;
                while (true)
                {
                    Action<MyEntityController> a = controlAcquired;
                    Action<MyEntityController> action3 = (Action<MyEntityController>) Delegate.Combine(a, value);
                    controlAcquired = Interlocked.CompareExchange<Action<MyEntityController>>(ref this.ControlAcquired, action3, a);
                    if (ReferenceEquals(controlAcquired, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntityController> controlAcquired = this.ControlAcquired;
                while (true)
                {
                    Action<MyEntityController> source = controlAcquired;
                    Action<MyEntityController> action3 = (Action<MyEntityController>) Delegate.Remove(source, value);
                    controlAcquired = Interlocked.CompareExchange<Action<MyEntityController>>(ref this.ControlAcquired, action3, source);
                    if (ReferenceEquals(controlAcquired, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyEntityController> ControlReleased
        {
            [CompilerGenerated] add
            {
                Action<MyEntityController> controlReleased = this.ControlReleased;
                while (true)
                {
                    Action<MyEntityController> a = controlReleased;
                    Action<MyEntityController> action3 = (Action<MyEntityController>) Delegate.Combine(a, value);
                    controlReleased = Interlocked.CompareExchange<Action<MyEntityController>>(ref this.ControlReleased, action3, a);
                    if (ReferenceEquals(controlReleased, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntityController> controlReleased = this.ControlReleased;
                while (true)
                {
                    Action<MyEntityController> source = controlReleased;
                    Action<MyEntityController> action3 = (Action<MyEntityController>) Delegate.Remove(source, value);
                    controlReleased = Interlocked.CompareExchange<Action<MyEntityController>>(ref this.ControlReleased, action3, source);
                    if (ReferenceEquals(controlReleased, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action<IMyEntityController> IMyControllerInfo.ControlAcquired
        {
            add
            {
                this.ControlAcquired += this.GetDelegate(value);
            }
            remove
            {
                this.ControlAcquired -= this.GetDelegate(value);
            }
        }

        event Action<IMyEntityController> IMyControllerInfo.ControlReleased
        {
            add
            {
                this.ControlReleased += this.GetDelegate(value);
            }
            remove
            {
                this.ControlReleased -= this.GetDelegate(value);
            }
        }

        public void AcquireControls()
        {
            if (this.m_controller != null)
            {
                Action<MyEntityController> controlAcquired = this.ControlAcquired;
                if (controlAcquired != null)
                {
                    controlAcquired(this.m_controller);
                }
            }
        }

        private Action<MyEntityController> GetDelegate(Action<IMyEntityController> value) => 
            ((Action<MyEntityController>) Delegate.CreateDelegate(typeof(Action<MyEntityController>), value.Target, value.Method));

        public bool IsLocallyControlled() => 
            ((this.Controller != null) && ((Sync.Clients != null) && ReferenceEquals(this.Controller.Player.Client, Sync.Clients.LocalClient)));

        public bool IsLocallyHumanControlled() => 
            ((this.Controller != null) && ReferenceEquals(this.Controller.Player, Sync.Clients.LocalClient.FirstPlayer));

        public bool IsRemotelyControlled() => 
            ((this.Controller != null) && !ReferenceEquals(this.Controller.Player.Client, Sync.Clients.LocalClient));

        public void ReleaseControls()
        {
            if (this.m_controller != null)
            {
                Action<MyEntityController> controlReleased = this.ControlReleased;
                if (controlReleased != null)
                {
                    controlReleased(this.m_controller);
                }
            }
        }

        bool IMyControllerInfo.IsLocallyControlled() => 
            this.IsLocallyControlled();

        bool IMyControllerInfo.IsLocallyHumanControlled() => 
            this.IsLocallyHumanControlled();

        bool IMyControllerInfo.IsRemotelyControlled() => 
            this.IsRemotelyControlled();

        public MyEntityController Controller
        {
            get => 
                this.m_controller;
            set
            {
                if (!ReferenceEquals(this.m_controller, value))
                {
                    this.ReleaseControls();
                    this.m_controller = value;
                    this.AcquireControls();
                }
            }
        }

        public long ControllingIdentityId =>
            ((this.Controller != null) ? this.Controller.Player.Identity.IdentityId : 0L);

        IMyEntityController IMyControllerInfo.Controller =>
            this.Controller;

        long IMyControllerInfo.ControllingIdentityId =>
            this.ControllingIdentityId;
    }
}

