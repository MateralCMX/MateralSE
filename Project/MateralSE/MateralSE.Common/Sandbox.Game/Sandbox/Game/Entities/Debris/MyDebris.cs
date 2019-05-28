namespace Sandbox.Game.Entities.Debris
{
    using Havok;
    using ParallelTasks;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
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
    using VRage.Game.Models;
    using VRage.Utils;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MyDebris : MySessionComponentBase
    {
        private static MyDebris m_static;
        private List<Vector3D> m_positionBuffer;
        private List<Vector3> m_voxelDebrisOffsets;
        private static string[] m_debrisModels = (from x in MyDefinitionManager.Static.GetDebrisDefinitions()
            where x.Type == MyDebrisType.Model
            select x.Model).ToArray<string>();
        private static MyDebrisDefinition[] m_debrisVoxels = (from x in MyDefinitionManager.Static.GetDebrisDefinitions()
            where x.Type == MyDebrisType.Voxel
            orderby x.MinAmount descending
            select x).ToArray<MyDebrisDefinition>();
        public static readonly float VoxelDebrisModelVolume = 0.15f;
        private MyConcurrentDictionary<MyModelShapeInfo, HkShape> m_shapes = new MyConcurrentDictionary<MyModelShapeInfo, HkShape>(0, null);
        private const int MaxDebrisCount = 0x21;
        private int m_debrisCount;
        private Queue<DebrisCreationInfo> m_creationBuffer = new Queue<DebrisCreationInfo>(0x21);
        private MyDebrisBaseDescription m_desc = new MyDebrisBaseDescription();
        private int m_debrisModelIndex;

        public MyEntity CreateDebris(string model)
        {
            if (!MyFakes.ENABLE_DEBRIS)
            {
                return null;
            }
            this.m_desc.Model = model;
            MyDebrisBase base1 = new MyDebrisBase();
            base1.Debris.Init(this.m_desc);
            Interlocked.Increment(ref this.m_debrisCount);
            this.m_desc.LifespanMinInMiliseconds = 0xfa0;
            this.m_desc.LifespanMaxInMiliseconds = 0x1b58;
            return base1;
        }

        public void CreateDirectedDebris(Vector3 sourceWorldPosition, Vector3 offsetDirection, float minSourceDistance, float maxSourceDistance, float minDeviationAngle, float maxDeviationAngle, int debrisPieces, float initialSpeed)
        {
            MyCreateDebrisWork work = MyCreateDebrisWork.Create();
            work.sourceWorldPosition = sourceWorldPosition;
            work.offsetDirection = offsetDirection;
            work.minSourceDistance = minSourceDistance;
            work.maxSourceDistance = maxSourceDistance;
            work.minDeviationAngle = minDeviationAngle;
            work.maxDeviationAngle = maxDeviationAngle;
            work.debrisPieces = debrisPieces;
            work.initialSpeed = initialSpeed;
            work.Context = this;
            Parallel.Start(work, work.CompletionCallback);
        }

        public void CreateDirectedDebris(Vector3 sourceWorldPosition, Vector3 offsetDirection, float minSourceDistance, float maxSourceDistance, float minDeviationAngle, float maxDeviationAngle, int debrisPieces, float initialSpeed, float maxAmount, MyVoxelMaterialDefinition material)
        {
            for (int i = 0; i < debrisPieces; i++)
            {
                float randomFloat = MyUtils.GetRandomFloat(minSourceDistance, maxSourceDistance);
                float radians = MyUtils.GetRandomFloat(minDeviationAngle, maxDeviationAngle);
                Matrix matrix = Matrix.CreateRotationX(MyUtils.GetRandomFloat(minDeviationAngle, maxDeviationAngle)) * Matrix.CreateRotationY(radians);
                Vector3 vector = Vector3.Transform(offsetDirection, matrix);
                Vector3 vector2 = sourceWorldPosition + (vector * randomFloat);
                Vector3 vector3 = vector * initialSpeed;
                DebrisCreationInfo info = new DebrisCreationInfo {
                    Type = DebrisType.Voxel,
                    Position = vector2,
                    Velocity = vector3,
                    Material = material,
                    Ammount = maxAmount
                };
                this.EnqueueDebrisCreation(info);
            }
        }

        public void CreateExplosionDebris(ref BoundingSphereD explosionSphere, MyEntity entity)
        {
            BoundingBoxD worldAABB = entity.PositionComp.WorldAABB;
            this.CreateExplosionDebris(ref explosionSphere, entity, ref worldAABB, 1f, true);
        }

        public unsafe void CreateExplosionDebris(ref BoundingSphereD explosionSphere, float voxelsCountInPercent, MyVoxelMaterialDefinition voxelMaterial, MyVoxelBase voxelMap)
        {
            MatrixD matrix = MatrixD.CreateRotationX((double) MyUtils.GetRandomRadian()) * MatrixD.CreateRotationY((double) MyUtils.GetRandomRadian());
            int count = this.m_voxelDebrisOffsets.Count;
            int num = this.m_voxelDebrisOffsets.Count;
            int num2 = 0;
            while (true)
            {
                if (num2 < num)
                {
                    MyDebrisVoxel voxel = this.CreateVoxelDebris(((float) explosionSphere.Radius) * 100f, ((float) explosionSphere.Radius) * 1000f);
                    if (voxel != null)
                    {
                        Vector3D result = (this.m_voxelDebrisOffsets[num2] * ((float) explosionSphere.Radius)) * 0.5780347f;
                        Vector3D* vectordPtr1 = (Vector3D*) ref result;
                        Vector3D.Transform(ref (Vector3D) ref vectordPtr1, ref matrix, out result);
                        result += explosionSphere.Center;
                        Vector3 vector = MyUtils.GetRandomVector3Normalized();
                        if (vector != Vector3.Zero)
                        {
                            (voxel.Debris as MyDebrisVoxel.MyDebrisVoxelLogic).Start(result, vector * MyUtils.GetRandomFloat(4f, 8f), voxelMaterial);
                        }
                        num2++;
                        continue;
                    }
                }
                return;
            }
        }

        public void CreateExplosionDebris(ref BoundingSphereD explosionSphere, MyEntity entity, ref BoundingBoxD bb, float scaleMultiplier = 1f, bool applyVelocity = true)
        {
            MyUtils.GetRandomVector3Normalized();
            MyUtils.GetRandomFloat(0f, (float) explosionSphere.Radius);
            this.GeneratePositions(bb, this.m_positionBuffer);
            Vector3 vector = (entity.Physics != null) ? entity.Physics.LinearVelocity : Vector3.Zero;
            foreach (Vector3D vectord in this.m_positionBuffer)
            {
                DebrisCreationInfo info = new DebrisCreationInfo {
                    Type = DebrisType.Random,
                    Position = vectord,
                    Velocity = applyVelocity ? ((MyUtils.GetRandomVector3Normalized() * MyUtils.GetRandomFloat(4f, 8f)) + vector) : Vector3.Zero
                };
                this.EnqueueDebrisCreation(info);
            }
        }

        private MyDebrisBase CreateRandomDebris()
        {
            MyDebrisBase base2 = null;
            if (this.m_debrisModelIndex < m_debrisModels.Length)
            {
                int debrisModelIndex = this.m_debrisModelIndex;
                if (debrisModelIndex > m_debrisModels.Length)
                {
                    this.m_debrisModelIndex = debrisModelIndex = debrisModelIndex % m_debrisModels.Length;
                }
                base2 = (MyDebrisBase) this.CreateDebris(m_debrisModels[debrisModelIndex]);
                this.m_debrisModelIndex++;
            }
            return base2;
        }

        private HkShape CreateShape(MyModel model, HkShapeType shapeType, float scale)
        {
            if ((model.HavokCollisionShapes != null) && (model.HavokCollisionShapes.Length != 0))
            {
                HkShape shape;
                if (model.HavokCollisionShapes.Length != 1)
                {
                    shape = (HkShape) new HkListShape(model.HavokCollisionShapes, HkReferencePolicy.None);
                }
                else
                {
                    shape = model.HavokCollisionShapes[0];
                    shape.AddReference();
                }
                return shape;
            }
            if (shapeType == HkShapeType.Sphere)
            {
                return (HkShape) new HkSphereShape(scale * model.BoundingSphere.Radius);
            }
            if (shapeType == HkShapeType.Box)
            {
                return (HkShape) new HkBoxShape(Vector3.Max((Vector3) (((scale * (model.BoundingBox.Max - model.BoundingBox.Min)) / 2f) - 0.05f), new Vector3(0.025f)), 0.02f);
            }
            if (shapeType != HkShapeType.ConvexVertices)
            {
                throw new InvalidOperationException("This shape is not supported");
            }
            List<Vector3> list = new List<Vector3>();
            for (int i = 0; i < model.GetVerticesCount(); i++)
            {
                list.Add((Vector3) (scale * model.GetVertex(i)));
            }
            return (HkShape) new HkConvexVerticesShape(list.GetInternalArray<Vector3>(), list.Count, true, 0.1f);
        }

        public MyEntity CreateTreeDebris(string model)
        {
            this.m_desc.Model = model;
            MyDebrisTree tree1 = new MyDebrisTree();
            tree1.Debris.Init(this.m_desc);
            Interlocked.Increment(ref this.m_debrisCount);
            this.m_desc.LifespanMinInMiliseconds = 0xfa0;
            this.m_desc.LifespanMaxInMiliseconds = 0x1b58;
            return tree1;
        }

        private MyDebrisVoxel CreateVoxelDebris(float minAmount, float maxAmount)
        {
            this.m_desc.Model = GetAnyAmountLessDebrisVoxel(minAmount, maxAmount);
            MyDebrisVoxel voxel1 = new MyDebrisVoxel();
            voxel1.Debris.Init(this.m_desc);
            Interlocked.Increment(ref this.m_debrisCount);
            return voxel1;
        }

        private void EnqueueDebrisCreation(DebrisCreationInfo info)
        {
            while (this.m_creationBuffer.Count >= 0x21)
            {
                this.m_creationBuffer.Dequeue();
            }
            if (MyFakes.ENABLE_DEBRIS)
            {
                this.m_creationBuffer.Enqueue(info);
            }
        }

        private void GeneratePositions(BoundingBoxD boundingBox, List<Vector3D> positionBuffer)
        {
            positionBuffer.Clear();
            Vector3D vectord = boundingBox.Max - boundingBox.Min;
            double num = (vectord.X * vectord.Y) * vectord.Z;
            int num2 = 0x18;
            if (num < 1.0)
            {
                num2 = 1;
            }
            else if (num < 10.0)
            {
                num2 = 12;
            }
            else if (num > 100.0)
            {
                num2 = 0x30;
            }
            Vector3D vectord1 = vectord * Math.Pow(((double) num2) / num, 0.3333333432674408);
            int num4 = (int) Math.Ceiling(vectord1.X);
            int num5 = (int) Math.Ceiling(vectord1.Y);
            int num6 = (int) Math.Ceiling(vectord1.Z);
            Vector3D vectord2 = new Vector3D(vectord.X / ((double) num4), vectord.Y / ((double) num5), vectord.Z / ((double) num6));
            Vector3D vectord3 = boundingBox.Min + (0.5 * vectord2);
            int num7 = 0;
            while (num7 < num4)
            {
                int num8 = 0;
                while (true)
                {
                    if (num8 >= num5)
                    {
                        num7++;
                        break;
                    }
                    int num9 = 0;
                    while (true)
                    {
                        if (num9 >= num6)
                        {
                            num8++;
                            break;
                        }
                        Vector3D item = vectord3 + new Vector3D(num7 * vectord2.X, num8 * vectord2.Y, num9 * vectord2.Z);
                        positionBuffer.Add(item);
                        num9++;
                    }
                }
            }
        }

        private void GenerateVoxelDebrisPositionOffsets(List<Vector3> offsetBuffer)
        {
            offsetBuffer.Clear();
            Vector3 vector = new Vector3(-0.7f);
            int num = 0;
            while (num < 2)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 >= 2)
                    {
                        num++;
                        break;
                    }
                    int num3 = 0;
                    while (true)
                    {
                        if (num3 >= 2)
                        {
                            num2++;
                            break;
                        }
                        Vector3 item = vector + new Vector3(num * 1.4f, num2 * 1.4f, num3 * 1.4f);
                        offsetBuffer.Add(item);
                        num3++;
                    }
                }
            }
        }

        public static string GetAmountBasedDebrisVoxel(float amount)
        {
            foreach (MyDebrisDefinition definition in m_debrisVoxels)
            {
                if (definition.MinAmount <= amount)
                {
                    return definition.Model;
                }
            }
            return m_debrisVoxels[0].Model;
        }

        public static string GetAnyAmountLessDebrisVoxel(float minAmount, float maxAmount)
        {
            int num = 0;
            int num2 = 0;
            MyDebrisDefinition[] debrisVoxels = m_debrisVoxels;
            for (int i = 0; i < debrisVoxels.Length; i++)
            {
                MyDebrisDefinition definition1 = debrisVoxels[i];
                if (definition1.MinAmount > maxAmount)
                {
                    num++;
                }
                if (definition1.MinAmount > minAmount)
                {
                    num2++;
                }
            }
            int index = MyUtils.GetRandomInt((num2 - num) + 1) + num;
            return m_debrisVoxels[index].Model;
        }

        public HkShape GetDebrisShape(MyModel model, HkShapeType shapeType, float scale)
        {
            HkShape shape;
            MyModelShapeInfo key = new MyModelShapeInfo {
                Model = model,
                ShapeType = shapeType,
                Scale = scale
            };
            if (!this.m_shapes.TryGetValue(key, out shape))
            {
                shape = this.CreateShape(model, shapeType, scale);
                this.m_shapes.TryAdd(key, shape);
            }
            return shape;
        }

        public static string GetRandomDebrisModel() => 
            m_debrisModels.GetRandomItem<string>();

        public static string GetRandomDebrisVoxel() => 
            m_debrisVoxels.GetRandomItem<MyDebrisDefinition>().Model;

        public override void LoadData()
        {
            int num;
            this.m_positionBuffer = new List<Vector3D>(0x18);
            this.m_voxelDebrisOffsets = new List<Vector3>(8);
            this.m_desc.LifespanMinInMiliseconds = 0x2710;
            this.m_desc.LifespanMaxInMiliseconds = 0x4e20;
            this.m_desc.OnCloseAction = new Action<MyDebrisBase>(this.OnDebrisClosed);
            this.GenerateVoxelDebrisPositionOffsets(this.m_voxelDebrisOffsets);
            Static = this;
            string[] debrisModels = m_debrisModels;
            for (num = 0; num < debrisModels.Length; num++)
            {
                MyModels.GetModelOnlyData(debrisModels[num]);
            }
            MyDebrisDefinition[] debrisVoxels = m_debrisVoxels;
            for (num = 0; num < debrisVoxels.Length; num++)
            {
                MyModels.GetModelOnlyData(debrisVoxels[num].Model);
            }
        }

        private void OnDebrisClosed(MyDebrisBase obj)
        {
            Interlocked.Decrement(ref this.m_debrisCount);
        }

        protected override void UnloadData()
        {
            if (Static != null)
            {
                foreach (KeyValuePair<MyModelShapeInfo, HkShape> pair in this.m_shapes)
                {
                    pair.Value.RemoveReference();
                }
                this.m_shapes.Clear();
                this.m_positionBuffer = null;
                Static = null;
                this.m_creationBuffer.Clear();
            }
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            int count = this.m_creationBuffer.Count;
            if (count > 20)
            {
                count /= 10;
            }
            while (true)
            {
                count--;
                if (count <= 0)
                {
                    return;
                }
                DebrisCreationInfo info = this.m_creationBuffer.Dequeue();
                DebrisType type = info.Type;
                if (type == DebrisType.Voxel)
                {
                    (this.CreateVoxelDebris(50f, info.Ammount).Debris as MyDebrisVoxel.MyDebrisVoxelLogic).Start(info.Position, info.Velocity, info.Material);
                    continue;
                }
                if (type != DebrisType.Random)
                {
                    throw new ArgumentOutOfRangeException();
                }
                MyDebrisBase base2 = this.CreateRandomDebris();
                if (base2 != null)
                {
                    base2.Debris.Start(info.Position, info.Velocity);
                }
            }
        }

        public static MyDebris Static
        {
            get => 
                m_static;
            private set => 
                (m_static = value);
        }

        public override System.Type[] Dependencies =>
            new System.Type[] { typeof(MyPhysics) };

        public bool TooManyDebris =>
            (this.m_debrisCount > 0x21);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyDebris.<>c <>9 = new MyDebris.<>c();
            public static Func<MyDebrisDefinition, bool> <>9__16_0;
            public static Func<MyDebrisDefinition, string> <>9__16_1;
            public static Func<MyDebrisDefinition, bool> <>9__16_2;
            public static Func<MyDebrisDefinition, float> <>9__16_3;

            internal bool <.ctor>b__16_0(MyDebrisDefinition x) => 
                (x.Type == MyDebrisType.Model);

            internal string <.ctor>b__16_1(MyDebrisDefinition x) => 
                x.Model;

            internal bool <.ctor>b__16_2(MyDebrisDefinition x) => 
                (x.Type == MyDebrisType.Voxel);

            internal float <.ctor>b__16_3(MyDebrisDefinition x) => 
                x.MinAmount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DebrisCreationInfo
        {
            public MyDebris.DebrisType Type;
            public float Ammount;
            public Vector3 Velocity;
            public Vector3D Position;
            public MyVoxelMaterialDefinition Material;
        }

        private enum DebrisType
        {
            Voxel,
            Random
        }

        private class MyCreateDebrisWork : AbstractWork
        {
            private static Stack<MyDebris.MyCreateDebrisWork> m_pool = new Stack<MyDebris.MyCreateDebrisWork>();
            public readonly Action CompletionCallback;
            private readonly List<DebrisData> m_pieces = new List<DebrisData>();
            public MyDebris Context;
            public int debrisPieces;
            public float initialSpeed;
            public float minSourceDistance;
            public float maxSourceDistance;
            public float minDeviationAngle;
            public float maxDeviationAngle;
            public Vector3 offsetDirection;
            public Vector3 sourceWorldPosition;

            private MyCreateDebrisWork()
            {
                this.CompletionCallback = new Action(this.OnWorkCompleted);
            }

            public static MyDebris.MyCreateDebrisWork Create()
            {
                if (m_pool.Count != 0)
                {
                    return m_pool.Pop();
                }
                MyDebris.MyCreateDebrisWork work1 = new MyDebris.MyCreateDebrisWork();
                work1.Options = Parallel.DefaultOptions;
                return work1;
            }

            public override void DoWork(WorkData unused)
            {
                if (MySession.Static.Ready)
                {
                    MyEntityIdentifier.InEntityCreationBlock = true;
                    MyEntityIdentifier.LazyInitPerThreadStorage(0x800);
                    int num = 0;
                    while (true)
                    {
                        if (num < this.debrisPieces)
                        {
                            MyDebrisBase base2 = this.Context.CreateRandomDebris();
                            if (base2 != null)
                            {
                                float randomFloat = MyUtils.GetRandomFloat(this.minDeviationAngle, this.maxDeviationAngle);
                                Matrix matrix = Matrix.CreateRotationX(MyUtils.GetRandomFloat(this.minDeviationAngle, this.maxDeviationAngle)) * Matrix.CreateRotationY(randomFloat);
                                Vector3 vector = Vector3.Transform(this.offsetDirection, matrix);
                                DebrisData item = new DebrisData {
                                    Object = base2,
                                    StartPos = this.sourceWorldPosition + (vector * MyUtils.GetRandomFloat(this.minSourceDistance, this.maxSourceDistance)),
                                    InitialVelocity = vector * this.initialSpeed
                                };
                                this.m_pieces.Add(item);
                                num++;
                                continue;
                            }
                        }
                        MyEntityIdentifier.ClearPerThreadEntities();
                        MyEntityIdentifier.InEntityCreationBlock = false;
                        return;
                    }
                }
            }

            private void OnWorkCompleted()
            {
                if (MySession.Static.Ready)
                {
                    MyEntityIdentifier.InEntityCreationBlock = true;
                    foreach (DebrisData data in this.m_pieces)
                    {
                        MyEntityIdentifier.AddEntityWithId(data.Object);
                        data.Object.Debris.Start(data.StartPos, data.InitialVelocity);
                    }
                    MyEntityIdentifier.InEntityCreationBlock = false;
                    this.Release();
                }
            }

            private void Release()
            {
                this.Context = null;
                this.m_pieces.Clear();
                m_pool.Push(this);
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct DebrisData
            {
                public MyDebrisBase Object;
                public Vector3 InitialVelocity;
                public Vector3 StartPos;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyModelShapeInfo
        {
            public MyModel Model;
            public HkShapeType ShapeType;
            public float Scale;
        }
    }
}

