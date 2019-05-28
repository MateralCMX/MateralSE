namespace Sandbox.Game.Entities.Blocks
{
    using Havok;
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents.Renders;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Utils;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyCubeBlockType(typeof(MyObjectBuilder_Wheel))]
    public class MyWheel : MyMotorRotor, Sandbox.ModAPI.IMyWheel, Sandbox.ModAPI.IMyMotorRotor, Sandbox.ModAPI.IMyAttachableTopBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyAttachableTopBlock, Sandbox.ModAPI.Ingame.IMyMotorRotor, Sandbox.ModAPI.Ingame.IMyWheel
    {
        private readonly MyStringHash m_wheelStringHash = MyStringHash.GetOrCompute("Wheel");
        private MyWheelModelsDefinition m_cachedModelsDefinition;
        public Vector3 LastUsedGroundNormal;
        private int m_modelSwapCountUp;
        private bool m_usesAlternativeModel;
        public bool m_isSuspensionMounted;
        private int m_slipCountdown;
        private int m_staticHitCount;
        private int m_contactCountdown;
        private float m_frictionCollector;
        private Vector3 m_lastFrameImpuse;
        private ConcurrentNormalAggregator m_contactNormals = new ConcurrentNormalAggregator(10);
        private readonly VRage.Sync.Sync<ParticleData, SyncDirection.FromServer> m_particleData;
        private static Dictionary<MyCubeGrid, Queue<MyTuple<DateTime, string>>> activityLog = new Dictionary<MyCubeGrid, Queue<MyTuple<DateTime, string>>>();
        private bool m_eachUpdateCallbackRegistered;

        public MyWheel()
        {
            this.Friction = 1.5f;
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.MyWheel_IsWorkingChanged);
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                this.Render = new MyRenderComponentWheel();
            }
        }

        public override void Attach(MyMechanicalConnectionBlockBase parent)
        {
            base.Attach(parent);
            this.m_isSuspensionMounted = base.Stator is MyMotorSuspension;
        }

        public override string CalculateCurrentModel(out Matrix orientation)
        {
            string str = base.CalculateCurrentModel(out orientation);
            if (base.CubeGrid.Physics == null)
            {
                return str;
            }
            if ((base.Stator == null) || !base.IsFunctional)
            {
                return str;
            }
            return (!this.m_usesAlternativeModel ? str : this.WheelModelsDefinition.AlternativeModel);
        }

        public override void ContactPointCallback(ref MyGridContactInfo value)
        {
            Vector3 normal = value.Event.ContactPoint.Normal;
            this.m_contactNormals.PushNext(ref normal);
            MyVoxelMaterialDefinition voxelSurfaceMaterial = value.VoxelSurfaceMaterial;
            if (voxelSurfaceMaterial != null)
            {
                this.m_frictionCollector = voxelSurfaceMaterial.Friction;
            }
            float friction = this.Friction;
            if ((this.m_isSuspensionMounted && ((value.CollidingEntity is MyCubeGrid) && (value.OtherBlock != null))) && (value.OtherBlock.FatBlock == null))
            {
                friction *= 0.07f;
                this.m_frictionCollector = 0.7f;
            }
            HkContactPointProperties contactProperties = value.Event.ContactProperties;
            contactProperties.Friction = friction;
            contactProperties.Restitution = 0.5f;
            value.EnableParticles = false;
            value.RubberDeformation = true;
            ulong simulationFrameCounter = MySandboxGame.Static.SimulationFrameCounter;
            if (simulationFrameCounter != this.LastContactFrameNumber)
            {
                Vector3 vector2;
                this.LastContactFrameNumber = simulationFrameCounter;
                Vector3D contactPosition = value.ContactPosition;
                if (this.m_contactNormals.GetAvgNormalCached(out vector2))
                {
                    normal = vector2;
                }
                string particleName = null;
                if (!(value.CollidingEntity is MyVoxelBase) || !MyFakes.ENABLE_DRIVING_PARTICLES)
                {
                    if ((value.CollidingEntity is MyCubeGrid) && MyFakes.ENABLE_DRIVING_PARTICLES)
                    {
                        MyStringHash materialAt = (value.CollidingEntity as MyCubeGrid).Physics.GetMaterialAt(contactPosition);
                        particleName = MyMaterialPropertiesHelper.Static.GetCollisionEffect(MyMaterialPropertiesHelper.CollisionType.Start, this.m_wheelStringHash, materialAt);
                    }
                }
                else if (voxelSurfaceMaterial != null)
                {
                    MyStringHash materialTypeNameHash = voxelSurfaceMaterial.MaterialTypeNameHash;
                    particleName = MyMaterialPropertiesHelper.Static.GetCollisionEffect(MyMaterialPropertiesHelper.CollisionType.Start, this.m_wheelStringHash, materialTypeNameHash);
                }
                if (this.Render != null)
                {
                    if (particleName != null)
                    {
                        this.Render.TrySpawnParticle(particleName, ref contactPosition, ref normal);
                    }
                    this.Render.UpdateParticle(ref contactPosition, ref normal);
                }
                if (((particleName != null) && Sync.IsServer) && (MySession.Static.Settings.OnlineMode != MyOnlineModeEnum.OFFLINE))
                {
                    ParticleData data = new ParticleData {
                        EffectName = particleName,
                        PositionRelative = (Vector3) (contactPosition - base.PositionComp.WorldMatrix.Translation),
                        Normal = value.Event.ContactPoint.Normal
                    };
                    this.m_particleData.Value = data;
                }
                this.RegisterPerFrameUpdate();
            }
        }

        public override void Detach(bool isWelding)
        {
            this.m_isSuspensionMounted = false;
            base.Detach(isWelding);
        }

        public static void DumpActivityLog()
        {
            Dictionary<MyCubeGrid, Queue<MyTuple<DateTime, string>>> activityLog = MyWheel.activityLog;
            lock (activityLog)
            {
                foreach (KeyValuePair<MyCubeGrid, Queue<MyTuple<DateTime, string>>> pair in MyWheel.activityLog)
                {
                    MyCubeGrid key = pair.Key;
                    MyLog.Default.WriteLine("GRID: " + key.DisplayName);
                    foreach (MyTuple<DateTime, string> tuple in pair.Value)
                    {
                        DateTime time = tuple.Item1;
                        MyLog.Default.WriteLine("[" + time.ToString("dd/MM hh:mm:ss:FFF") + "] " + tuple.Item2);
                    }
                    MyLog.Default.Flush();
                }
                MyWheel.activityLog.Clear();
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_CubeBlock objectBuilderCubeBlock = base.GetObjectBuilderCubeBlock(copy);
            MyObjectBuilder_Wheel wheel = objectBuilderCubeBlock as MyObjectBuilder_Wheel;
            if (wheel != null)
            {
                wheel.YieldLastComponent = base.SlimBlock.YieldLastComponent;
            }
            return objectBuilderCubeBlock;
        }

        private float GetObserverAngularVelocityDiff()
        {
            MyGridPhysics physics = base.CubeGrid.Physics;
            if ((physics != null) && (physics.LinearVelocity.LengthSquared() > 16f))
            {
                IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
                if (controlledEntity != null)
                {
                    VRage.Game.Entity.MyEntity entity = controlledEntity.Entity;
                    if (entity != null)
                    {
                        MyPhysicsComponentBase base2 = entity.GetTopMostParent(null).Physics;
                        if (base2 != null)
                        {
                            return (physics.AngularVelocity - base2.AngularVelocity).Length();
                        }
                    }
                }
            }
            return 0f;
        }

        public override void Init(MyObjectBuilder_CubeBlock builder, MyCubeGrid cubeGrid)
        {
            base.Init(builder, cubeGrid);
            MyObjectBuilder_Wheel wheel = builder as MyObjectBuilder_Wheel;
            if ((wheel != null) && !wheel.YieldLastComponent)
            {
                base.SlimBlock.DisableLastComponentYield();
            }
            if (!Sync.IsServer)
            {
                this.m_particleData.ValueChanged += new Action<SyncBase>(this.m_particleData_ValueChanged);
            }
            else
            {
                ParticleData data = new ParticleData {
                    EffectName = "",
                    PositionRelative = Vector3.Zero,
                    Normal = Vector3.Forward
                };
                this.m_particleData.Value = data;
            }
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        private bool IsAcceptableContact(HkRigidBody rb)
        {
            object userObject = rb.UserObject;
            if (userObject == null)
            {
                return false;
            }
            if (userObject == base.CubeGrid.Physics)
            {
                return false;
            }
            if (userObject is MyVoxelPhysicsBody)
            {
                return true;
            }
            MyGridPhysics physics = userObject as MyGridPhysics;
            return ((physics != null) && physics.IsStatic);
        }

        private void m_particleData_ValueChanged(SyncBase obj)
        {
            this.LastContactTime = DateTime.UtcNow;
            string effectName = this.m_particleData.Value.EffectName;
            Vector3D position = base.PositionComp.WorldMatrix.Translation + this.m_particleData.Value.PositionRelative;
            Vector3 normal = this.m_particleData.Value.Normal;
            if (this.Render != null)
            {
                this.Render.TrySpawnParticle(effectName, ref position, ref normal);
                this.Render.UpdateParticle(ref position, ref normal);
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        private void MyWheel_IsWorkingChanged(MyCubeBlock obj)
        {
            if (base.Stator != null)
            {
                base.Stator.UpdateIsWorking();
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            if (base.CubeGrid.Physics != null)
            {
                base.CubeGrid.Physics.RigidBody.CallbackLimit = 1;
                base.CubeGrid.Physics.RigidBody.CollisionAddedCallback += new CollisionEventHandler(this.RigidBody_CollisionAddedCallback);
                base.CubeGrid.Physics.RigidBody.CollisionRemovedCallback += new CollisionEventHandler(this.RigidBody_CollisionRemovedCallback);
            }
        }

        public override void OnRemovedFromScene(object source)
        {
            base.OnRemovedFromScene(source);
            if (base.CubeGrid.Physics != null)
            {
                base.CubeGrid.Physics.RigidBody.CollisionAddedCallback -= new CollisionEventHandler(this.RigidBody_CollisionAddedCallback);
                base.CubeGrid.Physics.RigidBody.CollisionRemovedCallback -= new CollisionEventHandler(this.RigidBody_CollisionRemovedCallback);
            }
        }

        private void RegisterPerFrameUpdate()
        {
            if (((base.NeedsUpdate & MyEntityUpdateEnum.EACH_FRAME) == MyEntityUpdateEnum.NONE) && !this.m_eachUpdateCallbackRegistered)
            {
                this.m_eachUpdateCallbackRegistered = true;
                MySandboxGame.Static.Invoke(delegate {
                    this.m_eachUpdateCallbackRegistered = false;
                    base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
                }, "WheelEachUpdate");
            }
        }

        private void RigidBody_CollisionAddedCallback(ref HkCollisionEvent e)
        {
            MyGridPhysics physics = base.CubeGrid.Physics;
            if (this.IsAcceptableContact(e.BodyA) || this.IsAcceptableContact(e.BodyB))
            {
                this.m_contactCountdown = 30;
                Interlocked.Increment(ref this.m_staticHitCount);
                this.RegisterPerFrameUpdate();
            }
        }

        private void RigidBody_CollisionRemovedCallback(ref HkCollisionEvent e)
        {
            MyGridPhysics physics = base.CubeGrid.Physics;
            if ((this.IsAcceptableContact(e.BodyA) || this.IsAcceptableContact(e.BodyB)) && (Interlocked.Decrement(ref this.m_staticHitCount) < 0))
            {
                Interlocked.Increment(ref this.m_staticHitCount);
            }
        }

        private bool SteeringLogic()
        {
            Vector3 vector2;
            Color? nullable;
            if (!base.IsFunctional)
            {
                return false;
            }
            MyGridPhysics physics = base.CubeGrid.Physics;
            if (physics == null)
            {
                return false;
            }
            if ((base.Stator != null) && MyFixedGrids.IsRooted(base.Stator.CubeGrid))
            {
                return false;
            }
            if (this.m_slipCountdown > 0)
            {
                this.m_slipCountdown--;
            }
            if (this.m_staticHitCount == 0)
            {
                if (this.m_contactCountdown <= 0)
                {
                    return false;
                }
                this.m_contactCountdown--;
                if (this.m_contactCountdown == 0)
                {
                    this.m_frictionCollector = 0f;
                    this.m_contactNormals.Clear();
                    return false;
                }
            }
            Vector3 linearVelocity = physics.LinearVelocity;
            if (MyUtils.IsZero(ref linearVelocity, 1E-05f) || !physics.IsActive)
            {
                return false;
            }
            MatrixD worldMatrix = base.WorldMatrix;
            Vector3D centerOfMassWorld = physics.CenterOfMassWorld;
            if (!this.m_contactNormals.GetAvgNormal(out vector2))
            {
                return false;
            }
            this.LastUsedGroundNormal = vector2;
            Vector3 up = (Vector3) worldMatrix.Up;
            Vector3 guideVector = Vector3.Cross(vector2, up);
            linearVelocity = Vector3.ProjectOnPlane(ref linearVelocity, ref vector2);
            Vector3 direction = Vector3.ProjectOnVector(ref linearVelocity, ref guideVector);
            Vector3 vector6 = direction - linearVelocity;
            if (MyUtils.IsZero(ref vector6, 1E-05f))
            {
                return false;
            }
            bool flag = false;
            bool flag2 = false;
            float num = 6f * this.m_frictionCollector;
            Vector3 vec = Vector3.ProjectOnVector(ref vector6, ref up);
            float num2 = vec.Length();
            bool flag3 = num2 > num;
            if (!flag3 && (this.m_slipCountdown == 0))
            {
                if (num2 < 0.1)
                {
                    flag2 = true;
                }
            }
            else
            {
                flag = true;
                vec = (vec * ((1f / num2) * num)) * (1f - MyPhysicsConfig.WheelSlipCutAwayRatio);
                if (flag3)
                {
                    this.m_slipCountdown = MyPhysicsConfig.WheelSlipCountdown;
                }
            }
            if (!flag2)
            {
                vec *= 1f - ((1f - this.m_frictionCollector) * MyPhysicsConfig.WheelSurfaceMaterialSteerRatio);
                Vector3 vector9 = Vector3.ProjectOnPlane(ref vec, ref vector2);
                MyMechanicalConnectionBlockBase stator = base.Stator;
                MyPhysicsBody body = null;
                if (stator != null)
                {
                    body = base.Stator.CubeGrid.Physics;
                }
                vector9 *= 0.1f;
                if (body == null)
                {
                    physics.ApplyImpulse(vector9 * physics.Mass, centerOfMassWorld);
                }
                else
                {
                    Vector3D zero = Vector3D.Zero;
                    MyMotorSuspension suspension = stator as MyMotorSuspension;
                    if (suspension != null)
                    {
                        Vector3 vector10;
                        vector9 *= MyMath.Clamp(suspension.Friction * 2f, 0f, 1f);
                        suspension.GetCoMVectors(out vector10);
                        zero = Vector3D.TransformNormal(-vector10, stator.CubeGrid.WorldMatrix);
                    }
                    Vector3D pos = centerOfMassWorld + zero;
                    float wheelImpulseBlending = MyPhysicsConfig.WheelImpulseBlending;
                    vector9 = (this.m_lastFrameImpuse * wheelImpulseBlending) + (vector9 * (1f - wheelImpulseBlending));
                    this.m_lastFrameImpuse = vector9;
                    body.ApplyImpulse(vector9 * body.Mass, pos);
                    if (MyDebugDrawSettings.DEBUG_DRAW_WHEEL_PHYSICS)
                    {
                        nullable = null;
                        MyRenderProxy.DebugDrawArrow3DDir(pos, -zero, Color.Red, nullable, false, 0.1, null, 0.5f, false);
                        MyRenderProxy.DebugDrawSphere(pos, 0.1f, Color.Yellow, 1f, false, false, true, false);
                    }
                }
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_WHEEL_PHYSICS)
            {
                nullable = null;
                MyRenderProxy.DebugDrawArrow3DDir(centerOfMassWorld, linearVelocity, Color.Yellow, nullable, false, 0.1, null, 0.5f, false);
                nullable = null;
                MyRenderProxy.DebugDrawArrow3DDir(centerOfMassWorld, direction, Color.Blue, nullable, false, 0.1, null, 0.5f, false);
                nullable = null;
                MyRenderProxy.DebugDrawArrow3DDir(centerOfMassWorld, vec, Color.MediumPurple, nullable, false, 0.1, null, 0.5f, false);
                nullable = null;
                MyRenderProxy.DebugDrawArrow3DDir(centerOfMassWorld + linearVelocity, vector6, Color.Red, nullable, false, 0.1, null, 0.5f, false);
                nullable = null;
                MyRenderProxy.DebugDrawArrow3DDir(centerOfMassWorld + up, vector2, Color.AliceBlue, nullable, false, 0.1, null, 0.5f, false);
                nullable = null;
                MyRenderProxy.DebugDrawArrow3DDir(centerOfMassWorld, Vector3.ProjectOnPlane(ref vec, ref vector2), flag ? Color.DarkRed : Color.IndianRed, nullable, false, 0.1, null, 0.5f, false);
                if (this.m_slipCountdown > 0)
                {
                    MyRenderProxy.DebugDrawText3D(centerOfMassWorld + (up * 2f), "Drift", Color.Red, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                }
                MyRenderProxy.DebugDrawText3D(centerOfMassWorld + (up * 1.2f), this.m_staticHitCount.ToString(), Color.Red, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
            return !flag2;
        }

        private void SwapModelLogic()
        {
            if ((!MyFakes.WHEEL_ALTERNATIVE_MODELS_ENABLED || (base.Stator == null)) || !base.IsFunctional)
            {
                if (this.m_usesAlternativeModel)
                {
                    this.m_usesAlternativeModel = false;
                    this.UpdateVisual();
                }
            }
            else
            {
                float angularVelocityThreshold = this.WheelModelsDefinition.AngularVelocityThreshold;
                float observerAngularVelocityDiff = this.GetObserverAngularVelocityDiff();
                if (!((this.m_usesAlternativeModel && ((observerAngularVelocityDiff + 5f) < angularVelocityThreshold)) | (!this.m_usesAlternativeModel && ((observerAngularVelocityDiff - 5f) > angularVelocityThreshold))))
                {
                    this.m_modelSwapCountUp = 0;
                }
                else
                {
                    this.m_modelSwapCountUp++;
                    if (this.m_modelSwapCountUp >= 5)
                    {
                        this.m_usesAlternativeModel = !this.m_usesAlternativeModel;
                        this.UpdateVisual();
                    }
                }
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (this.Render != null)
            {
                this.Render.UpdatePosition();
            }
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            this.SwapModelLogic();
            bool flag = this.SteeringLogic();
            if (!flag && (this.m_contactCountdown == 0))
            {
                this.m_lastFrameImpuse = Vector3.Zero;
                if (((this.Render == null) || ((this.Render != null) && !this.Render.UpdateNeeded)) && !base.HasDamageEffect)
                {
                    base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
                }
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_WHEEL_PHYSICS)
            {
                MatrixD worldMatrix = base.WorldMatrix;
                MyRenderProxy.DebugDrawCross(worldMatrix.Translation, worldMatrix.Up, worldMatrix.Forward, flag ? Color.Green : Color.Red, false, false);
            }
        }

        public static void WheelExplosionLog(MyCubeGrid grid, MyTerminalBlock block, string message)
        {
        }

        public float Friction { get; set; }

        public ulong LastContactFrameNumber { get; private set; }

        private MyRenderComponentWheel Render
        {
            get => 
                (base.Render as MyRenderComponentWheel);
            set => 
                (base.Render = value);
        }

        public ulong FramesSinceLastContact =>
            (MySandboxGame.Static.SimulationFrameCounter - this.LastContactFrameNumber);

        public DateTime LastContactTime { get; set; }

        private MyWheelModelsDefinition WheelModelsDefinition
        {
            get
            {
                if (this.m_cachedModelsDefinition == null)
                {
                    string subtypeName = base.BlockDefinition.Id.SubtypeName;
                    DictionaryReader<string, MyWheelModelsDefinition> wheelModelDefinitions = MyDefinitionManager.Static.GetWheelModelDefinitions();
                    if (!wheelModelDefinitions.TryGetValue(subtypeName, out this.m_cachedModelsDefinition))
                    {
                        MyDefinitionManager.Static.AddMissingWheelModelDefinition(subtypeName);
                        this.m_cachedModelsDefinition = wheelModelDefinitions[subtypeName];
                    }
                }
                return this.m_cachedModelsDefinition;
            }
        }

        public bool IsConsideredInContactWithStaticSurface =>
            ((this.m_staticHitCount <= 0) ? (this.m_contactCountdown > 0) : true);

        [StructLayout(LayoutKind.Sequential)]
        private struct ParticleData
        {
            public string EffectName;
            public Vector3 PositionRelative;
            public Vector3 Normal;
        }
    }
}

