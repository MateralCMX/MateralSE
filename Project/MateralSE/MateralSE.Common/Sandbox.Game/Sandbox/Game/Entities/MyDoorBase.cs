namespace Sandbox.Game.Entities
{
    using Havok;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Network;
    using VRage.Sync;
    using VRage.Utils;

    public abstract class MyDoorBase : MyFunctionalBlock
    {
        protected readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_open;

        public MyDoorBase()
        {
            this.CreateTerminalControls();
        }

        protected void CreateSubpartConstraint(VRage.Game.Entity.MyEntity subpart, out HkFixedConstraintData constraintData, out HkConstraint constraint)
        {
            constraintData = null;
            constraint = null;
            if (base.CubeGrid.Physics != null)
            {
                HkRigidBody rigidBody;
                uint info = HkGroupFilter.CalcFilterInfo(subpart.GetPhysicsBody().RigidBody.Layer, base.CubeGrid.GetPhysicsBody().HavokCollisionSystemID, 1, 1);
                subpart.Physics.RigidBody.SetCollisionFilterInfo(info);
                subpart.Physics.Enabled = true;
                constraintData = new HkFixedConstraintData();
                constraintData.SetSolvingMethod(HkSolvingMethod.MethodStabilized);
                constraintData.SetInertiaStabilizationFactor(1f);
                if ((base.CubeGrid.Physics.RigidBody2 == null) || !base.CubeGrid.Physics.Flags.HasFlag(RigidBodyFlag.RBF_DOUBLED_KINEMATIC))
                {
                    rigidBody = base.CubeGrid.Physics.RigidBody;
                }
                else
                {
                    rigidBody = base.CubeGrid.Physics.RigidBody2;
                }
                constraint = new HkConstraint(rigidBody, subpart.Physics.RigidBody, constraintData);
                constraint.WantRuntime = true;
            }
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyDoorBase>())
            {
                base.CreateTerminalControls();
                MyStringId tooltip = new MyStringId();
                MyTerminalControlOnOffSwitch<MyDoorBase> switch1 = new MyTerminalControlOnOffSwitch<MyDoorBase>("Open", MySpaceTexts.Blank, tooltip, new MyStringId?(MySpaceTexts.BlockAction_DoorOpen), new MyStringId?(MySpaceTexts.BlockAction_DoorClosed));
                MyTerminalControlOnOffSwitch<MyDoorBase> switch2 = new MyTerminalControlOnOffSwitch<MyDoorBase>("Open", MySpaceTexts.Blank, tooltip, new MyStringId?(MySpaceTexts.BlockAction_DoorOpen), new MyStringId?(MySpaceTexts.BlockAction_DoorClosed));
                switch2.Getter = x => x.Open;
                MyTerminalControlOnOffSwitch<MyDoorBase> local4 = switch2;
                MyTerminalControlOnOffSwitch<MyDoorBase> local5 = switch2;
                local5.Setter = (x, v) => x.SetOpenRequest(v, x.OwnerId);
                MyTerminalControlOnOffSwitch<MyDoorBase> onOff = local5;
                onOff.EnableToggleAction<MyDoorBase>();
                onOff.EnableOnOffActions<MyDoorBase>();
                MyTerminalControlFactory.AddControl<MyDoorBase>(onOff);
            }
        }

        protected void DisposeSubpartConstraint(ref HkConstraint constraint, ref HkFixedConstraintData constraintData)
        {
            if (constraint != null)
            {
                base.CubeGrid.Physics.RemoveConstraint(constraint);
                constraint.Dispose();
                constraint = null;
                constraintData = null;
            }
        }

        [Event(null, 0x42), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void OpenRequest(bool open, long identityId)
        {
            AdminSettingsEnum enum2;
            MyPlayer playerFromCharacter;
            MyRelationsBetweenPlayerAndBlock userRelationToOwner = base.GetUserRelationToOwner(identityId);
            MyIdentity identity = MySession.Static.Players.TryGetIdentity(identityId);
            if ((identity == null) || (identity.Character == null))
            {
                playerFromCharacter = null;
            }
            else
            {
                playerFromCharacter = MyPlayer.GetPlayerFromCharacter(identity.Character);
            }
            MyPlayer player = playerFromCharacter;
            bool flag = false;
            if (((player != null) && !userRelationToOwner.IsFriendly()) && MySession.Static.RemoteAdminSettings.TryGetValue(player.Client.SteamUserId, out enum2))
            {
                flag = enum2.HasFlag(AdminSettingsEnum.UseTerminals);
            }
            if (userRelationToOwner.IsFriendly() | flag)
            {
                this.Open = open;
            }
        }

        public void SetOpenRequest(bool open, long identityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyDoorBase, bool, long>(this, x => new Action<bool, long>(x.OpenRequest), open, identityId, targetEndpoint);
        }

        protected static void SetupDoorSubpart(MyEntitySubpart subpart, int havokCollisionSystemID, bool refreshInPlace)
        {
            if (((subpart != null) && ((subpart.Physics != null) && (subpart.ModelCollision.HavokCollisionShapes != null))) && (subpart.ModelCollision.HavokCollisionShapes.Length != 0))
            {
                uint info = HkGroupFilter.CalcFilterInfo(subpart.GetPhysicsBody().RigidBody.Layer, havokCollisionSystemID, 1, 1);
                subpart.Physics.RigidBody.SetCollisionFilterInfo(info);
                if ((subpart.GetPhysicsBody().HavokWorld != null) & refreshInPlace)
                {
                    subpart.GetPhysicsBody().HavokWorld.RefreshCollisionFilterOnEntity(subpart.Physics.RigidBody);
                }
            }
        }

        public bool Open
        {
            get => 
                ((bool) this.m_open);
            set
            {
                if (((this.m_open != value) && base.Enabled) && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
                {
                    this.m_open.Value = value;
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyDoorBase.<>c <>9 = new MyDoorBase.<>c();
            public static MyTerminalValueControl<MyDoorBase, bool>.GetterDelegate <>9__5_0;
            public static MyTerminalValueControl<MyDoorBase, bool>.SetterDelegate <>9__5_1;
            public static Func<MyDoorBase, Action<bool, long>> <>9__6_0;

            internal bool <CreateTerminalControls>b__5_0(MyDoorBase x) => 
                x.Open;

            internal void <CreateTerminalControls>b__5_1(MyDoorBase x, bool v)
            {
                x.SetOpenRequest(v, x.OwnerId);
            }

            internal Action<bool, long> <SetOpenRequest>b__6_0(MyDoorBase x) => 
                new Action<bool, long>(x.OpenRequest);
        }
    }
}

