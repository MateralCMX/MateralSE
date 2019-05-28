namespace Sandbox.Engine.Physics
{
    using Havok;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Replication;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    public static class MyDestructionHelper
    {
        public static readonly float MASS_REDUCTION_COEF = 0.04f;
        private static List<HkdShapeInstanceInfo> m_tmpInfos = new List<HkdShapeInstanceInfo>();
        private static List<HkdShapeInstanceInfo> m_tmpInfos2 = new List<HkdShapeInstanceInfo>();

        private static bool ContainsBlockWithoutGeneratedFracturedPieces(MyCubeBlock block)
        {
            bool flag;
            if (!block.BlockDefinition.CreateFracturedPieces)
            {
                return true;
            }
            if (block is MyCompoundCubeBlock)
            {
                using (List<MySlimBlock>.Enumerator enumerator = (block as MyCompoundCubeBlock).GetBlocks().GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        if (!enumerator.Current.BlockDefinition.CreateFracturedPieces)
                        {
                            return true;
                        }
                    }
                }
            }
            if (block is MyFracturedBlock)
            {
                using (List<MyDefinitionId>.Enumerator enumerator2 = (block as MyFracturedBlock).OriginalBlocks.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator2.MoveNext())
                        {
                            break;
                        }
                        MyDefinitionId current = enumerator2.Current;
                        if (!MyDefinitionManager.Static.GetCubeBlockDefinition(current).CreateFracturedPieces)
                        {
                            return true;
                        }
                    }
                }
                goto TR_0001;
            }
            else
            {
                goto TR_0001;
            }
            return flag;
        TR_0001:
            return false;
        }

        public static MyFracturedPiece CreateFracturePiece(MyFracturedBlock fracturedBlock, bool sync)
        {
            MyPhysicalModelDefinition definition;
            MatrixD worldMatrix = fracturedBlock.CubeGrid.PositionComp.WorldMatrix;
            worldMatrix.Translation = fracturedBlock.CubeGrid.GridIntegerToWorld(fracturedBlock.Position);
            MyFracturedPiece piece = CreateFracturePiece(ref fracturedBlock.Shape, ref worldMatrix, false);
            piece.OriginalBlocks = fracturedBlock.OriginalBlocks;
            if (MyDefinitionManager.Static.TryGetDefinition<MyPhysicalModelDefinition>(piece.OriginalBlocks[0], out definition))
            {
                piece.Physics.MaterialType = definition.PhysicalMaterial.Id.SubtypeId;
            }
            if (MyFakes.ENABLE_FRACTURE_PIECE_SHAPE_CHECK)
            {
                piece.DebugCheckValidShapes();
            }
            if (MyExternalReplicable.FindByObject(piece) == null)
            {
                Sandbox.Game.Entities.MyEntities.RaiseEntityCreated(piece);
            }
            Sandbox.Game.Entities.MyEntities.Add(piece, true);
            return piece;
        }

        public static MyFracturedPiece CreateFracturePiece(MyFractureComponentCubeBlock fractureBlockComponent, bool sync)
        {
            MyPhysicalModelDefinition definition;
            if (!fractureBlockComponent.Block.BlockDefinition.CreateFracturedPieces)
            {
                return null;
            }
            if (!fractureBlockComponent.Shape.IsValid())
            {
                MyLog.Default.WriteLine("Invalid shape in fracture component, Id: " + fractureBlockComponent.Block.BlockDefinition.Id.ToString() + ", closed: " + fractureBlockComponent.Block.FatBlock.Closed.ToString());
                return null;
            }
            MatrixD worldMatrix = fractureBlockComponent.Block.FatBlock.WorldMatrix;
            MyFracturedPiece piece = CreateFracturePiece(ref fractureBlockComponent.Shape, ref worldMatrix, false);
            piece.OriginalBlocks.Add(fractureBlockComponent.Block.BlockDefinition.Id);
            if (MyFakes.ENABLE_FRACTURE_PIECE_SHAPE_CHECK)
            {
                piece.DebugCheckValidShapes();
            }
            if (MyDefinitionManager.Static.TryGetDefinition<MyPhysicalModelDefinition>(piece.OriginalBlocks[0], out definition))
            {
                piece.Physics.MaterialType = definition.PhysicalMaterial.Id.SubtypeId;
            }
            if (MyExternalReplicable.FindByObject(piece) == null)
            {
                Sandbox.Game.Entities.MyEntities.RaiseEntityCreated(piece);
            }
            Sandbox.Game.Entities.MyEntities.Add(piece, true);
            return piece;
        }

        private static MyFracturedPiece CreateFracturePiece(ref HkdBreakableShape shape, ref MatrixD worldMatrix, bool isStatic)
        {
            MyFracturedPiece pieceFromPool = MyFracturedPiecesManager.Static.GetPieceFromPool(0L, false);
            pieceFromPool.PositionComp.WorldMatrix = worldMatrix;
            pieceFromPool.Physics.Flags = isStatic ? RigidBodyFlag.RBF_STATIC : RigidBodyFlag.RBF_DEBRIS;
            MyPhysicsBody physics = pieceFromPool.Physics;
            HkMassProperties massProperties = new HkMassProperties();
            shape.BuildMassProperties(ref massProperties);
            physics.InitialSolverDeactivation = HkSolverDeactivation.High;
            physics.CreateFromCollisionObject(shape.GetShape(), Vector3.Zero, worldMatrix, new HkMassProperties?(massProperties), 15);
            physics.LinearDamping = MyPerGameSettings.DefaultLinearDamping;
            physics.AngularDamping = MyPerGameSettings.DefaultAngularDamping;
            physics.BreakableBody = new HkdBreakableBody(shape, physics.RigidBody, null, (Matrix) worldMatrix);
            physics.BreakableBody.AfterReplaceBody += new BreakableBodyReplaced(physics.FracturedBody_AfterReplaceBody);
            if (pieceFromPool.SyncFlag)
            {
                pieceFromPool.CreateSync();
            }
            pieceFromPool.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            pieceFromPool.SetDataFromHavok(shape);
            pieceFromPool.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            shape.RemoveReference();
            return pieceFromPool;
        }

        public static MyFracturedPiece CreateFracturePiece(HkdBreakableBody b, ref MatrixD worldMatrix, List<MyDefinitionId> originalBlocks, MyCubeBlock block = null, bool sync = true)
        {
            if (IsBodyWithoutGeneratedFracturedPieces(b, block))
            {
                return null;
            }
            MyFracturedPiece pieceFromPool = MyFracturedPiecesManager.Static.GetPieceFromPool(0L, false);
            pieceFromPool.InitFromBreakableBody(b, worldMatrix, block);
            pieceFromPool.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            if ((originalBlocks != null) && (originalBlocks.Count != 0))
            {
                MyPhysicalModelDefinition definition;
                pieceFromPool.OriginalBlocks.Clear();
                pieceFromPool.OriginalBlocks.AddRange(originalBlocks);
                if (MyDefinitionManager.Static.TryGetDefinition<MyPhysicalModelDefinition>(originalBlocks[0], out definition))
                {
                    pieceFromPool.Physics.MaterialType = definition.PhysicalMaterial.Id.SubtypeId;
                }
            }
            if (MyFakes.ENABLE_FRACTURE_PIECE_SHAPE_CHECK)
            {
                pieceFromPool.DebugCheckValidShapes();
            }
            if (MyExternalReplicable.FindByObject(pieceFromPool) == null)
            {
                Sandbox.Game.Entities.MyEntities.RaiseEntityCreated(pieceFromPool);
            }
            Sandbox.Game.Entities.MyEntities.Add(pieceFromPool, true);
            return pieceFromPool;
        }

        public static MyFracturedPiece CreateFracturePiece(HkdBreakableShape shape, ref MatrixD worldMatrix, bool isStatic, MyDefinitionId? definition, bool sync)
        {
            MyFracturedPiece piece = CreateFracturePiece(ref shape, ref worldMatrix, isStatic);
            if (definition == null)
            {
                piece.Save = false;
            }
            else
            {
                MyPhysicalModelDefinition definition2;
                piece.OriginalBlocks.Clear();
                piece.OriginalBlocks.Add(definition.Value);
                if (MyDefinitionManager.Static.TryGetDefinition<MyPhysicalModelDefinition>(definition.Value, out definition2))
                {
                    piece.Physics.MaterialType = definition2.PhysicalMaterial.Id.SubtypeId;
                }
            }
            if (piece.Save && MyFakes.ENABLE_FRACTURE_PIECE_SHAPE_CHECK)
            {
                piece.DebugCheckValidShapes();
            }
            if (MyExternalReplicable.FindByObject(piece) == null)
            {
                Sandbox.Game.Entities.MyEntities.RaiseEntityCreated(piece);
            }
            Sandbox.Game.Entities.MyEntities.Add(piece, true);
            return piece;
        }

        private static bool DontCreateFracture(HkdBreakableShape breakableShape) => 
            (breakableShape.IsValid() ? ((breakableShape.UserObject & 2) != 0) : false);

        public static unsafe void FixPosition(MyFracturedPiece fp)
        {
            HkdBreakableShape breakableShape = fp.Physics.BreakableBody.BreakableShape;
            if (breakableShape.GetChildrenCount() != 0)
            {
                breakableShape.GetChildren(m_tmpInfos);
                Vector3 translation = m_tmpInfos[0].GetTransform().Translation;
                if (translation.LengthSquared() < 1f)
                {
                    m_tmpInfos.Clear();
                }
                else
                {
                    List<HkdConnection> resultList = new List<HkdConnection>();
                    HashSet<HkdBreakableShape> set = new HashSet<HkdBreakableShape>();
                    HashSet<HkdBreakableShape> set2 = new HashSet<HkdBreakableShape>();
                    set.Add(breakableShape);
                    breakableShape.GetConnectionList(resultList);
                    fp.PositionComp.SetPosition(Vector3D.Transform(translation, fp.PositionComp.WorldMatrix), null, false, true);
                    foreach (HkdShapeInstanceInfo info2 in m_tmpInfos)
                    {
                        Matrix transform = info2.GetTransform();
                        Matrix* matrixPtr1 = (Matrix*) ref transform;
                        matrixPtr1.Translation -= translation;
                        info2.SetTransform(ref transform);
                        m_tmpInfos2.Add(info2);
                        HkdBreakableShape shape = info2.Shape;
                        shape.GetConnectionList(resultList);
                        while (true)
                        {
                            if (!shape.HasParent)
                            {
                                set2.Add(info2.Shape);
                                break;
                            }
                            shape = shape.GetParent();
                            if (set.Add(shape))
                            {
                                shape.GetConnectionList(resultList);
                            }
                        }
                    }
                    m_tmpInfos.Clear();
                    HkdBreakableShape parent = (HkdBreakableShape) new HkdCompoundBreakableShape(new HkdBreakableShape?(breakableShape), m_tmpInfos2);
                    parent.RecalcMassPropsFromChildren();
                    ((HkdBreakableShape*) ref parent).SetChildrenParent(parent);
                    foreach (HkdConnection connection in resultList)
                    {
                        HkBaseSystem.EnableAssert(0x1745920b, true);
                        if (set2.Contains(connection.ShapeA) && set2.Contains(connection.ShapeB))
                        {
                            HkdConnection connection2 = connection;
                            parent.AddConnection(ref connection2);
                        }
                    }
                    fp.Physics.BreakableBody.BreakableShape = parent;
                    m_tmpInfos2.Clear();
                    parent.RecalcMassPropsFromChildren();
                }
            }
        }

        private static bool IsBodyWithoutGeneratedFracturedPieces(HkdBreakableBody b, MyCubeBlock block)
        {
            if (MyFakes.REMOVE_GENERATED_BLOCK_FRACTURES && ((block == null) || ContainsBlockWithoutGeneratedFracturedPieces(block)))
            {
                if (!b.BreakableShape.IsCompound())
                {
                    return DontCreateFracture(b.BreakableShape);
                }
                b.BreakableShape.GetChildren(m_tmpInfos);
                int index = m_tmpInfos.Count - 1;
                while (true)
                {
                    if (index >= 0)
                    {
                        HkdShapeInstanceInfo info = m_tmpInfos[index];
                        if (DontCreateFracture(info.Shape))
                        {
                            m_tmpInfos.RemoveAt(index);
                            index--;
                            continue;
                        }
                    }
                    if (m_tmpInfos.Count == 0)
                    {
                        return true;
                    }
                    m_tmpInfos.Clear();
                    break;
                }
            }
            return false;
        }

        public static bool IsFixed(HkdBreakableBodyInfo breakableBodyInfo)
        {
            new HkdBreakableBodyHelper(breakableBodyInfo).GetChildren(m_tmpInfos2);
            using (List<HkdShapeInstanceInfo>.Enumerator enumerator = m_tmpInfos2.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    HkdShapeInstanceInfo current = enumerator.Current;
                    if (IsFixed(current.Shape))
                    {
                        m_tmpInfos2.Clear();
                        return true;
                    }
                }
            }
            m_tmpInfos2.Clear();
            return false;
        }

        public static bool IsFixed(HkdBreakableShape breakableShape)
        {
            if (breakableShape.IsValid())
            {
                if ((breakableShape.UserObject & 4) != 0)
                {
                    return true;
                }
                breakableShape.GetChildren(m_tmpInfos);
                using (List<HkdShapeInstanceInfo>.Enumerator enumerator = m_tmpInfos.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        HkdShapeInstanceInfo current = enumerator.Current;
                        HkdBreakableShape shape = current.Shape;
                        if ((shape.UserObject & 4) != 0)
                        {
                            m_tmpInfos.Clear();
                            return true;
                        }
                    }
                }
                m_tmpInfos.Clear();
            }
            return false;
        }

        public static float MassFromHavok(float m) => 
            (!MyPerGameSettings.Destruction ? m : (m / MASS_REDUCTION_COEF));

        public static float MassToHavok(float m) => 
            (!MyPerGameSettings.Destruction ? m : (m * MASS_REDUCTION_COEF));

        public static unsafe void TriggerDestruction(HkWorld world, HkRigidBody body, Vector3 havokPosition, float radius = 0.0005f)
        {
            HkdFractureImpactDetails details = HkdFractureImpactDetails.Create();
            details.SetBreakingBody(body);
            details.SetContactPoint(havokPosition);
            details.SetDestructionRadius(radius);
            details.SetBreakingImpulse(MyDestructionConstants.STRENGTH * 10f);
            HkdFractureImpactDetails* detailsPtr1 = (HkdFractureImpactDetails*) ref details;
            detailsPtr1.Flag = details.Flag | HkdFractureImpactDetails.Flags.FLAG_DONT_RECURSE;
            MyPhysics.FractureImpactDetails details2 = new MyPhysics.FractureImpactDetails {
                Details = details,
                World = world
            };
            MyPhysics.EnqueueDestruction(details2);
        }

        public static unsafe void TriggerDestruction(float destructionImpact, MyPhysicsBody body, Vector3D position, Vector3 normal, float maxDestructionRadius)
        {
            if (body.BreakableBody != null)
            {
                float mass = body.Mass;
                HkdFractureImpactDetails details2 = HkdFractureImpactDetails.Create();
                details2.SetBreakingBody(body.RigidBody);
                details2.SetContactPoint((Vector3) body.WorldToCluster(position));
                details2.SetDestructionRadius(Math.Min(destructionImpact / 8000f, maxDestructionRadius));
                details2.SetBreakingImpulse(MyDestructionConstants.STRENGTH + (destructionImpact / 10000f));
                details2.SetParticleExpandVelocity(Math.Min((float) (destructionImpact / 10000f), (float) 3f));
                details2.SetParticlePosition((Vector3) body.WorldToCluster(position - (normal * 0.25f)));
                details2.SetParticleMass(1E+07f);
                details2.ZeroCollidingParticleVelocity();
                HkdFractureImpactDetails* detailsPtr1 = (HkdFractureImpactDetails*) ref details2;
                detailsPtr1.Flag = (details2.Flag | HkdFractureImpactDetails.Flags.FLAG_DONT_RECURSE) | HkdFractureImpactDetails.Flags.FLAG_TRIGGERED_DESTRUCTION;
                MyPhysics.FractureImpactDetails details = new MyPhysics.FractureImpactDetails {
                    Details = details2,
                    World = body.HavokWorld,
                    ContactInWorld = position,
                    Entity = (VRage.Game.Entity.MyEntity) body.Entity
                };
                MyPhysics.EnqueueDestruction(details);
            }
        }
    }
}

