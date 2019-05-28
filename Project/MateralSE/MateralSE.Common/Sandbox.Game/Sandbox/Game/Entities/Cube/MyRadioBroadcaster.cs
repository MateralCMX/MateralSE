namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Network;

    public class MyRadioBroadcaster : MyDataBroadcaster
    {
        public Action OnBroadcastRadiusChanged;
        private float m_broadcastRadius;
        private bool m_enabled;
        public bool WantsToBeEnabled = true;
        public int m_radioProxyID = -1;
        private bool m_registered;

        public MyRadioBroadcaster(float broadcastRadius = 100f)
        {
            this.m_broadcastRadius = broadcastRadius;
        }

        [Event(null, 0x76), Reliable, Broadcast]
        public void ChangeBroadcastRadius(float newRadius)
        {
            this.BroadcastRadius = newRadius;
        }

        public override void InitProxyObjectBuilder(MyObjectBuilder_ProxyAntenna ob)
        {
            base.InitProxyObjectBuilder(ob);
            ob.IsLaser = false;
            ob.BroadcastRadius = this.BroadcastRadius;
        }

        private bool IsProjection()
        {
            MyCubeBlock entity = base.Entity as MyCubeBlock;
            return ((entity != null) && ReferenceEquals(entity.CubeGrid.Physics, null));
        }

        public void MoveBroadcaster()
        {
            MyRadioBroadcasters.MoveBroadcaster(this);
        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
            if (base.Entity.GetTopMostParent(null).Physics != null)
            {
                this.m_registered = true;
                MyAntennaSystem.Static.RegisterAntenna(this);
            }
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            this.Enabled = false;
        }

        public override void OnRemovedFromScene()
        {
            base.OnRemovedFromScene();
            if (this.m_registered)
            {
                MyAntennaSystem.Static.UnregisterAntenna(this);
            }
            this.m_registered = false;
        }

        public void RaiseBroadcastRadiusChanged()
        {
            if (Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyRadioBroadcaster, float>(this, x => new Action<float>(x.ChangeBroadcastRadius), this.BroadcastRadius, targetEndpoint);
            }
        }

        public bool Enabled
        {
            get => 
                this.m_enabled;
            set
            {
                if (this.m_enabled != value)
                {
                    if (!this.IsProjection())
                    {
                        if (value)
                        {
                            MyRadioBroadcasters.AddBroadcaster(this);
                        }
                        else
                        {
                            MyRadioBroadcasters.RemoveBroadcaster(this);
                        }
                    }
                    this.m_enabled = value;
                }
            }
        }

        public float BroadcastRadius
        {
            get => 
                this.m_broadcastRadius;
            set
            {
                if (this.m_broadcastRadius != value)
                {
                    this.m_broadcastRadius = value;
                    if (this.m_enabled)
                    {
                        MyRadioBroadcasters.RemoveBroadcaster(this);
                        MyRadioBroadcasters.AddBroadcaster(this);
                    }
                    Action onBroadcastRadiusChanged = this.OnBroadcastRadiusChanged;
                    if (onBroadcastRadiusChanged != null)
                    {
                        onBroadcastRadiusChanged();
                    }
                }
            }
        }

        public int RadioProxyID
        {
            get => 
                this.m_radioProxyID;
            set => 
                (this.m_radioProxyID = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyRadioBroadcaster.<>c <>9 = new MyRadioBroadcaster.<>c();
            public static Func<MyRadioBroadcaster, Action<float>> <>9__20_0;

            internal Action<float> <RaiseBroadcastRadiusChanged>b__20_0(MyRadioBroadcaster x) => 
                new Action<float>(x.ChangeBroadcastRadius);
        }
    }
}

