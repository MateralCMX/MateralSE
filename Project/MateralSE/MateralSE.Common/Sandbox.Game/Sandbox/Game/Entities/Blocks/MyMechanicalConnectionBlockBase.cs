namespace Sandbox.Game.Entities.Blocks
{
    using Havok;
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyMechanicalConnectionBlock), typeof(Sandbox.ModAPI.Ingame.IMyMechanicalConnectionBlock) })]
    public abstract class MyMechanicalConnectionBlockBase : MyFunctionalBlock, Sandbox.ModAPI.IMyMechanicalConnectionBlock, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyMechanicalConnectionBlock
    {
        protected readonly VRage.Sync.Sync<State, SyncDirection.FromServer> m_connectionState;
        protected VRage.Sync.Sync<bool, SyncDirection.BothWays> m_forceWeld;
        protected VRage.Sync.Sync<float, SyncDirection.BothWays> m_weldSpeed;
        private float m_weldSpeedSq;
        private float m_unweldSpeedSq;
        private VRage.Sync.Sync<bool, SyncDirection.BothWays> m_shareInertiaTensor;
        private MyAttachableTopBlockBase m_topBlock;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_safetyDetach;
        protected static List<HkBodyCollision> m_penetrations = new List<HkBodyCollision>();
        protected static HashSet<MySlimBlock> m_tmpSet = new HashSet<MySlimBlock>();
        protected HkConstraint m_constraint;
        private bool m_needReattach;
        private bool m_updateAttach;
        protected bool SupportsSharedInertiaTensor = ((MySession.Static != null) && MySession.Static.IsSettingsExperimental());
        [CompilerGenerated]
        private Action<MyMechanicalConnectionBlockBase> AttachedEntityChanged;

        protected event Action<MyMechanicalConnectionBlockBase> AttachedEntityChanged
        {
            [CompilerGenerated] add
            {
                Action<MyMechanicalConnectionBlockBase> attachedEntityChanged = this.AttachedEntityChanged;
                while (true)
                {
                    Action<MyMechanicalConnectionBlockBase> a = attachedEntityChanged;
                    Action<MyMechanicalConnectionBlockBase> action3 = (Action<MyMechanicalConnectionBlockBase>) Delegate.Combine(a, value);
                    attachedEntityChanged = Interlocked.CompareExchange<Action<MyMechanicalConnectionBlockBase>>(ref this.AttachedEntityChanged, action3, a);
                    if (ReferenceEquals(attachedEntityChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyMechanicalConnectionBlockBase> attachedEntityChanged = this.AttachedEntityChanged;
                while (true)
                {
                    Action<MyMechanicalConnectionBlockBase> source = attachedEntityChanged;
                    Action<MyMechanicalConnectionBlockBase> action3 = (Action<MyMechanicalConnectionBlockBase>) Delegate.Remove(source, value);
                    attachedEntityChanged = Interlocked.CompareExchange<Action<MyMechanicalConnectionBlockBase>>(ref this.AttachedEntityChanged, action3, source);
                    if (ReferenceEquals(attachedEntityChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyMechanicalConnectionBlockBase()
        {
            this.m_connectionState.ValueChanged += o => this.OnAttachTargetChanged();
            this.m_connectionState.Validate = new SyncValidate<State>(this.ValidateTopBlockId);
            this.CreateTerminalControls();
            this.m_updateAttach = true;
        }

        protected virtual bool Attach(MyAttachableTopBlockBase topBlock, bool updateGroup = true)
        {
            if (topBlock.CubeGrid.Physics == null)
            {
                return false;
            }
            if ((base.CubeGrid.Physics == null) || !base.CubeGrid.Physics.Enabled)
            {
                return false;
            }
            this.m_topBlock = topBlock;
            this.TopBlock.Attach(this);
            if (updateGroup)
            {
                base.CubeGrid.OnPhysicsChanged += new Action<VRage.Game.Entity.MyEntity>(this.cubeGrid_OnPhysicsChanged);
                this.TopGrid.OnPhysicsChanged += new Action<VRage.Game.Entity.MyEntity>(this.cubeGrid_OnPhysicsChanged);
                this.TopBlock.OnClosing += new Action<VRage.Game.Entity.MyEntity>(this.TopBlock_OnClosing);
                if (!ReferenceEquals(base.CubeGrid, topBlock.CubeGrid))
                {
                    this.OnConstraintAdded(GridLinkTypeEnum.Physical, this.TopGrid);
                    this.OnConstraintAdded(GridLinkTypeEnum.Logical, this.TopGrid);
                    this.OnConstraintAdded(GridLinkTypeEnum.Mechanical, this.TopGrid);
                    MyGridPhysicalHierarchy.Static.CreateLink(base.EntityId, base.CubeGrid, this.TopGrid);
                }
                this.RaiseAttachedEntityChanged();
            }
            if (Sync.IsServer)
            {
                MatrixD xd1 = this.TopBlock.CubeGrid.WorldMatrix * MatrixD.Invert(base.WorldMatrix);
                State state = new State {
                    TopBlockId = new long?(this.TopBlock.EntityId),
                    Welded = this.m_welded
                };
                this.m_connectionState.Value = state;
            }
            this.m_isAttached = true;
            return true;
        }

        private void BreakLinks(MyCubeGrid topGrid, MyAttachableTopBlockBase topBlock)
        {
            if (!ReferenceEquals(base.CubeGrid, this.TopGrid))
            {
                this.OnConstraintRemoved(GridLinkTypeEnum.Physical, topGrid);
                this.OnConstraintRemoved(GridLinkTypeEnum.Logical, topGrid);
                this.OnConstraintRemoved(GridLinkTypeEnum.Mechanical, topGrid);
                MyGridPhysicalHierarchy.Static.BreakLink(base.EntityId, base.CubeGrid, topGrid);
            }
            if (base.CubeGrid != null)
            {
                base.CubeGrid.OnPhysicsChanged -= new Action<VRage.Game.Entity.MyEntity>(this.cubeGrid_OnPhysicsChanged);
            }
            if (topGrid != null)
            {
                topGrid.OnPhysicsChanged -= new Action<VRage.Game.Entity.MyEntity>(this.cubeGrid_OnPhysicsChanged);
            }
            if (topBlock != null)
            {
                topBlock.OnClosing -= new Action<VRage.Game.Entity.MyEntity>(this.TopBlock_OnClosing);
            }
        }

        protected void CallAttach()
        {
            if (!this.m_isAttached)
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyMechanicalConnectionBlockBase>(this, x => new Action(x.FindAndAttachTopServer), targetEndpoint);
            }
        }

        protected void CallDetach()
        {
            if (this.m_isAttached)
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyMechanicalConnectionBlockBase>(this, x => new Action(x.DetachRequest), targetEndpoint);
            }
        }

        private bool CanAttach(MyAttachableTopBlockBase top)
        {
            int num1;
            int num2;
            if (base.MarkedForClose || base.CubeGrid.MarkedForClose)
            {
                return false;
            }
            if (top.MarkedForClose || top.CubeGrid.MarkedForClose)
            {
                return false;
            }
            if ((base.CubeGrid.Physics == null) || (base.CubeGrid.Physics.RigidBody == null))
            {
                num1 = 1;
            }
            else
            {
                num1 = (int) !base.CubeGrid.Physics.RigidBody.InWorld;
            }
            if (num1 != 0)
            {
                return false;
            }
            if ((top.CubeGrid.Physics == null) || (top.CubeGrid.Physics.RigidBody == null))
            {
                num2 = 1;
            }
            else
            {
                num2 = (int) !top.CubeGrid.Physics.RigidBody.InWorld;
            }
            return ((num2 == 0) ? ReferenceEquals(top.CubeGrid.Physics.HavokWorld, base.CubeGrid.Physics.HavokWorld) : false);
        }

        protected virtual bool CanPlaceRotor(MyAttachableTopBlockBase rotorBlock, long builtBy) => 
            true;

        private bool CanPlaceTop()
        {
            Quaternion quaternion;
            Vector3D vectord;
            Vector3 vector;
            this.ComputeTopQueryBox(out vectord, out vector, out quaternion);
            using (MyUtils.ReuseCollection<HkBodyCollision>(ref m_penetrations))
            {
                MyPhysics.GetPenetrationsBox(ref vector, ref vectord, ref quaternion, m_penetrations, 15);
                using (List<HkBodyCollision>.Enumerator enumerator = m_penetrations.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        VRage.ModAPI.IMyEntity collisionEntity = enumerator.Current.GetCollisionEntity();
                        if ((collisionEntity != null) && !ReferenceEquals(collisionEntity, base.CubeGrid))
                        {
                            MyCubeGrid grid = collisionEntity as MyCubeGrid;
                            if (grid != null)
                            {
                                Vector3D worldStart = this.TransformPosition(ref vectord);
                                Vector3I? nullable = grid.RayCastBlocks(worldStart, worldStart + base.WorldMatrix.Up);
                                if (nullable != null)
                                {
                                    return ReferenceEquals(grid.GetCubeBlock(nullable.Value), null);
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        protected virtual bool CanPlaceTop(MyAttachableTopBlockBase topBlock, long builtBy) => 
            true;

        protected void CheckSafetyDetach()
        {
            if (Sync.IsServer && (this.m_constraint != null))
            {
                if (!base.CubeGrid.Physics.IsActive)
                {
                    MyGridPhysics physics = this.TopGrid.Physics;
                    if ((physics != null) && !physics.IsActive)
                    {
                        return;
                    }
                }
                float safetyDetach = this.SafetyDetach;
                if (this.GetConstraintDisplacementSq() > (safetyDetach * safetyDetach))
                {
                    this.Detach(true);
                }
            }
        }

        public abstract void ComputeTopQueryBox(out Vector3D pos, out Vector3 halfExtents, out Quaternion orientation);
        protected virtual bool CreateConstraint(MyAttachableTopBlockBase top)
        {
            if ((!this.CanAttach(top) || this.m_welded) || (base.CubeGrid.Physics.RigidBody == top.CubeGrid.Physics.RigidBody))
            {
                return false;
            }
            this.UpdateSharedTensorState();
            return true;
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyMechanicalConnectionBlockBase>())
            {
                base.CreateTerminalControls();
                MyTerminalControlSlider<MyMechanicalConnectionBlockBase> slider3 = new MyTerminalControlSlider<MyMechanicalConnectionBlockBase>("Weld speed", MySpaceTexts.BlockPropertyTitle_WeldSpeed, MySpaceTexts.Blank);
                MyTerminalControlSlider<MyMechanicalConnectionBlockBase> slider4 = new MyTerminalControlSlider<MyMechanicalConnectionBlockBase>("Weld speed", MySpaceTexts.BlockPropertyTitle_WeldSpeed, MySpaceTexts.Blank);
                slider4.SetLimits(block => 0f, block => MyGridPhysics.SmallShipMaxLinearVelocity());
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local45 = (MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate) slider4;
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local46 = (MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate) slider4;
                local46.DefaultValueGetter = block => MyGridPhysics.LargeShipMaxLinearVelocity() - 5f;
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local43 = local46;
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local44 = local46;
                local44.Visible = x => false;
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local41 = local44;
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local42 = local44;
                local42.Getter = x => (float) x.m_weldSpeed;
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local39 = local42;
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local40 = local42;
                local40.Setter = (x, v) => x.m_weldSpeed.Value = v;
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local37 = local40;
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local38 = local40;
                local38.Writer = (x, res) => res.AppendDecimal(((float) Math.Sqrt((double) x.m_weldSpeedSq)), 1).Append("m/s");
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local8 = local38;
                ((MyTerminalControlSlider<MyMechanicalConnectionBlockBase>) local8).EnableActions<MyMechanicalConnectionBlockBase>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyMechanicalConnectionBlockBase>((MyTerminalControl<MyMechanicalConnectionBlockBase>) local8);
                MyStringId? on = null;
                on = null;
                MyTerminalControlCheckbox<MyMechanicalConnectionBlockBase> checkbox1 = new MyTerminalControlCheckbox<MyMechanicalConnectionBlockBase>("Force weld", MySpaceTexts.BlockPropertyTitle_WeldForce, MySpaceTexts.Blank, on, on);
                MyTerminalControlCheckbox<MyMechanicalConnectionBlockBase> checkbox2 = new MyTerminalControlCheckbox<MyMechanicalConnectionBlockBase>("Force weld", MySpaceTexts.BlockPropertyTitle_WeldForce, MySpaceTexts.Blank, on, on);
                checkbox2.Visible = x => false;
                MyTerminalControlCheckbox<MyMechanicalConnectionBlockBase> local35 = checkbox2;
                MyTerminalControlCheckbox<MyMechanicalConnectionBlockBase> local36 = checkbox2;
                local36.Getter = x => (bool) x.m_forceWeld;
                MyTerminalControlCheckbox<MyMechanicalConnectionBlockBase> local33 = local36;
                MyTerminalControlCheckbox<MyMechanicalConnectionBlockBase> local34 = local36;
                local34.Setter = (x, v) => x.m_forceWeld.Value = v;
                MyTerminalControlCheckbox<MyMechanicalConnectionBlockBase> checkbox = local34;
                checkbox.EnableAction<MyMechanicalConnectionBlockBase>(null);
                MyTerminalControlFactory.AddControl<MyMechanicalConnectionBlockBase>(checkbox);
                MyTerminalControlSlider<MyMechanicalConnectionBlockBase> slider1 = new MyTerminalControlSlider<MyMechanicalConnectionBlockBase>("SafetyDetach", MySpaceTexts.BlockPropertyTitle_SafetyDetach, MySpaceTexts.BlockPropertyTooltip_SafetyDetach);
                MyTerminalControlSlider<MyMechanicalConnectionBlockBase> slider2 = new MyTerminalControlSlider<MyMechanicalConnectionBlockBase>("SafetyDetach", MySpaceTexts.BlockPropertyTitle_SafetyDetach, MySpaceTexts.BlockPropertyTooltip_SafetyDetach);
                slider2.Getter = x => x.SafetyDetach;
                MyTerminalControlSlider<MyMechanicalConnectionBlockBase> local31 = slider2;
                MyTerminalControlSlider<MyMechanicalConnectionBlockBase> local32 = slider2;
                local32.Setter = (x, v) => x.SafetyDetach = v;
                MyTerminalControlSlider<MyMechanicalConnectionBlockBase> local29 = local32;
                MyTerminalControlSlider<MyMechanicalConnectionBlockBase> local30 = local32;
                local30.DefaultValueGetter = x => 5f;
                MyTerminalControlSlider<MyMechanicalConnectionBlockBase> local27 = local30;
                MyTerminalControlSlider<MyMechanicalConnectionBlockBase> local28 = local30;
                local28.SetLimits(x => x.BlockDefinition.SafetyDetachMin, x => x.BlockDefinition.SafetyDetachMax);
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local25 = (MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate) local28;
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local26 = (MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate) local28;
                local26.Writer = (x, result) => MyValueFormatter.AppendDistanceInBestUnit(x.SafetyDetach, result);
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local23 = local26;
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local24 = local26;
                local24.Enabled = b => b.m_isAttached;
                MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate local20 = local24;
                ((MyTerminalControlSlider<MyMechanicalConnectionBlockBase>) local20).EnableActions<MyMechanicalConnectionBlockBase>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyMechanicalConnectionBlockBase>((MyTerminalControl<MyMechanicalConnectionBlockBase>) local20);
                if (MySandboxGame.Config.ExperimentalMode)
                {
                    on = null;
                    on = null;
                    MyTerminalControlCheckbox<MyMechanicalConnectionBlockBase> sharedInertiaTensor = new MyTerminalControlCheckbox<MyMechanicalConnectionBlockBase>("ShareInertiaTensor", MySpaceTexts.BlockPropertyTitle_ShareTensor, MySpaceTexts.BlockPropertyTooltip_ShareTensor, on, on) {
                        Enabled = x => x.SupportsSharedInertiaTensor,
                        Visible = x => x.SupportsSharedInertiaTensor
                    };
                    sharedInertiaTensor.Getter = x => SetTensorUIColor(x, x.ShareInertiaTensor, sharedInertiaTensor);
                    sharedInertiaTensor.Setter = (x, v) => x.ShareInertiaTensor = SetTensorUIColor(x, v, sharedInertiaTensor);
                    sharedInertiaTensor.EnableAction<MyMechanicalConnectionBlockBase>(null);
                    MyTerminalControlFactory.AddControl<MyMechanicalConnectionBlockBase>(sharedInertiaTensor);
                }
            }
        }

        private unsafe void CreateTopPart(out MyAttachableTopBlockBase topBlock, long builtBy, MyCubeBlockDefinitionGroup topGroup, bool smallToLarge, bool instantBuild)
        {
            if (topGroup == null)
            {
                topBlock = null;
            }
            else
            {
                MyCubeSize gridSizeEnum = base.CubeGrid.GridSizeEnum;
                if (smallToLarge && (gridSizeEnum == MyCubeSize.Large))
                {
                    gridSizeEnum = MyCubeSize.Small;
                }
                MatrixD topGridMatrix = this.GetTopGridMatrix();
                MyCubeBlockDefinition definition = topGroup[gridSizeEnum];
                ulong user = MySession.Static.Players.TryGetSteamId(builtBy);
                bool flag = MySession.Static.CreativeToolsEnabled(user);
                string failedBlockType = string.Empty;
                MySession.LimitResult result = MySession.Static.IsWithinWorldLimits(out failedBlockType, builtBy, definition.BlockPairName, flag ? definition.PCU : MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST, 1, 0, null);
                if (result != MySession.LimitResult.Passed)
                {
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyMechanicalConnectionBlockBase, MySession.LimitResult>(this, x => new Action<MySession.LimitResult>(x.NotifyTopPartFailed), result, new EndpointId(user));
                    topBlock = null;
                }
                else
                {
                    MyObjectBuilder_CubeBlock item = MyCubeGrid.CreateBlockObjectBuilder(definition, Vector3I.Zero, MyBlockOrientation.Identity, MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM), base.BuiltBy, MySession.Static.CreativeMode | instantBuild);
                    if (definition.Center != Vector3.Zero)
                    {
                        MatrixD* xdPtr1 = (MatrixD*) ref topGridMatrix;
                        xdPtr1.Translation = Vector3D.Transform((Vector3) (-definition.Center * MyDefinitionManager.Static.GetCubeSize(gridSizeEnum)), topGridMatrix);
                    }
                    MyObjectBuilder_AttachableTopBlockBase base2 = item as MyObjectBuilder_AttachableTopBlockBase;
                    if (base2 != null)
                    {
                        base2.YieldLastComponent = false;
                    }
                    MyObjectBuilder_Wheel wheel = item as MyObjectBuilder_Wheel;
                    if (wheel != null)
                    {
                        wheel.YieldLastComponent = false;
                    }
                    MyObjectBuilder_CubeGrid builder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_CubeGrid>();
                    builder.GridSizeEnum = gridSizeEnum;
                    builder.IsStatic = false;
                    builder.PositionAndOrientation = new MyPositionAndOrientation(topGridMatrix);
                    builder.CubeBlocks.Add(item);
                    MyCubeGrid entity = MyEntityFactory.CreateEntity<MyCubeGrid>(builder);
                    entity.Init(builder);
                    topBlock = (MyAttachableTopBlockBase) entity.GetCubeBlock(Vector3I.Zero).FatBlock;
                    if (!this.CanPlaceTop(topBlock, builtBy))
                    {
                        topBlock = null;
                        entity.Close();
                    }
                    else
                    {
                        Sandbox.Game.Entities.MyEntities.Add(entity, true);
                        MatrixD xd1 = topBlock.CubeGrid.WorldMatrix * MatrixD.Invert(base.WorldMatrix);
                        if (Sync.IsServer)
                        {
                            State state = new State {
                                TopBlockId = new long?(topBlock.EntityId)
                            };
                            this.m_connectionState.Value = state;
                        }
                    }
                }
            }
        }

        private void CreateTopPartAndAttach(long builtBy, bool smallToLarge, bool instantBuild)
        {
            MyAttachableTopBlockBase base2;
            this.CreateTopPart(out base2, builtBy, MyDefinitionManager.Static.TryGetDefinitionGroup(this.BlockDefinition.TopPart), smallToLarge, instantBuild);
            if (base2 != null)
            {
                this.Attach(base2, true);
            }
        }

        protected virtual void cubeGrid_OnPhysicsChanged()
        {
        }

        private void cubeGrid_OnPhysicsChanged(VRage.Game.Entity.MyEntity obj)
        {
            if (!Sandbox.Game.Entities.MyEntities.IsClosingAll)
            {
                this.cubeGrid_OnPhysicsChanged();
                if ((this.TopGrid != null) && (base.CubeGrid != null))
                {
                    if (!ReferenceEquals(MyCubeGridGroups.Static.Logical.GetGroup(this.TopBlock.CubeGrid), MyCubeGridGroups.Static.Logical.GetGroup(base.CubeGrid)))
                    {
                        this.m_needReattach = true;
                        base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                    }
                    else if ((this.TopGrid.Physics != null) && (base.CubeGrid.Physics != null))
                    {
                        if (this.m_constraint == null)
                        {
                            if (!this.m_welded)
                            {
                                this.m_needReattach = this.TopGrid.Physics.RigidBody != base.CubeGrid.Physics.RigidBody;
                                if (this.m_needReattach)
                                {
                                    base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                                }
                            }
                        }
                        else
                        {
                            int num1;
                            if ((this.m_constraint.RigidBodyA != base.CubeGrid.Physics.RigidBody) || (this.m_constraint.RigidBodyB != this.TopGrid.Physics.RigidBody))
                            {
                                num1 = (this.m_constraint.RigidBodyA != this.TopGrid.Physics.RigidBody) ? 0 : ((int) (this.m_constraint.RigidBodyB == base.CubeGrid.Physics.RigidBody));
                            }
                            else
                            {
                                num1 = 1;
                            }
                            if (num1 == 0)
                            {
                                this.m_needReattach = true;
                                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                            }
                        }
                    }
                }
            }
        }

        protected virtual void Detach(bool updateGroups = true)
        {
            this.Detach(this.TopGrid, updateGroups);
        }

        protected virtual void Detach(MyCubeGrid topGrid, bool updateGroups)
        {
            if (base.CubeGrid.Physics != null)
            {
                base.CubeGrid.Physics.AddDirtyBlock(base.SlimBlock);
            }
            if (this.m_welded)
            {
                this.UnweldGroup(topGrid);
            }
            if (updateGroups && !Sandbox.Game.Entities.MyEntities.IsClosingAll)
            {
                this.m_needReattach = false;
                this.BreakLinks(topGrid, this.TopBlock);
                if (Sync.IsServer)
                {
                    State state = new State {
                        TopBlockId = null,
                        Welded = false
                    };
                    this.m_connectionState.Value = state;
                }
            }
            this.DisposeConstraint(topGrid);
            if (this.TopBlock != null)
            {
                this.TopBlock.Detach(false);
            }
            this.m_topBlock = null;
            this.m_isAttached = false;
            if (!Sandbox.Game.Entities.MyEntities.IsClosingAll)
            {
                this.UpdateText();
            }
            if (updateGroups && !Sandbox.Game.Entities.MyEntities.IsClosingAll)
            {
                this.RaiseAttachedEntityChanged();
            }
        }

        [Event(null, 0x4b4), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        protected void DetachRequest()
        {
            this.Detach(true);
        }

        protected virtual void DisposeConstraint(MyCubeGrid topGrid)
        {
            MySharedTensorsGroups.BreakLinkIfExists(base.CubeGrid, topGrid, this);
        }

        [Event(null, 0x3ae), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void DoRecreateTop(long builderId, bool smallToLarge, bool instantBuild)
        {
            if (this.TopBlock == null)
            {
                this.CreateTopPartAndAttach(builderId, smallToLarge, instantBuild);
            }
        }

        [Event(null, 0x299), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        protected void FindAndAttachTopServer()
        {
            MyAttachableTopBlockBase top = this.FindMatchingTop();
            if (top != null)
            {
                this.TryAttach(top, true);
            }
        }

        private MyAttachableTopBlockBase FindMatchingTop()
        {
            Quaternion quaternion;
            Vector3D vectord;
            Vector3 vector;
            this.ComputeTopQueryBox(out vectord, out vector, out quaternion);
            using (MyUtils.ReuseCollection<HkBodyCollision>(ref m_penetrations))
            {
                MyPhysics.GetPenetrationsBox(ref vector, ref vectord, ref quaternion, m_penetrations, 15);
                using (List<HkBodyCollision>.Enumerator enumerator = m_penetrations.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        VRage.ModAPI.IMyEntity collisionEntity = enumerator.Current.GetCollisionEntity();
                        if ((collisionEntity != null) && !ReferenceEquals(collisionEntity, base.CubeGrid))
                        {
                            MyAttachableTopBlockBase base3;
                            MyAttachableTopBlockBase base2 = this.FindTopInGrid(collisionEntity, vectord);
                            if (base2 == null)
                            {
                                MyPhysicsBody physics = collisionEntity.Physics as MyPhysicsBody;
                                if (physics == null)
                                {
                                    continue;
                                }
                                HashSet<MyPhysicsBody>.Enumerator enumerator2 = physics.WeldInfo.Children.GetEnumerator();
                                try
                                {
                                    while (true)
                                    {
                                        if (!enumerator2.MoveNext())
                                        {
                                            break;
                                        }
                                        MyPhysicsBody current = enumerator2.Current;
                                        base2 = this.FindTopInGrid(current.Entity, vectord);
                                        if (base2 != null)
                                        {
                                            return base2;
                                        }
                                    }
                                    continue;
                                }
                                finally
                                {
                                    enumerator2.Dispose();
                                    continue;
                                }
                            }
                            else
                            {
                                base3 = base2;
                            }
                            return base3;
                        }
                    }
                }
            }
            return null;
        }

        private MyAttachableTopBlockBase FindTopInGrid(VRage.ModAPI.IMyEntity entity, Vector3D pos)
        {
            MyCubeGrid grid = entity as MyCubeGrid;
            if (grid != null)
            {
                Vector3D worldStart = this.TransformPosition(ref pos);
                Vector3I? nullable = grid.RayCastBlocks(worldStart, worldStart + base.WorldMatrix.Up);
                if (nullable != null)
                {
                    MySlimBlock cubeBlock = grid.GetCubeBlock(nullable.Value);
                    if ((cubeBlock != null) && (cubeBlock.FatBlock != null))
                    {
                        return (cubeBlock.FatBlock as MyAttachableTopBlockBase);
                    }
                }
            }
            return null;
        }

        public MyStringId GetAttachState()
        {
            if (this.m_welded || this.m_isWelding)
            {
                return MySpaceTexts.BlockPropertiesText_MotorLocked;
            }
            if (this.m_connectionState.Value.TopBlockId == null)
            {
                return MySpaceTexts.BlockPropertiesText_MotorDetached;
            }
            return ((this.m_connectionState.Value.TopBlockId.Value != 0) ? (!this.m_isAttached ? MySpaceTexts.BlockPropertiesText_MotorAttachingSpecific : MySpaceTexts.BlockPropertiesText_MotorAttached) : MySpaceTexts.BlockPropertiesText_MotorAttachingAny);
        }

        protected virtual float GetConstraintDisplacementSq()
        {
            Vector3 vector;
            Vector3 vector2;
            this.m_constraint.GetPivotsInWorld(out vector, out vector2);
            return (vector - vector2).LengthSquared();
        }

        public virtual Vector3? GetConstraintPosition(MyCubeGrid grid, bool opposite = false) => 
            null;

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_MechanicalConnectionBlock objectBuilderCubeBlock = base.GetObjectBuilderCubeBlock(copy) as MyObjectBuilder_MechanicalConnectionBlock;
            objectBuilderCubeBlock.WeldSpeed = (float) this.m_weldSpeed;
            objectBuilderCubeBlock.ForceWeld = (bool) this.m_forceWeld;
            objectBuilderCubeBlock.TopBlockId = this.m_connectionState.Value.TopBlockId;
            objectBuilderCubeBlock.IsWelded = this.m_connectionState.Value.Welded;
            objectBuilderCubeBlock.SafetyDetach = new float?(this.SafetyDetach);
            objectBuilderCubeBlock.ShareInertiaTensor = this.ShareInertiaTensor;
            return objectBuilderCubeBlock;
        }

        protected abstract MatrixD GetTopGridMatrix();
        protected override bool HasUnsafeSettingsCollector() => 
            (this.ShareInertiaTensor || base.HasUnsafeSettingsCollector());

        public override unsafe void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_MechanicalConnectionBlock block = objectBuilder as MyObjectBuilder_MechanicalConnectionBlock;
            this.m_weldSpeed.SetLocalValue(MathHelper.Clamp(block.WeldSpeed, 0f, MyGridPhysics.SmallShipMaxLinearVelocity()));
            this.m_forceWeld.SetLocalValue(block.ForceWeld);
            if ((block.TopBlockId != null) && (block.TopBlockId.Value != 0))
            {
                State* statePtr1;
                State newValue = new State {
                    TopBlockId = block.TopBlockId
                };
                statePtr1->Welded = block.IsWelded || block.ForceWeld;
                statePtr1 = (State*) ref newValue;
                this.m_connectionState.SetLocalValue(newValue);
            }
            if (!this.SupportsSharedInertiaTensor)
            {
                block.ShareInertiaTensor = false;
            }
            this.m_shareInertiaTensor.SetLocalValue(block.ShareInertiaTensor);
            this.m_shareInertiaTensor.ValueChanged += new Action<SyncBase>(this.ShareInertiaTensor_ValueChanged);
            float? safetyDetach = block.SafetyDetach;
            this.m_safetyDetach.SetLocalValue(MathHelper.Clamp((safetyDetach != null) ? safetyDetach.GetValueOrDefault() : this.BlockDefinition.SafetyDetach, this.BlockDefinition.SafetyDetachMin, this.BlockDefinition.SafetyDetachMax + 0.1f));
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME;
        }

        public void MarkForReattach()
        {
            this.m_needReattach = true;
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        [Event(null, 990), Reliable, Client]
        private void NotifyTopPartFailed(MySession.LimitResult result)
        {
            if (result != MySession.LimitResult.Passed)
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
                MyHud.Notifications.Add(MySession.GetNotificationForLimitResult(result));
            }
        }

        private void OnAttachTargetChanged()
        {
            this.m_updateAttach = true;
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void OnBuildSuccess(long builtBy, bool instantBuild)
        {
            base.OnBuildSuccess(builtBy, instantBuild);
            if (Sync.IsServer)
            {
                this.CreateTopPartAndAttach(builtBy, false, instantBuild);
            }
        }

        private void OnForceWeldChanged()
        {
            if (this.m_isAttached && Sync.IsServer)
            {
                if (this.m_forceWeld == null)
                {
                    base.RaisePropertiesChanged();
                }
                else if (!this.m_welded)
                {
                    this.WeldGroup(true);
                    this.UpdateText();
                }
            }
        }

        public override void OnRegisteredToGridSystems()
        {
            base.OnRegisteredToGridSystems();
            this.m_updateAttach = true;
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void OnRemovedFromScene(object source)
        {
            base.OnRemovedFromScene(source);
            if (this.m_isAttached)
            {
                this.m_needReattach = false;
                State state = this.m_connectionState.Value;
                this.Detach(true);
                if (Sync.IsServer)
                {
                    this.m_connectionState.Value = state;
                }
            }
            this.m_shareInertiaTensor.ValueChanged -= new Action<SyncBase>(this.ShareInertiaTensor_ValueChanged);
        }

        public virtual void OnTopBlockCubeGridChanged(MyCubeGrid oldGrid)
        {
            this.Detach(oldGrid, true);
            State newValue = new State {
                TopBlockId = new long?(this.TopBlock.EntityId),
                Welded = this.m_welded
            };
            this.m_connectionState.SetLocalValue(newValue);
            this.MarkForReattach();
        }

        public override void OnUnregisteredFromGridSystems()
        {
            base.OnUnregisteredFromGridSystems();
            if (Sync.IsServer && this.m_isAttached)
            {
                this.m_needReattach = false;
                State state = this.m_connectionState.Value;
                this.Detach(true);
                this.m_connectionState.Value = state;
            }
        }

        private void RaiseAttachedEntityChanged()
        {
            if (this.AttachedEntityChanged != null)
            {
                this.AttachedEntityChanged(this);
            }
        }

        private void Reattach(MyCubeGrid topGrid)
        {
            this.m_needReattach = false;
            if (this.TopBlock == null)
            {
                MyLog.Default.WriteLine("TopBlock null in MechanicalConnection.Reatach");
                this.m_updateAttach = true;
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
            else
            {
                bool updateGroups = !ReferenceEquals(MyCubeGridGroups.Static.Logical.GetGroup(topGrid), MyCubeGridGroups.Static.Logical.GetGroup(base.CubeGrid));
                MyAttachableTopBlockBase topBlock = this.TopBlock;
                this.Detach(topGrid, updateGroups);
                if (this.TryAttach(topBlock, updateGroups))
                {
                    if (topBlock.CubeGrid.Physics != null)
                    {
                        topBlock.CubeGrid.Physics.ForceActivate();
                    }
                }
                else
                {
                    if (!updateGroups)
                    {
                        this.BreakLinks(topGrid, topBlock);
                        this.RaiseAttachedEntityChanged();
                    }
                    if (Sync.IsServer)
                    {
                        State state = new State {
                            TopBlockId = 0L
                        };
                        this.m_connectionState.Value = state;
                    }
                }
            }
        }

        protected void RecreateTop(long? builderId = new long?(), bool smallToLarge = false, bool instantBuild = false)
        {
            long identityId = (builderId != null) ? builderId.Value : MySession.Static.LocalPlayerId;
            if (this.m_isAttached || !this.CanPlaceTop())
            {
                if (identityId == MySession.Static.LocalPlayerId)
                {
                    MyHud.Notifications.Add(MyNotificationSingletons.HeadAlreadyExists);
                }
            }
            else
            {
                MyCubeSize gridSizeEnum = base.CubeGrid.GridSizeEnum;
                if (smallToLarge && (gridSizeEnum == MyCubeSize.Large))
                {
                    gridSizeEnum = MyCubeSize.Small;
                }
                MyCubeBlockDefinition definition = MyDefinitionManager.Static.TryGetDefinitionGroup(this.BlockDefinition.TopPart)[gridSizeEnum];
                bool flag = MySession.Static.CreativeToolsEnabled(MySession.Static.Players.TryGetSteamId(identityId));
                if (MySession.Static.CheckLimitsAndNotify(identityId, definition.BlockPairName, flag ? definition.PCU : MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST, 1, 0, null))
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyMechanicalConnectionBlockBase, long, bool, bool>(this, x => new Action<long, bool, bool>(x.DoRecreateTop), identityId, smallToLarge, instantBuild, targetEndpoint);
                }
            }
        }

        private void RefreshConstraint()
        {
            if (this.m_welded)
            {
                if (this.m_constraint != null)
                {
                    this.DisposeConstraint(this.TopGrid);
                }
            }
            else
            {
                bool flag = this.m_constraint == null;
                if ((this.m_constraint != null) && !this.m_constraint.InWorld)
                {
                    this.DisposeConstraint(this.TopGrid);
                    flag = true;
                }
                if (flag && (this.TopBlock != null))
                {
                    this.CreateConstraint(this.TopBlock);
                    base.RaisePropertiesChanged();
                }
            }
        }

        void Sandbox.ModAPI.IMyMechanicalConnectionBlock.Attach(Sandbox.ModAPI.IMyAttachableTopBlock top, bool updateGroup)
        {
            if (top != null)
            {
                this.Attach((MyAttachableTopBlockBase) top, updateGroup);
            }
        }

        void Sandbox.ModAPI.Ingame.IMyMechanicalConnectionBlock.Attach()
        {
            State state = this.m_connectionState.Value;
            if (state.TopBlockId == null)
            {
                state = new State {
                    TopBlockId = 0L
                };
                this.m_connectionState.Value = state;
            }
        }

        void Sandbox.ModAPI.Ingame.IMyMechanicalConnectionBlock.Detach()
        {
            State state = new State {
                TopBlockId = null
            };
            this.m_connectionState.Value = state;
        }

        private static bool SetTensorUIColor(MyMechanicalConnectionBlockBase block, bool isUnsafeValue, MyTerminalControlCheckbox<MyMechanicalConnectionBlockBase> control)
        {
            if (!Sync.IsDedicated)
            {
                Vector4 colorMask = control.GetGuiControl().ColorMask;
                if (isUnsafeValue)
                {
                    colorMask = (Vector4) Color.Red;
                }
                control.GetGuiControl().Elements[0].ColorMask = colorMask;
            }
            return isUnsafeValue;
        }

        private void ShareInertiaTensor_ValueChanged(SyncBase obj)
        {
            this.UpdateSharedTensorState();
            base.OnUnsafeSettingsChanged();
        }

        private void TopBlock_OnClosing(VRage.Game.Entity.MyEntity obj)
        {
            this.Detach(true);
        }

        protected virtual Vector3D TransformPosition(ref Vector3D position) => 
            position;

        private bool TryAttach(MyAttachableTopBlockBase top, bool updateGroup = true) => 
            (this.CanAttach(top) && this.Attach(top, updateGroup));

        private void UnweldGroup(MyCubeGrid topGrid)
        {
            if (this.m_welded)
            {
                this.m_isWelding = true;
                MyWeldingGroups.Static.BreakLink(base.EntityId, base.CubeGrid, topGrid);
                if (Sync.IsServer)
                {
                    State state = new State {
                        TopBlockId = new long?(this.TopBlock.EntityId),
                        Welded = false
                    };
                    this.m_connectionState.Value = state;
                }
                this.m_welded = false;
                this.m_isWelding = false;
                base.RaisePropertiesChanged();
            }
        }

        private void UpdateAttachState()
        {
            this.m_updateAttach = false;
            this.m_needReattach = false;
            if (this.m_connectionState.Value.TopBlockId != null)
            {
                long? topBlockId = this.m_connectionState.Value.TopBlockId;
                long num = 0L;
                if (!((topBlockId.GetValueOrDefault() == num) & (topBlockId != null)))
                {
                    MyAttachableTopBlockBase base2;
                    if (this.TopBlock != null)
                    {
                        topBlockId = this.m_connectionState.Value.TopBlockId;
                        if ((this.TopBlock.EntityId == topBlockId.GetValueOrDefault()) & (topBlockId != null))
                        {
                            if (this.m_welded != this.m_connectionState.Value.Welded)
                            {
                                if (this.m_connectionState.Value.Welded)
                                {
                                    this.WeldGroup(true);
                                }
                                else
                                {
                                    this.UnweldGroup(this.TopGrid);
                                }
                            }
                            goto TR_0000;
                        }
                    }
                    State state = this.m_connectionState.Value;
                    if (this.TopBlock != null)
                    {
                        this.Detach(true);
                    }
                    if (!Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyAttachableTopBlockBase>(state.TopBlockId.Value, out base2, false) || !this.TryAttach(base2, true))
                    {
                        if (!Sync.IsServer || ((base2 != null) && !base2.MarkedForClose))
                        {
                            this.m_updateAttach = true;
                            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                        }
                        else
                        {
                            state = new State {
                                TopBlockId = null
                            };
                            this.m_connectionState.Value = state;
                        }
                    }
                }
                else
                {
                    if (this.m_isAttached)
                    {
                        this.Detach(true);
                    }
                    if (Sync.IsServer)
                    {
                        this.FindAndAttachTopServer();
                    }
                }
            }
            else if (this.m_isAttached)
            {
                this.Detach(true);
            }
        TR_0000:
            this.RefreshConstraint();
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            this.CheckSafetyDetach();
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if (this.m_updateAttach)
            {
                this.UpdateAttachState();
            }
            if (this.m_needReattach)
            {
                this.Reattach(this.TopGrid);
            }
            base.OnUnsafeSettingsChanged();
            bool flag = (base.CubeGrid != null) && base.CubeGrid.IsPreview;
            if ((this.m_updateAttach || this.m_needReattach) && !flag)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
        }

        private void UpdateSharedTensorState()
        {
            if (!this.ShareInertiaTensor)
            {
                MySharedTensorsGroups.BreakLinkIfExists(base.CubeGrid, this.TopGrid, this);
            }
            else if (this.TopGrid != null)
            {
                MySharedTensorsGroups.Link(base.CubeGrid, this.TopGrid, this);
            }
        }

        protected virtual void UpdateText()
        {
        }

        private bool ValidateTopBlockId(State newState)
        {
            if (newState.TopBlockId == null)
            {
                return true;
            }
            long? topBlockId = newState.TopBlockId;
            long num = 0L;
            return (((topBlockId.GetValueOrDefault() == num) & (topBlockId != null)) && (this.m_connectionState.Value.TopBlockId == null));
        }

        private void WeldGroup(bool force)
        {
            if (MyFakes.WELD_ROTORS)
            {
                this.m_isWelding = true;
                this.DisposeConstraint(this.TopGrid);
                MyWeldingGroups.Static.CreateLink(base.EntityId, base.CubeGrid, this.TopGrid);
                if (Sync.IsServer)
                {
                    MatrixD xd1 = this.TopBlock.CubeGrid.WorldMatrix * MatrixD.Invert(base.WorldMatrix);
                    State state = new State {
                        TopBlockId = new long?(this.TopBlock.EntityId),
                        Welded = true
                    };
                    this.m_connectionState.Value = state;
                }
                this.m_welded = true;
                this.m_isWelding = false;
                base.RaisePropertiesChanged();
            }
        }

        private void WeldSpeed_ValueChanged(SyncBase obj)
        {
            this.m_weldSpeedSq = (float) (this.m_weldSpeed * this.m_weldSpeed);
            this.m_unweldSpeedSq = Math.Max((float) (((float) this.m_weldSpeed) - 2f), (float) 0f);
            this.m_unweldSpeedSq *= this.m_unweldSpeedSq;
        }

        private bool ShareInertiaTensor
        {
            get => 
                ((bool) this.m_shareInertiaTensor);
            set
            {
                if (this.SupportsSharedInertiaTensor)
                {
                    this.m_shareInertiaTensor.Value = value;
                }
            }
        }

        public MyCubeGrid TopGrid =>
            this.TopBlock?.CubeGrid;

        public MyAttachableTopBlockBase TopBlock =>
            this.m_topBlock;

        protected bool m_isWelding { get; private set; }

        protected bool m_welded { get; private set; }

        protected bool m_isAttached { get; private set; }

        public float SafetyDetach
        {
            get => 
                this.m_safetyDetach.Value;
            set => 
                (this.m_safetyDetach.Value = value);
        }

        private MyMechanicalConnectionBlockBaseDefinition BlockDefinition =>
            ((MyMechanicalConnectionBlockBaseDefinition) base.BlockDefinition);

        protected HkConstraint SafeConstraint
        {
            get
            {
                this.RefreshConstraint();
                return this.m_constraint;
            }
        }

        bool Sandbox.ModAPI.Ingame.IMyMechanicalConnectionBlock.IsAttached =>
            this.m_isAttached;

        float Sandbox.ModAPI.Ingame.IMyMechanicalConnectionBlock.SafetyLockSpeed
        {
            get => 
                ((float) this.m_weldSpeed);
            set
            {
                float single1 = value;
                value = MathHelper.Clamp(single1, 0f, (base.CubeGrid.GridSizeEnum == MyCubeSize.Large) ? MyGridPhysics.LargeShipMaxLinearVelocity() : MyGridPhysics.SmallShipMaxLinearVelocity());
                this.m_weldSpeed.Value = value;
            }
        }

        bool Sandbox.ModAPI.Ingame.IMyMechanicalConnectionBlock.SafetyLock
        {
            get => 
                (this.m_isWelding || this.m_welded);
            set
            {
                if ((this.m_isWelding || this.m_welded) != value)
                {
                    this.m_forceWeld.Value = value;
                }
            }
        }

        bool Sandbox.ModAPI.Ingame.IMyMechanicalConnectionBlock.IsLocked =>
            (this.m_isWelding || this.m_welded);

        bool Sandbox.ModAPI.Ingame.IMyMechanicalConnectionBlock.PendingAttachment =>
            ((this.m_connectionState.Value.TopBlockId != null) && (this.m_connectionState.Value.TopBlockId.Value == 0L));

        VRage.Game.ModAPI.IMyCubeGrid Sandbox.ModAPI.IMyMechanicalConnectionBlock.TopGrid =>
            this.TopGrid;

        Sandbox.ModAPI.IMyAttachableTopBlock Sandbox.ModAPI.IMyMechanicalConnectionBlock.Top =>
            this.TopBlock;

        VRage.Game.ModAPI.Ingame.IMyCubeGrid Sandbox.ModAPI.Ingame.IMyMechanicalConnectionBlock.TopGrid =>
            this.TopGrid;

        Sandbox.ModAPI.Ingame.IMyAttachableTopBlock Sandbox.ModAPI.Ingame.IMyMechanicalConnectionBlock.Top =>
            this.TopBlock;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyMechanicalConnectionBlockBase.<>c <>9 = new MyMechanicalConnectionBlockBase.<>c();
            public static MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate <>9__43_0;
            public static MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate <>9__43_1;
            public static MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate <>9__43_2;
            public static Func<MyMechanicalConnectionBlockBase, bool> <>9__43_3;
            public static MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate <>9__43_4;
            public static MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.SetterDelegate <>9__43_5;
            public static MyTerminalControl<MyMechanicalConnectionBlockBase>.WriterDelegate <>9__43_6;
            public static Func<MyMechanicalConnectionBlockBase, bool> <>9__43_7;
            public static MyTerminalValueControl<MyMechanicalConnectionBlockBase, bool>.GetterDelegate <>9__43_8;
            public static MyTerminalValueControl<MyMechanicalConnectionBlockBase, bool>.SetterDelegate <>9__43_9;
            public static MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate <>9__43_10;
            public static MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.SetterDelegate <>9__43_11;
            public static MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate <>9__43_12;
            public static MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate <>9__43_13;
            public static MyTerminalValueControl<MyMechanicalConnectionBlockBase, float>.GetterDelegate <>9__43_14;
            public static MyTerminalControl<MyMechanicalConnectionBlockBase>.WriterDelegate <>9__43_15;
            public static Func<MyMechanicalConnectionBlockBase, bool> <>9__43_16;
            public static Func<MyMechanicalConnectionBlockBase, bool> <>9__43_17;
            public static Func<MyMechanicalConnectionBlockBase, bool> <>9__43_18;
            public static Func<MyMechanicalConnectionBlockBase, Action<long, bool, bool>> <>9__82_0;
            public static Func<MyMechanicalConnectionBlockBase, Action<MySession.LimitResult>> <>9__90_0;
            public static Func<MyMechanicalConnectionBlockBase, Action> <>9__97_0;
            public static Func<MyMechanicalConnectionBlockBase, Action> <>9__98_0;

            internal Action <CallAttach>b__98_0(MyMechanicalConnectionBlockBase x) => 
                new Action(x.FindAndAttachTopServer);

            internal Action <CallDetach>b__97_0(MyMechanicalConnectionBlockBase x) => 
                new Action(x.DetachRequest);

            internal float <CreateTerminalControls>b__43_0(MyMechanicalConnectionBlockBase block) => 
                0f;

            internal float <CreateTerminalControls>b__43_1(MyMechanicalConnectionBlockBase block) => 
                MyGridPhysics.SmallShipMaxLinearVelocity();

            internal float <CreateTerminalControls>b__43_10(MyMechanicalConnectionBlockBase x) => 
                x.SafetyDetach;

            internal void <CreateTerminalControls>b__43_11(MyMechanicalConnectionBlockBase x, float v)
            {
                x.SafetyDetach = v;
            }

            internal float <CreateTerminalControls>b__43_12(MyMechanicalConnectionBlockBase x) => 
                5f;

            internal float <CreateTerminalControls>b__43_13(MyMechanicalConnectionBlockBase x) => 
                x.BlockDefinition.SafetyDetachMin;

            internal float <CreateTerminalControls>b__43_14(MyMechanicalConnectionBlockBase x) => 
                x.BlockDefinition.SafetyDetachMax;

            internal void <CreateTerminalControls>b__43_15(MyMechanicalConnectionBlockBase x, StringBuilder result)
            {
                MyValueFormatter.AppendDistanceInBestUnit(x.SafetyDetach, result);
            }

            internal bool <CreateTerminalControls>b__43_16(MyMechanicalConnectionBlockBase b) => 
                b.m_isAttached;

            internal bool <CreateTerminalControls>b__43_17(MyMechanicalConnectionBlockBase x) => 
                x.SupportsSharedInertiaTensor;

            internal bool <CreateTerminalControls>b__43_18(MyMechanicalConnectionBlockBase x) => 
                x.SupportsSharedInertiaTensor;

            internal float <CreateTerminalControls>b__43_2(MyMechanicalConnectionBlockBase block) => 
                (MyGridPhysics.LargeShipMaxLinearVelocity() - 5f);

            internal bool <CreateTerminalControls>b__43_3(MyMechanicalConnectionBlockBase x) => 
                false;

            internal float <CreateTerminalControls>b__43_4(MyMechanicalConnectionBlockBase x) => 
                ((float) x.m_weldSpeed);

            internal void <CreateTerminalControls>b__43_5(MyMechanicalConnectionBlockBase x, float v)
            {
                x.m_weldSpeed.Value = v;
            }

            internal void <CreateTerminalControls>b__43_6(MyMechanicalConnectionBlockBase x, StringBuilder res)
            {
                res.AppendDecimal(((float) Math.Sqrt((double) x.m_weldSpeedSq)), 1).Append("m/s");
            }

            internal bool <CreateTerminalControls>b__43_7(MyMechanicalConnectionBlockBase x) => 
                false;

            internal bool <CreateTerminalControls>b__43_8(MyMechanicalConnectionBlockBase x) => 
                ((bool) x.m_forceWeld);

            internal void <CreateTerminalControls>b__43_9(MyMechanicalConnectionBlockBase x, bool v)
            {
                x.m_forceWeld.Value = v;
            }

            internal Action<MySession.LimitResult> <CreateTopPart>b__90_0(MyMechanicalConnectionBlockBase x) => 
                new Action<MySession.LimitResult>(x.NotifyTopPartFailed);

            internal Action<long, bool, bool> <RecreateTop>b__82_0(MyMechanicalConnectionBlockBase x) => 
                new Action<long, bool, bool>(x.DoRecreateTop);
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct State
        {
            public long? TopBlockId;
            public bool Welded;
        }
    }
}

