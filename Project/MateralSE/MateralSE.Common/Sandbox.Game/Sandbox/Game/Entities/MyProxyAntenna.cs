namespace Sandbox.Game.Entities
{
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Gui;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;

    [MyEntityType(typeof(MyObjectBuilder_ProxyAntenna), true)]
    internal class MyProxyAntenna : VRage.Game.Entity.MyEntity, IMyComponentOwner<MyIDModule>
    {
        private Dictionary<long, MyHudEntityParams> m_savedHudParams = new Dictionary<long, MyHudEntityParams>();
        private bool m_active;
        private MyIDModule m_IDModule = new MyIDModule();
        private bool m_registered;

        public void ChangeHudParams(List<MyObjectBuilder_HudEntityParams> newHudParams)
        {
            foreach (MyObjectBuilder_HudEntityParams @params in newHudParams)
            {
                this.m_savedHudParams[@params.EntityId] = new MyHudEntityParams(@params);
            }
        }

        public void ChangeOwner(long newOwner, MyOwnershipShareModeEnum newShare)
        {
            this.m_IDModule.Owner = newOwner;
            this.m_IDModule.ShareMode = newShare;
        }

        protected override void Closing()
        {
            if (this.m_registered)
            {
                MyAntennaSystem.Static.UnregisterAntenna(this.Broadcaster);
            }
            this.m_registered = false;
            base.Closing();
        }

        public override List<MyHudEntityParams> GetHudParams(bool allowBlink)
        {
            base.m_hudParams.Clear();
            foreach (MyHudEntityParams @params in this.m_savedHudParams.Values)
            {
                MyHudEntityParams item = new MyHudEntityParams {
                    EntityId = @params.EntityId,
                    FlagsEnum = @params.FlagsEnum,
                    Owner = @params.Owner,
                    Share = @params.Share,
                    Position = base.PositionComp.GetPosition(),
                    Text = @params.Text,
                    BlinkingTime = @params.BlinkingTime
                };
                base.m_hudParams.Add(item);
            }
            return base.m_hudParams;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            MyObjectBuilder_ProxyAntenna antenna = objectBuilder as MyObjectBuilder_ProxyAntenna;
            this.AntennaEntityId = antenna.AntennaEntityId;
            base.PositionComp.SetPosition((Vector3D) antenna.Position, null, false, true);
            this.IsCharacter = antenna.IsCharacter;
            if (antenna.IsLaser)
            {
                MyLaserBroadcaster broadcaster2 = new MyLaserBroadcaster();
                this.Broadcaster = broadcaster2;
                this.SuccessfullyContacting = antenna.SuccessfullyContacting;
                broadcaster2.StateText.Clear().Append(antenna.StateText);
                this.Receiver = new MyLaserReceiver();
            }
            else
            {
                MyRadioBroadcaster broadcaster = new MyRadioBroadcaster(100f);
                this.Broadcaster = broadcaster;
                broadcaster.BroadcastRadius = antenna.BroadcastRadius;
                base.PositionComp.OnPositionChanged += new Action<MyPositionComponentBase>(this.WorldPositionChanged);
                if (antenna.HasReceiver)
                {
                    this.Receiver = new MyRadioReceiver();
                }
            }
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            this.m_IDModule.Owner = antenna.Owner;
            this.m_IDModule.ShareMode = antenna.Share;
            MyAntennaSystem.BroadcasterInfo info = new MyAntennaSystem.BroadcasterInfo {
                EntityId = antenna.InfoEntityId,
                Name = antenna.InfoName
            };
            this.Info = info;
            foreach (MyObjectBuilder_HudEntityParams @params in antenna.HudParams)
            {
                this.m_savedHudParams[@params.EntityId] = new MyHudEntityParams(@params);
            }
            this.HasRemoteControl = antenna.HasRemote;
            this.MainRemoteControlOwner = antenna.MainRemoteOwner;
            this.MainRemoteControlId = antenna.MainRemoteId;
            this.MainRemoteControlSharing = antenna.MainRemoteSharing;
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            if (this.Receiver != null)
            {
                this.Receiver.UpdateBroadcastersInRange();
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            this.m_registered = true;
            MyAntennaSystem.Static.RegisterAntenna(this.Broadcaster);
        }

        bool IMyComponentOwner<MyIDModule>.GetComponent(out MyIDModule component)
        {
            component = this.m_IDModule;
            return (this.m_IDModule != null);
        }

        private void WorldPositionChanged(object source)
        {
            MyRadioBroadcaster broadcaster = this.Broadcaster as MyRadioBroadcaster;
            if (broadcaster != null)
            {
                broadcaster.MoveBroadcaster();
            }
        }

        public bool Active
        {
            get => 
                this.m_active;
            set
            {
                if (this.m_active != value)
                {
                    this.m_active = value;
                    if (this.Receiver != null)
                    {
                        this.Receiver.Enabled = value;
                        base.NeedsUpdate = !value ? (base.NeedsUpdate & ~MyEntityUpdateEnum.EACH_10TH_FRAME) : (base.NeedsUpdate | MyEntityUpdateEnum.EACH_10TH_FRAME);
                    }
                    MyRadioBroadcaster broadcaster = this.Broadcaster as MyRadioBroadcaster;
                    if (broadcaster != null)
                    {
                        broadcaster.Enabled = this.m_active;
                    }
                    else
                    {
                        MyLaserBroadcaster laser = this.Broadcaster as MyLaserBroadcaster;
                        if (laser != null)
                        {
                            if (!this.m_active)
                            {
                                MyAntennaSystem.Static.RemoveLaser(this.AntennaEntityId, false);
                            }
                            else if (!MyAntennaSystem.Static.LaserAntennas.ContainsKey(this.AntennaEntityId))
                            {
                                MyAntennaSystem.Static.AddLaser(this.AntennaEntityId, laser, false);
                            }
                        }
                    }
                }
            }
        }

        public bool IsCharacter { get; private set; }

        public MyDataBroadcaster Broadcaster
        {
            get => 
                base.Components.Get<MyDataBroadcaster>();
            set => 
                base.Components.Add<MyDataBroadcaster>(value);
        }

        public MyDataReceiver Receiver
        {
            get => 
                base.Components.Get<MyDataReceiver>();
            set => 
                base.Components.Add<MyDataReceiver>(value);
        }

        public MyAntennaSystem.BroadcasterInfo Info { get; set; }

        public long AntennaEntityId { get; private set; }

        public long? SuccessfullyContacting { get; set; }

        public bool HasRemoteControl { get; set; }

        public long? MainRemoteControlOwner { get; set; }

        public long? MainRemoteControlId { get; set; }

        public MyOwnershipShareModeEnum MainRemoteControlSharing { get; set; }
    }
}

