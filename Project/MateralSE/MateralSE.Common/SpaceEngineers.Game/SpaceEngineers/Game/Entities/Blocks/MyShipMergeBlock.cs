namespace SpaceEngineers.Game.Entities.Blocks
{
    using Havok;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.EntityComponents.DebugRenders;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyCubeBlockType(typeof(MyObjectBuilder_MergeBlock)), MyTerminalInterface(new System.Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyShipMergeBlock), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyShipMergeBlock) })]
    public class MyShipMergeBlock : MyFunctionalBlock, SpaceEngineers.Game.ModAPI.IMyShipMergeBlock, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, SpaceEngineers.Game.ModAPI.Ingame.IMyShipMergeBlock
    {
        private HkConstraint m_constraint;
        private MyShipMergeBlock m_other;
        private MyConcurrentHashSet<MyCubeGrid> m_gridList = new MyConcurrentHashSet<MyCubeGrid>();
        private ushort m_frameCounter;
        private UpdateBeforeFlags m_updateBeforeFlags;
        private Base6Directions.Direction m_forward;
        private Base6Directions.Direction m_right;
        private Base6Directions.Direction m_otherRight;
        private VRage.Sync.Sync<MergeState, SyncDirection.FromServer> m_mergeState;
        private bool HasConstraint;
        [CompilerGenerated]
        private Action BeforeMerge;

        private event Action BeforeMerge
        {
            [CompilerGenerated] add
            {
                Action beforeMerge = this.BeforeMerge;
                while (true)
                {
                    Action a = beforeMerge;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    beforeMerge = Interlocked.CompareExchange<Action>(ref this.BeforeMerge, action3, a);
                    if (ReferenceEquals(beforeMerge, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action beforeMerge = this.BeforeMerge;
                while (true)
                {
                    Action source = beforeMerge;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    beforeMerge = Interlocked.CompareExchange<Action>(ref this.BeforeMerge, action3, source);
                    if (ReferenceEquals(beforeMerge, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action SpaceEngineers.Game.ModAPI.IMyShipMergeBlock.BeforeMerge
        {
            add
            {
                this.BeforeMerge += value;
            }
            remove
            {
                this.BeforeMerge += value;
            }
        }

        public MyShipMergeBlock()
        {
            if (!Sync.IsServer)
            {
                this.m_mergeState.ValueChanged += x => this.UpdateEmissivity();
            }
        }

        private void AddConstraint(HkConstraint newConstraint)
        {
            this.HasConstraint = true;
            base.CubeGrid.Physics.AddConstraint(newConstraint);
        }

        private void CalculateMergeArea(out Vector3I minI, out Vector3I maxI)
        {
            Vector3I intVector = Base6Directions.GetIntVector(base.Orientation.TransformDirection(this.m_forward));
            minI = (Vector3I) (base.Min + intVector);
            maxI = (Vector3I) (base.Max + intVector);
            if (((intVector.X + intVector.Y) + intVector.Z) == -1)
            {
                maxI = (Vector3I) (maxI + ((maxI - minI) * intVector));
            }
            else
            {
                minI = (Vector3I) (minI + ((maxI - minI) * intVector));
            }
        }

        private void CalculateMergeData(ref MergeData data)
        {
            MyMergeBlockDefinition blockDefinition = base.BlockDefinition as MyMergeBlockDefinition;
            float num = (blockDefinition != null) ? blockDefinition.Strength : 0.1f;
            data.Distance = ((float) (base.WorldMatrix.Translation - this.m_other.WorldMatrix.Translation).Length()) - base.CubeGrid.GridSize;
            data.StrengthFactor = (float) Math.Exp((double) (-data.Distance / base.CubeGrid.GridSize));
            float num2 = MathHelper.Lerp(0f, num * ((base.CubeGrid.GridSizeEnum == MyCubeSize.Large) ? 0.005f : 0.1f), data.StrengthFactor);
            Vector3 velocityAtPoint = this.m_other.CubeGrid.Physics.GetVelocityAtPoint(this.m_other.PositionComp.GetPosition());
            data.RelativeVelocity = velocityAtPoint - base.CubeGrid.Physics.GetVelocityAtPoint(base.PositionComp.GetPosition());
            float num4 = data.RelativeVelocity.Length();
            data.ConstraintStrength = num2 / Math.Max((float) (3.6f / ((num4 > 0.1f) ? num4 : 0.1f)), (float) 1f);
            Vector3 vector3 = (Vector3) (this.m_other.PositionComp.GetPosition() - base.PositionComp.GetPosition());
            Vector3 directionVector = (Vector3) base.WorldMatrix.GetDirectionVector(this.m_forward);
            data.Distance = vector3.Length();
            data.PositionOk = data.Distance < (base.CubeGrid.GridSize + 0.17f);
            data.AxisDelta = (float) (directionVector + this.m_other.WorldMatrix.GetDirectionVector(this.m_forward)).Length();
            data.AxisOk = data.AxisDelta < 0.1f;
            data.RotationDelta = (float) (base.WorldMatrix.GetDirectionVector(this.m_right) - this.m_other.WorldMatrix.GetDirectionVector(this.m_other.m_otherRight)).Length();
            data.RotationOk = data.RotationDelta < 0.08f;
        }

        private Vector3I CalculateOtherGridOffset()
        {
            Vector3 vector2;
            Vector3 position = -this.m_other.ConstraintPositionInGridSpace() / this.m_other.CubeGrid.GridSize;
            Base6Directions.Direction newA = base.Orientation.TransformDirection(this.m_right);
            Base6Directions.Direction flippedDirection = Base6Directions.GetFlippedDirection(this.m_other.Orientation.TransformDirection(this.m_other.m_forward));
            MatrixI matrix = MatrixI.CreateRotation(this.m_other.CubeGrid.WorldMatrix.GetClosestDirection(base.CubeGrid.WorldMatrix.GetDirectionVector(newA)), flippedDirection, newA, base.Orientation.TransformDirection(this.m_forward));
            Vector3.Transform(ref position, ref matrix, out vector2);
            return Vector3I.Round((this.ConstraintPositionInGridSpace() / base.CubeGrid.GridSize) + vector2);
        }

        public override void CheckEmissiveState(bool force)
        {
            if (force && Sync.IsServer)
            {
                this.m_mergeState.Value = MergeState.UNSET;
            }
            this.UpdateState();
            if (this.m_mergeState != null)
            {
                this.UpdateEmissivity();
            }
        }

        protected override bool CheckIsWorking()
        {
            MyShipMergeBlock otherMergeBlock = this.GetOtherMergeBlock();
            return (((otherMergeBlock == null) || otherMergeBlock.FriendlyWithBlock(this)) && base.CheckIsWorking());
        }

        private bool CheckUnobstructed() => 
            ReferenceEquals(this.GetBlockInMergeArea(), null);

        protected override void Closing()
        {
            base.Closing();
            if (this.InConstraint)
            {
                this.RemoveConstraintInBoth();
            }
        }

        public override bool ConnectionAllowed(ref Vector3I otherBlockPos, ref Vector3I faceNormal, MyCubeBlockDefinition def) => 
            this.ConnectionAllowedInternal(ref faceNormal, def);

        public override bool ConnectionAllowed(ref Vector3I otherBlockMinPos, ref Vector3I otherBlockMaxPos, ref Vector3I faceNormal, MyCubeBlockDefinition def) => 
            this.ConnectionAllowedInternal(ref faceNormal, def);

        private bool ConnectionAllowedInternal(ref Vector3I faceNormal, MyCubeBlockDefinition def) => 
            (base.IsWorking || (!ReferenceEquals(def, base.BlockDefinition) || (base.Orientation.TransformDirectionInverse(Base6Directions.GetDirection((Vector3I) faceNormal)) != this.m_forward)));

        private Vector3 ConstraintPositionInGridSpace() => 
            ((base.Position * base.CubeGrid.GridSize) + (base.PositionComp.LocalMatrix.GetDirectionVector(this.m_forward) * (base.CubeGrid.GridSize * 0.5f)));

        private void CreateConstraint(MyCubeGrid other, MyShipMergeBlock block)
        {
            HkPrismaticConstraintData data = new HkPrismaticConstraintData();
            Base6Directions.Direction closestDirection = block.WorldMatrix.GetClosestDirection(base.WorldMatrix.GetDirectionVector(this.m_right));
            data.SetInBodySpace(this.ConstraintPositionInGridSpace(), block.ConstraintPositionInGridSpace(), base.PositionComp.LocalMatrix.GetDirectionVector(this.m_forward), -block.PositionComp.LocalMatrix.GetDirectionVector(this.m_forward), base.PositionComp.LocalMatrix.GetDirectionVector(this.m_right), block.PositionComp.LocalMatrix.GetDirectionVector(closestDirection), base.CubeGrid.Physics, other.Physics);
            HkMalleableConstraintData constraintData = new HkMalleableConstraintData();
            constraintData.SetData(data);
            data.ClearHandle();
            data = null;
            constraintData.Strength = 1E-05f;
            HkConstraint newConstraint = new HkConstraint(base.CubeGrid.Physics.RigidBody, other.Physics.RigidBody, constraintData);
            this.AddConstraint(newConstraint);
            this.SetConstraint(block, newConstraint, base.WorldMatrix.GetClosestDirection(block.WorldMatrix.GetDirectionVector(block.m_right)));
            this.m_other.SetConstraint(this, newConstraint, closestDirection);
        }

        private HkBvShape CreateFieldShape(Vector3 extents)
        {
            HkPhantomCallbackShape shape = new HkPhantomCallbackShape(new HkPhantomHandler(this.phantom_Enter), new HkPhantomHandler(this.phantom_Leave));
            return new HkBvShape((HkShape) new HkBoxShape(extents), (HkShape) shape, HkReferencePolicy.TakeOwnership);
        }

        private void DebugDrawInfo(Vector2 offset)
        {
            MergeData data = new MergeData();
            this.CalculateMergeData(ref data);
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, 75f) + offset, "x = " + data.StrengthFactor.ToString(), Color.Green, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, 0f) + offset, "Merge block strength: " + data.ConstraintStrength.ToString(), Color.Green, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, 15f) + offset, "Merge block dist: " + (data.Distance - base.CubeGrid.GridSize).ToString(), data.PositionOk ? Color.Green : Color.Red, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, 30f) + offset, "Frame counter: " + this.m_frameCounter.ToString(), (this.m_frameCounter >= 6) ? Color.Green : Color.Red, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, 45f) + offset, "Rotation difference: " + data.RotationDelta.ToString(), data.RotationOk ? Color.Green : Color.Red, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, 60f) + offset, "Axis difference: " + data.AxisDelta.ToString(), data.AxisOk ? Color.Green : Color.Red, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            float num = data.RelativeVelocity.Length();
            MyRenderProxy.DebugDrawText2D(new Vector2(0f, 90f) + offset, (num > 0.5f) ? "Quick" : "Slow", (num > 0.5f) ? Color.Red : Color.Green, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
        }

        private MySlimBlock GetBlockInMergeArea()
        {
            Vector3I vectori;
            Vector3I vectori2;
            this.CalculateMergeArea(out vectori, out vectori2);
            Vector3I pos = vectori;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref vectori, ref vectori2);
            while (iterator.IsValid())
            {
                MySlimBlock cubeBlock = base.CubeGrid.GetCubeBlock(pos);
                if (cubeBlock != null)
                {
                    return cubeBlock;
                }
                iterator.GetNext(out pos);
            }
            return null;
        }

        public override int GetBlockSpecificState() => 
            ((this.m_mergeState == 4) ? 2 : ((this.m_mergeState == 3) ? 1 : 0));

        private Vector3 GetMergeNormalWorld() => 
            ((Vector3) base.WorldMatrix.GetDirectionVector(this.m_forward));

        private MyShipMergeBlock GetOtherMergeBlock()
        {
            Vector3I vectori;
            Vector3I vectori2;
            this.CalculateMergeArea(out vectori, out vectori2);
            Vector3I pos = vectori;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref vectori, ref vectori2);
            while (iterator.IsValid())
            {
                MySlimBlock cubeBlock = base.CubeGrid.GetCubeBlock(pos);
                if ((cubeBlock != null) && (cubeBlock.FatBlock != null))
                {
                    MyShipMergeBlock fatBlock = cubeBlock.FatBlock as MyShipMergeBlock;
                    if (fatBlock != null)
                    {
                        Vector3I vectori4;
                        Vector3I vectori5;
                        fatBlock.CalculateMergeArea(out vectori4, out vectori5);
                        Vector3I intVector = Base6Directions.GetIntVector(base.Orientation.TransformDirection(this.m_forward));
                        vectori4 = vectori2 - (vectori4 + intVector);
                        vectori5 = ((Vector3I) (vectori5 + intVector)) - vectori;
                        if (((vectori4.X >= 0) && ((vectori4.Y >= 0) && ((vectori4.Z >= 0) && ((vectori5.X >= 0) && (vectori5.Y >= 0))))) && (vectori5.Z >= 0))
                        {
                            return fatBlock;
                        }
                    }
                }
                iterator.GetNext(out pos);
            }
            return null;
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            this.LoadDummies();
            base.SlimBlock.DeformationRatio = (base.BlockDefinition as MyMergeBlockDefinition).DeformationRatio;
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            base.NeedsWorldMatrix = true;
            base.AddDebugRenderComponent(new MyDebugRenderComponentShipMergeBlock(this));
        }

        private void LoadDummies()
        {
            foreach (KeyValuePair<string, MyModelDummy> pair in MyModels.GetModelOnlyDummies(base.BlockDefinition.Model).Dummies)
            {
                if (pair.Key.ToLower().Contains("merge"))
                {
                    Matrix matrix = pair.Value.Matrix;
                    Vector3 extents = matrix.Scale / 2f;
                    Vector3 vec = Vector3.DominantAxisProjection(matrix.Translation / extents);
                    vec.Normalize();
                    this.m_forward = Base6Directions.GetDirection(vec);
                    this.m_right = Base6Directions.GetPerpendicular(this.m_forward);
                    MatrixD worldTransform = MatrixD.Normalize(matrix) * base.WorldMatrix;
                    HkBvShape shape = this.CreateFieldShape(extents);
                    base.Physics = new MyPhysicsBody(this, RigidBodyFlag.RBF_UNLOCKED_SPEEDS | RigidBodyFlag.RBF_STATIC);
                    base.Physics.IsPhantom = true;
                    HkMassProperties? massProperties = null;
                    base.Physics.CreateFromCollisionObject((HkShape) shape, matrix.Translation, worldTransform, massProperties, 0x18);
                    base.Physics.Enabled = base.IsWorking;
                    base.Physics.RigidBody.ContactPointCallbackEnabled = true;
                    shape.Base.RemoveReference();
                    break;
                }
            }
        }

        private void MyShipMergeBlock_IsWorkingChanged(MyCubeBlock obj)
        {
            if (base.Physics != null)
            {
                base.Physics.Enabled = base.IsWorking;
            }
            if ((!base.IsWorking && (this.GetOtherMergeBlock() == null)) && this.InConstraint)
            {
                this.RemoveConstraintInBoth();
            }
            base.CheckConnectionAllowed = !base.IsWorking;
            base.CubeGrid.UpdateBlockNeighbours(base.SlimBlock);
            this.UpdateState();
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.MyShipMergeBlock_IsWorkingChanged);
            base.CheckConnectionAllowed = !base.IsWorking;
            base.Physics.Enabled = base.IsWorking;
            this.UpdateState();
            MyShipMergeBlock otherMergeBlock = this.GetOtherMergeBlock();
            if (otherMergeBlock != null)
            {
                otherMergeBlock.UpdateIsWorkingBeforeNextFrame();
            }
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            this.UpdateState();
        }

        protected override void OnOwnershipChanged()
        {
            base.OnOwnershipChanged();
            this.UpdateIsWorkingBeforeNextFrame();
        }

        public override void OnRemovedFromScene(object source)
        {
            base.OnRemovedFromScene(source);
            MyShipMergeBlock otherMergeBlock = this.GetOtherMergeBlock();
            if (otherMergeBlock != null)
            {
                otherMergeBlock.UpdateIsWorkingBeforeNextFrame();
            }
            this.RemoveConstraintInBoth();
            if (Sync.IsServer)
            {
                this.m_mergeState.Value = MergeState.UNSET;
            }
        }

        protected override void OnStartWorking()
        {
            this.UpdateState();
            base.OnStartWorking();
        }

        protected override void OnStopWorking()
        {
            this.UpdateState();
            base.OnStopWorking();
        }

        private void phantom_Enter(HkPhantomCallbackShape shape, HkRigidBody body)
        {
            List<VRage.ModAPI.IMyEntity> allEntities = body.GetAllEntities();
            foreach (MyCubeGrid grid in allEntities)
            {
                if (grid == null)
                {
                    continue;
                }
                if ((grid.GridSizeEnum == base.CubeGrid.GridSizeEnum) && (!ReferenceEquals(grid, base.CubeGrid) && (grid.Physics.RigidBody == body)))
                {
                    this.m_gridList.Add(grid);
                }
            }
            allEntities.Clear();
        }

        private void phantom_Leave(HkPhantomCallbackShape shape, HkRigidBody body)
        {
            List<VRage.ModAPI.IMyEntity> allEntities = body.GetAllEntities();
            foreach (VRage.ModAPI.IMyEntity entity in allEntities)
            {
                this.m_gridList.Remove(entity as MyCubeGrid);
            }
            allEntities.Clear();
        }

        protected void RemoveConstraint()
        {
            this.m_constraint = null;
            this.m_other = null;
            this.UpdateState();
            if (!base.HasDamageEffect)
            {
                base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        protected void RemoveConstraintInBoth()
        {
            if (!this.HasConstraint)
            {
                if (this.m_other != null)
                {
                    this.m_other.RemoveConstraintInBoth();
                }
            }
            else
            {
                this.m_other.RemoveConstraint();
                base.CubeGrid.Physics.RemoveConstraint(this.m_constraint);
                this.m_constraint.Dispose();
                this.RemoveConstraint();
                this.HasConstraint = false;
            }
        }

        protected void SetConstraint(MyShipMergeBlock otherBlock, HkConstraint constraint, Base6Directions.Direction otherRight)
        {
            if ((this.m_constraint == null) && (this.m_other == null))
            {
                this.m_constraint = constraint;
                this.m_other = otherBlock;
                this.m_otherRight = otherRight;
                this.UpdateState();
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (this.SafeConstraint != null)
            {
                if ((MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_CONNECTORS_AND_MERGE_BLOCKS) && (base.CustomName.ToString() == "DEBUG"))
                {
                    this.DebugDrawInfo(new Vector2(0f, 0f));
                    this.m_other.DebugDrawInfo(new Vector2(0f, 120f));
                    MyRenderProxy.DebugDrawLine3D(base.PositionComp.GetPosition(), base.PositionComp.GetPosition() + (base.WorldMatrix.GetDirectionVector(this.m_right) * 10.0), Color.Red, Color.Red, false, false);
                    MyRenderProxy.DebugDrawLine3D(this.m_other.PositionComp.GetPosition(), this.m_other.PositionComp.GetPosition() + (this.m_other.WorldMatrix.GetDirectionVector(this.m_other.m_otherRight) * 10.0), Color.Red, Color.Red, false, false);
                    MyRenderProxy.DebugDrawLine3D(base.PositionComp.GetPosition(), base.PositionComp.GetPosition() + (base.WorldMatrix.GetDirectionVector(this.m_otherRight) * 5.0), Color.Yellow, Color.Yellow, false, false);
                    MyRenderProxy.DebugDrawLine3D(this.m_other.PositionComp.GetPosition(), this.m_other.PositionComp.GetPosition() + (this.m_other.WorldMatrix.GetDirectionVector(this.m_other.m_right) * 5.0), Color.Yellow, Color.Yellow, false, false);
                }
                Vector3 velocityAtPoint = base.CubeGrid.Physics.GetVelocityAtPoint(base.PositionComp.GetPosition());
                Vector3 vector2 = this.m_other.CubeGrid.Physics.GetVelocityAtPoint(this.m_other.PositionComp.GetPosition()) - velocityAtPoint;
                if (vector2.Length() > 0.5f)
                {
                    MyGridPhysics physics = base.CubeGrid.Physics;
                    physics.LinearVelocity += vector2 * 0.05f;
                    MyGridPhysics physics2 = this.m_other.CubeGrid.Physics;
                    physics2.LinearVelocity -= vector2 * 0.05f;
                }
            }
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            if (!this.CheckUnobstructed())
            {
                if (this.SafeConstraint != null)
                {
                    this.RemoveConstraintInBoth();
                }
            }
            else if (this.SafeConstraint == null)
            {
                foreach (MyCubeGrid grid in this.m_gridList)
                {
                    if (grid.MarkedForClose)
                    {
                        continue;
                    }
                    Vector3I zero = Vector3I.Zero;
                    double maxValue = double.MaxValue;
                    LineD line = new LineD(base.Physics.ClusterToWorld(base.Physics.RigidBody.Position), base.Physics.ClusterToWorld(base.Physics.RigidBody.Position) + this.GetMergeNormalWorld());
                    if (grid.GetLineIntersectionExactGrid(ref line, ref zero, ref maxValue))
                    {
                        MyShipMergeBlock fatBlock = grid.GetCubeBlock(zero).FatBlock as MyShipMergeBlock;
                        if (fatBlock != null)
                        {
                            if ((!fatBlock.InConstraint && (fatBlock.IsWorking && (fatBlock.CheckUnobstructed() && (fatBlock.GetMergeNormalWorld().Dot(this.GetMergeNormalWorld()) <= 0f)))) && fatBlock.FriendlyWithBlock(this))
                            {
                                this.CreateConstraint(grid, fatBlock);
                                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                                this.m_updateBeforeFlags |= UpdateBeforeFlags.EnableConstraint;
                            }
                            break;
                        }
                    }
                }
            }
            else if (((base.CubeGrid.IsStatic || !this.m_other.CubeGrid.IsStatic) && (base.IsWorking && this.m_other.IsWorking)) && this.IsWithinWorldLimits)
            {
                MyMergeBlockDefinition blockDefinition = base.BlockDefinition as MyMergeBlockDefinition;
                if (blockDefinition != null)
                {
                    float strength = blockDefinition.Strength;
                }
                if ((((float) (base.WorldMatrix.Translation - this.m_other.WorldMatrix.Translation).Length()) - base.CubeGrid.GridSize) > (base.CubeGrid.GridSize * 3f))
                {
                    this.RemoveConstraintInBoth();
                }
                else
                {
                    MergeData data = new MergeData();
                    this.CalculateMergeData(ref data);
                    (this.m_constraint.ConstraintData as HkMalleableConstraintData).Strength = data.ConstraintStrength;
                    if ((!data.PositionOk || !data.AxisOk) || !data.RotationOk)
                    {
                        this.m_frameCounter = 0;
                    }
                    else
                    {
                        ushort frameCounter = this.m_frameCounter;
                        this.m_frameCounter = (ushort) (frameCounter + 1);
                        if (frameCounter >= 3)
                        {
                            Vector3I gridOffset = this.CalculateOtherGridOffset();
                            Vector3I vectori2 = this.m_other.CalculateOtherGridOffset();
                            if (!base.CubeGrid.CanMergeCubes(this.m_other.CubeGrid, gridOffset))
                            {
                                if (base.CubeGrid.GridSystems.ControlSystem.IsLocallyControlled || this.m_other.CubeGrid.GridSystems.ControlSystem.IsLocallyControlled)
                                {
                                    MyHud.Notifications.Add(MyNotificationSingletons.ObstructingBlockDuringMerge);
                                }
                            }
                            else
                            {
                                if (this.BeforeMerge != null)
                                {
                                    this.BeforeMerge();
                                }
                                if (Sync.IsServer)
                                {
                                    foreach (MySlimBlock block in base.CubeGrid.GetBlocks())
                                    {
                                        MyShipMergeBlock fatBlock = block.FatBlock as MyShipMergeBlock;
                                        if ((fatBlock != null) && (!ReferenceEquals(fatBlock, this) && fatBlock.InConstraint))
                                        {
                                            (block.FatBlock as MyShipMergeBlock).RemoveConstraintInBoth();
                                        }
                                    }
                                    if (base.CubeGrid.MergeGrid_MergeBlock(this.m_other.CubeGrid, gridOffset, true) == null)
                                    {
                                        this.m_other.CubeGrid.MergeGrid_MergeBlock(base.CubeGrid, vectori2, false);
                                    }
                                    this.RemoveConstraintInBoth();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateEmissivity()
        {
            switch (this.m_mergeState.Value)
            {
                case MergeState.UNSET:
                    break;

                case MergeState.NONE:
                {
                    MyShipMergeBlock otherMergeBlock = this.GetOtherMergeBlock();
                    if ((otherMergeBlock != null) && !otherMergeBlock.FriendlyWithBlock(this))
                    {
                        base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Working, base.Render.RenderObjectIDs[0], null);
                        return;
                    }
                    if (!base.IsFunctional)
                    {
                        this.SetEmissiveStateDamaged();
                        break;
                    }
                    this.SetEmissiveStateDisabled();
                    return;
                }
                case MergeState.WORKING:
                    base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Working, base.Render.RenderObjectIDs[0], null);
                    return;

                case MergeState.CONSTRAINED:
                    base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Constraint, base.Render.RenderObjectIDs[0], null);
                    return;

                case MergeState.LOCKED:
                    base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Locked, base.Render.RenderObjectIDs[0], null);
                    return;

                default:
                    return;
            }
        }

        public void UpdateIsWorkingBeforeNextFrame()
        {
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            this.m_updateBeforeFlags |= UpdateBeforeFlags.None | UpdateBeforeFlags.UpdateIsWorking;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if (this.m_updateBeforeFlags.HasFlag(UpdateBeforeFlags.EnableConstraint))
            {
                if (this.SafeConstraint != null)
                {
                    this.m_constraint.Enabled = true;
                }
            }
            else if (this.m_updateBeforeFlags.HasFlag(UpdateBeforeFlags.None | UpdateBeforeFlags.UpdateIsWorking))
            {
                base.UpdateIsWorking();
                this.UpdateState();
            }
            this.m_updateBeforeFlags = UpdateBeforeFlags.None;
        }

        private void UpdateState()
        {
            if (base.InScene && Sync.IsServer)
            {
                MergeState wORKING = MergeState.WORKING;
                MyShipMergeBlock otherMergeBlock = this.GetOtherMergeBlock();
                if (!base.IsWorking)
                {
                    wORKING = MergeState.NONE;
                }
                else if ((otherMergeBlock == null) || !otherMergeBlock.IsWorking)
                {
                    if (this.InConstraint)
                    {
                        wORKING = MergeState.CONSTRAINED;
                    }
                }
                else if (Base6Directions.GetFlippedDirection(otherMergeBlock.Orientation.TransformDirection(otherMergeBlock.m_forward)) == base.Orientation.TransformDirection(this.m_forward))
                {
                    wORKING = MergeState.LOCKED;
                }
                if (wORKING != ((MergeState) this.m_mergeState))
                {
                    this.m_mergeState.Value = wORKING;
                    this.UpdateEmissivity();
                    if (otherMergeBlock != null)
                    {
                        otherMergeBlock.UpdateIsWorkingBeforeNextFrame();
                    }
                }
            }
        }

        public bool InConstraint =>
            (this.m_constraint != null);

        private HkConstraint SafeConstraint
        {
            get
            {
                if ((this.m_constraint != null) && !this.m_constraint.InWorld)
                {
                    this.RemoveConstraintInBoth();
                }
                return this.m_constraint;
            }
        }

        public MyShipMergeBlock Other =>
            (this.m_other ?? this.GetOtherMergeBlock());

        public int GridCount =>
            this.m_gridList.Count;

        public Base6Directions.Direction OtherRight =>
            this.m_otherRight;

        private bool IsWithinWorldLimits =>
            ((MySession.Static.BlockLimitsEnabled != MyBlockLimitsEnabledEnum.NONE) ? ((MySession.Static.MaxGridSize == 0) || ((base.CubeGrid.BlocksCount + this.m_other.CubeGrid.BlocksCount) <= MySession.Static.MaxGridSize)) : true);

        public bool IsLocked =>
            (this.m_mergeState == 4);

        SpaceEngineers.Game.ModAPI.IMyShipMergeBlock SpaceEngineers.Game.ModAPI.IMyShipMergeBlock.Other =>
            this.Other;

        bool SpaceEngineers.Game.ModAPI.Ingame.IMyShipMergeBlock.IsConnected =>
            (this.Other != null);

        [StructLayout(LayoutKind.Sequential)]
        private struct MergeData
        {
            public bool PositionOk;
            public bool RotationOk;
            public bool AxisOk;
            public float Distance;
            public float RotationDelta;
            public float AxisDelta;
            public float ConstraintStrength;
            public float StrengthFactor;
            public Vector3 RelativeVelocity;
        }

        private enum MergeState
        {
            UNSET,
            NONE,
            WORKING,
            CONSTRAINED,
            LOCKED
        }

        [Flags]
        private enum UpdateBeforeFlags : byte
        {
            None = 0,
            EnableConstraint = 1,
            UpdateIsWorking = 2
        }
    }
}

