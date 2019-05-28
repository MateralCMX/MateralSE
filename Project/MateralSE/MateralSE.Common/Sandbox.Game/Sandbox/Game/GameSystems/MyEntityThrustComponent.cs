namespace Sandbox.Game.GameSystems
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Planet;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyEntityThrustComponent : MyEntityComponentBase
    {
        private static float MAX_DISTANCE_RELATIVE_DAMPENING = 100f;
        private static float MAX_DISTANCE_RELATIVE_DAMPENING_SQ = (MAX_DISTANCE_RELATIVE_DAMPENING * MAX_DISTANCE_RELATIVE_DAMPENING);
        private static readonly DirectionComparer m_directionComparer = new DirectionComparer();
        protected float m_lastPlanetaryInfluence = -1f;
        protected bool m_lastPlanetaryInfluenceHasAtmosphere;
        protected float m_lastPlanetaryGravityMagnitude;
        private int m_nextPlanetaryInfluenceRecalculation = -1;
        private const int m_maxInfluenceRecalculationInterval = 0x2710;
        private Vector3 m_maxNegativeThrust;
        private Vector3 m_maxPositiveThrust;
        protected readonly List<FuelTypeData> m_dataByFuelType = new List<FuelTypeData>();
        private readonly MyResourceSinkComponent m_resourceSink;
        private Vector3 m_totalMaxNegativeThrust;
        private Vector3 m_totalMaxPositiveThrust;
        protected readonly List<MyDefinitionId> m_fuelTypes = new List<MyDefinitionId>();
        private readonly Dictionary<MyDefinitionId, int> m_fuelTypeToIndex = new Dictionary<MyDefinitionId, int>(MyDefinitionId.Comparer);
        private readonly List<MyConveyorConnectedGroup> m_connectedGroups = new List<MyConveyorConnectedGroup>();
        protected MyResourceSinkComponent m_lastSink;
        protected FuelTypeData m_lastFuelTypeData;
        protected MyConveyorConnectedGroup m_lastGroup;
        protected Vector3 m_totalThrustOverride;
        protected float m_totalThrustOverridePower;
        private readonly List<MyConveyorConnectedGroup> m_groupsToTrySplit = new List<MyConveyorConnectedGroup>();
        private bool m_mergeAllGroupsDirty = true;
        [ThreadStatic]
        private static List<int> m_tmpGroupIndicesPerThread;
        [ThreadStatic]
        private static List<MyTuple<VRage.Game.Entity.MyEntity, Vector3I>> m_tmpEntitiesWithDirectionsPerThread;
        [ThreadStatic]
        private static List<MyConveyorConnectedGroup> m_tmpGroupsPerThread;
        protected readonly MyConcurrentQueue<MyTuple<VRage.Game.Entity.MyEntity, Vector3I, Func<bool>>> m_thrustEntitiesPending = new MyConcurrentQueue<MyTuple<VRage.Game.Entity.MyEntity, Vector3I, Func<bool>>>();
        protected readonly HashSet<VRage.Game.Entity.MyEntity> m_thrustEntitiesRemovedBeforeRegister = new HashSet<VRage.Game.Entity.MyEntity>();
        private MyConcurrentQueue<IMyConveyorEndpointBlock> m_conveyorEndpointsPending = new MyConcurrentQueue<IMyConveyorEndpointBlock>();
        private MyConcurrentQueue<IMyConveyorSegmentBlock> m_conveyorSegmentsPending = new MyConcurrentQueue<IMyConveyorSegmentBlock>();
        protected bool m_thrustsChanged;
        private Vector3 m_controlThrust;
        private bool m_lastControlThrustChanged;
        private bool m_controlThrustChanged;
        private long m_lastPowerUpdate;
        private Vector3? m_maxThrustOverride;
        private bool m_secondFrameUpdate;
        private bool m_dampenersEnabledLastFrame = true;
        private int m_counter;
        private bool m_enabled;

        protected MyEntityThrustComponent()
        {
            MyResourceDistributorComponent.InitializeMappings();
            this.m_resourceSink = new MyResourceSinkComponent(1);
        }

        private static void AddSinkToSystems(MyResourceSinkComponent resourceSink, MyCubeGrid cubeGrid)
        {
            if (cubeGrid != null)
            {
                MyCubeGridSystems gridSystems = cubeGrid.GridSystems;
                if ((gridSystems != null) && (gridSystems.ResourceDistributor != null))
                {
                    gridSystems.ResourceDistributor.AddSink(resourceSink);
                }
            }
        }

        protected abstract void AddToGroup(VRage.Game.Entity.MyEntity thrustEntity, MyConveyorConnectedGroup group);
        protected virtual Vector3 ApplyThrustModifiers(ref MyDefinitionId fuelType, ref Vector3 thrust, ref Vector3 thrustOverride, MyResourceSinkComponentBase resourceSink)
        {
            thrust += thrustOverride;
            thrust *= resourceSink.SuppliedRatioByType(fuelType);
            thrust *= MyFakes.THRUST_FORCE_RATIO;
            return thrust;
        }

        protected abstract float CalculateConsumptionMultiplier(VRage.Game.Entity.MyEntity thrustEntity, float naturalGravityStrength);
        protected abstract float CalculateForceMultiplier(VRage.Game.Entity.MyEntity thrustEntity, float planetaryInfluence, bool inAtmosphere);
        protected virtual float CalculateMass() => 
            this.Entity.Physics.Mass;

        private void ComputeAiThrust(Vector3 direction, FuelTypeData fuelData)
        {
            Vector3D vectord;
            Vector3 maxPositiveThrust;
            Vector3 maxNegativeThrust;
            Matrix orientation = (Matrix) this.Entity.PositionComp.WorldMatrixNormalizedInv.GetOrientation();
            Vector3 vector = Vector3.Clamp(direction, Vector3.Zero, Vector3.One);
            Vector3 vector2 = Vector3.Clamp(direction, -Vector3.One, Vector3.Zero);
            Vector3 vector3 = Vector3.Clamp(-Vector3.Transform(this.Entity.Physics.Gravity, ref orientation) * this.Entity.Physics.Mass, Vector3.Zero, Vector3.PositiveInfinity);
            Vector3 vector4 = Vector3.Clamp(-Vector3.Transform(this.Entity.Physics.Gravity, ref orientation) * this.Entity.Physics.Mass, Vector3.NegativeInfinity, Vector3.Zero);
            if (this.MaxThrustOverride == null)
            {
                maxPositiveThrust = fuelData.MaxPositiveThrust;
            }
            else
            {
                maxPositiveThrust = this.MaxThrustOverride.Value * Vector3I.Sign(fuelData.MaxPositiveThrust);
            }
            Vector3 max = maxPositiveThrust;
            if (this.MaxThrustOverride == null)
            {
                maxNegativeThrust = fuelData.MaxNegativeThrust;
            }
            else
            {
                maxNegativeThrust = this.MaxThrustOverride.Value * Vector3I.Sign(fuelData.MaxNegativeThrust);
            }
            Vector3 vector6 = maxNegativeThrust;
            Vector3 vector7 = Vector3.Clamp(max - vector3, Vector3.Zero, Vector3.PositiveInfinity);
            Vector3 vector8 = Vector3.Clamp(vector6 + vector4, Vector3.Zero, Vector3.PositiveInfinity);
            Vector3 vector9 = vector7 * vector;
            Vector3 vector10 = vector8 * -vector2;
            Vector3 zero = Vector3.Zero;
            if (Math.Max(vector9.Max(), vector10.Max()) > 0.001f)
            {
                Vector3 vector13 = vector * vector9;
                Vector3 vector14 = -vector2 * vector10;
                Vector3 vector15 = vector7 / vector13;
                Vector3 vector16 = vector8 / vector14;
                if (!vector15.X.IsValid())
                {
                    vector15.X = 1f;
                }
                if (!vector15.Y.IsValid())
                {
                    vector15.Y = 1f;
                }
                if (!vector15.Z.IsValid())
                {
                    vector15.Z = 1f;
                }
                if (!vector16.X.IsValid())
                {
                    vector16.X = 1f;
                }
                if (!vector16.Y.IsValid())
                {
                    vector16.Y = 1f;
                }
                if (!vector16.Z.IsValid())
                {
                    vector16.Z = 1f;
                }
                zero = Vector3.Clamp(((-vector14 * vector16) + (vector13 * vector15)) + (vector3 + vector4), -vector6, max);
            }
            float num = MyFakes.ENABLE_VR_REMOTE_CONTROL_WAYPOINTS_FAST_MOVEMENT ? 0.25f : 0.5f;
            Vector3 vector12 = Vector3.Transform(this.Entity.Physics.LinearVelocity + (this.Entity.Physics.Gravity / 2f), ref orientation);
            if (!Vector3.IsZero(direction))
            {
                vectord = Vector3.Reject(vector12, Vector3.Normalize(direction));
            }
            else
            {
                vectord = vector12;
            }
            zero = Vector3.Clamp(zero + ((-vectord / ((double) num)) * this.Entity.Physics.Mass), -vector6 * this.SlowdownFactor, max * this.SlowdownFactor);
            fuelData.CurrentThrust = zero;
        }

        private void ComputeBaseThrust(ref Vector3 controlThrust, FuelTypeData fuelData, bool applyDampeners, Vector3 dampeningVelocity)
        {
            if (this.Entity.Physics == null)
            {
                fuelData.CurrentThrust = Vector3.Zero;
            }
            else
            {
                Matrix worldMatrixNormalizedInv = (Matrix) this.Entity.PositionComp.WorldMatrixNormalizedInv;
                Vector3 vector = this.Entity.Physics.Gravity * 0.5f;
                Vector3 vector2 = Vector3.TransformNormal((applyDampeners ? this.Entity.Physics.LinearVelocity : Vector3.Zero) + vector, worldMatrixNormalizedInv);
                Vector3 vector3 = Vector3.Clamp(controlThrust, Vector3.Zero, Vector3.One);
                Vector3 zero = Vector3.Zero;
                Vector3 vector5 = dampeningVelocity;
                if (this.DampenersEnabled)
                {
                    zero = Vector3.IsZeroVector(controlThrust, 0.001f) * Vector3.IsZeroVector(fuelData.ThrustOverride);
                    Vector3 vector8 = Vector3.Zero;
                    if (vector2.X > vector5.X)
                    {
                        vector8.X = this.m_totalMaxNegativeThrust.X;
                    }
                    else if (vector2.X < vector5.X)
                    {
                        vector8.X = this.m_totalMaxPositiveThrust.X;
                    }
                    if (vector2.Y > vector5.Y)
                    {
                        vector8.Y = this.m_totalMaxNegativeThrust.Y;
                    }
                    else if (vector2.Y < vector5.Y)
                    {
                        vector8.Y = this.m_totalMaxPositiveThrust.Y;
                    }
                    if (vector2.Z > vector5.Z)
                    {
                        vector8.Z = this.m_totalMaxNegativeThrust.Z;
                    }
                    else if (vector2.Z < vector5.Z)
                    {
                        vector8.Z = this.m_totalMaxPositiveThrust.Z;
                    }
                    Vector3 vector9 = Vector3.Zero;
                    if (vector2.X > vector5.X)
                    {
                        vector9.X = fuelData.MaxNegativeThrust.X;
                    }
                    else if (vector2.X < vector5.X)
                    {
                        vector9.X = fuelData.MaxPositiveThrust.X;
                    }
                    if (vector2.Y > vector5.Y)
                    {
                        vector9.Y = fuelData.MaxNegativeThrust.Y;
                    }
                    else if (vector2.Y < vector5.Y)
                    {
                        vector9.Y = fuelData.MaxPositiveThrust.Y;
                    }
                    if (vector2.Z > vector5.Z)
                    {
                        vector9.Z = fuelData.MaxNegativeThrust.Z;
                    }
                    else if (vector2.Z < vector5.Z)
                    {
                        vector9.Z = fuelData.MaxPositiveThrust.Z;
                    }
                    Vector3 vector10 = vector9 / vector8;
                    if (!vector10.X.IsValid())
                    {
                        vector10.X = 1f;
                    }
                    if (!vector10.Y.IsValid())
                    {
                        vector10.Y = 1f;
                    }
                    if (!vector10.Z.IsValid())
                    {
                        vector10.Z = 1f;
                    }
                    zero *= vector10;
                }
                Vector3 vector6 = Vector3.Clamp((Vector3.Clamp(controlThrust, -Vector3.One, Vector3.Zero) * fuelData.MaxNegativeThrust) + (vector3 * fuelData.MaxPositiveThrust), -fuelData.MaxNegativeThrust, fuelData.MaxPositiveThrust);
                Vector3 vector7 = (((vector5 - vector2) / 0.5f) * this.CalculateMass()) * zero;
                if (!Vector3.IsZero(vector7))
                {
                    this.m_controlThrustChanged = true;
                    this.m_lastControlThrustChanged = this.m_controlThrustChanged;
                }
                vector6 = Vector3.Clamp(vector6 + vector7, -fuelData.MaxNegativeThrust * this.SlowdownFactor, fuelData.MaxPositiveThrust * this.SlowdownFactor);
                fuelData.CurrentThrust = vector6;
            }
        }

        private void ConveyorSystem_OnBeforeRemoveEndpointBlock(IMyConveyorEndpointBlock conveyorEndpointBlock)
        {
            if ((conveyorEndpointBlock != null) && this.IsThrustEntityType(conveyorEndpointBlock as VRage.Game.Entity.MyEntity))
            {
                MyConveyorConnectedGroup item = this.FindEntityGroup(conveyorEndpointBlock as VRage.Game.Entity.MyEntity);
                if (item != null)
                {
                    this.m_groupsToTrySplit.Add(item);
                }
            }
        }

        private void ConveyorSystem_OnBeforeRemoveSegmentBlock(IMyConveyorSegmentBlock conveyorSegmentBlock)
        {
            if (conveyorSegmentBlock != null)
            {
                MyConveyorConnectedGroup item = this.FindEntityGroup(conveyorSegmentBlock as VRage.Game.Entity.MyEntity);
                if (item != null)
                {
                    this.m_groupsToTrySplit.Add(item);
                }
            }
        }

        private void ConveyorSystem_OnPoweredChanged()
        {
            this.MergeAllGroupsDirty();
        }

        private void CubeGrid_OnBlockAdded(MySlimBlock addedBlock)
        {
            MyCubeBlock fatBlock = addedBlock.FatBlock;
            if (fatBlock != null)
            {
                IMyConveyorEndpointBlock instance = fatBlock as IMyConveyorEndpointBlock;
                IMyConveyorSegmentBlock block3 = fatBlock as IMyConveyorSegmentBlock;
                if ((instance != null) && !this.IsThrustEntityType(instance as VRage.Game.Entity.MyEntity))
                {
                    this.m_conveyorEndpointsPending.Enqueue(instance);
                }
                else if (block3 != null)
                {
                    this.m_conveyorSegmentsPending.Enqueue(block3);
                }
            }
        }

        private static void FindConnectedGroups(IMyConveyorEndpointBlock block, List<MyConveyorConnectedGroup> groups, List<int> outConnectedGroupIndices)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                MyConveyorConnectedGroup group = groups[i];
                if ((group.FirstEndpoint != null) && MyGridConveyorSystem.Reachable(group.FirstEndpoint, block.ConveyorEndpoint))
                {
                    outConnectedGroupIndices.Add(i);
                }
            }
        }

        private static void FindConnectedGroups(IMyConveyorSegmentBlock block, List<MyConveyorConnectedGroup> groups, List<int> outConnectedGroupIndices)
        {
            if (block.ConveyorSegment.ConveyorLine != null)
            {
                IMyConveyorEndpoint endpoint1 = block.ConveyorSegment.ConveyorLine.GetEndpoint(0);
                IMyConveyorEndpoint to = endpoint1 ?? block.ConveyorSegment.ConveyorLine.GetEndpoint(1);
                if (to != null)
                {
                    for (int i = 0; i < groups.Count; i++)
                    {
                        if (MyGridConveyorSystem.Reachable(groups[i].FirstEndpoint, to))
                        {
                            outConnectedGroupIndices.Add(i);
                        }
                    }
                }
            }
        }

        private MyConveyorConnectedGroup FindEntityGroup(VRage.Game.Entity.MyEntity thrustEntity)
        {
            MyConveyorConnectedGroup group = null;
            if (!this.IsThrustEntityType(thrustEntity))
            {
                IMyConveyorEndpoint to = null;
                IMyConveyorEndpointBlock block = thrustEntity as IMyConveyorEndpointBlock;
                IMyConveyorSegmentBlock block2 = thrustEntity as IMyConveyorSegmentBlock;
                if (block != null)
                {
                    to = block.ConveyorEndpoint;
                }
                else if ((block2 != null) && (block2.ConveyorSegment.ConveyorLine != null))
                {
                    IMyConveyorEndpoint endpoint = block2.ConveyorSegment.ConveyorLine.GetEndpoint(0);
                    to = endpoint ?? block2.ConveyorSegment.ConveyorLine.GetEndpoint(1);
                }
                if (to == null)
                {
                    return group;
                }
                else
                {
                    foreach (MyConveyorConnectedGroup group2 in this.m_connectedGroups)
                    {
                        if (MyGridConveyorSystem.Reachable(group2.FirstEndpoint, to))
                        {
                            group = group2;
                            break;
                        }
                    }
                    return group;
                }
            }
            if (MyResourceDistributorComponent.IsConveyorConnectionRequiredTotal(this.FuelType(thrustEntity)))
            {
                MyDefinitionId fuelId = this.FuelType(thrustEntity);
                foreach (MyConveyorConnectedGroup group3 in this.m_connectedGroups)
                {
                    int num;
                    if (group3.TryGetTypeIndex(ref fuelId, out num))
                    {
                        using (Dictionary<Vector3I, HashSet<VRage.Game.Entity.MyEntity>>.ValueCollection.Enumerator enumerator2 = group3.DataByFuelType[num].ThrustsByDirection.Values.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                if (enumerator2.Current.Contains(thrustEntity))
                                {
                                    group = group3;
                                    break;
                                }
                            }
                        }
                        if (group != null)
                        {
                            break;
                        }
                    }
                }
            }
            return group;
        }

        private void FlipNegativeInfinity(ref Vector3 v)
        {
            if (float.IsNegativeInfinity(v.X))
            {
                v.X = float.PositiveInfinity;
            }
            if (float.IsNegativeInfinity(v.Y))
            {
                v.Y = float.PositiveInfinity;
            }
            if (float.IsNegativeInfinity(v.Z))
            {
                v.Z = float.PositiveInfinity;
            }
        }

        protected abstract float ForceMagnitude(VRage.Game.Entity.MyEntity thrustEntity, float planetaryInfluence, bool inAtmosphere);
        protected abstract MyDefinitionId FuelType(VRage.Game.Entity.MyEntity thrustEntity);
        public unsafe Vector3 GetAutoPilotThrustForDirection(Vector3 direction)
        {
            foreach (FuelTypeData data in this.m_dataByFuelType)
            {
                this.ComputeAiThrust(this.AutoPilotControlThrust, data);
            }
            using (List<MyConveyorConnectedGroup>.Enumerator enumerator2 = this.m_connectedGroups.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    foreach (FuelTypeData data2 in enumerator2.Current.DataByFuelType)
                    {
                        this.ComputeAiThrust(this.AutoPilotControlThrust, data2);
                    }
                }
            }
            Vector3 vector = new Vector3();
            for (int i = 0; i < this.m_dataByFuelType.Count; i++)
            {
                Vector3 vector2;
                Vector3* vectorPtr1;
                Vector3* vectorPtr2;
                Vector3* vectorPtr3;
                MyDefinitionId fuelType = this.m_fuelTypes[i];
                FuelTypeData data3 = this.m_dataByFuelType[i];
                this.UpdatePowerAndThrustStrength(data3.CurrentThrust, fuelType, null, true);
                Vector3 vector3 = this.m_maxPositiveThrust + this.m_maxNegativeThrust;
                vectorPtr1->X = (vector3.X != 0f) ? ((data3.CurrentThrust.X * (data3.MaxPositiveThrust.X + data3.MaxNegativeThrust.X)) / vector3.X) : 0f;
                vectorPtr1 = (Vector3*) ref vector2;
                vectorPtr2->Y = (vector3.Y != 0f) ? ((data3.CurrentThrust.Y * (data3.MaxPositiveThrust.Y + data3.MaxNegativeThrust.Y)) / vector3.Y) : 0f;
                vectorPtr2 = (Vector3*) ref vector2;
                vectorPtr3->Z = (vector3.Z != 0f) ? ((data3.CurrentThrust.Z * (data3.MaxPositiveThrust.Z + data3.MaxNegativeThrust.Z)) / vector3.Z) : 0f;
                vectorPtr3 = (Vector3*) ref vector2;
                vector += this.ApplyThrustModifiers(ref fuelType, ref vector2, ref this.m_totalThrustOverride, this.m_resourceSink);
            }
            foreach (MyConveyorConnectedGroup group in this.m_connectedGroups)
            {
                for (int j = 0; j < group.DataByFuelType.Count; j++)
                {
                    Vector3 vector4;
                    Vector3* vectorPtr4;
                    Vector3* vectorPtr5;
                    Vector3* vectorPtr6;
                    MyDefinitionId fuelType = group.FuelTypes[j];
                    FuelTypeData data4 = group.DataByFuelType[j];
                    this.UpdatePowerAndThrustStrength(data4.CurrentThrust, fuelType, group, true);
                    Vector3 vector5 = group.MaxPositiveThrust + group.MaxNegativeThrust;
                    vectorPtr4->X = (vector5.X != 0f) ? ((data4.CurrentThrust.X * (data4.MaxPositiveThrust.X + data4.MaxNegativeThrust.X)) / vector5.X) : 0f;
                    vectorPtr4 = (Vector3*) ref vector4;
                    vectorPtr5->Y = (vector5.Y != 0f) ? ((data4.CurrentThrust.Y * (data4.MaxPositiveThrust.Y + data4.MaxNegativeThrust.Y)) / vector5.Y) : 0f;
                    vectorPtr5 = (Vector3*) ref vector4;
                    vectorPtr6->Z = (vector5.Z != 0f) ? ((data4.CurrentThrust.Z * (data4.MaxPositiveThrust.Z + data4.MaxNegativeThrust.Z)) / vector5.Z) : 0f;
                    vectorPtr6 = (Vector3*) ref vector4;
                    vector += this.ApplyThrustModifiers(ref fuelType, ref vector4, ref group.ThrustOverride, group.ResourceSink);
                }
            }
            this.m_lastControlThrustChanged = this.m_controlThrustChanged;
            this.m_controlThrustChanged = false;
            return vector;
        }

        public float GetLastThrustMultiplier(VRage.Game.Entity.MyEntity thrustEntity) => 
            this.CalculateForceMultiplier(thrustEntity, this.m_lastPlanetaryInfluence, this.m_lastPlanetaryInfluenceHasAtmosphere);

        protected float GetMaxPowerRequirement(FuelTypeData typeData, ref Vector3I direction) => 
            typeData.MaxRequirementsByDirection[direction];

        private float GetMaxRequirementsByDirection(FuelTypeData fuelData, Vector3I direction)
        {
            float num;
            return (!fuelData.MaxRequirementsByDirection.TryGetValue(direction, out num) ? 0f : num);
        }

        public float GetMaxThrustInDirection(Base6Directions.Direction direction)
        {
            switch (direction)
            {
                case Base6Directions.Direction.Backward:
                    return this.m_maxNegativeThrust.Z;

                case Base6Directions.Direction.Left:
                    return this.m_maxNegativeThrust.X;

                case Base6Directions.Direction.Right:
                    return this.m_maxPositiveThrust.X;

                case Base6Directions.Direction.Up:
                    return this.m_maxPositiveThrust.Y;

                case Base6Directions.Direction.Down:
                    return this.m_maxNegativeThrust.Y;
            }
            return this.m_maxPositiveThrust.Z;
        }

        protected int GetTypeIndex(ref MyDefinitionId fuelId)
        {
            int num2;
            int num = 0;
            if ((this.m_fuelTypeToIndex.Count > 1) && this.m_fuelTypeToIndex.TryGetValue(fuelId, out num2))
            {
                num = num2;
            }
            return num;
        }

        public bool HasThrustersInAllDirections(MyDefinitionId fuelId)
        {
            int num;
            if (!this.m_fuelTypeToIndex.TryGetValue(fuelId, out num))
            {
                return false;
            }
            FuelTypeData data = this.m_dataByFuelType[num];
            return ((((((true & (data.ThrustsByDirection[Vector3I.Backward].Count > 0)) & (data.ThrustsByDirection[Vector3I.Forward].Count > 0)) & (data.ThrustsByDirection[Vector3I.Up].Count > 0)) & (data.ThrustsByDirection[Vector3I.Down].Count > 0)) & (data.ThrustsByDirection[Vector3I.Left].Count > 0)) & (data.ThrustsByDirection[Vector3I.Right].Count > 0));
        }

        public virtual void Init()
        {
            this.Enabled = true;
            this.ThrustCount = 0;
            this.DampenersEnabled = true;
            this.MarkDirty(false);
            this.m_lastPowerUpdate = MySession.Static.GameplayFrameCounter;
        }

        private int InitializeType(MyDefinitionId fuelType, List<FuelTypeData> dataByTypeList, List<MyDefinitionId> fuelTypeList, Dictionary<MyDefinitionId, int> fuelTypeToIndex, MyResourceSinkComponent resourceSink)
        {
            FuelTypeData item = new FuelTypeData();
            item.ThrustsByDirection = new Dictionary<Vector3I, HashSet<VRage.Game.Entity.MyEntity>>(6, m_directionComparer);
            item.MaxRequirementsByDirection = new Dictionary<Vector3I, float>(6, m_directionComparer);
            item.CurrentRequiredFuelInput = 0.0001f;
            item.Efficiency = 0f;
            item.EnergyDensity = 0f;
            dataByTypeList.Add(item);
            int typeIndex = dataByTypeList.Count - 1;
            fuelTypeToIndex.Add(fuelType, typeIndex);
            fuelTypeList.Add(fuelType);
            foreach (Vector3I vectori in Base6Directions.IntDirections)
            {
                dataByTypeList[typeIndex].ThrustsByDirection[vectori] = new HashSet<VRage.Game.Entity.MyEntity>();
            }
            MyResourceSinkInfo sinkData = new MyResourceSinkInfo {
                ResourceTypeId = fuelType,
                MaxRequiredInput = 0f,
                RequiredInputFunc = () => RequiredFuelInput(dataByTypeList[typeIndex])
            };
            if (fuelTypeList.Count != 1)
            {
                resourceSink.AddType(ref sinkData);
            }
            else
            {
                resourceSink.Init(MyStringHash.GetOrCompute("Thrust"), sinkData);
                resourceSink.IsPoweredChanged += new Action(this.Sink_IsPoweredChanged);
                resourceSink.CurrentInputChanged += new MyCurrentResourceInputChangedDelegate(this.Sink_CurrentInputChanged);
                AddSinkToSystems(resourceSink, base.Container.Entity as MyCubeGrid);
            }
            return typeIndex;
        }

        public bool IsRegistered(VRage.Game.Entity.MyEntity entity, Vector3I forwardVector)
        {
            int num2;
            bool flag = false;
            MyDefinitionId typeId = this.FuelType(entity);
            IMyConveyorEndpointBlock block = entity as IMyConveyorEndpointBlock;
            if (MyResourceDistributorComponent.IsConveyorConnectionRequiredTotal(ref typeId) && (block != null))
            {
                foreach (MyConveyorConnectedGroup group in this.m_connectedGroups)
                {
                    int num;
                    if (group.TryGetTypeIndex(ref typeId, out num) && group.DataByFuelType[num].ThrustsByDirection[forwardVector].Contains(entity))
                    {
                        flag = true;
                        break;
                    }
                }
                return flag;
            }
            if (this.TryGetTypeIndex(ref typeId, out num2))
            {
                flag = this.m_dataByFuelType[num2].ThrustsByDirection[forwardVector].Contains(entity);
            }
            return flag;
        }

        protected abstract bool IsThrustEntityType(VRage.Game.Entity.MyEntity thrustEntity);
        public bool IsThrustPoweredByType(VRage.Game.Entity.MyEntity thrustEntity, ref MyDefinitionId fuelId) => 
            this.ResourceSink(thrustEntity).IsPoweredByType(fuelId);

        protected abstract bool IsUsed(VRage.Game.Entity.MyEntity thrustEntity);
        public void MarkDirty(bool recomputePlanetaryInfluence = false)
        {
            this.m_thrustsChanged = true;
            this.m_controlThrustChanged = true;
            this.m_nextPlanetaryInfluenceRecalculation = 0;
            if (this.Entity is MyCubeGrid)
            {
                (this.Entity as MyCubeGrid).MarkForUpdate();
            }
        }

        protected abstract float MaxPowerConsumption(VRage.Game.Entity.MyEntity thrustEntity);
        public void MergeAllGroupsDirty()
        {
            this.m_mergeAllGroupsDirty = true;
        }

        private void MergeGroups(List<MyConveyorConnectedGroup> groups, List<int> connectedGroupIndices)
        {
            int num = -2147483648;
            int thrustCount = -2147483648;
            foreach (int num3 in connectedGroupIndices)
            {
                MyConveyorConnectedGroup group2 = groups[num3];
                if (group2.ThrustCount > thrustCount)
                {
                    num = num3;
                    thrustCount = group2.ThrustCount;
                }
            }
            MyConveyorConnectedGroup group = groups[num];
            foreach (int num4 in connectedGroupIndices)
            {
                if (num4 != num)
                {
                    MyConveyorConnectedGroup group3 = groups[num4];
                    foreach (MyDefinitionId id in group3.FuelTypes)
                    {
                        int num5;
                        MyDefinitionId fuelId = id;
                        if (!group.TryGetTypeIndex(ref fuelId, out num5))
                        {
                            num5 = this.InitializeType(id, group.DataByFuelType, group.FuelTypes, group.FuelTypeToIndex, group.ResourceSink);
                        }
                        FuelTypeData data = group.DataByFuelType[num5];
                        FuelTypeData data2 = group3.DataByFuelType[num5];
                        data.MaxRequiredPowerInput += data2.MaxRequiredPowerInput;
                        data.MinRequiredPowerInput += data2.MinRequiredPowerInput;
                        data.CurrentRequiredFuelInput += data2.CurrentRequiredFuelInput;
                        data.MaxNegativeThrust += data2.MaxNegativeThrust;
                        data.MaxPositiveThrust += data2.MaxPositiveThrust;
                        data.ThrustOverride += data2.ThrustOverride;
                        data.ThrustOverridePower += data2.ThrustOverridePower;
                        data.ThrustCount += data2.ThrustCount;
                        Vector3I[] intDirections = Base6Directions.IntDirections;
                        int index = 0;
                        while (true)
                        {
                            float num7;
                            if (index >= intDirections.Length)
                            {
                                group.ThrustCount += group3.ThrustCount;
                                group.ThrustOverride += group3.ThrustOverride;
                                group.ThrustOverridePower += group3.ThrustOverridePower;
                                group.MaxNegativeThrust += group3.MaxNegativeThrust;
                                group.MaxPositiveThrust += group3.MaxPositiveThrust;
                                RemoveSinkFromSystems(group3.ResourceSink, base.Container.Entity as MyCubeGrid);
                                break;
                            }
                            Vector3I key = intDirections[index];
                            if (data2.MaxRequirementsByDirection.TryGetValue(key, out num7))
                            {
                                float num8;
                                data.MaxRequirementsByDirection[key] = !data.MaxRequirementsByDirection.TryGetValue(key, out num8) ? num7 : (num8 + num7);
                            }
                            if (!data.ThrustsByDirection.ContainsKey(key))
                            {
                                data.ThrustsByDirection[key] = new HashSet<VRage.Game.Entity.MyEntity>();
                            }
                            HashSet<VRage.Game.Entity.MyEntity> set = data.ThrustsByDirection[key];
                            if (data2.ThrustsByDirection.ContainsKey(key))
                            {
                                foreach (VRage.Game.Entity.MyEntity entity in data2.ThrustsByDirection[key])
                                {
                                    set.Add(entity);
                                    entity.Components.Remove<MyResourceSinkComponent>();
                                }
                            }
                            index++;
                        }
                    }
                }
            }
            connectedGroupIndices.Sort();
            for (int i = connectedGroupIndices.Count - 1; i >= 0; i--)
            {
                if (connectedGroupIndices[i] != num)
                {
                    if (connectedGroupIndices[i] < num)
                    {
                        num--;
                    }
                    groups.RemoveAtFast<MyConveyorConnectedGroup>(connectedGroupIndices[i]);
                    connectedGroupIndices.RemoveAt(i);
                }
            }
            group.ResourceSink.Update();
            connectedGroupIndices[0] = num;
        }

        protected abstract float MinPowerConsumption(VRage.Game.Entity.MyEntity thrustEntity);
        private void MoveSinkToNewEntity(VRage.Game.Entity.MyEntity entity, List<FuelTypeData> fuelData, int typeIndex, int thrustsLeftInGroup, MyResourceSinkComponentBase resourceSink, MyConveyorConnectedGroup containingGroup)
        {
            if ((base.Container.Entity is MyCubeGrid) && (entity.Components.Get<MyResourceSinkComponent>() == resourceSink))
            {
                entity.Components.Remove<MyResourceSinkComponent>();
                if (thrustsLeftInGroup > 0)
                {
                    foreach (HashSet<VRage.Game.Entity.MyEntity> set in fuelData[typeIndex].ThrustsByDirection.Values)
                    {
                        if (set.Count > 0)
                        {
                            bool flag = false;
                            foreach (VRage.Game.Entity.MyEntity entity2 in set)
                            {
                                if (!ReferenceEquals(entity2, entity))
                                {
                                    entity2.Components.Add<MyResourceSinkComponentBase>(resourceSink);
                                    AddSinkToSystems(resourceSink as MyResourceSinkComponent, this.Entity as MyCubeGrid);
                                    flag = true;
                                    if (containingGroup != null)
                                    {
                                        containingGroup.FirstEndpoint = (entity2 as IMyConveyorEndpointBlock).ConveyorEndpoint;
                                    }
                                    break;
                                }
                            }
                            if (flag)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            MyPlanets.Static.OnPlanetAdded += new Action<MyPlanet>(this.OnPlanetAddedOrRemoved);
            MyPlanets.Static.OnPlanetRemoved += new Action<MyPlanet>(this.OnPlanetAddedOrRemoved);
            MyCubeGrid entity = this.Entity as MyCubeGrid;
            if (entity != null)
            {
                entity.OnBlockAdded += new Action<MySlimBlock>(this.CubeGrid_OnBlockAdded);
                entity.GridSystems.ConveyorSystem.OnBeforeRemoveSegmentBlock += new Action<IMyConveyorSegmentBlock>(this.ConveyorSystem_OnBeforeRemoveSegmentBlock);
                entity.GridSystems.ConveyorSystem.OnBeforeRemoveEndpointBlock += new Action<IMyConveyorEndpointBlock>(this.ConveyorSystem_OnBeforeRemoveEndpointBlock);
                entity.GridSystems.ConveyorSystem.ResourceSink.IsPoweredChanged += new Action(this.ConveyorSystem_OnPoweredChanged);
            }
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            MyPlanets.Static.OnPlanetAdded -= new Action<MyPlanet>(this.OnPlanetAddedOrRemoved);
            MyPlanets.Static.OnPlanetRemoved -= new Action<MyPlanet>(this.OnPlanetAddedOrRemoved);
            using (List<MyConveyorConnectedGroup>.Enumerator enumerator = this.m_connectedGroups.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    RemoveSinkFromSystems(enumerator.Current.ResourceSink, base.Container.Entity as MyCubeGrid);
                }
            }
            RemoveSinkFromSystems(this.m_resourceSink, base.Container.Entity as MyCubeGrid);
            MyCubeGrid entity = this.Entity as MyCubeGrid;
            if (entity != null)
            {
                entity.OnBlockAdded -= new Action<MySlimBlock>(this.CubeGrid_OnBlockAdded);
                entity.GridSystems.ConveyorSystem.OnBeforeRemoveSegmentBlock -= new Action<IMyConveyorSegmentBlock>(this.ConveyorSystem_OnBeforeRemoveSegmentBlock);
                entity.GridSystems.ConveyorSystem.OnBeforeRemoveEndpointBlock -= new Action<IMyConveyorEndpointBlock>(this.ConveyorSystem_OnBeforeRemoveEndpointBlock);
                entity.GridSystems.ConveyorSystem.ResourceSink.IsPoweredChanged -= new Action(this.ConveyorSystem_OnPoweredChanged);
            }
        }

        private void OnPlanetAddedOrRemoved(MyPlanet planet)
        {
            if (this.Entity != null)
            {
                BoundingBoxD worldAABB = this.Entity.PositionComp.WorldAABB;
                if (planet.IntersectsWithGravityFast(ref worldAABB))
                {
                    this.MarkDirty(true);
                }
            }
        }

        protected float PowerAmountToFuel(ref MyDefinitionId fuelType, float powerAmount, MyConveyorConnectedGroup group)
        {
            int typeIndex = 0;
            if ((group == null) && !this.TryGetTypeIndex(ref fuelType, out typeIndex))
            {
                return 0f;
            }
            if ((group != null) && !group.TryGetTypeIndex(ref fuelType, out typeIndex))
            {
                return 0f;
            }
            List<FuelTypeData> list = (group != null) ? group.DataByFuelType : this.m_dataByFuelType;
            return (powerAmount / (list[typeIndex].Efficiency * list[typeIndex].EnergyDensity));
        }

        private void RecalculatePlanetaryInfluence()
        {
            BoundingBoxD worldAABB = this.Entity.PositionComp.WorldAABB;
            MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(ref worldAABB);
            float airDensity = 0f;
            if (closestPlanet == null)
            {
                this.m_nextPlanetaryInfluenceRecalculation = MySession.Static.GameplayFrameCounter + Math.Min(0x3e8, 0x2710);
            }
            else
            {
                airDensity = closestPlanet.GetAirDensity(worldAABB.Center);
                this.m_lastPlanetaryInfluenceHasAtmosphere = closestPlanet.HasAtmosphere;
                this.m_lastPlanetaryGravityMagnitude = closestPlanet.Components.Get<MyGravityProviderComponent>().GetGravityMultiplier(this.Entity.PositionComp.WorldMatrix.Translation);
                this.m_nextPlanetaryInfluenceRecalculation = MySession.Static.GameplayFrameCounter + Math.Min(100, 0x2710);
            }
            if (this.m_lastPlanetaryInfluence != airDensity)
            {
                this.MarkDirty(false);
                this.m_lastPlanetaryInfluence = airDensity;
            }
        }

        protected abstract bool RecomputeOverriddenParameters(VRage.Game.Entity.MyEntity thrustEntity, FuelTypeData fuelData);
        private void RecomputeThrustParameters()
        {
            this.m_secondFrameUpdate = true;
            if (!this.m_thrustsChanged && this.m_secondFrameUpdate)
            {
                this.m_secondFrameUpdate = false;
            }
            this.m_totalThrustOverride = Vector3.Zero;
            this.m_totalThrustOverridePower = 0f;
            this.m_maxPositiveThrust = new Vector3();
            this.m_maxNegativeThrust = new Vector3();
            this.m_totalMaxNegativeThrust = new Vector3();
            this.m_totalMaxPositiveThrust = new Vector3();
            this.MaxRequiredPowerInput = 0f;
            this.MinRequiredPowerInput = 0f;
            foreach (FuelTypeData data in this.m_dataByFuelType)
            {
                this.RecomputeTypeThrustParameters(data);
                this.MaxRequiredPowerInput += data.MaxRequiredPowerInput;
                this.MinRequiredPowerInput += data.MinRequiredPowerInput;
                this.m_maxPositiveThrust += data.MaxPositiveThrust;
                this.m_maxNegativeThrust += data.MaxNegativeThrust;
                this.m_totalThrustOverride += data.ThrustOverride;
                this.m_totalThrustOverridePower += data.ThrustOverridePower;
            }
            this.m_totalMaxNegativeThrust += this.m_maxNegativeThrust;
            this.m_totalMaxPositiveThrust += this.m_maxPositiveThrust;
            foreach (MyConveyorConnectedGroup group in this.m_connectedGroups)
            {
                group.MaxPositiveThrust = new Vector3();
                group.MaxNegativeThrust = new Vector3();
                group.ThrustOverride = new Vector3();
                group.ThrustOverridePower = 0f;
                foreach (FuelTypeData data2 in group.DataByFuelType)
                {
                    this.RecomputeTypeThrustParameters(data2);
                    this.MaxRequiredPowerInput += data2.MaxRequiredPowerInput;
                    this.MinRequiredPowerInput += data2.MinRequiredPowerInput;
                    group.MaxPositiveThrust += data2.MaxPositiveThrust;
                    group.MaxNegativeThrust += data2.MaxNegativeThrust;
                    group.ThrustOverride += data2.ThrustOverride;
                    group.ThrustOverridePower += data2.ThrustOverridePower;
                }
                this.m_totalMaxNegativeThrust += group.MaxNegativeThrust;
                this.m_totalMaxPositiveThrust += group.MaxPositiveThrust;
            }
        }

        private void RecomputeTypeThrustParameters(FuelTypeData fuelData)
        {
            fuelData.MaxRequiredPowerInput = 0f;
            fuelData.MinRequiredPowerInput = 0f;
            fuelData.MaxPositiveThrust = new Vector3();
            fuelData.MaxNegativeThrust = new Vector3();
            fuelData.MaxRequirementsByDirection.Clear();
            fuelData.ThrustOverride = new Vector3();
            fuelData.ThrustOverridePower = 0f;
            foreach (KeyValuePair<Vector3I, HashSet<VRage.Game.Entity.MyEntity>> pair in fuelData.ThrustsByDirection)
            {
                if (!fuelData.MaxRequirementsByDirection.ContainsKey(pair.Key))
                {
                    fuelData.MaxRequirementsByDirection[pair.Key] = 0f;
                }
                float num = 0f;
                foreach (VRage.Game.Entity.MyEntity entity in pair.Value)
                {
                    if (this.RecomputeOverriddenParameters(entity, fuelData))
                    {
                        continue;
                    }
                    if (this.IsUsed(entity))
                    {
                        float num2 = this.ForceMagnitude(entity, this.m_lastPlanetaryInfluence, this.m_lastPlanetaryInfluenceHasAtmosphere);
                        float num3 = this.CalculateForceMultiplier(entity, this.m_lastPlanetaryInfluence, this.m_lastPlanetaryInfluenceHasAtmosphere);
                        float num4 = this.CalculateConsumptionMultiplier(entity, this.m_lastPlanetaryGravityMagnitude);
                        if (!(entity is MyThrust) || (entity as MyThrust).IsPowered)
                        {
                            fuelData.MaxPositiveThrust += Vector3.Clamp(-pair.Key * num2, Vector3.Zero, Vector3.PositiveInfinity);
                            fuelData.MaxNegativeThrust += -Vector3.Clamp(-pair.Key * num2, Vector3.NegativeInfinity, Vector3.Zero);
                        }
                        else
                        {
                            fuelData.MaxPositiveThrust += 0f;
                            fuelData.MaxNegativeThrust += 0f;
                        }
                        num += (this.MaxPowerConsumption(entity) * num3) * num4;
                        fuelData.MinRequiredPowerInput += this.MinPowerConsumption(entity) * num4;
                    }
                }
                Dictionary<Vector3I, float> maxRequirementsByDirection = fuelData.MaxRequirementsByDirection;
                Vector3I key = pair.Key;
                maxRequirementsByDirection[key] += num;
            }
            fuelData.MaxRequiredPowerInput += Math.Max(this.GetMaxRequirementsByDirection(fuelData, Vector3I.Forward), this.GetMaxRequirementsByDirection(fuelData, Vector3I.Backward));
            fuelData.MaxRequiredPowerInput += Math.Max(this.GetMaxRequirementsByDirection(fuelData, Vector3I.Left), this.GetMaxRequirementsByDirection(fuelData, Vector3I.Right));
            fuelData.MaxRequiredPowerInput += Math.Max(this.GetMaxRequirementsByDirection(fuelData, Vector3I.Up), this.GetMaxRequirementsByDirection(fuelData, Vector3I.Down));
        }

        public virtual void Register(VRage.Game.Entity.MyEntity entity, Vector3I forwardVector, Func<bool> onRegisteredCallback = null)
        {
            Dictionary<Vector3I, HashSet<VRage.Game.Entity.MyEntity>> thrustsByDirection;
            MyResourceSinkComponent resourceSink;
            MyDefinitionId typeId = this.FuelType(entity);
            int num = -1;
            int typeIndex = -1;
            IMyConveyorEndpointBlock block = entity as IMyConveyorEndpointBlock;
            if (!MyResourceDistributorComponent.IsConveyorConnectionRequiredTotal(ref typeId) || (block == null))
            {
                if (this.TryGetTypeIndex(ref typeId, out typeIndex))
                {
                    entity.Components.Remove<MyResourceSinkComponent>();
                }
                else
                {
                    typeIndex = this.InitializeType(typeId, this.m_dataByFuelType, this.m_fuelTypes, this.m_fuelTypeToIndex, this.m_resourceSink);
                    if (this.m_fuelTypes.Count == 1)
                    {
                        entity.Components.Add<MyResourceSinkComponent>(this.m_resourceSink);
                    }
                }
                thrustsByDirection = this.m_dataByFuelType[typeIndex].ThrustsByDirection;
                resourceSink = this.m_resourceSink;
                FuelTypeData local2 = this.m_dataByFuelType[typeIndex];
                local2.ThrustCount++;
            }
            else
            {
                MyConveyorConnectedGroup group;
                FindConnectedGroups(block, this.m_connectedGroups, m_tmpGroupIndices);
                if (m_tmpGroupIndices.Count < 1)
                {
                    group = new MyConveyorConnectedGroup(block);
                    this.m_connectedGroups.Add(group);
                    num = this.m_connectedGroups.Count - 1;
                }
                else
                {
                    if (m_tmpGroupIndices.Count > 1)
                    {
                        this.MergeGroups(this.m_connectedGroups, m_tmpGroupIndices);
                    }
                    num = m_tmpGroupIndices[0];
                    group = this.m_connectedGroups[num];
                }
                if (!group.TryGetTypeIndex(ref typeId, out typeIndex))
                {
                    typeIndex = this.InitializeType(typeId, group.DataByFuelType, group.FuelTypes, group.FuelTypeToIndex, group.ResourceSink);
                    if (group.FuelTypes.Count == 1)
                    {
                        entity.Components.Add<MyResourceSinkComponent>(group.ResourceSink);
                    }
                }
                group.ThrustCount++;
                FuelTypeData local1 = group.DataByFuelType[typeIndex];
                local1.ThrustCount++;
                thrustsByDirection = group.DataByFuelType[typeIndex].ThrustsByDirection;
                resourceSink = group.ResourceSink;
                m_tmpGroupIndices.Clear();
            }
            this.m_lastSink = resourceSink;
            this.m_lastGroup = (num == -1) ? null : this.m_connectedGroups[num];
            this.m_lastFuelTypeData = (num == -1) ? this.m_dataByFuelType[typeIndex] : this.m_connectedGroups[num].DataByFuelType[typeIndex];
            thrustsByDirection[forwardVector].Add(entity);
            int num3 = this.ThrustCount + 1;
            this.ThrustCount = num3;
            this.MarkDirty(false);
        }

        protected virtual bool RegisterLazy(VRage.Game.Entity.MyEntity entity, Vector3I forwardVector, Func<bool> onRegisteredCallback) => 
            true;

        protected abstract void RemoveFromGroup(VRage.Game.Entity.MyEntity thrustEntity, MyConveyorConnectedGroup group);
        private static void RemoveSinkFromSystems(MyResourceSinkComponentBase resourceSink, MyCubeGrid cubeGrid)
        {
            if (cubeGrid != null)
            {
                MyCubeGridSystems gridSystems = cubeGrid.GridSystems;
                if ((gridSystems != null) && (gridSystems.ResourceDistributor != null))
                {
                    gridSystems.ResourceDistributor.RemoveSink(resourceSink as MyResourceSinkComponent, true, false);
                }
            }
        }

        private static float RequiredFuelInput(FuelTypeData typeData) => 
            typeData.CurrentRequiredFuelInput;

        public MyResourceSinkComponent ResourceSink(VRage.Game.Entity.MyEntity thrustEntity)
        {
            MyConveyorConnectedGroup group = this.FindEntityGroup(thrustEntity);
            return ((group == null) ? this.m_resourceSink : group.ResourceSink);
        }

        public void ResourceSinks(HashSet<MyResourceSinkComponent> outResourceSinks)
        {
            if (this.m_resourceSink != null)
            {
                outResourceSinks.Add(this.m_resourceSink);
            }
            foreach (MyConveyorConnectedGroup group in this.m_connectedGroups)
            {
                if (group.ResourceSink != null)
                {
                    outResourceSinks.Add(group.ResourceSink);
                }
            }
        }

        internal void SetRequiredFuelInput(ref MyDefinitionId fuelType, float newFuelInput, MyConveyorConnectedGroup group)
        {
            int typeIndex = 0;
            if (((group != null) || this.TryGetTypeIndex(ref fuelType, out typeIndex)) && ((group == null) || group.TryGetTypeIndex(ref fuelType, out typeIndex)))
            {
                ((group != null) ? group.DataByFuelType : this.m_dataByFuelType)[typeIndex].CurrentRequiredFuelInput = newFuelInput;
            }
        }

        private void Sink_CurrentInputChanged(MyDefinitionId resourceTypeId, float oldInput, MyResourceSinkComponent sink)
        {
            this.m_controlThrustChanged = true;
        }

        private void Sink_IsPoweredChanged()
        {
            this.MarkDirty(false);
        }

        private bool TryGetTypeIndex(ref MyDefinitionId fuelId, out int typeIndex)
        {
            typeIndex = 0;
            return (((this.m_fuelTypeToIndex.Count <= 1) || this.m_fuelTypeToIndex.TryGetValue(fuelId, out typeIndex)) && (this.m_fuelTypeToIndex.Count > 0));
        }

        private void TryMergeAllGroups()
        {
            if ((this.m_connectedGroups != null) && (this.m_connectedGroups.Count != 0))
            {
                int num = 0;
                while (true)
                {
                    MyConveyorConnectedGroup group = this.m_connectedGroups[num];
                    IMyConveyorEndpointBlock block = (group.FirstEndpoint != null) ? (group.FirstEndpoint.CubeBlock as IMyConveyorEndpointBlock) : null;
                    if (block != null)
                    {
                        FindConnectedGroups(block, this.m_connectedGroups, m_tmpGroupIndices);
                        if (m_tmpGroupIndices.Count > 1)
                        {
                            this.MergeGroups(this.m_connectedGroups, m_tmpGroupIndices);
                            num--;
                        }
                        m_tmpGroupIndices.Clear();
                        num++;
                    }
                    if (num >= this.m_connectedGroups.Count)
                    {
                        return;
                    }
                }
            }
        }

        private MyConveyorConnectedGroup TrySplitGroup(IMyConveyorEndpointBlock conveyorEndpointBlock, MyConveyorConnectedGroup groupOverride = null)
        {
            MyConveyorConnectedGroup group = null;
            VRage.Game.Entity.MyEntity thrustEntity = conveyorEndpointBlock as VRage.Game.Entity.MyEntity;
            group = groupOverride ?? this.FindEntityGroup(thrustEntity);
            if (group == null)
            {
                return null;
            }
            if ((conveyorEndpointBlock != null) && ReferenceEquals(conveyorEndpointBlock.ConveyorEndpoint, group.FirstEndpoint))
            {
                if (group.ThrustCount == 1)
                {
                    return group;
                }
                using (List<FuelTypeData>.Enumerator enumerator = group.DataByFuelType.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        bool flag = false;
                        using (Dictionary<Vector3I, HashSet<VRage.Game.Entity.MyEntity>>.ValueCollection.Enumerator enumerator2 = enumerator.Current.ThrustsByDirection.Values.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                foreach (VRage.Game.Entity.MyEntity entity2 in enumerator2.Current)
                                {
                                    if (!ReferenceEquals(entity2, thrustEntity))
                                    {
                                        group.FirstEndpoint = (entity2 as IMyConveyorEndpointBlock).ConveyorEndpoint;
                                        thrustEntity.Components.Remove<MyResourceSinkComponent>();
                                        entity2.Components.Add<MyResourceSinkComponent>(group.ResourceSink);
                                        AddSinkToSystems(group.ResourceSink, this.Entity as MyCubeGrid);
                                        flag = true;
                                        break;
                                    }
                                }
                                if (flag)
                                {
                                    break;
                                }
                            }
                        }
                        if (flag)
                        {
                            break;
                        }
                    }
                }
            }
            Vector3I[] intDirections = Base6Directions.IntDirections;
            int index = 0;
            while (index < intDirections.Length)
            {
                Vector3I vectori = intDirections[index];
                int num2 = 0;
                while (true)
                {
                    if (num2 >= group.FuelTypes.Count)
                    {
                        index++;
                        break;
                    }
                    FuelTypeData data = group.DataByFuelType[num2];
                    foreach (VRage.Game.Entity.MyEntity entity3 in data.ThrustsByDirection[vectori])
                    {
                        IMyConveyorEndpoint conveyorEndpoint = (entity3 as IMyConveyorEndpointBlock).ConveyorEndpoint;
                        if (!ReferenceEquals(thrustEntity, entity3) && !MyGridConveyorSystem.Reachable(conveyorEndpoint, group.FirstEndpoint))
                        {
                            MyDefinitionId resourceTypeId = this.FuelType(entity3);
                            group.ResourceSink.SetMaxRequiredInputByType(resourceTypeId, group.ResourceSink.MaxRequiredInputByType(resourceTypeId) - this.PowerAmountToFuel(ref resourceTypeId, this.MaxPowerConsumption(entity3), group));
                            data.ThrustCount--;
                            group.ThrustCount--;
                            m_tmpEntitiesWithDirections.Add(new MyTuple<VRage.Game.Entity.MyEntity, Vector3I>(entity3, vectori));
                        }
                    }
                    foreach (MyTuple<VRage.Game.Entity.MyEntity, Vector3I> tuple in m_tmpEntitiesWithDirections)
                    {
                        data.ThrustsByDirection[tuple.Item2].Remove(tuple.Item1);
                        this.RemoveFromGroup(tuple.Item1, group);
                    }
                    num2++;
                }
            }
            foreach (MyTuple<VRage.Game.Entity.MyEntity, Vector3I> local2 in m_tmpEntitiesWithDirections)
            {
                VRage.Game.Entity.MyEntity entity4 = local2.Item1;
                Vector3I vectori2 = local2.Item2;
                MyDefinitionId fuelId = this.FuelType(entity4);
                bool flag2 = false;
                foreach (MyConveyorConnectedGroup group3 in m_tmpGroups)
                {
                    if (MyGridConveyorSystem.Reachable((entity4 as IMyConveyorEndpointBlock).ConveyorEndpoint, group3.FirstEndpoint))
                    {
                        int num5;
                        if (!group3.TryGetTypeIndex(ref fuelId, out num5))
                        {
                            num5 = this.InitializeType(fuelId, group3.DataByFuelType, group3.FuelTypes, group3.FuelTypeToIndex, group3.ResourceSink);
                        }
                        FuelTypeData local3 = group3.DataByFuelType[num5];
                        local3.ThrustsByDirection[vectori2].Add(entity4);
                        this.AddToGroup(entity4, group3);
                        local3.ThrustCount++;
                        group3.ThrustCount++;
                        group3.ResourceSink.SetMaxRequiredInputByType(fuelId, group3.ResourceSink.MaxRequiredInputByType(fuelId) + this.PowerAmountToFuel(ref fuelId, this.MaxPowerConsumption(entity4), group3));
                        flag2 = true;
                        break;
                    }
                }
                if (!flag2)
                {
                    MyConveyorConnectedGroup item = new MyConveyorConnectedGroup(entity4 as IMyConveyorEndpointBlock);
                    m_tmpGroups.Add(item);
                    this.m_connectedGroups.Add(item);
                    int num3 = this.InitializeType(fuelId, item.DataByFuelType, item.FuelTypes, item.FuelTypeToIndex, item.ResourceSink);
                    entity4.Components.Add<MyResourceSinkComponent>(item.ResourceSink);
                    int typeIndex = group.GetTypeIndex(ref fuelId);
                    FuelTypeData local4 = item.DataByFuelType[num3];
                    local4.Efficiency = group.DataByFuelType[typeIndex].Efficiency;
                    local4.EnergyDensity = group.DataByFuelType[typeIndex].EnergyDensity;
                    local4.ThrustsByDirection[vectori2].Add(entity4);
                    this.AddToGroup(entity4, item);
                    local4.ThrustCount++;
                    item.ThrustCount++;
                    item.ResourceSink.SetMaxRequiredInputByType(fuelId, item.ResourceSink.MaxRequiredInputByType(fuelId) + this.PowerAmountToFuel(ref fuelId, this.MaxPowerConsumption(entity4), item));
                }
            }
            m_tmpGroups.Clear();
            m_tmpEntitiesWithDirections.Clear();
            return group;
        }

        private void TurnOffThrusterFlame(List<FuelTypeData> dataByFuelType)
        {
            using (List<FuelTypeData>.Enumerator enumerator = dataByFuelType.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    foreach (KeyValuePair<Vector3I, HashSet<VRage.Game.Entity.MyEntity>> pair in enumerator.Current.ThrustsByDirection)
                    {
                        foreach (MyThrust thrust in pair.Value)
                        {
                            if (thrust == null)
                            {
                                continue;
                            }
                            if (thrust.ThrustOverride <= 0f)
                            {
                                thrust.CurrentStrength = 0f;
                            }
                        }
                    }
                }
            }
        }

        public virtual void Unregister(VRage.Game.Entity.MyEntity entity, Vector3I forwardVector)
        {
            if (((entity != null) && (this.Entity != null)) && !this.Entity.MarkedForClose)
            {
                if (!this.IsRegistered(entity, forwardVector))
                {
                    this.m_thrustEntitiesRemovedBeforeRegister.Add(entity);
                }
                else
                {
                    int num4;
                    Dictionary<Vector3I, HashSet<VRage.Game.Entity.MyEntity>> thrustsByDirection = null;
                    int thrustsLeftInGroup = 0;
                    MyResourceSinkComponentBase resourceSink = null;
                    MyDefinitionId typeId = this.FuelType(entity);
                    List<FuelTypeData> fuelData = null;
                    int index = -1;
                    int typeIndex = -1;
                    IMyConveyorEndpointBlock conveyorEndpointBlock = entity as IMyConveyorEndpointBlock;
                    if (!MyResourceDistributorComponent.IsConveyorConnectionRequiredTotal(ref typeId) || (conveyorEndpointBlock == null))
                    {
                        if (!this.TryGetTypeIndex(ref typeId, out typeIndex))
                        {
                            return;
                        }
                        resourceSink = this.m_resourceSink;
                        thrustsByDirection = this.m_dataByFuelType[typeIndex].ThrustsByDirection;
                        fuelData = this.m_dataByFuelType;
                        thrustsLeftInGroup = 0;
                        foreach (FuelTypeData data in this.m_dataByFuelType)
                        {
                            thrustsLeftInGroup += data.ThrustCount;
                        }
                        thrustsLeftInGroup = Math.Max(thrustsLeftInGroup - 1, 0);
                    }
                    else
                    {
                        MyConveyorConnectedGroup group2 = this.TrySplitGroup(conveyorEndpointBlock, null);
                        if (!group2.TryGetTypeIndex(ref typeId, out typeIndex))
                        {
                            return;
                        }
                        if (group2.DataByFuelType[typeIndex].ThrustsByDirection[forwardVector].Contains(entity))
                        {
                            num4 = group2.ThrustCount - 1;
                            group2.ThrustCount = num4;
                            thrustsLeftInGroup = num4;
                            resourceSink = group2.ResourceSink;
                            thrustsByDirection = group2.DataByFuelType[typeIndex].ThrustsByDirection;
                            fuelData = group2.DataByFuelType;
                            for (int i = 0; i < this.m_connectedGroups.Count; i++)
                            {
                                if (this.m_connectedGroups[i] == group2)
                                {
                                    index = i;
                                    break;
                                }
                            }
                        }
                    }
                    if (thrustsByDirection != null)
                    {
                        MyConveyorConnectedGroup containingGroup = (index != -1) ? this.m_connectedGroups[index] : null;
                        this.MoveSinkToNewEntity(entity, fuelData, typeIndex, thrustsLeftInGroup, resourceSink, containingGroup);
                        thrustsByDirection[forwardVector].Remove(entity);
                        resourceSink.SetMaxRequiredInputByType(typeId, resourceSink.MaxRequiredInputByType(typeId) - this.PowerAmountToFuel(ref typeId, this.MaxPowerConsumption(entity), containingGroup));
                        FuelTypeData local1 = fuelData[typeIndex];
                        num4 = local1.ThrustCount - 1;
                        local1.ThrustCount = num4;
                        if (num4 == 0)
                        {
                            fuelData.RemoveAtFast<FuelTypeData>(typeIndex);
                            if (containingGroup != null)
                            {
                                containingGroup.FuelTypes.RemoveAtFast<MyDefinitionId>(typeIndex);
                                containingGroup.FuelTypeToIndex.Remove(typeId);
                            }
                            else
                            {
                                this.m_fuelTypes.RemoveAtFast<MyDefinitionId>(typeIndex);
                                this.m_fuelTypeToIndex.Remove(typeId);
                            }
                        }
                        if (thrustsLeftInGroup == 0)
                        {
                            RemoveSinkFromSystems(resourceSink, base.Container.Entity as MyCubeGrid);
                            if (index != -1)
                            {
                                this.m_connectedGroups.RemoveAt(index);
                            }
                        }
                        num4 = this.ThrustCount - 1;
                        this.ThrustCount = num4;
                        this.MarkDirty(false);
                    }
                }
            }
        }

        public virtual void UpdateBeforeSimulation(bool updateDampeners, VRage.Game.Entity.MyEntity relativeDampeningEntity)
        {
            if (this.Entity != null)
            {
                if (this.Entity.InScene)
                {
                    this.UpdateConveyorSystemChanges();
                }
                if (this.ThrustCount == 0)
                {
                    this.Entity.Components.Remove<MyEntityThrustComponent>();
                }
                else
                {
                    if (MySession.Static.GameplayFrameCounter >= this.m_nextPlanetaryInfluenceRecalculation)
                    {
                        this.RecalculatePlanetaryInfluence();
                    }
                    if (this.m_thrustsChanged)
                    {
                        this.RecomputeThrustParameters();
                        if (((this.Entity is MyCubeGrid) && (this.Entity.Physics != null)) && !this.Entity.Physics.RigidBody.IsActive)
                        {
                            (this.Entity as MyCubeGrid).ActivatePhysics();
                        }
                    }
                    if (this.Enabled && (this.Entity.Physics != null))
                    {
                        Vector3 zero;
                        MatrixD worldMatrixNormalizedInv = this.Entity.PositionComp.WorldMatrixNormalizedInv;
                        if ((relativeDampeningEntity == null) || (relativeDampeningEntity.Physics == null))
                        {
                            zero = Vector3.Zero;
                        }
                        else
                        {
                            zero = relativeDampeningEntity.Physics.LinearVelocity + ((30f * relativeDampeningEntity.Physics.LinearAcceleration) * 0.01666667f);
                        }
                        Vector3 dampeningVelocity = Vector3.TransformNormal(zero, worldMatrixNormalizedInv);
                        if (((dampeningVelocity.LengthSquared() > 0f) && ((this.Entity is MyCubeGrid) && (this.Entity.Physics != null))) && !this.Entity.Physics.RigidBody.IsActive)
                        {
                            (this.Entity as MyCubeGrid).ActivatePhysics();
                        }
                        this.UpdateThrusts(updateDampeners, dampeningVelocity);
                        if (this.m_thrustsChanged)
                        {
                            this.RecomputeThrustParameters();
                        }
                    }
                    if (!this.DampenersEnabled && this.m_dampenersEnabledLastFrame)
                    {
                        foreach (MyConveyorConnectedGroup group in this.m_connectedGroups)
                        {
                            if (group.DataByFuelType.Count > 0)
                            {
                                this.TurnOffThrusterFlame(group.DataByFuelType);
                            }
                        }
                        if (this.m_dataByFuelType.Count > 0)
                        {
                            this.TurnOffThrusterFlame(this.m_dataByFuelType);
                        }
                    }
                    this.m_dampenersEnabledLastFrame = this.DampenersEnabled;
                    this.m_thrustsChanged = false;
                }
            }
        }

        private void UpdateConveyorSystemChanges()
        {
            while (this.m_thrustEntitiesPending.Count > 0)
            {
                MyTuple<VRage.Game.Entity.MyEntity, Vector3I, Func<bool>> tuple = this.m_thrustEntitiesPending.Dequeue();
                if (this.IsThrustEntityType(tuple.Item1))
                {
                    if (this.m_thrustEntitiesRemovedBeforeRegister.Contains(tuple.Item1))
                    {
                        this.m_thrustEntitiesRemovedBeforeRegister.Remove(tuple.Item1);
                        continue;
                    }
                    this.RegisterLazy(tuple.Item1, tuple.Item2, tuple.Item3);
                }
            }
            while (this.m_conveyorSegmentsPending.Count > 0)
            {
                FindConnectedGroups(this.m_conveyorSegmentsPending.Dequeue(), this.m_connectedGroups, m_tmpGroupIndices);
                if (m_tmpGroupIndices.Count > 1)
                {
                    this.MergeGroups(this.m_connectedGroups, m_tmpGroupIndices);
                }
                m_tmpGroupIndices.Clear();
            }
            while (this.m_conveyorEndpointsPending.Count > 0)
            {
                FindConnectedGroups(this.m_conveyorEndpointsPending.Dequeue(), this.m_connectedGroups, m_tmpGroupIndices);
                if (m_tmpGroupIndices.Count > 1)
                {
                    this.MergeGroups(this.m_connectedGroups, m_tmpGroupIndices);
                }
                m_tmpGroupIndices.Clear();
            }
            foreach (MyConveyorConnectedGroup group in this.m_groupsToTrySplit)
            {
                this.TrySplitGroup(null, group);
            }
            this.m_groupsToTrySplit.Clear();
            if (this.m_mergeAllGroupsDirty)
            {
                this.TryMergeAllGroups();
                this.m_mergeAllGroupsDirty = false;
            }
        }

        private void UpdatePowerAndThrustStrength(Vector3 thrust, MyDefinitionId fuelType, MyConveyorConnectedGroup group, bool updateThrust)
        {
            if ((this.m_controlThrustChanged || this.m_lastControlThrustChanged) || !this.DampenersEnabled)
            {
                int typeIndex;
                MyResourceSinkComponent resourceSink;
                FuelTypeData data;
                float totalThrustOverridePower;
                if (group == null)
                {
                    typeIndex = this.GetTypeIndex(ref fuelType);
                    resourceSink = this.m_resourceSink;
                    data = this.m_dataByFuelType[typeIndex];
                    totalThrustOverridePower = this.m_totalThrustOverridePower;
                    this.m_lastPowerUpdate = MySession.Static.GameplayFrameCounter;
                }
                else
                {
                    typeIndex = group.GetTypeIndex(ref fuelType);
                    resourceSink = group.ResourceSink;
                    data = group.DataByFuelType[typeIndex];
                    totalThrustOverridePower = group.ThrustOverridePower;
                    group.LastPowerUpdate = MySession.Static.GameplayFrameCounter;
                }
                Vector3 vector = Vector3.Clamp(thrust / (data.MaxPositiveThrust + 1E-07f), Vector3.Zero, Vector3.One);
                Vector3 vector2 = Vector3.Clamp(-thrust / (data.MaxNegativeThrust + 1E-07f), Vector3.Zero, Vector3.One);
                float powerAmount = 0f;
                if (this.Enabled)
                {
                    powerAmount = Math.Max(((((((powerAmount + ((vector.X > 0f) ? (vector.X * this.GetMaxPowerRequirement(data, ref Vector3I.Left)) : 0f)) + ((vector.Y > 0f) ? (vector.Y * this.GetMaxPowerRequirement(data, ref Vector3I.Down)) : 0f)) + ((vector.Z > 0f) ? (vector.Z * this.GetMaxPowerRequirement(data, ref Vector3I.Forward)) : 0f)) + ((vector2.X > 0f) ? (vector2.X * this.GetMaxPowerRequirement(data, ref Vector3I.Right)) : 0f)) + ((vector2.Y > 0f) ? (vector2.Y * this.GetMaxPowerRequirement(data, ref Vector3I.Up)) : 0f)) + ((vector2.Z > 0f) ? (vector2.Z * this.GetMaxPowerRequirement(data, ref Vector3I.Backward)) : 0f)) + totalThrustOverridePower, data.MinRequiredPowerInput);
                }
                this.SetRequiredFuelInput(ref fuelType, this.PowerAmountToFuel(ref fuelType, powerAmount, group), group);
                resourceSink.Update();
                if (updateThrust)
                {
                    this.UpdateThrustStrength(data.ThrustsByDirection[Vector3I.Left], vector.X);
                    this.UpdateThrustStrength(data.ThrustsByDirection[Vector3I.Down], vector.Y);
                    this.UpdateThrustStrength(data.ThrustsByDirection[Vector3I.Forward], vector.Z);
                    this.UpdateThrustStrength(data.ThrustsByDirection[Vector3I.Right], vector2.X);
                    this.UpdateThrustStrength(data.ThrustsByDirection[Vector3I.Up], vector2.Y);
                    this.UpdateThrustStrength(data.ThrustsByDirection[Vector3I.Backward], vector2.Z);
                }
            }
        }

        public static void UpdateRelativeDampeningEntity(IMyControllableEntity controlledEntity, VRage.Game.Entity.MyEntity dampeningEntity)
        {
            if ((Sync.IsServer && (dampeningEntity != null)) && (dampeningEntity.PositionComp.WorldAABB.DistanceSquared(controlledEntity.Entity.PositionComp.GetPosition()) > MAX_DISTANCE_RELATIVE_DAMPENING_SQ))
            {
                controlledEntity.RelativeDampeningEntity = null;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MyPlayerCollection.ClearDampeningEntity), controlledEntity.Entity.EntityId, targetEndpoint, position);
            }
        }

        protected virtual unsafe void UpdateThrusts(bool applyDampeners, Vector3 dampeningVelocity)
        {
            for (int i = 0; i < this.m_dataByFuelType.Count; i++)
            {
                FuelTypeData fuelData = this.m_dataByFuelType[i];
                if (this.AutopilotEnabled)
                {
                    this.ComputeAiThrust(this.AutoPilotControlThrust, fuelData);
                }
                else
                {
                    this.ComputeBaseThrust(ref this.m_controlThrust, fuelData, applyDampeners, dampeningVelocity);
                }
            }
            int num2 = 0;
            while (num2 < this.m_connectedGroups.Count)
            {
                MyConveyorConnectedGroup group = this.m_connectedGroups[num2];
                int num3 = 0;
                while (true)
                {
                    if (num3 >= group.DataByFuelType.Count)
                    {
                        num2++;
                        break;
                    }
                    FuelTypeData fuelData = group.DataByFuelType[num3];
                    if (this.AutopilotEnabled)
                    {
                        this.ComputeAiThrust(this.AutoPilotControlThrust, fuelData);
                    }
                    else
                    {
                        this.ComputeBaseThrust(ref this.m_controlThrust, fuelData, applyDampeners, dampeningVelocity);
                    }
                    num3++;
                }
            }
            Vector3 vector = new Vector3();
            this.FinalThrust = vector;
            for (int j = 0; j < this.m_dataByFuelType.Count; j++)
            {
                Vector3 vector2;
                Vector3* vectorPtr1;
                Vector3* vectorPtr2;
                Vector3* vectorPtr3;
                MyDefinitionId fuelType = this.m_fuelTypes[j];
                FuelTypeData data3 = this.m_dataByFuelType[j];
                this.UpdatePowerAndThrustStrength(data3.CurrentThrust, fuelType, null, true);
                Vector3 vector3 = this.m_maxPositiveThrust + this.m_maxNegativeThrust;
                vectorPtr1->X = (vector3.X != 0f) ? ((data3.CurrentThrust.X * (data3.MaxPositiveThrust.X + data3.MaxNegativeThrust.X)) / vector3.X) : 0f;
                vectorPtr1 = (Vector3*) ref vector2;
                vectorPtr2->Y = (vector3.Y != 0f) ? ((data3.CurrentThrust.Y * (data3.MaxPositiveThrust.Y + data3.MaxNegativeThrust.Y)) / vector3.Y) : 0f;
                vectorPtr2 = (Vector3*) ref vector2;
                vectorPtr3->Z = (vector3.Z != 0f) ? ((data3.CurrentThrust.Z * (data3.MaxPositiveThrust.Z + data3.MaxNegativeThrust.Z)) / vector3.Z) : 0f;
                vectorPtr3 = (Vector3*) ref vector2;
                Vector3 vector4 = this.ApplyThrustModifiers(ref fuelType, ref vector2, ref this.m_totalThrustOverride, this.m_resourceSink);
                this.FinalThrust += vector4;
            }
            int num5 = 0;
            while (num5 < this.m_connectedGroups.Count)
            {
                MyConveyorConnectedGroup group = this.m_connectedGroups[num5];
                int num6 = 0;
                while (true)
                {
                    Vector3 vector5;
                    Vector3* vectorPtr4;
                    Vector3* vectorPtr5;
                    Vector3* vectorPtr6;
                    if (num6 >= group.DataByFuelType.Count)
                    {
                        num5++;
                        break;
                    }
                    MyDefinitionId fuelType = group.FuelTypes[num6];
                    FuelTypeData data4 = group.DataByFuelType[num6];
                    if (((this.Entity.Physics.RigidBody == null) || (this.Entity.Physics.RigidBody.IsActive || this.m_thrustsChanged)) || this.m_lastControlThrustChanged)
                    {
                        this.UpdatePowerAndThrustStrength(data4.CurrentThrust, fuelType, group, true);
                    }
                    Vector3 vector6 = group.MaxPositiveThrust + group.MaxNegativeThrust;
                    vectorPtr4->X = (vector6.X != 0f) ? ((data4.CurrentThrust.X * (data4.MaxPositiveThrust.X + data4.MaxNegativeThrust.X)) / vector6.X) : 0f;
                    vectorPtr4 = (Vector3*) ref vector5;
                    vectorPtr5->Y = (vector6.Y != 0f) ? ((data4.CurrentThrust.Y * (data4.MaxPositiveThrust.Y + data4.MaxNegativeThrust.Y)) / vector6.Y) : 0f;
                    vectorPtr5 = (Vector3*) ref vector5;
                    vectorPtr6->Z = (vector6.Z != 0f) ? ((data4.CurrentThrust.Z * (data4.MaxPositiveThrust.Z + data4.MaxNegativeThrust.Z)) / vector6.Z) : 0f;
                    vectorPtr6 = (Vector3*) ref vector5;
                    Vector3 vector7 = this.ApplyThrustModifiers(ref fuelType, ref vector5, ref group.ThrustOverride, group.ResourceSink);
                    this.FinalThrust += vector7;
                    num6++;
                }
            }
            this.m_lastControlThrustChanged = this.m_controlThrustChanged;
            this.m_controlThrustChanged = false;
        }

        protected abstract void UpdateThrustStrength(HashSet<VRage.Game.Entity.MyEntity> entities, float thrustForce);

        protected ListReader<MyConveyorConnectedGroup> ConnectedGroups =>
            new ListReader<MyConveyorConnectedGroup>(this.m_connectedGroups);

        private static List<int> m_tmpGroupIndices =>
            MyUtils.Init<List<int>>(ref m_tmpGroupIndicesPerThread);

        private static List<MyTuple<VRage.Game.Entity.MyEntity, Vector3I>> m_tmpEntitiesWithDirections =>
            MyUtils.Init<List<MyTuple<VRage.Game.Entity.MyEntity, Vector3I>>>(ref m_tmpEntitiesWithDirectionsPerThread);

        private static List<MyConveyorConnectedGroup> m_tmpGroups =>
            MyUtils.Init<List<MyConveyorConnectedGroup>>(ref m_tmpGroupsPerThread);

        protected bool ControlThrustChanged
        {
            get => 
                this.m_controlThrustChanged;
            set => 
                (this.m_controlThrustChanged = value);
        }

        public Vector3? MaxThrustOverride
        {
            get
            {
                if (MyFakes.ENABLE_VR_REMOTE_CONTROL_WAYPOINTS_FAST_MOVEMENT)
                {
                    return this.m_maxThrustOverride;
                }
                return null;
            }
            set => 
                (this.m_maxThrustOverride = value);
        }

        public VRage.Game.Entity.MyEntity Entity =>
            (base.Entity as VRage.Game.Entity.MyEntity);

        public float MaxRequiredPowerInput { get; private set; }

        public float MinRequiredPowerInput { get; private set; }

        public float SlowdownFactor { get; set; }

        public int ThrustCount { get; private set; }

        public bool DampenersEnabled { get; set; }

        public Vector3 ControlThrust
        {
            get => 
                this.m_controlThrust;
            set
            {
                this.m_controlThrustChanged |= this.m_controlThrust != value;
                this.m_controlThrust = value;
            }
        }

        public Vector3 FinalThrust { get; private set; }

        public Vector3 AutoPilotControlThrust { get; set; }

        public bool AutopilotEnabled { get; set; }

        public bool Enabled
        {
            get => 
                this.m_enabled;
            set => 
                (this.m_enabled = value);
        }

        public override string ComponentTypeDebugString =>
            "Thrust Component";

        public bool HasPower =>
            this.m_resourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyEntityThrustComponent.<>c <>9 = new MyEntityThrustComponent.<>c();
            public static Func<IMyEventOwner, Action<long>> <>9__168_0;

            internal Action<long> <UpdateRelativeDampeningEntity>b__168_0(IMyEventOwner s) => 
                new Action<long>(MyPlayerCollection.ClearDampeningEntity);
        }

        private class DirectionComparer : IEqualityComparer<Vector3I>
        {
            public bool Equals(Vector3I x, Vector3I y) => 
                (x == y);

            public int GetHashCode(Vector3I obj) => 
                ((obj.X + (8 * obj.Y)) + (0x40 * obj.Z));
        }

        public class FuelTypeData
        {
            public Dictionary<Vector3I, HashSet<VRage.Game.Entity.MyEntity>> ThrustsByDirection;
            public Dictionary<Vector3I, float> MaxRequirementsByDirection;
            public float CurrentRequiredFuelInput;
            public Vector3 MaxNegativeThrust;
            public Vector3 MaxPositiveThrust;
            public float MinRequiredPowerInput;
            public float MaxRequiredPowerInput;
            public int ThrustCount;
            public float Efficiency;
            public float EnergyDensity;
            public Vector3 CurrentThrust;
            public Vector3 ThrustOverride;
            public float ThrustOverridePower;
        }

        public class MyConveyorConnectedGroup
        {
            public readonly List<MyEntityThrustComponent.FuelTypeData> DataByFuelType;
            public readonly MyResourceSinkComponent ResourceSink;
            public int ThrustCount;
            public Vector3 MaxNegativeThrust;
            public Vector3 MaxPositiveThrust;
            public Vector3 ThrustOverride;
            public float ThrustOverridePower;
            public readonly List<MyDefinitionId> FuelTypes;
            public readonly Dictionary<MyDefinitionId, int> FuelTypeToIndex;
            public long LastPowerUpdate;
            public IMyConveyorEndpoint FirstEndpoint;

            public MyConveyorConnectedGroup(IMyConveyorEndpointBlock endpointBlock)
            {
                this.FirstEndpoint = endpointBlock.ConveyorEndpoint;
                this.DataByFuelType = new List<MyEntityThrustComponent.FuelTypeData>();
                this.ResourceSink = new MyResourceSinkComponent(1);
                this.LastPowerUpdate = MySession.Static.GameplayFrameCounter;
                this.FuelTypes = new List<MyDefinitionId>();
                this.FuelTypeToIndex = new Dictionary<MyDefinitionId, int>(MyDefinitionId.Comparer);
            }

            public int GetTypeIndex(ref MyDefinitionId fuelId)
            {
                int num2;
                int num = 0;
                if ((this.FuelTypeToIndex.Count > 1) && this.FuelTypeToIndex.TryGetValue(fuelId, out num2))
                {
                    num = num2;
                }
                return num;
            }

            public bool TryGetTypeIndex(ref MyDefinitionId fuelId, out int typeIndex)
            {
                typeIndex = 0;
                return (((this.FuelTypeToIndex.Count <= 1) || this.FuelTypeToIndex.TryGetValue(fuelId, out typeIndex)) && (this.FuelTypeToIndex.Count > 0));
            }
        }
    }
}

