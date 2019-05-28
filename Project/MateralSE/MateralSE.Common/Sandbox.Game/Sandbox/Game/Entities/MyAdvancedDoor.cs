namespace Sandbox.Game.Entities
{
    using Havok;
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Import;

    [MyCubeBlockType(typeof(MyObjectBuilder_AdvancedDoor)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyAdvancedDoor), typeof(Sandbox.ModAPI.Ingame.IMyAdvancedDoor) })]
    public class MyAdvancedDoor : MyDoorBase, Sandbox.ModAPI.IMyAdvancedDoor, Sandbox.ModAPI.IMyDoor, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyDoor, Sandbox.ModAPI.Ingame.IMyAdvancedDoor
    {
        private const float CLOSED_DISSASEMBLE_RATIO = 3.3f;
        private static readonly float EPSILON = 1E-09f;
        private int m_lastUpdateTime;
        private float m_time;
        private float m_totalTime = 99999f;
        private bool m_stateChange;
        private readonly List<MyEntitySubpart> m_subparts = new List<MyEntitySubpart>();
        private readonly List<int> m_subpartIDs = new List<int>();
        private readonly List<float> m_currentOpening = new List<float>();
        private readonly List<float> m_currentSpeed = new List<float>();
        private readonly List<MyEntity3DSoundEmitter> m_emitter = new List<MyEntity3DSoundEmitter>();
        private readonly List<Vector3> m_hingePosition = new List<Vector3>();
        private readonly List<MyObjectBuilder_AdvancedDoorDefinition.Opening> m_openingSequence = new List<MyObjectBuilder_AdvancedDoorDefinition.Opening>();
        private Matrix[] transMat = new Matrix[1];
        private Matrix[] rotMat = new Matrix[1];
        private int m_sequenceCount;
        private int m_subpartCount;
        [CompilerGenerated]
        private Action<bool> DoorStateChanged;
        [CompilerGenerated]
        private Action<Sandbox.ModAPI.IMyDoor, bool> OnDoorStateChanged;

        public event Action<bool> DoorStateChanged
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

        public event Action<Sandbox.ModAPI.IMyDoor, bool> OnDoorStateChanged
        {
            [CompilerGenerated] add
            {
                Action<Sandbox.ModAPI.IMyDoor, bool> onDoorStateChanged = this.OnDoorStateChanged;
                while (true)
                {
                    Action<Sandbox.ModAPI.IMyDoor, bool> a = onDoorStateChanged;
                    Action<Sandbox.ModAPI.IMyDoor, bool> action3 = (Action<Sandbox.ModAPI.IMyDoor, bool>) Delegate.Combine(a, value);
                    onDoorStateChanged = Interlocked.CompareExchange<Action<Sandbox.ModAPI.IMyDoor, bool>>(ref this.OnDoorStateChanged, action3, a);
                    if (ReferenceEquals(onDoorStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<Sandbox.ModAPI.IMyDoor, bool> onDoorStateChanged = this.OnDoorStateChanged;
                while (true)
                {
                    Action<Sandbox.ModAPI.IMyDoor, bool> source = onDoorStateChanged;
                    Action<Sandbox.ModAPI.IMyDoor, bool> action3 = (Action<Sandbox.ModAPI.IMyDoor, bool>) Delegate.Remove(source, value);
                    onDoorStateChanged = Interlocked.CompareExchange<Action<Sandbox.ModAPI.IMyDoor, bool>>(ref this.OnDoorStateChanged, action3, source);
                    if (ReferenceEquals(onDoorStateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyAdvancedDoor()
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

        private static void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyAdvancedDoor>())
            {
                MyStringId tooltip = new MyStringId();
                MyTerminalControlOnOffSwitch<MyAdvancedDoor> switch1 = new MyTerminalControlOnOffSwitch<MyAdvancedDoor>("Open", MySpaceTexts.Blank, tooltip, new MyStringId?(MySpaceTexts.BlockAction_DoorOpen), new MyStringId?(MySpaceTexts.BlockAction_DoorClosed));
                MyTerminalControlOnOffSwitch<MyAdvancedDoor> switch2 = new MyTerminalControlOnOffSwitch<MyAdvancedDoor>("Open", MySpaceTexts.Blank, tooltip, new MyStringId?(MySpaceTexts.BlockAction_DoorOpen), new MyStringId?(MySpaceTexts.BlockAction_DoorClosed));
                switch2.Getter = x => x.Open;
                MyTerminalControlOnOffSwitch<MyAdvancedDoor> local4 = switch2;
                MyTerminalControlOnOffSwitch<MyAdvancedDoor> local5 = switch2;
                local5.Setter = (x, v) => x.SetOpenRequest(v, x.OwnerId);
                MyTerminalControlOnOffSwitch<MyAdvancedDoor> onOff = local5;
                onOff.EnableToggleAction<MyAdvancedDoor>();
                onOff.EnableOnOffActions<MyAdvancedDoor>();
                MyTerminalControlFactory.AddControl<MyAdvancedDoor>(onOff);
            }
        }

        private void CubeGrid_HavokSystemIDChanged(int id)
        {
            this.UpdateHavokCollisionSystemID(id);
        }

        private void CubeGrid_OnPhysicsChanged(VRage.Game.Entity.MyEntity obj)
        {
            if (((this.m_subparts != null) && (this.m_subparts.Count != 0)) && ((obj.Physics != null) && (this.m_subparts[0].Physics != null)))
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_AdvancedDoor objectBuilderCubeBlock = (MyObjectBuilder_AdvancedDoor) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.Open = (bool) base.m_open;
            return objectBuilderCubeBlock;
        }

        public override void Init(MyObjectBuilder_CubeBlock builder, MyCubeGrid cubeGrid)
        {
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(this.BlockDefinition.ResourceSinkGroup, this.BlockDefinition.PowerConsumptionMoving, new Func<float>(this.UpdatePowerInput));
            base.ResourceSink = component;
            base.Init(builder, cubeGrid);
            MyObjectBuilder_AdvancedDoor door = (MyObjectBuilder_AdvancedDoor) builder;
            base.m_open.SetLocalValue(door.Open);
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
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.ResourceSink.Update();
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
                        MyObjectBuilder_AdvancedDoorDefinition.Opening local2 = this.m_openingSequence[k];
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
                        if (subpart2.Physics != null)
                        {
                            subpart2.Physics.Close();
                            subpart2.Physics = null;
                        }
                        if (((subpart2 != null) && ((subpart2.Physics == null) && (subpart2.ModelCollision.HavokCollisionShapes != null))) && (subpart2.ModelCollision.HavokCollisionShapes.Length != 0))
                        {
                            List<HkShape> list = subpart2.ModelCollision.HavokCollisionShapes.ToList<HkShape>();
                            HkListShape shape = new HkListShape(list.GetInternalArray<HkShape>(), list.Count, HkReferencePolicy.None);
                            subpart2.Physics = new MyPhysicsBody(subpart2, RigidBodyFlag.RBF_UNLOCKED_SPEEDS | RigidBodyFlag.RBF_KINEMATIC);
                            subpart2.Physics.IsPhantom = false;
                            HkMassProperties? massProperties = null;
                            (subpart2.Physics as MyPhysicsBody).CreateFromCollisionObject((HkShape) shape, Vector3.Zero, base.WorldMatrix, massProperties, 15);
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

        private MyEntitySubpart LoadSubpartFromName(string name)
        {
            MyEntitySubpart subpart;
            base.Subparts.TryGetValue(name, out subpart);
            if (subpart == null)
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
            if (base.CubeGrid.Physics != null)
            {
                this.UpdateHavokCollisionSystemID(base.CubeGrid.GetPhysicsBody().HavokCollisionSystemID);
            }
            base.OnCubeGridChanged(oldGrid);
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

        private void OnStateChange()
        {
            for (int i = 0; i < this.m_openingSequence.Count; i++)
            {
                float speed = this.m_openingSequence[i].Speed;
                this.m_currentSpeed[i] = (base.m_open != null) ? speed : -speed;
            }
            base.ResourceSink.Update();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds - 1;
            this.UpdateCurrentOpening();
            this.UpdateDoorPosition();
            if (base.m_open != null)
            {
                this.DoorStateChanged.InvokeIfNotNull<bool>((bool) base.m_open);
                this.OnDoorStateChanged.InvokeIfNotNull<Sandbox.ModAPI.IMyDoor, bool>(this, (bool) base.m_open);
            }
            this.m_stateChange = true;
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
            this.UpdateEmissivity();
        }

        void Sandbox.ModAPI.Ingame.IMyDoor.CloseDoor()
        {
            if (base.IsWorking && ((((Sandbox.ModAPI.Ingame.IMyDoor) this).Status - 2) > DoorStatus.Open))
            {
                ((Sandbox.ModAPI.Ingame.IMyDoor) this).ToggleDoor();
            }
        }

        void Sandbox.ModAPI.Ingame.IMyDoor.OpenDoor()
        {
            if (base.IsWorking && (((Sandbox.ModAPI.Ingame.IMyDoor) this).Status > DoorStatus.Open))
            {
                ((Sandbox.ModAPI.Ingame.IMyDoor) this).ToggleDoor();
            }
        }

        void Sandbox.ModAPI.Ingame.IMyDoor.ToggleDoor()
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

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (base.CubeGrid.Physics != null)
            {
                this.UpdateDoorPosition();
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if (this.FullyClosed)
            {
                this.m_time = 0f;
            }
            else if (this.FullyOpen)
            {
                if (this.m_totalTime != this.m_time)
                {
                    this.m_totalTime = this.m_time;
                }
                this.m_time = this.m_totalTime;
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
                    this.DoorStateChanged.InvokeIfNotNull<bool>((bool) base.m_open);
                    this.OnDoorStateChanged.InvokeIfNotNull<Sandbox.ModAPI.IMyDoor, bool>(this, (bool) base.m_open);
                }
                this.m_stateChange = false;
            }
            base.UpdateBeforeSimulation();
            this.UpdateCurrentOpening();
            this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
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
                        if (this.m_openingSequence[i].SequenceType == MyObjectBuilder_AdvancedDoorDefinition.Opening.Sequence.Linear)
                        {
                            this.m_currentOpening[i] = MathHelper.Clamp(this.m_currentOpening[i] + num4, 0f, maxOpen);
                        }
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
                        MyObjectBuilder_AdvancedDoorDefinition.Opening.MoveType move = this.m_openingSequence[num2].Move;
                        float num3 = this.m_currentOpening[num2];
                        int index = this.m_subpartIDs[num2];
                        if ((this.m_subparts.Count != 0) && (index >= 0))
                        {
                            if ((this.m_subparts[index] != null) && (this.m_subparts[index].Physics != null))
                            {
                                if (move == MyObjectBuilder_AdvancedDoorDefinition.Opening.MoveType.Slide)
                                {
                                    Matrix* matrixPtr1 = (Matrix*) ref this.transMat[index];
                                    matrixPtr1[0] *= Matrix.CreateTranslation((Vector3) (this.m_openingSequence[num2].SlideDirection * new Vector3(num3)));
                                }
                                else if (move == MyObjectBuilder_AdvancedDoorDefinition.Opening.MoveType.Rotate)
                                {
                                    float num5 = this.m_openingSequence[num2].InvertRotation ? -1f : 1f;
                                    float radians = 0f;
                                    float num7 = 0f;
                                    float num8 = 0f;
                                    if (this.m_openingSequence[num2].RotationAxis == MyObjectBuilder_AdvancedDoorDefinition.Opening.Rotation.X)
                                    {
                                        radians = MathHelper.ToRadians((float) (num3 * num5));
                                    }
                                    else if (this.m_openingSequence[num2].RotationAxis == MyObjectBuilder_AdvancedDoorDefinition.Opening.Rotation.Y)
                                    {
                                        num7 = MathHelper.ToRadians((float) (num3 * num5));
                                    }
                                    else if (this.m_openingSequence[num2].RotationAxis == MyObjectBuilder_AdvancedDoorDefinition.Opening.Rotation.Z)
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
                    uint info = HkGroupFilter.CalcFilterInfo(15, HavokCollisionSystemID, 1, 1);
                    subpart.Physics.RigidBody.SetCollisionFilterInfo(info);
                    if (subpart.GetPhysicsBody().HavokWorld != null)
                    {
                        subpart.GetPhysicsBody().HavokWorld.RefreshCollisionFilterOnEntity(subpart.Physics.RigidBody);
                    }
                }
            }
        }

        private void UpdateHavokCollisionSystemID(VRage.Game.Entity.MyEntity obj)
        {
            if ((((obj != null) && (!obj.MarkedForClose && (obj.GetPhysicsBody() != null))) && (this.m_subparts[0].GetPhysicsBody() != null)) && (obj.GetPhysicsBody().HavokCollisionSystemID != this.m_subparts[0].GetPhysicsBody().HavokCollisionSystemID))
            {
                this.UpdateHavokCollisionSystemID(obj.GetPhysicsBody().HavokCollisionSystemID);
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            this.UpdateHavokCollisionSystemID(base.CubeGrid);
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

        public override float DisassembleRatio =>
            (base.DisassembleRatio * (base.Open ? 1f : 3.3f));

        DoorStatus Sandbox.ModAPI.Ingame.IMyDoor.Status
        {
            get
            {
                float openRatio = this.OpenRatio;
                return ((base.m_open == null) ? ((openRatio < EPSILON) ? DoorStatus.Closed : DoorStatus.Closing) : (((1f - openRatio) < EPSILON) ? DoorStatus.Open : DoorStatus.Opening));
            }
        }

        bool Sandbox.ModAPI.IMyDoor.IsFullyClosed =>
            this.FullyClosed;

        [Obsolete("Use Sandbox.ModAPI.IMyDoor.IsFullyClosed")]
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

        private MyAdvancedDoorDefinition BlockDefinition =>
            ((MyAdvancedDoorDefinition) base.BlockDefinition);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyAdvancedDoor.<>c <>9 = new MyAdvancedDoor.<>c();
            public static Predicate<float> <>9__34_0;
            public static MyTerminalValueControl<MyAdvancedDoor, bool>.GetterDelegate <>9__46_0;
            public static MyTerminalValueControl<MyAdvancedDoor, bool>.SetterDelegate <>9__46_1;
            public static Func<MyModelBone, bool> <>9__52_0;

            internal bool <CreateTerminalControls>b__46_0(MyAdvancedDoor x) => 
                x.Open;

            internal void <CreateTerminalControls>b__46_1(MyAdvancedDoor x, bool v)
            {
                x.SetOpenRequest(v, x.OwnerId);
            }

            internal bool <get_FullyClosed>b__34_0(float v) => 
                (v > 0f);

            internal bool <InitSubparts>b__52_0(MyModelBone b) => 
                !b.Name.Contains("Root");
        }
    }
}

