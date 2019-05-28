namespace Sandbox.Game.GameSystems
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication;
    using Sandbox.Game.World;
    using Sandbox.Game.World.Generator;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;

    [StaticEventOwner]
    public class MyGridJumpDriveSystem
    {
        public const float JUMP_DRIVE_DELAY = 10f;
        public const double MIN_JUMP_DISTANCE = 5000.0;
        private MyCubeGrid m_grid;
        private HashSet<MyJumpDrive> m_jumpDrives = new HashSet<MyJumpDrive>();
        private List<MyEntity> m_entitiesInRange = new List<MyEntity>();
        private List<MyObjectSeed> m_objectsInRange = new List<MyObjectSeed>();
        private List<BoundingBoxD> m_obstaclesInRange = new List<BoundingBoxD>();
        private List<MyCharacter> m_characters = new List<MyCharacter>();
        private Vector3D m_selectedDestination;
        private Vector3D m_jumpDirection;
        private Vector3D m_jumpDirectionNorm;
        private Vector3 m_effectOffset = Vector3.Zero;
        private bool m_isJumping;
        private float m_prevJumpTime;
        private bool m_jumped;
        private long m_userId;
        private float m_jumpTimeLeft;
        private bool m_playEffect;
        private Vector3D? m_savedJumpDirection;
        private float? m_savedRemainingJumpTime;
        private MySoundPair m_chargingSound = new MySoundPair("ShipJumpDriveCharging", true);
        private MySoundPair m_jumpInSound = new MySoundPair("ShipJumpDriveJumpIn", true);
        private MySoundPair m_jumpOutSound = new MySoundPair("ShipJumpDriveJumpOut", true);
        protected MyEntity3DSoundEmitter m_soundEmitter;
        private MyEntity3DSoundEmitter m_soundEmitterJumpIn;
        private MyParticleEffect m_effect;

        public MyGridJumpDriveSystem(MyCubeGrid grid)
        {
            this.m_grid = grid;
            this.m_soundEmitter = new MyEntity3DSoundEmitter(this.m_grid, false, 1f);
            this.m_soundEmitterJumpIn = new MyEntity3DSoundEmitter(this.m_grid, false, 1f);
        }

        public void AbortJump(MyJumpFailReason reason)
        {
            this.StopParticleEffect();
            this.m_soundEmitter.StopSound(true, true);
            this.m_soundEmitterJumpIn.StopSound(true, true);
            if (this.m_isJumping && this.IsLocalCharacterAffectedByJump(false))
            {
                this.ShowNotification(reason);
            }
            this.CleanupAfterJump();
        }

        public void AfterGridClose()
        {
            if (this.m_isJumping)
            {
                this.m_soundEmitter.StopSound(true, true);
                this.m_soundEmitterJumpIn.StopSound(true, true);
                this.CleanupAfterJump();
            }
        }

        public bool CheckReceivedCoordinates(ref Vector3D pos)
        {
            if (this.m_jumpTimeLeft > 1f)
            {
                return true;
            }
            if ((Vector3D.DistanceSquared(this.m_grid.PositionComp.GetPosition(), pos) <= 100000000.0) || !this.m_jumped)
            {
                return true;
            }
            MySandboxGame.Log.WriteLine($"Wrong position packet received, dist={Vector3D.Distance(this.m_grid.PositionComp.GetPosition(), pos)}, T={this.m_jumpTimeLeft})");
            return false;
        }

        private void CleanupAfterJump()
        {
            using (HashSet<MyJumpDrive>.Enumerator enumerator = this.m_jumpDrives.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.IsJumping = false;
                }
            }
            if (this.IsLocalCharacterAffectedByJump(false))
            {
                MySector.MainCamera.FieldOfView = MySandboxGame.Config.FieldOfView;
            }
            this.m_jumped = false;
            this.m_isJumping = false;
            this.m_effect = null;
        }

        private Vector3D ClosestPointOnBounds(BoundingBoxD b, Vector3D p)
        {
            Vector3D vectord = (p - b.Center) / b.HalfExtents;
            int num = vectord.AbsMaxComponent();
            if (num == 0)
            {
                p.X = (vectord.X <= 0.0) ? b.Min.X : b.Max.X;
            }
            else if (num == 1)
            {
                p.Y = (vectord.Y <= 0.0) ? b.Min.Y : b.Max.Y;
            }
            else if (num == 2)
            {
                p.Z = (vectord.Z <= 0.0) ? b.Min.Z : b.Max.Z;
            }
            return p;
        }

        private void DepleteJumpDrives(double distance, long userId)
        {
            double currentMass = this.m_grid.GetCurrentMass();
            foreach (MyJumpDrive drive in this.m_jumpDrives)
            {
                if (drive.CanJumpAndHasAccess(userId))
                {
                    drive.IsJumping = true;
                    double num2 = drive.BlockDefinition.MaxJumpMass / currentMass;
                    if (num2 > 1.0)
                    {
                        num2 = 1.0;
                    }
                    double num3 = drive.BlockDefinition.MaxJumpDistance * num2;
                    if (num3 >= distance)
                    {
                        drive.SetStoredPower(1f - ((float) (distance / num3)));
                        break;
                    }
                    distance -= num3;
                    drive.SetStoredPower(0f);
                }
            }
        }

        private unsafe Vector3D? FindSuitableJumpLocation(Vector3D desiredLocation)
        {
            BoundingBoxD physicalGroupAABB = this.m_grid.GetPhysicalGroupAABB();
            physicalGroupAABB.Inflate((double) 1000.0);
            BoundingBoxD inflated = ((BoundingBoxD*) ref physicalGroupAABB).GetInflated(physicalGroupAABB.HalfExtents * 10.0);
            ((BoundingBoxD*) ref inflated).Translate(desiredLocation - inflated.Center);
            MyProceduralWorldGenerator.Static.OverlapAllPlanetSeedsInSphere(new BoundingSphereD(inflated.Center, inflated.HalfExtents.AbsMax()), this.m_objectsInRange);
            Vector3D point = desiredLocation;
            foreach (MyObjectSeed seed in this.m_objectsInRange)
            {
                BoundingBoxD boundingVolume = seed.BoundingVolume;
                if (boundingVolume.Contains(point) != ContainmentType.Disjoint)
                {
                    Vector3D vectord3 = point - seed.BoundingVolume.Center;
                    vectord3.Normalize();
                    vectord3 *= seed.BoundingVolume.HalfExtents * 1.5;
                    point = seed.BoundingVolume.Center + vectord3;
                    break;
                }
            }
            this.m_objectsInRange.Clear();
            MyProceduralWorldGenerator.Static.OverlapAllAsteroidSeedsInSphere(new BoundingSphereD(inflated.Center, inflated.HalfExtents.AbsMax()), this.m_objectsInRange);
            foreach (MyObjectSeed seed2 in this.m_objectsInRange)
            {
                this.m_obstaclesInRange.Add(seed2.BoundingVolume);
            }
            this.m_objectsInRange.Clear();
            MyGamePruningStructure.GetTopMostEntitiesInBox(ref inflated, this.m_entitiesInRange, MyEntityQueryType.Both);
            foreach (MyEntity entity in this.m_entitiesInRange)
            {
                if (!(entity is MyPlanet))
                {
                    this.m_obstaclesInRange.Add(entity.PositionComp.WorldAABB.GetInflated(physicalGroupAABB.HalfExtents));
                }
            }
            int num = 10;
            int num2 = 0;
            BoundingBoxD? nullable = null;
            bool flag = false;
            bool flag2 = false;
            while (true)
            {
                if (num2 < num)
                {
                    num2++;
                    flag = false;
                    foreach (BoundingBoxD xd4 in this.m_obstaclesInRange)
                    {
                        ContainmentType type = xd4.Contains(point);
                        if ((type == ContainmentType.Contains) || (type == ContainmentType.Intersects))
                        {
                            if (nullable == null)
                            {
                                nullable = new BoundingBoxD?(xd4);
                            }
                            nullable = new BoundingBoxD?(new BoundingBoxD?(nullable.Value.Include(xd4)).Value.Inflate((double) 1.0));
                            point = this.ClosestPointOnBounds(nullable.Value, point);
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        continue;
                    }
                    flag2 = true;
                }
                this.m_obstaclesInRange.Clear();
                this.m_entitiesInRange.Clear();
                this.m_objectsInRange.Clear();
                if (flag2)
                {
                    return new Vector3D?(point);
                }
                return null;
            }
        }

        private void GetCharactersInBoundingBox(BoundingBoxD boundingBox, List<MyCharacter> characters)
        {
            MyGamePruningStructure.GetAllEntitiesInBox(ref boundingBox, this.m_entitiesInRange, MyEntityQueryType.Both);
            foreach (MyCharacter character in this.m_entitiesInRange)
            {
                if (character != null)
                {
                    characters.Add(character);
                }
            }
            this.m_entitiesInRange.Clear();
        }

        private StringBuilder GetConfirmationText(string name, double distance, double actualDistance, long userId, bool obstacleDetected)
        {
            float num3;
            int count = this.m_jumpDrives.Count;
            int num2 = this.m_jumpDrives.Count<MyJumpDrive>(x => x.CanJumpAndHasAccess(userId));
            distance /= 1000.0;
            actualDistance /= 1000.0;
            if (((float) (actualDistance / distance)) > 1f)
            {
                num3 = 1f;
            }
            this.GetCharactersInBoundingBox(this.m_grid.GetPhysicalGroupAABB(), this.m_characters);
            int num4 = 0;
            int num5 = 0;
            foreach (MyCharacter character in this.m_characters)
            {
                if (character.IsDead)
                {
                    continue;
                }
                num4++;
                if (character.Parent != null)
                {
                    num5++;
                }
            }
            this.m_characters.Clear();
            string str = obstacleDetected ? MyTexts.Get(MySpaceTexts.Jump_Obstacle).ToString() : "";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Jump_Destination)).Append(name).Append("\n");
            stringBuilder.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Jump_Distance)).Append(distance.ToString("N")).Append(" km\n");
            stringBuilder.Append(MyTexts.Get(MySpaceTexts.Jump_Achievable).ToString() + str + ": ").Append(num3.ToString("P")).Append(" (").Append(actualDistance.ToString("N")).Append(" km)\n");
            stringBuilder.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Jump_Weight)).Append(MyHud.ShipInfo.Mass.ToString("N")).Append(" kg\n");
            stringBuilder.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Jump_DriveCount)).Append(num2).Append("/").Append(count).Append("\n");
            stringBuilder.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Jump_CrewCount)).Append(num5).Append("/").Append(num4).Append("\n");
            return stringBuilder;
        }

        public Vector3D? GetJumpDriveDirection()
        {
            if (this.m_isJumping && !this.m_jumped)
            {
                return new Vector3D?(this.m_jumpDirection);
            }
            return null;
        }

        public double GetMaxJumpDistance(long userId)
        {
            double num = 0.0;
            double num2 = 0.0;
            double currentMass = this.m_grid.GetCurrentMass();
            foreach (MyJumpDrive drive in this.m_jumpDrives)
            {
                if (drive.CanJumpAndHasAccess(userId))
                {
                    num += drive.BlockDefinition.MaxJumpDistance;
                    num2 += drive.BlockDefinition.MaxJumpDistance * (drive.BlockDefinition.MaxJumpMass / currentMass);
                }
            }
            return Math.Min(num, num2);
        }

        internal float? GetRemainingJumpTime()
        {
            if (this.m_isJumping && !this.m_jumped)
            {
                return new float?(this.m_jumpTimeLeft);
            }
            return null;
        }

        private StringBuilder GetWarningText(double actualDistance, bool obstacleDetected)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (obstacleDetected)
            {
                stringBuilder.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Jump_ObstacleTruncation));
            }
            stringBuilder.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Jump_DistanceToDest)).Append(actualDistance.ToString("N")).Append(" m\n");
            stringBuilder.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Jump_MinDistance)).Append(5000.0.ToString("N")).Append(" m\n");
            return stringBuilder;
        }

        public void Init(Vector3D? jumpDriveDirection, float? remainingTimeForJump)
        {
            this.m_savedJumpDirection = jumpDriveDirection;
            this.m_savedRemainingJumpTime = remainingTimeForJump;
        }

        private bool IsJumpValid(long userId, out MyJumpFailReason reason)
        {
            reason = MyJumpFailReason.None;
            if (MyFakes.TESTING_JUMPDRIVE)
            {
                return true;
            }
            if (this.m_grid.MarkedForClose)
            {
                reason = MyJumpFailReason.Other;
                return false;
            }
            if (this.m_grid.CanBeTeleported(this, out reason))
            {
                if (this.GetMaxJumpDistance(userId) >= 5000.0)
                {
                    return true;
                }
                reason = MyJumpFailReason.ShortDistance;
            }
            return false;
        }

        private bool IsLocalCharacterAffectedByJump(bool forceRecompute = false)
        {
            if ((MySession.Static.LocalCharacter == null) || !(MySession.Static.ControlledEntity is MyShipController))
            {
                this.m_playEffect = false;
                MySector.MainCamera.FieldOfView = MySandboxGame.Config.FieldOfView;
                return false;
            }
            if (this.m_playEffect && !forceRecompute)
            {
                return true;
            }
            this.GetCharactersInBoundingBox(this.m_grid.GetPhysicalGroupAABB(), this.m_characters);
            using (List<MyCharacter>.Enumerator enumerator = this.m_characters.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyCharacter current = enumerator.Current;
                    if (ReferenceEquals(current, MySession.Static.LocalCharacter) && (current.Parent != null))
                    {
                        this.m_characters.Clear();
                        this.m_playEffect = true;
                        return true;
                    }
                }
            }
            this.m_characters.Clear();
            this.m_playEffect = false;
            return false;
        }

        private void Jump(Vector3D jumpTarget, long userId)
        {
            double maxJumpDistance = this.GetMaxJumpDistance(userId);
            this.m_jumpDirection = jumpTarget - this.m_grid.WorldMatrix.Translation;
            Vector3D.Normalize(ref this.m_jumpDirection, out this.m_jumpDirectionNorm);
            double num2 = this.m_jumpDirection.Length();
            if (num2 > maxJumpDistance)
            {
                double num3 = maxJumpDistance / num2;
                this.m_jumpDirection *= num3;
            }
            this.m_selectedDestination = this.m_grid.WorldMatrix.Translation + this.m_jumpDirection;
            this.m_isJumping = true;
            this.m_jumped = false;
            this.m_jumpTimeLeft = MyFakes.TESTING_JUMPDRIVE ? 1f : 10f;
            this.m_grid.GridSystems.JumpSystem.m_jumpTimeLeft = this.m_jumpTimeLeft;
            bool? nullable = null;
            this.m_soundEmitter.PlaySound(this.m_chargingSound, false, false, false, false, false, nullable);
            this.m_prevJumpTime = 0f;
            this.m_userId = userId;
            this.m_grid.MarkForUpdate();
        }

        [Event(null, 0x405), Reliable, Server, Broadcast]
        private static void OnAbortJump(long entityId)
        {
            MyCubeGrid grid;
            MyEntities.TryGetEntityById<MyCubeGrid>(entityId, out grid, false);
            if (grid == null)
            {
                if (Sync.IsServer && !MyEventContext.Current.IsLocallyInvoked)
                {
                    (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
                    MyEventContext.ValidationFailed();
                }
            }
            else
            {
                MyExternalReplicable replicable = MyExternalReplicable.FindByObject(grid);
                if (Sync.IsServer && !MyEventContext.Current.IsLocallyInvoked)
                {
                    ValidationResult passed = ValidationResult.Passed;
                    if (replicable != null)
                    {
                        passed = replicable.HasRights(new EndpointId(MyEventContext.Current.Sender.Value), ValidationType.Controlled);
                    }
                    if (passed != ValidationResult.Passed)
                    {
                        (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, passed.HasFlag(ValidationResult.Kick), null, true);
                        MyEventContext.ValidationFailed();
                        return;
                    }
                }
                grid.GridSystems.JumpSystem.AbortJump(MyJumpFailReason.None);
            }
        }

        [Event(null, 0x3e4), Reliable, Server, Broadcast]
        private static void OnJumpFailure(long entityId, MyJumpFailReason reason)
        {
            MyCubeGrid grid;
            MyEntities.TryGetEntityById<MyCubeGrid>(entityId, out grid, false);
            MyCubeGrid grid1 = grid;
        }

        [Event(null, 0x3c2), Reliable, Server]
        private static void OnJumpRequested(long entityId, Vector3D jumpTarget, long userId)
        {
            MyCubeGrid grid;
            MyEntities.TryGetEntityById<MyCubeGrid>(entityId, out grid, false);
            if (grid != null)
            {
                grid.GridSystems.JumpSystem.OnRequestJumpFromClient(jumpTarget, userId);
            }
        }

        [Event(null, 0x3d3), Reliable, ServerInvoked, Broadcast]
        private static void OnJumpSuccess(long entityId, Vector3D jumpTarget, long userId)
        {
            MyCubeGrid grid;
            MyEntities.TryGetEntityById<MyCubeGrid>(entityId, out grid, false);
            if (grid != null)
            {
                grid.GridSystems.JumpSystem.Jump(jumpTarget, userId);
            }
        }

        [Event(null, 0x3f5), Reliable, Broadcast]
        private static void OnPerformJump(long entityId, Vector3D jumpTarget)
        {
            MyCubeGrid grid;
            MyEntities.TryGetEntityById<MyCubeGrid>(entityId, out grid, false);
            if (grid != null)
            {
                grid.GridSystems.JumpSystem.PerformJump(jumpTarget);
            }
        }

        private void OnRequestJumpFromClient(Vector3D jumpTarget, long userId)
        {
            MyJumpFailReason reason;
            if (!this.IsJumpValid(userId, out reason))
            {
                this.SendJumpFailure(reason);
            }
            else
            {
                this.m_jumpDirection = jumpTarget - this.m_grid.WorldMatrix.Translation;
                Vector3D.Normalize(ref this.m_jumpDirection, out this.m_jumpDirectionNorm);
                double maxJumpDistance = this.GetMaxJumpDistance(userId);
                double num2 = (jumpTarget - this.m_grid.WorldMatrix.Translation).Length();
                double num3 = num2;
                if (num2 > maxJumpDistance)
                {
                    double num4 = maxJumpDistance / num2;
                    num3 = maxJumpDistance;
                    this.m_jumpDirection *= num4;
                }
                jumpTarget = this.m_grid.WorldMatrix.Translation + this.m_jumpDirection;
                if (num3 < 4800.0)
                {
                    this.SendJumpFailure(MyJumpFailReason.ShortDistance);
                }
                else
                {
                    Vector3D? nullable = this.FindSuitableJumpLocation(jumpTarget);
                    if (nullable == null)
                    {
                        this.SendJumpFailure(MyJumpFailReason.NoLocation);
                    }
                    else
                    {
                        this.SendJumpSuccess(nullable.Value, userId);
                    }
                }
            }
        }

        private void PerformJump(Vector3D jumpTarget)
        {
            Vector2? nullable;
            double? nullable2;
            this.m_jumpDirection = jumpTarget - this.m_grid.WorldMatrix.Translation;
            Vector3D.Normalize(ref this.m_jumpDirection, out this.m_jumpDirectionNorm);
            this.DepleteJumpDrives(this.m_jumpDirection.Length(), this.m_userId);
            bool flag = false;
            if (this.IsLocalCharacterAffectedByJump(false))
            {
                flag = true;
            }
            if (flag)
            {
                nullable = null;
                MyThirdPersonSpectator.Static.ResetViewerAngle(nullable);
                nullable2 = null;
                MyThirdPersonSpectator.Static.ResetViewerDistance(nullable2);
                MyThirdPersonSpectator.Static.RecalibrateCameraPosition(false);
            }
            this.m_jumped = true;
            MatrixD worldMatrix = this.m_grid.WorldMatrix;
            worldMatrix.Translation = this.m_grid.WorldMatrix.Translation + this.m_jumpDirection;
            this.m_grid.Teleport(worldMatrix, null, false);
            if (flag)
            {
                nullable = null;
                MyThirdPersonSpectator.Static.ResetViewerAngle(nullable);
                nullable2 = null;
                MyThirdPersonSpectator.Static.ResetViewerDistance(nullable2);
                MyThirdPersonSpectator.Static.RecalibrateCameraPosition(false);
            }
        }

        private void PlayParticleEffect()
        {
            if (this.m_effect == null)
            {
                MatrixD worldMatrix = MatrixD.CreateFromDir(-this.m_jumpDirectionNorm);
                this.m_effectOffset = (Vector3) ((this.m_jumpDirectionNorm * this.m_grid.PositionComp.WorldAABB.HalfExtents.AbsMax()) * 2.0);
                worldMatrix.Translation = this.m_grid.PositionComp.WorldAABB.Center + this.m_effectOffset;
                MyParticlesManager.TryCreateParticleEffect("Warp", worldMatrix, out this.m_effect);
            }
        }

        public void RegisterJumpDrive(MyJumpDrive jumpDrive)
        {
            this.m_jumpDrives.Add(jumpDrive);
        }

        public void RequestAbort()
        {
            if (this.m_isJumping && !this.m_jumped)
            {
                this.SendAbortJump();
            }
        }

        private void RequestJump(Vector3D jumpTarget, long userId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, Vector3D, long>(s => new Action<long, Vector3D, long>(MyGridJumpDriveSystem.OnJumpRequested), this.m_grid.EntityId, jumpTarget, userId, targetEndpoint, position);
            if (MyVisualScriptLogicProvider.GridJumped != null)
            {
                MyVisualScriptLogicProvider.GridJumped(userId, this.m_grid.Name, this.m_grid.EntityId);
            }
        }

        public void RequestJump(string destinationName, Vector3D destination, long userId)
        {
            if (!Vector3.IsZero(MyGravityProviderSystem.CalculateNaturalGravityInPoint(this.m_grid.WorldMatrix.Translation)))
            {
                MyHudNotification notification = new MyHudNotification(MySpaceTexts.NotificationCannotJumpFromGravity, 0x5dc, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                MyHud.Notifications.Add(notification);
            }
            else if (!Vector3.IsZero(MyGravityProviderSystem.CalculateNaturalGravityInPoint(destination)))
            {
                MyHudNotification notification = new MyHudNotification(MySpaceTexts.NotificationCannotJumpIntoGravity, 0x5dc, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                MyHud.Notifications.Add(notification);
            }
            else
            {
                MyJumpFailReason reason;
                if (!this.IsJumpValid(userId, out reason))
                {
                    this.ShowNotification(reason);
                }
                else if ((MySession.Static.Settings.WorldSizeKm > 0) && (destination.Length() > (MySession.Static.Settings.WorldSizeKm * 500)))
                {
                    MyHudNotification notification = new MyHudNotification(MySpaceTexts.NotificationCannotJumpOutsideWorld, 0x5dc, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                    MyHud.Notifications.Add(notification);
                }
                else
                {
                    MyStringId? nullable2;
                    this.m_selectedDestination = destination;
                    double maxJumpDistance = this.GetMaxJumpDistance(userId);
                    this.m_jumpDirection = destination - this.m_grid.WorldMatrix.Translation;
                    Vector3D.Normalize(ref this.m_jumpDirection, out this.m_jumpDirectionNorm);
                    double distance = this.m_jumpDirection.Length();
                    double actualDistance = distance;
                    if (distance > maxJumpDistance)
                    {
                        double num4 = maxJumpDistance / distance;
                        actualDistance = maxJumpDistance;
                        this.m_jumpDirection *= num4;
                    }
                    Vector3D vectord = Vector3D.Normalize(destination - this.m_grid.WorldMatrix.Translation);
                    Vector3D from = this.m_grid.WorldMatrix.Translation + (this.m_grid.PositionComp.LocalAABB.Extents.Max() * vectord);
                    LineD line = new LineD(from, destination);
                    MyIntersectionResultLineTriangleEx? nullable = MyEntities.GetIntersectionWithLine(ref line, this.m_grid, null, true, true, true, IntersectionFlags.ALL_TRIANGLES, 0f, false);
                    if (nullable != null)
                    {
                        MyEntity entity = nullable.Value.Entity as MyEntity;
                        Vector3D translation = entity.WorldMatrix.Translation;
                        Vector3D vectord4 = MyUtils.GetClosestPointOnLine(ref from, ref destination, ref translation);
                        if (nullable.Value.Entity is MyPlanet)
                        {
                            MyHudNotification notification = new MyHudNotification(MySpaceTexts.NotificationCannotJumpIntoGravity, 0x5dc, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                            MyHud.Notifications.Add(notification);
                            return;
                        }
                        destination = vectord4 - (vectord * (entity.PositionComp.LocalAABB.Extents.Length() + this.m_grid.PositionComp.LocalAABB.HalfExtents.Length()));
                        this.m_selectedDestination = destination;
                        this.m_jumpDirection = this.m_selectedDestination - from;
                        Vector3D.Normalize(ref this.m_jumpDirection, out this.m_jumpDirectionNorm);
                        actualDistance = this.m_jumpDirection.Length();
                    }
                    if (actualDistance >= 5000.0)
                    {
                        nullable2 = null;
                        nullable2 = null;
                        nullable2 = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, this.GetConfirmationText(destinationName, distance, actualDistance, userId, nullable != null), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), nullable2, nullable2, nullable2, nullable2, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                            reason = MyJumpFailReason.None;
                            if ((result != MyGuiScreenMessageBox.ResultEnum.YES) || !this.IsJumpValid(userId, out reason))
                            {
                                this.SendAbortJump();
                            }
                            else
                            {
                                this.RequestJump(this.m_selectedDestination, userId);
                            }
                        }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, new Vector2(0.839375f, 0.3675f)));
                    }
                    else
                    {
                        nullable2 = null;
                        nullable2 = null;
                        nullable2 = null;
                        nullable2 = null;
                        Vector2? size = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, this.GetWarningText(actualDistance, nullable != null), MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), nullable2, nullable2, nullable2, nullable2, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                    }
                    if (MyFakes.TESTING_JUMPDRIVE)
                    {
                        this.m_jumpDirection *= 1000.0;
                    }
                }
            }
        }

        private void SendAbortJump()
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MyGridJumpDriveSystem.OnAbortJump), this.m_grid.EntityId, targetEndpoint, position);
        }

        private void SendJumpFailure(MyJumpFailReason reason)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, MyJumpFailReason>(s => new Action<long, MyJumpFailReason>(MyGridJumpDriveSystem.OnJumpFailure), this.m_grid.EntityId, reason, targetEndpoint, position);
        }

        private void SendJumpSuccess(Vector3D jumpTarget, long userId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, Vector3D, long>(s => new Action<long, Vector3D, long>(MyGridJumpDriveSystem.OnJumpSuccess), this.m_grid.EntityId, jumpTarget, userId, targetEndpoint, position);
        }

        private void SendPerformJump(Vector3D jumpTarget)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, Vector3D>(s => new Action<long, Vector3D>(MyGridJumpDriveSystem.OnPerformJump), this.m_grid.EntityId, jumpTarget, targetEndpoint, position);
        }

        private void ShowNotification(MyJumpFailReason reason)
        {
            if (!Sync.IsDedicated)
            {
                switch (reason)
                {
                    case MyJumpFailReason.Static:
                    {
                        MyHudNotification notification = new MyHudNotification(MySpaceTexts.NotificationJumpAbortedStatic, 0x5dc, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                        MyHud.Notifications.Add(notification);
                        return;
                    }
                    case MyJumpFailReason.Locked:
                    {
                        MyHudNotification notification2 = new MyHudNotification(MySpaceTexts.NotificationJumpAbortedLocked, 0x5dc, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                        MyHud.Notifications.Add(notification2);
                        return;
                    }
                    case MyJumpFailReason.ShortDistance:
                    {
                        MyHudNotification notification4 = new MyHudNotification(MySpaceTexts.NotificationJumpAbortedShortDistance, 0x5dc, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                        MyHud.Notifications.Add(notification4);
                        return;
                    }
                    case MyJumpFailReason.AlreadyJumping:
                    {
                        MyHudNotification notification5 = new MyHudNotification(MySpaceTexts.NotificationJumpAbortedAlreadyJumping, 0x5dc, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                        MyHud.Notifications.Add(notification5);
                        return;
                    }
                    case MyJumpFailReason.NoLocation:
                    {
                        MyHudNotification notification3 = new MyHudNotification(MySpaceTexts.NotificationJumpAbortedNoLocation, 0x5dc, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                        MyHud.Notifications.Add(notification3);
                        return;
                    }
                    case MyJumpFailReason.Other:
                    {
                        MyHudNotification notification6 = new MyHudNotification(MySpaceTexts.NotificationJumpAborted, 0x5dc, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important);
                        MyHud.Notifications.Add(notification6);
                        return;
                    }
                }
            }
        }

        private void StopParticleEffect()
        {
            if (this.m_effect != null)
            {
                this.m_effect.StopEmitting(10f);
                this.m_effect = null;
            }
        }

        public void UnregisterJumpDrive(MyJumpDrive jumpDrive)
        {
            this.m_jumpDrives.Remove(jumpDrive);
            MySector.MainCamera.FieldOfView = MySandboxGame.Config.FieldOfView;
        }

        public void UpdateBeforeSimulation()
        {
            if (this.m_savedJumpDirection != null)
            {
                this.m_selectedDestination = this.m_savedJumpDirection.Value;
                this.m_isJumping = true;
                this.m_jumped = false;
                this.m_jumpTimeLeft = (this.m_savedRemainingJumpTime != null) ? this.m_savedRemainingJumpTime.Value : 0f;
                this.m_savedJumpDirection = null;
                this.m_savedRemainingJumpTime = null;
            }
            this.UpdateJumpDriveSystem();
        }

        private void UpdateJumpDriveSystem()
        {
            if (this.m_isJumping)
            {
                float jumpTimeLeft = this.m_jumpTimeLeft;
                if (this.m_effect == null)
                {
                    this.PlayParticleEffect();
                }
                else
                {
                    this.UpdateParticleEffect();
                }
                this.m_jumpTimeLeft -= 0.01666667f;
                if (jumpTimeLeft > 0.4f)
                {
                    double num2 = Math.Round((double) jumpTimeLeft);
                    if ((num2 != this.m_prevJumpTime) && this.IsLocalCharacterAffectedByJump(true))
                    {
                        MyHudNotification notification = new MyHudNotification(MySpaceTexts.NotificationJumpWarmupTime, 500, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 3, MyNotificationLevel.Normal);
                        object[] arguments = new object[] { num2 };
                        notification.SetTextFormatArguments(arguments);
                        MyHud.Notifications.Add(notification);
                    }
                }
                else
                {
                    bool? nullable;
                    if (jumpTimeLeft > 0f)
                    {
                        this.IsLocalCharacterAffectedByJump(true);
                        if ((this.m_soundEmitter.SoundId != this.m_jumpOutSound.Arcade) && (this.m_soundEmitter.SoundId != this.m_jumpOutSound.Realistic))
                        {
                            nullable = null;
                            this.m_soundEmitter.PlaySound(this.m_jumpOutSound, false, false, false, false, false, nullable);
                        }
                        this.UpdateJumpEffect(jumpTimeLeft / 0.4f);
                        if (jumpTimeLeft < 0.3f)
                        {
                        }
                    }
                    else if (this.m_jumped)
                    {
                        if (jumpTimeLeft <= -0.6f)
                        {
                            this.CleanupAfterJump();
                        }
                        else
                        {
                            if (!this.m_soundEmitterJumpIn.IsPlaying)
                            {
                                nullable = null;
                                this.m_soundEmitterJumpIn.PlaySound(this.m_jumpInSound, false, false, false, false, false, nullable);
                            }
                            this.UpdateJumpEffect(jumpTimeLeft / -0.6f);
                        }
                    }
                    else if (Sync.IsServer)
                    {
                        Vector3D? nullable2 = this.FindSuitableJumpLocation(this.m_selectedDestination);
                        double maxJumpDistance = this.GetMaxJumpDistance(this.m_userId);
                        MyJumpFailReason none = MyJumpFailReason.None;
                        if (((nullable2 == null) || (this.m_jumpDirection.Length() > maxJumpDistance)) || !this.IsJumpValid(this.m_userId, out none))
                        {
                            this.SendAbortJump();
                        }
                        else
                        {
                            this.SendPerformJump(nullable2.Value);
                            this.PerformJump(nullable2.Value);
                        }
                    }
                }
                this.m_prevJumpTime = (float) Math.Round((double) jumpTimeLeft);
            }
        }

        private void UpdateJumpEffect(float t)
        {
            if (this.m_playEffect)
            {
                float num = MathHelper.ToRadians((float) 170f);
                float num2 = MathHelper.SmoothStep(MySandboxGame.Config.FieldOfView, num, 1f - t);
                MySector.MainCamera.FieldOfView = num2;
            }
        }

        private void UpdateParticleEffect()
        {
            if (this.m_effect != null)
            {
                MatrixD worldMatrix = this.m_effect.WorldMatrix;
                worldMatrix.Translation = this.m_grid.PositionComp.WorldAABB.Center + this.m_effectOffset;
                this.m_effect.WorldMatrix = worldMatrix;
            }
        }

        public bool NeedsPerFrameUpdate =>
            ((this.m_savedJumpDirection != null) || this.m_isJumping);

        public bool IsJumping =>
            this.m_isJumping;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGridJumpDriveSystem.<>c <>9 = new MyGridJumpDriveSystem.<>c();
            public static Func<IMyEventOwner, Action<long, Vector3D, long>> <>9__60_0;
            public static Func<IMyEventOwner, Action<long, Vector3D, long>> <>9__62_0;
            public static Func<IMyEventOwner, Action<long, MyGridJumpDriveSystem.MyJumpFailReason>> <>9__64_0;
            public static Func<IMyEventOwner, Action<long, Vector3D>> <>9__66_0;
            public static Func<IMyEventOwner, Action<long>> <>9__68_0;

            internal Action<long, Vector3D, long> <RequestJump>b__60_0(IMyEventOwner s) => 
                new Action<long, Vector3D, long>(MyGridJumpDriveSystem.OnJumpRequested);

            internal Action<long> <SendAbortJump>b__68_0(IMyEventOwner s) => 
                new Action<long>(MyGridJumpDriveSystem.OnAbortJump);

            internal Action<long, MyGridJumpDriveSystem.MyJumpFailReason> <SendJumpFailure>b__64_0(IMyEventOwner s) => 
                new Action<long, MyGridJumpDriveSystem.MyJumpFailReason>(MyGridJumpDriveSystem.OnJumpFailure);

            internal Action<long, Vector3D, long> <SendJumpSuccess>b__62_0(IMyEventOwner s) => 
                new Action<long, Vector3D, long>(MyGridJumpDriveSystem.OnJumpSuccess);

            internal Action<long, Vector3D> <SendPerformJump>b__66_0(IMyEventOwner s) => 
                new Action<long, Vector3D>(MyGridJumpDriveSystem.OnPerformJump);
        }

        public enum MyJumpFailReason
        {
            None,
            Static,
            Locked,
            ShortDistance,
            AlreadyJumping,
            NoLocation,
            Other
        }
    }
}

