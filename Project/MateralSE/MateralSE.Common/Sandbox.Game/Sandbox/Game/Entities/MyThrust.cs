namespace Sandbox.Game.Entities
{
    using Havok;
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using Sandbox.RenderDirect.ActorComponents;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Import;

    [MyCubeBlockType(typeof(MyObjectBuilder_Thrust)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyThrust), typeof(Sandbox.ModAPI.Ingame.IMyThrust) })]
    public class MyThrust : MyFunctionalBlock, Sandbox.ModAPI.IMyThrust, Sandbox.ModAPI.Ingame.IMyThrust, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, IMyConveyorEndpointBlock
    {
        private Vector3D m_particleLocalOffset = Vector3D.Zero;
        private MyParticleEffect m_landingEffect;
        private static int m_maxNumberLandingEffects = 10;
        private static int m_landingEffectCount = 0;
        private MyPhysics.HitInfo? m_lastHitInfo;
        private MyEntityThrustComponent m_thrustComponent;
        public float ThrustLengthRand;
        private float m_maxBillboardDistanceSquared;
        private bool m_propellerActive;
        private VRage.Game.Entity.MyEntity m_propellerEntity;
        private bool m_flamesCalculate;
        private bool m_propellerCalculate;
        private float m_propellerMaxDistance;
        private static readonly ConcurrentDictionary<string, List<MyThrustFlameAnimator.FlameInfo>> m_flameCache = new ConcurrentDictionary<string, List<MyThrustFlameAnimator.FlameInfo>>();
        private ListReader<MyThrustFlameAnimator.FlameInfo> m_flames;
        private const int FRAME_DELAY = 100;
        private static readonly List<HkBodyCollision> m_flameCollisionsList = new List<HkBodyCollision>();
        private int m_parallelThrustDamageTaskCount;
        public float LastKnownForceMultiplier;
        private float m_currentStrength;
        private bool m_renderNeedsUpdate;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_thrustOverride;
        [CompilerGenerated]
        private Action<MyThrust, float> ThrustOverrideChanged;
        private MyStringId m_flameLengthMaterialId;
        private MyStringId m_flamePointMaterialId;
        private static HashSet<HkShape> m_blockSet = new HashSet<HkShape>();
        private static List<VRage.ModAPI.IMyEntity> m_alreadyDamagedEntities = new List<VRage.ModAPI.IMyEntity>();
        private float m_thrustMultiplier = 1f;
        private float m_powerConsumptionMultiplier = 1f;
        private MyMultilineConveyorEndpoint m_conveyorEndpoint;

        event Action<Sandbox.ModAPI.IMyThrust, float> Sandbox.ModAPI.IMyThrust.ThrustOverrideChanged
        {
            add
            {
                this.ThrustOverrideChanged += GetDelegate(value);
            }
            remove
            {
                this.ThrustOverrideChanged -= GetDelegate(value);
            }
        }

        public event Action<MyThrust, float> ThrustOverrideChanged
        {
            [CompilerGenerated] add
            {
                Action<MyThrust, float> thrustOverrideChanged = this.ThrustOverrideChanged;
                while (true)
                {
                    Action<MyThrust, float> a = thrustOverrideChanged;
                    Action<MyThrust, float> action3 = (Action<MyThrust, float>) Delegate.Combine(a, value);
                    thrustOverrideChanged = Interlocked.CompareExchange<Action<MyThrust, float>>(ref this.ThrustOverrideChanged, action3, a);
                    if (ReferenceEquals(thrustOverrideChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyThrust, float> thrustOverrideChanged = this.ThrustOverrideChanged;
                while (true)
                {
                    Action<MyThrust, float> source = thrustOverrideChanged;
                    Action<MyThrust, float> action3 = (Action<MyThrust, float>) Delegate.Remove(source, value);
                    thrustOverrideChanged = Interlocked.CompareExchange<Action<MyThrust, float>>(ref this.ThrustOverrideChanged, action3, source);
                    if (ReferenceEquals(thrustOverrideChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyThrust()
        {
            this.CreateTerminalControls();
            this.Render = new MyRenderComponentThrust();
            base.AddDebugRenderComponent(new MyDebugRenderComponentThrust(this));
            this.m_thrustOverride.ValueChanged += x => this.ThrustOverrideValueChanged();
        }

        public bool AllowSelfPulling() => 
            false;

        protected override bool CheckIsWorking() => 
            (this.IsPowered && base.CheckIsWorking());

        protected override void Closing()
        {
            if (this.m_landingEffect != null)
            {
                this.m_landingEffect.Stop(false);
                this.m_landingEffect = null;
                m_landingEffectCount--;
            }
            base.Closing();
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            if (base.CubeGrid.GridSystems.ResourceDistributor != null)
            {
                base.CubeGrid.GridSystems.ResourceDistributor.ConveyorSystem_OnPoweredChanged();
            }
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyThrust>())
            {
                base.CreateTerminalControls();
                float threshold = 1f;
                MyTerminalControlSlider<MyThrust> slider1 = new MyTerminalControlSlider<MyThrust>("Override", MySpaceTexts.BlockPropertyTitle_ThrustOverride, MySpaceTexts.BlockPropertyDescription_ThrustOverride);
                MyTerminalControlSlider<MyThrust> slider2 = new MyTerminalControlSlider<MyThrust>("Override", MySpaceTexts.BlockPropertyTitle_ThrustOverride, MySpaceTexts.BlockPropertyDescription_ThrustOverride);
                slider2.Getter = x => (float) ((x.m_thrustOverride * x.BlockDefinition.ForceMagnitude) * 0.01f);
                MyTerminalControlSlider<MyThrust> local2 = slider2;
                local2.Setter = delegate (MyThrust x, float v) {
                    x.m_thrustOverride.Value = (v <= threshold) ? 0f : ((v / x.BlockDefinition.ForceMagnitude) * 100f);
                    x.RaisePropertiesChanged();
                };
                local2.DefaultValue = 0f;
                local2.SetLimits(x => 0f, x => x.BlockDefinition.ForceMagnitude);
                MyTerminalControlSlider<MyThrust> slider = local2;
                slider.EnableActions<MyThrust>(0.05f, null, null);
                slider.Writer = delegate (MyThrust x, StringBuilder result) {
                    if (x.ThrustOverride < 1f)
                    {
                        result.Append(MyTexts.Get(MyCommonTexts.Disabled));
                    }
                    else
                    {
                        MyValueFormatter.AppendForceInBestUnit(x.ThrustOverride * x.m_thrustComponent.GetLastThrustMultiplier(x), result);
                    }
                };
                MyTerminalControlFactory.AddControl<MyThrust>(slider);
            }
        }

        public void CubeBlock_OnWorkingChanged(MyCubeBlock block)
        {
            if (this.m_landingEffect != null)
            {
                this.m_landingEffect.Stop(false);
                this.m_landingEffect = null;
                m_landingEffectCount--;
            }
            bool flag = false;
            if (base.IsWorking)
            {
                flag = this.UpdateRenderDistance();
            }
            else
            {
                flag = this.m_flamesCalculate || this.m_flamesCalculate;
                this.m_flamesCalculate = false;
                this.m_propellerCalculate = false;
            }
            if (flag)
            {
                this.InvokeRenderUpdate();
            }
        }

        private void DamageGrid(MyThrustFlameAnimator.FlameInfo flameInfo, LineD l, MyCubeGrid grid, HashSet<HkShape> shapes)
        {
            float num = flameInfo.Radius * this.BlockDefinition.FlameDamageLengthScale;
            Vector3 max = new Vector3((double) num, (double) num, l.Length * 0.5);
            MatrixD worldMatrix = base.WorldMatrix;
            worldMatrix.Translation = ((l.To - l.From) * 0.5) + l.From;
            BoundingBoxD box = new BoundingBoxD(-max, max);
            MyOrientedBoundingBoxD xd1 = new MyOrientedBoundingBoxD(box, worldMatrix);
            List<MySlimBlock> blocks = new List<MySlimBlock>();
            grid.GetBlocksIntersectingOBB(box, worldMatrix, blocks);
            foreach (MySlimBlock block in blocks)
            {
                if (ReferenceEquals(block, base.SlimBlock))
                {
                    continue;
                }
                if ((block != null) && ((base.CubeGrid.GridSizeEnum == MyCubeSize.Large) || (block.BlockDefinition.DeformationRatio > 0.25)))
                {
                    List<HkShape> shapesFromPosition = block.CubeGrid.GetShapesFromPosition(block.Min);
                    if (shapesFromPosition != null)
                    {
                        foreach (HkShape shape in shapesFromPosition)
                        {
                            if (shapes.Contains(shape))
                            {
                                MyHitInfo? hitInfo = null;
                                block.DoDamage(100f * this.BlockDefinition.FlameDamage, MyDamageType.Environment, true, hitInfo, base.EntityId);
                            }
                        }
                    }
                }
            }
        }

        public LineD GetDamageCapsuleLine(MyThrustFlameAnimator.FlameInfo info, ref MatrixD matrixWorld)
        {
            Vector3D from = Vector3D.Transform(info.Position, matrixWorld);
            Vector3D vectord2 = Vector3.TransformNormal(info.Direction, matrixWorld);
            float num = ((this.ThrustLengthRand * info.Radius) * 0.5f) * this.BlockDefinition.FlameDamageLengthScale;
            if (num > info.Radius)
            {
                return new LineD(from, from + (vectord2 * ((2f * num) - info.Radius)), (double) ((2f * num) - info.Radius));
            }
            return new LineD(from + (vectord2 * num), from + (vectord2 * num), 0.0) { Direction = vectord2 };
        }

        public static Action<MyThrust, float> GetDelegate(Action<Sandbox.ModAPI.IMyThrust, float> value) => 
            ((Action<MyThrust, float>) Delegate.CreateDelegate(typeof(Action<MyThrust, float>), value.Target, value.Method));

        private string GetDirectionString()
        {
            Vector3I gridThrustDirection = this.GridThrustDirection;
            if (gridThrustDirection != Vector3I.Zero)
            {
                if (gridThrustDirection.X == 1)
                {
                    return MyTexts.GetString(MyCommonTexts.Thrust_Left);
                }
                if (gridThrustDirection.X == -1)
                {
                    return MyTexts.GetString(MyCommonTexts.Thrust_Right);
                }
                if (gridThrustDirection.Y == 1)
                {
                    return MyTexts.GetString(MyCommonTexts.Thrust_Down);
                }
                if (gridThrustDirection.Y == -1)
                {
                    return MyTexts.GetString(MyCommonTexts.Thrust_Up);
                }
                if (gridThrustDirection.Z == 1)
                {
                    return MyTexts.GetString(MyCommonTexts.Thrust_Forward);
                }
                if (gridThrustDirection.Z == -1)
                {
                    return MyTexts.GetString(MyCommonTexts.Thrust_Back);
                }
            }
            return null;
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_Thrust objectBuilderCubeBlock = (MyObjectBuilder_Thrust) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.ThrustOverride = this.ThrustOverride;
            return objectBuilderCubeBlock;
        }

        public PullInformation GetPullInformation() => 
            null;

        public PullInformation GetPushInformation() => 
            null;

        public override void GetTerminalName(StringBuilder result)
        {
            string directionString = this.GetDirectionString();
            if (directionString == null)
            {
                base.GetTerminalName(result);
            }
            else
            {
                result.Append(this.DisplayNameText).Append(" (").Append(directionString).Append(") ");
            }
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            MyEntityThrustComponent component;
            MyFuelConverterInfo fuelConverter;
            if (!cubeGrid.Components.TryGet<MyEntityThrustComponent>(out component))
            {
                component = new MyThrusterBlockThrustComponent();
                component.Init();
                cubeGrid.Components.Add<MyEntityThrustComponent>(component);
            }
            this.m_thrustComponent = component;
            this.BlockDefinition = (MyThrustDefinition) base.BlockDefinition;
            MyDefinitionId defId = new MyDefinitionId();
            if (!this.BlockDefinition.FuelConverter.FuelId.IsNull())
            {
                defId = this.BlockDefinition.FuelConverter.FuelId;
            }
            this.m_flameLengthMaterialId = MyStringId.GetOrCompute(this.BlockDefinition.FlameLengthMaterial);
            this.m_flamePointMaterialId = MyStringId.GetOrCompute(this.BlockDefinition.FlamePointMaterial);
            MyGasProperties properties = null;
            if (MyFakes.ENABLE_HYDROGEN_FUEL)
            {
                MyDefinitionManager.Static.TryGetDefinition<MyGasProperties>(defId, out properties);
            }
            MyGasProperties properties2 = properties;
            if (properties == null)
            {
                MyGasProperties local1 = properties;
                MyGasProperties properties1 = new MyGasProperties();
                properties1.Id = MyResourceDistributorComponent.ElectricityId;
                properties1.EnergyDensity = 1f;
                properties2 = properties1;
            }
            this.FuelDefinition = properties2;
            base.Init(objectBuilder, cubeGrid);
            base.NeedsWorldMatrix = false;
            base.InvalidateOnMove = false;
            MyObjectBuilder_Thrust thrust = (MyObjectBuilder_Thrust) objectBuilder;
            this.m_thrustOverride.SetLocalValue((MathHelper.Clamp(thrust.ThrustOverride, 0f, this.BlockDefinition.ForceMagnitude) * 100f) / this.BlockDefinition.ForceMagnitude);
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_FlareDefinition), this.BlockDefinition.FlameFlare);
            MyFlareDefinition definition = MyDefinitionManager.Static.GetDefinition(id) as MyFlareDefinition;
            this.Flares = definition ?? new MyFlareDefinition();
            this.m_maxBillboardDistanceSquared = this.BlockDefinition.FlameVisibilityDistance * this.BlockDefinition.FlameVisibilityDistance;
            this.UpdateDetailedInfo();
            if (MyFakes.ENABLE_HYDROGEN_FUEL)
            {
                fuelConverter = this.BlockDefinition.FuelConverter;
            }
            else
            {
                MyFuelConverterInfo info1 = new MyFuelConverterInfo();
                info1.Efficiency = 1f;
                fuelConverter = info1;
            }
            this.FuelConverterDefinition = fuelConverter;
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.CubeBlock_OnWorkingChanged);
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_conveyorEndpoint = new MyMultilineConveyorEndpoint(this);
        }

        protected override MyEntitySubpart InstantiateSubpart(MyModelDummy subpartDummy, ref MyEntitySubpart.Data data)
        {
            MyEntitySubpart subpart1 = base.InstantiateSubpart(subpartDummy, ref data);
            subpart1.NeedsWorldMatrix = false;
            subpart1.Render = new MyRenderComponentThrust.MyPropellerRenderComponent();
            return subpart1;
        }

        private void InvokeRenderUpdate()
        {
            if (!this.m_renderNeedsUpdate && !Sandbox.Engine.Platform.Game.IsDedicated)
            {
                this.m_renderNeedsUpdate = true;
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            }
        }

        private void LoadDummies()
        {
            MyModel context = base.Model;
            this.m_flames = m_flameCache.GetOrAdd<string, List<MyThrustFlameAnimator.FlameInfo>, MyModel>(context.AssetName, context, delegate (MyModel m, string _) {
                List<MyThrustFlameAnimator.FlameInfo> list = new List<MyThrustFlameAnimator.FlameInfo>();
                foreach (KeyValuePair<string, MyModelDummy> pair in from s in m.Dummies
                    orderby s.Key
                    select s)
                {
                    if (pair.Key.StartsWith("thruster_flame", StringComparison.InvariantCultureIgnoreCase))
                    {
                        MyThrustFlameAnimator.FlameInfo item = new MyThrustFlameAnimator.FlameInfo {
                            Position = pair.Value.Matrix.Translation,
                            Direction = Vector3.Normalize(pair.Value.Matrix.Forward),
                            Radius = Math.Max(pair.Value.Matrix.Scale.X, pair.Value.Matrix.Scale.Y) * 0.5f
                        };
                        list.Add(item);
                    }
                }
                return list;
            });
            if (this.BlockDefinition != null)
            {
                this.m_propellerActive = this.LoadPropeller();
            }
            this.Render.UpdateFlameAnimatorData();
        }

        private bool LoadPropeller()
        {
            MyEntitySubpart subpart;
            if ((!this.BlockDefinition.PropellerUse || (this.BlockDefinition.PropellerEntity == null)) || !base.Subparts.TryGetValue(this.BlockDefinition.PropellerEntity, out subpart))
            {
                return false;
            }
            this.m_propellerEntity = subpart;
            this.m_propellerMaxDistance = this.BlockDefinition.PropellerMaxDistance * this.BlockDefinition.PropellerMaxDistance;
            return true;
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            this.LoadDummies();
        }

        public override void OnRegisteredToGridSystems()
        {
            MyEntityThrustComponent component;
            base.OnRegisteredToGridSystems();
            if (!base.CubeGrid.Components.TryGet<MyEntityThrustComponent>(out component))
            {
                component = new MyThrusterBlockThrustComponent();
                component.Init();
                base.CubeGrid.Components.Add<MyEntityThrustComponent>(component);
            }
            this.m_thrustComponent = component;
            this.m_thrustComponent.Register(this, this.ThrustForwardVector, new Func<bool>(this.OnRegisteredToThrustComponent));
            this.m_thrustComponent.DampenersEnabled = base.CubeGrid.DampenersEnabled;
        }

        private bool OnRegisteredToThrustComponent()
        {
            MyResourceSinkComponent component1 = this.m_thrustComponent.ResourceSink(this);
            component1.IsPoweredChanged += new Action(this.Sink_IsPoweredChanged);
            component1.Update();
            return true;
        }

        public override void OnUnregisteredFromGridSystems()
        {
            base.OnUnregisteredFromGridSystems();
            if (!base.CubeGrid.MarkedForClose)
            {
                this.m_thrustComponent.ResourceSink(this).IsPoweredChanged -= new Action(this.Sink_IsPoweredChanged);
                this.m_thrustComponent.Unregister(this, this.ThrustForwardVector);
            }
        }

        public static void RandomizeFlameProperties(float strength, float flameScale, ref float thrustRadiusRand, ref float thrustLengthRand)
        {
            thrustRadiusRand = MyUtils.GetRandomFloat(0.9f, 1.1f);
        }

        private void RenderUpdate()
        {
            MyRenderComponentThrust render = this.Render;
            MyThrustDefinition blockDefinition = this.BlockDefinition;
            float currentStrength = this.m_currentStrength;
            render.UpdateFlameProperties(this.m_flamesCalculate, currentStrength);
            if (this.m_propellerActive)
            {
                float num2 = 0f;
                if (this.m_propellerCalculate)
                {
                    num2 = (currentStrength > 0f) ? blockDefinition.PropellerFullSpeed : blockDefinition.PropellerIdleSpeed;
                }
                render.UpdatePropellerSpeed(num2 * 6.283185f);
            }
            this.m_renderNeedsUpdate = false;
            base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_10TH_FRAME;
        }

        public void Sink_IsPoweredChanged()
        {
            base.UpdateIsWorking();
        }

        private void ThrustDamageAsync()
        {
            if (((this.m_flames.Count > 0) && (MySession.Static.ThrusterDamage && (base.IsWorking && (base.CubeGrid.InScene && (base.CubeGrid.Physics != null))))) && base.CubeGrid.Physics.Enabled)
            {
                if (!MySandboxGame.IsPaused)
                {
                    this.ThrustLengthRand = ((this.CurrentStrength * 10f) * MyUtils.GetRandomFloat(0.6f, 1f)) * this.BlockDefinition.FlameLengthScale;
                }
                if ((Sync.IsServer && ((this.CurrentStrength != 0f) || MyFakes.INACTIVE_THRUSTER_DMG)) && MyFakes.INACTIVE_THRUSTER_DMG)
                {
                    foreach (MyThrustFlameAnimator.FlameInfo info in this.m_flames)
                    {
                        MatrixD worldMatrix = base.WorldMatrix;
                        LineD damageCapsuleLine = this.GetDamageCapsuleLine(info, ref worldMatrix);
                        this.ThrustDamageShapeCast(damageCapsuleLine, info, m_flameCollisionsList);
                        this.ThrustDamageDealDamage(info, m_flameCollisionsList);
                    }
                }
            }
        }

        private void ThrustDamageDealDamage(MyThrustFlameAnimator.FlameInfo flameInfo, List<HkBodyCollision> flameCollisionsList)
        {
            using (MyUtils.ReuseCollection<HkShape>(ref m_blockSet))
            {
                using (MyUtils.ReuseCollection<VRage.ModAPI.IMyEntity>(ref m_alreadyDamagedEntities))
                {
                    foreach (HkBodyCollision collision in flameCollisionsList)
                    {
                        MyCubeGrid collisionEntity = collision.GetCollisionEntity() as MyCubeGrid;
                        if (collisionEntity != null)
                        {
                            m_blockSet.Add(collisionEntity.Physics.RigidBody.GetShape().GetContainer().GetShape(collision.ShapeKey));
                        }
                    }
                    using (List<HkBodyCollision>.Enumerator enumerator = flameCollisionsList.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            VRage.ModAPI.IMyEntity collisionEntity = enumerator.Current.GetCollisionEntity();
                            if ((collisionEntity != null) && !collisionEntity.Equals(this))
                            {
                                if (!(collisionEntity is MyCharacter))
                                {
                                    collisionEntity = collisionEntity.GetTopMostParent(null);
                                }
                                if (!m_alreadyDamagedEntities.Contains(collisionEntity))
                                {
                                    m_alreadyDamagedEntities.Add(collisionEntity);
                                    if (collisionEntity is IMyDestroyableObject)
                                    {
                                        MyHitInfo? hitInfo = null;
                                        (collisionEntity as IMyDestroyableObject).DoDamage((flameInfo.Radius * this.BlockDefinition.FlameDamage) * 100f, MyDamageType.Environment, true, hitInfo, base.EntityId);
                                    }
                                    else if (collisionEntity is MyCubeGrid)
                                    {
                                        MyCubeGrid grid = collisionEntity as MyCubeGrid;
                                        if (grid.BlocksDestructionEnabled)
                                        {
                                            MatrixD worldMatrix = base.WorldMatrix;
                                            LineD damageCapsuleLine = this.GetDamageCapsuleLine(flameInfo, ref worldMatrix);
                                            this.DamageGrid(flameInfo, damageCapsuleLine, grid, m_blockSet);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            flameCollisionsList.Clear();
        }

        private void ThrustDamageShapeCast(LineD damageLine, MyThrustFlameAnimator.FlameInfo flameInfo, List<HkBodyCollision> outFlameCollisionsList)
        {
            HkShape shape = (damageLine.Length == 0.0) ? ((HkShape) new HkSphereShape(flameInfo.Radius * this.BlockDefinition.FlameDamageLengthScale)) : ((HkShape) new HkCapsuleShape(Vector3.Zero, (Vector3) (damageLine.To - damageLine.From), flameInfo.Radius * this.BlockDefinition.FlameDamageLengthScale));
            MyPhysics.GetPenetrationsShape(shape, ref damageLine.From, ref Quaternion.Identity, outFlameCollisionsList, 15);
            shape.RemoveReference();
        }

        private void ThrustOverrideValueChanged()
        {
            this.ThrustOverrideChanged.InvokeIfNotNull<MyThrust, float>(this, this.ThrustOverride);
        }

        private void ThrustParticles()
        {
            if (base.IsWorking)
            {
                Matrix matrix;
                base.GetLocalMatrix(out matrix);
                Vector3 translation = matrix.Translation;
                float gridScale = base.CubeGrid.GridScale;
                foreach (MyThrustFlameAnimator.FlameInfo info in this.Flames)
                {
                    MyPhysics.HitInfo? nullable1;
                    Vector3D from = Vector3D.Transform(Vector3D.TransformNormal(info.Position, matrix) + translation, base.CubeGrid.WorldMatrix);
                    Vector3D vectord = Vector3D.TransformNormal(Vector3D.TransformNormal(info.Direction, matrix), base.CubeGrid.WorldMatrix);
                    if (this.ThrustLengthRand > 1E-05f)
                    {
                        nullable1 = MyPhysics.CastRay(from, from + (((vectord * this.ThrustLengthRand) * 2.5) * info.Radius), 15);
                    }
                    else
                    {
                        nullable1 = null;
                    }
                    this.m_lastHitInfo = nullable1;
                    VRage.Game.Entity.MyEntity entity = (this.m_lastHitInfo != null) ? (this.m_lastHitInfo.Value.HkHitInfo.GetHitEntity() as VRage.Game.Entity.MyEntity) : null;
                    bool flag = false;
                    string effectName = "Landing_Jet_Ground";
                    if (entity != null)
                    {
                        if (!(entity is MyVoxelPhysics) && !(entity is MyVoxelMap))
                        {
                            if ((entity.GetTopMostParent(null) is MyCubeGrid) && !ReferenceEquals(entity.GetTopMostParent(null), base.GetTopMostParent(null)))
                            {
                                flag = true;
                                effectName = (base.CubeGrid.GridSizeEnum != MyCubeSize.Large) ? "Landing_Jet_Grid_Small" : "Landing_Jet_Grid_Large";
                            }
                        }
                        else
                        {
                            flag = true;
                            effectName = "Landing_Jet_Ground";
                            MyVoxelBase self = null;
                            if (entity is MyVoxelPhysics)
                            {
                                self = (entity as MyVoxelPhysics).RootVoxel;
                                effectName = "Landing_Jet_Ground";
                            }
                            else
                            {
                                self = entity as MyVoxelMap;
                                effectName = "Landing_Jet_Ground_Dust";
                            }
                            Vector3D position = this.m_lastHitInfo.Value.Position;
                            MyVoxelMaterialDefinition materialAt = self.GetMaterialAt(ref position);
                            if ((materialAt != null) && !string.IsNullOrEmpty(materialAt.LandingEffect))
                            {
                                effectName = materialAt.LandingEffect;
                            }
                        }
                    }
                    if (!flag)
                    {
                        if (this.m_landingEffect != null)
                        {
                            this.m_landingEffect.Stop(false);
                            this.m_landingEffect = null;
                            m_landingEffectCount--;
                            base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
                        }
                    }
                    else if (this.m_landingEffect != null)
                    {
                        if (this.m_lastHitInfo != null)
                        {
                            this.m_particleLocalOffset = Vector3D.Transform(this.m_lastHitInfo.Value.Position, base.PositionComp.WorldMatrixInvScaled);
                        }
                    }
                    else if ((m_landingEffectCount < m_maxNumberLandingEffects) && MyParticlesManager.TryCreateParticleEffect(effectName, MatrixD.CreateFromTransformScale(Quaternion.CreateFromForwardUp(-this.m_lastHitInfo.Value.HkHitInfo.Normal, Vector3.CalculatePerpendicularVector(this.m_lastHitInfo.Value.HkHitInfo.Normal)), this.m_lastHitInfo.Value.Position, Vector3D.One), out this.m_landingEffect))
                    {
                        m_landingEffectCount++;
                        this.m_landingEffect.UserScale = base.CubeGrid.GridSize;
                        base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
                    }
                }
            }
        }

        private void ThrustParticlesPositionUpdate()
        {
            if (this.m_landingEffect != null)
            {
                Vector3D trans = Vector3D.Transform(this.m_particleLocalOffset, base.WorldMatrix);
                this.m_landingEffect.SetTranslation(trans);
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            this.ThrustParticlesPositionUpdate();
        }

        public override void UpdateAfterSimulation10()
        {
            if (this.m_renderNeedsUpdate)
            {
                this.RenderUpdate();
            }
            base.UpdateAfterSimulation10();
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            base.UpdateIsWorking();
            if (base.IsWorking)
            {
                this.UpdateSoundState();
                if (this.UpdateRenderDistance())
                {
                    this.RenderUpdate();
                }
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            this.ThrustDamageAsync();
            base.UpdateBeforeSimulation100();
            this.ThrustParticles();
        }

        private void UpdateDetailedInfo()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(this.BlockDefinition.DisplayNameText);
            base.DetailedInfo.AppendFormat("\n", Array.Empty<object>());
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxRequiredInput));
            if (this.FuelDefinition.Id.SubtypeName == "Electricity")
            {
                MyValueFormatter.AppendWorkInBestUnit(this.MaxPowerConsumption, base.DetailedInfo);
            }
            else
            {
                MyValueFormatter.AppendVolumeInBestUnit(this.MaxPowerConsumption, base.DetailedInfo);
            }
            base.DetailedInfo.AppendFormat("\n", Array.Empty<object>());
            base.RaisePropertiesChanged();
        }

        private bool UpdateRenderDistance()
        {
            if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                return false;
            }
            bool flag = false;
            double num = Vector3D.DistanceSquared(MySector.MainCamera.Position, base.PositionComp.GetPosition());
            bool flag2 = num < this.m_maxBillboardDistanceSquared;
            if (flag2 != this.m_flamesCalculate)
            {
                flag = true;
                this.m_flamesCalculate = flag2;
            }
            if (this.m_propellerActive)
            {
                bool flag3 = num < this.m_propellerMaxDistance;
                if (flag3 != this.m_propellerCalculate)
                {
                    flag = true;
                    this.m_propellerCalculate = flag3;
                }
            }
            return flag;
        }

        private void UpdateSoundState()
        {
            if (base.m_soundEmitter != null)
            {
                if (this.CurrentStrength <= 0.1f)
                {
                    base.m_soundEmitter.StopSound(false, true);
                }
                else if (!base.m_soundEmitter.IsPlaying)
                {
                    bool? nullable = null;
                    base.m_soundEmitter.PlaySound(this.BlockDefinition.PrimarySound, true, false, false, false, false, nullable);
                }
                if ((base.m_soundEmitter.Sound != null) && base.m_soundEmitter.Sound.IsPlaying)
                {
                    float semitones = (8f * (this.CurrentStrength - (0.5f * MyConstants.MAX_THRUST))) / MyConstants.MAX_THRUST;
                    base.m_soundEmitter.Sound.FrequencyRatio = MyAudio.Static.SemitonesToFrequencyRatio(semitones);
                }
            }
        }

        public MyThrustDefinition BlockDefinition { get; private set; }

        public MyRenderComponentThrust Render
        {
            get => 
                ((MyRenderComponentThrust) base.Render);
            set => 
                (base.Render = value);
        }

        public MyFuelConverterInfo FuelConverterDefinition { get; private set; }

        public MyFlareDefinition Flares { get; private set; }

        public MyGasProperties FuelDefinition { get; private set; }

        public VRage.Game.Entity.MyEntity Propeller =>
            this.m_propellerEntity;

        public Vector3 ThrustForce =>
            ((Vector3) (-this.ThrustForwardVector * (this.BlockDefinition.ForceMagnitude * this.m_thrustMultiplier)));

        public float ThrustForceLength =>
            (this.BlockDefinition.ForceMagnitude * this.m_thrustMultiplier);

        public float ThrustOverride
        {
            get => 
                ((float) (((this.m_thrustOverride * this.m_thrustMultiplier) * this.BlockDefinition.ForceMagnitude) * 0.01f));
            set
            {
                float f = value / ((this.m_thrustMultiplier * this.BlockDefinition.ForceMagnitude) * 0.01f);
                if (float.IsInfinity(f) || float.IsNaN(f))
                {
                    f = 0f;
                }
                this.m_thrustOverride.Value = MathHelper.Clamp(f, 0f, 100f);
            }
        }

        public float ThrustOverrideOverForceLen =>
            ((float) (this.m_thrustOverride * 0.01f));

        public Vector3I ThrustForwardVector =>
            Base6Directions.GetIntVector(base.Orientation.Forward);

        public bool IsPowered =>
            this.m_thrustComponent.IsThrustPoweredByType(this, ref this.FuelDefinition.Id);

        public float MaxPowerConsumption =>
            (this.BlockDefinition.MaxPowerConsumption * this.m_powerConsumptionMultiplier);

        public float MinPowerConsumption =>
            (this.BlockDefinition.MinPowerConsumption * this.m_powerConsumptionMultiplier);

        public float CurrentStrength
        {
            get => 
                this.m_currentStrength;
            set
            {
                if (this.m_currentStrength != value)
                {
                    this.m_currentStrength = value;
                    this.InvokeRenderUpdate();
                }
            }
        }

        public ListReader<MyThrustFlameAnimator.FlameInfo> Flames =>
            this.m_flames;

        public MyStringId FlameLengthMaterial =>
            this.m_flameLengthMaterialId;

        public MyStringId FlamePointMaterial =>
            this.m_flamePointMaterialId;

        public float FlameDamageLengthScale =>
            this.BlockDefinition.FlameDamageLengthScale;

        public Vector3I GridThrustDirection
        {
            get
            {
                Quaternion quaternion;
                MyShipController controlledEntity = MySession.Static.ControlledEntity as MyShipController;
                if (controlledEntity == null)
                {
                    controlledEntity = base.CubeGrid.GridSystems.ControlSystem.GetShipController();
                }
                if (controlledEntity == null)
                {
                    return Vector3I.Zero;
                }
                controlledEntity.Orientation.GetQuaternion(out quaternion);
                return Vector3I.Transform(this.ThrustForwardVector, Quaternion.Inverse(quaternion));
            }
        }

        float Sandbox.ModAPI.Ingame.IMyThrust.ThrustOverride
        {
            get => 
                this.ThrustOverride;
            set => 
                (this.ThrustOverride = value);
        }

        float Sandbox.ModAPI.Ingame.IMyThrust.ThrustOverridePercentage
        {
            get => 
                ((float) (this.m_thrustOverride / 100f));
            set => 
                (this.m_thrustOverride.Value = MathHelper.Clamp(value, 0f, 1f) * 100f);
        }

        float Sandbox.ModAPI.IMyThrust.ThrustMultiplier
        {
            get => 
                this.m_thrustMultiplier;
            set
            {
                this.m_thrustMultiplier = value;
                if (this.m_thrustMultiplier < 0.01f)
                {
                    this.m_thrustMultiplier = 0.01f;
                }
                if (this.m_thrustComponent != null)
                {
                    this.m_thrustComponent.MarkDirty(false);
                }
            }
        }

        float Sandbox.ModAPI.IMyThrust.PowerConsumptionMultiplier
        {
            get => 
                this.m_powerConsumptionMultiplier;
            set
            {
                this.m_powerConsumptionMultiplier = value;
                if (this.m_powerConsumptionMultiplier < 0.01f)
                {
                    this.m_powerConsumptionMultiplier = 0.01f;
                }
                if (this.m_thrustComponent != null)
                {
                    this.m_thrustComponent.MarkDirty(false);
                }
                this.UpdateDetailedInfo();
            }
        }

        float Sandbox.ModAPI.Ingame.IMyThrust.MaxThrust =>
            (this.BlockDefinition.ForceMagnitude * this.m_thrustMultiplier);

        float Sandbox.ModAPI.Ingame.IMyThrust.MaxEffectiveThrust =>
            ((this.BlockDefinition.ForceMagnitude * this.m_thrustMultiplier) * this.m_thrustComponent.GetLastThrustMultiplier(this));

        float Sandbox.ModAPI.Ingame.IMyThrust.CurrentThrust =>
            ((this.CurrentStrength * this.BlockDefinition.ForceMagnitude) * this.m_thrustMultiplier);

        Vector3I Sandbox.ModAPI.Ingame.IMyThrust.GridThrustDirection =>
            this.GridThrustDirection;

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_conveyorEndpoint;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyThrust.<>c <>9 = new MyThrust.<>c();
            public static MyTerminalValueControl<MyThrust, float>.GetterDelegate <>9__79_0;
            public static MyTerminalValueControl<MyThrust, float>.GetterDelegate <>9__79_2;
            public static MyTerminalValueControl<MyThrust, float>.GetterDelegate <>9__79_3;
            public static MyTerminalControl<MyThrust>.WriterDelegate <>9__79_4;
            public static Func<KeyValuePair<string, MyModelDummy>, string> <>9__94_1;
            public static Func<MyModel, string, List<MyThrustFlameAnimator.FlameInfo>> <>9__94_0;

            internal float <CreateTerminalControls>b__79_0(MyThrust x) => 
                ((float) ((x.m_thrustOverride * x.BlockDefinition.ForceMagnitude) * 0.01f));

            internal float <CreateTerminalControls>b__79_2(MyThrust x) => 
                0f;

            internal float <CreateTerminalControls>b__79_3(MyThrust x) => 
                x.BlockDefinition.ForceMagnitude;

            internal void <CreateTerminalControls>b__79_4(MyThrust x, StringBuilder result)
            {
                if (x.ThrustOverride < 1f)
                {
                    result.Append(MyTexts.Get(MyCommonTexts.Disabled));
                }
                else
                {
                    MyValueFormatter.AppendForceInBestUnit(x.ThrustOverride * x.m_thrustComponent.GetLastThrustMultiplier(x), result);
                }
            }

            internal List<MyThrustFlameAnimator.FlameInfo> <LoadDummies>b__94_0(MyModel m, string _)
            {
                List<MyThrustFlameAnimator.FlameInfo> list = new List<MyThrustFlameAnimator.FlameInfo>();
                foreach (KeyValuePair<string, MyModelDummy> pair in from s in m.Dummies
                    orderby s.Key
                    select s)
                {
                    if (pair.Key.StartsWith("thruster_flame", StringComparison.InvariantCultureIgnoreCase))
                    {
                        MyThrustFlameAnimator.FlameInfo item = new MyThrustFlameAnimator.FlameInfo {
                            Position = pair.Value.Matrix.Translation,
                            Direction = Vector3.Normalize(pair.Value.Matrix.Forward),
                            Radius = Math.Max(pair.Value.Matrix.Scale.X, pair.Value.Matrix.Scale.Y) * 0.5f
                        };
                        list.Add(item);
                    }
                }
                return list;
            }

            internal string <LoadDummies>b__94_1(KeyValuePair<string, MyModelDummy> s) => 
                s.Key;
        }
    }
}

