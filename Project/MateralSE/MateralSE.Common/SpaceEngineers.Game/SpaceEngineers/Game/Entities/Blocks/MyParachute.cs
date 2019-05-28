namespace SpaceEngineers.Game.Entities.Blocks
{
    using Havok;
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Sync;
    using VRageMath;
    using VRageRender.Import;

    [MyCubeBlockType(typeof(MyObjectBuilder_Parachute)), MyTerminalInterface(new System.Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyParachute), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyParachute) })]
    public class MyParachute : MyDoorBase, SpaceEngineers.Game.ModAPI.IMyParachute, SpaceEngineers.Game.ModAPI.Ingame.IMyParachute, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, IMyConveyorEndpointBlock
    {
        private static readonly float EPSILON = 1E-09f;
        private const float MIN_DEPLOYHEIGHT = 10f;
        private const float MAX_DEPLOYHEIGHT = 10000f;
        private const double DENSITYOFAIRINONEATMO = 1.225;
        private const float NO_DRAG_SPEED_SQRD = 0.1f;
        private const float NO_DRAG_SPEED_RANGE = 20f;
        private int m_lastUpdateTime;
        private float m_time;
        private float m_totalTime = 99999f;
        private bool m_stateChange;
        private List<MyEntitySubpart> m_subparts = new List<MyEntitySubpart>();
        private List<int> m_subpartIDs = new List<int>();
        private List<float> m_currentOpening = new List<float>();
        private List<float> m_currentSpeed = new List<float>();
        private List<MyEntity3DSoundEmitter> m_emitter = new List<MyEntity3DSoundEmitter>();
        private List<Vector3> m_hingePosition = new List<Vector3>();
        private List<MyObjectBuilder_ParachuteDefinition.Opening> m_openingSequence = new List<MyObjectBuilder_ParachuteDefinition.Opening>();
        private MyMultilineConveyorEndpoint m_conveyorEndpoint;
        private Matrix[] transMat = new Matrix[1];
        private Matrix[] rotMat = new Matrix[1];
        private int m_sequenceCount;
        private int m_subpartCount;
        protected readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_autoDeploy;
        protected readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_deployHeight;
        private MyPlanet m_nearPlanetCache;
        private MyEntitySubpart m_parachuteSubpart;
        private Vector3 m_lastParachuteVelocityVector = Vector3.Zero;
        private Vector3 m_lastParachuteScale = Vector3.Zero;
        private Vector3 m_gravityCache = Vector3.Zero;
        private Vector3D m_chuteScale = Vector3D.Zero;
        private Vector3D? m_closestPointCache;
        private int m_parachuteAnimationState;
        private int m_cutParachuteTimer;
        private bool m_canDeploy;
        private bool m_canCheckAutoDeploy;
        private bool m_atmosphereDirty = true;
        private float m_minAtmosphere = 0.2f;
        private float m_dragCoefficient = 1f;
        private float m_atmosphereDensityCache;
        private MyFixedPoint m_requiredItemsInInventory = 0;
        private Quaternion m_lastParachuteRotation = Quaternion.Identity;
        private Matrix m_lastParachuteLocalMatrix = Matrix.Identity;
        private MatrixD m_lastParachuteWorldMatrix = MatrixD.Identity;
        [CompilerGenerated]
        private Action<bool> DoorStateChanged;
        [CompilerGenerated]
        private Action<bool> ParachuteStateChanged;

        private event Action<bool> DoorStateChanged
        {
            [CompilerGenerated] add
            {
                Action<bool> doorStateChanged = this.DoorStateChanged;
                while (true)
                {
                    Action<bool> a = doorStateChanged;
                    Action<bool> action3 = (Action<bool>) Delegate.Combine(a, value);
                    doorStateChanged = Interlocked.CompareExchange<Action<bool>>(ref this.DoorStateChanged, action3, a);
                    if (ReferenceEquals(doorStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<bool> doorStateChanged = this.DoorStateChanged;
                while (true)
                {
                    Action<bool> source = doorStateChanged;
                    Action<bool> action3 = (Action<bool>) Delegate.Remove(source, value);
                    doorStateChanged = Interlocked.CompareExchange<Action<bool>>(ref this.DoorStateChanged, action3, source);
                    if (ReferenceEquals(doorStateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        private event Action<bool> ParachuteStateChanged
        {
            [CompilerGenerated] add
            {
                Action<bool> parachuteStateChanged = this.ParachuteStateChanged;
                while (true)
                {
                    Action<bool> a = parachuteStateChanged;
                    Action<bool> action3 = (Action<bool>) Delegate.Combine(a, value);
                    parachuteStateChanged = Interlocked.CompareExchange<Action<bool>>(ref this.ParachuteStateChanged, action3, a);
                    if (ReferenceEquals(parachuteStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<bool> parachuteStateChanged = this.ParachuteStateChanged;
                while (true)
                {
                    Action<bool> source = parachuteStateChanged;
                    Action<bool> action3 = (Action<bool>) Delegate.Remove(source, value);
                    parachuteStateChanged = Interlocked.CompareExchange<Action<bool>>(ref this.ParachuteStateChanged, action3, source);
                    if (ReferenceEquals(parachuteStateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action<bool> SpaceEngineers.Game.ModAPI.IMyParachute.DoorStateChanged
        {
            add
            {
                this.DoorStateChanged += value;
            }
            remove
            {
                this.DoorStateChanged -= value;
            }
        }

        event Action<bool> SpaceEngineers.Game.ModAPI.IMyParachute.ParachuteStateChanged
        {
            add
            {
                this.ParachuteStateChanged += value;
            }
            remove
            {
                this.ParachuteStateChanged -= value;
            }
        }

        public MyParachute()
        {
            this.m_subparts.Clear();
            this.m_subpartIDs.Clear();
            this.m_currentOpening.Clear();
            this.m_currentSpeed.Clear();
            this.m_emitter.Clear();
            this.m_hingePosition.Clear();
            this.m_openingSequence.Clear();
            base.m_open.ValueChanged += x => this.OnStateChange();
        }

        public bool AllowSelfPulling() => 
            false;

        public void AttemptPullRequiredInventoryItems()
        {
            if (this.BlockDefinition.MaterialDeployCost > this.m_requiredItemsInInventory)
            {
                MyGridConveyorSystem.ItemPullRequest(this, this.GetInventory(0), base.OwnerId, this.BlockDefinition.MaterialDefinitionId, new MyFixedPoint?(this.BlockDefinition.MaterialDeployCost - this.m_requiredItemsInInventory), false);
            }
        }

        [Event(null, 0x158), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void AutoDeployRequest(bool autodeploy, long identityId)
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
                this.AutoDeploy = autodeploy;
            }
        }

        private void CheckAutoDeploy()
        {
            if ((this.m_closestPointCache != null) && (Vector3D.Distance(this.m_closestPointCache.Value, base.WorldMatrix.Translation) < this.DeployHeight))
            {
                ((SpaceEngineers.Game.ModAPI.Ingame.IMyParachute) this).OpenDoor();
            }
        }

        private bool CheckDeployChute()
        {
            if (base.CubeGrid.Physics == null)
            {
                return false;
            }
            if (!this.CanDeploy)
            {
                return false;
            }
            if (this.m_parachuteAnimationState > 0)
            {
                return false;
            }
            if (this.Atmosphere < this.m_minAtmosphere)
            {
                return false;
            }
            if (!MySession.Static.CreativeMode)
            {
                if (this.GetInventory(0).GetItemAmount(this.BlockDefinition.MaterialDefinitionId, MyItemFlags.None, false) < this.BlockDefinition.MaterialDeployCost)
                {
                    this.CanDeploy = false;
                    return false;
                }
                this.GetInventory(0).RemoveItemsOfType(this.BlockDefinition.MaterialDeployCost, this.BlockDefinition.MaterialDefinitionId, MyItemFlags.None, false);
            }
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyParachute>(this, x => new Action(x.DoDeployChute), targetEndpoint);
            return true;
        }

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        protected override void Closing()
        {
            for (int i = 0; i < this.m_emitter.Count; i++)
            {
                if (this.m_emitter[i] != null)
                {
                    this.m_emitter[i].StopSound(true, true);
                }
            }
            base.CubeGrid.OnHavokSystemIDChanged -= new Action<int>(this.CubeGrid_HavokSystemIDChanged);
            base.Closing();
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyParachute>())
            {
                base.CreateTerminalControls();
                MyTerminalControlCheckbox<MyParachute> checkbox1 = new MyTerminalControlCheckbox<MyParachute>("AutoDeploy", MySpaceTexts.Parachute_AutoDeploy, MySpaceTexts.Parachute_AutoDeployTooltip, new MyStringId?(MySpaceTexts.Parachute_AutoDeployOn), new MyStringId?(MySpaceTexts.Parachute_AutoDeployOff));
                MyTerminalControlCheckbox<MyParachute> checkbox2 = new MyTerminalControlCheckbox<MyParachute>("AutoDeploy", MySpaceTexts.Parachute_AutoDeploy, MySpaceTexts.Parachute_AutoDeployTooltip, new MyStringId?(MySpaceTexts.Parachute_AutoDeployOn), new MyStringId?(MySpaceTexts.Parachute_AutoDeployOff));
                checkbox2.Getter = x => x.AutoDeploy;
                MyTerminalControlCheckbox<MyParachute> local12 = checkbox2;
                MyTerminalControlCheckbox<MyParachute> local13 = checkbox2;
                local13.Setter = (x, v) => x.SetAutoDeployRequest(v, x.OwnerId);
                MyTerminalControlCheckbox<MyParachute> checkbox = local13;
                checkbox.EnableAction<MyParachute>(null);
                MyTerminalControlFactory.AddControl<MyParachute>(checkbox);
                MyTerminalControlSlider<MyParachute> slider1 = new MyTerminalControlSlider<MyParachute>("AutoDeployHeight", MySpaceTexts.Parachute_DeployHeightTitle, MySpaceTexts.Parachute_DeployHeightTooltip);
                MyTerminalControlSlider<MyParachute> slider2 = new MyTerminalControlSlider<MyParachute>("AutoDeployHeight", MySpaceTexts.Parachute_DeployHeightTitle, MySpaceTexts.Parachute_DeployHeightTooltip);
                slider2.Getter = x => x.DeployHeight;
                MyTerminalControlSlider<MyParachute> local10 = slider2;
                MyTerminalControlSlider<MyParachute> local11 = slider2;
                local11.Setter = (x, v) => x.SetDeployHeightRequest(v, x.OwnerId);
                MyTerminalControlSlider<MyParachute> local8 = local11;
                MyTerminalControlSlider<MyParachute> local9 = local11;
                local9.Writer = (b, v) => v.Append($"{b.DeployHeight:N0} m");
                MyTerminalControlSlider<MyParachute> control = local9;
                control.SetLogLimits((float) 10f, (float) 10000f);
                MyTerminalControlFactory.AddControl<MyParachute>(control);
            }
        }

        private void CubeGrid_HavokSystemIDChanged(int id)
        {
            this.UpdateHavokCollisionSystemID(id);
        }

        private void CubeGrid_OnPhysicsChanged(VRage.Game.Entity.MyEntity obj)
        {
            if ((((this.m_subparts != null) && (this.m_subparts.Count != 0)) && ((obj.Physics != null) && (this.m_subparts[0].Physics != null))) && (obj.GetPhysicsBody().HavokCollisionSystemID != this.m_subparts[0].GetPhysicsBody().HavokCollisionSystemID))
            {
                this.UpdateHavokCollisionSystemID(obj.GetPhysicsBody().HavokCollisionSystemID);
            }
        }

        [Event(null, 370), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void DeployHeightRequest(float deployHeight, long identityId)
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
                this.DeployHeight = deployHeight;
            }
        }

        [Event(null, 0x3ea), Reliable, ServerInvoked, Broadcast]
        private void DoDeployChute()
        {
            this.m_parachuteAnimationState = 1;
            this.m_lastParachuteRotation = Quaternion.Identity;
            this.m_lastParachuteScale = Vector3.Zero;
            this.m_cutParachuteTimer = 0;
            if (this.m_parachuteSubpart == null)
            {
                this.m_parachuteSubpart = this.LoadSubpartFromName(this.BlockDefinition.ParachuteSubpartName);
            }
            this.m_parachuteSubpart.Render.Visible = true;
            if (this.ParachuteStateChanged != null)
            {
                this.ParachuteStateChanged(true);
            }
        }

        public Vector3D GetArtificialGravity() => 
            MyGravityProviderSystem.CalculateArtificialGravityInPoint(base.WorldMatrix.Translation, 1f);

        public Vector3D GetNaturalGravity() => 
            MyGravityProviderSystem.CalculateNaturalGravityInPoint(base.WorldMatrix.Translation);

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_Parachute objectBuilderCubeBlock = (MyObjectBuilder_Parachute) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.Open = (bool) base.m_open;
            objectBuilderCubeBlock.AutoDeploy = (bool) this.m_autoDeploy;
            objectBuilderCubeBlock.DeployHeight = (float) this.m_deployHeight;
            objectBuilderCubeBlock.ParachuteState = this.m_parachuteAnimationState;
            return objectBuilderCubeBlock;
        }

        public PullInformation GetPullInformation()
        {
            PullInformation information1 = new PullInformation();
            information1.Inventory = this.GetInventory(0);
            information1.OwnerID = base.OwnerId;
            information1.ItemDefinition = this.BlockDefinition.MaterialDefinitionId;
            return information1;
        }

        public PullInformation GetPushInformation() => 
            null;

        public Vector3D GetTotalGravity() => 
            MyGravityProviderSystem.CalculateTotalGravityInPoint(base.WorldMatrix.Translation);

        public Vector3D GetVelocity()
        {
            MyPhysicsComponentBase physics = base.Parent?.Physics;
            return ((physics == null) ? Vector3D.Zero : new Vector3D(physics.GetVelocityAtPoint(base.PositionComp.GetPosition())));
        }

        public override void Init(MyObjectBuilder_CubeBlock builder, MyCubeGrid cubeGrid)
        {
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(this.BlockDefinition.ResourceSinkGroup, this.BlockDefinition.PowerConsumptionMoving, new Func<float>(this.UpdatePowerInput));
            base.ResourceSink = component;
            base.Init(builder, cubeGrid);
            MyObjectBuilder_Parachute parachute = (MyObjectBuilder_Parachute) builder;
            base.m_open.Value = parachute.Open;
            this.m_deployHeight.Value = parachute.DeployHeight;
            this.m_autoDeploy.Value = parachute.AutoDeploy;
            this.m_parachuteAnimationState = parachute.ParachuteState;
            if (this.m_parachuteAnimationState > 50)
            {
                this.m_parachuteAnimationState = 0;
            }
            this.m_dragCoefficient = this.BlockDefinition.DragCoefficient;
            this.m_minAtmosphere = this.BlockDefinition.MinimumAtmosphereLevel;
            component.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            component.Update();
            if (!base.Enabled || !base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
            {
                this.UpdateDoorPosition();
            }
            this.OnStateChange();
            if (base.m_open != null)
            {
                this.UpdateDoorPosition();
            }
            this.InitializeConveyorEndpoint();
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.m_conveyorEndpoint));
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.ResourceSink.Update();
            MyInventory inventory = this.GetInventory(0);
            MyComponentDefinition componentDefinition = MyDefinitionManager.Static.GetComponentDefinition(this.BlockDefinition.MaterialDefinitionId);
            if (inventory == null)
            {
                Vector3 one = Vector3.One;
                inventory = new MyInventory(componentDefinition.Volume * this.BlockDefinition.MaterialDeployCost, one, MyInventoryFlags.CanReceive);
                base.Components.Add<MyInventoryBase>(inventory);
            }
            this.inventory_ContentsChanged(inventory);
            inventory.ContentsChanged += new Action<MyInventoryBase>(this.inventory_ContentsChanged);
            MyInventoryConstraint constraint = new MyInventoryConstraint(MySpaceTexts.Parachute_ConstraintItem, null, true);
            constraint.Add(this.BlockDefinition.MaterialDefinitionId);
            constraint.Icon = MyGuiConstants.TEXTURE_ICON_FILTER_COMPONENT;
            inventory.Constraint = constraint;
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_conveyorEndpoint = new MyMultilineConveyorEndpoint(this);
        }

        private void InitSubparts()
        {
            if (base.CubeGrid.CreatePhysics)
            {
                this.m_subparts.Clear();
                this.m_subpartIDs.Clear();
                this.m_currentOpening.Clear();
                this.m_currentSpeed.Clear();
                this.m_emitter.Clear();
                this.m_hingePosition.Clear();
                this.m_openingSequence.Clear();
                for (int i = 0; i < this.BlockDefinition.Subparts.Length; i++)
                {
                    MyEntitySubpart item = this.LoadSubpartFromName(this.BlockDefinition.Subparts[i].Name);
                    if (item != null)
                    {
                        this.m_subparts.Add(item);
                        if (this.BlockDefinition.Subparts[i].PivotPosition != null)
                        {
                            this.m_hingePosition.Add(this.BlockDefinition.Subparts[i].PivotPosition.Value);
                        }
                        else
                        {
                            MyModelBone bone = item.Model.Bones.First<MyModelBone>(b => !b.Name.Contains("Root"));
                            if (bone != null)
                            {
                                this.m_hingePosition.Add(bone.Transform.Translation);
                            }
                        }
                    }
                }
                int length = this.BlockDefinition.OpeningSequence.Length;
                for (int j = 0; j < length; j++)
                {
                    if (string.IsNullOrEmpty(this.BlockDefinition.OpeningSequence[j].IDs))
                    {
                        this.m_openingSequence.Add(this.BlockDefinition.OpeningSequence[j]);
                        this.m_subpartIDs.Add(this.BlockDefinition.OpeningSequence[j].ID);
                    }
                    else
                    {
                        char[] separator = new char[] { ',' };
                        string[] strArray = this.BlockDefinition.OpeningSequence[j].IDs.Split(separator);
                        for (int m = 0; m < strArray.Length; m++)
                        {
                            char[] chArray2 = new char[] { '-' };
                            string[] strArray2 = strArray[m].Split(chArray2);
                            if (strArray2.Length != 2)
                            {
                                this.m_openingSequence.Add(this.BlockDefinition.OpeningSequence[j]);
                                this.m_subpartIDs.Add(Convert.ToInt32(strArray[m]));
                            }
                            else
                            {
                                for (int n = Convert.ToInt32(strArray2[0]); n <= Convert.ToInt32(strArray2[1]); n++)
                                {
                                    this.m_openingSequence.Add(this.BlockDefinition.OpeningSequence[j]);
                                    this.m_subpartIDs.Add(n);
                                }
                            }
                        }
                    }
                }
                for (int k = 0; k < this.m_openingSequence.Count; k++)
                {
                    this.m_currentOpening.Add(0f);
                    this.m_currentSpeed.Add(0f);
                    this.m_emitter.Add(new MyEntity3DSoundEmitter(this, true, 1f));
                    if (this.m_openingSequence[k].MaxOpen < 0f)
                    {
                        MyObjectBuilder_ParachuteDefinition.Opening local2 = this.m_openingSequence[k];
                        local2.MaxOpen *= -1f;
                        this.m_openingSequence[k].InvertRotation = !this.m_openingSequence[k].InvertRotation;
                    }
                }
                this.m_sequenceCount = this.m_openingSequence.Count;
                this.m_subpartCount = this.m_subparts.Count;
                Array.Resize<Matrix>(ref this.transMat, this.m_subpartCount);
                Array.Resize<Matrix>(ref this.rotMat, this.m_subpartCount);
                this.UpdateDoorPosition();
                if (base.CubeGrid.Projector == null)
                {
                    foreach (MyEntitySubpart subpart2 in this.m_subparts)
                    {
                        subpart2.Physics = null;
                        if ((subpart2 != null) && ((subpart2.Physics == null) && ((subpart2.ModelCollision.HavokCollisionShapes != null) && (subpart2.ModelCollision.HavokCollisionShapes.Length != 0))))
                        {
                            List<HkShape> list = subpart2.ModelCollision.HavokCollisionShapes.ToList<HkShape>();
                            HkListShape shape = new HkListShape(list.GetInternalArray<HkShape>(), list.Count, HkReferencePolicy.None);
                            subpart2.Physics = new MyPhysicsBody(subpart2, RigidBodyFlag.RBF_UNLOCKED_SPEEDS | RigidBodyFlag.RBF_DOUBLED_KINEMATIC | RigidBodyFlag.RBF_KINEMATIC);
                            subpart2.Physics.IsPhantom = false;
                            HkMassProperties? massProperties = null;
                            (subpart2.Physics as MyPhysicsBody).CreateFromCollisionObject((HkShape) shape, Vector3.Zero, base.WorldMatrix, massProperties, 0x11);
                            subpart2.Physics.Enabled = true;
                            shape.Base.RemoveReference();
                        }
                    }
                    base.CubeGrid.OnHavokSystemIDChanged -= new Action<int>(this.CubeGrid_HavokSystemIDChanged);
                    base.CubeGrid.OnHavokSystemIDChanged += new Action<int>(this.CubeGrid_HavokSystemIDChanged);
                    base.CubeGrid.OnPhysicsChanged -= new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnPhysicsChanged);
                    base.CubeGrid.OnPhysicsChanged += new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnPhysicsChanged);
                    if (base.CubeGrid.Physics != null)
                    {
                        this.UpdateHavokCollisionSystemID(base.CubeGrid.GetPhysicsBody().HavokCollisionSystemID);
                    }
                }
            }
        }

        private void inventory_ContentsChanged(MyInventoryBase obj)
        {
            if (MySession.Static.CreativeMode)
            {
                this.CanDeploy = true;
            }
            else
            {
                this.m_requiredItemsInInventory = obj.GetItemAmount(this.BlockDefinition.MaterialDefinitionId, MyItemFlags.None, false);
                if (this.m_requiredItemsInInventory >= this.BlockDefinition.MaterialDeployCost)
                {
                    this.CanDeploy = true;
                }
                else
                {
                    this.CanDeploy = false;
                }
            }
        }

        private MyEntitySubpart LoadSubpartFromName(string name)
        {
            MyEntitySubpart subpart;
            if (!base.Subparts.TryGetValue(name, out subpart))
            {
                subpart = new MyEntitySubpart();
                string model = Path.Combine(Path.GetDirectoryName(base.Model.AssetName), name) + ".mwm";
                subpart.Render.EnableColorMaskHsv = base.Render.EnableColorMaskHsv;
                subpart.Render.ColorMaskHsv = base.Render.ColorMaskHsv;
                subpart.Render.TextureChanges = base.Render.TextureChanges;
                float? scale = null;
                subpart.Init(null, model, this, scale, null);
                base.Subparts[name] = subpart;
                if (base.InScene)
                {
                    subpart.OnAddedToScene(this);
                }
            }
            return subpart;
        }

        public override void OnBuildSuccess(long builtBy, bool instantBuild)
        {
            base.ResourceSink.Update();
            this.UpdateHavokCollisionSystemID(base.CubeGrid.GetPhysicsBody().HavokCollisionSystemID);
            base.OnBuildSuccess(builtBy, instantBuild);
        }

        public override void OnCubeGridChanged(MyCubeGrid oldGrid)
        {
            oldGrid.OnHavokSystemIDChanged -= new Action<int>(this.CubeGrid_HavokSystemIDChanged);
            base.CubeGrid.OnHavokSystemIDChanged += new Action<int>(this.CubeGrid_HavokSystemIDChanged);
            oldGrid.OnPhysicsChanged -= new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnPhysicsChanged);
            base.CubeGrid.OnPhysicsChanged += new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnPhysicsChanged);
            if (base.CubeGrid.Physics != null)
            {
                this.UpdateHavokCollisionSystemID(base.CubeGrid.GetPhysicsBody().HavokCollisionSystemID);
            }
            base.OnCubeGridChanged(oldGrid);
        }

        public override void OnDestroy()
        {
            base.ReleaseInventory(this.GetInventory(0), true);
            base.OnDestroy();
        }

        protected override void OnEnabledChanged()
        {
            base.ResourceSink.Update();
            base.OnEnabledChanged();
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            this.InitSubparts();
        }

        public override void OnRemovedByCubeBuilder()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnRemovedByCubeBuilder();
        }

        private void OnStateChange()
        {
            for (int i = 0; i < this.m_openingSequence.Count; i++)
            {
                float speed = this.m_openingSequence[i].Speed;
                this.m_currentSpeed[i] = (base.m_open != null) ? speed : -speed;
            }
            base.ResourceSink.Update();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds - 1;
            this.UpdateCurrentOpening();
            this.UpdateDoorPosition();
            if (base.m_open != null)
            {
                Action<bool> doorStateChanged = this.DoorStateChanged;
                if (doorStateChanged != null)
                {
                    doorStateChanged((bool) base.m_open);
                }
            }
            this.m_stateChange = true;
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
            this.UpdateEmissivity();
        }

        private void RemoveChute()
        {
            this.m_parachuteAnimationState = 0;
            if (this.m_parachuteSubpart != null)
            {
                this.m_parachuteSubpart.Render.Visible = false;
            }
        }

        public void SetAutoDeployRequest(bool autodeploy, long identityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyParachute, bool, long>(this, x => new Action<bool, long>(x.AutoDeployRequest), autodeploy, identityId, targetEndpoint);
        }

        public void SetDeployHeightRequest(float deployHeight, long identityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyParachute, float, long>(this, x => new Action<float, long>(x.DeployHeightRequest), deployHeight, identityId, targetEndpoint);
        }

        void SpaceEngineers.Game.ModAPI.Ingame.IMyParachute.CloseDoor()
        {
            if (base.IsWorking && ((((SpaceEngineers.Game.ModAPI.Ingame.IMyParachute) this).Status - 2) > DoorStatus.Open))
            {
                ((SpaceEngineers.Game.ModAPI.Ingame.IMyParachute) this).ToggleDoor();
            }
        }

        void SpaceEngineers.Game.ModAPI.Ingame.IMyParachute.OpenDoor()
        {
            if (base.IsWorking && (((SpaceEngineers.Game.ModAPI.Ingame.IMyParachute) this).Status > DoorStatus.Open))
            {
                ((SpaceEngineers.Game.ModAPI.Ingame.IMyParachute) this).ToggleDoor();
            }
        }

        void SpaceEngineers.Game.ModAPI.Ingame.IMyParachute.ToggleDoor()
        {
            if (base.IsWorking)
            {
                base.SetOpenRequest(!base.Open, base.OwnerId);
            }
        }

        private void StartSound(int emitterId, MySoundPair cuePair)
        {
            if (((this.m_emitter[emitterId].Sound == null) || !this.m_emitter[emitterId].Sound.IsPlaying) || ((this.m_emitter[emitterId].SoundId != cuePair.Arcade) && (this.m_emitter[emitterId].SoundId != cuePair.Realistic)))
            {
                this.m_emitter[emitterId].StopSound(true, true);
                bool? nullable = null;
                this.m_emitter[emitterId].PlaySingleSound(cuePair, false, false, false, nullable);
            }
        }

        public bool TryGetClosestPoint(out Vector3D? closestPoint)
        {
            closestPoint = 0;
            if (!MyGravityProviderSystem.IsPositionInNaturalGravity(base.PositionComp.GetPosition(), 0.0))
            {
                return false;
            }
            BoundingBoxD worldAABB = base.PositionComp.WorldAABB;
            this.m_nearPlanetCache = MyGamePruningStructure.GetClosestPlanet(ref worldAABB);
            if (this.m_nearPlanetCache == null)
            {
                return false;
            }
            Vector3D centerOfMassWorld = base.CubeGrid.Physics.CenterOfMassWorld;
            closestPoint = new Vector3D?(this.m_nearPlanetCache.GetClosestSurfacePointGlobal(ref centerOfMassWorld));
            return true;
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (base.CubeGrid.Physics != null)
            {
                this.m_atmosphereDirty = true;
                this.UpdateDoorPosition();
                this.UpdateParachutePosition();
            }
        }

        public override void UpdateBeforeSimulation()
        {
            this.m_atmosphereDirty = true;
            if (this.FullyClosed)
            {
                this.m_time = 0f;
                this.UpdateCutChute();
                if ((this.m_parachuteSubpart != null) && (this.m_parachuteSubpart.Render.RenderObjectIDs[0] != uint.MaxValue))
                {
                    this.m_parachuteSubpart.Render.Visible = false;
                }
            }
            else if (!this.FullyOpen)
            {
                this.UpdateParachute();
            }
            else
            {
                if (this.m_totalTime != this.m_time)
                {
                    this.m_totalTime = this.m_time;
                }
                this.m_time = this.m_totalTime;
                this.UpdateParachute();
            }
            if (Sync.IsServer && this.m_canCheckAutoDeploy)
            {
                this.CheckAutoDeploy();
            }
            for (int i = 0; i < this.m_openingSequence.Count; i++)
            {
                float maxOpen = this.m_openingSequence[i].MaxOpen;
                if ((base.Open && (this.m_currentOpening[i] == maxOpen)) || (!base.Open && (this.m_currentOpening[i] == 0f)))
                {
                    if (((this.m_emitter[i] != null) && this.m_emitter[i].IsPlaying) && this.m_emitter[i].Loop)
                    {
                        this.m_emitter[i].StopSound(false, true);
                    }
                    this.m_currentSpeed[i] = 0f;
                }
                if ((!base.Enabled || ((base.ResourceSink == null) || !base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))) || (this.m_currentSpeed[i] == 0f))
                {
                    if (this.m_emitter[i] != null)
                    {
                        this.m_emitter[i].StopSound(false, true);
                    }
                }
                else
                {
                    string str = "";
                    str = !base.Open ? this.m_openingSequence[i].CloseSound : this.m_openingSequence[i].OpenSound;
                    if (!string.IsNullOrEmpty(str))
                    {
                        this.StartSound(i, new MySoundPair(str, true));
                    }
                }
            }
            if (this.m_stateChange && (((base.m_open != null) && this.FullyOpen) || ((base.m_open == null) && this.FullyClosed)))
            {
                base.ResourceSink.Update();
                base.RaisePropertiesChanged();
                if (base.m_open == null)
                {
                    Action<bool> doorStateChanged = this.DoorStateChanged;
                    if (doorStateChanged != null)
                    {
                        doorStateChanged((bool) base.m_open);
                    }
                }
                this.m_stateChange = false;
            }
            base.UpdateBeforeSimulation();
            this.UpdateCurrentOpening();
            this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
        }

        public override void UpdateBeforeSimulation10()
        {
            if (base.CubeGrid.Physics != null)
            {
                this.m_gravityCache = (Vector3) this.GetTotalGravity();
                this.m_canCheckAutoDeploy = false;
                this.UpdateNearPlanet();
                if (!this.CanDeploy)
                {
                    this.AttemptPullRequiredInventoryItems();
                }
                if ((this.AutoDeploy && (this.CanDeploy && ((base.CubeGrid.Physics.LinearVelocity.LengthSquared() > 2f) && (this.Atmosphere > this.m_minAtmosphere)))) && (Vector3.Dot(this.m_gravityCache, base.CubeGrid.Physics.LinearVelocity) > 0.6f))
                {
                    this.m_canCheckAutoDeploy = this.TryGetClosestPoint(out this.m_closestPointCache);
                }
            }
            base.UpdateBeforeSimulation10();
        }

        private void UpdateCurrentOpening()
        {
            if ((base.Enabled && (base.ResourceSink != null)) && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
            {
                float num = ((float) (MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastUpdateTime)) / 1000f;
                this.m_time += (((float) (MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastUpdateTime)) / 1000f) * ((base.m_open != null) ? 1f : -1f);
                this.m_time = MathHelper.Clamp(this.m_time, 0f, this.m_totalTime);
                for (int i = 0; i < this.m_openingSequence.Count; i++)
                {
                    float num3 = (base.m_open != null) ? this.m_openingSequence[i].OpenDelay : this.m_openingSequence[i].CloseDelay;
                    if (((base.m_open != null) && (this.m_time > num3)) || ((base.m_open == null) && (this.m_time < (this.m_totalTime - num3))))
                    {
                        float num4 = this.m_currentSpeed[i] * num;
                        float maxOpen = this.m_openingSequence[i].MaxOpen;
                        if (this.m_openingSequence[i].SequenceType == MyObjectBuilder_ParachuteDefinition.Opening.Sequence.Linear)
                        {
                            this.m_currentOpening[i] = MathHelper.Clamp(this.m_currentOpening[i] + num4, 0f, maxOpen);
                        }
                    }
                }
            }
        }

        private void UpdateCutChute()
        {
            if ((base.CubeGrid.Physics != null) && (this.m_parachuteAnimationState != 0))
            {
                if (this.m_parachuteAnimationState > 100)
                {
                    this.RemoveChute();
                }
                else
                {
                    if (this.m_parachuteAnimationState < 50)
                    {
                        this.m_parachuteAnimationState = 50;
                    }
                    if ((this.m_parachuteAnimationState == 50) && (this.ParachuteStateChanged != null))
                    {
                        this.ParachuteStateChanged(false);
                    }
                    this.m_parachuteAnimationState++;
                    if (this.m_parachuteSubpart != null)
                    {
                        this.m_lastParachuteWorldMatrix.Translation += this.m_gravityCache * 0.05f;
                        Matrix localMatrix = (Matrix) (this.m_lastParachuteWorldMatrix * MatrixD.Invert(base.WorldMatrix));
                        this.m_parachuteSubpart.PositionComp.SetLocalMatrix(ref localMatrix, null, true);
                    }
                }
            }
        }

        private unsafe void UpdateDoorPosition()
        {
            if (base.CubeGrid.Physics != null)
            {
                for (int i = 0; i < this.m_subpartCount; i++)
                {
                    this.transMat[i] = Matrix.Identity;
                    this.rotMat[i] = Matrix.Identity;
                }
                int num2 = 0;
                while (true)
                {
                    if (num2 < this.m_sequenceCount)
                    {
                        MyObjectBuilder_ParachuteDefinition.Opening.MoveType move = this.m_openingSequence[num2].Move;
                        float num3 = this.m_currentOpening[num2];
                        int index = this.m_subpartIDs[num2];
                        if ((this.m_subparts.Count != 0) && (index >= 0))
                        {
                            if ((this.m_subparts[index] != null) && (this.m_subparts[index].Physics != null))
                            {
                                if (move == MyObjectBuilder_ParachuteDefinition.Opening.MoveType.Slide)
                                {
                                    Matrix* matrixPtr1 = (Matrix*) ref this.transMat[index];
                                    matrixPtr1[0] *= Matrix.CreateTranslation((Vector3) (this.m_openingSequence[num2].SlideDirection * new Vector3(num3)));
                                }
                                else if (move == MyObjectBuilder_ParachuteDefinition.Opening.MoveType.Rotate)
                                {
                                    float num5 = this.m_openingSequence[num2].InvertRotation ? -1f : 1f;
                                    float radians = 0f;
                                    float num7 = 0f;
                                    float num8 = 0f;
                                    if (this.m_openingSequence[num2].RotationAxis == MyObjectBuilder_ParachuteDefinition.Opening.Rotation.X)
                                    {
                                        radians = MathHelper.ToRadians((float) (num3 * num5));
                                    }
                                    else if (this.m_openingSequence[num2].RotationAxis == MyObjectBuilder_ParachuteDefinition.Opening.Rotation.Y)
                                    {
                                        num7 = MathHelper.ToRadians((float) (num3 * num5));
                                    }
                                    else if (this.m_openingSequence[num2].RotationAxis == MyObjectBuilder_ParachuteDefinition.Opening.Rotation.Z)
                                    {
                                        num8 = MathHelper.ToRadians((float) (num3 * num5));
                                    }
                                    Vector3 position = (this.m_openingSequence[num2].PivotPosition == null) ? this.m_hingePosition[index] : ((Vector3) this.m_openingSequence[num2].PivotPosition.Value);
                                    Matrix* matrixPtr2 = (Matrix*) ref this.rotMat[index];
                                    matrixPtr2[0] *= (Matrix.CreateTranslation(-position) * ((Matrix.CreateRotationX(radians) * Matrix.CreateRotationY(num7)) * Matrix.CreateRotationZ(num8))) * Matrix.CreateTranslation(position);
                                }
                                if (this.m_subparts[index].Physics.LinearVelocity != base.CubeGrid.Physics.LinearVelocity)
                                {
                                    this.m_subparts[index].Physics.LinearVelocity = base.CubeGrid.Physics.LinearVelocity;
                                }
                                if (this.m_subparts[index].Physics.AngularVelocity != base.CubeGrid.Physics.AngularVelocity)
                                {
                                    this.m_subparts[index].Physics.AngularVelocity = base.CubeGrid.Physics.AngularVelocity;
                                }
                            }
                            num2++;
                            continue;
                        }
                    }
                    for (int j = 0; j < this.m_subpartCount; j++)
                    {
                        this.m_subparts[j].PositionComp.LocalMatrix = this.rotMat[j] * this.transMat[j];
                    }
                    return;
                }
            }
        }

        private void UpdateEmissivity()
        {
            if ((!base.Enabled || (base.ResourceSink == null)) || !base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
            {
                UpdateEmissiveParts(base.Render.RenderObjectIDs[0], 0f, Color.Red, Color.White);
            }
            else
            {
                UpdateEmissiveParts(base.Render.RenderObjectIDs[0], 1f, Color.Green, Color.White);
                this.OnStateChange();
            }
        }

        internal void UpdateHavokCollisionSystemID(int HavokCollisionSystemID)
        {
            foreach (MyEntitySubpart subpart in this.m_subparts)
            {
                if (subpart == null)
                {
                    continue;
                }
                if ((subpart.Physics != null) && ((subpart.ModelCollision.HavokCollisionShapes != null) && (subpart.ModelCollision.HavokCollisionShapes.Length != 0)))
                {
                    uint info = HkGroupFilter.CalcFilterInfo(0x11, HavokCollisionSystemID, 1, 1);
                    subpart.Physics.RigidBody.SetCollisionFilterInfo(info);
                    info = HkGroupFilter.CalcFilterInfo(0x10, HavokCollisionSystemID, 1, 1);
                    subpart.Physics.RigidBody2.SetCollisionFilterInfo(info);
                    if (subpart.GetPhysicsBody().HavokWorld != null)
                    {
                        subpart.GetPhysicsBody().HavokWorld.RefreshCollisionFilterOnEntity(subpart.Physics.RigidBody);
                        subpart.GetPhysicsBody().HavokWorld.RefreshCollisionFilterOnEntity(subpart.Physics.RigidBody2);
                    }
                }
            }
        }

        private void UpdateNearPlanet()
        {
            BoundingBoxD worldAABB = base.PositionComp.WorldAABB;
            this.m_nearPlanetCache = MyGamePruningStructure.GetClosestPlanet(ref worldAABB);
        }

        public override void UpdateOnceBeforeFrame()
        {
            this.UpdateNearPlanet();
        }

        private void UpdateParachute()
        {
            if (base.CubeGrid.Physics != null)
            {
                if (this.m_parachuteAnimationState > 50)
                {
                    if ((!Sync.IsServer || (!this.CanDeploy || !this.FullyOpen)) || !this.CheckDeployChute())
                    {
                        this.UpdateCutChute();
                    }
                }
                else
                {
                    if (((this.m_parachuteAnimationState == 0) && (Sync.IsServer && this.CanDeploy)) && this.FullyOpen)
                    {
                        this.CheckDeployChute();
                    }
                    if ((this.m_parachuteAnimationState > 0) && (this.m_parachuteAnimationState < 50))
                    {
                        this.m_parachuteAnimationState++;
                    }
                    Vector3 zero = Vector3.Zero;
                    bool flag = false;
                    float num = base.CubeGrid.Physics.LinearVelocity.LengthSquared();
                    if (num > 2f)
                    {
                        zero = base.CubeGrid.Physics.LinearVelocity;
                        this.m_cutParachuteTimer = 0;
                    }
                    else if (0.1f <= num)
                    {
                        flag = true;
                        zero = base.CubeGrid.Physics.LinearVelocity;
                    }
                    else
                    {
                        flag = true;
                        if (Vector3.Distance(Vector3.Lerp(this.m_lastParachuteVelocityVector, -this.m_gravityCache, 0.05f), -this.m_gravityCache) < 0.05f)
                        {
                            this.m_cutParachuteTimer++;
                            if (this.m_cutParachuteTimer > 60)
                            {
                                if (Sync.IsServer)
                                {
                                    ((SpaceEngineers.Game.ModAPI.Ingame.IMyParachute) this).CloseDoor();
                                }
                                this.UpdateCutChute();
                                return;
                            }
                        }
                    }
                    double d = (10.0 * (this.Atmosphere - this.BlockDefinition.ReefAtmosphereLevel)) * (((double) this.m_parachuteAnimationState) / 50.0);
                    if ((d <= 0.5) || double.IsNaN(d))
                    {
                        d = 0.5;
                    }
                    else
                    {
                        d = Math.Log(d - 0.99) + 5.0;
                        if ((d < 0.5) || double.IsNaN(d))
                        {
                            d = 0.5;
                        }
                    }
                    this.m_chuteScale.Z = (Math.Log(((double) this.m_parachuteAnimationState) / 1.5) * base.CubeGrid.GridSize) * 20.0;
                    this.m_chuteScale.X = this.m_chuteScale.Y = (d * this.BlockDefinition.RadiusMultiplier) * base.CubeGrid.GridSize;
                    this.m_lastParachuteVelocityVector = zero;
                    Vector3D vectord = Vector3D.Normalize(zero);
                    Quaternion quaternion = Quaternion.CreateFromRotationMatrix(Matrix.CreateFromDir((Vector3) vectord, new Vector3(0f, 1f, 0f)).GetOrientation());
                    quaternion = Quaternion.Lerp(this.m_lastParachuteRotation, quaternion, 0.02f);
                    this.m_chuteScale = Vector3D.Lerp(this.m_lastParachuteScale, this.m_chuteScale, 0.02);
                    double num3 = this.m_chuteScale.X / 2.0;
                    this.m_lastParachuteScale = (Vector3) this.m_chuteScale;
                    this.m_lastParachuteRotation = quaternion;
                    MatrixD xd = MatrixD.Invert(base.WorldMatrix);
                    this.m_lastParachuteWorldMatrix = MatrixD.CreateFromTransformScale(this.m_lastParachuteRotation, base.WorldMatrix.Translation + (base.WorldMatrix.Up * (((double) base.CubeGrid.GridSize) / 2.0)), this.m_lastParachuteScale);
                    this.m_lastParachuteLocalMatrix = (Matrix) (this.m_lastParachuteWorldMatrix * xd);
                    if (!(!(num3 != 0.0) | flag) && (zero.LengthSquared() > 1f))
                    {
                        Vector3D vectord2 = -vectord;
                        double num4 = (3.1415926535897931 * num3) * num3;
                        double scaleFactor = (((2.5 * (this.Atmosphere * 1.225)) * zero.LengthSquared()) * num4) * this.DragCoefficient;
                        if ((scaleFactor > 0.0) && !base.CubeGrid.Physics.IsStatic)
                        {
                            float? maxSpeed = null;
                            base.CubeGrid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, new Vector3?((Vector3) Vector3D.Multiply(vectord2, scaleFactor)), new Vector3D?(base.WorldMatrix.Translation), new Vector3?(Vector3.Zero), maxSpeed, true, false);
                        }
                    }
                }
            }
        }

        private void UpdateParachutePosition()
        {
            if ((this.m_parachuteSubpart != null) && (this.m_parachuteAnimationState > 0))
            {
                this.m_parachuteSubpart.PositionComp.SetLocalMatrix(ref this.m_lastParachuteLocalMatrix, null, true);
            }
        }

        protected float UpdatePowerInput()
        {
            if (!base.Enabled || !base.IsFunctional)
            {
                return 0f;
            }
            return ((this.OpeningSpeed != 0f) ? this.BlockDefinition.PowerConsumptionMoving : this.BlockDefinition.PowerConsumptionIdle);
        }

        public override void UpdateSoundEmitters()
        {
            for (int i = 0; i < this.m_emitter.Count; i++)
            {
                if (this.m_emitter[i] != null)
                {
                    this.m_emitter[i].Update();
                }
            }
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            this.UpdateEmissivity();
        }

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_conveyorEndpoint;

        DoorStatus SpaceEngineers.Game.ModAPI.Ingame.IMyParachute.Status
        {
            get
            {
                float openRatio = this.OpenRatio;
                return ((base.m_open == null) ? ((openRatio < EPSILON) ? DoorStatus.Closed : DoorStatus.Closing) : (((1f - openRatio) < EPSILON) ? DoorStatus.Open : DoorStatus.Opening));
            }
        }

        public bool FullyClosed =>
            (this.m_currentOpening.FindAll(v => v > 0f).Count == 0);

        public bool FullyOpen
        {
            get
            {
                for (int i = 0; i < this.m_currentOpening.Count; i++)
                {
                    if (this.m_openingSequence[i].MaxOpen != this.m_currentOpening[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public float OpenRatio
        {
            get
            {
                for (int i = 0; i < this.m_currentOpening.Count; i++)
                {
                    if (this.m_currentOpening[i] > 0f)
                    {
                        return this.m_currentOpening[i];
                    }
                }
                return 0f;
            }
        }

        public float OpeningSpeed
        {
            get
            {
                for (int i = 0; i < this.m_currentSpeed.Count; i++)
                {
                    if (this.m_currentSpeed[i] > 0f)
                    {
                        return this.m_currentSpeed[i];
                    }
                }
                return 0f;
            }
        }

        public bool AutoDeploy
        {
            get => 
                ((bool) this.m_autoDeploy);
            set
            {
                if (this.m_autoDeploy != value)
                {
                    this.m_autoDeploy.Value = value;
                }
            }
        }

        public float DeployHeight
        {
            get => 
                ((float) this.m_deployHeight);
            set
            {
                float single1 = MathHelper.Clamp(value, 10f, 10000f);
                value = single1;
                if (this.m_deployHeight != value)
                {
                    this.m_deployHeight.Value = value;
                }
            }
        }

        public float DragCoefficient =>
            this.m_dragCoefficient;

        public bool CanDeploy
        {
            get => 
                this.m_canDeploy;
            set => 
                (this.m_canDeploy = value);
        }

        public float Atmosphere
        {
            get
            {
                float num;
                if (!this.m_atmosphereDirty)
                {
                    return this.m_atmosphereDensityCache;
                }
                this.m_atmosphereDirty = false;
                if (this.m_nearPlanetCache == null)
                {
                    this.m_atmosphereDensityCache = num = 0f;
                    return num;
                }
                this.m_atmosphereDensityCache = num = this.m_nearPlanetCache.GetAirDensity(base.WorldMatrix.Translation);
                return num;
            }
        }

        private MyParachuteDefinition BlockDefinition =>
            ((MyParachuteDefinition) base.BlockDefinition);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyParachute.<>c <>9 = new MyParachute.<>c();
            public static Predicate<float> <>9__49_0;
            public static MyTerminalValueControl<MyParachute, bool>.GetterDelegate <>9__77_0;
            public static MyTerminalValueControl<MyParachute, bool>.SetterDelegate <>9__77_1;
            public static MyTerminalValueControl<MyParachute, float>.GetterDelegate <>9__77_2;
            public static MyTerminalValueControl<MyParachute, float>.SetterDelegate <>9__77_3;
            public static MyTerminalControl<MyParachute>.WriterDelegate <>9__77_4;
            public static Func<MyParachute, Action<bool, long>> <>9__78_0;
            public static Func<MyParachute, Action<float, long>> <>9__80_0;
            public static Func<MyModelBone, bool> <>9__88_0;
            public static Func<MyParachute, Action> <>9__100_0;

            internal Action <CheckDeployChute>b__100_0(MyParachute x) => 
                new Action(x.DoDeployChute);

            internal bool <CreateTerminalControls>b__77_0(MyParachute x) => 
                x.AutoDeploy;

            internal void <CreateTerminalControls>b__77_1(MyParachute x, bool v)
            {
                x.SetAutoDeployRequest(v, x.OwnerId);
            }

            internal float <CreateTerminalControls>b__77_2(MyParachute x) => 
                x.DeployHeight;

            internal void <CreateTerminalControls>b__77_3(MyParachute x, float v)
            {
                x.SetDeployHeightRequest(v, x.OwnerId);
            }

            internal void <CreateTerminalControls>b__77_4(MyParachute b, StringBuilder v)
            {
                v.Append($"{b.DeployHeight:N0} m");
            }

            internal bool <get_FullyClosed>b__49_0(float v) => 
                (v > 0f);

            internal bool <InitSubparts>b__88_0(MyModelBone b) => 
                !b.Name.Contains("Root");

            internal Action<bool, long> <SetAutoDeployRequest>b__78_0(MyParachute x) => 
                new Action<bool, long>(x.AutoDeployRequest);

            internal Action<float, long> <SetDeployHeightRequest>b__80_0(MyParachute x) => 
                new Action<float, long>(x.DeployHeightRequest);
        }
    }
}

