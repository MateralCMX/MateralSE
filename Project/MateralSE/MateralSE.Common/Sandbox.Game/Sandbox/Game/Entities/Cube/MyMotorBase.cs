namespace Sandbox.Game.Entities.Cube
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Multiplayer;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRageMath;

    [MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyMotorBase), typeof(Sandbox.ModAPI.Ingame.IMyMotorBase) })]
    public abstract class MyMotorBase : MyMechanicalConnectionBlockBase, Sandbox.ModAPI.IMyMotorBase, Sandbox.ModAPI.IMyMechanicalConnectionBlock, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyMechanicalConnectionBlock, Sandbox.ModAPI.Ingame.IMyMotorBase
    {
        private const string ROTOR_DUMMY_KEY = "electric_motor";
        private static List<HkBodyCollision> m_penetrations = new List<HkBodyCollision>();
        private Vector3 m_dummyPos;
        protected readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_dummyDisplacement;

        event Action<Sandbox.ModAPI.IMyMotorBase> Sandbox.ModAPI.IMyMotorBase.AttachedEntityChanged
        {
            add
            {
                base.AttachedEntityChanged += this.GetDelegate(value);
            }
            remove
            {
                base.AttachedEntityChanged -= this.GetDelegate(value);
            }
        }

        protected MyMotorBase()
        {
        }

        protected void CheckDisplacementLimits()
        {
            if (base.TopGrid != null)
            {
                if (base.TopGrid.GridSizeEnum != MyCubeSize.Small)
                {
                    if (this.DummyDisplacement < this.MotorDefinition.RotorDisplacementMin)
                    {
                        this.DummyDisplacement = this.MotorDefinition.RotorDisplacementMin;
                    }
                    if (this.DummyDisplacement > this.MotorDefinition.RotorDisplacementMax)
                    {
                        this.DummyDisplacement = this.MotorDefinition.RotorDisplacementMax;
                    }
                }
                else
                {
                    if (this.DummyDisplacement < this.MotorDefinition.RotorDisplacementMinSmall)
                    {
                        this.DummyDisplacement = this.MotorDefinition.RotorDisplacementMinSmall;
                    }
                    if (this.DummyDisplacement > this.MotorDefinition.RotorDisplacementMaxSmall)
                    {
                        this.DummyDisplacement = this.MotorDefinition.RotorDisplacementMaxSmall;
                    }
                }
            }
        }

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
        }

        protected virtual float ComputeRequiredPowerInput() => 
            (base.CheckIsWorking() ? base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId) : 0f);

        public override void ComputeTopQueryBox(out Vector3D pos, out Vector3 halfExtents, out Quaternion orientation)
        {
            MatrixD worldMatrix = base.WorldMatrix;
            orientation = Quaternion.CreateFromRotationMatrix(worldMatrix);
            halfExtents = (Vector3.One * base.CubeGrid.GridSize) * 0.35f;
            halfExtents.Y = base.CubeGrid.GridSize * 0.25f;
            pos = worldMatrix.Translation + ((0.35f * base.CubeGrid.GridSize) * base.WorldMatrix.Up);
        }

        protected override void DisposeConstraint(MyCubeGrid topGrid)
        {
            if (base.m_constraint != null)
            {
                base.CubeGrid.Physics.RemoveConstraint(base.m_constraint);
                base.m_constraint.Dispose();
                base.m_constraint = null;
            }
            base.DisposeConstraint(topGrid);
        }

        private Action<MyMechanicalConnectionBlockBase> GetDelegate(Action<Sandbox.ModAPI.IMyMotorBase> value) => 
            ((Action<MyMechanicalConnectionBlockBase>) Delegate.CreateDelegate(typeof(Action<MyMechanicalConnectionBlockBase>), value.Target, value.Method));

        protected override MatrixD GetTopGridMatrix() => 
            MatrixD.CreateWorld(Vector3D.Transform(this.DummyPosition, base.CubeGrid.WorldMatrix), base.WorldMatrix.Forward, base.WorldMatrix.Up);

        public override unsafe void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.SyncFlag = true;
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(this.MotorDefinition.ResourceSinkGroup, this.MotorDefinition.RequiredPowerInput, new Func<float>(this.ComputeRequiredPowerInput));
            component.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.ResourceSink = component;
            base.Init(objectBuilder, cubeGrid);
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.ResourceSink.Update();
            this.m_dummyDisplacement.SetLocalValue(0f);
            this.m_dummyDisplacement.ValueChanged += new Action<SyncBase>(this.m_dummyDisplacement_ValueChanged);
            this.LoadDummyPosition();
            MyObjectBuilder_MotorBase base2 = objectBuilder as MyObjectBuilder_MotorBase;
            if ((Sync.IsServer && (base2.RotorEntityId != null)) && (base2.RotorEntityId.Value != 0))
            {
                MyMechanicalConnectionBlockBase.State* statePtr1;
                MyMechanicalConnectionBlockBase.State state = new MyMechanicalConnectionBlockBase.State {
                    TopBlockId = base2.RotorEntityId
                };
                statePtr1->Welded = (base2.WeldedEntityId != null) || base2.ForceWeld;
                statePtr1 = (MyMechanicalConnectionBlockBase.State*) ref state;
                base.m_connectionState.Value = state;
            }
            base.AddDebugRenderComponent(new MyDebugRenderComponentMotorBase(this));
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        private void LoadDummyPosition()
        {
            foreach (KeyValuePair<string, MyModelDummy> pair in MyModels.GetModelOnlyDummies(base.BlockDefinition.Model).Dummies)
            {
                if (pair.Key.StartsWith("electric_motor", StringComparison.InvariantCultureIgnoreCase))
                {
                    Matrix matrix = Matrix.Normalize(pair.Value.Matrix);
                    this.m_dummyPos = matrix.Translation;
                    break;
                }
            }
        }

        private void m_dummyDisplacement_ValueChanged(SyncBase obj)
        {
            if (Sync.IsServer)
            {
                this.CheckDisplacementLimits();
            }
            if (base.m_constraint != null)
            {
                base.CubeGrid.Physics.RigidBody.Activate();
                if (base.TopGrid != null)
                {
                    base.TopGrid.Physics.RigidBody.Activate();
                }
            }
        }

        protected override void OnEnabledChanged()
        {
            base.ResourceSink.Update();
            base.OnEnabledChanged();
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
        }

        void Sandbox.ModAPI.IMyMotorBase.Attach(Sandbox.ModAPI.IMyMotorRotor rotor, bool updateGroup)
        {
            ((Sandbox.ModAPI.IMyMechanicalConnectionBlock) this).Attach(rotor, updateGroup);
        }

        protected override Vector3D TransformPosition(ref Vector3D position) => 
            Vector3D.Transform(this.DummyPosition, base.CubeGrid.WorldMatrix);

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            this.UpdateSoundState();
        }

        protected virtual void UpdateSoundState()
        {
            if ((MySandboxGame.IsGameReady && (base.m_soundEmitter != null)) && base.IsWorking)
            {
                if ((base.TopGrid == null) || (base.TopGrid.Physics == null))
                {
                    base.m_soundEmitter.StopSound(true, true);
                }
                else
                {
                    if (!base.IsWorking || (Math.Abs(base.TopGrid.Physics.RigidBody.DeltaAngle.W) <= 0.00025f))
                    {
                        base.m_soundEmitter.StopSound(false, true);
                    }
                    else
                    {
                        bool? nullable = null;
                        base.m_soundEmitter.PlaySingleSound(base.BlockDefinition.PrimarySound, true, false, false, nullable);
                    }
                    if ((base.m_soundEmitter.Sound != null) && base.m_soundEmitter.Sound.IsPlaying)
                    {
                        float semitones = (4f * (Math.Abs(this.RotorAngularVelocity.Length()) - (0.5f * this.MaxRotorAngularVelocity))) / this.MaxRotorAngularVelocity;
                        base.m_soundEmitter.Sound.FrequencyRatio = MyAudio.Static.SemitonesToFrequencyRatio(semitones);
                    }
                }
            }
        }

        public Vector3 DummyPosition
        {
            get
            {
                Vector3 zero = Vector3.Zero;
                if (this.m_dummyPos.Length() <= 0f)
                {
                    zero = new Vector3(0f, (float) this.m_dummyDisplacement, 0f);
                }
                else
                {
                    zero = Vector3.DominantAxisProjection(this.m_dummyPos);
                    zero.Normalize();
                    zero *= this.m_dummyDisplacement;
                }
                return Vector3.Transform(this.m_dummyPos + zero, base.PositionComp.LocalMatrix);
            }
        }

        public float DummyDisplacement
        {
            get => 
                (((float) this.m_dummyDisplacement) + this.ModelDummyDisplacement);
            set
            {
                if (!this.m_dummyDisplacement.Value.IsEqual((value - this.ModelDummyDisplacement), 0.0001f))
                {
                    this.m_dummyDisplacement.Value = value - this.ModelDummyDisplacement;
                }
            }
        }

        public MyCubeGrid RotorGrid =>
            base.TopGrid;

        public MyCubeBlock Rotor =>
            base.TopBlock;

        public float RequiredPowerInput =>
            this.MotorDefinition.RequiredPowerInput;

        protected MyMotorStatorDefinition MotorDefinition =>
            ((MyMotorStatorDefinition) base.BlockDefinition);

        protected virtual float ModelDummyDisplacement =>
            0f;

        public Vector3 RotorAngularVelocity =>
            (base.CubeGrid.Physics.RigidBody.AngularVelocity - base.TopGrid.Physics.RigidBody.AngularVelocity);

        public Vector3 AngularVelocity
        {
            get => 
                base.TopGrid.Physics.RigidBody.AngularVelocity;
            set => 
                (base.TopGrid.Physics.RigidBody.AngularVelocity = value);
        }

        public float MaxRotorAngularVelocity =>
            MyGridPhysics.GetShipMaxAngularVelocity(base.CubeGrid.GridSizeEnum);

        VRage.Game.ModAPI.IMyCubeGrid Sandbox.ModAPI.IMyMotorBase.RotorGrid =>
            base.TopGrid;

        VRage.Game.ModAPI.IMyCubeBlock Sandbox.ModAPI.IMyMotorBase.Rotor =>
            base.TopBlock;
    }
}

