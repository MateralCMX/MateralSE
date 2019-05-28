namespace Sandbox.Game.Entities
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Models;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Import;

    [MyCubeBlockType(typeof(MyObjectBuilder_CompoundCubeBlock))]
    public class MyCompoundCubeBlock : MyCubeBlock, IMyDecalProxy
    {
        private static List<VertexArealBoneIndexWeight> m_boneIndexWeightTmp;
        private static readonly string COMPOUND_DUMMY = "compound_";
        private static readonly ushort BLOCK_IN_COMPOUND_LOCAL_ID = 0x8000;
        private static readonly ushort BLOCK_IN_COMPOUND_LOCAL_MAX_VALUE = 0x7fff;
        private static readonly MyStringId BUILD_TYPE_ANY = MyStringId.GetOrCompute("any");
        private static readonly string COMPOUND_BLOCK_SUBTYPE_NAME = "CompoundBlock";
        private static readonly HashSet<string> m_tmpTemplates = new HashSet<string>();
        private static readonly List<MyModelDummy> m_tmpDummies = new List<MyModelDummy>();
        private static readonly List<MyModelDummy> m_tmpOtherDummies = new List<MyModelDummy>();
        private readonly Dictionary<ushort, MySlimBlock> m_mapIdToBlock = new Dictionary<ushort, MySlimBlock>();
        private readonly List<MySlimBlock> m_blocks = new List<MySlimBlock>();
        private ushort m_nextId;
        private ushort m_localNextId;
        private readonly HashSet<string> m_templates = new HashSet<string>();
        private static readonly List<uint> m_tmpIds = new List<uint>();

        public MyCompoundCubeBlock()
        {
            base.PositionComp = new MyCompoundBlockPosComponent();
            base.Render = new MyRenderComponentCompoundCubeBlock();
        }

        public bool Add(MySlimBlock block, out ushort id)
        {
            id = this.CreateId(block);
            return this.Add(id, block);
        }

        public bool Add(ushort id, MySlimBlock block)
        {
            if (!this.CanAddBlock(block))
            {
                return false;
            }
            if (this.m_mapIdToBlock.ContainsKey(id))
            {
                return false;
            }
            this.m_mapIdToBlock.Add(id, block);
            this.m_blocks.Add(block);
            MatrixD identity = MatrixD.Identity;
            GetBlockLocalMatrixFromGridPositionAndOrientation(block, ref identity);
            MatrixD worldMatrix = identity * base.Parent.WorldMatrix;
            block.FatBlock.PositionComp.SetWorldMatrix(worldMatrix, this, true, true, true, false, false, false);
            block.FatBlock.Hierarchy.Parent = base.Hierarchy;
            block.FatBlock.OnAddedToScene(this);
            base.CubeGrid.UpdateBlockNeighbours(base.SlimBlock);
            this.RefreshTemplates();
            if (block.IsMultiBlockPart)
            {
                base.CubeGrid.AddMultiBlockInfo(block);
            }
            return true;
        }

        public bool CanAddBlock(MySlimBlock block)
        {
            if ((block == null) || (block.FatBlock == null))
            {
                return false;
            }
            if (this.m_mapIdToBlock.ContainsValue(block))
            {
                return false;
            }
            if (!(block.FatBlock is MyCompoundCubeBlock))
            {
                return this.CanAddBlock(block.BlockDefinition, new MyBlockOrientation?(block.Orientation), block.MultiBlockId, false);
            }
            using (List<MySlimBlock>.Enumerator enumerator = (block.FatBlock as MyCompoundCubeBlock).GetBlocks().GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MySlimBlock current = enumerator.Current;
                    if (!this.CanAddBlock(current.BlockDefinition, new MyBlockOrientation?(current.Orientation), current.MultiBlockId, false))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool CanAddBlock(MyCubeBlockDefinition definition, MyBlockOrientation? orientation, int multiBlockId = 0, bool ignoreSame = false)
        {
            bool flag;
            string[] strArray;
            if (!IsCompoundEnabled(definition))
            {
                return false;
            }
            if (!MyFakes.ENABLE_COMPOUND_BLOCK_COLLISION_DUMMIES)
            {
                if (orientation != null)
                {
                    using (Dictionary<ushort, MySlimBlock>.Enumerator enumerator2 = this.m_mapIdToBlock.GetEnumerator())
                    {
                        while (true)
                        {
                            if (enumerator2.MoveNext())
                            {
                                MyBlockOrientation orientation2;
                                MyBlockOrientation? nullable;
                                KeyValuePair<ushort, MySlimBlock> current = enumerator2.Current;
                                if (current.Value.BlockDefinition.Id.SubtypeId == definition.Id.SubtypeId)
                                {
                                    orientation2 = current.Value.Orientation;
                                    nullable = orientation;
                                    if ((nullable != null) ? (orientation2 == nullable.GetValueOrDefault()) : false)
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                                MyStringId buildType = definition.BuildType;
                                if (!(current.Value.BlockDefinition.BuildType == definition.BuildType))
                                {
                                    continue;
                                }
                                orientation2 = current.Value.Orientation;
                                nullable = orientation;
                                if (!((nullable != null) ? (orientation2 == nullable.GetValueOrDefault()) : false))
                                {
                                    continue;
                                }
                                flag = false;
                            }
                            else
                            {
                                goto TR_0041;
                            }
                            break;
                        }
                        return flag;
                    }
                }
            }
            else
            {
                Matrix matrix;
                if (orientation == null)
                {
                    return false;
                }
                if (this.m_blocks.Count == 0)
                {
                    return true;
                }
                orientation.Value.GetMatrix(out matrix);
                m_tmpOtherDummies.Clear();
                GetCompoundCollisionDummies(definition, m_tmpOtherDummies);
                using (List<MySlimBlock>.Enumerator enumerator = this.m_blocks.GetEnumerator())
                {
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            MySlimBlock current = enumerator.Current;
                            if ((current.BlockDefinition.Id.SubtypeId != definition.Id.SubtypeId) || (current.Orientation != orientation.Value))
                            {
                                if (((multiBlockId == 0) || (current.MultiBlockId != multiBlockId)) && !current.BlockDefinition.IsGeneratedBlock)
                                {
                                    Matrix matrix2;
                                    m_tmpDummies.Clear();
                                    GetCompoundCollisionDummies(current.BlockDefinition, m_tmpDummies);
                                    current.Orientation.GetMatrix(out matrix2);
                                    if (CompoundDummiesIntersect(ref matrix2, ref matrix, m_tmpDummies, m_tmpOtherDummies))
                                    {
                                        m_tmpDummies.Clear();
                                        m_tmpOtherDummies.Clear();
                                        flag = false;
                                        break;
                                    }
                                }
                                continue;
                            }
                            if (ignoreSame)
                            {
                                continue;
                            }
                            flag = false;
                        }
                        else
                        {
                            m_tmpDummies.Clear();
                            m_tmpOtherDummies.Clear();
                            return true;
                        }
                        break;
                    }
                    return flag;
                }
            }
            goto TR_0041;
        TR_0019:
            return true;
        TR_0041:
            strArray = definition.CompoundTemplates;
            int index = 0;
            while (true)
            {
                while (true)
                {
                    if (index >= strArray.Length)
                    {
                        return false;
                    }
                    string item = strArray[index];
                    if (this.m_templates.Contains(item))
                    {
                        MyCompoundBlockTemplateDefinition templateDefinition = GetTemplateDefinition(item);
                        if ((templateDefinition != null) && (templateDefinition.Bindings != null))
                        {
                            MyCompoundBlockTemplateDefinition.MyCompoundBlockBinding templateDefinitionBinding = GetTemplateDefinitionBinding(templateDefinition, definition);
                            if (templateDefinitionBinding != null)
                            {
                                if (templateDefinitionBinding.BuildType == BUILD_TYPE_ANY)
                                {
                                    return true;
                                }
                                if (!templateDefinitionBinding.Multiple)
                                {
                                    bool flag2 = false;
                                    foreach (KeyValuePair<ushort, MySlimBlock> pair2 in this.m_mapIdToBlock)
                                    {
                                        if (pair2.Value.BlockDefinition.BuildType == definition.BuildType)
                                        {
                                            flag2 = true;
                                            break;
                                        }
                                    }
                                    if (flag2)
                                    {
                                        break;
                                    }
                                }
                                if (orientation == null)
                                {
                                    goto TR_0019;
                                }
                                else
                                {
                                    bool flag3 = false;
                                    foreach (KeyValuePair<ushort, MySlimBlock> pair3 in this.m_mapIdToBlock)
                                    {
                                        MyCompoundBlockTemplateDefinition.MyCompoundBlockBinding binding2 = GetTemplateDefinitionBinding(templateDefinition, pair3.Value.BlockDefinition);
                                        if ((binding2 != null) && (binding2.BuildType != BUILD_TYPE_ANY))
                                        {
                                            MyCompoundBlockTemplateDefinition.MyCompoundBlockRotationBinding binding3 = GetRotationBinding(templateDefinition, definition, pair3.Value.BlockDefinition);
                                            if (binding3 != null)
                                            {
                                                if (!(binding3.BuildTypeReference == definition.BuildType))
                                                {
                                                    if (this.IsRotationValid(pair3.Value.Orientation, orientation.Value, binding3.Rotations))
                                                    {
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    if (this.IsRotationValid(orientation.Value, pair3.Value.Orientation, binding3.Rotations))
                                                    {
                                                        continue;
                                                    }
                                                    if ((binding3.BuildTypeReference == pair3.Value.BlockDefinition.BuildType) && this.IsRotationValid(pair3.Value.Orientation, orientation.Value, binding3.Rotations))
                                                    {
                                                        continue;
                                                    }
                                                }
                                                flag3 = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (!flag3)
                                    {
                                        goto TR_0019;
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
                index++;
            }
        }

        public static bool CanAddBlocks(MyCubeBlockDefinition definition, MyBlockOrientation orientation, MyCubeBlockDefinition otherDefinition, MyBlockOrientation otherOrientation)
        {
            Matrix matrix;
            Matrix matrix2;
            if (!IsCompoundEnabled(definition) || !IsCompoundEnabled(otherDefinition))
            {
                return false;
            }
            if (!MyFakes.ENABLE_COMPOUND_BLOCK_COLLISION_DUMMIES)
            {
                return true;
            }
            orientation.GetMatrix(out matrix);
            m_tmpDummies.Clear();
            GetCompoundCollisionDummies(definition, m_tmpDummies);
            otherOrientation.GetMatrix(out matrix2);
            m_tmpOtherDummies.Clear();
            GetCompoundCollisionDummies(otherDefinition, m_tmpOtherDummies);
            m_tmpDummies.Clear();
            m_tmpOtherDummies.Clear();
            return !CompoundDummiesIntersect(ref matrix, ref matrix2, m_tmpDummies, m_tmpOtherDummies);
        }

        protected override void Closing()
        {
            foreach (KeyValuePair<ushort, MySlimBlock> pair in this.m_mapIdToBlock)
            {
                if (pair.Value.FatBlock != null)
                {
                    pair.Value.FatBlock.Close();
                }
            }
            base.Closing();
        }

        private static bool CompoundDummiesIntersect(ref Matrix thisRotation, ref Matrix otherRotation, List<MyModelDummy> thisDummies, List<MyModelDummy> otherDummies)
        {
            using (List<MyModelDummy>.Enumerator enumerator = thisDummies.GetEnumerator())
            {
                bool flag;
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        Matrix matrix2;
                        MyModelDummy current = enumerator.Current;
                        Vector3 forward = current.Matrix.Forward;
                        Vector3 max = new Vector3(current.Matrix.Right.Length(), current.Matrix.Up.Length(), forward.Length()) * 0.5f;
                        BoundingBox box = new BoundingBox(-max, max);
                        Matrix matrix = Matrix.Normalize(current.Matrix);
                        Matrix.Multiply(ref matrix, ref thisRotation, out matrix2);
                        Matrix.Invert(ref matrix2, out matrix);
                        List<MyModelDummy>.Enumerator enumerator2 = otherDummies.GetEnumerator();
                        try
                        {
                            while (true)
                            {
                                Matrix matrix4;
                                if (!enumerator2.MoveNext())
                                {
                                    break;
                                }
                                MyModelDummy dummy2 = enumerator2.Current;
                                forward = dummy2.Matrix.Forward;
                                Vector3 vector3 = new Vector3(dummy2.Matrix.Right.Length(), dummy2.Matrix.Up.Length(), forward.Length()) * 0.5f;
                                Matrix matrix3 = Matrix.Normalize(dummy2.Matrix);
                                Matrix.Multiply(ref matrix3, ref otherRotation, out matrix4);
                                Matrix.Multiply(ref matrix4, ref matrix, out matrix3);
                                MyOrientedBoundingBox box2 = MyOrientedBoundingBox.Create(new BoundingBox(-vector3, vector3), matrix3);
                                if (box2.Intersects(ref box))
                                {
                                    return true;
                                }
                            }
                            continue;
                        }
                        finally
                        {
                            enumerator2.Dispose();
                            continue;
                        }
                    }
                    else
                    {
                        goto TR_0000;
                    }
                    break;
                }
                return flag;
            }
        TR_0000:
            return false;
        }

        public override bool ConnectionAllowed(ref Vector3I otherBlockPos, ref Vector3I faceNormal, MyCubeBlockDefinition def)
        {
            using (Dictionary<ushort, MySlimBlock>.Enumerator enumerator = this.m_mapIdToBlock.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<ushort, MySlimBlock> current = enumerator.Current;
                    if ((current.Value.FatBlock != null) && current.Value.FatBlock.ConnectionAllowed(ref otherBlockPos, ref faceNormal, def))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool ConnectionAllowed(ref Vector3I otherBlockMinPos, ref Vector3I otherBlockMaxPos, ref Vector3I faceNormal, MyCubeBlockDefinition def)
        {
            using (Dictionary<ushort, MySlimBlock>.Enumerator enumerator = this.m_mapIdToBlock.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<ushort, MySlimBlock> current = enumerator.Current;
                    if ((current.Value.FatBlock != null) && current.Value.FatBlock.ConnectionAllowed(ref otherBlockMinPos, ref otherBlockMaxPos, ref faceNormal, def))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static MyObjectBuilder_CompoundCubeBlock CreateBuilder(List<MyObjectBuilder_CubeBlock> cubeBlockBuilders)
        {
            MyObjectBuilder_CompoundCubeBlock local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_CompoundCubeBlock>(COMPOUND_BLOCK_SUBTYPE_NAME);
            local1.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
            local1.Min = cubeBlockBuilders[0].Min;
            local1.BlockOrientation = new MyBlockOrientation(ref Quaternion.Identity);
            local1.ColorMaskHSV = cubeBlockBuilders[0].ColorMaskHSV;
            local1.Blocks = cubeBlockBuilders.ToArray();
            return local1;
        }

        public static MyObjectBuilder_CompoundCubeBlock CreateBuilder(MyObjectBuilder_CubeBlock cubeBlockBuilder)
        {
            MyObjectBuilder_CompoundCubeBlock local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_CompoundCubeBlock>(COMPOUND_BLOCK_SUBTYPE_NAME);
            local1.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
            local1.Min = cubeBlockBuilder.Min;
            local1.BlockOrientation = new MyBlockOrientation(ref Quaternion.Identity);
            local1.ColorMaskHSV = cubeBlockBuilder.ColorMaskHSV;
            local1.Blocks = new MyObjectBuilder_CubeBlock[] { cubeBlockBuilder };
            return local1;
        }

        private ushort CreateId(MySlimBlock block)
        {
            ushort key = 0;
            if (block.BlockDefinition.IsGeneratedBlock)
            {
                key = (ushort) (this.m_localNextId | BLOCK_IN_COMPOUND_LOCAL_ID);
                while (true)
                {
                    if (!this.m_mapIdToBlock.ContainsKey(key))
                    {
                        this.m_localNextId = (ushort) (this.m_localNextId + 1);
                        break;
                    }
                    this.m_localNextId = (this.m_localNextId != BLOCK_IN_COMPOUND_LOCAL_MAX_VALUE) ? ((ushort) (this.m_localNextId + 1)) : 0;
                    key = (ushort) (this.m_localNextId | BLOCK_IN_COMPOUND_LOCAL_ID);
                }
            }
            else
            {
                key = this.m_nextId;
                while (true)
                {
                    if (!this.m_mapIdToBlock.ContainsKey(key))
                    {
                        this.m_nextId = (ushort) (this.m_nextId + 1);
                        break;
                    }
                    this.m_nextId = (this.m_nextId != BLOCK_IN_COMPOUND_LOCAL_MAX_VALUE) ? ((ushort) (this.m_nextId + 1)) : 0;
                    key = this.m_nextId;
                }
            }
            return key;
        }

        private void DebugDrawAABB(BoundingBox aabb, Matrix localMatrix)
        {
            Matrix matrix = (Matrix.CreateScale((Vector3) (2f * aabb.HalfExtents)) * localMatrix) * base.PositionComp.WorldMatrix;
            MyRenderProxy.DebugDrawAxis(MatrixD.Normalize(matrix), 0.1f, false, false, false);
            MyRenderProxy.DebugDrawOBB(matrix, Color.Green, 0.1f, false, false, true, false);
        }

        private void DebugDrawOBB(MyOrientedBoundingBox obb, Matrix localMatrix)
        {
            Matrix matrix = (Matrix.CreateFromTransformScale(obb.Orientation, obb.Center, (Vector3) (2f * obb.HalfExtent)) * localMatrix) * base.PositionComp.WorldMatrix;
            MyRenderProxy.DebugDrawAxis(MatrixD.Normalize(matrix), 0.1f, false, false, false);
            MyRenderProxy.DebugDrawOBB(matrix, Vector3.One, 0.1f, false, false, true, false);
        }

        internal void DoDamage(float damage, MyStringHash damageType, MyHitInfo? hitInfo, long attackerId)
        {
            float num = 0f;
            foreach (KeyValuePair<ushort, MySlimBlock> pair in this.m_mapIdToBlock)
            {
                num += pair.Value.MaxIntegrity;
            }
            for (int i = this.m_blocks.Count - 1; i >= 0; i--)
            {
                MySlimBlock block = this.m_blocks[i];
                block.DoDamage(damage * (block.MaxIntegrity / num), damageType, hitInfo, true, attackerId);
            }
        }

        public MySlimBlock GetBlock(ushort id)
        {
            MySlimBlock block;
            return (!this.m_mapIdToBlock.TryGetValue(id, out block) ? null : block);
        }

        public ushort? GetBlockId(MySlimBlock block)
        {
            KeyValuePair<ushort, MySlimBlock> pair = this.m_mapIdToBlock.FirstOrDefault<KeyValuePair<ushort, MySlimBlock>>(p => p.Value == block);
            if (pair.Value == block)
            {
                return new ushort?(pair.Key);
            }
            return null;
        }

        private static void GetBlockLocalMatrixFromGridPositionAndOrientation(MySlimBlock block, ref MatrixD localMatrix)
        {
            Matrix matrix;
            block.Orientation.GetMatrix(out matrix);
            localMatrix = matrix;
            localMatrix.Translation = (Vector3D) (block.CubeGrid.GridSize * block.Position);
        }

        public ListReader<MySlimBlock> GetBlocks() => 
            this.m_blocks;

        public int GetBlocksCount() => 
            this.m_blocks.Count;

        private static void GetCompoundCollisionDummies(MyCubeBlockDefinition definition, List<MyModelDummy> outDummies)
        {
            MyModel modelOnlyDummies = MyModels.GetModelOnlyDummies(definition.Model);
            if (modelOnlyDummies != null)
            {
                foreach (KeyValuePair<string, MyModelDummy> pair in modelOnlyDummies.Dummies)
                {
                    if (pair.Key.ToLower().StartsWith(COMPOUND_DUMMY))
                    {
                        outDummies.Add(pair.Value);
                    }
                }
            }
        }

        public static MyCubeBlockDefinition GetCompoundCubeBlockDefinition() => 
            MyDefinitionManager.Static.GetCubeBlockDefinition(new MyDefinitionId(typeof(MyObjectBuilder_CompoundCubeBlock), COMPOUND_BLOCK_SUBTYPE_NAME));

        public override BoundingBox GetGeometryLocalBox()
        {
            BoundingBox box = BoundingBox.CreateInvalid();
            foreach (MySlimBlock block in this.GetBlocks())
            {
                if (block.FatBlock != null)
                {
                    Matrix matrix;
                    block.Orientation.GetMatrix(out matrix);
                    box.Include(block.FatBlock.Model.BoundingBox.Transform(matrix));
                }
            }
            return box;
        }

        public bool GetIntersectionWithLine(ref LineD line, out MyIntersectionResultLineTriangleEx? t, out ushort blockId, IntersectionFlags flags = 3, bool checkZFight = false, bool ignoreGenerated = false)
        {
            t = 0;
            blockId = 0;
            double maxValue = double.MaxValue;
            bool flag = false;
            foreach (KeyValuePair<ushort, MySlimBlock> pair in this.m_mapIdToBlock)
            {
                MyIntersectionResultLineTriangleEx? nullable;
                MySlimBlock block = pair.Value;
                if ((!ignoreGenerated || !block.BlockDefinition.IsGeneratedBlock) && (block.FatBlock.GetIntersectionWithLine(ref line, out nullable, IntersectionFlags.ALL_TRIANGLES) && (nullable != null)))
                {
                    double num2 = (nullable.Value.IntersectionPointInWorldSpace - line.From).LengthSquared();
                    if ((num2 < maxValue) && (!checkZFight || (maxValue >= (num2 + 0.0010000000474974513))))
                    {
                        maxValue = num2;
                        t = nullable;
                        blockId = pair.Key;
                        flag = true;
                    }
                }
            }
            return flag;
        }

        public bool GetIntersectionWithLine_FullyBuiltProgressModels(ref LineD line, out MyIntersectionResultLineTriangleEx? t, out ushort blockId, IntersectionFlags flags = 3, bool checkZFight = false, bool ignoreGenerated = false)
        {
            t = 0;
            blockId = 0;
            double maxValue = double.MaxValue;
            bool flag = false;
            foreach (KeyValuePair<ushort, MySlimBlock> pair in this.m_mapIdToBlock)
            {
                MySlimBlock block = pair.Value;
                if (!ignoreGenerated || !block.BlockDefinition.IsGeneratedBlock)
                {
                    MyModel modelOnlyData = MyModels.GetModelOnlyData(block.BlockDefinition.Model);
                    if (modelOnlyData != null)
                    {
                        MyIntersectionResultLineTriangleEx? nullable = modelOnlyData.GetTrianglePruningStructure().GetIntersectionWithLine(block.FatBlock, ref line, flags);
                        if (nullable != null)
                        {
                            double num2 = (nullable.Value.IntersectionPointInWorldSpace - line.From).LengthSquared();
                            if ((num2 < maxValue) && (!checkZFight || (maxValue >= (num2 + 0.0010000000474974513))))
                            {
                                maxValue = num2;
                                t = nullable;
                                blockId = pair.Key;
                                flag = true;
                            }
                        }
                    }
                }
            }
            return flag;
        }

        public override float GetMass()
        {
            float num = 0f;
            foreach (KeyValuePair<ushort, MySlimBlock> pair in this.m_mapIdToBlock)
            {
                num += pair.Value.GetMass();
            }
            return num;
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_CompoundCubeBlock objectBuilderCubeBlock = (MyObjectBuilder_CompoundCubeBlock) base.GetObjectBuilderCubeBlock(copy);
            if (this.m_mapIdToBlock.Count > 0)
            {
                objectBuilderCubeBlock.Blocks = new MyObjectBuilder_CubeBlock[this.m_mapIdToBlock.Count];
                objectBuilderCubeBlock.BlockIds = new ushort[this.m_mapIdToBlock.Count];
                int index = 0;
                foreach (KeyValuePair<ushort, MySlimBlock> pair in this.m_mapIdToBlock)
                {
                    objectBuilderCubeBlock.BlockIds[index] = pair.Key;
                    objectBuilderCubeBlock.Blocks[index] = copy ? pair.Value.GetCopyObjectBuilder() : pair.Value.GetObjectBuilder(false);
                    index++;
                }
            }
            return objectBuilderCubeBlock;
        }

        private static MyCompoundBlockTemplateDefinition.MyCompoundBlockRotationBinding GetRotationBinding(MyCompoundBlockTemplateDefinition.MyCompoundBlockBinding binding, MyCubeBlockDefinition blockDefinition)
        {
            if (binding.RotationBinds != null)
            {
                foreach (MyCompoundBlockTemplateDefinition.MyCompoundBlockRotationBinding binding2 in binding.RotationBinds)
                {
                    if (binding2.BuildTypeReference == blockDefinition.BuildType)
                    {
                        return binding2;
                    }
                }
            }
            return null;
        }

        private static MyCompoundBlockTemplateDefinition.MyCompoundBlockRotationBinding GetRotationBinding(MyCompoundBlockTemplateDefinition templateDefinition, MyCubeBlockDefinition blockDefinition1, MyCubeBlockDefinition blockDefinition2)
        {
            MyCompoundBlockTemplateDefinition.MyCompoundBlockBinding templateDefinitionBinding = GetTemplateDefinitionBinding(templateDefinition, blockDefinition1);
            if (templateDefinitionBinding == null)
            {
                return null;
            }
            MyCompoundBlockTemplateDefinition.MyCompoundBlockRotationBinding rotationBinding = GetRotationBinding(templateDefinitionBinding, blockDefinition2);
            if (rotationBinding != null)
            {
                return rotationBinding;
            }
            templateDefinitionBinding = GetTemplateDefinitionBinding(templateDefinition, blockDefinition2);
            if (templateDefinitionBinding == null)
            {
                return null;
            }
            return GetRotationBinding(templateDefinitionBinding, blockDefinition1);
        }

        private static MyCompoundBlockTemplateDefinition GetTemplateDefinition(string template) => 
            MyDefinitionManager.Static.GetCompoundBlockTemplateDefinition(new MyDefinitionId(typeof(MyObjectBuilder_CompoundBlockTemplateDefinition), template));

        private static MyCompoundBlockTemplateDefinition.MyCompoundBlockBinding GetTemplateDefinitionBinding(MyCompoundBlockTemplateDefinition templateDefinition, MyCubeBlockDefinition blockDefinition)
        {
            foreach (MyCompoundBlockTemplateDefinition.MyCompoundBlockBinding binding in templateDefinition.Bindings)
            {
                if (binding.BuildType == BUILD_TYPE_ANY)
                {
                    return binding;
                }
            }
            foreach (MyCompoundBlockTemplateDefinition.MyCompoundBlockBinding binding2 in templateDefinition.Bindings)
            {
                if ((binding2.BuildType == blockDefinition.BuildType) && (blockDefinition.BuildType != MyStringId.NullOrEmpty))
                {
                    return binding2;
                }
            }
            return null;
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_CompoundCubeBlock block = objectBuilder as MyObjectBuilder_CompoundCubeBlock;
            if (block.Blocks != null)
            {
                if (block.BlockIds == null)
                {
                    for (int i = 0; i < block.Blocks.Length; i++)
                    {
                        MyObjectBuilder_CubeBlock builder = block.Blocks[i];
                        object obj3 = MyCubeBlockFactory.CreateCubeBlock(builder);
                        MySlimBlock block5 = obj3 as MySlimBlock;
                        if (block5 == null)
                        {
                            block5 = new MySlimBlock();
                        }
                        block5.Init(builder, cubeGrid, obj3 as MyCubeBlock);
                        block5.FatBlock.HookMultiplayer();
                        block5.FatBlock.Hierarchy.Parent = base.Hierarchy;
                        ushort key = this.CreateId(block5);
                        this.m_mapIdToBlock.Add(key, block5);
                        this.m_blocks.Add(block5);
                    }
                }
                else
                {
                    int index = 0;
                    while (true)
                    {
                        if (index >= block.Blocks.Length)
                        {
                            this.RefreshNextId();
                            break;
                        }
                        ushort key = block.BlockIds[index];
                        if (!this.m_mapIdToBlock.ContainsKey(key))
                        {
                            MyObjectBuilder_CubeBlock builder = block.Blocks[index];
                            object obj2 = MyCubeBlockFactory.CreateCubeBlock(builder);
                            MySlimBlock block3 = obj2 as MySlimBlock;
                            if (block3 == null)
                            {
                                block3 = new MySlimBlock();
                            }
                            block3.Init(builder, cubeGrid, obj2 as MyCubeBlock);
                            if (block3.FatBlock != null)
                            {
                                block3.FatBlock.HookMultiplayer();
                                block3.FatBlock.Hierarchy.Parent = base.Hierarchy;
                                this.m_mapIdToBlock.Add(key, block3);
                                this.m_blocks.Add(block3);
                            }
                        }
                        index++;
                    }
                }
            }
            this.RefreshTemplates();
            base.AddDebugRenderComponent(new MyDebugRenderComponentCompoundBlock(this));
        }

        public static bool IsCompoundEnabled(MyCubeBlockDefinition blockDefinition) => 
            (MyFakes.ENABLE_COMPOUND_BLOCKS ? ((blockDefinition != null) ? ((blockDefinition.CubeSize == MyCubeSize.Large) ? (!(blockDefinition.Size != Vector3I.One) ? (!MyFakes.ENABLE_COMPOUND_BLOCK_COLLISION_DUMMIES ? ((blockDefinition.CompoundTemplates != null) && (blockDefinition.CompoundTemplates.Length != 0)) : blockDefinition.CompoundEnabled) : false) : false) : false) : false);

        private bool IsRotationValid(MyBlockOrientation refOrientation, MyBlockOrientation orientation, MyBlockOrientation[] validRotations)
        {
            MatrixI xi2;
            MatrixI xi = new MatrixI(Vector3I.Zero, refOrientation.Forward, refOrientation.Up);
            MatrixI.Invert(ref xi, out xi2);
            Matrix floatMatrix = xi2.GetFloatMatrix();
            Base6Directions.Direction closestDirection = Base6Directions.GetClosestDirection(Vector3.TransformNormal((Vector3) Base6Directions.GetIntVector(orientation.Forward), floatMatrix));
            Base6Directions.Direction direction2 = Base6Directions.GetClosestDirection(Vector3.TransformNormal((Vector3) Base6Directions.GetIntVector(orientation.Up), floatMatrix));
            foreach (MyBlockOrientation orientation2 in validRotations)
            {
                if ((orientation2.Forward == closestDirection) && (orientation2.Up == direction2))
                {
                    return true;
                }
            }
            return false;
        }

        public override void OnAddedToScene(object source)
        {
            foreach (KeyValuePair<ushort, MySlimBlock> pair in this.m_mapIdToBlock)
            {
                if (pair.Value.FatBlock != null)
                {
                    pair.Value.FatBlock.OnAddedToScene(source);
                }
            }
            base.OnAddedToScene(source);
        }

        public override void OnCubeGridChanged(MyCubeGrid oldGrid)
        {
            base.OnCubeGridChanged(oldGrid);
            foreach (KeyValuePair<ushort, MySlimBlock> pair in this.m_mapIdToBlock)
            {
                pair.Value.CubeGrid = base.CubeGrid;
            }
        }

        public override void OnRemovedFromScene(object source)
        {
            foreach (KeyValuePair<ushort, MySlimBlock> pair in this.m_mapIdToBlock)
            {
                if (pair.Value.FatBlock != null)
                {
                    pair.Value.FatBlock.OnRemovedFromScene(source);
                }
            }
            base.OnRemovedFromScene(source);
        }

        internal override void OnTransformed(ref MatrixI transform)
        {
            foreach (KeyValuePair<ushort, MySlimBlock> pair in this.m_mapIdToBlock)
            {
                pair.Value.Transform(ref transform);
            }
        }

        private void RefreshNextId()
        {
            foreach (KeyValuePair<ushort, MySlimBlock> pair in this.m_mapIdToBlock)
            {
                if ((pair.Key & BLOCK_IN_COMPOUND_LOCAL_ID) == BLOCK_IN_COMPOUND_LOCAL_ID)
                {
                    ushort num = pair.Key & ~BLOCK_IN_COMPOUND_LOCAL_ID;
                    this.m_localNextId = Math.Max(this.m_localNextId, num);
                    continue;
                }
                ushort key = pair.Key;
                this.m_nextId = Math.Max(this.m_nextId, key);
            }
            this.m_nextId = (this.m_nextId != BLOCK_IN_COMPOUND_LOCAL_MAX_VALUE) ? ((ushort) (this.m_nextId + 1)) : 0;
            if (this.m_localNextId == BLOCK_IN_COMPOUND_LOCAL_MAX_VALUE)
            {
                this.m_localNextId = 0;
            }
            else
            {
                this.m_localNextId = (ushort) (this.m_localNextId + 1);
            }
        }

        private void RefreshTemplates()
        {
            this.m_templates.Clear();
            if (!MyFakes.ENABLE_COMPOUND_BLOCK_COLLISION_DUMMIES)
            {
                foreach (KeyValuePair<ushort, MySlimBlock> pair in this.m_mapIdToBlock)
                {
                    if (pair.Value.BlockDefinition.CompoundTemplates != null)
                    {
                        string[] compoundTemplates;
                        int num;
                        if (this.m_templates.Count == 0)
                        {
                            compoundTemplates = pair.Value.BlockDefinition.CompoundTemplates;
                            num = 0;
                            while (num < compoundTemplates.Length)
                            {
                                string item = compoundTemplates[num];
                                this.m_templates.Add(item);
                                num++;
                            }
                            continue;
                        }
                        m_tmpTemplates.Clear();
                        compoundTemplates = pair.Value.BlockDefinition.CompoundTemplates;
                        num = 0;
                        while (true)
                        {
                            if (num >= compoundTemplates.Length)
                            {
                                this.m_templates.IntersectWith(m_tmpTemplates);
                                break;
                            }
                            string item = compoundTemplates[num];
                            m_tmpTemplates.Add(item);
                            num++;
                        }
                    }
                }
            }
        }

        public bool Remove(MySlimBlock block, bool merged = false)
        {
            KeyValuePair<ushort, MySlimBlock> pair = this.m_mapIdToBlock.FirstOrDefault<KeyValuePair<ushort, MySlimBlock>>(p => p.Value == block);
            return ((pair.Value == block) && this.Remove(pair.Key, merged));
        }

        public bool Remove(ushort blockId, bool merged = false)
        {
            MySlimBlock block;
            if (!this.m_mapIdToBlock.TryGetValue(blockId, out block))
            {
                return false;
            }
            this.m_mapIdToBlock.Remove(blockId);
            this.m_blocks.Remove(block);
            if (!merged)
            {
                if (block.IsMultiBlockPart)
                {
                    base.CubeGrid.RemoveMultiBlockInfo(block);
                }
                block.FatBlock.OnRemovedFromScene(this);
                block.FatBlock.Close();
            }
            if (ReferenceEquals(block.FatBlock.Hierarchy.Parent, base.Hierarchy))
            {
                block.FatBlock.Hierarchy.Parent = null;
            }
            if (!merged)
            {
                base.CubeGrid.UpdateBlockNeighbours(base.SlimBlock);
            }
            this.RefreshTemplates();
            return true;
        }

        private void UpdateBlocksWorldMatrix(ref MatrixD parentWorldMatrix, object source = null)
        {
            MatrixD identity = MatrixD.Identity;
            foreach (KeyValuePair<ushort, MySlimBlock> pair in this.m_mapIdToBlock)
            {
                if (pair.Value.FatBlock != null)
                {
                    GetBlockLocalMatrixFromGridPositionAndOrientation(pair.Value, ref identity);
                    MatrixD worldMatrix = identity * parentWorldMatrix;
                    pair.Value.FatBlock.PositionComp.SetWorldMatrix(worldMatrix, this, true, true, true, false, false, false);
                }
            }
        }

        public override void UpdateVisual()
        {
            foreach (KeyValuePair<ushort, MySlimBlock> pair in this.m_mapIdToBlock)
            {
                if (pair.Value.FatBlock != null)
                {
                    pair.Value.FatBlock.UpdateVisual();
                }
            }
            base.UpdateVisual();
        }

        internal override void UpdateWorldMatrix()
        {
            base.UpdateWorldMatrix();
            foreach (KeyValuePair<ushort, MySlimBlock> pair in this.m_mapIdToBlock)
            {
                if (pair.Value.FatBlock != null)
                {
                    pair.Value.FatBlock.UpdateWorldMatrix();
                }
            }
        }

        void IMyDecalProxy.AddDecals(ref MyHitInfo hitInfo, MyStringHash source, object customdata, IMyDecalHandler decalHandler, MyStringHash material)
        {
            MyCubeGrid.MyCubeGridHitInfo gridHitInfo = customdata as MyCubeGrid.MyCubeGridHitInfo;
            if (gridHitInfo != null)
            {
                MyPhysicalMaterialDefinition physicalMaterial = this.m_mapIdToBlock.First<KeyValuePair<ushort, MySlimBlock>>().Value.BlockDefinition.PhysicalMaterial;
                MyDecalRenderInfo renderInfo = new MyDecalRenderInfo {
                    Position = Vector3D.Transform(hitInfo.Position, base.CubeGrid.PositionComp.WorldMatrixInvScaled),
                    Normal = (Vector3) Vector3D.TransformNormal(hitInfo.Normal, base.CubeGrid.PositionComp.WorldMatrixInvScaled),
                    RenderObjectIds = base.CubeGrid.Render.RenderObjectIDs,
                    Source = source
                };
                VertexBoneIndicesWeights? affectingBoneIndicesWeights = gridHitInfo.Triangle.GetAffectingBoneIndicesWeights(ref m_boneIndexWeightTmp);
                if (affectingBoneIndicesWeights != null)
                {
                    renderInfo.BoneIndices = affectingBoneIndicesWeights.Value.Indices;
                    renderInfo.BoneWeights = affectingBoneIndicesWeights.Value.Weights;
                }
                renderInfo.Material = (material.GetHashCode() != 0) ? material : MyStringHash.GetOrCompute(physicalMaterial.Id.SubtypeName);
                m_tmpIds.Clear();
                decalHandler.AddDecal(ref renderInfo, m_tmpIds);
                foreach (uint num in m_tmpIds)
                {
                    base.CubeGrid.RenderData.AddDecal(base.Position, gridHitInfo, num);
                }
            }
        }

        private class MyCompoundBlockPosComponent : MyCubeBlock.MyBlockPosComponent
        {
            private MyCompoundCubeBlock m_block;

            public override void OnAddedToContainer()
            {
                base.OnAddedToContainer();
                this.m_block = base.Container.Entity as MyCompoundCubeBlock;
            }

            public override void UpdateWorldMatrix(ref MatrixD parentWorldMatrix, object source = null, bool updateChildren = true, bool forceUpdateAllChildren = false)
            {
                this.m_block.UpdateBlocksWorldMatrix(ref parentWorldMatrix, source);
                base.UpdateWorldMatrix(ref parentWorldMatrix, source, updateChildren, forceUpdateAllChildren);
            }
        }

        private class MyDebugRenderComponentCompoundBlock : MyDebugRenderComponent
        {
            private readonly MyCompoundCubeBlock m_compoundBlock;

            public MyDebugRenderComponentCompoundBlock(MyCompoundCubeBlock compoundBlock) : base(compoundBlock)
            {
                this.m_compoundBlock = compoundBlock;
            }

            public override void DebugDraw()
            {
                foreach (MySlimBlock block in this.m_compoundBlock.GetBlocks())
                {
                    if (block.FatBlock != null)
                    {
                        block.FatBlock.DebugDraw();
                    }
                }
            }
        }
    }
}

