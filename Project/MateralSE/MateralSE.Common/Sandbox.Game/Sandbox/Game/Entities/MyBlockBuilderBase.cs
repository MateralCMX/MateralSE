namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Components.Session;
    using VRage.Game.Definitions.SessionComponents;
    using VRage.Game.Entity;
    using VRage.Game.Models;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyBlockBuilderBase : MySessionComponentBase
    {
        protected static readonly MyStringId[] m_rotationControls = new MyStringId[] { MyControlsSpace.CUBE_ROTATE_VERTICAL_POSITIVE, MyControlsSpace.CUBE_ROTATE_VERTICAL_NEGATIVE, MyControlsSpace.CUBE_ROTATE_HORISONTAL_POSITIVE, MyControlsSpace.CUBE_ROTATE_HORISONTAL_NEGATIVE, MyControlsSpace.CUBE_ROTATE_ROLL_POSITIVE, MyControlsSpace.CUBE_ROTATE_ROLL_NEGATIVE };
        protected static MyCubeBuilderDefinition m_cubeBuilderDefinition;
        private static float m_intersectionDistance;
        protected static readonly int[] m_rotationDirections = new int[] { -1, 1, 1, -1, 1, -1 };
        protected MyCubeGrid m_currentGrid;
        protected MatrixD m_invGridWorldMatrix = MatrixD.Identity;
        protected MyVoxelBase m_currentVoxelBase;
        protected Sandbox.Engine.Physics.MyPhysics.HitInfo? m_hitInfo;
        private static IMyPlacementProvider m_placementProvider;

        static MyBlockBuilderBase()
        {
            PlacementProvider = new MyDefaultPlacementProvider(IntersectionDistance);
        }

        protected MyBlockBuilderBase()
        {
        }

        public abstract void Activate(MyDefinitionId? blockDefinitionId = new MyDefinitionId?());
        protected static void AddFastBuildModelWithSubparts(ref MatrixD matrix, List<MatrixD> matrices, List<string> models, MyCubeBlockDefinition blockDefinition, float gridScale)
        {
            if (!string.IsNullOrEmpty(blockDefinition.Model))
            {
                matrices.Add(matrix);
                models.Add(blockDefinition.Model);
                MyEntitySubpart.Data outData = new MyEntitySubpart.Data();
                MyModel modelOnlyData = MyModels.GetModelOnlyData(blockDefinition.Model);
                modelOnlyData.Rescale(gridScale);
                foreach (KeyValuePair<string, MyModelDummy> pair in modelOnlyData.Dummies)
                {
                    MyCubeBlockDefinition definition;
                    MatrixD xd;
                    Vector3 vector;
                    if (MyEntitySubpart.GetSubpartFromDummy(blockDefinition.Model, pair.Key, pair.Value, ref outData))
                    {
                        MyModel model = MyModels.GetModelOnlyData(outData.File);
                        if (model != null)
                        {
                            model.Rescale(gridScale);
                        }
                        MatrixD item = MatrixD.Multiply(outData.InitialTransform, matrix);
                        matrices.Add(item);
                        models.Add(outData.File);
                        continue;
                    }
                    if (MyFakes.ENABLE_SUBBLOCKS && (MyCubeBlock.GetSubBlockDataFromDummy(blockDefinition, pair.Key, pair.Value, false, out definition, out xd, out vector) && !string.IsNullOrEmpty(definition.Model)))
                    {
                        Vector3I vectori4;
                        MyModel model2 = MyModels.GetModelOnlyData(definition.Model);
                        if (model2 != null)
                        {
                            model2.Rescale(gridScale);
                        }
                        Vector3I vectori = Vector3I.Round(Vector3.DominantAxisProjection((Vector3) xd.Forward));
                        Vector3I vectori2 = Vector3I.One - Vector3I.Abs(vectori);
                        Vector3I vectori3 = Vector3I.Round(Vector3.DominantAxisProjection((Vector3) (xd.Right * vectori2)));
                        Vector3I.Cross(ref vectori3, ref vectori, out vectori4);
                        xd.Forward = (Vector3D) vectori;
                        xd.Right = (Vector3D) vectori3;
                        xd.Up = (Vector3D) vectori4;
                        MatrixD item = MatrixD.Multiply(xd, matrix);
                        matrices.Add(item);
                        models.Add(definition.Model);
                    }
                }
                if ((MyFakes.ENABLE_GENERATED_BLOCKS && !blockDefinition.IsGeneratedBlock) && (blockDefinition.GeneratedBlockDefinitions != null))
                {
                    foreach (MyDefinitionId id in blockDefinition.GeneratedBlockDefinitions)
                    {
                        MyCubeBlockDefinition definition2;
                        if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(id, out definition2))
                        {
                            MyModel model3 = MyModels.GetModelOnlyData(definition2.Model);
                            if (model3 != null)
                            {
                                model3.Rescale(gridScale);
                            }
                        }
                    }
                }
            }
        }

        protected internal virtual void ChooseHitObject()
        {
            MyCubeGrid grid;
            MyVoxelBase base2;
            this.FindClosestPlacementObject(out grid, out base2);
            this.CurrentGrid = grid;
            this.CurrentVoxelBase = base2;
            this.m_invGridWorldMatrix = (this.CurrentGrid != null) ? this.CurrentGrid.PositionComp.WorldMatrixInvScaled : MatrixD.Identity;
        }

        public static void ComputeSteps(Vector3I start, Vector3I end, Vector3I rotatedSize, out Vector3I stepDelta, out Vector3I counter, out int stepCount)
        {
            Vector3I vectori = end - start;
            stepDelta = Vector3I.Sign(vectori) * rotatedSize;
            counter = (Vector3I) ((Vector3I.Abs(end - start) / rotatedSize) + Vector3I.One);
            stepCount = counter.Size;
        }

        public abstract void Deactivate();
        public MyCubeGrid FindClosestGrid() => 
            PlacementProvider.ClosestGrid;

        public bool FindClosestPlacementObject(out MyCubeGrid closestGrid, out MyVoxelBase closestVoxelMap)
        {
            closestGrid = null;
            closestVoxelMap = null;
            this.m_hitInfo = null;
            if (MySession.Static.ControlledEntity == null)
            {
                return false;
            }
            closestGrid = PlacementProvider.ClosestGrid;
            closestVoxelMap = PlacementProvider.ClosestVoxelMap;
            this.m_hitInfo = PlacementProvider.HitInfo;
            return ((closestGrid != null) || (closestVoxelMap != null));
        }

        protected bool GetBlockAddPosition(float gridSize, bool placingSmallGridOnLargeStatic, out MySlimBlock intersectedBlock, out Vector3D intersectedBlockPos, out Vector3D intersectExactPos, out Vector3I addPositionBlock, out Vector3I addDirectionBlock, out ushort? compoundBlockId)
        {
            Vector3I vectori;
            intersectedBlock = null;
            intersectedBlockPos = new Vector3D();
            Vector3 vector = new Vector3();
            intersectExactPos = vector;
            addDirectionBlock = new Vector3I();
            addPositionBlock = new Vector3I();
            compoundBlockId = 0;
            if (this.CurrentVoxelBase != null)
            {
                Vector3I intVector = Base6Directions.GetIntVector(Base6Directions.GetClosestDirection(this.m_hitInfo.Value.HkHitInfo.Normal));
                Vector3D worldPos = IntersectionStart + ((IntersectionDistance * this.m_hitInfo.Value.HkHitInfo.HitFraction) * Vector3D.Normalize(IntersectionDirection));
                addPositionBlock = MyCubeGrid.StaticGlobalGrid_WorldToUGInt(worldPos + (((0.1f * Vector3.Half) * intVector) * gridSize), gridSize, CubeBuilderDefinition.BuildingSettings.StaticGridAlignToCenter);
                addDirectionBlock = intVector;
                intersectedBlockPos = (Vector3D) (addPositionBlock - intVector);
                intersectExactPos = MyCubeGrid.StaticGlobalGrid_WorldToUG(worldPos, gridSize, CubeBuilderDefinition.BuildingSettings.StaticGridAlignToCenter);
                intersectExactPos = ((Vector3D.One - Vector3.Abs((Vector3) intVector)) * intersectExactPos) + ((intersectedBlockPos + (0.5f * intVector)) * Vector3.Abs((Vector3) intVector));
                return true;
            }
            Vector3D? nullable = this.GetIntersectedBlockData(ref this.m_invGridWorldMatrix, out intersectExactPos, out intersectedBlock, out compoundBlockId);
            if (nullable == null)
            {
                return false;
            }
            intersectedBlockPos = nullable.Value;
            if (!this.GetCubeAddAndRemovePositions(Vector3I.Round(intersectedBlockPos), placingSmallGridOnLargeStatic, out addPositionBlock, out addDirectionBlock, out vectori))
            {
                return false;
            }
            if (!placingSmallGridOnLargeStatic)
            {
                if (!MyFakes.ENABLE_BLOCK_PLACING_ON_INTERSECTED_POSITION)
                {
                    if (this.CurrentGrid.CubeExists(addPositionBlock))
                    {
                        return false;
                    }
                }
                else
                {
                    Vector3I vectori3 = Vector3I.Round(intersectedBlockPos);
                    if (vectori3 != vectori)
                    {
                        if (this.m_hitInfo != null)
                        {
                            Vector3I intVector = Base6Directions.GetIntVector(Base6Directions.GetClosestDirection(this.m_hitInfo.Value.HkHitInfo.Normal));
                            addDirectionBlock = intVector;
                        }
                        vectori = vectori3;
                        addPositionBlock = (Vector3I) (vectori + addDirectionBlock);
                    }
                }
            }
            if (placingSmallGridOnLargeStatic)
            {
                vectori = Vector3I.Round(intersectedBlockPos);
            }
            intersectedBlockPos = (Vector3D) vectori;
            intersectedBlock = this.CurrentGrid.GetCubeBlock(vectori);
            return (intersectedBlock != null);
        }

        protected bool GetCubeAddAndRemovePositions(Vector3I intersectedCube, bool placingSmallGridOnLargeStatic, out Vector3I addPos, out Vector3I addDir, out Vector3I removePos)
        {
            bool flag = false;
            addPos = new Vector3I();
            addDir = new Vector3I();
            removePos = new Vector3I();
            MatrixD worldMatrixInvScaled = this.CurrentGrid.PositionComp.WorldMatrixInvScaled;
            addPos = intersectedCube;
            addDir = Vector3I.Forward;
            Vector3D position = Vector3D.Transform(IntersectionStart, worldMatrixInvScaled);
            Vector3D direction = Vector3D.Normalize(Vector3D.TransformNormal(IntersectionDirection, worldMatrixInvScaled));
            RayD ray = new RayD(position, direction);
            int num = 0;
            while (true)
            {
                if (num < 100)
                {
                    BoundingBoxD cubeBoundingBox = this.GetCubeBoundingBox(addPos);
                    if (placingSmallGridOnLargeStatic || (cubeBoundingBox.Contains(position) != ContainmentType.Contains))
                    {
                        double? nullable = cubeBoundingBox.Intersects(ray);
                        if (nullable != null)
                        {
                            removePos = addPos;
                            Vector3I vectori = Vector3I.Sign(Vector3.DominantAxisProjection((Vector3) ((position + (direction * nullable.Value)) - (removePos * this.CurrentGrid.GridSize))));
                            addPos = (Vector3I) (removePos + vectori);
                            addDir = vectori;
                            if (this.CurrentGrid.CubeExists(addPos))
                            {
                                num++;
                                continue;
                            }
                            flag = true;
                        }
                    }
                }
                return flag;
            }
        }

        protected BoundingBoxD GetCubeBoundingBox(Vector3I cubePos)
        {
            Vector3D vectord = (Vector3D) (cubePos * this.CurrentGrid.GridSize);
            Vector3 vector = new Vector3(0.06f, 0.06f, 0.06f);
            return new BoundingBoxD((vectord - new Vector3D((double) (this.CurrentGrid.GridSize / 2f))) - vector, (vectord + new Vector3D((double) (this.CurrentGrid.GridSize / 2f))) + vector);
        }

        protected Vector3D? GetIntersectedBlockData(ref MatrixD inverseGridWorldMatrix, out Vector3D intersection, out MySlimBlock intersectedBlock, out ushort? compoundBlockId)
        {
            intersection = Vector3D.Zero;
            intersectedBlock = null;
            compoundBlockId = 0;
            if (this.CurrentGrid == null)
            {
                return null;
            }
            double maxValue = double.MaxValue;
            Vector3D? nullable = null;
            LineD line = new LineD(IntersectionStart, IntersectionStart + (IntersectionDirection * IntersectionDistance));
            Vector3I zero = Vector3I.Zero;
            if (!this.CurrentGrid.GetLineIntersectionExactGrid(ref line, ref zero, ref maxValue, new Sandbox.Engine.Physics.MyPhysics.HitInfo?(this.m_hitInfo.Value)))
            {
                return null;
            }
            maxValue = Math.Sqrt(maxValue);
            nullable = new Vector3D?((Vector3D) zero);
            intersectedBlock = this.CurrentGrid.GetCubeBlock(zero);
            if (intersectedBlock == null)
            {
                return null;
            }
            if (intersectedBlock.FatBlock is MyCompoundCubeBlock)
            {
                ushort num2;
                MyIntersectionResultLineTriangleEx? nullable4;
                MyCompoundCubeBlock fatBlock = intersectedBlock.FatBlock as MyCompoundCubeBlock;
                ushort? blockId = null;
                if (fatBlock.GetIntersectionWithLine(ref line, out nullable4, out num2, IntersectionFlags.ALL_TRIANGLES, false, false))
                {
                    blockId = new ushort?(num2);
                }
                else if (fatBlock.GetBlocksCount() == 1)
                {
                    blockId = fatBlock.GetBlockId(fatBlock.GetBlocks()[0]);
                }
                compoundBlockId = blockId;
            }
            Vector3D vectord = Vector3D.Transform(IntersectionStart, (MatrixD) inverseGridWorldMatrix);
            intersection = vectord + (maxValue * Vector3D.Normalize(Vector3D.TransformNormal(IntersectionDirection, (MatrixD) inverseGridWorldMatrix)));
            intersection *= 1f / this.CurrentGrid.GridSize;
            return nullable;
        }

        public override void InitFromDefinition(MySessionComponentDefinition definition)
        {
            base.InitFromDefinition(definition);
            m_cubeBuilderDefinition = definition as MyCubeBuilderDefinition;
            MyCubeBuilderDefinition cubeBuilderDefinition = m_cubeBuilderDefinition;
            IntersectionDistance = m_cubeBuilderDefinition.DefaultBlockBuildingDistance;
        }

        protected Vector3I? IntersectCubes(MyCubeGrid grid, out double distance)
        {
            distance = 3.4028234663852886E+38;
            LineD line = new LineD(IntersectionStart, IntersectionStart + (IntersectionDirection * IntersectionDistance));
            Vector3I zero = Vector3I.Zero;
            double maxValue = double.MaxValue;
            if (grid.GetLineIntersectionExactGrid(ref line, ref zero, ref maxValue))
            {
                distance = Math.Sqrt(maxValue);
                return new Vector3I?(zero);
            }
            return null;
        }

        protected Vector3D? IntersectExact(MyCubeGrid grid, ref MatrixD inverseGridWorldMatrix, out Vector3D intersection, out MySlimBlock intersectedBlock)
        {
            double num;
            intersection = Vector3D.Zero;
            LineD line = new LineD(IntersectionStart, IntersectionStart + (IntersectionDirection * IntersectionDistance));
            Vector3D? nullable = grid.GetLineIntersectionExactAll(ref line, out num, out intersectedBlock);
            if (nullable != null)
            {
                Vector3D vectord = Vector3D.Transform(IntersectionStart, (MatrixD) inverseGridWorldMatrix);
                intersection = vectord + (num * Vector3D.Normalize(Vector3D.TransformNormal(IntersectionDirection, (MatrixD) inverseGridWorldMatrix)));
                intersection *= 1f / grid.GridSize;
            }
            return nullable;
        }

        protected void IntersectInflated(List<Vector3I> outHitPositions, MyCubeGrid grid)
        {
            float maxDist = 2000f;
            Vector3I gridSizeInflate = new Vector3I(100, 100, 100);
            if (grid != null)
            {
                PlacementProvider.RayCastGridCells(grid, outHitPositions, gridSizeInflate, maxDist);
            }
            else
            {
                float cubeSize = MyDefinitionManager.Static.GetCubeSize(this.CurrentBlockDefinition.CubeSize);
                MyCubeGrid.RayCastStaticCells(IntersectionStart, IntersectionStart + (IntersectionDirection * maxDist), outHitPositions, cubeSize, new Vector3I?(gridSizeInflate), false);
            }
        }

        public static float IntersectionDistance
        {
            get => 
                m_intersectionDistance;
            set
            {
                m_intersectionDistance = value;
                if (PlacementProvider != null)
                {
                    PlacementProvider.IntersectionDistance = value;
                }
            }
        }

        protected internal abstract MyCubeGrid CurrentGrid { get; protected set; }

        protected internal abstract MyVoxelBase CurrentVoxelBase { get; protected set; }

        protected abstract MyCubeBlockDefinition CurrentBlockDefinition { get; set; }

        public Sandbox.Engine.Physics.MyPhysics.HitInfo? HitInfo =>
            this.m_hitInfo;

        private static bool AdminSpectatorIsBuilding =>
            (MyFakes.ENABLE_ADMIN_SPECTATOR_BUILDING && ((MySession.Static != null) && ((MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.Spectator) && ((MyMultiplayer.Static != null) && MySession.Static.IsUserAdmin(Sync.MyId)))));

        private static bool DeveloperSpectatorIsBuilding =>
            ((MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.Spectator) && (!MySession.Static.SurvivalMode || MyInput.Static.ENABLE_DEVELOPER_KEYS));

        public static bool SpectatorIsBuilding =>
            (DeveloperSpectatorIsBuilding || AdminSpectatorIsBuilding);

        public static bool CameraControllerSpectator
        {
            get
            {
                MyCameraControllerEnum cameraControllerEnum = MySession.Static.GetCameraControllerEnum();
                return ((cameraControllerEnum == MyCameraControllerEnum.Spectator) || ((cameraControllerEnum == MyCameraControllerEnum.SpectatorDelta) || (cameraControllerEnum == MyCameraControllerEnum.SpectatorOrbit)));
            }
        }

        public static Vector3D IntersectionStart =>
            PlacementProvider.RayStart;

        public static Vector3D IntersectionDirection =>
            PlacementProvider.RayDirection;

        public Vector3D FreePlacementTarget =>
            (IntersectionStart + (IntersectionDirection * IntersectionDistance));

        public static IMyPlacementProvider PlacementProvider
        {
            get => 
                m_placementProvider;
            set => 
                (m_placementProvider = value ?? new MyDefaultPlacementProvider(IntersectionDistance));
        }

        public static MyCubeBuilderDefinition CubeBuilderDefinition =>
            m_cubeBuilderDefinition;

        public abstract bool IsActivated { get; }
    }
}

