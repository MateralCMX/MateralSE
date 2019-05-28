namespace Sandbox.Game.Entities
{
    using Havok;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Models;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyEntityType(typeof(MyObjectBuilder_FracturedPiece), true)]
    public class MyFracturedPiece : VRage.Game.Entity.MyEntity, IMyDestroyableObject, IMyEventProxy, IMyEventOwner
    {
        [CompilerGenerated]
        private Action<VRage.Game.Entity.MyEntity> OnRemove;
        public HkdBreakableShape Shape;
        public HitInfo InitialHit;
        private float m_hitPoints;
        public List<MyDefinitionId> OriginalBlocks = new List<MyDefinitionId>();
        private List<HkdShapeInstanceInfo> m_children = new List<HkdShapeInstanceInfo>();
        private List<VRage.Game.MyObjectBuilder_FracturedPiece.Shape> m_shapes = new List<VRage.Game.MyObjectBuilder_FracturedPiece.Shape>();
        private List<HkdShapeInstanceInfo> m_shapeInfos = new List<HkdShapeInstanceInfo>();
        private MyTimeSpan m_markedBreakImpulse = MyTimeSpan.Zero;
        private HkEasePenetrationAction m_easePenetrationAction;
        private MyEntity3DSoundEmitter m_soundEmitter;
        private DateTime m_soundStart;
        private bool m_obstacleContact;
        private bool m_groundContact;
        private VRage.Sync.Sync<bool, SyncDirection.FromServer> m_fallSoundShouldPlay;
        private MySoundPair m_fallSound;
        private VRage.Sync.Sync<string, SyncDirection.FromServer> m_fallSoundString;
        private bool m_contactSet;

        public event Action<VRage.Game.Entity.MyEntity> OnRemove
        {
            [CompilerGenerated] add
            {
                Action<VRage.Game.Entity.MyEntity> onRemove = this.OnRemove;
                while (true)
                {
                    Action<VRage.Game.Entity.MyEntity> a = onRemove;
                    Action<VRage.Game.Entity.MyEntity> action3 = (Action<VRage.Game.Entity.MyEntity>) Delegate.Combine(a, value);
                    onRemove = Interlocked.CompareExchange<Action<VRage.Game.Entity.MyEntity>>(ref this.OnRemove, action3, a);
                    if (ReferenceEquals(onRemove, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<VRage.Game.Entity.MyEntity> onRemove = this.OnRemove;
                while (true)
                {
                    Action<VRage.Game.Entity.MyEntity> source = onRemove;
                    Action<VRage.Game.Entity.MyEntity> action3 = (Action<VRage.Game.Entity.MyEntity>) Delegate.Remove(source, value);
                    onRemove = Interlocked.CompareExchange<Action<VRage.Game.Entity.MyEntity>>(ref this.OnRemove, action3, source);
                    if (ReferenceEquals(onRemove, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyFracturedPiece()
        {
            base.SyncFlag = true;
            base.PositionComp = new MyFracturePiecePositionComponent();
            base.Render = new MyRenderComponentFracturedPiece();
            base.Render.NeedsDraw = true;
            base.Render.PersistentFlags = MyPersistentEntityFlags2.Enabled;
            base.AddDebugRenderComponent(new MyFracturedPieceDebugDraw(this));
            this.UseDamageSystem = false;
            base.NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
            this.m_fallSoundString.SetLocalValue("");
            this.m_fallSoundString.ValueChanged += x => this.SetFallSound();
        }

        private void CreateEasyPenetrationAction(float duration)
        {
            if ((this.Physics != null) && (this.Physics.RigidBody != null))
            {
                this.m_easePenetrationAction = new HkEasePenetrationAction(this.Physics.RigidBody, duration);
                this.m_easePenetrationAction.InitialAllowedPenetrationDepthMultiplier = 5f;
                this.m_easePenetrationAction.InitialAdditionalAllowedPenetrationDepth = 2f;
            }
        }

        public void DebugCheckValidShapes()
        {
            bool flag = false;
            HashSet<Tuple<string, float>> outNamesAndBuildProgress = new HashSet<Tuple<string, float>>();
            HashSet<Tuple<string, float>> set2 = new HashSet<Tuple<string, float>>();
            foreach (MyDefinitionId id in this.OriginalBlocks)
            {
                MyCubeBlockDefinition definition;
                if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(id, out definition))
                {
                    flag = true;
                    MyFracturedBlock.GetAllBlockBreakableShapeNames(definition, outNamesAndBuildProgress);
                }
            }
            MyFracturedBlock.GetAllBlockBreakableShapeNames(this.Shape, set2, 0f);
            foreach (Tuple<string, float> tuple in set2)
            {
                bool flag2 = false;
                foreach (Tuple<string, float> tuple2 in outNamesAndBuildProgress)
                {
                    if (tuple.Item1 == tuple2.Item1)
                    {
                        flag2 = true;
                        break;
                    }
                }
                if (!flag2 & flag)
                {
                    tuple.Item1.ToLower().Contains("compound");
                }
            }
        }

        public bool DoDamage(float damage, MyStringHash damageType, bool sync, MyHitInfo? hitInfo, long attackerId)
        {
            if (Sync.IsServer)
            {
                MyDamageInformation info = new MyDamageInformation(false, damage, damageType, attackerId);
                if (this.UseDamageSystem)
                {
                    MyDamageSystem.Static.RaiseBeforeDamageApplied(this, ref info);
                }
                this.m_hitPoints -= info.Amount;
                if (this.UseDamageSystem)
                {
                    MyDamageSystem.Static.RaiseAfterDamageApplied(this, info);
                }
                if (this.m_hitPoints <= 0f)
                {
                    MyFracturedPiecesManager.Static.RemoveFracturePiece(this, 2f, false, true);
                    if (this.UseDamageSystem)
                    {
                        MyDamageSystem.Static.RaiseDestroyed(this, info);
                    }
                }
            }
            return true;
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            VRage.Game.MyObjectBuilder_FracturedPiece.Shape shape2;
            MyObjectBuilder_FracturedPiece objectBuilder = base.GetObjectBuilder(copy) as MyObjectBuilder_FracturedPiece;
            foreach (MyDefinitionId id in this.OriginalBlocks)
            {
                objectBuilder.BlockDefinitions.Add((SerializableDefinitionId) id);
            }
            if (this.Physics == null)
            {
                foreach (VRage.Game.MyObjectBuilder_FracturedPiece.Shape shape in this.m_shapes)
                {
                    shape2 = new VRage.Game.MyObjectBuilder_FracturedPiece.Shape {
                        Name = shape.Name,
                        Orientation = shape.Orientation
                    };
                    objectBuilder.Shapes.Add(shape2);
                }
                return objectBuilder;
            }
            if (!this.Physics.BreakableBody.BreakableShape.IsCompound() && !string.IsNullOrEmpty(this.Physics.BreakableBody.BreakableShape.Name))
            {
                shape2 = new VRage.Game.MyObjectBuilder_FracturedPiece.Shape {
                    Name = this.Physics.BreakableBody.BreakableShape.Name
                };
                objectBuilder.Shapes.Add(shape2);
            }
            else
            {
                this.Physics.BreakableBody.BreakableShape.GetChildren(this.m_children);
                if (this.m_children.Count == 0)
                {
                    return objectBuilder;
                }
                int count = this.m_children.Count;
                int num2 = 0;
                while (true)
                {
                    if (num2 >= count)
                    {
                        foreach (HkdShapeInstanceInfo info2 in this.m_children)
                        {
                            string shapeName = info2.ShapeName;
                            if (!string.IsNullOrEmpty(shapeName))
                            {
                                shape2 = new VRage.Game.MyObjectBuilder_FracturedPiece.Shape {
                                    Name = shapeName,
                                    Orientation = Quaternion.CreateFromRotationMatrix(info2.GetTransform().GetOrientation()),
                                    Fixed = MyDestructionHelper.IsFixed(info2.Shape)
                                };
                                VRage.Game.MyObjectBuilder_FracturedPiece.Shape item = shape2;
                                objectBuilder.Shapes.Add(item);
                            }
                        }
                        if (this.Physics.IsInWorld)
                        {
                            MyPositionAndOrientation orientation = objectBuilder.PositionAndOrientation.Value;
                            orientation.Position = this.Physics.ClusterToWorld(Vector3.Transform(this.m_children[0].GetTransform().Translation, this.Physics.RigidBody.GetRigidBodyMatrix()));
                            objectBuilder.PositionAndOrientation = new MyPositionAndOrientation?(orientation);
                        }
                        this.m_children.Clear();
                        break;
                    }
                    HkdShapeInstanceInfo info = this.m_children[num2];
                    if (string.IsNullOrEmpty(info.ShapeName))
                    {
                        info.GetChildren(this.m_children);
                    }
                    num2++;
                }
            }
            return objectBuilder;
        }

        public override unsafe void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            MyObjectBuilder_FracturedPiece piece = objectBuilder as MyObjectBuilder_FracturedPiece;
            if (piece.Shapes.Count != 0)
            {
                foreach (VRage.Game.MyObjectBuilder_FracturedPiece.Shape shape in piece.Shapes)
                {
                    this.Render.AddPiece(shape.Name, Matrix.CreateFromQuaternion((Quaternion) shape.Orientation));
                }
                this.OriginalBlocks.Clear();
                foreach (SerializableDefinitionId id in piece.BlockDefinitions)
                {
                    MyPhysicalModelDefinition definition;
                    string modelAsset = null;
                    if (MyDefinitionManager.Static.TryGetDefinition<MyPhysicalModelDefinition>(id, out definition))
                    {
                        modelAsset = definition.Model;
                    }
                    MyCubeBlockDefinition definition2 = null;
                    MyDefinitionManager.Static.TryGetDefinition<MyCubeBlockDefinition>(id, out definition2);
                    if (modelAsset != null)
                    {
                        modelAsset = definition.Model;
                        if (MyModels.GetModelOnlyData(modelAsset).HavokBreakableShapes == null)
                        {
                            MyDestructionData.Static.LoadModelDestruction(modelAsset, definition, Vector3.One, true, false);
                        }
                        HkdBreakableShape shape2 = MyModels.GetModelOnlyData(modelAsset).HavokBreakableShapes[0];
                        Quaternion? rotation = null;
                        Vector3? translation = null;
                        HkdShapeInstanceInfo item = new HkdShapeInstanceInfo(shape2, rotation, translation);
                        this.m_children.Add(item);
                        shape2.GetChildren(this.m_children);
                        if ((definition2 != null) && (definition2.BuildProgressModels != null))
                        {
                            MyCubeBlockDefinition.BuildProgressModel[] buildProgressModels = definition2.BuildProgressModels;
                            for (int j = 0; j < buildProgressModels.Length; j++)
                            {
                                modelAsset = buildProgressModels[j].File;
                                if (MyModels.GetModelOnlyData(modelAsset).HavokBreakableShapes == null)
                                {
                                    MyDestructionData.Static.LoadModelDestruction(modelAsset, definition2, Vector3.One, true, false);
                                }
                                shape2 = MyModels.GetModelOnlyData(modelAsset).HavokBreakableShapes[0];
                                rotation = null;
                                translation = null;
                                item = new HkdShapeInstanceInfo(shape2, rotation, translation);
                                this.m_children.Add(item);
                                shape2.GetChildren(this.m_children);
                            }
                        }
                        this.OriginalBlocks.Add(id);
                    }
                }
                this.m_shapes.AddRange(piece.Shapes);
                Vector3? nullable = null;
                int count = 0;
                for (int i = 0; i < this.m_children.Count; i++)
                {
                    HkdShapeInstanceInfo child = this.m_children[i];
                    Func<VRage.Game.MyObjectBuilder_FracturedPiece.Shape, bool> predicate = s => s.Name == child.ShapeName;
                    IEnumerable<VRage.Game.MyObjectBuilder_FracturedPiece.Shape> source = this.m_shapes.Where<VRage.Game.MyObjectBuilder_FracturedPiece.Shape>(predicate);
                    if (source.Count<VRage.Game.MyObjectBuilder_FracturedPiece.Shape>() <= 0)
                    {
                        child.GetChildren(this.m_children);
                    }
                    else
                    {
                        VRage.Game.MyObjectBuilder_FracturedPiece.Shape item = source.First<VRage.Game.MyObjectBuilder_FracturedPiece.Shape>();
                        Matrix transform = Matrix.CreateFromQuaternion((Quaternion) item.Orientation);
                        if ((nullable == null) && (item.Name == this.m_shapes[0].Name))
                        {
                            nullable = new Vector3?(child.GetTransform().Translation);
                            count = this.m_shapeInfos.Count;
                        }
                        transform.Translation = child.GetTransform().Translation;
                        HkdShapeInstanceInfo info2 = new HkdShapeInstanceInfo(child.Shape.Clone(), transform);
                        if (item.Fixed)
                        {
                            info2.Shape.SetFlagRecursively(HkdBreakableShape.Flags.IS_FIXED);
                        }
                        this.m_shapeInfos.Add(info2);
                        this.m_shapes.Remove(item);
                    }
                }
                if (this.m_shapeInfos.Count == 0)
                {
                    List<string> source = new List<string>();
                    foreach (VRage.Game.MyObjectBuilder_FracturedPiece.Shape shape5 in piece.Shapes)
                    {
                        source.Add(shape5.Name);
                    }
                    string str2 = source.Aggregate<string>((str1, str2) => str1 + ", " + str2);
                    this.OriginalBlocks.Aggregate<MyDefinitionId, string>("", (str, defId) => str + ", " + defId.ToString());
                    throw new Exception("No relevant shape was found for fractured piece. It was probably reexported and names changed. Shapes: " + str2 + ". Original blocks: " + str2);
                }
                if (nullable != null)
                {
                    int num4 = 0;
                    while (true)
                    {
                        if (num4 >= this.m_shapeInfos.Count)
                        {
                            Matrix m = this.m_shapeInfos[count].GetTransform();
                            m.Translation = Vector3.Zero;
                            this.m_shapeInfos[count].SetTransform(ref m);
                            break;
                        }
                        Matrix transform = this.m_shapeInfos[num4].GetTransform();
                        Matrix* matrixPtr1 = (Matrix*) ref transform;
                        matrixPtr1.Translation -= nullable.Value;
                        this.m_shapeInfos[num4].SetTransform(ref transform);
                        num4++;
                    }
                }
                if (this.m_shapeInfos.Count > 0)
                {
                    MyPhysicalModelDefinition definition3;
                    if (this.m_shapeInfos.Count == 1)
                    {
                        this.Shape = this.m_shapeInfos[0].Shape;
                    }
                    else
                    {
                        HkdBreakableShape? oldParent = null;
                        this.Shape = (HkdBreakableShape) new HkdCompoundBreakableShape(oldParent, this.m_shapeInfos);
                        this.Shape.RecalcMassPropsFromChildren();
                    }
                    this.Shape.SetStrenght(MyDestructionConstants.STRENGTH);
                    HkMassProperties massProperties = new HkMassProperties();
                    this.Shape.BuildMassProperties(ref massProperties);
                    this.Shape.SetChildrenParent(this.Shape);
                    this.Physics = new MyPhysicsBody(this, RigidBodyFlag.RBF_DEBRIS);
                    this.Physics.CanUpdateAccelerations = true;
                    this.Physics.InitialSolverDeactivation = HkSolverDeactivation.High;
                    this.Physics.CreateFromCollisionObject(this.Shape.GetShape(), Vector3.Zero, base.PositionComp.WorldMatrix, new HkMassProperties?(massProperties), 15);
                    this.Physics.BreakableBody = new HkdBreakableBody(this.Shape, this.Physics.RigidBody, null, (Matrix) base.PositionComp.WorldMatrix);
                    MyPhysicsBody physics = this.Physics;
                    this.Physics.BreakableBody.AfterReplaceBody += new BreakableBodyReplaced(physics.FracturedBody_AfterReplaceBody);
                    if ((this.OriginalBlocks.Count > 0) && MyDefinitionManager.Static.TryGetDefinition<MyPhysicalModelDefinition>(this.OriginalBlocks[0], out definition3))
                    {
                        this.Physics.MaterialType = definition3.PhysicalMaterial.Id.SubtypeId;
                    }
                    HkRigidBody rigidBody = this.Physics.RigidBody;
                    if (MyDestructionHelper.IsFixed(this.Physics.BreakableBody.BreakableShape))
                    {
                        rigidBody.UpdateMotionType(HkMotionType.Fixed);
                        rigidBody.LinearVelocity = Vector3.Zero;
                        rigidBody.AngularVelocity = Vector3.Zero;
                    }
                    this.Physics.Enabled = true;
                }
                this.m_children.Clear();
                this.m_shapeInfos.Clear();
            }
        }

        internal void InitFromBreakableBody(HkdBreakableBody b, MatrixD worldMatrix, MyCubeBlock block)
        {
            MyPhysicalModelDefinition definition;
            int num1;
            this.OriginalBlocks.Clear();
            if (block != null)
            {
                if (block is MyCompoundCubeBlock)
                {
                    foreach (MySlimBlock block2 in (block as MyCompoundCubeBlock).GetBlocks())
                    {
                        this.OriginalBlocks.Add(block2.BlockDefinition.Id);
                    }
                }
                else if (block is MyFracturedBlock)
                {
                    this.OriginalBlocks.AddRange((block as MyFracturedBlock).OriginalBlocks);
                }
                else
                {
                    this.OriginalBlocks.Add(block.BlockDefinition.Id);
                }
            }
            HkRigidBody rigidBody = b.GetRigidBody();
            bool flag = MyDestructionHelper.IsFixed(b.BreakableShape);
            if (flag)
            {
                rigidBody.UpdateMotionType(HkMotionType.Fixed);
                rigidBody.LinearVelocity = Vector3.Zero;
                rigidBody.AngularVelocity = Vector3.Zero;
            }
            if (base.SyncFlag)
            {
                base.CreateSync();
            }
            base.PositionComp.WorldMatrix = worldMatrix;
            if (flag || !Sync.IsServer)
            {
                num1 = 4;
            }
            else
            {
                num1 = 0x200;
            }
            this.Physics.Flags = (RigidBodyFlag) num1;
            this.Physics.BreakableBody = b;
            rigidBody.UserObject = this.Physics;
            if (flag)
            {
                rigidBody.Layer = 13;
            }
            else
            {
                rigidBody.Motion.SetDeactivationClass(HkSolverDeactivation.High);
                rigidBody.EnableDeactivation = true;
                if (!MyFakes.REDUCE_FRACTURES_COUNT)
                {
                    rigidBody.Layer = 15;
                }
                else if ((b.BreakableShape.Volume >= 1f) || (MyRandom.Instance.Next(6) <= 1))
                {
                    rigidBody.Layer = 15;
                }
                else
                {
                    rigidBody.Layer = 14;
                }
            }
            MyPhysicsBody physics = this.Physics;
            this.Physics.BreakableBody.AfterReplaceBody += new BreakableBodyReplaced(physics.FracturedBody_AfterReplaceBody);
            if ((this.OriginalBlocks.Count > 0) && MyDefinitionManager.Static.TryGetDefinition<MyPhysicalModelDefinition>(this.OriginalBlocks[0], out definition))
            {
                this.Physics.MaterialType = definition.PhysicalMaterial.Id.SubtypeId;
            }
            this.Physics.Enabled = true;
            MyDestructionHelper.FixPosition(this);
            this.SetDataFromHavok(b.BreakableShape);
            Vector3 centerOfMassLocal = b.GetRigidBody().CenterOfMassLocal;
            Vector3 centerOfMassWorld = b.GetRigidBody().CenterOfMassWorld;
            Vector3 coM = b.BreakableShape.CoM;
            b.GetRigidBody().CenterOfMassLocal = coM;
            b.BreakableShape.RemoveReference();
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            MyCubeBlockDefinition blockDefinition = null;
            if (((this.Physics.HavokWorld != null) && ((this.OriginalBlocks.Count != 0) && MyDefinitionManager.Static.TryGetCubeBlockDefinition(this.OriginalBlocks[0], out blockDefinition))) && (blockDefinition.CubeSize == MyCubeSize.Large))
            {
                float maxImpulse = this.Physics.Mass * 0.4f;
                this.Physics.HavokWorld.BreakOffPartsUtil.MarkEntityBreakable(this.Physics.RigidBody, maxImpulse);
                this.m_markedBreakImpulse = MySandboxGame.Static.TotalTime;
            }
        }

        public void OnDestroy()
        {
            if (Sync.IsServer)
            {
                MyFracturedPiecesManager.Static.RemoveFracturePiece(this, 2f, false, true);
            }
        }

        public override void OnRemovedFromScene(object source)
        {
            base.OnRemovedFromScene(source);
            this.UnmarkEntityBreakable(false);
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.StopSound(true, true);
            }
            if (this.OnRemove != null)
            {
                this.OnRemove(this);
            }
        }

        public void RegisterObstacleContact(ref HkContactPointEvent e)
        {
            if ((!this.m_obstacleContact && this.m_fallSoundShouldPlay.Value) && ((DateTime.UtcNow - this.m_soundStart).TotalSeconds >= 1.0))
            {
                this.m_obstacleContact = true;
            }
        }

        public void SetDataFromHavok(HkdBreakableShape shape)
        {
            this.Shape = shape;
            this.Shape.AddReference();
            if (this.Render != null)
            {
                if (!shape.IsCompound() && !string.IsNullOrEmpty(shape.Name))
                {
                    this.Render.AddPiece(shape.Name, Matrix.Identity);
                }
                else
                {
                    shape.GetChildren(this.m_shapeInfos);
                    foreach (HkdShapeInstanceInfo info in this.m_shapeInfos)
                    {
                        if (info.IsValid())
                        {
                            this.Render.AddPiece(info.ShapeName, info.GetTransform());
                        }
                    }
                    this.m_shapeInfos.Clear();
                }
            }
            this.m_hitPoints = this.Shape.Volume * 100f;
        }

        private void SetFallSound()
        {
            if ((this.OriginalBlocks != null) && this.OriginalBlocks[0].TypeId.ToString().Equals("MyObjectBuilder_Tree"))
            {
                this.m_fallSound = new MySoundPair(this.m_fallSoundString.Value, true);
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        public void StartFallSound(string sound)
        {
            this.m_groundContact = false;
            this.m_obstacleContact = false;
            this.m_fallSoundString.Value = sound;
            this.m_soundStart = DateTime.UtcNow;
            this.m_fallSoundShouldPlay.Value = true;
            if (!this.m_contactSet && ((Sandbox.Engine.Platform.Game.IsDedicated || (MyMultiplayer.Static == null)) || MyMultiplayer.Static.IsServer))
            {
                this.Physics.RigidBody.ContactSoundCallback += new ContactPointEventHandler(this.RegisterObstacleContact);
            }
            this.m_contactSet = true;
        }

        private void UnmarkEntityBreakable(bool checkTime)
        {
            if ((this.m_markedBreakImpulse != MyTimeSpan.Zero) && (!checkTime || ((MySandboxGame.Static.TotalTime - this.m_markedBreakImpulse) > MyTimeSpan.FromSeconds(1.5))))
            {
                this.m_markedBreakImpulse = MyTimeSpan.Zero;
                if ((this.Physics != null) && (this.Physics.HavokWorld != null))
                {
                    this.Physics.HavokWorld.BreakOffPartsUtil.UnmarkEntityBreakable(this.Physics.RigidBody);
                    if (checkTime)
                    {
                        this.CreateEasyPenetrationAction(1f);
                    }
                }
            }
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            if (this.m_markedBreakImpulse != MyTimeSpan.Zero)
            {
                this.UnmarkEntityBreakable(true);
            }
            if ((!this.m_fallSoundShouldPlay.Value && (this.Physics.LinearVelocity.LengthSquared() > 25f)) && ((DateTime.UtcNow - this.m_soundStart).TotalSeconds >= 1.0))
            {
                this.m_fallSoundShouldPlay.Value = true;
                this.m_obstacleContact = false;
                this.m_groundContact = false;
            }
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                if (!this.m_fallSoundShouldPlay.Value)
                {
                    if ((this.m_soundEmitter != null) && this.m_soundEmitter.IsPlaying)
                    {
                        this.m_soundEmitter.StopSound(false, true);
                    }
                }
                else
                {
                    if (this.m_soundEmitter == null)
                    {
                        this.m_soundEmitter = new MyEntity3DSoundEmitter(this, false, 1f);
                    }
                    if ((!this.m_soundEmitter.IsPlaying && (this.m_fallSound != null)) && !ReferenceEquals(this.m_fallSound, MySoundPair.Empty))
                    {
                        bool? nullable = null;
                        this.m_soundEmitter.PlaySound(this.m_fallSound, true, true, false, false, false, nullable);
                    }
                }
            }
            if (this.m_obstacleContact && !this.m_groundContact)
            {
                if (this.Physics.LinearVelocity.Y > 0f)
                {
                    goto TR_0000;
                }
                else if (this.Physics.LinearVelocity.LengthSquared() >= 9f)
                {
                    this.m_obstacleContact = false;
                }
                else
                {
                    goto TR_0000;
                }
            }
            return;
        TR_0000:
            this.m_groundContact = true;
            this.m_fallSoundShouldPlay.Value = false;
            this.m_soundStart = DateTime.UtcNow;
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.Update();
                if (this.m_soundEmitter.IsPlaying && ((DateTime.UtcNow - this.m_soundStart).TotalSeconds >= 15.0))
                {
                    this.m_fallSoundShouldPlay.Value = false;
                }
            }
            Vector3 vector = MyGravityProviderSystem.CalculateTotalGravityInPoint(base.PositionComp.GetPosition());
            this.Physics.RigidBody.Gravity = vector;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            this.Physics.Enabled = true;
            this.Physics.RigidBody.Activate();
            this.Physics.RigidBody.ContactPointCallbackDelay = 0;
            this.Physics.RigidBody.ContactSoundCallbackEnabled = true;
            if (this.InitialHit != null)
            {
                this.Physics.ApplyImpulse(this.InitialHit.Impulse, this.Physics.CenterOfMassWorld);
                MyPhysics.FractureImpactDetails details = new MyPhysics.FractureImpactDetails {
                    Entity = this,
                    World = this.Physics.HavokWorld,
                    ContactInWorld = this.InitialHit.Position
                };
                HkdFractureImpactDetails details2 = HkdFractureImpactDetails.Create();
                details2.SetBreakingBody(this.Physics.RigidBody);
                details2.SetContactPoint((Vector3) this.Physics.WorldToCluster(this.InitialHit.Position));
                details2.SetDestructionRadius(0.05f);
                details2.SetBreakingImpulse(30000f);
                details2.SetParticleVelocity(this.InitialHit.Impulse);
                details2.SetParticlePosition((Vector3) this.Physics.WorldToCluster(this.InitialHit.Position));
                details2.SetParticleMass(500f);
                details.Details = details2;
                MyPhysics.EnqueueDestruction(details);
            }
            Vector3 vector = MyGravityProviderSystem.CalculateTotalGravityInPoint(base.PositionComp.GetPosition());
            this.Physics.RigidBody.Gravity = vector;
        }

        public MyRenderComponentFracturedPiece Render =>
            ((MyRenderComponentFracturedPiece) base.Render);

        public MyPhysicsBody Physics
        {
            get => 
                (base.Physics as MyPhysicsBody);
            set => 
                (base.Physics = value);
        }

        public float Integrity =>
            this.m_hitPoints;

        public bool UseDamageSystem { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyFracturedPiece.<>c <>9 = new MyFracturedPiece.<>c();
            public static Func<string, string, string> <>9__28_1;
            public static Func<string, MyDefinitionId, string> <>9__28_2;

            internal string <Init>b__28_1(string str1, string str2) => 
                (str1 + ", " + str2);

            internal string <Init>b__28_2(string str, MyDefinitionId defId) => 
                (str + ", " + defId.ToString());
        }

        public class HitInfo
        {
            public Vector3D Position;
            public Vector3 Impulse;
        }

        private class MyFracturedPieceDebugDraw : MyDebugRenderComponentBase
        {
            private MyFracturedPiece m_piece;

            public MyFracturedPieceDebugDraw(MyFracturedPiece piece)
            {
                this.m_piece = piece;
            }

            public override void DebugDraw()
            {
                if (MyDebugDrawSettings.DEBUG_DRAW_FRACTURED_PIECES)
                {
                    MyRenderProxy.DebugDrawAxis(this.m_piece.WorldMatrix, 1f, false, false, false);
                    if ((this.m_piece.Physics != null) && (this.m_piece.Physics.RigidBody != null))
                    {
                        MyPhysicsBody physics = this.m_piece.Physics;
                        HkRigidBody rigidBody = physics.RigidBody;
                        Vector3 worldCoord = (Vector3) physics.ClusterToWorld(rigidBody.CenterOfMassWorld);
                        BoundingBoxD xd1 = new BoundingBoxD(worldCoord - (Vector3D.One * 0.10000000149011612), worldCoord + (Vector3D.One * 0.10000000149011612));
                        string text = $"{rigidBody.GetMotionType()}
, {physics.Friction}
{physics.Entity.EntityId.ToString().Substring(0, 5)}";
                        MyRenderProxy.DebugDrawText3D(worldCoord, text, Color.White, 0.6f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    }
                }
            }

            public override void DebugDrawInvalidTriangles()
            {
            }
        }
    }
}

