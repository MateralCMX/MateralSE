namespace Sandbox.Game.Entities.Cube
{
    using Havok;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems.StructuralIntegrity;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGridShape : IDisposable
    {
        public const int MAX_SHAPE_COUNT = 0xfd6f;
        private MyVoxelSegmentation m_segmenter;
        private MyCubeBlockCollector m_blockCollector = new MyCubeBlockCollector();
        private HkMassProperties m_massProperties;
        private HkMassProperties m_originalMassProperties;
        private bool m_originalMassPropertiesSet;
        private List<HkShape> m_tmpShapes = new List<HkShape>();
        private HashSet<MySlimBlock> m_tmpRemovedBlocks = new HashSet<MySlimBlock>();
        private HashSet<Vector3I> m_tmpRemovedCubes = new HashSet<Vector3I>();
        private HashSet<Vector3I> m_tmpAdditionalCubes = new HashSet<Vector3I>();
        private MyCubeGrid m_grid;
        private HkGridShape m_root;
        private static FastResourceLock m_shapeAccessLock = new FastResourceLock();
        private Dictionary<Vector3I, HkdShapeInstanceInfo> m_blocksShapes = new Dictionary<Vector3I, HkdShapeInstanceInfo>();
        private const int MassCellSize = 4;
        private MyGridMassComputer m_massElements;
        [ThreadStatic]
        private static List<HkMassElement> s_tmpElements;
        public static uint INVALID_COMPOUND_ID = uint.MaxValue;
        [ThreadStatic]
        private static List<Vector3S> m_removalMins;
        [ThreadStatic]
        private static List<Vector3S> m_removalMaxes;
        [ThreadStatic]
        private static List<bool> m_removalResults;
        private HashSet<Vector3I> m_updateConnections = new HashSet<Vector3I>();
        private List<HkBodyCollision> m_penetrations = new List<HkBodyCollision>();
        private List<MyVoxelBase> m_overlappingVoxels = new List<MyVoxelBase>();
        public HashSet<Vector3I> BlocksConnectedToWorld = new HashSet<Vector3I>();
        private List<HkdShapeInstanceInfo> m_shapeInfosList = new List<HkdShapeInstanceInfo>();
        private List<HkdShapeInstanceInfo> m_shapeInfosList2 = new List<HkdShapeInstanceInfo>();
        private List<HkdConnection> m_connectionsToAddCache = new List<HkdConnection>();
        private List<HkShape> m_khpShapeList = new List<HkShape>();
        private static List<HkdShapeInstanceInfo> m_tmpChildren = new List<HkdShapeInstanceInfo>();
        private HashSet<MySlimBlock> m_processedBlock = new HashSet<MySlimBlock>();
        private static List<HkdShapeInstanceInfo> m_shapeInfosList3 = new List<HkdShapeInstanceInfo>();
        private Dictionary<Vector3I, List<HkdConnection>> m_connections = new Dictionary<Vector3I, List<HkdConnection>>();
        private static object m_sharedParentLock = new object();
        private bool m_isSharedTensorDirty;

        public MyGridShape(MyCubeGrid grid)
        {
            this.m_grid = grid;
            if (!MyPerGameSettings.Destruction)
            {
                if (MyPerGameSettings.UseGridSegmenter)
                {
                    this.m_segmenter = new MyVoxelSegmentation();
                }
                this.m_massElements = new MyGridMassComputer(4, 0.05f);
                try
                {
                    this.m_blockCollector.Collect(grid, this.m_segmenter, MyVoxelSegmentationType.Simple, this.m_massElements);
                    this.m_root = new HkGridShape(this.m_grid.GridSize, HkReferencePolicy.None);
                    this.AddShapesFromCollector();
                    if (!this.m_grid.IsStatic)
                    {
                        this.UpdateMassProperties();
                    }
                }
                finally
                {
                    this.m_blockCollector.Clear();
                }
            }
        }

        private void AddConnections()
        {
            int count = 0;
            foreach (List<HkdConnection> list in this.m_connections.Values)
            {
                count += list.Count;
            }
            this.BreakableShape.ClearConnections();
            this.BreakableShape.ReplaceConnections(this.m_connections, count);
        }

        private void AddShapesFromCollector()
        {
            int num = 0;
            int num2 = 0;
            while (num2 < this.m_blockCollector.ShapeInfos.Count)
            {
                MyCubeBlockCollector.ShapeInfo info = this.m_blockCollector.ShapeInfos[num2];
                this.m_tmpShapes.Clear();
                int num3 = 0;
                while (true)
                {
                    if (num3 >= info.Count)
                    {
                        num += info.Count;
                        if ((this.m_root.ShapeCount + this.m_tmpShapes.Count) > 0xfd6f)
                        {
                            MyHud.Notifications.Add(MyNotificationSingletons.GridReachedPhysicalLimit);
                        }
                        if ((this.m_root.ShapeCount + this.m_tmpShapes.Count) < 0x10000)
                        {
                            this.m_root.AddShapes(this.m_tmpShapes, new Vector3S(info.Min), new Vector3S(info.Max));
                        }
                        num2++;
                        break;
                    }
                    this.m_tmpShapes.Add(this.m_blockCollector.Shapes[num + num3]);
                    num3++;
                }
            }
            this.m_tmpShapes.Clear();
        }

        private bool CheckConnection(HkdConnection c)
        {
            HkdBreakableShape shapeA = c.ShapeA;
            while (shapeA.HasParent)
            {
                shapeA = shapeA.GetParent();
            }
            if (shapeA != this.BreakableShape)
            {
                return false;
            }
            shapeA = c.ShapeB;
            while (shapeA.HasParent)
            {
                shapeA = shapeA.GetParent();
            }
            return !(shapeA != this.BreakableShape);
        }

        [Conditional("DEBUG")]
        private unsafe void CheckShapePositions(List<MyCubeBlockCollector.ShapeInfo> infos)
        {
            foreach (MyCubeBlockCollector.ShapeInfo info in infos)
            {
                Vector3I vectori;
                vectori.X = info.Min.X;
                while (vectori.X <= info.Max.X)
                {
                    vectori.Y = info.Min.Y;
                    while (true)
                    {
                        if (vectori.Y > info.Max.Y)
                        {
                            int* numPtr3 = (int*) ref vectori.X;
                            numPtr3[0]++;
                            break;
                        }
                        vectori.Z = info.Min.Z;
                        while (true)
                        {
                            if (vectori.Z > info.Max.Z)
                            {
                                int* numPtr2 = (int*) ref vectori.Y;
                                numPtr2[0]++;
                                break;
                            }
                            int* numPtr1 = (int*) ref vectori.Z;
                            numPtr1[0]++;
                        }
                    }
                }
            }
        }

        private static void CollectBlockInventories(List<MyCubeBlock> blocks, float cargoMassMultiplier, List<HkMassElement> massElementsOut)
        {
            foreach (MyCubeBlock block in blocks)
            {
                float m = 0f;
                if (block is MyCockpit)
                {
                    MyCockpit cockpit = block as MyCockpit;
                    if (cockpit.Pilot != null)
                    {
                        m += cockpit.Pilot.BaseMass;
                    }
                }
                if (block.HasInventory)
                {
                    for (int i = 0; i < block.InventoryCount; i++)
                    {
                        MyInventory inventory = block.GetInventory(i);
                        if (inventory != null)
                        {
                            m += ((float) inventory.CurrentMass) * cargoMassMultiplier;
                        }
                    }
                }
                if (m > 0f)
                {
                    Vector3 position = ((block.Min + block.Max) * 0.5f) * block.CubeGrid.GridSize;
                    HkMassProperties properties = new HkMassProperties();
                    HkMassElement item = new HkMassElement {
                        Properties = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties((((block.Max - block.Min) + Vector3I.One) * block.CubeGrid.GridSize) / 2f, MyPerGameSettings.Destruction ? MyDestructionHelper.MassToHavok(m) : m),
                        Tranform = Matrix.CreateTranslation(position)
                    };
                    massElementsOut.Add(item);
                }
            }
        }

        private void ConnectBlocks(HkdBreakableShape parent, MySlimBlock blockA, MySlimBlock blockB, List<HkdConnection> blockConnections)
        {
            if (this.m_blocksShapes.ContainsKey(blockA.Position) && this.m_blocksShapes.ContainsKey(blockB.Position))
            {
                HkdShapeInstanceInfo info = this.m_blocksShapes[blockA.Position];
                HkdShapeInstanceInfo info2 = this.m_blocksShapes[blockB.Position];
                info2.GetChildren(this.m_shapeInfosList2);
                bool flag = info2.Shape.GetChildrenCount() == 0;
                foreach (HkdShapeInstanceInfo info3 in this.m_shapeInfosList2)
                {
                    info3.DynamicParent = HkdShapeInstanceInfo.INVALID_INDEX;
                }
                Vector3 vector = (Vector3) (blockB.Position * this.m_grid.GridSize);
                Vector3 pivotA = (Vector3) (blockA.Position * this.m_grid.GridSize);
                Vector3 vector3 = Vector3.Normalize((Vector3) (blockB.Position - blockA.Position));
                Matrix orientation = info2.GetTransform().GetOrientation();
                int num = 0;
                while (num < this.m_shapeInfosList2.Count)
                {
                    HkdShapeInstanceInfo info4 = this.m_shapeInfosList2[num];
                    Matrix transform = info4.GetTransform();
                    ushort dynamicParent = info4.DynamicParent;
                    while (true)
                    {
                        if (dynamicParent == HkdShapeInstanceInfo.INVALID_INDEX)
                        {
                            Vector4 vector4;
                            Vector4 vector5;
                            transform *= orientation;
                            info4.Shape.GetShape().GetLocalAABB(0.1f, out vector4, out vector5);
                            if ((((pivotA - (vector + Vector3.Transform(new Vector3(vector4), transform))) * vector3).AbsMax() <= 1.35f) || (((pivotA - (vector + Vector3.Transform(new Vector3(vector5), transform))) * vector3).AbsMax() <= 1.35f))
                            {
                                flag = true;
                                HkdConnection item = CreateConnection(info.Shape, info4.Shape, pivotA, vector + Vector3.Transform(info4.CoM, transform));
                                blockConnections.Add(item);
                                info4.GetChildren(this.m_shapeInfosList2);
                                for (int i = this.m_shapeInfosList2.Count - info4.Shape.GetChildrenCount(); i < this.m_shapeInfosList2.Count; i++)
                                {
                                    HkdShapeInstanceInfo info6 = this.m_shapeInfosList2[i];
                                    info6.DynamicParent = (ushort) num;
                                }
                            }
                            num++;
                            break;
                        }
                        transform *= this.m_shapeInfosList2[dynamicParent].GetTransform();
                        HkdShapeInstanceInfo info5 = this.m_shapeInfosList2[dynamicParent];
                        dynamicParent = info5.DynamicParent;
                    }
                }
                if (flag)
                {
                    HkdConnection item = CreateConnection(info.Shape, info2.Shape, (Vector3) (blockA.Position * this.m_grid.GridSize), (Vector3) (blockB.Position * this.m_grid.GridSize));
                    blockConnections.Add(item);
                }
                this.m_shapeInfosList2.Clear();
            }
        }

        public static void ConnectShapesWithChildren(HkdBreakableShape parent, HkdBreakableShape shapeA, HkdBreakableShape shapeB)
        {
            object sharedParentLock = m_sharedParentLock;
            lock (sharedParentLock)
            {
                HkdConnection connection = CreateConnection(shapeA, shapeB, shapeA.CoM, shapeB.CoM);
                connection.AddToCommonParent();
                connection.RemoveReference();
                shapeB.GetChildren(m_shapeInfosList3);
                foreach (HkdShapeInstanceInfo info in m_shapeInfosList3)
                {
                    HkdConnection connection2 = CreateConnection(shapeA, info.Shape, shapeA.CoM, shapeB.CoM);
                    connection2.AddToCommonParent();
                    connection2.RemoveReference();
                }
                m_shapeInfosList3.Clear();
            }
        }

        private unsafe HkdBreakableShape? CreateBlockShape(MySlimBlock b, out Matrix blockTransform)
        {
            blockTransform = Matrix.Identity;
            if (b.FatBlock == null)
            {
                return null;
            }
            HkdBreakableShape shape = new HkdBreakableShape();
            Matrix identity = Matrix.Identity;
            if (b.FatBlock is MyCompoundCubeBlock)
            {
                blockTransform.Translation = b.FatBlock.PositionComp.LocalMatrix.Translation;
                MyCompoundCubeBlock fatBlock = b.FatBlock as MyCompoundCubeBlock;
                if (fatBlock.GetBlocksCount() == 1)
                {
                    MySlimBlock block = fatBlock.GetBlocks()[0];
                    ushort? blockId = fatBlock.GetBlockId(block);
                    MyFractureComponentBase fractureComponent = block.GetFractureComponent();
                    if (fractureComponent != null)
                    {
                        shape = fractureComponent.Shape;
                        shape.AddReference();
                    }
                    else
                    {
                        Matrix matrix3;
                        MyCubeBlockDefinition blockDefinition = block.FatBlock.BlockDefinition;
                        string model = block.CalculateCurrentModel(out matrix3);
                        if (!MyFakes.LAZY_LOAD_DESTRUCTION && !HasBreakableShape(model, blockDefinition))
                        {
                            object[] objArray1 = new object[] { "Breakable shape not preallocated: ", model, " definition: ", blockDefinition };
                            MySandboxGame.Log.WriteLine(string.Concat(objArray1));
                            GetBreakableShape(model, blockDefinition, true);
                        }
                        if (MyFakes.LAZY_LOAD_DESTRUCTION || HasBreakableShape(model, blockDefinition))
                        {
                            shape = GetBreakableShape(model, blockDefinition, false);
                        }
                    }
                    if (shape.IsValid())
                    {
                        HkPropertyBase base4 = (HkPropertyBase) new HkSimpleValueProperty(blockId.Value);
                        shape.SetPropertyRecursively(HkdBreakableShape.PROPERTY_BLOCK_COMPOUND_ID, base4);
                        base4.RemoveReference();
                    }
                    block.Orientation.GetMatrix(out identity);
                    blockTransform = identity * blockTransform;
                }
                else
                {
                    Vector3I vectori1 = b.Position * this.m_grid.GridSize;
                    float num = 0f;
                    foreach (MySlimBlock block3 in fatBlock.GetBlocks())
                    {
                        block3.Orientation.GetMatrix(out identity);
                        identity.Translation = Vector3.Zero;
                        ushort? blockId = fatBlock.GetBlockId(block3);
                        MyFractureComponentBase fractureComponent = block3.GetFractureComponent();
                        if (fractureComponent != null)
                        {
                            shape = fractureComponent.Shape;
                            HkdBreakableShape* shapePtr1 = (HkdBreakableShape*) ref shape;
                            shapePtr1.UserObject |= 1;
                            shape.AddReference();
                            this.m_shapeInfosList2.Add(new HkdShapeInstanceInfo(shape, identity));
                        }
                        else
                        {
                            Matrix matrix4;
                            MyCubeBlockDefinition blockDefinition = block3.BlockDefinition;
                            string model = block3.CalculateCurrentModel(out matrix4);
                            if (!MyFakes.LAZY_LOAD_DESTRUCTION && !HasBreakableShape(model, blockDefinition))
                            {
                                object[] objArray2 = new object[] { "Breakable shape not preallocated: ", model, " definition: ", blockDefinition };
                                MySandboxGame.Log.WriteLine(string.Concat(objArray2));
                                GetBreakableShape(model, blockDefinition, true);
                            }
                            if (MyFakes.LAZY_LOAD_DESTRUCTION || HasBreakableShape(model, blockDefinition))
                            {
                                shape = GetBreakableShape(model, blockDefinition, false);
                                HkdBreakableShape* shapePtr2 = (HkdBreakableShape*) ref shape;
                                shapePtr2.UserObject |= 1;
                                num += blockDefinition.Mass;
                                this.m_shapeInfosList2.Add(new HkdShapeInstanceInfo(shape, identity));
                            }
                        }
                        if (shape.IsValid())
                        {
                            HkPropertyBase base6 = (HkPropertyBase) new HkSimpleValueProperty(blockId.Value);
                            shape.SetPropertyRecursively(HkdBreakableShape.PROPERTY_BLOCK_COMPOUND_ID, base6);
                            base6.RemoveReference();
                        }
                    }
                    if (this.m_shapeInfosList2.Count == 0)
                    {
                        return null;
                    }
                    HkdBreakableShape? oldParent = null;
                    HkdBreakableShape shape2 = (HkdBreakableShape) new HkdCompoundBreakableShape(oldParent, this.m_shapeInfosList2);
                    shape2.RecalcMassPropsFromChildren();
                    HkMassProperties massProperties = new HkMassProperties();
                    shape2.BuildMassProperties(ref massProperties);
                    shape = new HkdBreakableShape(shape2.GetShape(), ref massProperties);
                    shape2.RemoveReference();
                    foreach (HkdShapeInstanceInfo info in this.m_shapeInfosList2)
                    {
                        shape.AddShape(ref info);
                    }
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= this.m_shapeInfosList2.Count)
                        {
                            foreach (HkdShapeInstanceInfo info3 in this.m_shapeInfosList2)
                            {
                                info3.Shape.RemoveReference();
                                info3.RemoveReference();
                            }
                            this.m_shapeInfosList2.Clear();
                            break;
                        }
                        int num3 = 0;
                        while (true)
                        {
                            if (num3 >= this.m_shapeInfosList2.Count)
                            {
                                num2++;
                                break;
                            }
                            if (num2 != num3)
                            {
                                ConnectShapesWithChildren(shape, this.m_shapeInfosList2[num2].Shape, this.m_shapeInfosList2[num3].Shape);
                            }
                            num3++;
                        }
                    }
                }
            }
            else
            {
                Matrix matrix5;
                b.Orientation.GetMatrix(out blockTransform);
                blockTransform.Translation = b.FatBlock.PositionComp.LocalMatrix.Translation;
                string model = b.CalculateCurrentModel(out matrix5);
                if (b.FatBlock is MyFracturedBlock)
                {
                    shape = (b.FatBlock as MyFracturedBlock).Shape;
                    if (!shape.IsValid())
                    {
                        throw new Exception("Fractured block Breakable shape invalid!");
                    }
                    shape.AddReference();
                }
                else
                {
                    MyFractureComponentBase fractureComponent = b.GetFractureComponent();
                    if (fractureComponent != null)
                    {
                        shape = fractureComponent.Shape;
                        shape.AddReference();
                    }
                    else
                    {
                        if (!MyFakes.LAZY_LOAD_DESTRUCTION && !HasBreakableShape(model, b.BlockDefinition))
                        {
                            object[] objArray3 = new object[] { "Breakable shape not preallocated: ", model, " definition: ", b.BlockDefinition };
                            MySandboxGame.Log.WriteLine(string.Concat(objArray3));
                            GetBreakableShape(model, b.BlockDefinition, true);
                        }
                        if (MyFakes.LAZY_LOAD_DESTRUCTION || HasBreakableShape(model, b.BlockDefinition))
                        {
                            shape = GetBreakableShape(model, b.BlockDefinition, false);
                        }
                    }
                }
            }
            HkPropertyBase prop = (HkPropertyBase) new HkVec3IProperty(b.Position);
            if (!shape.IsValid())
            {
                object[] objArray4 = new object[] { "BreakableShape not valid: ", b.BlockDefinition.Id, " pos: ", b.Min, " grid cubes: ", b.CubeGrid.BlocksCount };
                MySandboxGame.Log.WriteLine(string.Concat(objArray4));
                if (b.FatBlock is MyCompoundCubeBlock)
                {
                    MyCompoundCubeBlock fatBlock = b.FatBlock as MyCompoundCubeBlock;
                    MySandboxGame.Log.WriteLine("Compound blocks count: " + fatBlock.GetBlocksCount());
                    foreach (MySlimBlock block5 in fatBlock.GetBlocks())
                    {
                        MySandboxGame.Log.WriteLine("Block in compound: " + block5.BlockDefinition.Id);
                    }
                }
            }
            shape.SetPropertyRecursively(HkdBreakableShape.PROPERTY_GRID_POSITION, prop);
            prop.RemoveReference();
            return new HkdBreakableShape?(shape);
        }

        public HkdBreakableShape? CreateBreakableShape()
        {
            this.m_blocksShapes.Clear();
            foreach (MySlimBlock block in this.m_grid.GetBlocks())
            {
                Matrix matrix;
                HkdBreakableShape? nullable = this.CreateBlockShape(block, out matrix);
                if (nullable != null)
                {
                    HkdShapeInstanceInfo item = new HkdShapeInstanceInfo(nullable.Value, matrix);
                    this.m_shapeInfosList.Add(item);
                    this.m_blocksShapes[block.Position] = item;
                }
            }
            if (this.m_blocksShapes.Count == 0)
            {
                return null;
            }
            if (this.BreakableShape.IsValid())
            {
                this.BreakableShape.RemoveReference();
            }
            HkdBreakableShape? oldParent = null;
            this.BreakableShape = (HkdBreakableShape) new HkdCompoundBreakableShape(oldParent, this.m_shapeInfosList);
            this.BreakableShape.SetChildrenParent(this.BreakableShape);
            try
            {
                this.BreakableShape.SetStrenghtRecursively(MyDestructionConstants.STRENGTH, 0.7f);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine(exception);
                MyLog.Default.WriteLine("BS Valid: " + this.BreakableShape.IsValid().ToString());
                HkdBreakableShape breakableShape = this.BreakableShape;
                MyLog.Default.WriteLine("BS Child count: " + breakableShape.GetChildrenCount());
                MyLog.Default.WriteLine("Grid shapes: " + this.m_shapeInfosList.Count);
                foreach (HkdShapeInstanceInfo info2 in this.m_shapeInfosList)
                {
                    breakableShape = info2.Shape;
                    if (!breakableShape.IsValid())
                    {
                        MyLog.Default.WriteLine("Invalid child!");
                        continue;
                    }
                    MyLog.Default.WriteLine("Child strength: " + info2.Shape.GetStrenght());
                }
                MyLog.Default.WriteLine("Grid Blocks count: " + this.m_grid.GetBlocks().Count);
                MyLog.Default.WriteLine("Grid MarkedForClose: " + this.m_grid.MarkedForClose.ToString());
                HashSet<MyDefinitionId> set = new HashSet<MyDefinitionId>();
                foreach (MySlimBlock block2 in this.m_grid.GetBlocks())
                {
                    if ((block2.FatBlock != null) && block2.FatBlock.MarkedForClose)
                    {
                        MyLog.Default.WriteLine("Block marked for close: " + block2.BlockDefinition.Id);
                    }
                    if (set.Count >= 50)
                    {
                        break;
                    }
                    if (block2.FatBlock is MyCompoundCubeBlock)
                    {
                        foreach (MySlimBlock block3 in (block2.FatBlock as MyCompoundCubeBlock).GetBlocks())
                        {
                            set.Add(block3.BlockDefinition.Id);
                            if ((block3.FatBlock != null) && block3.FatBlock.MarkedForClose)
                            {
                                MyLog.Default.WriteLine("Block in compound marked for close: " + block3.BlockDefinition.Id);
                            }
                        }
                        continue;
                    }
                    set.Add(block2.BlockDefinition.Id);
                }
                foreach (MyDefinitionId id in set)
                {
                    MyLog.Default.WriteLine("Block definition: " + id);
                }
                throw new InvalidOperationException();
            }
            this.CreateConnectionsManually(this.BreakableShape);
            this.m_shapeInfosList.Clear();
            return new HkdBreakableShape?(this.BreakableShape);
        }

        private static HkdConnection CreateConnection(HkdBreakableShape aShape, HkdBreakableShape bShape, Vector3 pivotA, Vector3 pivotB) => 
            new HkdConnection(aShape, bShape, pivotA, pivotB, bShape.CoM - aShape.CoM, 6.25f);

        public void CreateConnectionsManually(HkdBreakableShape shape)
        {
            this.m_connections.Clear();
            foreach (MySlimBlock block in this.m_grid.CubeBlocks)
            {
                if (this.m_blocksShapes.ContainsKey(block.Position))
                {
                    if (!this.m_connections.ContainsKey(block.Position))
                    {
                        this.m_connections[block.Position] = new List<HkdConnection>();
                    }
                    List<HkdConnection> blockConnections = this.m_connections[block.Position];
                    foreach (MySlimBlock block2 in block.Neighbours)
                    {
                        if (this.m_blocksShapes.ContainsKey(block2.Position))
                        {
                            this.ConnectBlocks(shape, block, block2, blockConnections);
                        }
                    }
                }
            }
            this.AddConnections();
        }

        public void CreateConnectionToWorld(HkdBreakableBody destructionBody, HkWorld havokWorld)
        {
            if (this.BlocksConnectedToWorld.Count != 0)
            {
                HkdFixedConnectivity connectivity = HkdFixedConnectivity.Create();
                foreach (Vector3I vectori in this.BlocksConnectedToWorld)
                {
                    HkdShapeInstanceInfo info = this.m_blocksShapes[vectori];
                    HkdFixedConnectivity.Connection c = new HkdFixedConnectivity.Connection(Vector3.Zero, Vector3.Up, 1f, info.Shape, havokWorld.GetFixedBody(), 0);
                    connectivity.AddConnection(ref c);
                    c.RemoveReference();
                }
                destructionBody.SetFixedConnectivity(connectivity);
                connectivity.RemoveReference();
            }
        }

        internal void DebugDraw()
        {
            HkdShapeInstanceInfo info;
            Matrix transform;
            Vector3D vectord2;
            if (MyDebugDrawSettings.BREAKABLE_SHAPE_CHILD_COUNT)
            {
                foreach (KeyValuePair<Vector3I, HkdShapeInstanceInfo> pair in this.m_blocksShapes)
                {
                    transform = pair.Value.GetTransform();
                    info = pair.Value;
                    Vector3D worldCoord = this.m_grid.GridIntegerToWorld((transform.Translation + info.CoM) / this.m_grid.GridSize);
                    vectord2 = worldCoord - MySector.MainCamera.Position;
                    if (vectord2.Length() <= 20.0)
                    {
                        MyRenderProxy.DebugDrawText3D(worldCoord, MyValueFormatter.GetFormatedInt(pair.Value.Shape.GetChildrenCount()), Color.White, 0.65f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                    }
                }
            }
            if (MyHonzaInputComponent.DefaultComponent.ShowRealBlockMass != MyHonzaInputComponent.DefaultComponent.ShownMassEnum.None)
            {
                vectord2 = this.m_grid.PositionComp.GetPosition() - MySector.MainCamera.Position;
                if (vectord2.Length() <= (20.0 + this.m_grid.PositionComp.WorldVolume.Radius))
                {
                    foreach (KeyValuePair<Vector3I, HkdShapeInstanceInfo> pair2 in this.m_blocksShapes)
                    {
                        transform = pair2.Value.GetTransform();
                        info = pair2.Value;
                        Vector3D worldCoord = this.m_grid.GridIntegerToWorld((transform.Translation + info.CoM) / this.m_grid.GridSize);
                        vectord2 = worldCoord - MySector.MainCamera.Position;
                        if (vectord2.Length() <= 20.0)
                        {
                            MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(pair2.Key);
                            if (cubeBlock != null)
                            {
                                float mass = cubeBlock.GetMass();
                                if (cubeBlock.FatBlock is MyFracturedBlock)
                                {
                                    mass = this.m_blocksShapes[cubeBlock.Position].Shape.GetMass();
                                }
                                MyHonzaInputComponent.DefaultComponent.ShownMassEnum showRealBlockMass = MyHonzaInputComponent.DefaultComponent.ShowRealBlockMass;
                                if (showRealBlockMass == MyHonzaInputComponent.DefaultComponent.ShownMassEnum.Real)
                                {
                                    mass = MyDestructionHelper.MassFromHavok(mass);
                                }
                                else if (showRealBlockMass == MyHonzaInputComponent.DefaultComponent.ShownMassEnum.SI)
                                {
                                    mass = MyAdvancedStaticSimulator.MassToSI(MyDestructionHelper.MassFromHavok(mass));
                                }
                                MyRenderProxy.DebugDrawText3D(worldCoord, MyValueFormatter.GetFormatedFloat(mass, (mass < 10f) ? 2 : 0), Color.White, 0.6f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            using (Dictionary<Vector3I, List<HkdConnection>>.ValueCollection.Enumerator enumerator = this.m_connections.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    foreach (HkdConnection connection in enumerator.Current)
                    {
                        connection.RemoveReference();
                    }
                }
            }
            this.m_connections.Clear();
            if (this.BreakableShape.IsValid())
            {
                this.BreakableShape.RemoveReference();
                this.BreakableShape.ClearHandle();
            }
            foreach (HkdShapeInstanceInfo info in this.m_blocksShapes.Values)
            {
                if (!info.IsReferenceValid())
                {
                    MyLog.Default.WriteLine("Block shape was disposed already in MyGridShape.Dispose!");
                }
                if (info.Shape.IsValid())
                {
                    info.Shape.RemoveReference();
                }
                info.RemoveReference();
            }
            this.m_blocksShapes.Clear();
            if (!MyPerGameSettings.Destruction)
            {
                this.m_root.Base.RemoveReference();
            }
        }

        private static unsafe void ExpandBlock(Vector3I cubePos, MyCubeGrid grid, HashSet<MySlimBlock> existingBlocks, HashSet<Vector3I> expandResult)
        {
            MySlimBlock cubeBlock = grid.GetCubeBlock(cubePos);
            if ((cubeBlock != null) && existingBlocks.Add(cubeBlock))
            {
                Vector3I vectori;
                vectori.X = cubeBlock.Min.X;
                while (vectori.X <= cubeBlock.Max.X)
                {
                    vectori.Y = cubeBlock.Min.Y;
                    while (true)
                    {
                        if (vectori.Y > cubeBlock.Max.Y)
                        {
                            int* numPtr3 = (int*) ref vectori.X;
                            numPtr3[0]++;
                            break;
                        }
                        vectori.Z = cubeBlock.Min.Z;
                        while (true)
                        {
                            if (vectori.Z > cubeBlock.Max.Z)
                            {
                                int* numPtr2 = (int*) ref vectori.Y;
                                numPtr2[0]++;
                                break;
                            }
                            expandResult.Add(vectori);
                            int* numPtr1 = (int*) ref vectori.Z;
                            numPtr1[0]++;
                        }
                    }
                }
            }
        }

        private static unsafe void ExpandBlock(Vector3I cubePos, MyCubeGrid grid, HashSet<MySlimBlock> existingBlocks, HashSet<Vector3I> checkList, HashSet<Vector3I> expandResult)
        {
            MySlimBlock cubeBlock = grid.GetCubeBlock(cubePos);
            if ((cubeBlock != null) && existingBlocks.Add(cubeBlock))
            {
                Vector3I vectori;
                vectori.X = cubeBlock.Min.X;
                while (vectori.X <= cubeBlock.Max.X)
                {
                    vectori.Y = cubeBlock.Min.Y;
                    while (true)
                    {
                        if (vectori.Y > cubeBlock.Max.Y)
                        {
                            int* numPtr3 = (int*) ref vectori.X;
                            numPtr3[0]++;
                            break;
                        }
                        vectori.Z = cubeBlock.Min.Z;
                        while (true)
                        {
                            if (vectori.Z > cubeBlock.Max.Z)
                            {
                                int* numPtr2 = (int*) ref vectori.Y;
                                numPtr2[0]++;
                                break;
                            }
                            if (!checkList.Contains(vectori))
                            {
                                expandResult.Add(vectori);
                            }
                            int* numPtr1 = (int*) ref vectori.Z;
                            numPtr1[0]++;
                        }
                    }
                }
            }
        }

        public void FindConnectionsToWorld()
        {
            this.FindConnectionsToWorld(this.m_grid.GetBlocks());
        }

        private void FindConnectionsToWorld(HashSet<MySlimBlock> blocks)
        {
            if (this.m_grid.IsStatic && ((this.m_grid.Physics == null) || (this.m_grid.Physics.LinearVelocity.LengthSquared() <= 0f)))
            {
                int num = 0;
                Quaternion identity = Quaternion.Identity;
                MatrixD worldMatrix = this.m_grid.WorldMatrix;
                MyGamePruningStructure.GetAllVoxelMapsInBox(ref this.m_grid.PositionComp.WorldAABB, this.m_overlappingVoxels);
                foreach (MySlimBlock block in blocks)
                {
                    Vector3D vectord;
                    Matrix matrix;
                    BoundingBox geometryLocalBox = block.FatBlock.GetGeometryLocalBox();
                    Vector3 halfExtents = geometryLocalBox.Size / 2f;
                    block.ComputeScaledCenter(out vectord);
                    vectord = Vector3D.Transform(vectord + geometryLocalBox.Center, worldMatrix);
                    block.Orientation.GetMatrix(out matrix);
                    identity = Quaternion.CreateFromRotationMatrix((MatrixD) (matrix * worldMatrix.GetOrientation()));
                    MyPhysics.GetPenetrationsBox(ref halfExtents, ref vectord, ref identity, this.m_penetrations, 14);
                    num++;
                    bool flag = false;
                    using (List<HkBodyCollision>.Enumerator enumerator2 = this.m_penetrations.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            IMyEntity collisionEntity = enumerator2.Current.GetCollisionEntity();
                            if ((collisionEntity != null) && (collisionEntity is MyVoxelBase))
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                    {
                        BoundingBoxD aabb = geometryLocalBox + block.Position;
                        using (List<MyVoxelBase>.Enumerator enumerator3 = this.m_overlappingVoxels.GetEnumerator())
                        {
                            while (enumerator3.MoveNext())
                            {
                                if (enumerator3.Current.IsAnyAabbCornerInside(ref worldMatrix, aabb))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                    }
                    this.m_penetrations.Clear();
                    if (flag && !this.BlocksConnectedToWorld.Contains(block.Position))
                    {
                        this.m_blocksShapes[block.Position].GetChildren(this.m_shapeInfosList2);
                        int num2 = 0;
                        while (true)
                        {
                            if (num2 >= this.m_shapeInfosList2.Count)
                            {
                                this.m_shapeInfosList2.Clear();
                                this.BlocksConnectedToWorld.Add(block.Position);
                                break;
                            }
                            HkdShapeInstanceInfo info2 = this.m_shapeInfosList2[num2];
                            if (info2.Shape.GetChildrenCount() <= 0)
                            {
                                info2.Shape.SetFlagRecursively(HkdBreakableShape.Flags.IS_FIXED);
                            }
                            else
                            {
                                info2.Shape.GetChildren(this.m_shapeInfosList2);
                            }
                            num2++;
                        }
                    }
                }
                this.m_overlappingVoxels.Clear();
            }
        }

        public MySlimBlock GetBlockFromShapeKey(uint shapeKey)
        {
            Vector3I vectori;
            this.m_root.GetShapeMin(shapeKey, out vectori);
            return this.m_grid.GetCubeBlock(vectori);
        }

        public float GetBlockMass(Vector3I position)
        {
            if (this.m_blocksShapes.ContainsKey(position))
            {
                return MyDestructionHelper.MassFromHavok(this.m_blocksShapes[position].Shape.GetMass());
            }
            return (!this.m_grid.CubeExists(position) ? 1f : this.m_grid.GetCubeBlock(position).GetMass());
        }

        private static HkdBreakableShape GetBreakableShape(string model, MyCubeBlockDefinition block, bool forceLoadDestruction = false)
        {
            if (MyFakes.LAZY_LOAD_DESTRUCTION | forceLoadDestruction)
            {
                MyModel modelOnlyData = MyModels.GetModelOnlyData(model);
                if (modelOnlyData.HavokBreakableShapes == null)
                {
                    MyDestructionData.Static.LoadModelDestruction(model, block, modelOnlyData.BoundingBoxSize, true, false);
                }
            }
            return MyDestructionData.Static.BlockShapePool.GetBreakableShape(model, block);
        }

        public void GetShapeBounds(uint shapeKey, out Vector3I min, out Vector3I max)
        {
            this.m_root.GetShapeBounds(shapeKey, out min, out max);
        }

        public List<HkShape> GetShapesFromPosition(Vector3I pos)
        {
            List<HkShape> resultList = new List<HkShape>();
            this.m_root.GetShape(pos, resultList);
            return resultList;
        }

        public void GetShapesInInterval(Vector3I min, Vector3I max, List<HkShape> shapeList)
        {
            this.m_root.GetShapesInInterval((Vector3) min, (Vector3) max, shapeList);
        }

        private static bool HasBreakableShape(string model, MyCubeBlockDefinition block)
        {
            MyModel modelOnlyData = MyModels.GetModelOnlyData(model);
            return ((modelOnlyData != null) && ((modelOnlyData.HavokBreakableShapes != null) && (modelOnlyData.HavokBreakableShapes.Length != 0)));
        }

        public void MarkBreakable(HkRigidBody rigidBody)
        {
            if ((this.m_grid.GetPhysicsBody().HavokWorld != null) && this.m_grid.BlocksDestructionEnabled)
            {
                this.MarkBreakable(this.m_grid.GetPhysicsBody().HavokWorld, rigidBody);
            }
        }

        public void MarkBreakable(HkWorld world, HkRigidBody rigidBody)
        {
            if (!MyPerGameSettings.Destruction)
            {
                world.BreakOffPartsUtil.MarkEntityBreakable(rigidBody, this.BreakImpulse);
            }
        }

        public void MarkSharedTensorDirty()
        {
            this.m_isSharedTensorDirty = true;
        }

        public static implicit operator HkShape(MyGridShape shape) => 
            ((HkShape) shape.m_root);

        public void RecalculateConnectionsToWorld(HashSet<MySlimBlock> blocks)
        {
            this.BlocksConnectedToWorld.Clear();
            this.FindConnectionsToWorld(blocks);
            if (this.m_grid.StructuralIntegrity != null)
            {
                this.m_grid.StructuralIntegrity.ForceRecalculation();
            }
        }

        public unsafe void RecomputeSharedTensorIfNeeded()
        {
            if (this.m_isSharedTensorDirty)
            {
                this.m_isSharedTensorDirty = false;
                using (MyUtils.ReuseCollection<HkMassElement>(ref s_tmpElements))
                {
                    HkMassElement element;
                    HashSetReader<MyGroups<MyCubeGrid, MySharedTensorData>.Node> gridsInSameGroup = MySharedTensorsGroups.GetGridsInSameGroup(this.m_grid);
                    if (gridsInSameGroup.IsValid)
                    {
                        MatrixD worldMatrixNormalizedInv = this.m_grid.PositionComp.WorldMatrixNormalizedInv;
                        using (HashSet<MyGroups<MyCubeGrid, MySharedTensorData>.Node>.Enumerator enumerator = gridsInSameGroup.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                MyCubeGrid nodeData = enumerator.Current.NodeData;
                                if (!ReferenceEquals(nodeData, this.m_grid))
                                {
                                    MyGridPhysics physics = nodeData.Physics;
                                    if (physics != null)
                                    {
                                        HkMassProperties properties3 = physics.Shape.m_massProperties;
                                        float* singlePtr1 = (float*) ref properties3.Mass;
                                        singlePtr1[0] /= 10f;
                                        element = new HkMassElement {
                                            Properties = properties3,
                                            Tranform = (Matrix) (nodeData.PositionComp.WorldMatrix * worldMatrixNormalizedInv)
                                        };
                                        s_tmpElements.Add(element);
                                    }
                                }
                            }
                        }
                    }
                    element = new HkMassElement {
                        Tranform = Matrix.Identity,
                        Properties = this.m_massProperties
                    };
                    s_tmpElements.Add(element);
                    HkMassProperties massProperties = this.m_massProperties;
                    massProperties.InertiaTensor = HkInertiaTensorComputer.CombineMassProperties(s_tmpElements).InertiaTensor;
                    this.m_grid.Physics.RigidBody.SetMassProperties(ref massProperties);
                    this.m_grid.NotifyMassPropertiesChanged();
                }
            }
        }

        public void RefreshBlocks(HkRigidBody rigidBody, HashSet<Vector3I> dirtyBlocks)
        {
            this.m_originalMassPropertiesSet = false;
            this.UpdateDirtyBlocks(dirtyBlocks, true);
            if ((rigidBody.GetMotionType() != HkMotionType.Keyframed) && !MyPerGameSettings.Destruction)
            {
                this.UpdateMassProperties();
            }
        }

        public void RefreshMass()
        {
            this.m_blockCollector.CollectMassElements(this.m_grid, this.m_massElements);
            this.UpdateMass(this.m_grid.Physics.RigidBody, false);
            this.m_grid.SetInventoryMassDirty();
        }

        internal void RemoveBlock(MySlimBlock block)
        {
            this.m_tmpRemovedCubes.Add(block.Min);
            this.m_root.RemoveShapes(this.m_tmpRemovedCubes, null, null, null);
            this.m_tmpRemovedCubes.Clear();
        }

        public void SetMass(HkRigidBody rigidBody)
        {
            if (!this.m_grid.Physics.IsWelded && (this.m_grid.GetPhysicsBody().WeldInfo.Children.Count == 0))
            {
                rigidBody.Mass = this.m_massProperties.Mass;
                rigidBody.SetMassProperties(ref this.m_massProperties);
            }
            else
            {
                this.m_grid.GetPhysicsBody().WeldedRigidBody.SetMassProperties(ref this.m_massProperties);
                this.m_grid.GetPhysicsBody().WeldInfo.SetMassProps(this.m_massProperties);
                this.m_grid.Physics.UpdateMassProps();
            }
            this.m_grid.NotifyMassPropertiesChanged();
        }

        private void SetMassProperties(MyPhysicsBody rb, List<HkMassElement> massElements)
        {
            this.m_massProperties = HkInertiaTensorComputer.CombineMassProperties(massElements);
            massElements.Clear();
            if (rb.IsWelded || (rb.WeldInfo.Children.Count != 0))
            {
                rb.WeldedRigidBody.SetMassProperties(ref this.m_massProperties);
                rb.WeldInfo.SetMassProps(this.m_massProperties);
                rb.UpdateMassProps();
            }
            else
            {
                HkRigidBody rigidBody = rb.RigidBody;
                if (!rigidBody.IsFixedOrKeyframed || (Vector3.Distance(rigidBody.CenterOfMassLocal, this.m_massProperties.CenterOfMass) > 1f))
                {
                    rigidBody.SetMassProperties(ref this.m_massProperties);
                }
                MySharedTensorsGroups.MarkGroupDirty(this.m_grid);
                MyGridPhysicalGroupData.InvalidateSharedMassPropertiesCache(this.m_grid);
                rb.ActivateIfNeeded();
            }
            this.m_grid.NotifyMassPropertiesChanged();
        }

        public void UnmarkBreakable(HkRigidBody rigidBody)
        {
            if ((this.m_grid.GetPhysicsBody().HavokWorld != null) && this.m_grid.BlocksDestructionEnabled)
            {
                this.UnmarkBreakable(this.m_grid.GetPhysicsBody().HavokWorld, rigidBody);
            }
        }

        public void UnmarkBreakable(HkWorld world, HkRigidBody rigidBody)
        {
            if (!MyPerGameSettings.Destruction)
            {
                world.BreakOffPartsUtil.UnmarkEntityBreakable(rigidBody);
            }
        }

        private void UpdateConnections(Vector3I dirty)
        {
            List<Vector3I> list1 = new List<Vector3I>(7);
            list1.Add(dirty);
            list1.Add(dirty + Vector3I.Up);
            list1.Add(dirty + Vector3I.Down);
            list1.Add(dirty + Vector3I.Left);
            list1.Add(dirty + Vector3I.Right);
            list1.Add(dirty + Vector3I.Forward);
            list1.Add(dirty + Vector3I.Backward);
            foreach (Vector3I vectori in list1)
            {
                if (this.m_connections.ContainsKey(vectori))
                {
                    foreach (HkdConnection connection in this.m_connections[vectori])
                    {
                        connection.RemoveReference();
                    }
                    this.m_connections[vectori].Clear();
                }
                MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(vectori);
                if (cubeBlock != null)
                {
                    if (this.m_connections.ContainsKey(cubeBlock.Position))
                    {
                        foreach (HkdConnection connection2 in this.m_connections[cubeBlock.Position])
                        {
                            connection2.RemoveReference();
                        }
                        this.m_connections[cubeBlock.Position].Clear();
                    }
                    this.m_updateConnections.Add(cubeBlock.Position);
                }
                this.m_updateConnections.Add(vectori);
            }
        }

        private void UpdateConnectionsManually(HkdBreakableShape shape, HashSet<Vector3I> dirtyCubes)
        {
            uint num = 0;
            foreach (Vector3I vectori in dirtyCubes)
            {
                MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(vectori);
                if ((cubeBlock != null) && !this.m_processedBlock.Contains(cubeBlock))
                {
                    if (!this.m_connections.ContainsKey(cubeBlock.Position))
                    {
                        this.m_connections[cubeBlock.Position] = new List<HkdConnection>();
                    }
                    List<HkdConnection> blockConnections = this.m_connections[cubeBlock.Position];
                    foreach (MySlimBlock block2 in cubeBlock.Neighbours)
                    {
                        this.ConnectBlocks(shape, cubeBlock, block2, blockConnections);
                        num++;
                    }
                    this.m_processedBlock.Add(cubeBlock);
                }
            }
            this.m_processedBlock.Clear();
        }

        internal unsafe void UpdateDirtyBlocks(HashSet<Vector3I> dirtyCubes, bool recreateShape = true)
        {
            if (dirtyCubes.Count > 0)
            {
                if (MyPerGameSettings.Destruction)
                {
                    HkdBreakableShape breakableShape = this.BreakableShape;
                    if (breakableShape.IsValid())
                    {
                        int num = 0;
                        HashSet<MySlimBlock> blocks = new HashSet<MySlimBlock>();
                        foreach (Vector3I vectori in dirtyCubes)
                        {
                            this.UpdateConnections(vectori);
                            this.BlocksConnectedToWorld.Remove(vectori);
                            if (this.m_blocksShapes.ContainsKey(vectori))
                            {
                                HkdShapeInstanceInfo info = this.m_blocksShapes[vectori];
                                info.Shape.RemoveReference();
                                info.RemoveReference();
                                this.m_blocksShapes.Remove(vectori);
                            }
                            MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(vectori);
                            if ((cubeBlock != null) && !blocks.Contains(cubeBlock))
                            {
                                if ((cubeBlock.Position != vectori) && this.m_blocksShapes.ContainsKey(cubeBlock.Position))
                                {
                                    HkdShapeInstanceInfo info2 = this.m_blocksShapes[cubeBlock.Position];
                                    info2.Shape.RemoveReference();
                                    info2.RemoveReference();
                                    this.m_blocksShapes.Remove(cubeBlock.Position);
                                }
                                blocks.Add(cubeBlock);
                                num++;
                            }
                        }
                        foreach (MySlimBlock block2 in blocks)
                        {
                            Matrix matrix;
                            HkdBreakableShape? nullable = this.CreateBlockShape(block2, out matrix);
                            if (nullable != null)
                            {
                                this.m_blocksShapes[block2.Position] = new HkdShapeInstanceInfo(nullable.Value, matrix);
                            }
                        }
                        foreach (HkdShapeInstanceInfo info3 in this.m_blocksShapes.Values)
                        {
                            this.m_shapeInfosList.Add(info3);
                        }
                        if (blocks.Count > 0)
                        {
                            this.FindConnectionsToWorld(blocks);
                        }
                        if (recreateShape)
                        {
                            this.BreakableShape.RemoveReference();
                            HkdBreakableShape? oldParent = null;
                            this.BreakableShape = (HkdBreakableShape) new HkdCompoundBreakableShape(oldParent, this.m_shapeInfosList);
                            this.BreakableShape.SetChildrenParent(this.BreakableShape);
                            this.BreakableShape.BuildMassProperties(ref this.m_massProperties);
                            this.BreakableShape.SetStrenghtRecursively(MyDestructionConstants.STRENGTH, 0.7f);
                        }
                        this.UpdateConnectionsManually(this.BreakableShape, this.m_updateConnections);
                        this.m_updateConnections.Clear();
                        this.AddConnections();
                        this.m_shapeInfosList.Clear();
                        return;
                    }
                }
                try
                {
                    if (m_removalMins == null)
                    {
                        m_removalMins = new List<Vector3S>();
                    }
                    if (m_removalMaxes == null)
                    {
                        m_removalMaxes = new List<Vector3S>();
                    }
                    if (m_removalResults == null)
                    {
                        m_removalResults = new List<bool>();
                    }
                    foreach (Vector3I vectori3 in dirtyCubes)
                    {
                        if (this.m_tmpRemovedCubes.Add(vectori3))
                        {
                            ExpandBlock(vectori3, this.m_grid, this.m_tmpRemovedBlocks, this.m_tmpRemovedCubes);
                        }
                    }
                    m_removalMins.Clear();
                    m_removalMaxes.Clear();
                    m_removalResults.Clear();
                    using (NativeShapeLock.AcquireExclusiveUsing())
                    {
                        this.m_root.RemoveShapes(this.m_tmpRemovedCubes, m_removalMins, m_removalMaxes, m_removalResults);
                        int num2 = 0;
                        while (true)
                        {
                            Vector3I vectori2;
                            if (num2 >= m_removalMins.Count)
                            {
                                while (true)
                                {
                                    if (this.m_tmpAdditionalCubes.Count <= 0)
                                    {
                                        this.m_blockCollector.CollectArea(this.m_grid, this.m_tmpRemovedCubes, this.m_segmenter, MyVoxelSegmentationType.Simple, this.m_massElements);
                                        this.AddShapesFromCollector();
                                        break;
                                    }
                                    m_removalMins.Clear();
                                    m_removalMaxes.Clear();
                                    m_removalResults.Clear();
                                    this.m_root.RemoveShapes(this.m_tmpAdditionalCubes, m_removalMins, m_removalMaxes, m_removalResults);
                                    this.m_tmpAdditionalCubes.Clear();
                                    for (int i = 0; i < m_removalMins.Count; i++)
                                    {
                                        if (m_removalResults[i])
                                        {
                                            vectori2.X = m_removalMins[i].X;
                                            while (vectori2.X <= m_removalMaxes[i].X)
                                            {
                                                vectori2.Y = m_removalMins[i].Y;
                                                while (true)
                                                {
                                                    if (vectori2.Y > m_removalMaxes[i].Y)
                                                    {
                                                        int* numPtr6 = (int*) ref vectori2.X;
                                                        numPtr6[0]++;
                                                        break;
                                                    }
                                                    vectori2.Z = m_removalMins[i].Z;
                                                    while (true)
                                                    {
                                                        if (vectori2.Z > m_removalMaxes[i].Z)
                                                        {
                                                            int* numPtr5 = (int*) ref vectori2.Y;
                                                            numPtr5[0]++;
                                                            break;
                                                        }
                                                        if (this.m_tmpRemovedCubes.Add(vectori2))
                                                        {
                                                            ExpandBlock(vectori2, this.m_grid, this.m_tmpRemovedBlocks, this.m_tmpRemovedCubes, this.m_tmpAdditionalCubes);
                                                        }
                                                        int* numPtr4 = (int*) ref vectori2.Z;
                                                        numPtr4[0]++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                            if (m_removalResults[num2])
                            {
                                vectori2.X = m_removalMins[num2].X;
                                while (vectori2.X <= m_removalMaxes[num2].X)
                                {
                                    vectori2.Y = m_removalMins[num2].Y;
                                    while (true)
                                    {
                                        if (vectori2.Y > m_removalMaxes[num2].Y)
                                        {
                                            int* numPtr3 = (int*) ref vectori2.X;
                                            numPtr3[0]++;
                                            break;
                                        }
                                        vectori2.Z = m_removalMins[num2].Z;
                                        while (true)
                                        {
                                            if (vectori2.Z > m_removalMaxes[num2].Z)
                                            {
                                                int* numPtr2 = (int*) ref vectori2.Y;
                                                numPtr2[0]++;
                                                break;
                                            }
                                            if (this.m_tmpRemovedCubes.Add(vectori2))
                                            {
                                                ExpandBlock(vectori2, this.m_grid, this.m_tmpRemovedBlocks, this.m_tmpRemovedCubes, this.m_tmpAdditionalCubes);
                                            }
                                            int* numPtr1 = (int*) ref vectori2.Z;
                                            numPtr1[0]++;
                                        }
                                    }
                                }
                            }
                            num2++;
                        }
                    }
                }
                finally
                {
                    m_removalMins.Clear();
                    m_removalMaxes.Clear();
                    m_removalResults.Clear();
                    this.m_blockCollector.Clear();
                    this.m_tmpRemovedBlocks.Clear();
                    this.m_tmpRemovedCubes.Clear();
                    this.m_tmpAdditionalCubes.Clear();
                }
            }
        }

        private void UpdateMass(HkRigidBody rigidBody, bool setMass = true)
        {
            if (rigidBody.GetMotionType() != HkMotionType.Keyframed)
            {
                if (!MyPerGameSettings.Destruction)
                {
                    this.UpdateMassProperties();
                }
                if (setMass)
                {
                    this.SetMass(rigidBody);
                }
            }
        }

        public void UpdateMassFromInventories(List<MyCubeBlock> blocks, MyPhysicsBody rb)
        {
            if ((rb.RigidBody != null) && (rb.RigidBody.IsFixed || !rb.RigidBody.IsFixedOrKeyframed))
            {
                HkMassElement element;
                float cargoMassMultiplier = 1f / MySession.Static.BlocksInventorySizeMultiplier;
                if (MyFakes.ENABLE_STATIC_INVENTORY_MASS)
                {
                    cargoMassMultiplier = 0f;
                }
                List<HkMassElement> tmpElements = TmpElements;
                CollectBlockInventories(blocks, cargoMassMultiplier, tmpElements);
                if (!MyPerGameSettings.Destruction)
                {
                    element = new HkMassElement {
                        Properties = this.m_originalMassProperties,
                        Tranform = Matrix.Identity
                    };
                    tmpElements.Add(element);
                }
                else
                {
                    HkMassProperties massProperties = new HkMassProperties();
                    this.BreakableShape.BuildMassProperties(ref massProperties);
                    element = new HkMassElement {
                        Properties = massProperties,
                        Tranform = Matrix.Identity
                    };
                    tmpElements.Add(element);
                }
                this.SetMassProperties(rb, tmpElements);
            }
        }

        private void UpdateMassProperties()
        {
            this.m_massProperties = this.m_massElements.UpdateMass();
            if (!this.m_originalMassPropertiesSet)
            {
                this.m_originalMassProperties = this.m_massProperties;
                this.m_originalMassPropertiesSet = true;
            }
        }

        public void UpdateShape(HkRigidBody rigidBody, HkRigidBody rigidBody2, HkdBreakableBody destructionBody)
        {
            if (destructionBody != null)
            {
                destructionBody.BreakableShape = this.BreakableShape;
                this.CreateConnectionToWorld(destructionBody, this.m_grid.Physics.HavokWorld);
            }
            else
            {
                rigidBody.SetShape((HkShape) this.m_root);
                if (rigidBody2 != null)
                {
                    rigidBody2.SetShape((HkShape) this.m_root);
                }
            }
        }

        public float BreakImpulse
        {
            get
            {
                if (((this.m_grid == null) || (this.m_grid.Physics == null)) || this.m_grid.Physics.IsStatic)
                {
                    return 1E+07f;
                }
                return Math.Max(this.m_grid.Physics.Mass * MyFakes.DEFORMATION_MINIMUM_VELOCITY, MyFakes.DEFORMATION_MIN_BREAK_IMPULSE);
            }
        }

        public HkdBreakableShape BreakableShape { get; set; }

        private static List<HkMassElement> TmpElements =>
            MyUtils.Init<List<HkMassElement>>(ref s_tmpElements);

        public HkMassProperties? MassProperties
        {
            get
            {
                if (!this.m_grid.IsStatic)
                {
                    return new HkMassProperties?(this.m_massProperties);
                }
                return null;
            }
        }

        public HkMassProperties? BaseMassProperties
        {
            get
            {
                if (!this.m_grid.IsStatic && this.m_originalMassPropertiesSet)
                {
                    return new HkMassProperties?(this.m_originalMassProperties);
                }
                return null;
            }
        }

        public int ShapeCount =>
            this.m_root.ShapeCount;

        public static FastResourceLock NativeShapeLock =>
            m_shapeAccessLock;
    }
}

