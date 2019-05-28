namespace Sandbox.Game.Entities
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Gui;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRageMath;

    public class MyDataBroadcaster : MyEntityComponentBase, IMyEventProxy, IMyEventOwner
    {
        public bool CanBeUsedByPlayer(long playerId) => 
            CanBeUsedByPlayer(playerId, base.Entity);

        public static bool CanBeUsedByPlayer(long playerId, IMyEntity Entity)
        {
            MyIDModule module;
            IMyComponentOwner<MyIDModule> owner = Entity as IMyComponentOwner<MyIDModule>;
            if ((owner == null) || !owner.GetComponent(out module))
            {
                return true;
            }
            MyRelationsBetweenPlayerAndBlock userRelationToOwner = module.GetUserRelationToOwner(playerId);
            return ((userRelationToOwner != MyRelationsBetweenPlayerAndBlock.NoOwnership) && ((userRelationToOwner - 3) > MyRelationsBetweenPlayerAndBlock.Owner));
        }

        public List<MyHudEntityParams> GetHudParams(bool allowBlink) => 
            ((VRage.Game.Entity.MyEntity) base.Entity).GetHudParams(allowBlink);

        private static MyTerminalBlock GetRemoteConrolForGrid(MyCubeGrid cubeGrid)
        {
            if (cubeGrid.HasMainRemoteControl())
            {
                return cubeGrid.MainRemoteControl;
            }
            MyFatBlockReader<MyRemoteControl> fatBlocks = cubeGrid.GetFatBlocks<MyRemoteControl>();
            if (!fatBlocks.MoveNext())
            {
                return null;
            }
            MyRemoteControl current = fatBlocks.Current;
            return (fatBlocks.MoveNext() ? null : current);
        }

        private MyOwnershipShareModeEnum GetShare()
        {
            MyIDModule module = this.TryGetEntityIdModule();
            return ((module != null) ? module.ShareMode : MyOwnershipShareModeEnum.None);
        }

        public virtual void InitProxyObjectBuilder(MyObjectBuilder_ProxyAntenna ob)
        {
            ob.HasReceiver = this.Receiver != null;
            ob.IsCharacter = base.Entity is MyCharacter;
            ob.Position = this.BroadcastPosition;
            ob.HudParams = new List<MyObjectBuilder_HudEntityParams>();
            foreach (MyHudEntityParams @params in this.GetHudParams(false))
            {
                ob.HudParams.Add(@params.GetObjectBuilder());
            }
            ob.InfoEntityId = this.Info.EntityId;
            ob.InfoName = this.Info.Name;
            ob.Owner = this.Owner;
            ob.Share = this.GetShare();
            ob.AntennaEntityId = base.Entity.EntityId;
            ob.PersistentFlags |= MyPersistentEntityFlags2.InScene | MyPersistentEntityFlags2.Enabled;
            ob.HasRemote = this.HasRemoteControl;
            ob.MainRemoteOwner = this.MainRemoteControlOwner;
            ob.MainRemoteId = this.MainRemoteControlId;
            ob.MainRemoteSharing = this.MainRemoteControlSharing;
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            if (Sync.IsServer)
            {
                MyCubeBlock entity = base.Entity as MyCubeBlock;
                if (entity != null)
                {
                    entity.CubeGrid.OnNameChanged += new Action<MyCubeGrid>(this.RaiseNameChanged);
                    MyTerminalBlock block2 = entity as MyTerminalBlock;
                    if (block2 != null)
                    {
                        block2.CustomNameChanged += new Action<MyTerminalBlock>(this.RaiseAntennaNameChanged);
                    }
                }
            }
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            if (Sync.IsServer)
            {
                MyCubeBlock entity = base.Entity as MyCubeBlock;
                if (entity != null)
                {
                    entity.CubeGrid.OnNameChanged -= new Action<MyCubeGrid>(this.RaiseNameChanged);
                    MyTerminalBlock block2 = entity as MyTerminalBlock;
                    if (block2 != null)
                    {
                        block2.CustomNameChanged -= new Action<MyTerminalBlock>(this.RaiseAntennaNameChanged);
                    }
                }
            }
        }

        [Event(null, 0x16c), Reliable, Broadcast]
        private void OnNameChanged(string newName)
        {
            MyProxyAntenna entity = base.Entity as MyProxyAntenna;
            if (entity != null)
            {
                MyAntennaSystem.BroadcasterInfo info = new MyAntennaSystem.BroadcasterInfo {
                    EntityId = entity.Info.EntityId,
                    Name = newName
                };
                entity.Info = info;
            }
        }

        [Event(null, 0x162), Reliable, Broadcast]
        private void OnOwnerChanged(long newOwner, MyOwnershipShareModeEnum newShare)
        {
            MyProxyAntenna entity = base.Entity as MyProxyAntenna;
            if (entity != null)
            {
                entity.ChangeOwner(newOwner, newShare);
            }
        }

        [Event(null, 0x185), Reliable, Broadcast]
        private void OnUpdateHudParams(List<MyObjectBuilder_HudEntityParams> newHudParams)
        {
            MyProxyAntenna entity = base.Entity as MyProxyAntenna;
            if (entity != null)
            {
                entity.ChangeHudParams(newHudParams);
            }
        }

        public void RaiseAntennaNameChanged(MyTerminalBlock block)
        {
            this.UpdateHudParams(block);
        }

        public void RaiseNameChanged(MyCubeGrid grid)
        {
            if (Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyDataBroadcaster, string>(this, x => new Action<string>(x.OnNameChanged), this.Info.Name, targetEndpoint);
            }
        }

        public void RaiseOwnerChanged()
        {
            if (Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyDataBroadcaster, long, MyOwnershipShareModeEnum>(this, x => new Action<long, MyOwnershipShareModeEnum>(x.OnOwnerChanged), this.Owner, this.GetShare(), targetEndpoint);
                this.UpdateHudParams(base.Entity as VRage.Game.Entity.MyEntity);
            }
        }

        private MyIDModule TryGetEntityIdModule()
        {
            MyIDModule module;
            IMyComponentOwner<MyIDModule> entity = base.Entity as IMyComponentOwner<MyIDModule>;
            if ((entity == null) || !entity.GetComponent(out module))
            {
                return null;
            }
            return module;
        }

        private MyCubeGrid TryGetHostingGrid()
        {
            MyCubeBlock entity = base.Entity as MyCubeBlock;
            return entity?.CubeGrid;
        }

        public void UpdateHudParams(VRage.Game.Entity.MyEntity entity)
        {
            if (Sync.IsServer)
            {
                List<MyObjectBuilder_HudEntityParams> list = new List<MyObjectBuilder_HudEntityParams>();
                foreach (MyHudEntityParams @params in entity.GetHudParams(false))
                {
                    list.Add(@params.GetObjectBuilder());
                }
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyDataBroadcaster, List<MyObjectBuilder_HudEntityParams>>(this, x => new Action<List<MyObjectBuilder_HudEntityParams>>(x.OnUpdateHudParams), list, targetEndpoint);
            }
        }

        public void UpdateRemoteControlInfo()
        {
            if ((Sync.IsServer && (base.Entity != null)) && ((VRage.Game.Entity.MyEntity) base.Entity).IsReadyForReplication)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyDataBroadcaster, bool, long?, MyOwnershipShareModeEnum, long?>(this, x => new Action<bool, long?, MyOwnershipShareModeEnum, long?>(x.UpdateRemoteControlState), this.HasRemoteControl, this.MainRemoteControlOwner, this.MainRemoteControlSharing, this.MainRemoteControlId, targetEndpoint);
            }
        }

        [Event(null, 0x176), Reliable, Broadcast]
        private void UpdateRemoteControlState(bool hasRemote, long? owner, MyOwnershipShareModeEnum sharing, long? remoteId)
        {
            MyProxyAntenna entity = base.Entity as MyProxyAntenna;
            if (entity != null)
            {
                entity.HasRemoteControl = hasRemote;
                entity.MainRemoteControlOwner = owner;
                entity.MainRemoteControlId = remoteId;
                entity.MainRemoteControlSharing = sharing;
            }
        }

        public Vector3D BroadcastPosition =>
            base.Entity.PositionComp.GetPosition();

        public override string ComponentTypeDebugString =>
            "MyDataBroadcaster";

        public MyDataReceiver Receiver
        {
            get
            {
                MyDataReceiver component = null;
                if (base.Container != null)
                {
                    base.Container.TryGet<MyDataReceiver>(out component);
                }
                return component;
            }
        }

        public bool Closed =>
            ((base.Entity == null) || (base.Entity.MarkedForClose || base.Entity.Closed));

        public long Owner
        {
            get
            {
                MyIDModule module = this.TryGetEntityIdModule();
                return ((module != null) ? module.Owner : 0L);
            }
        }

        public virtual bool ShowOnHud =>
            true;

        public bool ShowInTerminal =>
            (!(base.Entity is MyCharacter) ? (!(base.Entity is MyCubeBlock) ? ((base.Entity is MyProxyAntenna) && !(base.Entity as MyProxyAntenna).IsCharacter) : true) : false);

        public MyAntennaSystem.BroadcasterInfo Info
        {
            get
            {
                if (base.Entity is MyCharacter)
                {
                    return new MyAntennaSystem.BroadcasterInfo { 
                        EntityId = base.Entity.EntityId,
                        Name = base.Entity.DisplayName
                    };
                }
                if (base.Entity is MyCubeBlock)
                {
                    MyCubeGrid logicalGroupRepresentative = MyAntennaSystem.Static.GetLogicalGroupRepresentative((base.Entity as MyCubeBlock).CubeGrid);
                    return new MyAntennaSystem.BroadcasterInfo { 
                        EntityId = logicalGroupRepresentative.EntityId,
                        Name = logicalGroupRepresentative.DisplayName
                    };
                }
                if (base.Entity is MyProxyAntenna)
                {
                    return (base.Entity as MyProxyAntenna).Info;
                }
                return new MyAntennaSystem.BroadcasterInfo();
            }
        }

        public long AntennaEntityId
        {
            get
            {
                MyProxyAntenna entity = base.Entity as MyProxyAntenna;
                return ((entity == null) ? base.Entity.EntityId : entity.AntennaEntityId);
            }
        }

        public bool HasRemoteControl
        {
            get
            {
                if (base.Entity is MyCharacter)
                {
                    return false;
                }
                MyCubeGrid grid = this.TryGetHostingGrid();
                return ((grid == null) ? ((base.Entity is MyProxyAntenna) && (base.Entity as MyProxyAntenna).HasRemoteControl) : (grid.GetFatBlockCount<MyRemoteControl>() > 0));
            }
        }

        public long? MainRemoteControlOwner
        {
            get
            {
                if (!(base.Entity is MyCharacter))
                {
                    MyCubeGrid cubeGrid = this.TryGetHostingGrid();
                    if (cubeGrid == null)
                    {
                        if (base.Entity is MyProxyAntenna)
                        {
                            return (base.Entity as MyProxyAntenna).MainRemoteControlOwner;
                        }
                        return null;
                    }
                    MyTerminalBlock remoteConrolForGrid = GetRemoteConrolForGrid(cubeGrid);
                    if (remoteConrolForGrid != null)
                    {
                        return new long?(remoteConrolForGrid.OwnerId);
                    }
                }
                return null;
            }
        }

        public long? MainRemoteControlId
        {
            get
            {
                if (!(base.Entity is MyCharacter))
                {
                    MyCubeGrid cubeGrid = this.TryGetHostingGrid();
                    if (cubeGrid == null)
                    {
                        if (base.Entity is MyProxyAntenna)
                        {
                            return (base.Entity as MyProxyAntenna).MainRemoteControlId;
                        }
                        return null;
                    }
                    MyTerminalBlock remoteConrolForGrid = GetRemoteConrolForGrid(cubeGrid);
                    if (remoteConrolForGrid != null)
                    {
                        return new long?(remoteConrolForGrid.EntityId);
                    }
                }
                return null;
            }
        }

        public MyOwnershipShareModeEnum MainRemoteControlSharing
        {
            get
            {
                if (base.Entity is MyCharacter)
                {
                    return MyOwnershipShareModeEnum.None;
                }
                MyCubeGrid cubeGrid = this.TryGetHostingGrid();
                if (cubeGrid == null)
                {
                    return (!(base.Entity is MyProxyAntenna) ? MyOwnershipShareModeEnum.None : (base.Entity as MyProxyAntenna).MainRemoteControlSharing);
                }
                MyTerminalBlock remoteConrolForGrid = GetRemoteConrolForGrid(cubeGrid);
                return ((remoteConrolForGrid != null) ? remoteConrolForGrid.IDModule.ShareMode : MyOwnershipShareModeEnum.None);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyDataBroadcaster.<>c <>9 = new MyDataBroadcaster.<>c();
            public static Func<MyDataBroadcaster, Action<long, MyOwnershipShareModeEnum>> <>9__32_0;
            public static Func<MyDataBroadcaster, Action<string>> <>9__33_0;
            public static Func<MyDataBroadcaster, Action<bool, long?, MyOwnershipShareModeEnum, long?>> <>9__35_0;
            public static Func<MyDataBroadcaster, Action<List<MyObjectBuilder_HudEntityParams>>> <>9__36_0;

            internal Action<string> <RaiseNameChanged>b__33_0(MyDataBroadcaster x) => 
                new Action<string>(x.OnNameChanged);

            internal Action<long, MyOwnershipShareModeEnum> <RaiseOwnerChanged>b__32_0(MyDataBroadcaster x) => 
                new Action<long, MyOwnershipShareModeEnum>(x.OnOwnerChanged);

            internal Action<List<MyObjectBuilder_HudEntityParams>> <UpdateHudParams>b__36_0(MyDataBroadcaster x) => 
                new Action<List<MyObjectBuilder_HudEntityParams>>(x.OnUpdateHudParams);

            internal Action<bool, long?, MyOwnershipShareModeEnum, long?> <UpdateRemoteControlInfo>b__35_0(MyDataBroadcaster x) => 
                new Action<bool, long?, MyOwnershipShareModeEnum, long?>(x.UpdateRemoteControlState);
        }
    }
}

