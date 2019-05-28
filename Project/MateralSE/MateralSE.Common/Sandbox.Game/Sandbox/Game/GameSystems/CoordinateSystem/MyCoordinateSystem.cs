namespace Sandbox.Game.GameSystems.CoordinateSystem
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Components.Session;
    using VRage.Game.Definitions.SessionComponents;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Network;
    using VRage.Serialization;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 0x3e8, typeof(MyObjectBuilder_CoordinateSystem), (Type) null), StaticEventOwner]
    public class MyCoordinateSystem : MySessionComponentBase
    {
        [CompilerGenerated]
        private static Action OnCoordinateChange;
        public static MyCoordinateSystem Static;
        private double m_angleTolerance = 0.0001;
        private double m_positionTolerance = 0.001;
        private int m_coorsSystemSize = 0x3e8;
        private int m_coorsSystemSizeSq = 0xf4240;
        private Dictionary<long, MyLocalCoordSys> m_localCoordSystems = new Dictionary<long, MyLocalCoordSys>();
        private long m_lastCoordSysId = 1L;
        private bool m_drawBoundingBox;
        private long m_selectedCoordSys;
        private long m_lastSelectedCoordSys;
        private bool m_localCoordExist;
        private bool m_selectionChanged;
        private bool m_visible;

        public static  event Action OnCoordinateChange
        {
            [CompilerGenerated] add
            {
                Action onCoordinateChange = OnCoordinateChange;
                while (true)
                {
                    Action a = onCoordinateChange;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onCoordinateChange = Interlocked.CompareExchange<Action>(ref OnCoordinateChange, action3, a);
                    if (ReferenceEquals(onCoordinateChange, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onCoordinateChange = OnCoordinateChange;
                while (true)
                {
                    Action source = onCoordinateChange;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onCoordinateChange = Interlocked.CompareExchange<Action>(ref OnCoordinateChange, action3, source);
                    if (ReferenceEquals(onCoordinateChange, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyCoordinateSystem()
        {
            Static = this;
            if (Sync.IsServer)
            {
                MyEntities.OnEntityAdd += new Action<MyEntity>(this.MyEntities_OnEntityCreate);
            }
        }

        [Event(null, 0xf1), Reliable, BroadcastExcept]
        private static void CoordSysCreated_Client(MyCreateCoordSysBuffer createBuffer)
        {
            MyTransformD transform = new MyTransformD {
                Position = createBuffer.Position,
                Rotation = createBuffer.Rotation
            };
            Static.CreateCoordSys_ClientInternal(ref transform, createBuffer.Id);
        }

        [Event(null, 0x143), Reliable, Broadcast]
        private static void CoorSysRemoved_Client(long coordSysId)
        {
            Static.RemoveCoordSys(coordSysId);
        }

        public unsafe void CreateCoordSys(MyCubeGrid cubeGrid, bool staticGridAlignToCenter, bool sync = false)
        {
            MyTransformD origin = new MyTransformD(cubeGrid.PositionComp.WorldMatrix);
            origin.Rotation.Normalize();
            float gridSize = cubeGrid.GridSize;
            if (!staticGridAlignToCenter)
            {
                Vector3D* vectordPtr1 = (Vector3D*) ref origin.Position;
                vectordPtr1[0] -= (((origin.Rotation.Forward + origin.Rotation.Right) + origin.Rotation.Up) * gridSize) * 0.5f;
            }
            MyLocalCoordSys sys = new MyLocalCoordSys(origin, this.m_coorsSystemSize);
            while (true)
            {
                long lastCoordSysId = this.m_lastCoordSysId;
                this.m_lastCoordSysId = lastCoordSysId + 1L;
                long key = lastCoordSysId;
                if (!this.m_localCoordSystems.ContainsKey(key))
                {
                    sys.Id = key;
                    this.m_localCoordSystems.Add(key, sys);
                    if (cubeGrid.LocalCoordSystem != 0)
                    {
                        this.UnregisterCubeGrid(cubeGrid);
                    }
                    this.RegisterCubeGrid(cubeGrid, sys);
                    MyCreateCoordSysBuffer buffer = new MyCreateCoordSysBuffer {
                        Position = origin.Position,
                        Rotation = origin.Rotation,
                        Id = key
                    };
                    if (sync)
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        Vector3D? position = null;
                        MyMultiplayer.RaiseStaticEvent<MyCreateCoordSysBuffer>(x => new Action<MyCreateCoordSysBuffer>(MyCoordinateSystem.CoordSysCreated_Client), buffer, targetEndpoint, position);
                    }
                    return;
                }
            }
        }

        private void CreateCoordSys_ClientInternal(ref MyTransformD transform, long coordSysId)
        {
            MyLocalCoordSys sys = new MyLocalCoordSys(transform, this.m_coorsSystemSize) {
                Id = coordSysId
            };
            this.m_localCoordSystems.Add(coordSysId, sys);
        }

        private void CubeGrid_OnClose(MyEntity obj)
        {
            MyCubeGrid cubeGrid = obj as MyCubeGrid;
            if (cubeGrid != null)
            {
                this.UnregisterCubeGrid(cubeGrid);
            }
        }

        private void CubeGrid_OnPhysicsChanged(MyEntity obj)
        {
            MyCubeGrid cubeGrid = obj as MyCubeGrid;
            if ((cubeGrid != null) && !cubeGrid.IsStatic)
            {
                this.UnregisterCubeGrid(cubeGrid);
            }
        }

        public override void Draw()
        {
            if (!this.m_visible)
            {
                return;
            }
            if (this.m_selectedCoordSys == 0)
            {
                this.m_drawBoundingBox = false;
            }
            else if (this.m_selectedCoordSys != 0)
            {
                this.m_drawBoundingBox = true;
            }
            if (MyFakes.ENABLE_DEBUG_DRAW_COORD_SYS)
            {
                using (Dictionary<long, MyLocalCoordSys>.ValueCollection.Enumerator enumerator = this.m_localCoordSystems.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Draw();
                    }
                    goto TR_0001;
                }
            }
            if (this.m_drawBoundingBox)
            {
                MyLocalCoordSys coordSysById = this.GetCoordSysById(this.m_selectedCoordSys);
                if (coordSysById != null)
                {
                    coordSysById.Draw();
                }
            }
        TR_0001:
            base.Draw();
        }

        private MyLocalCoordSys GetClosestCoordSys(ref Vector3D position, bool checkContain = true)
        {
            MyLocalCoordSys sys = null;
            double maxValue = double.MaxValue;
            foreach (MyLocalCoordSys sys2 in this.m_localCoordSystems.Values)
            {
                if (!checkContain || sys2.Contains(ref position))
                {
                    double num2 = (sys2.Origin.Position - position).LengthSquared();
                    if (num2 < maxValue)
                    {
                        sys = sys2;
                        maxValue = num2;
                    }
                }
            }
            return sys;
        }

        private MyLocalCoordSys GetCoordSysById(long id) => 
            (!this.m_localCoordSystems.ContainsKey(id) ? null : this.m_localCoordSystems[id]);

        public Color GetCoordSysColor(long coordSysId) => 
            (!this.m_localCoordSystems.ContainsKey(coordSysId) ? Color.White : this.m_localCoordSystems[coordSysId].RenderColor);

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            MyObjectBuilder_CoordinateSystem objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_CoordinateSystem;
            objectBuilder.LastCoordSysId = this.m_lastCoordSysId;
            foreach (KeyValuePair<long, MyLocalCoordSys> pair in this.m_localCoordSystems)
            {
                MyObjectBuilder_CoordinateSystem.CoordSysInfo item = new MyObjectBuilder_CoordinateSystem.CoordSysInfo {
                    Id = pair.Value.Id,
                    EntityCount = pair.Value.EntityCounter,
                    Position = pair.Value.Origin.Position,
                    Rotation = pair.Value.Origin.Rotation
                };
                objectBuilder.CoordSystems.Add(item);
            }
            return objectBuilder;
        }

        public static void GetPosRoundedToGrid(ref Vector3D vecToRound, double gridSize, bool isStaticGridAlignToCenter)
        {
            if (isStaticGridAlignToCenter)
            {
                vecToRound = (Vector3D) (Vector3L.Round(vecToRound / gridSize) * gridSize);
            }
            else
            {
                vecToRound = (Vector3D) ((Vector3L.Round((vecToRound / gridSize) + 0.5) * gridSize) - (0.5 * gridSize));
            }
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            MyObjectBuilder_CoordinateSystem system = sessionComponent as MyObjectBuilder_CoordinateSystem;
            this.m_lastCoordSysId = system.LastCoordSysId;
            foreach (MyObjectBuilder_CoordinateSystem.CoordSysInfo info in system.CoordSystems)
            {
                MyTransformD origin = new MyTransformD {
                    Position = (Vector3D) info.Position,
                    Rotation = (Quaternion) info.Rotation
                };
                MyLocalCoordSys sys = new MyLocalCoordSys(origin, this.m_coorsSystemSize) {
                    Id = info.Id
                };
                this.m_localCoordSystems.Add(info.Id, sys);
            }
        }

        public override void InitFromDefinition(MySessionComponentDefinition definition)
        {
            base.InitFromDefinition(definition);
            MyCoordinateSystemDefinition definition2 = definition as MyCoordinateSystemDefinition;
            MyCoordinateSystemDefinition definition1 = definition2;
            this.m_coorsSystemSize = definition2.CoordSystemSize;
            this.m_coorsSystemSizeSq = this.m_coorsSystemSize * this.m_coorsSystemSize;
            this.m_angleTolerance = definition2.AngleTolerance;
            this.m_positionTolerance = definition2.PositionTolerance;
        }

        public bool IsAnyLocalCoordSysExist(ref Vector3D worldPos)
        {
            using (Dictionary<long, MyLocalCoordSys>.ValueCollection.Enumerator enumerator = this.m_localCoordSystems.Values.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (enumerator.Current.Contains(ref worldPos))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsLocalCoordSysExist(ref MatrixD tranform, double gridSize)
        {
            using (Dictionary<long, MyLocalCoordSys>.ValueCollection.Enumerator enumerator = this.m_localCoordSystems.Values.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyLocalCoordSys current = enumerator.Current;
                    Vector3D translation = tranform.Translation;
                    if (current.Contains(ref translation))
                    {
                        double num = Math.Abs(Vector3D.Dot(current.Origin.Rotation.Forward, tranform.Forward));
                        double num2 = Math.Abs(Vector3D.Dot(current.Origin.Rotation.Up, tranform.Up));
                        if (((num < this.m_angleTolerance) || (num > (1.0 - this.m_angleTolerance))) && ((num2 < this.m_angleTolerance) || (num2 > (1.0 - this.m_angleTolerance))))
                        {
                            double num3 = gridSize / 2.0;
                            Vector3D vectord1 = Vector3D.Transform(translation - current.Origin.Position, Quaternion.Inverse(current.Origin.Rotation));
                            double num4 = Math.Abs((double) (vectord1.X % num3));
                            double num5 = Math.Abs((double) (vectord1.Y % num3));
                            double num6 = Math.Abs((double) (vectord1.Z % num3));
                            if ((((num4 < this.m_positionTolerance) || (num4 > (num3 - this.m_positionTolerance))) && ((num5 < this.m_positionTolerance) || (num5 > (num3 - this.m_positionTolerance)))) && ((num6 < this.m_positionTolerance) || (num6 > (num3 - this.m_positionTolerance))))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public override void LoadData()
        {
            base.LoadData();
        }

        private void MyEntities_OnEntityCreate(MyEntity obj)
        {
            MyCubeGrid cubeGrid = obj as MyCubeGrid;
            if ((cubeGrid != null) && (cubeGrid.LocalCoordSystem != 0))
            {
                MyLocalCoordSys coordSysById = this.GetCoordSysById(cubeGrid.LocalCoordSystem);
                if (coordSysById != null)
                {
                    this.RegisterCubeGrid(cubeGrid, coordSysById);
                }
            }
        }

        public void RegisterCubeGrid(MyCubeGrid cubeGrid)
        {
            if (cubeGrid.LocalCoordSystem == 0)
            {
                Vector3D position = cubeGrid.PositionComp.GetPosition();
                MyLocalCoordSys closestCoordSys = this.GetClosestCoordSys(ref position, true);
                if (closestCoordSys != null)
                {
                    this.RegisterCubeGrid(cubeGrid, closestCoordSys);
                }
            }
        }

        private void RegisterCubeGrid(MyCubeGrid cubeGrid, MyLocalCoordSys coordSys)
        {
            cubeGrid.OnClose += new Action<MyEntity>(this.CubeGrid_OnClose);
            cubeGrid.OnPhysicsChanged += new Action<MyEntity>(this.CubeGrid_OnPhysicsChanged);
            cubeGrid.LocalCoordSystem = coordSys.Id;
            coordSys.EntityCounter += 1L;
        }

        private void RemoveCoordSys(long coordSysId)
        {
            this.m_localCoordSystems.Remove(coordSysId);
        }

        public void ResetSelection()
        {
            this.m_lastSelectedCoordSys = 0L;
            this.m_selectedCoordSys = 0L;
            this.m_drawBoundingBox = false;
        }

        public CoordSystemData SnapWorldPosToClosestGrid(ref Vector3D worldPos, double gridSize, bool staticGridAlignToCenter, long? id = new long?())
        {
            MyLocalCoordSys closestCoordSys;
            this.m_lastSelectedCoordSys = this.m_selectedCoordSys;
            if ((id == null) || (id.Value == 0))
            {
                closestCoordSys = this.GetClosestCoordSys(ref worldPos, true);
            }
            else
            {
                closestCoordSys = this.GetCoordSysById(id.Value);
            }
            MyLocalCoordSys sys = closestCoordSys;
            if (sys != null)
            {
                this.m_selectedCoordSys = sys.Id;
            }
            else
            {
                sys = new MyLocalCoordSys(new MyTransformD((Vector3D) (Vector3L.Round(worldPos / gridSize) * gridSize)), this.m_coorsSystemSize);
                this.m_selectedCoordSys = 0L;
            }
            this.m_localCoordExist = this.m_selectedCoordSys != 0;
            if (this.m_selectedCoordSys == this.m_lastSelectedCoordSys)
            {
                this.m_selectionChanged = false;
            }
            else
            {
                this.m_selectionChanged = true;
                if (OnCoordinateChange != null)
                {
                    OnCoordinateChange();
                }
            }
            CoordSystemData data = new CoordSystemData();
            Quaternion rotation = sys.Origin.Rotation;
            Vector3D position = sys.Origin.Position;
            Vector3D vecToRound = Vector3D.Transform(worldPos - position, Quaternion.Inverse(rotation));
            GetPosRoundedToGrid(ref vecToRound, gridSize, staticGridAlignToCenter);
            data.Id = this.m_selectedCoordSys;
            data.LocalSnappedPos = vecToRound;
            MyTransformD md = new MyTransformD {
                Position = position + Vector3D.Transform(vecToRound, rotation),
                Rotation = rotation
            };
            data.SnappedTransform = md;
            data.Origin = sys.Origin;
            return data;
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            this.m_lastCoordSysId = 1L;
            this.m_localCoordSystems.Clear();
            this.m_drawBoundingBox = false;
            this.m_selectedCoordSys = 0L;
            this.m_lastSelectedCoordSys = 0L;
        }

        private void UnregisterCubeGrid(MyCubeGrid cubeGrid)
        {
            cubeGrid.OnClose -= new Action<MyEntity>(this.CubeGrid_OnClose);
            cubeGrid.OnPhysicsChanged -= new Action<MyEntity>(this.CubeGrid_OnPhysicsChanged);
            long localCoordSystem = cubeGrid.LocalCoordSystem;
            MyLocalCoordSys coordSysById = this.GetCoordSysById(localCoordSystem);
            cubeGrid.LocalCoordSystem = 0L;
            if (coordSysById != null)
            {
                coordSysById.EntityCounter -= 1L;
                if (coordSysById.EntityCounter <= 0L)
                {
                    this.RemoveCoordSys(coordSysById.Id);
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<long>(x => new Action<long>(MyCoordinateSystem.CoorSysRemoved_Client), localCoordSystem, targetEndpoint, position);
                }
            }
        }

        public long SelectedCoordSys =>
            this.m_selectedCoordSys;

        public long LastSelectedCoordSys =>
            this.m_lastSelectedCoordSys;

        public bool LocalCoordExist =>
            this.m_localCoordExist;

        public bool Visible
        {
            get => 
                this.m_visible;
            set => 
                (this.m_visible = value);
        }

        public int CoordSystemSize =>
            this.m_coorsSystemSize;

        public int CoordSystemSizeSquared =>
            this.m_coorsSystemSizeSq;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCoordinateSystem.<>c <>9 = new MyCoordinateSystem.<>c();
            public static Func<IMyEventOwner, Action<MyCoordinateSystem.MyCreateCoordSysBuffer>> <>9__40_0;
            public static Func<IMyEventOwner, Action<long>> <>9__47_0;

            internal Action<MyCoordinateSystem.MyCreateCoordSysBuffer> <CreateCoordSys>b__40_0(IMyEventOwner x) => 
                new Action<MyCoordinateSystem.MyCreateCoordSysBuffer>(MyCoordinateSystem.CoordSysCreated_Client);

            internal Action<long> <UnregisterCubeGrid>b__47_0(IMyEventOwner x) => 
                new Action<long>(MyCoordinateSystem.CoorSysRemoved_Client);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CoordSystemData
        {
            public long Id;
            public MyTransformD SnappedTransform;
            public MyTransformD Origin;
            public Vector3D LocalSnappedPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyCreateCoordSysBuffer
        {
            public long Id;
            public Vector3D Position;
            [Serialize(MyPrimitiveFlags.Normalized)]
            public Quaternion Rotation;
        }
    }
}

