namespace Sandbox.Game.GameSystems
{
    using Sandbox;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.AI;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyDroneAI : MyRemoteControl.IRemoteControlAutomaticBehaviour
    {
        private MyRemoteControl m_remoteControl;
        private MyEntity3DSoundEmitter m_soundEmitter;
        private bool m_resetSound;
        private int m_frameCounter;
        public float m_maxPlayerDistance;
        public float m_maxPlayerDistanceSq;
        private bool m_rotateToTarget;
        private bool m_canRotateToTarget;
        private MyDroneAIData m_currentPreset;
        private bool m_avoidCollisions;
        private bool m_alternativebehaviorSwitched;
        private int m_waypointDelayMs;
        private int m_waypointReachedTimeMs;
        private Vector3D m_returnPosition;
        private int m_lostStartTimeMs;
        private int m_waypointStartTimeMs;
        private int m_lastTargetUpdate;
        private int m_lastWeaponUpdate;
        private bool m_farAwayFromTarget;
        private VRage.Game.Entity.MyEntity m_currentTarget;
        private List<MyUserControllableGun> m_weapons;
        private List<MyFunctionalBlock> m_tools;
        private bool m_shooting;
        private bool m_operational;
        private bool m_canSkipWaypoint;
        private bool m_cycleWaypoints;
        private List<VRage.Game.Entity.MyEntity> m_forcedWaypoints;
        private List<DroneTarget> m_targetsList;
        private List<DroneTarget> m_targetsFiltered;
        private TargetPrioritization m_prioritizationStyle;
        public bool m_loadItems;
        private bool m_loadEntities;
        private long m_loadCurrentTarget;
        private List<MyObjectBuilder_AutomaticBehaviour.DroneTargetSerializable> m_loadTargetList;
        private List<long> m_loadWaypointList;
        private MyWeaponBehavior m_currentWeaponBehavior;
        private List<float> m_weaponBehaviorTimes;
        private List<int> m_weaponBehaviorAssignedRules;
        private List<bool> m_weaponBehaviorWeaponLock;
        private float m_weaponBehaviorCooldown;
        private bool m_weaponBehaviorActive;

        public MyDroneAI()
        {
            this.m_rotateToTarget = true;
            this.m_canRotateToTarget = true;
            this.m_weapons = new List<MyUserControllableGun>();
            this.m_tools = new List<MyFunctionalBlock>();
            this.m_operational = true;
            this.m_canSkipWaypoint = true;
            this.m_forcedWaypoints = new List<VRage.Game.Entity.MyEntity>();
            this.m_targetsList = new List<DroneTarget>();
            this.m_targetsFiltered = new List<DroneTarget>();
            this.m_prioritizationStyle = TargetPrioritization.PriorityRandom;
            this.m_loadItems = true;
            this.m_weaponBehaviorTimes = new List<float>();
            this.m_weaponBehaviorAssignedRules = new List<int>();
            this.m_weaponBehaviorWeaponLock = new List<bool>();
        }

        public MyDroneAI(MyRemoteControl remoteControl, string presetName, bool activate, List<VRage.Game.Entity.MyEntity> waypoints, List<DroneTarget> targets, int playerPriority, TargetPrioritization prioritizationStyle, float maxPlayerDistance, bool cycleWaypoints)
        {
            this.m_rotateToTarget = true;
            this.m_canRotateToTarget = true;
            this.m_weapons = new List<MyUserControllableGun>();
            this.m_tools = new List<MyFunctionalBlock>();
            this.m_operational = true;
            this.m_canSkipWaypoint = true;
            this.m_forcedWaypoints = new List<VRage.Game.Entity.MyEntity>();
            this.m_targetsList = new List<DroneTarget>();
            this.m_targetsFiltered = new List<DroneTarget>();
            this.m_prioritizationStyle = TargetPrioritization.PriorityRandom;
            this.m_loadItems = true;
            this.m_weaponBehaviorTimes = new List<float>();
            this.m_weaponBehaviorAssignedRules = new List<int>();
            this.m_weaponBehaviorWeaponLock = new List<bool>();
            this.m_remoteControl = remoteControl;
            this.m_returnPosition = this.m_remoteControl.PositionComp.GetPosition();
            this.m_currentPreset = MyDroneAIDataStatic.LoadPreset(presetName);
            this.Ambushing = false;
            this.LoadDroneAIData();
            this.m_lastTargetUpdate = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            this.m_lastWeaponUpdate = this.m_lastTargetUpdate;
            this.m_waypointReachedTimeMs = this.m_lastTargetUpdate;
            this.m_forcedWaypoints = waypoints ?? new List<VRage.Game.Entity.MyEntity>();
            this.m_targetsList = targets ?? new List<DroneTarget>();
            this.PlayerPriority = playerPriority;
            this.m_prioritizationStyle = prioritizationStyle;
            this.MaxPlayerDistance = maxPlayerDistance;
            this.m_cycleWaypoints = cycleWaypoints;
            this.NeedUpdate = activate;
        }

        private void ChangeWeaponBehavior()
        {
            bool flag3;
            this.m_currentWeaponBehavior = null;
            this.m_weaponBehaviorTimes.Clear();
            this.m_weaponBehaviorAssignedRules.Clear();
            this.m_weaponBehaviorWeaponLock.Clear();
            if (this.m_currentTarget == null)
            {
                this.m_weaponBehaviorCooldown = this.m_currentPreset.WeaponBehaviorNotFoundDelay;
                return;
            }
            List<int> list = new List<int>();
            int maxValue = 0;
            bool hitVoxel = false;
            bool hitGrid = false;
            Vector3 from = (Vector3) (((this.m_remoteControl.CubeGrid.PositionComp.LocalVolume.Radius * this.m_remoteControl.CubeGrid.WorldMatrix.Forward) * 1.1000000238418579) + this.m_remoteControl.CubeGrid.PositionComp.WorldAABB.Center);
            this.RaycastCheck(from, out hitVoxel, out hitGrid);
            using (List<MyWeaponBehavior>.Enumerator enumerator = this.m_currentPreset.WeaponBehaviors.GetEnumerator())
            {
                while (true)
                {
                    MyWeaponBehavior current;
                    bool flag4;
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            current = enumerator.Current;
                            flag4 = true;
                            if (!current.IgnoresVoxels & hitVoxel)
                            {
                                flag4 = false;
                            }
                            if (!current.IgnoresGrids & hitGrid)
                            {
                                flag4 = false;
                            }
                            flag3 = false;
                            if (flag4 && (current.WeaponRules.Count > 0))
                            {
                                if (current.RequirementsIsWhitelist || (current.Requirements.Count > 0))
                                {
                                    foreach (MyUserControllableGun gun in this.m_weapons)
                                    {
                                        if (!gun.Enabled)
                                        {
                                            continue;
                                        }
                                        if (gun.IsFunctional && gun.IsStationary())
                                        {
                                            flag3 = current.Requirements.Contains(gun.BlockDefinition.Id.TypeId.ToString());
                                            if (!current.RequirementsIsWhitelist)
                                            {
                                                flag3 = !flag3;
                                            }
                                            if (flag3)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    break;
                                }
                                flag3 = true;
                            }
                        }
                        else
                        {
                            goto TR_0027;
                        }
                        break;
                    }
                    if (!(flag3 & flag4) || (current.WeaponRules.Count <= 0))
                    {
                        list.Add(0);
                    }
                    else
                    {
                        int item = Math.Max(0, current.Priority);
                        list.Add(item);
                        maxValue += item;
                    }
                }
            }
        TR_0027:
            if (maxValue > 0)
            {
                int num3 = MyUtils.GetRandomInt(0, maxValue) + 1;
                int item = 0;
                while (true)
                {
                    if (item < list.Count)
                    {
                        if (num3 > list[item])
                        {
                            num3 -= list[item];
                            item++;
                            continue;
                        }
                        this.m_currentWeaponBehavior = this.m_currentPreset.WeaponBehaviors[item];
                    }
                    if (this.m_currentWeaponBehavior == null)
                    {
                        break;
                    }
                    foreach (MyWeaponRule local1 in this.m_currentWeaponBehavior.WeaponRules)
                    {
                        this.m_weaponBehaviorTimes.Add(-1f);
                    }
                    foreach (MyUserControllableGun gun2 in this.m_weapons)
                    {
                        this.m_weaponBehaviorWeaponLock.Add(false);
                        bool flag5 = false;
                        if (this.m_currentWeaponBehavior.RequirementsIsWhitelist || (this.m_currentWeaponBehavior.Requirements.Count > 0))
                        {
                            flag3 = this.m_currentWeaponBehavior.Requirements.Contains(gun2.BlockDefinition.Id.TypeId.ToString());
                            if (!this.m_currentWeaponBehavior.RequirementsIsWhitelist)
                            {
                                flag3 = !flag3;
                            }
                            if (!flag3 || !gun2.IsStationary())
                            {
                                this.m_weaponBehaviorAssignedRules.Add(-1);
                                continue;
                            }
                        }
                        item = 0;
                        while (true)
                        {
                            if (item < this.m_currentWeaponBehavior.WeaponRules.Count)
                            {
                                if (!string.IsNullOrEmpty(this.m_currentWeaponBehavior.WeaponRules[item].Weapon) && !this.m_currentWeaponBehavior.WeaponRules[item].Weapon.Equals(gun2.BlockDefinition.Id.TypeId.ToString()))
                                {
                                    item++;
                                    continue;
                                }
                                flag5 = true;
                                this.m_weaponBehaviorAssignedRules.Add(item);
                                this.m_weaponBehaviorTimes[item] = MyUtils.GetRandomFloat(this.m_currentWeaponBehavior.WeaponRules[item].TimeMin, this.m_currentWeaponBehavior.WeaponRules[item].TimeMax);
                            }
                            if (!flag5)
                            {
                                this.m_weaponBehaviorAssignedRules.Add(-1);
                            }
                            break;
                        }
                    }
                    this.m_weaponBehaviorActive = true;
                    return;
                }
            }
            this.m_weaponBehaviorCooldown = this.m_currentPreset.WeaponBehaviorNotFoundDelay;
        }

        public void DebugDraw()
        {
            if (this.m_remoteControl.CurrentWaypoint != null)
            {
                MyRenderProxy.DebugDrawSphere(this.m_remoteControl.CurrentWaypoint.Coords, 0.5f, Color.Aquamarine, 1f, true, false, true, false);
            }
            if (this.m_currentTarget != null)
            {
                MyRenderProxy.DebugDrawSphere(this.m_currentTarget.PositionComp.GetPosition(), 2f, this.m_canRotateToTarget ? Color.Green : Color.Red, 1f, true, false, true, false);
            }
        }

        private bool FindNewTarget()
        {
            List<DroneTarget> list = new List<DroneTarget>();
            if (this.PlayerPriority > 0)
            {
                using (IEnumerator<MyPlayer> enumerator = MySession.Static.Players.GetOnlinePlayers().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        IMyControllableEntity controlledEntity = enumerator.Current.Controller.ControlledEntity;
                        if ((controlledEntity != null) && (!(controlledEntity is MyCharacter) || !((MyCharacter) controlledEntity).IsDead))
                        {
                            Vector3D translation = controlledEntity.Entity.WorldMatrix.Translation;
                            if (Vector3D.DistanceSquared(this.m_remoteControl.PositionComp.GetPosition(), translation) < this.m_maxPlayerDistanceSq)
                            {
                                list.Add(new DroneTarget((VRage.Game.Entity.MyEntity) controlledEntity, this.PlayerPriority));
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < this.m_targetsList.Count; i++)
            {
                if (this.IsValidTarget(this.m_targetsList[i].Target))
                {
                    list.Add(this.m_targetsList[i]);
                }
            }
            this.m_targetsFiltered.Clear();
            this.m_targetsFiltered = list;
            if (list.Count == 0)
            {
                return false;
            }
            bool flag = this.m_prioritizationStyle == TargetPrioritization.Random;
            switch (this.m_prioritizationStyle)
            {
                case TargetPrioritization.ClosestFirst:
                {
                    double maxValue = double.MaxValue;
                    foreach (DroneTarget target in list)
                    {
                        double num5 = Vector3D.DistanceSquared(this.m_remoteControl.PositionComp.GetPosition(), target.Target.PositionComp.GetPosition());
                        if (num5 < maxValue)
                        {
                            maxValue = num5;
                            this.m_currentTarget = target.Target;
                        }
                    }
                    return true;
                }
                case TargetPrioritization.PriorityRandom:
                case TargetPrioritization.Random:
                {
                    int maxValue = 0;
                    foreach (DroneTarget target2 in list)
                    {
                        maxValue += flag ? 1 : Math.Max(0, target2.Priority);
                    }
                    int num4 = MyUtils.GetRandomInt(0, maxValue) + 1;
                    foreach (DroneTarget target3 in list)
                    {
                        int num6 = flag ? 1 : Math.Max(0, target3.Priority);
                        if (num4 <= num6)
                        {
                            this.m_currentTarget = target3.Target;
                            break;
                        }
                        num4 -= num6;
                    }
                    return true;
                }
            }
            list.Sort();
            this.m_currentTarget = list[list.Count - 1].Target;
            return true;
        }

        public MyObjectBuilder_AutomaticBehaviour GetObjectBuilder()
        {
            MyObjectBuilder_DroneAI eai = new MyObjectBuilder_DroneAI {
                CollisionAvoidance = this.CollisionAvoidance,
                CurrentTarget = (this.m_currentTarget != null) ? this.m_currentTarget.EntityId : 0L,
                CycleWaypoints = this.m_cycleWaypoints,
                IsActive = this.IsActive,
                MaxPlayerDistance = this.m_maxPlayerDistance,
                NeedUpdate = this.NeedUpdate,
                InAmbushMode = this.Ambushing,
                PlayerPriority = this.PlayerPriority,
                PrioritizationStyle = this.m_prioritizationStyle,
                TargetList = new List<MyObjectBuilder_AutomaticBehaviour.DroneTargetSerializable>()
            };
            foreach (DroneTarget target in this.m_targetsList)
            {
                if (target.Target != null)
                {
                    eai.TargetList.Add(new MyObjectBuilder_AutomaticBehaviour.DroneTargetSerializable(target.Target.EntityId, target.Priority));
                }
            }
            eai.WaypointList = new List<long>();
            foreach (VRage.Game.Entity.MyEntity entity in this.m_forcedWaypoints)
            {
                if (entity != null)
                {
                    eai.WaypointList.Add(entity.EntityId);
                }
            }
            eai.CurrentPreset = this.m_currentPreset.Name;
            eai.AlternativebehaviorSwitched = this.m_alternativebehaviorSwitched;
            eai.ReturnPosition = this.m_returnPosition;
            eai.CanSkipWaypoint = this.m_canSkipWaypoint;
            eai.SpeedLimit = this.SpeedLimit;
            return eai;
        }

        private Vector3D GetRandomPoint()
        {
            int num = 0;
            MatrixD xd = MatrixD.CreateFromDir(Vector3D.Normalize(this.m_currentTarget.PositionComp.GetPosition() - this.m_remoteControl.PositionComp.GetPosition()));
            while (true)
            {
                Vector3D vectord = xd.Right * MyUtils.GetRandomFloat(-this.m_currentPreset.Width, this.m_currentPreset.Width);
                Vector3D vectord2 = xd.Up * MyUtils.GetRandomFloat(-this.m_currentPreset.Height, this.m_currentPreset.Height);
                Vector3D vectord3 = xd.Forward * MyUtils.GetRandomFloat(-this.m_currentPreset.Depth, this.m_currentPreset.Depth);
                Vector3D vectord4 = ((this.m_remoteControl.PositionComp.GetPosition() + vectord) + vectord2) + vectord3;
                Vector3D vectord5 = vectord4 - this.m_remoteControl.PositionComp.GetPosition();
                if ((((float) vectord5.LengthSquared()) > this.m_currentPreset.MinStrafeDistanceSq) || (++num >= 10))
                {
                    return vectord4;
                }
            }
        }

        private void HoverMechanic(ref Vector3D pos)
        {
            Vector3 vector = MyGravityProviderSystem.CalculateNaturalGravityInPoint(pos);
            if (vector.LengthSquared() > 0f)
            {
                MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(pos);
                if (closestPlanet != null)
                {
                    Vector3D closestSurfacePointGlobal = closestPlanet.GetClosestSurfacePointGlobal(ref pos);
                    float num = (float) Vector3D.Distance(closestSurfacePointGlobal, pos);
                    if (Vector3D.DistanceSquared(closestPlanet.PositionComp.GetPosition(), closestSurfacePointGlobal) > Vector3D.DistanceSquared(closestPlanet.PositionComp.GetPosition(), pos))
                    {
                        num *= -1f;
                    }
                    if (num < this.m_currentPreset.PlanetHoverMin)
                    {
                        pos = closestSurfacePointGlobal - (Vector3D.Normalize(vector) * this.m_currentPreset.PlanetHoverMin);
                    }
                    else if (num > this.m_currentPreset.PlanetHoverMax)
                    {
                        pos = closestSurfacePointGlobal - (Vector3D.Normalize(vector) * this.m_currentPreset.PlanetHoverMax);
                    }
                }
            }
        }

        private bool IsValidTarget(VRage.Game.Entity.MyEntity target)
        {
            if (!(target is MyCharacter) || ((MyCharacter) target).IsDead)
            {
                return ((target is MyCubeBlock) && ((MyCubeBlock) target).IsFunctional);
            }
            return true;
        }

        public void Load(MyObjectBuilder_AutomaticBehaviour objectBuilder, MyRemoteControl remoteControl)
        {
            MyObjectBuilder_DroneAI eai = objectBuilder as MyObjectBuilder_DroneAI;
            if (eai != null)
            {
                this.m_remoteControl = remoteControl;
                this.m_currentPreset = MyDroneAIDataStatic.LoadPreset(eai.CurrentPreset);
                this.LoadDroneAIData();
                this.m_lastTargetUpdate = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                this.m_lastWeaponUpdate = this.m_lastTargetUpdate;
                this.m_waypointReachedTimeMs = this.m_lastTargetUpdate;
                this.m_forcedWaypoints = new List<VRage.Game.Entity.MyEntity>();
                this.m_loadWaypointList = eai.WaypointList;
                this.m_targetsList = new List<DroneTarget>();
                this.m_loadTargetList = eai.TargetList;
                this.m_currentTarget = null;
                this.m_loadCurrentTarget = eai.CurrentTarget;
                this.Ambushing = eai.InAmbushMode;
                this.m_returnPosition = (Vector3D) eai.ReturnPosition;
                this.PlayerPriority = eai.PlayerPriority;
                this.m_prioritizationStyle = eai.PrioritizationStyle;
                this.MaxPlayerDistance = eai.MaxPlayerDistance;
                this.m_cycleWaypoints = eai.CycleWaypoints;
                this.m_alternativebehaviorSwitched = eai.AlternativebehaviorSwitched;
                this.CollisionAvoidance = eai.CollisionAvoidance;
                this.m_canSkipWaypoint = eai.CanSkipWaypoint;
                if (eai.SpeedLimit != float.MinValue)
                {
                    this.SpeedLimit = eai.SpeedLimit;
                }
                this.NeedUpdate = eai.NeedUpdate;
                this.IsActive = eai.IsActive;
                this.m_loadEntities = true;
            }
        }

        private void LoadDroneAIData()
        {
            if (this.m_currentPreset != null)
            {
                this.m_avoidCollisions = this.m_currentPreset.AvoidCollisions;
                this.m_rotateToTarget = this.m_currentPreset.RotateToPlayer;
                this.PlayerYAxisOffset = this.m_currentPreset.PlayerYAxisOffset;
                this.WaypointThresholdDistance = this.m_currentPreset.WaypointThresholdDistance;
                this.SpeedLimit = this.m_currentPreset.SpeedLimit;
                if (string.IsNullOrEmpty(this.m_currentPreset.SoundLoop))
                {
                    if (this.m_soundEmitter != null)
                    {
                        this.m_soundEmitter.StopSound(true, true);
                    }
                }
                else
                {
                    if (this.m_soundEmitter == null)
                    {
                        this.m_soundEmitter = new MyEntity3DSoundEmitter(this.m_remoteControl, true, 1f);
                    }
                    MySoundPair objA = new MySoundPair(this.m_currentPreset.SoundLoop, true);
                    if (!ReferenceEquals(objA, MySoundPair.Empty))
                    {
                        bool? nullable = null;
                        this.m_soundEmitter.PlaySound(objA, true, false, false, false, false, nullable);
                    }
                }
            }
        }

        public void LoadEntities()
        {
            this.m_loadEntities = false;
            if (this.m_loadWaypointList != null)
            {
                foreach (long num in this.m_loadWaypointList)
                {
                    VRage.Game.Entity.MyEntity entity;
                    if (num <= 0L)
                    {
                        continue;
                    }
                    if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(num, out entity, false))
                    {
                        this.m_forcedWaypoints.Add(entity);
                    }
                }
                this.m_loadWaypointList.Clear();
            }
            if (this.m_loadTargetList != null)
            {
                foreach (MyObjectBuilder_AutomaticBehaviour.DroneTargetSerializable serializable in this.m_loadTargetList)
                {
                    VRage.Game.Entity.MyEntity entity2;
                    if (serializable.TargetId <= 0L)
                    {
                        continue;
                    }
                    if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(serializable.TargetId, out entity2, false))
                    {
                        this.m_targetsList.Add(new DroneTarget(entity2, serializable.Priority));
                    }
                }
                this.m_targetsList.Clear();
            }
            if (this.m_loadCurrentTarget > 0L)
            {
                VRage.Game.Entity.MyEntity entity3;
                Sandbox.Game.Entities.MyEntities.TryGetEntityById(this.m_loadCurrentTarget, out entity3, false);
                this.m_currentTarget = entity3;
            }
        }

        public void LoadShipGear()
        {
            this.m_loadItems = false;
            this.m_remoteControl.CubeGrid.GetBlocks();
            this.m_weapons = new List<MyUserControllableGun>();
            this.m_tools = new List<MyFunctionalBlock>();
            using (List<MyCubeGrid>.Enumerator enumerator = MyCubeGridGroups.Static.Logical.GetGroupNodes(this.m_remoteControl.CubeGrid).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    foreach (MySlimBlock block in enumerator.Current.GetBlocks())
                    {
                        if (block.FatBlock is MyUserControllableGun)
                        {
                            this.m_weapons.Add(block.FatBlock as MyUserControllableGun);
                        }
                        if (block.FatBlock is MyShipToolBase)
                        {
                            this.m_tools.Add(block.FatBlock as MyFunctionalBlock);
                        }
                        if (block.FatBlock is MyShipDrill)
                        {
                            this.m_tools.Add(block.FatBlock as MyFunctionalBlock);
                        }
                    }
                }
            }
        }

        private void RaycastCheck(Vector3 from, out bool hitVoxel, out bool hitGrid)
        {
            hitVoxel = false;
            hitGrid = false;
            Vector3 translation = (Vector3) this.m_currentTarget.WorldMatrix.Translation;
            if (this.m_currentTarget is MyCharacter)
            {
                translation += this.m_currentTarget.WorldMatrix.Up * this.PlayerYAxisOffset;
            }
            MyPhysics.HitInfo? nullable = MyPhysics.CastRay(from, translation, 15);
            IMyEntity objB = null;
            if (((nullable != null) && ((nullable.Value.HkHitInfo.Body != null) && (nullable.Value.HkHitInfo.Body.UserObject != null))) && (nullable.Value.HkHitInfo.Body.UserObject is MyPhysicsBody))
            {
                objB = ((MyPhysicsBody) nullable.Value.HkHitInfo.Body.UserObject).Entity;
            }
            if (((objB != null) && (!ReferenceEquals(this.m_currentTarget, objB) && (!ReferenceEquals(this.m_currentTarget.Parent, objB) && (((this.m_currentTarget.Parent == null) || !ReferenceEquals(this.m_currentTarget.Parent, objB.Parent)) && !(objB is MyMissile))))) && !(objB is MyFloatingObject))
            {
                if (objB is MyVoxelBase)
                {
                    hitVoxel = true;
                }
                else
                {
                    hitGrid = true;
                }
            }
        }

        public static bool SetAIToGrid(MyCubeGrid grid, string behaviour, float activationDistance)
        {
            bool flag;
            using (MyFatBlockReader<MyRemoteControl> reader = grid.GetFatBlocks<MyRemoteControl>())
            {
                if (!reader.MoveNext())
                {
                    flag = false;
                }
                else
                {
                    MyDroneAI automaticBehaviour = new MyDroneAI(reader.Current, behaviour, true, null, null, 1, TargetPrioritization.PriorityRandom, activationDistance, false);
                    MyRemoteControl current = reader.Current;
                    current.SetAutomaticBehaviour(automaticBehaviour);
                    current.SetAutoPilotEnabled(true);
                    flag = true;
                }
            }
            return flag;
        }

        public void StopWorking()
        {
            if ((this.m_soundEmitter != null) && this.m_soundEmitter.IsPlaying)
            {
                this.m_soundEmitter.StopSound(false, true);
                this.m_resetSound = true;
            }
        }

        public void TargetAdd(DroneTarget target)
        {
            if (!this.m_targetsList.Contains(target))
            {
                this.m_targetsList.Add(target);
            }
        }

        public void TargetClear()
        {
            this.m_targetsList.Clear();
        }

        public void TargetLoseCurrent()
        {
            this.m_currentTarget = null;
        }

        public void TargetRemove(VRage.Game.Entity.MyEntity target)
        {
            for (int i = 0; i < this.m_targetsList.Count; i++)
            {
                if (ReferenceEquals(this.m_targetsList[i].Target, target))
                {
                    this.m_targetsList.RemoveAt(i);
                    i--;
                }
            }
        }

        public void Update()
        {
            this.m_frameCounter++;
            if (this.m_resetSound)
            {
                MySoundPair objA = new MySoundPair(this.m_currentPreset.SoundLoop, true);
                if (!ReferenceEquals(objA, MySoundPair.Empty))
                {
                    bool? nullable = null;
                    this.m_soundEmitter.PlaySound(objA, true, false, false, false, false, nullable);
                }
                this.m_resetSound = false;
            }
            if ((this.m_soundEmitter != null) && ((this.m_frameCounter % 100) == 0))
            {
                this.m_soundEmitter.Update();
            }
            if (Sync.IsServer)
            {
                if (this.m_loadItems)
                {
                    this.LoadShipGear();
                }
                if (this.m_loadEntities)
                {
                    this.LoadEntities();
                }
                if (this.IsActive || this.NeedUpdate)
                {
                    this.UpdateWaypoint();
                }
            }
        }

        private void UpdateWaypoint()
        {
            int totalGamePlayTimeInMilliseconds = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            if ((this.m_currentTarget != null) && ((totalGamePlayTimeInMilliseconds - this.m_lastTargetUpdate) >= 0x3e8))
            {
                this.m_lastTargetUpdate = totalGamePlayTimeInMilliseconds;
                if (!this.IsValidTarget(this.m_currentTarget))
                {
                    this.m_currentTarget = null;
                }
            }
            if (((this.m_currentTarget == null) && ((totalGamePlayTimeInMilliseconds - this.m_lastTargetUpdate) >= 0x3e8)) || ((totalGamePlayTimeInMilliseconds - this.m_lostStartTimeMs) >= this.m_currentPreset.LostTimeMs))
            {
                this.FindNewTarget();
                this.m_lastTargetUpdate = totalGamePlayTimeInMilliseconds;
                if (this.m_currentTarget != null)
                {
                    this.m_lostStartTimeMs = totalGamePlayTimeInMilliseconds;
                    this.m_farAwayFromTarget = true;
                }
            }
            if ((!this.Ambushing && (this.m_farAwayFromTarget && ((totalGamePlayTimeInMilliseconds - this.m_lastTargetUpdate) >= 0x1388))) && this.m_canSkipWaypoint)
            {
                this.m_lastTargetUpdate = totalGamePlayTimeInMilliseconds;
                this.NeedUpdate = true;
            }
            float distSq = -1f;
            if (this.m_weaponBehaviorCooldown > 0f)
            {
                this.m_weaponBehaviorCooldown -= 0.01666667f;
            }
            if (this.m_operational && ((totalGamePlayTimeInMilliseconds - this.m_lastWeaponUpdate) >= 300))
            {
                this.m_lastWeaponUpdate = totalGamePlayTimeInMilliseconds;
                distSq = (this.m_currentTarget != null) ? Vector3.DistanceSquared((Vector3) this.m_currentTarget.PositionComp.GetPosition(), (Vector3) this.m_remoteControl.PositionComp.GetPosition()) : -1f;
                if (!this.m_currentPreset.UsesWeaponBehaviors || (this.m_weaponBehaviorCooldown <= 0f))
                {
                    this.WeaponsUpdate(distSq);
                }
                this.m_canRotateToTarget = (distSq < this.m_currentPreset.RotationLimitSq) && (distSq >= 0f);
            }
            if (!this.m_operational || this.m_shooting)
            {
                this.m_lostStartTimeMs = totalGamePlayTimeInMilliseconds;
            }
            if ((!this.Ambushing && ((totalGamePlayTimeInMilliseconds - this.m_waypointReachedTimeMs) >= this.m_currentPreset.WaypointMaxTime)) && this.m_canSkipWaypoint)
            {
                this.NeedUpdate = true;
            }
            if ((!this.Ambushing && (this.m_remoteControl.CurrentWaypoint == null)) && (this.WaypointList.Count > 0))
            {
                this.NeedUpdate = true;
            }
            if (this.NeedUpdate)
            {
                Vector3D position;
                int avoidCollisions;
                this.IsActive = true;
                if ((distSq < 0f) && (this.m_currentTarget != null))
                {
                    distSq = Vector3.DistanceSquared((Vector3) this.m_currentTarget.PositionComp.GetPosition(), (Vector3) this.m_remoteControl.PositionComp.GetPosition());
                }
                this.m_farAwayFromTarget = distSq > this.m_currentPreset.MaxManeuverDistanceSq;
                this.m_canRotateToTarget = (distSq < this.m_currentPreset.RotationLimitSq) && (distSq >= 0f);
                bool needUpdate = this.NeedUpdate;
                if (this.m_remoteControl.HasWaypoints())
                {
                    this.m_remoteControl.ClearWaypoints();
                }
                this.m_remoteControl.SetAutoPilotEnabled(true);
                this.NeedUpdate = needUpdate;
                this.m_canSkipWaypoint = true;
                string name = "Player Vicinity";
                if (this.m_forcedWaypoints.Count > 0)
                {
                    if (this.m_cycleWaypoints)
                    {
                        this.m_forcedWaypoints.Add(this.m_forcedWaypoints[0]);
                    }
                    position = this.m_forcedWaypoints[0].PositionComp.GetPosition();
                    name = this.m_forcedWaypoints[0].Name;
                    this.m_forcedWaypoints.RemoveAt(0);
                    this.m_canSkipWaypoint = false;
                }
                else if (this.m_currentTarget == null)
                {
                    position = this.m_remoteControl.WorldMatrix.Translation + (Vector3.One * 0.01f);
                }
                else if (!this.m_operational && this.m_currentPreset.UseKamikazeBehavior)
                {
                    if (this.m_remoteControl.TargettingAimDelta > 0.019999999552965164)
                    {
                        return;
                    }
                    position = (this.m_currentTarget.PositionComp.GetPosition() + ((this.m_currentTarget.WorldMatrix.Up * this.PlayerYAxisOffset) * 2.0)) - (Vector3D.Normalize(this.m_remoteControl.PositionComp.GetPosition() - this.m_currentTarget.PositionComp.GetPosition()) * this.m_currentPreset.KamikazeBehaviorDistance);
                }
                else if (!this.m_operational && !this.m_currentPreset.UseKamikazeBehavior)
                {
                    position = this.m_returnPosition + (Vector3.One * 0.01f);
                }
                else if (this.m_farAwayFromTarget)
                {
                    position = this.m_currentTarget.PositionComp.GetPosition() + (Vector3D.Normalize(this.m_remoteControl.PositionComp.GetPosition() - this.m_currentTarget.PositionComp.GetPosition()) * this.m_currentPreset.PlayerTargetDistance);
                    if (this.m_currentPreset.UsePlanetHover)
                    {
                        this.HoverMechanic(ref position);
                    }
                }
                else
                {
                    if ((totalGamePlayTimeInMilliseconds - this.m_waypointReachedTimeMs) <= this.m_waypointDelayMs)
                    {
                        return;
                    }
                    position = this.GetRandomPoint();
                    name = "Strafe";
                    if (this.m_currentPreset.UsePlanetHover)
                    {
                        this.HoverMechanic(ref position);
                    }
                }
                (position - this.m_remoteControl.WorldMatrix.Translation).Normalize();
                this.m_waypointReachedTimeMs = totalGamePlayTimeInMilliseconds;
                bool flag2 = this.m_currentPreset.UseKamikazeBehavior && !this.m_operational;
                this.m_remoteControl.ChangeFlightMode(FlightMode.OneWay);
                this.m_remoteControl.SetAutoPilotSpeedLimit(flag2 ? 100f : this.SpeedLimit);
                if (flag2 || !this.m_canSkipWaypoint)
                {
                    avoidCollisions = 0;
                }
                else
                {
                    avoidCollisions = (int) this.m_avoidCollisions;
                }
                this.m_remoteControl.SetCollisionAvoidance((bool) avoidCollisions);
                this.m_remoteControl.ChangeDirection(Base6Directions.Direction.Forward);
                this.m_remoteControl.AddWaypoint(position, name);
                this.NeedUpdate = false;
                this.IsActive = true;
            }
        }

        public void WaypointAdd(VRage.Game.Entity.MyEntity target)
        {
            if ((target != null) && !this.m_forcedWaypoints.Contains(target))
            {
                this.m_forcedWaypoints.Add(target);
            }
        }

        public void WaypointAdvanced()
        {
            if (Sync.IsServer)
            {
                this.m_waypointReachedTimeMs = MySandboxGame.TotalGamePlayTimeInMilliseconds + MyUtils.GetRandomInt(this.m_currentPreset.WaypointDelayMsMin, this.m_currentPreset.WaypointDelayMsMax);
                if ((!this.Ambushing && this.IsActive) && (((this.m_remoteControl.CurrentWaypoint != null) || (this.m_targetsFiltered.Count > 0)) || (this.m_forcedWaypoints.Count > 0)))
                {
                    this.NeedUpdate = true;
                }
            }
        }

        public void WaypointClear()
        {
            this.m_forcedWaypoints.Clear();
        }

        private void WeaponsUpdate(float distSq)
        {
            int num;
            this.m_shooting = false;
            if (this.m_currentPreset.UsesWeaponBehaviors && this.m_weaponBehaviorActive)
            {
                bool flag6 = false;
                num = 0;
                while (true)
                {
                    if (num >= this.m_weaponBehaviorTimes.Count)
                    {
                        if (flag6)
                        {
                            break;
                        }
                        this.m_weaponBehaviorActive = false;
                        this.m_weaponBehaviorCooldown = MyUtils.GetRandomFloat(this.m_currentWeaponBehavior.TimeMin, this.m_currentWeaponBehavior.TimeMax);
                        num = 0;
                        while (num < this.m_weapons.Count)
                        {
                            this.m_weapons[num].SetShooting(false);
                            num++;
                        }
                        return;
                    }
                    if (this.m_weaponBehaviorTimes[num] >= 0f)
                    {
                        flag6 = true;
                    }
                    num++;
                }
            }
            bool shooting = true;
            if (this.m_currentPreset.UsesWeaponBehaviors && !this.m_weaponBehaviorActive)
            {
                this.ChangeWeaponBehavior();
                if (!this.m_weaponBehaviorActive)
                {
                    shooting = false;
                }
            }
            bool canBeDisabled = this.m_currentPreset.CanBeDisabled;
            bool flag3 = false;
            bool hitVoxel = false;
            bool hitGrid = false;
            int num2 = 0;
            if (this.m_weapons == null)
            {
                goto TR_0025;
            }
            else if (this.m_weapons.Count <= 0)
            {
                goto TR_0025;
            }
            else
            {
                num = 0;
            }
            goto TR_0060;
        TR_0025:
            if (this.m_currentPreset.UsesWeaponBehaviors && this.m_shooting)
            {
                num = 0;
                while (num < this.m_weaponBehaviorTimes.Count)
                {
                    List<float> weaponBehaviorTimes = this.m_weaponBehaviorTimes;
                    int num3 = num;
                    weaponBehaviorTimes[num3] -= 0.3f;
                    num++;
                }
            }
            if ((this.m_tools != null) && (this.m_tools.Count > 0))
            {
                num = 0;
                while (num < this.m_tools.Count)
                {
                    if (this.m_tools[num].IsFunctional)
                    {
                        canBeDisabled = false;
                        if (this.m_currentPreset.UseTools)
                        {
                            if (((distSq >= this.m_currentPreset.ToolsUsageSq) || (distSq < 0f)) || !this.m_canRotateToTarget)
                            {
                                this.m_tools[num].Enabled = false;
                            }
                            else
                            {
                                this.m_tools[num].Enabled = true;
                                this.Ambushing = false;
                            }
                        }
                    }
                    num++;
                }
            }
            this.m_operational = !canBeDisabled;
            if (canBeDisabled)
            {
                this.m_rotateToTarget = true;
                this.m_weapons.Clear();
                this.m_tools.Clear();
                if (this.m_remoteControl.HasWaypoints())
                {
                    this.m_remoteControl.ClearWaypoints();
                }
                this.NeedUpdate = true;
                this.m_forcedWaypoints.Clear();
            }
            if (!flag3 && !this.m_alternativebehaviorSwitched)
            {
                this.m_rotateToTarget = true;
                if (this.m_currentPreset.AlternativeBehavior.Length > 0)
                {
                    this.m_currentPreset = MyDroneAIDataStatic.LoadPreset(this.m_currentPreset.AlternativeBehavior);
                    this.LoadDroneAIData();
                }
                this.m_alternativebehaviorSwitched = true;
            }
            return;
        TR_0026:
            num++;
            goto TR_0060;
        TR_0027:
            this.m_weapons.RemoveAt(num);
            num--;
            goto TR_0026;
        TR_0033:
            if (!this.m_weapons[num].IsStationary())
            {
                if ((this.Ambushing && (this.m_weapons[num] is MyLargeTurretBase)) && ((MyLargeTurretBase) this.m_weapons[num]).IsShooting)
                {
                    this.Ambushing = false;
                    this.m_shooting = true;
                }
                flag3 = true;
            }
            goto TR_0026;
        TR_004A:
            if ((!this.m_currentPreset.UsesWeaponBehaviors || !this.m_weaponBehaviorActive) || !this.m_currentWeaponBehavior.WeaponRules[this.m_weaponBehaviorAssignedRules[num]].FiringAfterLosingSight)
            {
                this.m_weapons[num].SetShooting(false);
            }
            goto TR_0033;
        TR_0060:
            while (true)
            {
                if (num < this.m_weapons.Count)
                {
                    if (this.m_weapons[num].Closed)
                    {
                        goto TR_0027;
                    }
                    else
                    {
                        if (!ReferenceEquals(this.m_weapons[num].CubeGrid, this.m_remoteControl.CubeGrid) && !MyCubeGridGroups.Static.Logical.HasSameGroup(this.m_weapons[num].CubeGrid, this.m_remoteControl.CubeGrid))
                        {
                            goto TR_0027;
                        }
                        if (!this.m_weapons[num].Enabled && this.m_weapons[num].IsFunctional)
                        {
                            canBeDisabled = false;
                            if (!this.m_weapons[num].IsStationary())
                            {
                                flag3 = true;
                            }
                        }
                        else
                        {
                            MyGunStatusEnum enum2;
                            if ((this.m_weapons[num].CanOperate() && this.m_weapons[num].CanShoot(out enum2)) && (enum2 == MyGunStatusEnum.OK))
                            {
                                canBeDisabled = false;
                                if (this.m_currentPreset.UseStaticWeaponry && this.m_weapons[num].IsStationary())
                                {
                                    if (this.m_currentPreset.UsesWeaponBehaviors && this.m_weaponBehaviorActive)
                                    {
                                        if (this.m_weaponBehaviorAssignedRules[num] == -1)
                                        {
                                            break;
                                        }
                                        if (this.m_weaponBehaviorWeaponLock[num])
                                        {
                                            this.m_shooting = shooting;
                                            break;
                                        }
                                        if (this.m_weaponBehaviorTimes[this.m_weaponBehaviorAssignedRules[num]] < 0f)
                                        {
                                            this.m_weapons[num].SetShooting(false);
                                            break;
                                        }
                                    }
                                    if (this.m_remoteControl.TargettingAimDelta > 0.05000000074505806)
                                    {
                                        goto TR_004A;
                                    }
                                    else if (distSq >= this.m_currentPreset.StaticWeaponryUsageSq)
                                    {
                                        goto TR_004A;
                                    }
                                    else if (distSq < 0f)
                                    {
                                        goto TR_004A;
                                    }
                                    else if (!this.m_canRotateToTarget)
                                    {
                                        goto TR_004A;
                                    }
                                    else
                                    {
                                        this.m_shooting = shooting;
                                        if (!this.m_weaponBehaviorActive)
                                        {
                                            break;
                                        }
                                        if (this.m_currentPreset.UsesWeaponBehaviors && (!this.m_currentWeaponBehavior.WeaponRules[this.m_weaponBehaviorAssignedRules[num]].CanGoThroughVoxels || !this.m_currentWeaponBehavior.IgnoresGrids))
                                        {
                                            if (num2 < 10)
                                            {
                                                num2++;
                                                this.RaycastCheck((Vector3) this.m_weapons[num].GetWeaponMuzzleWorldPosition(), out hitVoxel, out hitGrid);
                                            }
                                            if ((hitVoxel && !this.m_currentWeaponBehavior.WeaponRules[this.m_weaponBehaviorAssignedRules[num]].CanGoThroughVoxels) || (hitGrid && !this.m_currentWeaponBehavior.IgnoresGrids))
                                            {
                                                this.m_weapons[num].SetShooting(false);
                                                break;
                                            }
                                        }
                                        if (!this.m_currentPreset.UsesWeaponBehaviors || (this.m_weaponBehaviorTimes[this.m_weaponBehaviorAssignedRules[num]] != 0f))
                                        {
                                            this.m_weapons[num].SetShooting(shooting);
                                        }
                                        else
                                        {
                                            this.m_weapons[num].ShootFromTerminal((Vector3) this.m_weapons[num].WorldMatrix.Forward);
                                        }
                                        this.Ambushing = false;
                                        if (this.m_currentPreset.UsesWeaponBehaviors && this.m_currentWeaponBehavior.WeaponRules[this.m_weaponBehaviorAssignedRules[num]].FiringAfterLosingSight)
                                        {
                                            this.m_weaponBehaviorWeaponLock[num] = shooting;
                                        }
                                    }
                                }
                                goto TR_0033;
                            }
                        }
                    }
                }
                else
                {
                    goto TR_0025;
                }
                break;
            }
            goto TR_0026;
        }

        public bool NeedUpdate { get; private set; }

        public bool IsActive { get; private set; }

        public bool RotateToTarget
        {
            get => 
                (this.m_canRotateToTarget && this.m_rotateToTarget);
            set => 
                (this.m_rotateToTarget = value);
        }

        public bool CollisionAvoidance
        {
            get => 
                this.m_avoidCollisions;
            set => 
                (this.m_avoidCollisions = value);
        }

        public Vector3D OriginPoint
        {
            get => 
                this.m_returnPosition;
            set => 
                (this.m_returnPosition = value);
        }

        public int PlayerPriority { get; set; }

        public TargetPrioritization PrioritizationStyle
        {
            get => 
                this.m_prioritizationStyle;
            set => 
                (this.m_prioritizationStyle = value);
        }

        public VRage.Game.Entity.MyEntity CurrentTarget
        {
            get => 
                this.m_currentTarget;
            set => 
                (this.m_currentTarget = value);
        }

        public string CurrentBehavior =>
            ((this.m_currentPreset != null) ? this.m_currentPreset.Name : "");

        public List<DroneTarget> TargetList =>
            this.m_targetsFiltered;

        public List<VRage.Game.Entity.MyEntity> WaypointList =>
            this.m_forcedWaypoints;

        public bool WaypointActive =>
            !this.m_canSkipWaypoint;

        public bool Ambushing { get; set; }

        public bool Operational =>
            this.m_operational;

        public float SpeedLimit { get; set; }

        public float MaxPlayerDistance
        {
            get => 
                this.m_maxPlayerDistance;
            private set
            {
                this.m_maxPlayerDistance = value;
                this.m_maxPlayerDistanceSq = value * value;
            }
        }

        public float PlayerYAxisOffset { get; private set; }

        public float WaypointThresholdDistance { get; private set; }

        public bool ResetStuckDetection =>
            this.IsActive;

        public bool CycleWaypoints
        {
            get => 
                this.m_cycleWaypoints;
            set => 
                (this.m_cycleWaypoints = value);
        }
    }
}

