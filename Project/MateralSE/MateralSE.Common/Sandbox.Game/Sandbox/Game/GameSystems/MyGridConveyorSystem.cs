namespace Sandbox.Game.GameSystems
{
    using ParallelTasks;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Algorithms;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Groups;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGridConveyorSystem
    {
        private static readonly float CONVEYOR_SYSTEM_CONSUMPTION = 0.005f;
        private readonly HashSet<MyCubeBlock> m_inventoryBlocks = new HashSet<MyCubeBlock>();
        private readonly HashSet<IMyConveyorEndpointBlock> m_conveyorEndpointBlocks = new HashSet<IMyConveyorEndpointBlock>();
        private readonly HashSet<MyConveyorLine> m_lines = new HashSet<MyConveyorLine>();
        private readonly HashSet<MyShipConnector> m_connectors = new HashSet<MyShipConnector>();
        [CompilerGenerated]
        private Action<MyCubeBlock> BlockAdded;
        [CompilerGenerated]
        private Action<MyCubeBlock> BlockRemoved;
        [CompilerGenerated]
        private Action<IMyConveyorEndpointBlock> OnBeforeRemoveEndpointBlock;
        [CompilerGenerated]
        private Action<IMyConveyorSegmentBlock> OnBeforeRemoveSegmentBlock;
        private MyCubeGrid m_grid;
        private bool m_needsRecomputation = true;
        private HashSet<MyCubeGrid> m_tmpConnectedGrids = new HashSet<MyCubeGrid>();
        [ThreadStatic]
        private static List<MyPhysicalInventoryItem> m_tmpInventoryItems;
        [ThreadStatic]
        private static PullRequestItemSet m_tmpRequestedItemSetPerThread;
        [ThreadStatic]
        private static MyPathFindingSystem<IMyConveyorEndpoint> m_pathfinding = new MyPathFindingSystem<IMyConveyorEndpoint>(0x80, null);
        private static Dictionary<Tuple<IMyConveyorEndpointBlock, IMyConveyorEndpointBlock>, Task> m_currentTransferComputationTasks = new Dictionary<Tuple<IMyConveyorEndpointBlock, IMyConveyorEndpointBlock>, Task>();
        private Dictionary<ConveyorLinePosition, MyConveyorLine> m_lineEndpoints;
        private Dictionary<Vector3I, MyConveyorLine> m_linePoints;
        private HashSet<MyConveyorLine> m_deserializedLines;
        public bool IsClosing;
        public MyStringId HudMessage = MyStringId.NullOrEmpty;
        [ThreadStatic]
        private static long m_playerIdForAccessiblePredicate;
        [ThreadStatic]
        private static MyDefinitionId m_inventoryItemDefinitionId;
        private static Predicate<IMyConveyorEndpoint> IsAccessAllowedPredicate = new Predicate<IMyConveyorEndpoint>(MyGridConveyorSystem.IsAccessAllowed);
        private static Predicate<IMyPathEdge<IMyConveyorEndpoint>> IsConveyorLargePredicate = new Predicate<IMyPathEdge<IMyConveyorEndpoint>>(MyGridConveyorSystem.IsConveyorLarge);
        private static Predicate<IMyPathEdge<IMyConveyorEndpoint>> IsConveyorSmallPredicate = new Predicate<IMyPathEdge<IMyConveyorEndpoint>>(MyGridConveyorSystem.IsConveyorSmall);
        [ThreadStatic]
        private static List<IMyConveyorEndpoint> m_reachableBuffer;
        private Dictionary<IMyConveyorEndpointBlock, ConveyorEndpointMapping> m_conveyorConnections = new Dictionary<IMyConveyorEndpointBlock, ConveyorEndpointMapping>();
        private bool m_isRecomputingGraph;
        private bool m_isRecomputationInterrupted;
        private bool m_isRecomputationIsAborted;
        private const double MAX_RECOMPUTE_DURATION_MILLISECONDS = 10.0;
        private Dictionary<IMyConveyorEndpointBlock, ConveyorEndpointMapping> m_conveyorConnectionsForThread = new Dictionary<IMyConveyorEndpointBlock, ConveyorEndpointMapping>();
        private IEnumerator<IMyConveyorEndpointBlock> m_endpointIterator;
        private FastResourceLock m_iteratorLock = new FastResourceLock();
        public bool NeedsUpdateLines;

        public event Action<MyCubeBlock> BlockAdded
        {
            [CompilerGenerated] add
            {
                Action<MyCubeBlock> blockAdded = this.BlockAdded;
                while (true)
                {
                    Action<MyCubeBlock> a = blockAdded;
                    Action<MyCubeBlock> action3 = (Action<MyCubeBlock>) Delegate.Combine(a, value);
                    blockAdded = Interlocked.CompareExchange<Action<MyCubeBlock>>(ref this.BlockAdded, action3, a);
                    if (ReferenceEquals(blockAdded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeBlock> blockAdded = this.BlockAdded;
                while (true)
                {
                    Action<MyCubeBlock> source = blockAdded;
                    Action<MyCubeBlock> action3 = (Action<MyCubeBlock>) Delegate.Remove(source, value);
                    blockAdded = Interlocked.CompareExchange<Action<MyCubeBlock>>(ref this.BlockAdded, action3, source);
                    if (ReferenceEquals(blockAdded, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCubeBlock> BlockRemoved
        {
            [CompilerGenerated] add
            {
                Action<MyCubeBlock> blockRemoved = this.BlockRemoved;
                while (true)
                {
                    Action<MyCubeBlock> a = blockRemoved;
                    Action<MyCubeBlock> action3 = (Action<MyCubeBlock>) Delegate.Combine(a, value);
                    blockRemoved = Interlocked.CompareExchange<Action<MyCubeBlock>>(ref this.BlockRemoved, action3, a);
                    if (ReferenceEquals(blockRemoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeBlock> blockRemoved = this.BlockRemoved;
                while (true)
                {
                    Action<MyCubeBlock> source = blockRemoved;
                    Action<MyCubeBlock> action3 = (Action<MyCubeBlock>) Delegate.Remove(source, value);
                    blockRemoved = Interlocked.CompareExchange<Action<MyCubeBlock>>(ref this.BlockRemoved, action3, source);
                    if (ReferenceEquals(blockRemoved, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<IMyConveyorEndpointBlock> OnBeforeRemoveEndpointBlock
        {
            [CompilerGenerated] add
            {
                Action<IMyConveyorEndpointBlock> onBeforeRemoveEndpointBlock = this.OnBeforeRemoveEndpointBlock;
                while (true)
                {
                    Action<IMyConveyorEndpointBlock> a = onBeforeRemoveEndpointBlock;
                    Action<IMyConveyorEndpointBlock> action3 = (Action<IMyConveyorEndpointBlock>) Delegate.Combine(a, value);
                    onBeforeRemoveEndpointBlock = Interlocked.CompareExchange<Action<IMyConveyorEndpointBlock>>(ref this.OnBeforeRemoveEndpointBlock, action3, a);
                    if (ReferenceEquals(onBeforeRemoveEndpointBlock, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<IMyConveyorEndpointBlock> onBeforeRemoveEndpointBlock = this.OnBeforeRemoveEndpointBlock;
                while (true)
                {
                    Action<IMyConveyorEndpointBlock> source = onBeforeRemoveEndpointBlock;
                    Action<IMyConveyorEndpointBlock> action3 = (Action<IMyConveyorEndpointBlock>) Delegate.Remove(source, value);
                    onBeforeRemoveEndpointBlock = Interlocked.CompareExchange<Action<IMyConveyorEndpointBlock>>(ref this.OnBeforeRemoveEndpointBlock, action3, source);
                    if (ReferenceEquals(onBeforeRemoveEndpointBlock, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<IMyConveyorSegmentBlock> OnBeforeRemoveSegmentBlock
        {
            [CompilerGenerated] add
            {
                Action<IMyConveyorSegmentBlock> onBeforeRemoveSegmentBlock = this.OnBeforeRemoveSegmentBlock;
                while (true)
                {
                    Action<IMyConveyorSegmentBlock> a = onBeforeRemoveSegmentBlock;
                    Action<IMyConveyorSegmentBlock> action3 = (Action<IMyConveyorSegmentBlock>) Delegate.Combine(a, value);
                    onBeforeRemoveSegmentBlock = Interlocked.CompareExchange<Action<IMyConveyorSegmentBlock>>(ref this.OnBeforeRemoveSegmentBlock, action3, a);
                    if (ReferenceEquals(onBeforeRemoveSegmentBlock, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<IMyConveyorSegmentBlock> onBeforeRemoveSegmentBlock = this.OnBeforeRemoveSegmentBlock;
                while (true)
                {
                    Action<IMyConveyorSegmentBlock> source = onBeforeRemoveSegmentBlock;
                    Action<IMyConveyorSegmentBlock> action3 = (Action<IMyConveyorSegmentBlock>) Delegate.Remove(source, value);
                    onBeforeRemoveSegmentBlock = Interlocked.CompareExchange<Action<IMyConveyorSegmentBlock>>(ref this.OnBeforeRemoveSegmentBlock, action3, source);
                    if (ReferenceEquals(onBeforeRemoveSegmentBlock, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGridConveyorSystem(MyCubeGrid grid)
        {
            this.m_grid = grid;
            this.m_lineEndpoints = null;
            this.m_linePoints = null;
            this.m_deserializedLines = null;
            this.ResourceSink = new MyResourceSinkComponent(1);
            this.ResourceSink.Init(MyStringHash.GetOrCompute("Conveyors"), CONVEYOR_SYSTEM_CONSUMPTION, new Func<float>(this.CalculateConsumption));
            this.ResourceSink.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            this.ResourceSink.Update();
        }

        public void Add(MyCubeBlock block)
        {
            this.m_inventoryBlocks.Add(block);
            Action<MyCubeBlock> blockAdded = this.BlockAdded;
            if (blockAdded != null)
            {
                blockAdded(block);
            }
        }

        public void AddConveyorBlock(IMyConveyorEndpointBlock endpointBlock)
        {
            using (this.m_iteratorLock.AcquireExclusiveUsing())
            {
                this.m_endpointIterator = null;
                this.m_conveyorEndpointBlocks.Add(endpointBlock);
                if (endpointBlock is MyShipConnector)
                {
                    this.m_connectors.Add(endpointBlock as MyShipConnector);
                }
                IMyConveyorEndpoint conveyorEndpoint = endpointBlock.ConveyorEndpoint;
                for (int i = 0; i < conveyorEndpoint.GetLineCount(); i++)
                {
                    ConveyorLinePosition endpointPosition = conveyorEndpoint.GetPosition(i);
                    MyConveyorLine conveyorLine = conveyorEndpoint.GetConveyorLine(i);
                    if ((this.m_deserializedLines == null) || !this.m_deserializedLines.Contains(conveyorLine))
                    {
                        MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(endpointPosition.NeighbourGridPosition);
                        if (cubeBlock == null)
                        {
                            this.m_lines.Add(conveyorLine);
                        }
                        else
                        {
                            IMyConveyorEndpointBlock fatBlock = cubeBlock.FatBlock as IMyConveyorEndpointBlock;
                            IMyConveyorSegmentBlock segmentBlock = cubeBlock.FatBlock as IMyConveyorSegmentBlock;
                            if (segmentBlock != null)
                            {
                                if (!this.TryMergeEndpointSegment(endpointBlock, segmentBlock, endpointPosition))
                                {
                                    this.m_lines.Add(conveyorLine);
                                }
                            }
                            else if (fatBlock == null)
                            {
                                this.m_lines.Add(conveyorLine);
                            }
                            else if (!this.TryMergeEndpointEndpoint(endpointBlock, fatBlock, endpointPosition, endpointPosition.GetConnectingPosition()))
                            {
                                this.m_lines.Add(conveyorLine);
                            }
                        }
                    }
                }
            }
        }

        private static void AddReachableEndpoints(IMyConveyorEndpointBlock processedBlock, List<IMyConveyorEndpointBlock> resultList, MyInventoryFlags flagToCheck, MyDefinitionId? definitionId = new MyDefinitionId?())
        {
            foreach (IMyConveyorEndpoint endpoint in Pathfinding)
            {
                if ((!ReferenceEquals(endpoint.CubeBlock, processedBlock) || processedBlock.AllowSelfPulling()) && ((endpoint.CubeBlock != null) && endpoint.CubeBlock.HasInventory))
                {
                    IMyConveyorEndpointBlock cubeBlock = endpoint.CubeBlock as IMyConveyorEndpointBlock;
                    if (cubeBlock != null)
                    {
                        MyCubeBlock thisEntity = endpoint.CubeBlock;
                        bool flag = false;
                        int index = 0;
                        while (true)
                        {
                            if (index < thisEntity.InventoryCount)
                            {
                                MyInventory inventory = thisEntity.GetInventory(index);
                                if (((inventory.GetFlags() & flagToCheck) == 0) || ((definitionId != null) && !inventory.CheckConstraint(definitionId.Value)))
                                {
                                    index++;
                                    continue;
                                }
                                flag = true;
                            }
                            if (flag && !resultList.Contains(cubeBlock))
                            {
                                resultList.Add(cubeBlock);
                            }
                            break;
                        }
                    }
                }
            }
        }

        public void AddSegmentBlock(IMyConveyorSegmentBlock segmentBlock)
        {
            this.AddSegmentBlockInternal(segmentBlock, segmentBlock.ConveyorSegment.ConnectingPosition1);
            this.AddSegmentBlockInternal(segmentBlock, segmentBlock.ConveyorSegment.ConnectingPosition2);
            if (!this.m_lines.Contains(segmentBlock.ConveyorSegment.ConveyorLine) && (segmentBlock.ConveyorSegment.ConveyorLine != null))
            {
                this.m_lines.Add(segmentBlock.ConveyorSegment.ConveyorLine);
            }
        }

        private void AddSegmentBlockInternal(IMyConveyorSegmentBlock segmentBlock, ConveyorLinePosition connectingPosition)
        {
            MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(connectingPosition.LocalGridPosition);
            if ((cubeBlock != null) && ((this.m_deserializedLines == null) || !this.m_deserializedLines.Contains(segmentBlock.ConveyorSegment.ConveyorLine)))
            {
                IMyConveyorEndpointBlock fatBlock = cubeBlock.FatBlock as IMyConveyorEndpointBlock;
                IMyConveyorSegmentBlock oldSegmentBlock = cubeBlock.FatBlock as IMyConveyorSegmentBlock;
                if (oldSegmentBlock != null)
                {
                    MyConveyorLine conveyorLine = segmentBlock.ConveyorSegment.ConveyorLine;
                    if (this.m_lines.Contains(conveyorLine))
                    {
                        this.m_lines.Remove(conveyorLine);
                    }
                    if (oldSegmentBlock.ConveyorSegment.CanConnectTo(connectingPosition, segmentBlock.ConveyorSegment.ConveyorLine.Type))
                    {
                        this.MergeSegmentSegment(segmentBlock, oldSegmentBlock);
                    }
                }
                if (fatBlock != null)
                {
                    MyConveyorLine conveyorLine = fatBlock.ConveyorEndpoint.GetConveyorLine(connectingPosition);
                    if (this.TryMergeEndpointSegment(fatBlock, segmentBlock, connectingPosition))
                    {
                        this.m_lines.Remove(conveyorLine);
                    }
                }
            }
        }

        public void AfterBlockDeserialization()
        {
            this.m_lineEndpoints = null;
            this.m_linePoints = null;
            this.m_deserializedLines = null;
            using (HashSet<MyConveyorLine>.Enumerator enumerator = this.m_lines.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateIsFunctional();
                }
            }
        }

        public void AfterGridClose()
        {
            this.m_lines.Clear();
        }

        public static void AppendReachableEndpoints(IMyConveyorEndpoint source, long playerId, List<IMyConveyorEndpoint> reachable, MyPhysicalInventoryItem item, Predicate<IMyConveyorEndpoint> endpointFilter = null)
        {
            IMyConveyorEndpointBlock cubeBlock = source.CubeBlock as IMyConveyorEndpointBlock;
            if (cubeBlock != null)
            {
                MyPathFindingSystem<IMyConveyorEndpoint> pathfinding = Pathfinding;
                lock (pathfinding)
                {
                    SetTraversalPlayerId(playerId);
                    MyDefinitionId id = item.Content.GetId();
                    SetTraversalInventoryItemDefinitionId(id);
                    Pathfinding.FindReachable(cubeBlock.ConveyorEndpoint, reachable, endpointFilter, IsAccessAllowedPredicate, NeedsLargeTube(id) ? IsConveyorLargePredicate : null);
                }
            }
        }

        public void BeforeBlockDeserialization(List<MyObjectBuilder_ConveyorLine> lines)
        {
            if (lines != null)
            {
                this.m_lineEndpoints = new Dictionary<ConveyorLinePosition, MyConveyorLine>(lines.Count * 2);
                this.m_linePoints = new Dictionary<Vector3I, MyConveyorLine>(lines.Count * 4);
                this.m_deserializedLines = new HashSet<MyConveyorLine>();
                foreach (MyObjectBuilder_ConveyorLine line in lines)
                {
                    MyConveyorLine line2 = new MyConveyorLine();
                    line2.Init(line, this.m_grid);
                    if (line2.CheckSectionConsistency())
                    {
                        ConveyorLinePosition key = new ConveyorLinePosition((Vector3I) line.StartPosition, line.StartDirection);
                        ConveyorLinePosition position2 = new ConveyorLinePosition((Vector3I) line.EndPosition, line.EndDirection);
                        try
                        {
                            this.m_lineEndpoints.Add(key, line2);
                            this.m_lineEndpoints.Add(position2, line2);
                            foreach (Vector3I vectori in line2)
                            {
                                this.m_linePoints.Add(vectori, line2);
                            }
                            this.m_deserializedLines.Add(line2);
                            this.m_lines.Add(line2);
                            continue;
                        }
                        catch (ArgumentException)
                        {
                            this.m_lineEndpoints = null;
                            this.m_deserializedLines = null;
                            this.m_linePoints = null;
                            this.m_lines.Clear();
                        }
                        break;
                    }
                }
            }
        }

        public float CalculateConsumption()
        {
            float num = 0f;
            using (HashSet<MyConveyorLine>.Enumerator enumerator = this.m_lines.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!enumerator.Current.IsFunctional)
                    {
                        continue;
                    }
                    num += 1E-07f;
                }
            }
            return num;
        }

        private static bool CanTransfer(IMyConveyorEndpointBlock start, IMyConveyorEndpointBlock endPoint, MyDefinitionId itemId, bool isPush)
        {
            ConveyorEndpointMapping conveyorEndpointMapping = (start as MyCubeBlock).CubeGrid.GridSystems.ConveyorSystem.GetConveyorEndpointMapping(start);
            if (0 != 0)
            {
                TransferData data1 = new TransferData(start, endPoint, itemId, isPush);
                data1.ComputeTransfer();
                return data1.m_canTransfer;
            }
            bool canTransfer = true;
            if (conveyorEndpointMapping.TryGetTransfer(endPoint, itemId, isPush, out canTransfer))
            {
                return canTransfer;
            }
            Tuple<IMyConveyorEndpointBlock, IMyConveyorEndpointBlock> key = new Tuple<IMyConveyorEndpointBlock, IMyConveyorEndpointBlock>(start, endPoint);
            Dictionary<Tuple<IMyConveyorEndpointBlock, IMyConveyorEndpointBlock>, Task> currentTransferComputationTasks = m_currentTransferComputationTasks;
            lock (currentTransferComputationTasks)
            {
                if (!m_currentTransferComputationTasks.ContainsKey(key))
                {
                    TransferData workData = new TransferData(start, endPoint, itemId, isPush);
                    m_currentTransferComputationTasks.Add(key, Parallel.Start(new Action<WorkData>(MyGridConveyorSystem.ComputeTransferData), new Action<WorkData>(MyGridConveyorSystem.OnTransferDataComputed), workData));
                }
            }
            return false;
        }

        public static bool ComputeCanTransfer(IMyConveyorEndpointBlock start, IMyConveyorEndpointBlock end, MyDefinitionId? itemId)
        {
            using (MyUtils.ReuseCollection<IMyConveyorEndpoint>(ref m_reachableBuffer))
            {
                MyPathFindingSystem<IMyConveyorEndpoint> pathfinding = Pathfinding;
                lock (pathfinding)
                {
                    SetTraversalPlayerId(start.ConveyorEndpoint.CubeBlock.OwnerId);
                    if (itemId != null)
                    {
                        SetTraversalInventoryItemDefinitionId(itemId.Value);
                    }
                    else
                    {
                        MyDefinitionId item = new MyDefinitionId();
                        SetTraversalInventoryItemDefinitionId(item);
                    }
                    Predicate<IMyPathEdge<IMyConveyorEndpoint>> edgeTraversable = null;
                    if ((itemId != null) && NeedsLargeTube(itemId.Value))
                    {
                        edgeTraversable = IsConveyorLargePredicate;
                    }
                    Pathfinding.FindReachable(start.ConveyorEndpoint, m_reachableBuffer, b => (b != null) && ReferenceEquals(b.CubeBlock, end), IsAccessAllowedPredicate, edgeTraversable);
                }
                return (m_reachableBuffer.Count != 0);
            }
        }

        private ConveyorEndpointMapping ComputeMappingForBlock(IMyConveyorEndpointBlock processedBlock)
        {
            PullInformation information2;
            MyPathFindingSystem<IMyConveyorEndpoint> pathfinding;
            MyDefinitionId id;
            MyDefinitionId? nullable;
            ConveyorEndpointMapping mapping = new ConveyorEndpointMapping();
            PullInformation pullInformation = processedBlock.GetPullInformation();
            if (pullInformation != null)
            {
                mapping.pullElements = new List<IMyConveyorEndpointBlock>();
                pathfinding = Pathfinding;
                lock (pathfinding)
                {
                    SetTraversalPlayerId(pullInformation.OwnerID);
                    id = new MyDefinitionId();
                    if (pullInformation.ItemDefinition != id)
                    {
                        SetTraversalInventoryItemDefinitionId(pullInformation.ItemDefinition);
                        using (new MyConveyorLine.InvertedConductivity())
                        {
                            PrepareTraversal(processedBlock.ConveyorEndpoint, null, IsAccessAllowedPredicate, NeedsLargeTube(pullInformation.ItemDefinition) ? IsConveyorLargePredicate : null);
                            nullable = null;
                            AddReachableEndpoints(processedBlock, mapping.pullElements, MyInventoryFlags.CanSend, nullable);
                            goto TR_0025;
                        }
                    }
                    if (pullInformation.Constraint != null)
                    {
                        id = new MyDefinitionId();
                        SetTraversalInventoryItemDefinitionId(id);
                        using (new MyConveyorLine.InvertedConductivity())
                        {
                            PrepareTraversal(processedBlock.ConveyorEndpoint, null, IsAccessAllowedPredicate, null);
                            nullable = null;
                            AddReachableEndpoints(processedBlock, mapping.pullElements, MyInventoryFlags.CanSend, nullable);
                        }
                    }
                }
            }
        TR_0025:
            information2 = processedBlock.GetPushInformation();
            if (information2 != null)
            {
                mapping.pushElements = new List<IMyConveyorEndpointBlock>();
                pathfinding = Pathfinding;
                lock (pathfinding)
                {
                    SetTraversalPlayerId(information2.OwnerID);
                    HashSet<MyDefinitionId> definitions = new HashSet<MyDefinitionId>();
                    id = new MyDefinitionId();
                    if (information2.ItemDefinition != id)
                    {
                        definitions.Add(information2.ItemDefinition);
                    }
                    if (information2.Constraint != null)
                    {
                        foreach (MyDefinitionId id2 in information2.Constraint.ConstrainedIds)
                        {
                            definitions.Add(id2);
                        }
                        foreach (MyObjectBuilderType type in information2.Constraint.ConstrainedTypes)
                        {
                            MyDefinitionManager.Static.TryGetDefinitionsByTypeId(type, definitions);
                        }
                    }
                    if ((definitions.Count == 0) && ((information2.Constraint == null) || (information2.Constraint.Description == "Empty constraint")))
                    {
                        id = new MyDefinitionId();
                        SetTraversalInventoryItemDefinitionId(id);
                        PrepareTraversal(processedBlock.ConveyorEndpoint, null, IsAccessAllowedPredicate, null);
                        nullable = null;
                        AddReachableEndpoints(processedBlock, mapping.pushElements, MyInventoryFlags.CanReceive, nullable);
                    }
                    else
                    {
                        foreach (MyDefinitionId id3 in definitions)
                        {
                            SetTraversalInventoryItemDefinitionId(id3);
                            if (NeedsLargeTube(id3))
                            {
                                PrepareTraversal(processedBlock.ConveyorEndpoint, null, IsAccessAllowedPredicate, IsConveyorLargePredicate);
                            }
                            else
                            {
                                PrepareTraversal(processedBlock.ConveyorEndpoint, null, IsAccessAllowedPredicate, null);
                            }
                            AddReachableEndpoints(processedBlock, mapping.pushElements, MyInventoryFlags.CanReceive, new MyDefinitionId?(id3));
                        }
                    }
                }
            }
            return mapping;
        }

        private static void ComputeTransferData(WorkData workData)
        {
            TransferData data = workData as TransferData;
            if (data == null)
            {
                workData.FlagAsFailed();
            }
            else
            {
                data.ComputeTransfer();
            }
        }

        public static MyFixedPoint ConveyorSystemItemAmount(IMyConveyorEndpointBlock start, MyInventory destinationInventory, long playerId, MyDefinitionId itemId)
        {
            MyFixedPoint point = 0;
            using (new MyConveyorLine.InvertedConductivity())
            {
                MyPathFindingSystem<IMyConveyorEndpoint> pathfinding = Pathfinding;
                lock (pathfinding)
                {
                    SetTraversalPlayerId(playerId);
                    SetTraversalInventoryItemDefinitionId(itemId);
                    PrepareTraversal(start.ConveyorEndpoint, null, IsAccessAllowedPredicate, NeedsLargeTube(itemId) ? IsConveyorLargePredicate : null);
                    foreach (IMyConveyorEndpoint endpoint in Pathfinding)
                    {
                        MyCubeBlock cubeBlock;
                        if ((endpoint.CubeBlock == null) || !endpoint.CubeBlock.HasInventory)
                        {
                            cubeBlock = null;
                        }
                        else
                        {
                            cubeBlock = endpoint.CubeBlock;
                        }
                        MyCubeBlock thisEntity = cubeBlock;
                        if (thisEntity != null)
                        {
                            for (int i = 0; i < thisEntity.InventoryCount; i++)
                            {
                                MyInventory objA = thisEntity.GetInventory(i);
                                if (((objA.GetFlags() & MyInventoryFlags.CanSend) != 0) && !ReferenceEquals(objA, destinationInventory))
                                {
                                    point += objA.GetItemAmount(itemId, MyItemFlags.None, false);
                                }
                            }
                        }
                    }
                }
            }
            return point;
        }

        public void DebugDraw(MyCubeGrid grid)
        {
            using (HashSet<MyConveyorLine>.Enumerator enumerator = this.m_lines.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.DebugDraw(grid);
                }
            }
            MyRenderProxy.DebugDrawText2D(new Vector2(1f, 1f), "Conveyor lines: " + this.m_lines.Count, Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
        }

        public void DebugDrawLinePackets()
        {
            using (HashSet<MyConveyorLine>.Enumerator enumerator = this.m_lines.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.DebugDrawPackets();
                }
            }
        }

        public static void FindReachable(IMyConveyorEndpoint from, List<IMyConveyorEndpoint> reachableVertices, Predicate<IMyConveyorEndpoint> vertexFilter = null, Predicate<IMyConveyorEndpoint> vertexTraversable = null, Predicate<IMyPathEdge<IMyConveyorEndpoint>> edgeTraversable = null)
        {
            MyPathFindingSystem<IMyConveyorEndpoint> pathfinding = Pathfinding;
            lock (pathfinding)
            {
                Pathfinding.FindReachable(from, reachableVertices, vertexFilter, vertexTraversable, edgeTraversable);
            }
        }

        public void FlagForRecomputation()
        {
            MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Group group = MyGridPhysicalHierarchy.Static.GetGroup(this.m_grid);
            if (group != null)
            {
                using (HashSet<MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node>.Enumerator enumerator = group.Nodes.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.NodeData.GridSystems.ConveyorSystem.m_needsRecomputation = true;
                    }
                }
            }
        }

        public ConveyorEndpointMapping GetConveyorEndpointMapping(IMyConveyorEndpointBlock block) => 
            (!this.m_conveyorConnections.ContainsKey(block) ? new ConveyorEndpointMapping() : this.m_conveyorConnections[block]);

        public MyConveyorLine GetDeserializingLine(ConveyorLinePosition position)
        {
            MyConveyorLine line;
            if (this.m_lineEndpoints == null)
            {
                return null;
            }
            this.m_lineEndpoints.TryGetValue(position, out line);
            return line;
        }

        public MyConveyorLine GetDeserializingLine(Vector3I position)
        {
            MyConveyorLine line;
            if (this.m_linePoints == null)
            {
                return null;
            }
            this.m_linePoints.TryGetValue(position, out line);
            return line;
        }

        private static bool IsAccessAllowed(IMyConveyorEndpoint endpoint)
        {
            if (endpoint.CubeBlock.GetUserRelationToOwner(m_playerIdForAccessiblePredicate) == MyRelationsBetweenPlayerAndBlock.Enemies)
            {
                return false;
            }
            MyConveyorSorter cubeBlock = endpoint.CubeBlock as MyConveyorSorter;
            if (cubeBlock == null)
            {
                return true;
            }
            MyDefinitionId id = new MyDefinitionId();
            return (!(m_inventoryItemDefinitionId != id) || cubeBlock.IsAllowed(m_inventoryItemDefinitionId));
        }

        private static bool IsConveyorLarge(IMyPathEdge<IMyConveyorEndpoint> conveyorLine) => 
            (!(conveyorLine is MyConveyorLine) || ((conveyorLine as MyConveyorLine).Type == MyObjectBuilder_ConveyorLine.LineType.LARGE_LINE));

        private static bool IsConveyorSmall(IMyPathEdge<IMyConveyorEndpoint> conveyorLine) => 
            (!(conveyorLine is MyConveyorLine) || ((conveyorLine as MyConveyorLine).Type == MyObjectBuilder_ConveyorLine.LineType.SMALL_LINE));

        private static bool ItemPullAll(IMyConveyorEndpointBlock start, MyInventory destinationInventory)
        {
            int num;
            MyCubeBlock block2;
            int inventoryCount;
            int num3;
            MyCubeBlock block = start as MyCubeBlock;
            if (block == null)
            {
                return false;
            }
            bool flag = false;
            MyGridConveyorSystem conveyorSystem = block.CubeGrid.GridSystems.ConveyorSystem;
            ConveyorEndpointMapping conveyorEndpointMapping = conveyorSystem.GetConveyorEndpointMapping(start);
            if (conveyorEndpointMapping.pullElements == null)
            {
                if (!conveyorSystem.m_isRecomputingGraph)
                {
                    conveyorSystem.RecomputeConveyorEndpoints();
                }
                return flag;
            }
            else
            {
                num = 0;
            }
            goto TR_0018;
        TR_0002:
            num++;
            goto TR_0018;
        TR_0003:
            if (destinationInventory.CargoPercentage < 0.99f)
            {
                goto TR_0002;
            }
            return flag;
        TR_0004:
            num3++;
        TR_0013:
            while (true)
            {
                if (num3 < inventoryCount)
                {
                    MyInventory objA = block2.GetInventory(num3);
                    if ((objA.GetFlags() & MyInventoryFlags.CanSend) == 0)
                    {
                        goto TR_0004;
                    }
                    else if (ReferenceEquals(objA, destinationInventory))
                    {
                        goto TR_0004;
                    }
                    else
                    {
                        MyPhysicalInventoryItem[] itemArray = objA.GetItems().ToArray();
                        for (int i = 0; i < itemArray.Length; i++)
                        {
                            MyDefinitionId definitionId = itemArray[i].GetDefinitionId();
                            MyFixedPoint b = destinationInventory.ComputeAmountThatFits(definitionId, 0f, 0f);
                            if ((b > 0) && CanTransfer(start, conveyorEndpointMapping.pullElements[num], definitionId, false))
                            {
                                MyInventory.Transfer(objA, destinationInventory, definitionId, MyItemFlags.None, new MyFixedPoint?(MyFixedPoint.Min(objA.GetItemAmount(definitionId, MyItemFlags.None, false), b)), false);
                                flag = true;
                                if (destinationInventory.CargoPercentage >= 0.99f)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    goto TR_0003;
                }
                break;
            }
            if (destinationInventory.CargoPercentage >= 0.99f)
            {
                goto TR_0003;
            }
            goto TR_0004;
        TR_0018:
            while (true)
            {
                if (num >= conveyorEndpointMapping.pullElements.Count)
                {
                    break;
                }
                block2 = conveyorEndpointMapping.pullElements[num] as MyCubeBlock;
                if (block2 != null)
                {
                    inventoryCount = block2.InventoryCount;
                    num3 = 0;
                    goto TR_0013;
                }
                goto TR_0002;
            }
            return flag;
        }

        public static MyFixedPoint ItemPullRequest(IMyConveyorEndpointBlock start, MyInventory destinationInventory, long playerId, MyDefinitionId itemId, MyFixedPoint? amount = new MyFixedPoint?(), bool remove = false)
        {
            int num;
            MyCubeBlock block = start as MyCubeBlock;
            if (block == null)
            {
                return 0;
            }
            MyFixedPoint point = 0;
            MyGridConveyorSystem conveyorSystem = block.CubeGrid.GridSystems.ConveyorSystem;
            ConveyorEndpointMapping conveyorEndpointMapping = conveyorSystem.GetConveyorEndpointMapping(start);
            if (conveyorEndpointMapping.pullElements == null)
            {
                if (!conveyorSystem.m_isRecomputingGraph)
                {
                    conveyorSystem.RecomputeConveyorEndpoints();
                }
                return point;
            }
            else
            {
                num = 0;
            }
            goto TR_001C;
        TR_0002:
            num++;
            goto TR_001C;
        TR_0003:
            if (destinationInventory.CargoPercentage < 0.99f)
            {
                goto TR_0002;
            }
            return point;
        TR_001C:
            while (true)
            {
                if (num >= conveyorEndpointMapping.pullElements.Count)
                {
                    break;
                }
                MyCubeBlock thisEntity = conveyorEndpointMapping.pullElements[num] as MyCubeBlock;
                if (thisEntity != null)
                {
                    int inventoryCount = thisEntity.InventoryCount;
                    int index = 0;
                    while (true)
                    {
                        while (true)
                        {
                            if (index < inventoryCount)
                            {
                                MyInventory objA = thisEntity.GetInventory(index);
                                if ((((objA.GetFlags() & MyInventoryFlags.CanSend) != 0) && !ReferenceEquals(objA, destinationInventory)) && CanTransfer(start, conveyorEndpointMapping.pullElements[num], itemId, false))
                                {
                                    MyFixedPoint point2 = objA.GetItemAmount(itemId, MyItemFlags.None, false);
                                    if (amount == null)
                                    {
                                        point = !remove ? (point + MyInventory.Transfer(objA, destinationInventory, itemId, MyItemFlags.None, new MyFixedPoint?(point2), false)) : (point + objA.RemoveItemsOfType(point2, itemId, MyItemFlags.None, false));
                                    }
                                    else
                                    {
                                        MyFixedPoint? nullable1;
                                        point2 = (amount != null) ? MyFixedPoint.Min(point2, amount.Value) : point2;
                                        if (point2 == 0)
                                        {
                                            break;
                                        }
                                        point = !remove ? (point + MyInventory.Transfer(objA, destinationInventory, itemId, MyItemFlags.None, new MyFixedPoint?(point2), false)) : (point + objA.RemoveItemsOfType(point2, itemId, MyItemFlags.None, false));
                                        MyFixedPoint? nullable = amount;
                                        MyFixedPoint point3 = point2;
                                        if (nullable != null)
                                        {
                                            nullable1 = new MyFixedPoint?(nullable.GetValueOrDefault() - point3);
                                        }
                                        else
                                        {
                                            nullable1 = null;
                                        }
                                        amount = nullable1;
                                        if (amount.Value == 0)
                                        {
                                            return point;
                                        }
                                    }
                                    if (destinationInventory.CargoPercentage >= 0.99f)
                                    {
                                        goto TR_0003;
                                    }
                                }
                            }
                            else
                            {
                                goto TR_0003;
                            }
                            break;
                        }
                        index++;
                    }
                }
                goto TR_0002;
            }
            return point;
        }

        public static bool ItemPushRequest(IMyConveyorEndpointBlock start, MyInventory srcInventory, long playerId, MyPhysicalInventoryItem toSend, MyFixedPoint? amount = new MyFixedPoint?())
        {
            MyDefinitionId id;
            MyFixedPoint point;
            int num;
            MyCubeBlock block = start as MyCubeBlock;
            if (block == null)
            {
                return false;
            }
            bool flag = false;
            MyGridConveyorSystem conveyorSystem = block.CubeGrid.GridSystems.ConveyorSystem;
            ConveyorEndpointMapping conveyorEndpointMapping = conveyorSystem.GetConveyorEndpointMapping(start);
            if (conveyorEndpointMapping.pushElements == null)
            {
                if (!conveyorSystem.m_isRecomputingGraph)
                {
                    conveyorSystem.RecomputeConveyorEndpoints();
                }
                return flag;
            }
            else
            {
                id = toSend.Content.GetId();
                point = toSend.Amount;
                if (amount != null)
                {
                    point = amount.Value;
                }
                num = 0;
            }
            goto TR_0013;
        TR_0002:
            num++;
        TR_0013:
            while (true)
            {
                if (num >= conveyorEndpointMapping.pushElements.Count)
                {
                    break;
                }
                MyCubeBlock thisEntity = conveyorEndpointMapping.pushElements[num] as MyCubeBlock;
                if (thisEntity != null)
                {
                    int inventoryCount = thisEntity.InventoryCount;
                    int index = 0;
                    while (true)
                    {
                        if (index < inventoryCount)
                        {
                            MyInventory objA = thisEntity.GetInventory(index);
                            if (((objA.GetFlags() & MyInventoryFlags.CanReceive) != 0) && !ReferenceEquals(objA, srcInventory))
                            {
                                MyFixedPoint point2 = MyFixedPoint.Min(objA.ComputeAmountThatFits(id, 0f, 0f), point);
                                if ((objA.CheckConstraint(id) && (point2 != 0)) && CanTransfer(start, conveyorEndpointMapping.pushElements[num], toSend.GetDefinitionId(), true))
                                {
                                    MyInventory.Transfer(srcInventory, objA, toSend.ItemId, -1, new MyFixedPoint?(point2), false);
                                    flag = true;
                                    point -= point2;
                                }
                            }
                            index++;
                            continue;
                        }
                        if (point <= 0)
                        {
                            break;
                        }
                        goto TR_0002;
                    }
                    break;
                }
                goto TR_0002;
            }
            return flag;
        }

        private void MergeSegmentSegment(IMyConveyorSegmentBlock newSegmentBlock, IMyConveyorSegmentBlock oldSegmentBlock)
        {
            MyConveyorLine conveyorLine = newSegmentBlock.ConveyorSegment.ConveyorLine;
            MyConveyorLine objB = oldSegmentBlock.ConveyorSegment.ConveyorLine;
            if (!ReferenceEquals(conveyorLine, objB))
            {
                objB.Merge(conveyorLine, newSegmentBlock);
            }
            this.UpdateLineReferences(conveyorLine, objB);
            newSegmentBlock.ConveyorSegment.SetConveyorLine(objB);
        }

        private static bool NeedsLargeTube(MyDefinitionId itemDefinitionId)
        {
            MyPhysicalItemDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(itemDefinitionId);
            return ((physicalItemDefinition != null) ? ((itemDefinitionId.TypeId != typeof(MyObjectBuilder_PhysicalGunObject)) ? (physicalItemDefinition.Size.AbsMax() > 0.25f) : false) : true);
        }

        private void OnConveyorEndpointMappingUpdateCompleted()
        {
            using (this.m_iteratorLock.AcquireExclusiveUsing())
            {
                if (this.m_isRecomputationIsAborted)
                {
                    this.StartRecomputationThread();
                }
                else
                {
                    foreach (KeyValuePair<IMyConveyorEndpointBlock, ConveyorEndpointMapping> pair in this.m_conveyorConnectionsForThread)
                    {
                        if (this.m_conveyorConnections.ContainsKey(pair.Key))
                        {
                            this.m_conveyorConnections[pair.Key] = pair.Value;
                            continue;
                        }
                        this.m_conveyorConnections.Add(pair.Key, pair.Value);
                    }
                    this.m_conveyorConnectionsForThread.Clear();
                    if (this.m_isRecomputationInterrupted)
                    {
                        Parallel.Start(new Action(this.UpdateConveyorEndpointMapping), new Action(this.OnConveyorEndpointMappingUpdateCompleted));
                    }
                    else
                    {
                        this.m_endpointIterator = null;
                        this.m_isRecomputingGraph = false;
                        using (HashSet<MyConveyorLine>.Enumerator enumerator2 = this.m_lines.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                enumerator2.Current.UpdateIsWorking();
                            }
                        }
                    }
                }
            }
        }

        private static void OnTransferDataComputed(WorkData workData)
        {
            if ((workData == null) && MyFakes.FORCE_NO_WORKER)
            {
                MyLog.Default.WriteLine("OnTransferDataComputed: workData is null on MyGridConveyorSystem to Check");
            }
            else
            {
                TransferData data = workData as TransferData;
                if (data == null)
                {
                    workData.FlagAsFailed();
                }
                else
                {
                    data.StoreTransferState();
                    Tuple<IMyConveyorEndpointBlock, IMyConveyorEndpointBlock> key = new Tuple<IMyConveyorEndpointBlock, IMyConveyorEndpointBlock>(data.m_start, data.m_endPoint);
                    Dictionary<Tuple<IMyConveyorEndpointBlock, IMyConveyorEndpointBlock>, Task> currentTransferComputationTasks = m_currentTransferComputationTasks;
                    lock (currentTransferComputationTasks)
                    {
                        m_currentTransferComputationTasks.Remove(key);
                    }
                }
            }
        }

        public static void PrepareTraversal(IMyConveyorEndpoint startingVertex, Predicate<IMyConveyorEndpoint> vertexFilter = null, Predicate<IMyConveyorEndpoint> vertexTraversable = null, Predicate<IMyPathEdge<IMyConveyorEndpoint>> edgeTraversable = null)
        {
            MyPathFindingSystem<IMyConveyorEndpoint> pathfinding = Pathfinding;
            lock (pathfinding)
            {
                Pathfinding.PrepareTraversal(startingVertex, vertexFilter, vertexTraversable, edgeTraversable);
            }
        }

        public static bool PullAllRequest(IMyConveyorEndpointBlock start, MyInventory destinationInventory, long playerId, bool all)
        {
            SetTraversalPlayerId(playerId);
            m_tmpRequestedItemSet.Set(all);
            m_tmpRequestedItemSet.Clear();
            return ItemPullAll(start, destinationInventory);
        }

        public static bool PullAllRequest(IMyConveyorEndpointBlock start, MyInventory destinationInventory, long playerId, MyObjectBuilderType? typeId = new MyObjectBuilderType?())
        {
            SetTraversalPlayerId(playerId);
            m_tmpRequestedItemSet.Set(typeId);
            m_tmpRequestedItemSet.Clear();
            return ItemPullAll(start, destinationInventory);
        }

        public static bool PullAllRequest(IMyConveyorEndpointBlock start, MyInventory destinationInventory, long playerId, MyInventoryConstraint requestedTypeIds, MyFixedPoint? maxAmount = new MyFixedPoint?(), bool pullFullBottles = true)
        {
            MyCubeBlock block = start as MyCubeBlock;
            if (block == null)
            {
                return false;
            }
            m_tmpRequestedItemSet.Set(requestedTypeIds);
            MyGridConveyorSystem conveyorSystem = block.CubeGrid.GridSystems.ConveyorSystem;
            ConveyorEndpointMapping conveyorEndpointMapping = conveyorSystem.GetConveyorEndpointMapping(start);
            if (conveyorEndpointMapping.pullElements == null)
            {
                if (!conveyorSystem.m_isRecomputingGraph)
                {
                    conveyorSystem.RecomputeConveyorEndpoints();
                }
                return false;
            }
            bool flag = false;
            int num = 0;
            while (true)
            {
                if (num >= conveyorEndpointMapping.pullElements.Count)
                {
                    break;
                }
                MyCubeBlock thisEntity = conveyorEndpointMapping.pullElements[num] as MyCubeBlock;
                if (thisEntity != null)
                {
                    int inventoryCount = thisEntity.InventoryCount;
                    int index = 0;
                    while (true)
                    {
                        if (index >= inventoryCount)
                        {
                            break;
                        }
                        MyInventory objA = thisEntity.GetInventory(index);
                        if (((objA.GetFlags() & MyInventoryFlags.CanSend) != 0) && !ReferenceEquals(objA, destinationInventory))
                        {
                            using (MyUtils.ReuseCollection<MyPhysicalInventoryItem>(ref m_tmpInventoryItems))
                            {
                                foreach (MyPhysicalInventoryItem item in objA.GetItems())
                                {
                                    m_tmpInventoryItems.Add(item);
                                }
                                using (List<MyPhysicalInventoryItem>.Enumerator enumerator = m_tmpInventoryItems.GetEnumerator())
                                {
                                    while (true)
                                    {
                                        if (!enumerator.MoveNext())
                                        {
                                            break;
                                        }
                                        MyPhysicalInventoryItem current = enumerator.Current;
                                        if (destinationInventory.VolumeFillFactor < 1f)
                                        {
                                            MyDefinitionId itemId = current.Content.GetId();
                                            if ((requestedTypeIds != null) && !m_tmpRequestedItemSet.Contains(itemId))
                                            {
                                                continue;
                                            }
                                            if (CanTransfer(start, conveyorEndpointMapping.pullElements[num], itemId, false))
                                            {
                                                MyFixedPoint amount = current.Amount;
                                                MyObjectBuilder_GasContainerObject content = current.Content as MyObjectBuilder_GasContainerObject;
                                                if ((pullFullBottles || (content == null)) || (content.GasLevel < 1f))
                                                {
                                                    if (!MySession.Static.CreativeMode)
                                                    {
                                                        MyFixedPoint a = destinationInventory.ComputeAmountThatFits(current.Content.GetId(), 0f, 0f);
                                                        if (maxAmount != null)
                                                        {
                                                            a = MyFixedPoint.Min(a, maxAmount.Value);
                                                        }
                                                        if ((current.Content.TypeId != typeof(MyObjectBuilder_Ore)) && (current.Content.TypeId != typeof(MyObjectBuilder_Ingot)))
                                                        {
                                                            a = MyFixedPoint.Floor(a);
                                                        }
                                                        amount = MyFixedPoint.Min(a, amount);
                                                    }
                                                    if (amount != 0)
                                                    {
                                                        if (maxAmount != null)
                                                        {
                                                            MyFixedPoint? nullable1;
                                                            MyFixedPoint? nullable = maxAmount;
                                                            MyFixedPoint point3 = amount;
                                                            if (nullable != null)
                                                            {
                                                                nullable1 = new MyFixedPoint?(nullable.GetValueOrDefault() - point3);
                                                            }
                                                            else
                                                            {
                                                                nullable1 = null;
                                                            }
                                                            maxAmount = nullable1;
                                                        }
                                                        flag = true;
                                                        MyInventory.Transfer(objA, destinationInventory, current.Content.GetId(), MyItemFlags.None, new MyFixedPoint?(amount), false);
                                                        if (destinationInventory.CargoPercentage >= 0.99f)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            continue;
                                        }
                                        return true;
                                    }
                                }
                            }
                            if (destinationInventory.CargoPercentage >= 0.99f)
                            {
                                break;
                            }
                        }
                        index++;
                    }
                    if (destinationInventory.CargoPercentage >= 0.99f)
                    {
                        break;
                    }
                }
                num++;
            }
            return flag;
        }

        public static void PushAnyRequest(IMyConveyorEndpointBlock start, MyInventory srcInventory, long playerId)
        {
            if (!srcInventory.Empty())
            {
                foreach (MyPhysicalInventoryItem item in srcInventory.GetItems().ToArray())
                {
                    MyFixedPoint? amount = null;
                    ItemPushRequest(start, srcInventory, playerId, item, amount);
                }
            }
        }

        public static bool Reachable(IMyConveyorEndpoint from, IMyConveyorEndpoint to)
        {
            MyPathFindingSystem<IMyConveyorEndpoint> pathfinding = Pathfinding;
            lock (pathfinding)
            {
                return Pathfinding.Reachable(from, to);
            }
        }

        private void Receiver_IsPoweredChanged()
        {
            this.UpdateLines();
        }

        private void RecomputeConveyorEndpoints()
        {
            this.m_conveyorConnections.Clear();
            if (this.m_isRecomputingGraph)
            {
                this.m_isRecomputationIsAborted = true;
            }
            else
            {
                this.StartRecomputationThread();
            }
        }

        public static void RecomputeMappingForBlock(IMyConveyorEndpointBlock processedBlock)
        {
            MyCubeBlock block = processedBlock as MyCubeBlock;
            if (((block != null) && ((block.CubeGrid != null) && (block.CubeGrid.GridSystems != null))) && (block.CubeGrid.GridSystems.ConveyorSystem != null))
            {
                ConveyorEndpointMapping mapping = block.CubeGrid.GridSystems.ConveyorSystem.ComputeMappingForBlock(processedBlock);
                if (block.CubeGrid.GridSystems.ConveyorSystem.m_conveyorConnections.ContainsKey(processedBlock))
                {
                    block.CubeGrid.GridSystems.ConveyorSystem.m_conveyorConnections[processedBlock] = mapping;
                }
                else
                {
                    block.CubeGrid.GridSystems.ConveyorSystem.m_conveyorConnections.Add(processedBlock, mapping);
                }
                if (block.CubeGrid.GridSystems.ConveyorSystem.m_isRecomputingGraph)
                {
                    if (block.CubeGrid.GridSystems.ConveyorSystem.m_conveyorConnectionsForThread.ContainsKey(processedBlock))
                    {
                        block.CubeGrid.GridSystems.ConveyorSystem.m_conveyorConnectionsForThread[processedBlock] = mapping;
                    }
                    else
                    {
                        block.CubeGrid.GridSystems.ConveyorSystem.m_conveyorConnectionsForThread.Add(processedBlock, mapping);
                    }
                }
            }
        }

        public void Remove(MyCubeBlock block)
        {
            this.m_inventoryBlocks.Remove(block);
            Action<MyCubeBlock> blockRemoved = this.BlockRemoved;
            if (blockRemoved != null)
            {
                blockRemoved(block);
            }
        }

        public void RemoveConveyorBlock(IMyConveyorEndpointBlock block)
        {
            using (this.m_iteratorLock.AcquireExclusiveUsing())
            {
                this.m_endpointIterator = null;
                this.m_conveyorEndpointBlocks.Remove(block);
                if (block is MyShipConnector)
                {
                    this.m_connectors.Remove(block as MyShipConnector);
                }
                if (!this.IsClosing)
                {
                    if (this.OnBeforeRemoveEndpointBlock != null)
                    {
                        this.OnBeforeRemoveEndpointBlock(block);
                    }
                    for (int i = 0; i < block.ConveyorEndpoint.GetLineCount(); i++)
                    {
                        MyConveyorLine conveyorLine = block.ConveyorEndpoint.GetConveyorLine(i);
                        conveyorLine.DisconnectEndpoint(block.ConveyorEndpoint);
                        if (conveyorLine.IsDegenerate)
                        {
                            this.m_lines.Remove(conveyorLine);
                        }
                    }
                }
            }
        }

        public void RemoveSegmentBlock(IMyConveyorSegmentBlock segmentBlock)
        {
            if (!this.IsClosing)
            {
                if (this.OnBeforeRemoveSegmentBlock != null)
                {
                    this.OnBeforeRemoveSegmentBlock(segmentBlock);
                }
                MyConveyorLine conveyorLine = segmentBlock.ConveyorSegment.ConveyorLine;
                MyConveyorLine oldLine = segmentBlock.ConveyorSegment.ConveyorLine.RemovePortion(segmentBlock.ConveyorSegment.ConnectingPosition1.NeighbourGridPosition, segmentBlock.ConveyorSegment.ConnectingPosition2.NeighbourGridPosition);
                if (conveyorLine.IsDegenerate)
                {
                    this.m_lines.Remove(conveyorLine);
                }
                if (oldLine != null)
                {
                    this.UpdateLineReferences(oldLine, oldLine);
                    this.m_lines.Add(oldLine);
                }
            }
        }

        public void SerializeLines(List<MyObjectBuilder_ConveyorLine> resultList)
        {
            foreach (MyConveyorLine line in this.m_lines)
            {
                if ((!line.IsEmpty || !line.IsDisconnected) || (line.Length != 1))
                {
                    resultList.Add(line.GetObjectBuilder());
                }
            }
        }

        private static void SetTraversalInventoryItemDefinitionId(MyDefinitionId item = new MyDefinitionId())
        {
            m_inventoryItemDefinitionId = item;
        }

        private static void SetTraversalPlayerId(long playerId)
        {
            m_playerIdForAccessiblePredicate = playerId;
        }

        private void StartRecomputationThread()
        {
            this.m_conveyorConnectionsForThread.Clear();
            this.m_isRecomputingGraph = true;
            this.m_isRecomputationIsAborted = false;
            this.m_isRecomputationInterrupted = false;
            this.m_endpointIterator = null;
            Parallel.Start(new Action(this.UpdateConveyorEndpointMapping), new Action(this.OnConveyorEndpointMappingUpdateCompleted));
        }

        public void ToggleConnectors()
        {
            bool flag = false;
            foreach (MyShipConnector connector in this.m_connectors)
            {
                flag |= connector.Connected;
            }
            foreach (MyShipConnector connector2 in this.m_connectors)
            {
                if (connector2.GetPlayerRelationToOwner() != MyRelationsBetweenPlayerAndBlock.Enemies)
                {
                    if (flag && connector2.Connected)
                    {
                        connector2.TryDisconnect();
                        this.HudMessage = MySpaceTexts.NotificationConnectorsDisabled;
                    }
                    if (!flag)
                    {
                        connector2.TryConnect();
                        this.HudMessage = !connector2.InConstraint ? MyStringId.NullOrEmpty : MySpaceTexts.NotificationConnectorsEnabled;
                    }
                }
            }
        }

        private bool TryMergeEndpointEndpoint(IMyConveyorEndpointBlock endpointBlock1, IMyConveyorEndpointBlock endpointBlock2, ConveyorLinePosition pos1, ConveyorLinePosition pos2)
        {
            MyConveyorLine conveyorLine = endpointBlock1.ConveyorEndpoint.GetConveyorLine(pos1);
            if (conveyorLine == null)
            {
                return false;
            }
            MyConveyorLine newLine = endpointBlock2.ConveyorEndpoint.GetConveyorLine(pos2);
            if (newLine == null)
            {
                return false;
            }
            if (conveyorLine.Type != newLine.Type)
            {
                return false;
            }
            if (conveyorLine.GetEndpoint(1) == null)
            {
                conveyorLine.Reverse();
            }
            if (newLine.GetEndpoint(0) == null)
            {
                newLine.Reverse();
            }
            newLine.Merge(conveyorLine, null);
            endpointBlock1.ConveyorEndpoint.SetConveyorLine(pos1, newLine);
            conveyorLine.RecalculateConductivity();
            newLine.RecalculateConductivity();
            return true;
        }

        private bool TryMergeEndpointSegment(IMyConveyorEndpointBlock endpoint, IMyConveyorSegmentBlock segmentBlock, ConveyorLinePosition endpointPosition)
        {
            MyConveyorLine conveyorLine = endpoint.ConveyorEndpoint.GetConveyorLine(endpointPosition);
            if (conveyorLine == null)
            {
                return false;
            }
            if (!segmentBlock.ConveyorSegment.CanConnectTo(endpointPosition.GetConnectingPosition(), conveyorLine.Type))
            {
                return false;
            }
            MyConveyorLine newLine = segmentBlock.ConveyorSegment.ConveyorLine;
            newLine.Merge(conveyorLine, segmentBlock);
            endpoint.ConveyorEndpoint.SetConveyorLine(endpointPosition, newLine);
            conveyorLine.RecalculateConductivity();
            newLine.RecalculateConductivity();
            return true;
        }

        public void UpdateAfterSimulation()
        {
            this.UpdateLinesLazy();
        }

        public void UpdateAfterSimulation100()
        {
            if (this.m_needsRecomputation)
            {
                MySimpleProfiler.Begin("Conveyor", MySimpleProfiler.ProfilingBlockType.BLOCK, "UpdateAfterSimulation100");
                this.RecomputeConveyorEndpoints();
                this.m_needsRecomputation = false;
                MySimpleProfiler.End("UpdateAfterSimulation100");
            }
        }

        public void UpdateBeforeSimulation()
        {
            MySimpleProfiler.Begin("Conveyor", MySimpleProfiler.ProfilingBlockType.BLOCK, "UpdateBeforeSimulation");
            foreach (MyConveyorLine line in this.m_lines)
            {
                if (!line.IsEmpty)
                {
                    line.Update();
                }
            }
            MySimpleProfiler.End("UpdateBeforeSimulation");
        }

        public void UpdateBeforeSimulation10()
        {
            MySimpleProfiler.Begin("Conveyor", MySimpleProfiler.ProfilingBlockType.BLOCK, "UpdateBeforeSimulation10");
            this.ResourceSink.Update();
            MySimpleProfiler.End("UpdateBeforeSimulation10");
        }

        private void UpdateConveyorEndpointMapping()
        {
            using (this.m_iteratorLock.AcquireExclusiveUsing())
            {
                long timestamp = Stopwatch.GetTimestamp();
                this.m_isRecomputationInterrupted = false;
                if (this.m_endpointIterator == null)
                {
                    this.m_endpointIterator = this.m_conveyorEndpointBlocks.GetEnumerator();
                    this.m_endpointIterator.MoveNext();
                }
                IMyConveyorEndpointBlock current = this.m_endpointIterator.Current;
                while (current != null)
                {
                    if (this.m_isRecomputationIsAborted)
                    {
                        this.m_isRecomputationInterrupted = true;
                    }
                    else
                    {
                        TimeSpan span = new TimeSpan(Stopwatch.GetTimestamp() - timestamp);
                        if (span.TotalMilliseconds > 10.0)
                        {
                            this.m_isRecomputationInterrupted = true;
                        }
                        else
                        {
                            this.m_conveyorConnectionsForThread.Add(current, this.ComputeMappingForBlock(current));
                            if (this.m_endpointIterator != null)
                            {
                                this.m_endpointIterator.MoveNext();
                                current = this.m_endpointIterator.Current;
                                continue;
                            }
                            this.m_isRecomputationIsAborted = true;
                            this.m_isRecomputationInterrupted = true;
                        }
                    }
                    break;
                }
            }
        }

        private void UpdateLineReferences(MyConveyorLine oldLine, MyConveyorLine newLine)
        {
            for (int i = 0; i < 2; i++)
            {
                if (oldLine.GetEndpoint(i) != null)
                {
                    oldLine.GetEndpoint(i).SetConveyorLine(oldLine.GetEndpointPosition(i), newLine);
                }
            }
            foreach (Vector3I vectori in oldLine)
            {
                MySlimBlock cubeBlock = this.m_grid.GetCubeBlock(vectori);
                if (cubeBlock != null)
                {
                    IMyConveyorSegmentBlock fatBlock = cubeBlock.FatBlock as IMyConveyorSegmentBlock;
                    if (fatBlock != null)
                    {
                        fatBlock.ConveyorSegment.SetConveyorLine(newLine);
                    }
                }
            }
            oldLine.RecalculateConductivity();
            newLine.RecalculateConductivity();
        }

        public void UpdateLines()
        {
            this.NeedsUpdateLines = true;
            this.m_grid.MarkForUpdate();
        }

        public void UpdateLinesLazy()
        {
            if (this.NeedsUpdateLines)
            {
                this.NeedsUpdateLines = false;
                this.FlagForRecomputation();
            }
        }

        private static PullRequestItemSet m_tmpRequestedItemSet
        {
            get
            {
                if (m_tmpRequestedItemSetPerThread == null)
                {
                    m_tmpRequestedItemSetPerThread = new PullRequestItemSet();
                }
                return m_tmpRequestedItemSetPerThread;
            }
        }

        private static MyPathFindingSystem<IMyConveyorEndpoint> Pathfinding
        {
            get
            {
                if (m_pathfinding == null)
                {
                    m_pathfinding = new MyPathFindingSystem<IMyConveyorEndpoint>(0x80, null);
                }
                return m_pathfinding;
            }
        }

        public MyResourceSinkComponent ResourceSink { get; private set; }

        public bool IsInteractionPossible
        {
            get
            {
                bool flag = false;
                foreach (MyShipConnector connector in this.m_connectors)
                {
                    flag |= connector.InConstraint;
                }
                return flag;
            }
        }

        public bool Connected
        {
            get
            {
                bool flag = false;
                foreach (MyShipConnector connector in this.m_connectors)
                {
                    flag |= connector.Connected;
                }
                return flag;
            }
        }

        public HashSetReader<IMyConveyorEndpointBlock> ConveyorEndpointBlocks =>
            new HashSetReader<IMyConveyorEndpointBlock>(this.m_conveyorEndpointBlocks);

        public HashSetReader<MyCubeBlock> InventoryBlocks =>
            new HashSetReader<MyCubeBlock>(this.m_inventoryBlocks);

        public class ConveyorEndpointMapping
        {
            public List<IMyConveyorEndpointBlock> pullElements;
            public List<IMyConveyorEndpointBlock> pushElements;
            public Dictionary<Tuple<IMyConveyorEndpointBlock, MyDefinitionId, bool>, bool> testedTransfers = new Dictionary<Tuple<IMyConveyorEndpointBlock, MyDefinitionId, bool>, bool>();

            public void AddTransfer(IMyConveyorEndpointBlock block, MyDefinitionId itemId, bool isPush, bool canTransfer)
            {
                Tuple<IMyConveyorEndpointBlock, MyDefinitionId, bool> tuple = new Tuple<IMyConveyorEndpointBlock, MyDefinitionId, bool>(block, itemId, isPush);
                this.testedTransfers[tuple] = canTransfer;
            }

            public bool TryGetTransfer(IMyConveyorEndpointBlock block, MyDefinitionId itemId, bool isPush, out bool canTransfer)
            {
                Tuple<IMyConveyorEndpointBlock, MyDefinitionId, bool> key = new Tuple<IMyConveyorEndpointBlock, MyDefinitionId, bool>(block, itemId, isPush);
                return this.testedTransfers.TryGetValue(key, out canTransfer);
            }
        }

        private class PullRequestItemSet
        {
            private bool m_all;
            private MyObjectBuilderType? m_obType;
            private MyInventoryConstraint m_constraint;

            public void Clear()
            {
                this.m_all = false;
                this.m_obType = null;
                this.m_constraint = null;
            }

            public bool Contains(MyDefinitionId itemId)
            {
                if (!this.m_all && ((this.m_obType == null) || (this.m_obType.Value != itemId.TypeId)))
                {
                    return ((this.m_constraint != null) && this.m_constraint.Check(itemId));
                }
                return true;
            }

            public void Set(MyInventoryConstraint inventoryConstraint)
            {
                this.Clear();
                this.m_constraint = inventoryConstraint;
            }

            public void Set(bool all)
            {
                this.Clear();
                this.m_all = all;
            }

            public void Set(MyObjectBuilderType? itemTypeId)
            {
                this.Clear();
                this.m_obType = itemTypeId;
            }
        }

        private class TransferData : WorkData
        {
            public IMyConveyorEndpointBlock m_start;
            public IMyConveyorEndpointBlock m_endPoint;
            public MyDefinitionId m_itemId;
            public bool m_isPush;
            public bool m_canTransfer;

            public TransferData(IMyConveyorEndpointBlock start, IMyConveyorEndpointBlock endPoint, MyDefinitionId itemId, bool isPush)
            {
                this.m_start = start;
                this.m_endPoint = endPoint;
                this.m_itemId = itemId;
                this.m_isPush = isPush;
            }

            public void ComputeTransfer()
            {
                IMyConveyorEndpointBlock start = this.m_start;
                IMyConveyorEndpointBlock endPoint = this.m_endPoint;
                if (!this.m_isPush)
                {
                    MyUtils.Swap<IMyConveyorEndpointBlock>(ref start, ref endPoint);
                }
                this.m_canTransfer = MyGridConveyorSystem.ComputeCanTransfer(start, endPoint, new MyDefinitionId?(this.m_itemId));
            }

            public void StoreTransferState()
            {
                (this.m_start as MyCubeBlock).CubeGrid.GridSystems.ConveyorSystem.GetConveyorEndpointMapping(this.m_start).AddTransfer(this.m_endPoint, this.m_itemId, this.m_isPush, this.m_canTransfer);
            }
        }
    }
}

