namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Groups;
    using VRageMath;

    internal class MyThrusterBlockThrustComponent : MyEntityThrustComponent
    {
        private float m_levitationPeriodLength = 1.3f;
        private float m_levitationTorqueCoeficient = 0.25f;

        protected override void AddToGroup(VRage.Game.Entity.MyEntity thrustEntity, MyEntityThrustComponent.MyConveyorConnectedGroup group)
        {
            MyThrust thrust = thrustEntity as MyThrust;
            if (thrust != null)
            {
                group.ResourceSink.IsPoweredChanged += new Action(thrust.Sink_IsPoweredChanged);
            }
        }

        protected override float CalculateConsumptionMultiplier(VRage.Game.Entity.MyEntity thrustEntity, float naturalGravityStrength)
        {
            MyThrust thrust = thrustEntity as MyThrust;
            return ((thrust != null) ? (1f + (thrust.BlockDefinition.ConsumptionFactorPerG * (naturalGravityStrength / 9.81f))) : 1f);
        }

        protected override float CalculateForceMultiplier(VRage.Game.Entity.MyEntity thrustEntity, float planetaryInfluence, bool inAtmosphere)
        {
            float effectivenessAtMinInfluence = 1f;
            MyThrustDefinition blockDefinition = (thrustEntity as MyThrust).BlockDefinition;
            if (blockDefinition.NeedsAtmosphereForInfluence && !inAtmosphere)
            {
                effectivenessAtMinInfluence = blockDefinition.EffectivenessAtMinInfluence;
            }
            else if (blockDefinition.MaxPlanetaryInfluence != blockDefinition.MinPlanetaryInfluence)
            {
                float num2 = (planetaryInfluence - blockDefinition.MinPlanetaryInfluence) * blockDefinition.InvDiffMinMaxPlanetaryInfluence;
                effectivenessAtMinInfluence = MathHelper.Lerp(blockDefinition.EffectivenessAtMinInfluence, blockDefinition.EffectivenessAtMaxInfluence, MathHelper.Clamp(num2, 0f, 1f));
            }
            return effectivenessAtMinInfluence;
        }

        protected override float CalculateMass()
        {
            MyGroups<MyCubeGrid, MyGridPhysicalDynamicGroupData>.Group group = MyCubeGridGroups.Static.PhysicalDynamic.GetGroup(this.Entity);
            MyGridPhysics physics = this.Entity.Physics;
            float num = (physics.WeldedRigidBody != null) ? physics.WeldedRigidBody.Mass : this.GetGridMass(this.CubeGrid);
            MyCubeGrid objA = null;
            float num2 = 0f;
            if (group != null)
            {
                float num3 = 0f;
                using (HashSet<MyGroups<MyCubeGrid, MyGridPhysicalDynamicGroupData>.Node>.Enumerator enumerator = group.Nodes.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyCubeGrid nodeData = enumerator.Current.NodeData;
                        if (!nodeData.IsStatic && ((nodeData.Physics != null) && !MyFixedGrids.IsRooted(nodeData)))
                        {
                            int hasPower;
                            MyEntityThrustComponent component = nodeData.Components.Get<MyEntityThrustComponent>();
                            if ((component == null) || !component.Enabled)
                            {
                                hasPower = 0;
                            }
                            else
                            {
                                hasPower = (int) component.HasPower;
                            }
                            if (hasPower == 0)
                            {
                                num2 += (nodeData.Physics.WeldedRigidBody != null) ? nodeData.Physics.WeldedRigidBody.Mass : this.GetGridMass(nodeData);
                            }
                            else
                            {
                                float radius = nodeData.PositionComp.LocalVolume.Radius;
                                if ((radius > num3) || ((radius == num3) && ((objA == null) || (nodeData.EntityId > objA.EntityId))))
                                {
                                    num3 = radius;
                                    objA = nodeData;
                                }
                            }
                        }
                    }
                }
            }
            if (ReferenceEquals(objA, this.CubeGrid))
            {
                num += num2;
            }
            return num;
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.MarkDirty(false);
            if (((this.CubeGrid != null) && (this.CubeGrid.Physics != null)) && !this.CubeGrid.Physics.RigidBody.IsActive)
            {
                this.CubeGrid.ActivatePhysics();
            }
        }

        protected override float ForceMagnitude(VRage.Game.Entity.MyEntity thrustEntity, float planetaryInfluence, bool inAtmosphere)
        {
            MyThrust thrust = thrustEntity as MyThrust;
            if (thrust == null)
            {
                return 0f;
            }
            float num = (thrustEntity is IMyThrust) ? (thrustEntity as IMyThrust).ThrustMultiplier : 1f;
            return ((thrust.BlockDefinition.ForceMagnitude * num) * this.CalculateForceMultiplier(thrust, planetaryInfluence, inAtmosphere));
        }

        protected override MyDefinitionId FuelType(VRage.Game.Entity.MyEntity thrustEntity)
        {
            MyThrust thrust = thrustEntity as MyThrust;
            return ((thrust.FuelDefinition != null) ? thrust.FuelDefinition.Id : MyResourceDistributorComponent.ElectricityId);
        }

        private float GetGridMass(MyCubeGrid grid)
        {
            if (!Sync.IsServer)
            {
                if (MyFixedGrids.IsRooted(grid))
                {
                    return 0f;
                }
                HkMassProperties? massProperties = grid.Physics.Shape.MassProperties;
                if (massProperties != null)
                {
                    return massProperties.Value.Mass;
                }
            }
            return grid.Physics.Mass;
        }

        private static bool IsOverridden(MyThrust thrust)
        {
            MyEntityThrustComponent component;
            return ((thrust != null) && ((thrust.ThrustOverride > 0f) && (thrust.Enabled && (thrust.IsFunctional && (!thrust.CubeGrid.Components.TryGet<MyEntityThrustComponent>(out component) || !component.AutopilotEnabled)))));
        }

        protected override bool IsThrustEntityType(VRage.Game.Entity.MyEntity thrustEntity) => 
            (thrustEntity is MyThrust);

        protected override bool IsUsed(VRage.Game.Entity.MyEntity thrustEntity)
        {
            MyThrust thrust = thrustEntity as MyThrust;
            return ((thrust != null) ? (thrust.Enabled && (thrust.IsFunctional && (thrust.ThrustOverride == 0f))) : false);
        }

        protected override float MaxPowerConsumption(VRage.Game.Entity.MyEntity thrustEntity) => 
            (thrustEntity as MyThrust).MaxPowerConsumption;

        protected override float MinPowerConsumption(VRage.Game.Entity.MyEntity thrustEntity) => 
            (thrustEntity as MyThrust).MinPowerConsumption;

        private void MyThrust_ThrustOverrideChanged(MyThrust block, float newValue)
        {
            base.MarkDirty(false);
        }

        protected override bool RecomputeOverriddenParameters(VRage.Game.Entity.MyEntity thrustEntity, MyEntityThrustComponent.FuelTypeData fuelData)
        {
            MyThrust thrust = thrustEntity as MyThrust;
            if (thrust == null)
            {
                return false;
            }
            if (!IsOverridden(thrust))
            {
                return false;
            }
            Vector3 vector = (Vector3) ((thrust.ThrustOverride * -thrust.ThrustForwardVector) * this.CalculateForceMultiplier(thrustEntity, base.m_lastPlanetaryInfluence, base.m_lastPlanetaryInfluenceHasAtmosphere));
            float num = (thrust.ThrustOverride / thrust.ThrustForce.Length()) * thrust.MaxPowerConsumption;
            if (fuelData.ThrustsByDirection[thrust.ThrustForwardVector].Contains(thrustEntity))
            {
                fuelData.ThrustOverride += vector;
                fuelData.ThrustOverridePower += num;
            }
            return true;
        }

        public override void Register(VRage.Game.Entity.MyEntity entity, Vector3I forwardVector, Func<bool> onRegisteredCallback)
        {
            if (entity is MyThrust)
            {
                base.m_thrustEntitiesPending.Enqueue(new MyTuple<VRage.Game.Entity.MyEntity, Vector3I, Func<bool>>(entity, forwardVector, onRegisteredCallback));
            }
        }

        protected override bool RegisterLazy(VRage.Game.Entity.MyEntity entity, Vector3I forwardVector, Func<bool> onRegisteredCallback)
        {
            base.RegisterLazy(entity, forwardVector, onRegisteredCallback);
            base.Register(entity, forwardVector, onRegisteredCallback);
            MyThrust thrust = entity as MyThrust;
            MyDefinitionId resourceTypeId = this.FuelType(entity);
            base.m_lastFuelTypeData.EnergyDensity = thrust.FuelDefinition.EnergyDensity;
            base.m_lastFuelTypeData.Efficiency = thrust.BlockDefinition.FuelConverter.Efficiency;
            base.m_lastSink.SetMaxRequiredInputByType(resourceTypeId, base.m_lastSink.MaxRequiredInputByType(resourceTypeId) + base.PowerAmountToFuel(ref resourceTypeId, thrust.MaxPowerConsumption, base.m_lastGroup));
            thrust.EnabledChanged += new Action<MyTerminalBlock>(this.thrust_EnabledChanged);
            thrust.ThrustOverrideChanged += new Action<MyThrust, float>(this.MyThrust_ThrustOverrideChanged);
            thrust.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.SlowdownFactor = Math.Max(thrust.BlockDefinition.SlowdownFactor, base.SlowdownFactor);
            if (onRegisteredCallback != null)
            {
                onRegisteredCallback();
            }
            return true;
        }

        protected override void RemoveFromGroup(VRage.Game.Entity.MyEntity thrustEntity, MyEntityThrustComponent.MyConveyorConnectedGroup group)
        {
            MyThrust thrust = thrustEntity as MyThrust;
            if (thrust != null)
            {
                group.ResourceSink.IsPoweredChanged -= new Action(thrust.Sink_IsPoweredChanged);
            }
        }

        private void thrust_EnabledChanged(MyTerminalBlock obj)
        {
            base.MarkDirty(false);
            if (((this.CubeGrid != null) && (this.CubeGrid.Physics != null)) && !this.CubeGrid.Physics.RigidBody.IsActive)
            {
                this.CubeGrid.ActivatePhysics();
            }
        }

        public override void Unregister(VRage.Game.Entity.MyEntity entity, Vector3I forwardVector)
        {
            base.Unregister(entity, forwardVector);
            MyThrust thrust = entity as MyThrust;
            if (thrust != null)
            {
                thrust.SlimBlock.ComponentStack.IsFunctionalChanged -= new Action(this.ComponentStack_IsFunctionalChanged);
                thrust.ThrustOverrideChanged -= new Action<MyThrust, float>(this.MyThrust_ThrustOverrideChanged);
                thrust.EnabledChanged -= new Action<MyTerminalBlock>(this.thrust_EnabledChanged);
                base.SlowdownFactor = 0f;
                foreach (Vector3I vectori in Base6Directions.IntDirections)
                {
                    List<MyEntityThrustComponent.FuelTypeData>.Enumerator enumerator;
                    using (enumerator = base.m_dataByFuelType.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            foreach (MyThrust thrust2 in enumerator.Current.ThrustsByDirection[vectori])
                            {
                                if (thrust2 != null)
                                {
                                    base.SlowdownFactor = Math.Max(thrust2.BlockDefinition.SlowdownFactor, base.SlowdownFactor);
                                }
                            }
                        }
                    }
                    using (List<MyEntityThrustComponent.MyConveyorConnectedGroup>.Enumerator enumerator3 = base.ConnectedGroups.GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            using (enumerator = enumerator3.Current.DataByFuelType.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    foreach (MyThrust thrust3 in enumerator.Current.ThrustsByDirection[vectori])
                                    {
                                        if (thrust3 != null)
                                        {
                                            base.SlowdownFactor = Math.Max(thrust3.BlockDefinition.SlowdownFactor, base.SlowdownFactor);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void UpdateThrusts(bool enableDampeners, Vector3 dampeningVelocity)
        {
            base.UpdateThrusts(enableDampeners, dampeningVelocity);
            MyCubeGrid cubeGrid = this.CubeGrid;
            if (cubeGrid != null)
            {
                MyGridPhysics physics = cubeGrid.Physics;
                if (((physics != null) && !physics.IsStatic) && physics.Enabled)
                {
                    Vector3 finalThrust = base.FinalThrust;
                    if (finalThrust.LengthSquared() > 0.0001f)
                    {
                        if (physics.IsWelded)
                        {
                            finalThrust = Vector3.TransformNormal(Vector3.TransformNormal(finalThrust, this.CubeGrid.WorldMatrix), Matrix.Invert(this.CubeGrid.Physics.RigidBody.GetRigidBodyMatrix()));
                        }
                        MyGridPhysicalGroupData.GroupSharedPxProperties groupSharedProperties = MyGridPhysicalGroupData.GetGroupSharedProperties(cubeGrid, true);
                        float? maxSpeed = null;
                        if (groupSharedProperties.GridCount == 1)
                        {
                            maxSpeed = new float?(MyGridPhysics.GetShipMaxLinearVelocity(cubeGrid.GridSizeEnum));
                        }
                        else
                        {
                            MyCubeGrid root = MyGridPhysicalHierarchy.Static.GetRoot(cubeGrid);
                            MyGridPhysics physics2 = root.Physics;
                            if ((physics2 != null) && !physics2.IsStatic)
                            {
                                Vector3D vectord2 = Vector3D.TransformNormal(finalThrust, cubeGrid.WorldMatrix);
                                Vector3 linearVelocity = physics2.LinearVelocity;
                                Vector3D vectord3 = linearVelocity + ((vectord2 * 0.01666666753590107) / ((double) groupSharedProperties.Mass));
                                float shipMaxLinearVelocity = MyGridPhysics.GetShipMaxLinearVelocity(root.GridSizeEnum);
                                if (vectord3.LengthSquared() > (shipMaxLinearVelocity * shipMaxLinearVelocity))
                                {
                                    float num2 = Vector3.Dot((Vector3) vectord2, linearVelocity) / linearVelocity.LengthSquared();
                                    if (num2 > 0f)
                                    {
                                        finalThrust = (Vector3) Vector3D.TransformNormal(vectord2 - (num2 * linearVelocity), cubeGrid.PositionComp.WorldMatrixNormalizedInv);
                                    }
                                }
                            }
                        }
                        Vector3D vectord = !ReferenceEquals(groupSharedProperties.ReferenceGrid, cubeGrid) ? Vector3D.Transform(groupSharedProperties.CoMWorld, cubeGrid.PositionComp.WorldMatrixNormalizedInv) : groupSharedProperties.PxProperties.CenterOfMass;
                        Vector3? torque = null;
                        physics.AddForce(MyPhysicsForceType.ADD_BODY_FORCE_AND_BODY_TORQUE, new Vector3?(finalThrust), new Vector3D?(vectord), torque, maxSpeed, false, true);
                    }
                }
            }
        }

        protected override void UpdateThrustStrength(HashSet<VRage.Game.Entity.MyEntity> thrusters, float thrustForce)
        {
            foreach (MyThrust thrust in thrusters)
            {
                if (thrust != null)
                {
                    if ((thrustForce == 0f) && !IsOverridden(thrust))
                    {
                        thrust.CurrentStrength = 0f;
                        continue;
                    }
                    float num = this.CalculateForceMultiplier(thrust, base.m_lastPlanetaryInfluence, base.m_lastPlanetaryInfluenceHasAtmosphere);
                    MyResourceSinkComponent component = base.ResourceSink(thrust);
                    if (!IsOverridden(thrust))
                    {
                        thrust.CurrentStrength = !this.IsUsed(thrust) ? 0f : ((num * thrustForce) * component.SuppliedRatioByType(thrust.FuelDefinition.Id));
                    }
                    else if (!MySession.Static.CreativeMode || !thrust.IsWorking)
                    {
                        thrust.CurrentStrength = ((num * thrust.ThrustOverride) * component.SuppliedRatioByType(thrust.FuelDefinition.Id)) / thrust.ThrustForce.Length();
                    }
                    else
                    {
                        thrust.CurrentStrength = num * thrust.ThrustOverrideOverForceLen;
                    }
                }
            }
        }

        private MyCubeGrid Entity =>
            (base.Entity as MyCubeGrid);

        private MyCubeGrid CubeGrid =>
            this.Entity;
    }
}

