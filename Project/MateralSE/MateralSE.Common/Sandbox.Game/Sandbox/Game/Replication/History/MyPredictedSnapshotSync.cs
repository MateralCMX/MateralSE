namespace Sandbox.Game.Replication.History
{
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game.Entity;
    using VRage.Game.Networking;
    using VRage.Library.Utils;
    using VRage.Profiler;
    using VRageMath;
    using VRageRender;

    public class MyPredictedSnapshotSync : IMySnapshotSync
    {
        public static float DELTA_FACTOR = 0.8f;
        public static int SMOOTH_ITERATIONS = 30;
        public static bool POSITION_CORRECTION = true;
        public static bool SMOOTH_POSITION_CORRECTION = true;
        public static float MIN_POSITION_DELTA = 0.005f;
        public static float MAX_POSITION_DELTA = 0.5f;
        public static bool ROTATION_CORRECTION = true;
        public static bool SMOOTH_ROTATION_CORRECTION = true;
        public static float MIN_ROTATION_ANGLE = 0.03490659f;
        public static float MAX_ROTATION_ANGLE = 0.1745329f;
        public static bool LINEAR_VELOCITY_CORRECTION = true;
        public static bool SMOOTH_LINEAR_VELOCITY_CORRECTION = true;
        public static float MIN_LINEAR_VELOCITY_DELTA = 0.01f;
        public static float MAX_LINEAR_VELOCITY_DELTA = 4f;
        public static bool ANGULAR_VELOCITY_CORRECTION = true;
        public static bool SMOOTH_ANGULAR_VELOCITY_CORRECTION = true;
        public static float MIN_ANGULAR_VELOCITY_DELTA = 0.01f;
        public static float MAX_ANGULAR_VELOCITY_DELTA = 0.5f;
        public static float MIN_VELOCITY_CHANGE_TO_RESET = 10f;
        public static bool SKIP_CORRECTIONS_FOR_DEBUG_ENTITY;
        public static float SMOOTH_DISTANCE = 150f;
        public static bool ApplyTrend = true;
        public static bool ForceAnimated = false;
        public readonly MyMovingAverage AverageCorrection = new MyMovingAverage(60, 0x3e8);
        private readonly MyEntity m_entity;
        private readonly MySnapshotHistory m_clientHistory = new MySnapshotHistory();
        private readonly MySnapshotHistory m_receivedQueue = new MySnapshotHistory();
        private readonly MySnapshotFlags m_currentFlags = new MySnapshotFlags();
        private bool m_inited;
        private ResetType m_wasReset = ResetType.Initial;
        private int m_animDeltaLinearVelocityIterations;
        private MyTimeSpan m_animDeltaLinearVelocityTimestamp;
        private Vector3 m_animDeltaLinearVelocity;
        private int m_animDeltaPositionIterations;
        private MyTimeSpan m_animDeltaPositionTimestamp;
        private Vector3D m_animDeltaPosition;
        private int m_animDeltaRotationIterations;
        private MyTimeSpan m_animDeltaRotationTimestamp;
        private Quaternion m_animDeltaRotation;
        private int m_animDeltaAngularVelocityIterations;
        private MyTimeSpan m_animDeltaAngularVelocityTimestamp;
        private Vector3 m_animDeltaAngularVelocity;
        private MyTimeSpan m_lastServerTimestamp;
        private MyTimeSpan m_trendStart;
        private Vector3 m_lastServerVelocity;
        private int m_stopSuspected;
        private MyTimeSpan m_debugLastClientTimestamp;
        private MySnapshot m_debugLastClientSnapshot;
        private MySnapshot m_debugLastServerSnapshot;
        private MySnapshot m_debugLastDelta;
        private static float TREND_TIMEOUT = 0.2f;

        public MyPredictedSnapshotSync(MyEntity entity)
        {
            this.m_entity = entity;
        }

        private void DebugDraw(ref MySnapshotHistory.MyItem serverItem, ref MySnapshot currentSnapshot, MyTimeSpan clientTimestamp, MyPredictedSnapshotSyncSetup setup)
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_NETWORK_SYNC)
            {
                MatrixD xd;
                bool flag;
                MatrixD xd3;
                serverItem.Snapshot.GetMatrix(this.m_entity, out xd, true, true);
                MyRenderProxy.DebugDrawAxis(xd, 1f, false, false, false);
                MatrixD worldMatrix = this.m_entity.WorldMatrix;
                MyRenderProxy.DebugDrawAxis(worldMatrix, 0.2f, false, false, false);
                Color? colorTo = null;
                MyRenderProxy.DebugDrawArrow3DDir(worldMatrix.Translation, serverItem.Snapshot.GetLinearVelocity(setup.ApplyPhysicsLocal), Color.White, colorTo, false, 0.1, null, 0.5f, false);
                double milliseconds = (serverItem.Timestamp - clientTimestamp).Milliseconds;
                float scale = (float) Math.Abs((double) (milliseconds / 32.0));
                MyRenderProxy.DebugDrawAABB(new BoundingBoxD(xd.Translation - Vector3.One, xd.Translation + Vector3.One), (milliseconds < 0.0) ? Color.Red : Color.Green, 1f, scale, false, false, false);
                MyEntity parent = MySnapshot.GetParent(this.m_entity, out flag);
                if (parent != null)
                {
                    colorTo = null;
                    MyRenderProxy.DebugDrawArrow3D(worldMatrix.Translation, parent.WorldMatrix.Translation, Color.Blue, colorTo, false, 0.1, null, 0.5f, false);
                }
                currentSnapshot.GetMatrix(this.m_entity, out xd3, true, false);
                colorTo = null;
                MyRenderProxy.DebugDrawArrow3D(this.m_entity.WorldMatrix.Translation, xd3.Translation, Color.Goldenrod, colorTo, false, 0.1, null, 0.5f, false);
            }
        }

        public void Destroy()
        {
            this.Reset(false);
        }

        public long GetParentId() => 
            (!this.m_receivedQueue.Empty() ? this.m_receivedQueue.GetLastParentId() : -1L);

        private bool InitPrediction(MyTimeSpan clientTimestamp, MySnapshotSyncSetup setup)
        {
            MySnapshotHistory.MyItem item;
            this.m_receivedQueue.GetItem(clientTimestamp, out item);
            if (item.Valid)
            {
                bool flag;
                MyEntity parent = MySnapshot.GetParent(this.m_entity, out flag);
                if (((parent == null) ? 0L : parent.EntityId) == item.Snapshot.ParentId)
                {
                    MySnapshotCache.Add(this.m_entity, ref item.Snapshot, setup, true);
                    this.m_inited = true;
                    return true;
                }
                this.m_inited = false;
            }
            return false;
        }

        public void Read(ref MySnapshot snapshot, MyTimeSpan timeStamp)
        {
            if ((this.m_entity.Parent == null) && (this.m_entity.Physics != null))
            {
                if ((this.m_entity.Physics.IsInWorld && ((this.m_entity.Physics.RigidBody != null) && !this.m_entity.Physics.RigidBody.IsActive)) && snapshot.Active)
                {
                    this.m_entity.Physics.RigidBody.Activate();
                }
                if (!this.m_receivedQueue.Empty())
                {
                    MySnapshotHistory.MyItem item;
                    this.m_receivedQueue.GetLast(out item, 0);
                    if ((snapshot.ParentId != item.Snapshot.ParentId) || (snapshot.InheritRotation != item.Snapshot.InheritRotation))
                    {
                        this.m_receivedQueue.Reset();
                    }
                }
                this.m_receivedQueue.Add(ref snapshot, timeStamp);
                this.m_receivedQueue.Prune(timeStamp, MyTimeSpan.Zero, 10);
            }
        }

        public void Reset(bool reinit = false)
        {
            int num;
            this.m_clientHistory.Reset();
            this.m_animDeltaAngularVelocityIterations = num = 0;
            this.m_animDeltaPositionIterations = num = num;
            this.m_animDeltaRotationIterations = this.m_animDeltaLinearVelocityIterations = num;
            this.m_lastServerVelocity = (Vector3) Vector3D.PositiveInfinity;
            this.m_wasReset = ResetType.Reset;
            this.m_trendStart = MyTimeSpan.FromSeconds(-1.0);
            if (reinit)
            {
                this.m_inited = false;
            }
        }

        public long Update(MyTimeSpan clientTimestamp, MySnapshotSyncSetup setup)
        {
            bool flag;
            if ((MyFakes.MULTIPLAYER_SKIP_PREDICTION_SUBGRIDS && (MySnapshot.GetParent(this.m_entity, out flag) != null)) || MyFakes.MULTIPLAYER_SKIP_PREDICTION)
            {
                this.Reset(false);
                this.m_receivedQueue.Reset();
                return -1L;
            }
            if (this.m_entity.InScene)
            {
                if (this.m_entity.Parent != null)
                {
                    return -1L;
                }
                if (this.m_entity.Physics == null)
                {
                    return -1L;
                }
                if (((this.m_entity.Physics.RigidBody != null) && !this.m_entity.Physics.RigidBody.IsActive) && !(setup as MyPredictedSnapshotSyncSetup).UpdateAlways)
                {
                    return -1L;
                }
                if ((MySnapshotCache.DEBUG_ENTITY_ID == this.m_entity.EntityId) && SKIP_CORRECTIONS_FOR_DEBUG_ENTITY)
                {
                    return -1L;
                }
                if (this.m_inited)
                {
                    this.UpdatePrediction(clientTimestamp, setup);
                }
                else
                {
                    this.InitPrediction(clientTimestamp, setup);
                }
            }
            return -1L;
        }

        private void UpdateForceStop(ref MySnapshot delta, ref MySnapshot currentSnapshot, ref MySnapshotHistory.MyItem serverItem, MyPredictedSnapshotSyncSetup setup)
        {
            if ((!this.m_lastServerVelocity.IsValid() || !setup.ApplyPhysicsLinear) || !setup.AllowForceStop)
            {
                this.m_lastServerVelocity = serverItem.Snapshot.LinearVelocity;
            }
            else
            {
                Vector3 vector = serverItem.Snapshot.LinearVelocity - this.m_lastServerVelocity;
                this.m_lastServerVelocity = serverItem.Snapshot.LinearVelocity;
                if (this.m_stopSuspected > 0)
                {
                    float num = (MIN_VELOCITY_CHANGE_TO_RESET / 2f) * (MIN_VELOCITY_CHANGE_TO_RESET / 2f);
                    if ((serverItem.Snapshot.LinearVelocity - currentSnapshot.LinearVelocity).LengthSquared() > num)
                    {
                        this.Reset(false);
                        this.m_wasReset = ResetType.ForceStop;
                        serverItem.Snapshot.Diff(ref currentSnapshot, out delta);
                        this.m_stopSuspected = 0;
                    }
                }
                if (vector.LengthSquared() > (MIN_VELOCITY_CHANGE_TO_RESET * MIN_VELOCITY_CHANGE_TO_RESET))
                {
                    this.m_stopSuspected = 10;
                    bool enableNetworkPositionTracking = MyCompilationSymbols.EnableNetworkPositionTracking;
                }
                else if (this.m_stopSuspected > 0)
                {
                    this.m_stopSuspected--;
                }
            }
        }

        private unsafe MySnapshotHistory.MyItem UpdateFromServerQueue(MyTimeSpan clientTimestamp, MyPredictedSnapshotSyncSetup setup, ref MySnapshot currentSnapshot, out MySnapshotHistory.MyItem serverItem)
        {
            this.m_currentFlags.Init(false);
            bool flag = false;
            this.m_receivedQueue.GetItem(clientTimestamp, out serverItem);
            if (!serverItem.Valid)
            {
                if (!this.m_receivedQueue.Empty())
                {
                    flag = true;
                }
            }
            else if (!(serverItem.Timestamp != this.m_lastServerTimestamp))
            {
                serverItem.Valid = false;
                flag = this.m_wasReset != ResetType.NoReset;
            }
            else
            {
                MySnapshotHistory.MyItem item;
                this.m_clientHistory.Get(serverItem.Timestamp, out item);
                if ((!item.Valid || (((item.Type != MySnapshotHistory.SnapshotType.Exact) && (item.Type != MySnapshotHistory.SnapshotType.Interpolation)) || (serverItem.Snapshot.ParentId != item.Snapshot.ParentId))) || (serverItem.Snapshot.InheritRotation != item.Snapshot.InheritRotation))
                {
                    if ((item.Type == MySnapshotHistory.SnapshotType.TooNew) && !item.Snapshot.Active)
                    {
                        this.Reset(true);
                    }
                    else
                    {
                        if ((!POSITION_CORRECTION || !item.Valid) || ((serverItem.Snapshot.ParentId == item.Snapshot.ParentId) && (serverItem.Snapshot.InheritRotation == item.Snapshot.InheritRotation)))
                        {
                            this.m_trendStart = MyTimeSpan.FromSeconds(-1.0);
                        }
                        else if (this.m_trendStart.Seconds < 0.0)
                        {
                            this.m_trendStart = clientTimestamp;
                        }
                        else if (((clientTimestamp - this.m_trendStart).Seconds > TREND_TIMEOUT) && ApplyTrend)
                        {
                            this.Reset(true);
                            serverItem.Valid = false;
                            return serverItem;
                        }
                        serverItem.Valid = false;
                        flag = this.m_wasReset != ResetType.NoReset;
                        if ((this.m_wasReset == ResetType.NoReset) && MyCompilationSymbols.EnableNetworkPositionTracking)
                        {
                        }
                    }
                }
                else
                {
                    MySnapshot snapshot;
                    this.m_lastServerTimestamp = serverItem.Timestamp;
                    if (this.UpdateTrend(setup, ref serverItem, ref item))
                    {
                        return serverItem;
                    }
                    if (serverItem.Snapshot.Active || setup.IsControlled)
                    {
                        serverItem.Snapshot.Diff(ref item.Snapshot, out snapshot);
                    }
                    else
                    {
                        serverItem.Snapshot.Diff(ref currentSnapshot, out snapshot);
                        if (!snapshot.CheckThresholds(0.0001f, 0.0001f, 0.0001f, 0.0001f))
                        {
                            serverItem.Valid = false;
                            return serverItem;
                        }
                        this.Reset(true);
                    }
                    this.m_debugLastDelta = snapshot;
                    this.UpdateForceStop(ref snapshot, ref currentSnapshot, ref serverItem, setup);
                    if (this.m_wasReset != ResetType.NoReset)
                    {
                        this.m_wasReset = ResetType.NoReset;
                        serverItem.Snapshot = snapshot;
                        serverItem.Type = MySnapshotHistory.SnapshotType.Reset;
                        return serverItem;
                    }
                    int num = (int) (SMOOTH_ITERATIONS * setup.IterationsFactor);
                    bool flag2 = false;
                    if (!setup.ApplyPosition || !POSITION_CORRECTION)
                    {
                        snapshot.Position = Vector3D.Zero;
                        this.m_animDeltaPositionIterations = 0;
                    }
                    else
                    {
                        float num2 = setup.MaxPositionFactor * setup.MaxPositionFactor;
                        double num3 = snapshot.Position.LengthSquared();
                        if (num3 > ((MAX_POSITION_DELTA * MAX_POSITION_DELTA) * num2))
                        {
                            Vector3D position = snapshot.Position;
                            snapshot.Position = position * (position.Normalize() - (MAX_POSITION_DELTA * (1f - DELTA_FACTOR)));
                            flag2 = true;
                            this.m_animDeltaPositionIterations = 0;
                            this.m_currentFlags.ApplyPosition = true;
                        }
                        else if (!SMOOTH_POSITION_CORRECTION || !setup.Smoothing)
                        {
                            this.m_animDeltaPositionIterations = 0;
                        }
                        else
                        {
                            float num5 = MIN_POSITION_DELTA * setup.MinPositionFactor;
                            if (num3 > (num5 * num5))
                            {
                                this.m_animDeltaPositionIterations = num;
                            }
                            if (this.m_animDeltaPositionIterations > 0)
                            {
                                this.m_animDeltaPosition = snapshot.Position / ((double) this.m_animDeltaPositionIterations);
                                this.m_animDeltaPositionTimestamp = serverItem.Timestamp;
                            }
                            snapshot.Position = Vector3D.Zero;
                        }
                    }
                    if (!setup.ApplyRotation || !ROTATION_CORRECTION)
                    {
                        snapshot.Rotation = Quaternion.Identity;
                        this.m_animDeltaRotationIterations = 0;
                    }
                    else
                    {
                        Vector3 vector;
                        float num6;
                        snapshot.Rotation.GetAxisAngle(out vector, out num6);
                        if (num6 > 3.141593f)
                        {
                            vector = -vector;
                            num6 = 6.283186f - num6;
                        }
                        if (num6 > (MAX_ROTATION_ANGLE * setup.MaxRotationFactor))
                        {
                            snapshot.Rotation = Quaternion.CreateFromAxisAngle(vector, num6 - (MAX_ROTATION_ANGLE * (1f - DELTA_FACTOR)));
                            snapshot.Rotation.Normalize();
                            flag2 = true;
                            this.m_animDeltaRotationIterations = 0;
                            this.m_currentFlags.ApplyRotation = true;
                        }
                        else if (!SMOOTH_ROTATION_CORRECTION || !setup.Smoothing)
                        {
                            this.m_animDeltaRotationIterations = 0;
                        }
                        else
                        {
                            if (num6 > MIN_ROTATION_ANGLE)
                            {
                                this.m_animDeltaRotationIterations = num;
                            }
                            if (this.m_animDeltaRotationIterations > 0)
                            {
                                this.m_animDeltaRotation = Quaternion.CreateFromAxisAngle(vector, num6 / ((float) this.m_animDeltaRotationIterations));
                                this.m_animDeltaRotationTimestamp = serverItem.Timestamp;
                            }
                            snapshot.Rotation = Quaternion.Identity;
                        }
                    }
                    if (!setup.ApplyPhysicsLinear || !LINEAR_VELOCITY_CORRECTION)
                    {
                        snapshot.LinearVelocity = Vector3.Zero;
                        this.m_animDeltaLinearVelocityIterations = 0;
                    }
                    else
                    {
                        float num7 = MIN_LINEAR_VELOCITY_DELTA * MIN_LINEAR_VELOCITY_DELTA;
                        float num8 = (setup.MinLinearFactor * setup.MinLinearFactor) * num7;
                        float num9 = snapshot.LinearVelocity.LengthSquared();
                        if ((serverItem.Snapshot.LinearVelocity.LengthSquared() == 0f) && (num9 < num7))
                        {
                            flag2 = true;
                            this.m_animDeltaLinearVelocityIterations = 0;
                            this.m_currentFlags.ApplyPhysicsLinear = true;
                        }
                        else if (SMOOTH_LINEAR_VELOCITY_CORRECTION && setup.Smoothing)
                        {
                            if (num9 > num8)
                            {
                                this.m_animDeltaLinearVelocityIterations = num;
                            }
                            if (this.m_animDeltaLinearVelocityIterations > 0)
                            {
                                this.m_animDeltaLinearVelocity = (snapshot.LinearVelocity * DELTA_FACTOR) / ((float) this.m_animDeltaLinearVelocityIterations);
                                this.m_animDeltaLinearVelocityTimestamp = serverItem.Timestamp;
                            }
                            snapshot.LinearVelocity = Vector3.Zero;
                        }
                        else if (num9 <= (((MAX_LINEAR_VELOCITY_DELTA * MAX_LINEAR_VELOCITY_DELTA) * setup.MaxLinearFactor) * setup.MaxLinearFactor))
                        {
                            this.m_animDeltaLinearVelocityIterations = 0;
                        }
                        else
                        {
                            Vector3* vectorPtr1 = (Vector3*) ref snapshot.LinearVelocity;
                            vectorPtr1[0] *= DELTA_FACTOR;
                            flag2 = true;
                            this.m_animDeltaLinearVelocityIterations = 0;
                            this.m_currentFlags.ApplyPhysicsLinear = true;
                        }
                    }
                    if (!setup.ApplyPhysicsAngular || !ANGULAR_VELOCITY_CORRECTION)
                    {
                        snapshot.AngularVelocity = Vector3.Zero;
                        this.m_animDeltaAngularVelocityIterations = 0;
                    }
                    else
                    {
                        float num10 = snapshot.AngularVelocity.LengthSquared();
                        if (num10 > (((MAX_ANGULAR_VELOCITY_DELTA * MAX_ANGULAR_VELOCITY_DELTA) * setup.MaxAngularFactor) * setup.MaxAngularFactor))
                        {
                            Vector3* vectorPtr2 = (Vector3*) ref snapshot.AngularVelocity;
                            vectorPtr2[0] *= DELTA_FACTOR;
                            flag2 = true;
                            this.m_currentFlags.ApplyPhysicsAngular = true;
                            this.m_animDeltaAngularVelocityIterations = 0;
                        }
                        else if (!SMOOTH_ANGULAR_VELOCITY_CORRECTION || !setup.Smoothing)
                        {
                            this.m_animDeltaAngularVelocityIterations = 0;
                        }
                        else
                        {
                            if (num10 > (((MIN_ANGULAR_VELOCITY_DELTA * MIN_ANGULAR_VELOCITY_DELTA) * setup.MinAngularFactor) * setup.MinAngularFactor))
                            {
                                this.m_animDeltaAngularVelocityIterations = num;
                            }
                            if (this.m_animDeltaAngularVelocityIterations > 0)
                            {
                                this.m_animDeltaAngularVelocity = (snapshot.AngularVelocity * DELTA_FACTOR) / ((float) this.m_animDeltaAngularVelocityIterations);
                                this.m_animDeltaAngularVelocityTimestamp = serverItem.Timestamp;
                            }
                            snapshot.AngularVelocity = Vector3.Zero;
                        }
                    }
                    if (MyCompilationSymbols.EnableNetworkPositionTracking & flag2)
                    {
                        long num1 = MySnapshotCache.DEBUG_ENTITY_ID;
                        long num11 = this.m_entity.EntityId;
                    }
                    serverItem.Snapshot = snapshot;
                    serverItem.Valid = flag2;
                }
            }
            if (!flag)
            {
                return serverItem;
            }
            else if (serverItem.Valid && (((serverItem.Type == MySnapshotHistory.SnapshotType.Exact) || (serverItem.Type == MySnapshotHistory.SnapshotType.Interpolation)) || (serverItem.Type == MySnapshotHistory.SnapshotType.Extrapolation)))
            {
                MySnapshot snapshot2;
                long num12 = this.m_entity.EntityId;
                long num13 = MySnapshotCache.DEBUG_ENTITY_ID;
                serverItem.Snapshot.Diff(ref currentSnapshot, out snapshot2);
                this.m_currentFlags.Init(setup);
                this.m_currentFlags.ApplyPhysicsLinear &= LINEAR_VELOCITY_CORRECTION;
                this.m_currentFlags.ApplyPhysicsAngular &= ANGULAR_VELOCITY_CORRECTION;
                serverItem.Valid = snapshot2.Active;
                serverItem.Snapshot = snapshot2;
                serverItem.Type = MySnapshotHistory.SnapshotType.Reset;
                this.m_debugLastDelta = snapshot2;
                return serverItem;
            }
            serverItem.Valid = false;
            long entityId = this.m_entity.EntityId;
            long num15 = MySnapshotCache.DEBUG_ENTITY_ID;
            return serverItem;
        }

        private unsafe bool UpdatePrediction(MyTimeSpan clientTimestamp, MySnapshotSyncSetup setup)
        {
            MySnapshotHistory.MyItem item;
            int num1;
            MyPredictedSnapshotSyncSetup snapshotFlags = setup as MyPredictedSnapshotSyncSetup;
            bool flag = (this.m_entity.WorldMatrix.Translation - MySector.MainCamera.Position).LengthSquared() < (SMOOTH_DISTANCE * SMOOTH_DISTANCE);
            if (!flag)
            {
                snapshotFlags = snapshotFlags.NotSmoothed;
            }
            if (!snapshotFlags.Smoothing)
            {
                int num2;
                this.m_animDeltaAngularVelocityIterations = num2 = 0;
                this.m_animDeltaRotationIterations = num2 = num2;
                this.m_animDeltaPositionIterations = this.m_animDeltaLinearVelocityIterations = num2;
            }
            MySnapshot snapshot = new MySnapshot(this.m_entity, setup.ApplyPhysicsLocal, setup.InheritRotation);
            MySnapshot snapshot2 = snapshot;
            if (!this.m_clientHistory.Empty())
            {
                MySnapshotHistory.MyItem item3;
                this.m_clientHistory.GetLast(out item3, 0);
                if ((snapshot.ParentId != item3.Snapshot.ParentId) || (snapshot.InheritRotation != item3.Snapshot.InheritRotation))
                {
                    MySnapshotHistory.MyItem item4;
                    this.Reset(false);
                    this.m_receivedQueue.GetLast(out item4, 0);
                    if ((item4.Snapshot.ParentId == snapshot.ParentId) && (item4.Snapshot.InheritRotation == snapshot.InheritRotation))
                    {
                        snapshot.LinearVelocity = item4.Snapshot.LinearVelocity;
                        MySnapshotCache.Add(this.m_entity, ref snapshot, snapshotFlags, true);
                        this.m_wasReset = ResetType.NoReset;
                    }
                }
            }
            this.m_clientHistory.Add(ref snapshot, clientTimestamp);
            MySnapshotHistory.MyItem item2 = this.UpdateFromServerQueue(clientTimestamp, snapshotFlags, ref snapshot, out item);
            float seconds = (float) (this.m_lastServerTimestamp - item.Timestamp).Seconds;
            bool flag2 = false;
            Vector3 zero = Vector3.Zero;
            bool flag3 = false;
            if (item2.Valid)
            {
                if ((item2.Snapshot.Position != Vector3D.Zero) || (item2.Snapshot.Rotation.W != 1f))
                {
                    flag2 = true;
                }
                snapshot.Add(ref item2.Snapshot);
                this.m_clientHistory.ApplyDelta(item2.Timestamp, ref item2.Snapshot);
                zero = (Vector3) item2.Snapshot.Position;
                flag3 = true;
            }
            if (((this.m_animDeltaPositionIterations > 0) || (this.m_animDeltaLinearVelocityIterations > 0)) || (this.m_animDeltaRotationIterations > 0))
            {
                num1 = 1;
            }
            else
            {
                num1 = (int) (this.m_animDeltaAngularVelocityIterations > 0);
            }
            if (num1 != 0)
            {
                if (this.m_animDeltaPositionIterations > 0)
                {
                    this.m_clientHistory.ApplyDeltaPosition(this.m_animDeltaPositionTimestamp, this.m_animDeltaPosition);
                    Vector3D* vectordPtr1 = (Vector3D*) ref snapshot.Position;
                    vectordPtr1[0] += this.m_animDeltaPosition;
                    this.m_animDeltaPositionIterations--;
                    this.m_currentFlags.ApplyPosition = true;
                    zero += this.m_animDeltaPosition;
                }
                if (this.m_animDeltaLinearVelocityIterations > 0)
                {
                    this.m_clientHistory.ApplyDeltaLinearVelocity(this.m_animDeltaLinearVelocityTimestamp, this.m_animDeltaLinearVelocity);
                    Vector3* vectorPtr1 = (Vector3*) ref snapshot.LinearVelocity;
                    vectorPtr1[0] += this.m_animDeltaLinearVelocity;
                    this.m_animDeltaLinearVelocityIterations--;
                    this.m_currentFlags.ApplyPhysicsLinear = true;
                }
                if (this.m_animDeltaAngularVelocityIterations > 0)
                {
                    this.m_clientHistory.ApplyDeltaAngularVelocity(this.m_animDeltaAngularVelocityTimestamp, this.m_animDeltaAngularVelocity);
                    Vector3* vectorPtr2 = (Vector3*) ref snapshot.AngularVelocity;
                    vectorPtr2[0] += this.m_animDeltaAngularVelocity;
                    this.m_animDeltaAngularVelocityIterations--;
                    this.m_currentFlags.ApplyPhysicsAngular = true;
                }
                if (this.m_animDeltaRotationIterations > 0)
                {
                    this.m_clientHistory.ApplyDeltaRotation(this.m_animDeltaRotationTimestamp, this.m_animDeltaRotation);
                    MySnapshot* snapshotPtr1 = (MySnapshot*) ref snapshot;
                    snapshotPtr1->Rotation = snapshot.Rotation * Quaternion.Inverse(this.m_animDeltaRotation);
                    snapshot.Rotation.Normalize();
                    this.m_animDeltaRotationIterations--;
                    this.m_currentFlags.ApplyRotation = true;
                }
                flag3 = true;
            }
            if (flag3)
            {
                this.DebugDraw(ref item, ref snapshot, clientTimestamp, snapshotFlags);
                this.m_currentFlags.ApplyPhysicsLocal = setup.ApplyPhysicsLocal;
                this.m_currentFlags.InheritRotation = setup.InheritRotation;
                bool reset = (item2.Type == MySnapshotHistory.SnapshotType.Reset) | flag2;
                MySnapshotCache.Add(this.m_entity, ref snapshot, this.m_currentFlags, reset);
            }
            this.AverageCorrection.Enqueue(zero.Length());
            if (MySnapshotCache.DEBUG_ENTITY_ID == this.m_entity.EntityId)
            {
                MyStatsGraph.ProfileAdvanced(true);
                MyStatsGraph.Begin("Prediction", 0x7fffffff, "UpdatePrediction", 0x14d, @"E:\Repo1\Sources\Sandbox.Game\Game\Replication\History\MyPredictedSnapshotSync.cs");
                MyStatsGraph.CustomTime("applySnapshot", flag3 ? 1f : 0.5f, "{0}", "UpdatePrediction", 0x14e, @"E:\Repo1\Sources\Sandbox.Game\Game\Replication\History\MyPredictedSnapshotSync.cs");
                MyStatsGraph.CustomTime("smoothing", flag ? 1f : 0.5f, "{0}", "UpdatePrediction", 0x14f, @"E:\Repo1\Sources\Sandbox.Game\Game\Replication\History\MyPredictedSnapshotSync.cs");
                if (snapshotFlags.ApplyPosition)
                {
                    MyStatsGraph.CustomTime("Pos", (float) this.m_debugLastDelta.Position.Length(), "{0}", "UpdatePrediction", 0x153, @"E:\Repo1\Sources\Sandbox.Game\Game\Replication\History\MyPredictedSnapshotSync.cs");
                }
                if (snapshotFlags.ApplyRotation)
                {
                    float num3;
                    Vector3 vector2;
                    this.m_debugLastDelta.Rotation.GetAxisAngle(out vector2, out num3);
                    MyStatsGraph.CustomTime("Rot", Math.Abs(num3), "{0}", "UpdatePrediction", 0x15b, @"E:\Repo1\Sources\Sandbox.Game\Game\Replication\History\MyPredictedSnapshotSync.cs");
                }
                if (snapshotFlags.ApplyPhysicsLinear)
                {
                    MyStatsGraph.CustomTime("linVel", this.m_debugLastDelta.LinearVelocity.Length(), "{0}", "UpdatePrediction", 0x15f, @"E:\Repo1\Sources\Sandbox.Game\Game\Replication\History\MyPredictedSnapshotSync.cs");
                }
                if (snapshotFlags.ApplyPhysicsAngular)
                {
                    MyStatsGraph.CustomTime("angVel", Math.Abs(this.m_debugLastDelta.AngularVelocity.Length()), "{0}", "UpdatePrediction", 0x161, @"E:\Repo1\Sources\Sandbox.Game\Game\Replication\History\MyPredictedSnapshotSync.cs");
                }
                float? bytesTransfered = null;
                MyStatsGraph.End(bytesTransfered, 0f, "", "{0} B", null, "UpdatePrediction", 0x163, @"E:\Repo1\Sources\Sandbox.Game\Game\Replication\History\MyPredictedSnapshotSync.cs");
                MyStatsGraph.ProfileAdvanced(false);
                if (flag3)
                {
                    if (!snapshotFlags.ApplyPosition)
                    {
                        if (!snapshotFlags.ApplyRotation)
                        {
                            bool applyPhysicsAngular = snapshotFlags.ApplyPhysicsAngular;
                        }
                    }
                    else
                    {
                        MySnapshot snapshot3;
                        Vector3D vectord1 = (item.Snapshot.Position - this.m_debugLastServerSnapshot.Position) / ((double) seconds);
                        this.m_debugLastServerSnapshot = item.Snapshot;
                        float num4 = (float) (this.m_debugLastClientTimestamp - clientTimestamp).Seconds;
                        Vector3D vectord2 = (snapshot.Position - this.m_debugLastClientSnapshot.Position) / ((double) num4);
                        this.m_debugLastClientSnapshot = snapshot;
                        this.m_debugLastClientTimestamp = clientTimestamp;
                        snapshot2.Diff(ref snapshot, out snapshot3);
                        snapshot3.Position.Length();
                        snapshot3.LinearVelocity.Length();
                        item2.Snapshot.Position.Length();
                        this.m_animDeltaPosition.Length();
                        MyCubeGrid entity = this.m_entity as MyCubeGrid;
                        if (entity != null)
                        {
                            MyPistonBase entityConnectingToParent = MyGridPhysicalHierarchy.Static.GetEntityConnectingToParent(entity) as MyPistonBase;
                            if (entityConnectingToParent != null)
                            {
                                float currentPosition = entityConnectingToParent.CurrentPosition;
                            }
                        }
                    }
                }
            }
            this.m_clientHistory.Prune(this.m_lastServerTimestamp, MyTimeSpan.Zero, 3);
            return flag3;
        }

        private bool UpdateTrend(MyPredictedSnapshotSyncSetup setup, ref MySnapshotHistory.MyItem serverItem, ref MySnapshotHistory.MyItem item)
        {
            if ((!setup.UserTrend || (this.m_receivedQueue.Count <= 1)) || !POSITION_CORRECTION)
            {
                this.m_trendStart = MyTimeSpan.FromSeconds(-1.0);
            }
            else
            {
                MySnapshotHistory.MyItem item2;
                MySnapshotHistory.MyItem item3;
                MySnapshotHistory.MyItem item4;
                this.m_receivedQueue.GetFirst(out item2);
                this.m_receivedQueue.GetLast(out item3, 0);
                Vector3 vector = Vector3.Sign((Vector3) ((item3.Snapshot.Position - item2.Snapshot.Position) / (item3.Timestamp.Seconds - item2.Timestamp.Seconds)), 1f);
                this.m_clientHistory.GetLast(out item4, 0);
                Vector3 vector2 = Vector3.Sign((Vector3) ((item4.Snapshot.Position - item.Snapshot.Position) / (item4.Timestamp.Seconds - item.Timestamp.Seconds)), 1f);
                if ((vector != Vector3.Zero) || (vector == vector2))
                {
                    this.m_trendStart = MyTimeSpan.FromSeconds(-1.0);
                }
                else if (this.m_trendStart.Seconds < 0.0)
                {
                    this.m_trendStart = item.Timestamp;
                }
                else if (((item.Timestamp - this.m_trendStart).Seconds > TREND_TIMEOUT) && ApplyTrend)
                {
                    this.Reset(true);
                    serverItem.Valid = false;
                    return true;
                }
            }
            return false;
        }

        public bool Inited =>
            this.m_inited;

        private enum ResetType
        {
            NoReset,
            Initial,
            Reset,
            ForceStop
        }
    }
}

