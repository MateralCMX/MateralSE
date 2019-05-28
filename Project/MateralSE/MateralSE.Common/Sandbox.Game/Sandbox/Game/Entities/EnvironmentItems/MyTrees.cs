namespace Sandbox.Game.Entities.EnvironmentItems
{
    using Havok;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Debris;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders.Definitions;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyEntityType(typeof(MyObjectBuilder_TreesMedium), false), MyEntityType(typeof(MyObjectBuilder_Trees), true), StaticEventOwner]
    public class MyTrees : MyEnvironmentItems, IMyDecalProxy
    {
        private List<MyCutTreeInfo> m_cutTreeInfos = new List<MyCutTreeInfo>();
        private const float MAX_TREE_CUT_DURATION = 60f;
        private const int BrokenTreeLifeSpan = 0x4e20;

        public static void ApplyImpulseToTreeFracture(ref MatrixD worldMatrix, ref Vector3 hitNormal, List<HkdShapeInstanceInfo> shapeList, ref HkdBreakableShape compound, MyFracturedPiece fp, float forceMultiplier = 1f)
        {
            float mass = compound.GetMass();
            Vector3 coMMaxY = Vector3.MinValue;
            shapeList.ForEach(s => coMMaxY = (s.CoM.Y > coMMaxY.Y) ? s.CoM : coMMaxY);
            Vector3 vector = hitNormal;
            vector.Y = 0f;
            vector.Normalize();
            Vector3 impulse = (Vector3) (((0.3f * forceMultiplier) * mass) * vector);
            fp.Physics.Enabled = true;
            fp.Physics.RigidBody.AngularDamping = MyPerGameSettings.DefaultAngularDamping;
            fp.Physics.RigidBody.LinearDamping = MyPerGameSettings.DefaultLinearDamping;
            fp.Physics.RigidBody.ApplyPointImpulse(impulse, (Vector3) fp.Physics.WorldToCluster(Vector3D.Transform(coMMaxY, worldMatrix)));
        }

        private void CreateBreakableShape(MyEnvironmentItemDefinition itemDefinition, ref MyEnvironmentItems.MyEnvironmentItemData itemData, ref Vector3D hitWorldPosition, Vector3 hitNormal, float forceMultiplier, string fallSound = "")
        {
            HkdBreakableShape oldBreakableShape = MyModels.GetModelOnlyData(itemDefinition.Model).HavokBreakableShapes[0].Clone();
            MatrixD transformMatrix = itemData.Transform.TransformMatrix;
            oldBreakableShape.SetMassRecursively(500f);
            oldBreakableShape.SetStrenghtRecursively(5000f, 0.7f);
            oldBreakableShape.GetChildren(base.m_childrenTmp);
            HkdBreakableShape[] havokBreakableShapes = MyModels.GetModelOnlyData(itemDefinition.Model).HavokBreakableShapes;
            Vector3D.Transform(hitWorldPosition, MatrixD.Normalize(MatrixD.Invert(transformMatrix)));
            float num = (float) (hitWorldPosition.Y - itemData.Transform.Position.Y);
            List<HkdShapeInstanceInfo> shapeList = new List<HkdShapeInstanceInfo>();
            List<HkdShapeInstanceInfo> list2 = new List<HkdShapeInstanceInfo>();
            HkdShapeInstanceInfo? nullable = null;
            foreach (HkdShapeInstanceInfo info in base.m_childrenTmp)
            {
                if ((nullable == null) || (info.CoM.Y < nullable.Value.CoM.Y))
                {
                    nullable = new HkdShapeInstanceInfo?(info);
                }
                if (info.CoM.Y > num)
                {
                    list2.Add(info);
                }
                else
                {
                    shapeList.Add(info);
                }
            }
            if (shapeList.Count != 2)
            {
                if ((shapeList.Count == 0) && list2.Remove(nullable.Value))
                {
                    shapeList.Add(nullable.Value);
                }
            }
            else if ((shapeList[0].CoM.Y < shapeList[1].CoM.Y) && (num < (shapeList[1].CoM.Y + 1.25f)))
            {
                list2.Insert(0, shapeList[1]);
                shapeList.RemoveAt(1);
            }
            else if ((shapeList[0].CoM.Y > shapeList[1].CoM.Y) && (num < (shapeList[0].CoM.Y + 1.25f)))
            {
                list2.Insert(0, shapeList[0]);
                shapeList.RemoveAt(0);
            }
            if (shapeList.Count > 0)
            {
                CreateFracturePiece(itemDefinition, oldBreakableShape, transformMatrix, hitNormal, shapeList, forceMultiplier, true, "");
            }
            if (list2.Count > 0)
            {
                CreateFracturePiece(itemDefinition, oldBreakableShape, transformMatrix, hitNormal, list2, forceMultiplier, false, fallSound);
            }
            base.m_childrenTmp.Clear();
        }

        public static void CreateFracturePiece(MyEnvironmentItemDefinition itemDefinition, HkdBreakableShape oldBreakableShape, MatrixD worldMatrix, Vector3 hitNormal, List<HkdShapeInstanceInfo> shapeList, float forceMultiplier, bool canContainFixedChildren, string fallSound = "")
        {
            bool isStatic = false;
            if (canContainFixedChildren)
            {
                foreach (HkdShapeInstanceInfo info in shapeList)
                {
                    info.Shape.SetMotionQualityRecursively(HkdBreakableShape.BodyQualityType.QUALITY_DEBRIS);
                    Vector3D translation = worldMatrix.Translation + (worldMatrix.Up * 1.5);
                    Quaternion rotation = Quaternion.CreateFromRotationMatrix(worldMatrix.GetOrientation());
                    MyPhysics.GetPenetrationsShape(info.Shape.GetShape(), ref translation, ref rotation, MyEnvironmentItems.m_tmpResults, 15);
                    bool flag2 = false;
                    using (List<HkBodyCollision>.Enumerator enumerator2 = MyEnvironmentItems.m_tmpResults.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            if (enumerator2.Current.GetCollisionEntity() is MyVoxelMap)
                            {
                                info.Shape.SetFlagRecursively(HkdBreakableShape.Flags.IS_FIXED);
                                isStatic = true;
                                flag2 = true;
                            }
                            else if (!flag2)
                            {
                                continue;
                            }
                            break;
                        }
                    }
                    MyEnvironmentItems.m_tmpResults.Clear();
                }
            }
            HkdBreakableShape shape = (HkdBreakableShape) new HkdCompoundBreakableShape(new HkdBreakableShape?(oldBreakableShape), shapeList);
            shape.RecalcMassPropsFromChildren();
            MyFracturedPiece fp = MyDestructionHelper.CreateFracturePiece(shape, ref worldMatrix, isStatic, new MyDefinitionId?(itemDefinition.Id), true);
            if ((fp != null) && !canContainFixedChildren)
            {
                ApplyImpulseToTreeFracture(ref worldMatrix, ref hitNormal, shapeList, ref shape, fp, forceMultiplier);
                fp.Physics.ForceActivate();
                if (fallSound.Length > 0)
                {
                    fp.StartFallSound(fallSound);
                }
            }
        }

        private void CutTree(int itemInstanceId, Vector3D hitWorldPosition, Vector3 hitNormal, float forceMultiplier = 1f)
        {
            int num;
            HkStaticCompoundShape shape = (HkStaticCompoundShape) base.Physics.RigidBody.GetShape();
            if (base.m_localIdToPhysicsShapeInstanceId.TryGetValue(itemInstanceId, out num))
            {
                MyEnvironmentItems.MyEnvironmentItemData itemData = base.m_itemsData[itemInstanceId];
                MyDefinitionId id = new MyDefinitionId(base.Definition.ItemDefinitionType, itemData.SubtypeId);
                MyTreeDefinition environmentItemDefinition = (MyTreeDefinition) MyDefinitionManager.Static.GetEnvironmentItemDefinition(id);
                if ((base.RemoveItem(itemInstanceId, num, true, true) && ((environmentItemDefinition != null) && (environmentItemDefinition.BreakSound != null))) && (environmentItemDefinition.BreakSound.Length > 0))
                {
                    EndpointId targetEndpoint = new EndpointId();
                    MyMultiplayer.RaiseStaticEvent<Vector3D, string>(s => new Action<Vector3D, string>(MyTrees.PlaySound), hitWorldPosition, environmentItemDefinition.BreakSound, targetEndpoint, new Vector3D?(hitWorldPosition));
                }
                if (MyPerGameSettings.Destruction && (MyModels.GetModelOnlyData(environmentItemDefinition.Model).HavokBreakableShapes != null))
                {
                    if ((environmentItemDefinition.FallSound != null) && (environmentItemDefinition.FallSound.Length > 0))
                    {
                        this.CreateBreakableShape(environmentItemDefinition, ref itemData, ref hitWorldPosition, hitNormal, forceMultiplier, environmentItemDefinition.FallSound);
                    }
                    else
                    {
                        this.CreateBreakableShape(environmentItemDefinition, ref itemData, ref hitWorldPosition, hitNormal, forceMultiplier, "");
                    }
                }
            }
        }

        protected override unsafe MyEntity DestroyItem(int itemInstanceId)
        {
            int num;
            MyEntity entity;
            if (!base.m_localIdToPhysicsShapeInstanceId.TryGetValue(itemInstanceId, out num))
            {
                num = -1;
            }
            MyEnvironmentItems.MyEnvironmentItemData data = base.m_itemsData[itemInstanceId];
            base.RemoveItem(itemInstanceId, num, false, true);
            Vector3D position = data.Transform.Position;
            string modelAsset = data.Model.AssetName.Insert(data.Model.AssetName.Length - 4, "_broken");
            bool flag = false;
            if (MyModels.GetModelOnlyData(modelAsset) == null)
            {
                entity = MyDebris.Static.CreateDebris(data.Model.AssetName);
            }
            else
            {
                flag = true;
                entity = MyDebris.Static.CreateDebris(modelAsset);
            }
            MyDebrisBase.MyDebrisBaseLogic gameLogic = entity.GameLogic as MyDebrisBase.MyDebrisBaseLogic;
            gameLogic.LifespanInMiliseconds = 0x4e20;
            MatrixD xd = MatrixD.CreateFromQuaternion(data.Transform.Rotation);
            MatrixD* xdPtr1 = (MatrixD*) ref xd;
            xdPtr1.Translation = position + (xd.Up * (flag ? ((double) 0) : ((double) 5)));
            gameLogic.Start(xd, Vector3.Zero, false);
            return entity;
        }

        public override unsafe void DoDamage(float damage, int itemInstanceId, Vector3D position, Vector3 normal, MyStringHash type)
        {
            MyParticleEffect effect;
            MyEnvironmentItems.MyEnvironmentItemData data = base.m_itemsData[itemInstanceId];
            MyDefinitionId id = new MyDefinitionId(base.Definition.ItemDefinitionType, data.SubtypeId);
            MyTreeDefinition environmentItemDefinition = (MyTreeDefinition) MyDefinitionManager.Static.GetEnvironmentItemDefinition(id);
            MyParticlesManager.TryCreateParticleEffect(environmentItemDefinition.CutEffect, MatrixD.CreateWorld(position, Vector3.CalculatePerpendicularVector(normal), normal), out effect);
            if (Sync.IsServer)
            {
                MyCutTreeInfo item = new MyCutTreeInfo();
                int index = -1;
                int num2 = 0;
                while (true)
                {
                    if (num2 < this.m_cutTreeInfos.Count)
                    {
                        item = this.m_cutTreeInfos[num2];
                        if (itemInstanceId != item.ItemInstanceId)
                        {
                            num2++;
                            continue;
                        }
                        index = num2;
                    }
                    if (index == -1)
                    {
                        item = new MyCutTreeInfo {
                            ItemInstanceId = itemInstanceId
                        };
                        MyCutTreeInfo* infoPtr1 = (MyCutTreeInfo*) ref item;
                        infoPtr1->MaxPoints = item.HitPoints = environmentItemDefinition.HitPoints;
                        index = this.m_cutTreeInfos.Count;
                        this.m_cutTreeInfos.Add(item);
                    }
                    item.LastHit = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                    float* singlePtr1 = (float*) ref item.HitPoints;
                    singlePtr1[0] -= damage;
                    if (item.Progress < 1f)
                    {
                        this.m_cutTreeInfos[index] = item;
                        return;
                    }
                    this.CutTree(itemInstanceId, position, normal, (type == MyDamageType.Drill) ? 1f : 4f);
                    this.m_cutTreeInfos.RemoveAtFast<MyCutTreeInfo>(index);
                    return;
                }
            }
        }

        public static bool IsEntityFracturedTree(IMyEntity entity)
        {
            if ((!(entity is MyFracturedPiece) || ((((MyFracturedPiece) entity).OriginalBlocks == null) || (((MyFracturedPiece) entity).OriginalBlocks.Count <= 0))) || (((((MyFracturedPiece) entity).OriginalBlocks[0].TypeId != typeof(MyObjectBuilder_Tree)) && (((MyFracturedPiece) entity).OriginalBlocks[0].TypeId != typeof(MyObjectBuilder_DestroyableItem))) && (((MyFracturedPiece) entity).OriginalBlocks[0].TypeId != typeof(MyObjectBuilder_TreeDefinition))))
            {
                return false;
            }
            return (((MyFracturedPiece) entity).Physics != null);
        }

        protected override void OnRemoveItem(int instanceId, ref Matrix matrix, MyStringHash myStringId, int userData)
        {
            base.OnRemoveItem(instanceId, ref matrix, myStringId, userData);
        }

        [Event(null, 0x9d), Reliable, Server, Broadcast]
        private static void PlaySound(Vector3D position, string cueName)
        {
            MySoundPair objA = new MySoundPair(cueName, true);
            if (!ReferenceEquals(objA, MySoundPair.Empty))
            {
                MyEntity3DSoundEmitter emitter = MyAudioComponent.TryGetSoundEmitter();
                if (emitter != null)
                {
                    emitter.SetPosition(new Vector3D?(position));
                    bool? nullable = null;
                    emitter.PlaySound(objA, false, false, false, false, false, nullable);
                }
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            this.UpdateTreeInfos();
        }

        private void UpdateTreeInfos()
        {
            int totalGamePlayTimeInMilliseconds = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            int num2 = 0xea60;
            for (int i = this.m_cutTreeInfos.Count - 1; i >= 0; i--)
            {
                MyCutTreeInfo info = this.m_cutTreeInfos[i];
                if ((totalGamePlayTimeInMilliseconds - info.LastHit) > num2)
                {
                    this.m_cutTreeInfos.RemoveAtFast<MyCutTreeInfo>(i);
                }
            }
        }

        void IMyDecalProxy.AddDecals(ref MyHitInfo hitInfo, MyStringHash source, object customdata, IMyDecalHandler decalHandler, MyStringHash material)
        {
            MyDecalRenderInfo renderInfo = new MyDecalRenderInfo {
                Position = hitInfo.Position,
                Normal = hitInfo.Normal,
                RenderObjectIds = null,
                Flags = MyDecalFlags.World,
                Source = source
            };
            renderInfo.Material = (material.GetHashCode() != 0) ? material : base.Physics.MaterialType;
            decalHandler.AddDecal(ref renderInfo, null);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTrees.<>c <>9 = new MyTrees.<>c();
            public static Func<IMyEventOwner, Action<Vector3D, string>> <>9__8_0;

            internal Action<Vector3D, string> <CutTree>b__8_0(IMyEventOwner s) => 
                new Action<Vector3D, string>(MyTrees.PlaySound);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyCutTreeInfo
        {
            public int ItemInstanceId;
            public int LastHit;
            public float HitPoints;
            public float MaxPoints;
            public float Progress =>
                MathHelper.Clamp((float) ((this.MaxPoints - this.HitPoints) / this.MaxPoints), (float) 0f, (float) 1f);
        }
    }
}

