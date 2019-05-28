namespace Sandbox.Game.GameSystems.StructuralIntegrity
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyJacobianConstraintSimulator : IMyIntegritySimulator
    {
        private static readonly ConstraintComparer m_constraintComparer = new ConstraintComparer();
        private Dictionary<MyTuple<MySlimBlock, MySlimBlock>, ConstraintBase> m_constraints = new Dictionary<MyTuple<MySlimBlock, MySlimBlock>, ConstraintBase>(m_constraintComparer);
        private Dictionary<MySlimBlock, BlockState> m_statesByBlock = new Dictionary<MySlimBlock, BlockState>();
        private SolverFast m_solver = new SolverFast();
        private bool m_structureChanged = true;
        private int m_constraintAtomCount;

        public MyJacobianConstraintSimulator(int capacity)
        {
        }

        public void Add(MySlimBlock block)
        {
            BlockState orCreateState = this.GetOrCreateState(block);
            foreach (MySlimBlock block2 in block.Neighbours)
            {
                if (!this.HasConstraintBetween(block, block2))
                {
                    this.Add(orCreateState, this.GetOrCreateState(block2));
                }
            }
        }

        private void Add(BlockState a, BlockState b)
        {
            Vector3D vectord;
            Vector3D vectord2;
            a.Block.ComputeScaledCenter(out vectord);
            b.Block.ComputeScaledCenter(out vectord2);
            if ((vectord - vectord2).Sum < 0.0)
            {
                MyUtils.Swap<BlockState>(ref a, ref b);
            }
            MyTuple<MySlimBlock, MySlimBlock> tuple = new MyTuple<MySlimBlock, MySlimBlock>(a.Block, b.Block);
            ConstraintBase base2 = new ConstraintFixed1DoF();
            base2.Bind(a, b);
            this.m_constraints[tuple] = base2;
            this.m_structureChanged = true;
        }

        public void Close()
        {
        }

        public void DebugDraw()
        {
            using (Dictionary<MyTuple<MySlimBlock, MySlimBlock>, ConstraintBase>.ValueCollection.Enumerator enumerator = this.m_constraints.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.DebugDraw();
                }
            }
        }

        public void Draw()
        {
        }

        public void ForceRecalc()
        {
        }

        private void GenerateForces()
        {
            Vector3 vector = (Vector3) (9.8f * Vector3.Down);
            foreach (BlockState state in this.m_statesByBlock.Values)
            {
                if (!state.IsFixed)
                {
                    state.LinearAcceleration = vector;
                }
            }
        }

        private MyCubeGrid GetGrid()
        {
            MyCubeGrid cubeGrid = null;
            Dictionary<MySlimBlock, BlockState>.KeyCollection.Enumerator enumerator = this.m_statesByBlock.Keys.GetEnumerator();
            if (enumerator.MoveNext())
            {
                cubeGrid = enumerator.Current.CubeGrid;
            }
            return cubeGrid;
        }

        private BlockState GetOrCreateState(MySlimBlock block)
        {
            BlockState state;
            if (!this.m_statesByBlock.TryGetValue(block, out state))
            {
                state = new BlockState(block);
                this.m_statesByBlock.Add(block, state);
            }
            return state;
        }

        public float GetSupportedWeight(Vector3I pos) => 
            0f;

        public float GetTension(Vector3I pos) => 
            0f;

        private bool HasConstraintBetween(MySlimBlock a, MySlimBlock b) => 
            this.m_constraints.ContainsKey(new MyTuple<MySlimBlock, MySlimBlock>(a, b));

        private static void Integrate(BlockState state, float dt)
        {
            state.LinearVelocity += (state.LinearAcceleration * dt) - (0.005f * state.LinearVelocity);
            state.AngularVelocity += (state.AngularAcceleration * dt) - (0.005f * state.AngularVelocity);
        }

        public bool IsConnectionFine(MySlimBlock blockA, MySlimBlock blockB)
        {
            throw new NotImplementedException();
        }

        private void RebuildStructure()
        {
            if (this.m_structureChanged)
            {
                this.GetGrid();
                int num = 0;
                using (Dictionary<MySlimBlock, BlockState>.ValueCollection.Enumerator enumerator = this.m_statesByBlock.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Index = num;
                        num++;
                    }
                }
                this.m_constraintAtomCount = 0;
                foreach (KeyValuePair<MyTuple<MySlimBlock, MySlimBlock>, ConstraintBase> pair in this.m_constraints)
                {
                    pair.Value.Index = this.m_constraintAtomCount;
                    this.m_constraintAtomCount += pair.Value.AtomCount;
                }
                this.m_structureChanged = false;
            }
        }

        public void Remove(MySlimBlock block)
        {
            this.m_statesByBlock.Remove(block);
            foreach (MySlimBlock block2 in block.Neighbours)
            {
                this.m_constraints.Remove(new MyTuple<MySlimBlock, MySlimBlock>(block, block2));
                this.m_structureChanged = true;
            }
        }

        public bool Simulate(float deltaTime)
        {
            if (this.m_constraints.Count == 0)
            {
                return false;
            }
            this.RebuildStructure();
            this.GenerateForces();
            Dictionary<MySlimBlock, BlockState>.ValueCollection.Enumerator blocks = this.m_statesByBlock.Values.GetEnumerator();
            Dictionary<MyTuple<MySlimBlock, MySlimBlock>, ConstraintBase>.ValueCollection.Enumerator enumerator = this.m_constraints.Values.GetEnumerator();
            this.m_solver.Setup<Dictionary<MySlimBlock, BlockState>.ValueCollection.Enumerator, Dictionary<MyTuple<MySlimBlock, MySlimBlock>, ConstraintBase>.ValueCollection.Enumerator>(deltaTime, this.m_statesByBlock.Count, blocks, this.m_constraintAtomCount, enumerator);
            this.m_solver.Solve(10);
            this.m_solver.ApplySolution<Dictionary<MySlimBlock, BlockState>.ValueCollection.Enumerator, Dictionary<MyTuple<MySlimBlock, MySlimBlock>, ConstraintBase>.ValueCollection.Enumerator>(blocks, enumerator);
            foreach (BlockState state in this.m_statesByBlock.Values)
            {
                if (!state.IsFixed)
                {
                    Integrate(state, deltaTime);
                }
                state.LinearAcceleration = Vector3.Zero;
                state.AngularAcceleration = Vector3.Zero;
            }
            return true;
        }

        public bool EnabledMovement =>
            false;

        private class BlockState
        {
            public MySlimBlock Block;
            public bool IsFixed;
            public int Index;
            public Vector3 LocalInertiaInv;
            public Vector3 LinearAcceleration;
            public Vector3 LinearVelocity;
            public Vector3 AngularAcceleration;
            public Vector3 AngularVelocity;
            public MyTransform Transform;

            public BlockState(MySlimBlock block)
            {
                Vector3D vectord;
                Vector3 vector;
                this.Block = block;
                this.IsFixed = MyCubeGrid.IsInVoxels(this.Block, true);
                block.ComputeScaledCenter(out vectord);
                block.ComputeScaledHalfExtents(out vector);
                vector *= 2f;
                this.Transform = new MyTransform((Vector3) vectord);
                float num = 0.08333334f * block.BlockDefinition.Mass;
                this.LocalInertiaInv = (Vector3) (1f / new Vector3(num * ((vector.Y * vector.Y) + (vector.Z * vector.Z)), num * ((vector.X * vector.X) + (vector.Z * vector.Z)), num * ((vector.X * vector.X) + (vector.Y * vector.Y))));
            }

            public void ComputeTransformedInvInertia(out Matrix outInvInertia)
            {
                Matrix matrix;
                Matrix matrix2;
                Matrix.CreateScale(ref this.LocalInertiaInv, out matrix);
                Matrix.CreateFromQuaternion(ref this.Transform.Rotation, out matrix2);
                outInvInertia = Matrix.Multiply(Matrix.Multiply(matrix2, matrix), Matrix.Transpose(matrix2));
            }

            public float Mass =>
                (this.IsFixed ? float.PositiveInfinity : this.Block.BlockDefinition.Mass);
        }

        private abstract class ConstraintBase
        {
            protected MyJacobianConstraintSimulator.BlockState m_blockA;
            protected MyJacobianConstraintSimulator.BlockState m_blockB;
            protected MyTransform m_pivotA;
            protected MyTransform m_pivotB;
            public int Index;
            protected Color m_debugLineColor;

            protected ConstraintBase()
            {
            }

            public virtual void Bind(MyJacobianConstraintSimulator.BlockState blockA, MyJacobianConstraintSimulator.BlockState blockB)
            {
                Vector3D vectord;
                Vector3D vectord2;
                this.m_blockA = blockA;
                this.m_blockB = blockB;
                blockA.Block.ComputeScaledCenter(out vectord);
                blockB.Block.ComputeScaledCenter(out vectord2);
                Vector3 vector = (Vector3) ((vectord + vectord2) * 0.5);
                this.m_pivotA = new MyTransform(vector - vectord);
                this.m_pivotB = new MyTransform(vector - vectord2);
            }

            public virtual void DebugDraw()
            {
                MyTransform transform = new MyTransform((Matrix) this.m_blockA.Block.CubeGrid.WorldMatrix);
                Vector3 pointFrom = MyTransform.Transform(ref this.m_blockA.Transform.Position, ref transform);
                Vector3 pointTo = MyTransform.Transform(ref this.m_blockB.Transform.Position, ref transform);
                MyRenderProxy.DebugDrawLine3D(pointFrom, pointTo, this.m_debugLineColor, this.m_debugLineColor, false, false);
            }

            public abstract void ReadStrain(MyJacobianConstraintSimulator.SolverConstraint[] constraints);
            public static void ResetMaxStrain()
            {
                MaxStrain = 0f;
            }

            public abstract void SetAtoms(MyJacobianConstraintSimulator.SolverConstraint[] constraints, float invTimeStep);

            public static float MaxStrain
            {
                [CompilerGenerated]
                get => 
                    <MaxStrain>k__BackingField;
                [CompilerGenerated]
                protected set => 
                    (<MaxStrain>k__BackingField = value);
            }

            public MyJacobianConstraintSimulator.BlockState BlockA =>
                this.m_blockA;

            public MyJacobianConstraintSimulator.BlockState BlockB =>
                this.m_blockB;

            public abstract int AtomCount { get; }
        }

        private class ConstraintComparer : IEqualityComparer<MyTuple<MySlimBlock, MySlimBlock>>
        {
            public bool Equals(MyTuple<MySlimBlock, MySlimBlock> x, MyTuple<MySlimBlock, MySlimBlock> y)
            {
                if ((x.Item1 != y.Item1) || (x.Item2 != y.Item2))
                {
                    return ((x.Item1 == y.Item2) && (x.Item2 == y.Item1));
                }
                return true;
            }

            public int GetHashCode(MyTuple<MySlimBlock, MySlimBlock> obj) => 
                (obj.Item1.GetHashCode() ^ obj.Item2.GetHashCode());
        }

        private class ConstraintFixed1DoF : MyJacobianConstraintSimulator.ConstraintBase
        {
            private float m_strain;
            private float m_appliedImpuls;

            public override void DebugDraw()
            {
                float num = MathHelper.Clamp((float) (this.m_strain / MyJacobianConstraintSimulator.ConstraintBase.MaxStrain), (float) 0f, (float) 1f);
                base.m_debugLineColor = (num >= 0.5f) ? Color.Lerp(Color.Yellow, Color.Red, (num - 0.5f) * 2f) : Color.Lerp(Color.Green, Color.Yellow, num * 2f);
                base.DebugDraw();
            }

            public override void ReadStrain(MyJacobianConstraintSimulator.SolverConstraint[] constraints)
            {
                this.m_appliedImpuls = constraints[base.Index].m_appliedImpulse;
                this.m_strain = Math.Abs(this.m_appliedImpuls);
                if (MyJacobianConstraintSimulator.ConstraintBase.MaxStrain < this.m_strain)
                {
                    MyJacobianConstraintSimulator.ConstraintBase.MaxStrain = this.m_strain;
                }
            }

            public override void SetAtoms(MyJacobianConstraintSimulator.SolverConstraint[] constraints, float invTimeStep)
            {
                Vector3 vector = Vector3.Transform(base.m_pivotA.Position, base.m_blockA.Transform.Rotation);
                Vector3 vector2 = Vector3.Transform(base.m_pivotB.Position, base.m_blockB.Transform.Rotation);
                constraints[base.Index].m_JaLinearAxis.Y = 1f;
                constraints[base.Index].m_JbLinearAxis.Y = -1f;
                float num = ((0.85f * invTimeStep) * invTimeStep) * (((vector2.Y + base.m_blockB.Transform.Position.Y) - vector.Y) - base.m_blockA.Transform.Position.Y);
                constraints[base.Index].m_rhs = num;
                constraints[base.Index].m_appliedImpulse = this.m_appliedImpuls;
            }

            public override int AtomCount =>
                1;
        }

        private class ConstraintFixed3DoF : MyJacobianConstraintSimulator.ConstraintBase
        {
            private float m_strain;
            private float[] m_appliedImpulses;

            public ConstraintFixed3DoF()
            {
                this.m_appliedImpulses = new float[this.AtomCount];
            }

            public override void DebugDraw()
            {
                float num = MathHelper.Clamp((float) (this.m_strain / MyJacobianConstraintSimulator.ConstraintBase.MaxStrain), (float) 0f, (float) 1f);
                base.m_debugLineColor = (num >= 0.5f) ? Color.Lerp(Color.Yellow, Color.Red, (num - 0.5f) * 2f) : Color.Lerp(Color.Green, Color.Yellow, num * 2f);
                base.DebugDraw();
            }

            public override void ReadStrain(MyJacobianConstraintSimulator.SolverConstraint[] constraints)
            {
                this.m_strain = 0f;
                for (int i = 0; i < this.AtomCount; i++)
                {
                    this.m_appliedImpulses[i] = constraints[base.Index + i].m_appliedImpulse;
                    this.m_strain += Math.Abs(this.m_appliedImpulses[i]);
                }
                if (MyJacobianConstraintSimulator.ConstraintBase.MaxStrain < this.m_strain)
                {
                    MyJacobianConstraintSimulator.ConstraintBase.MaxStrain = this.m_strain;
                }
            }

            public override void SetAtoms(MyJacobianConstraintSimulator.SolverConstraint[] constraints, float invTimeStep)
            {
                Vector3 vector = Vector3.Transform(base.m_pivotA.Position, base.m_blockA.Transform.Rotation);
                Vector3 vector2 = Vector3.Transform(base.m_pivotB.Position, base.m_blockB.Transform.Rotation);
                constraints[base.Index].m_JaLinearAxis.X = 1f;
                constraints[base.Index + 1].m_JaLinearAxis.Y = 1f;
                constraints[base.Index + 2].m_JaLinearAxis.Z = 1f;
                constraints[base.Index].m_JbLinearAxis.X = -1f;
                constraints[base.Index + 1].m_JbLinearAxis.Y = -1f;
                constraints[base.Index + 2].m_JbLinearAxis.Z = -1f;
                Vector3 vector3 = (Vector3) (((0.85f * invTimeStep) * invTimeStep) * (((vector2 + base.m_blockB.Transform.Position) - vector) - base.m_blockA.Transform.Position));
                constraints[base.Index].m_rhs = vector3.X;
                constraints[base.Index + 1].m_rhs = vector3.Y;
                constraints[base.Index + 2].m_rhs = vector3.Z;
                for (int i = 0; i < this.AtomCount; i++)
                {
                    constraints[base.Index + i].m_appliedImpulse = this.m_appliedImpulses[i];
                }
            }

            public override int AtomCount =>
                3;
        }

        private class ConstraintFixed6DoF : MyJacobianConstraintSimulator.ConstraintBase
        {
            private float m_strain;
            private float[] m_appliedImpulses;

            public ConstraintFixed6DoF()
            {
                this.m_appliedImpulses = new float[this.AtomCount];
            }

            public override void DebugDraw()
            {
                float num = MathHelper.Clamp((float) (this.m_strain / MyJacobianConstraintSimulator.ConstraintBase.MaxStrain), (float) 0f, (float) 1f);
                base.m_debugLineColor = (num >= 0.5f) ? Color.Lerp(Color.Yellow, Color.Red, (num - 0.5f) * 2f) : Color.Lerp(Color.Green, Color.Yellow, num * 2f);
                base.DebugDraw();
            }

            public override void ReadStrain(MyJacobianConstraintSimulator.SolverConstraint[] constraints)
            {
                this.m_strain = 0f;
                for (int i = 0; i < this.AtomCount; i++)
                {
                    this.m_appliedImpulses[i] = constraints[base.Index + i].m_appliedImpulse;
                    this.m_strain += Math.Abs(this.m_appliedImpulses[i]);
                }
                if (MyJacobianConstraintSimulator.ConstraintBase.MaxStrain < this.m_strain)
                {
                    MyJacobianConstraintSimulator.ConstraintBase.MaxStrain = this.m_strain;
                }
            }

            public override void SetAtoms(MyJacobianConstraintSimulator.SolverConstraint[] constraints, float invTimeStep)
            {
                Vector3 vector4;
                Vector3 vector = Vector3.Transform(base.m_pivotA.Position, base.m_blockA.Transform.Rotation);
                Vector3 vector2 = Vector3.Transform(base.m_pivotB.Position, base.m_blockB.Transform.Rotation);
                constraints[base.Index].m_JaLinearAxis.X = 1f;
                constraints[base.Index + 1].m_JaLinearAxis.Y = 1f;
                constraints[base.Index + 2].m_JaLinearAxis.Z = 1f;
                constraints[base.Index].m_JbLinearAxis.X = -1f;
                constraints[base.Index + 1].m_JbLinearAxis.Y = -1f;
                constraints[base.Index + 2].m_JbLinearAxis.Z = -1f;
                constraints[base.Index].m_JaAngularAxis = new Vector3(0f, vector.Z, -vector.Y);
                constraints[base.Index + 1].m_JaAngularAxis = new Vector3(-vector.Z, 0f, vector.X);
                constraints[base.Index + 2].m_JaAngularAxis = new Vector3(vector.Y, -vector.X, 0f);
                constraints[base.Index].m_JbAngularAxis = new Vector3(0f, -vector2.Z, vector2.Y);
                constraints[base.Index + 1].m_JbAngularAxis = new Vector3(vector2.Z, 0f, -vector2.X);
                constraints[base.Index + 2].m_JbAngularAxis = new Vector3(-vector2.Y, vector2.X, 0f);
                constraints[base.Index + 3].m_JaAngularAxis.X = 1f;
                constraints[base.Index + 4].m_JaAngularAxis.Y = 1f;
                constraints[base.Index + 5].m_JaAngularAxis.Z = 1f;
                constraints[base.Index + 3].m_JbAngularAxis.X = -1f;
                constraints[base.Index + 4].m_JbAngularAxis.Y = -1f;
                constraints[base.Index + 5].m_JbAngularAxis.Z = -1f;
                Vector3 vector3 = (Vector3) (((0.85f * invTimeStep) * invTimeStep) * (((vector2 + base.m_blockB.Transform.Position) - vector) - base.m_blockA.Transform.Position));
                constraints[base.Index].m_rhs = vector3.X;
                constraints[base.Index + 1].m_rhs = vector3.Y;
                constraints[base.Index + 2].m_rhs = vector3.Z;
                Matrix.GetEulerAnglesXYZ(ref Matrix.CreateFromQuaternion(Quaternion.Multiply(base.m_blockA.Transform.Rotation, Quaternion.Inverse(base.m_blockB.Transform.Rotation))), out vector4);
                vector4 *= (0.85f * invTimeStep) * invTimeStep;
                constraints[base.Index + 3].m_rhs = vector4.X;
                constraints[base.Index + 4].m_rhs = vector4.Y;
                constraints[base.Index + 5].m_rhs = vector4.Z;
                for (int i = 0; i < this.AtomCount; i++)
                {
                    constraints[base.Index + i].m_appliedImpulse = this.m_appliedImpulses[i];
                }
            }

            public override int AtomCount =>
                6;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SolverBody
        {
            public float InvMass;
            public Matrix InvInertiaWorld;
            public Vector3 DeltaLinearAcceleration;
            public Vector3 DeltaAngularAcceleration;
            public void ApplyImpulse(ref Vector3 linearComponent, ref Vector3 angularComponent, float impulseMagnitude)
            {
                this.DeltaLinearAcceleration += impulseMagnitude * linearComponent;
                this.DeltaAngularAcceleration += impulseMagnitude * angularComponent;
            }

            public void ApplyImpulse(ref Vector3 linearComponent, float impulseMagnitude)
            {
                this.DeltaLinearAcceleration += impulseMagnitude * linearComponent;
            }

            public void Reset()
            {
                this.InvMass = 0f;
                this.InvInertiaWorld = Matrix.Zero;
                this.DeltaLinearAcceleration = Vector3.Zero;
                this.DeltaAngularAcceleration = Vector3.Zero;
            }

            public override string ToString() => 
                $"dAl {this.DeltaLinearAcceleration}";
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SolverConstraint
        {
            public Vector3 m_JaAngularAxis;
            public Vector3 m_JbAngularAxis;
            public Vector3 m_JaLinearAxis;
            public Vector3 m_JbLinearAxis;
            public float m_rhs;
            public Vector3 m_angularComponentA;
            public Vector3 m_angularComponentB;
            public float m_jacDiagABInv;
            public float m_appliedImpulse;
            public int m_solverBodyIdA;
            public int m_solverBodyIdB;
            public void Reset()
            {
                this.m_JaAngularAxis = Vector3.Zero;
                this.m_JaLinearAxis = Vector3.Zero;
                this.m_JbAngularAxis = Vector3.Zero;
                this.m_JbLinearAxis = Vector3.Zero;
                this.m_angularComponentA = Vector3.Zero;
                this.m_angularComponentB = Vector3.Zero;
                this.m_rhs = 0f;
                this.m_jacDiagABInv = 0f;
                this.m_appliedImpulse = 0f;
                this.m_solverBodyIdA = 0;
                this.m_solverBodyIdB = 0;
            }

            public override string ToString() => 
                $"J = [ {this.m_JaLinearAxis} | {this.m_JaAngularAxis} | {this.m_JbLinearAxis} | {this.m_JbAngularAxis} ], Impulse = {this.m_appliedImpulse}, RHS = {this.m_rhs}";
        }

        private class SolverFast
        {
            private MyJacobianConstraintSimulator.SolverConstraint[] m_constraints;
            private MyJacobianConstraintSimulator.SolverBody[] m_bodies;
            private int m_constraintAtomCount;
            private float m_deltaTime;
            private float m_invDeltaTime;

            private void AddBlock(MyJacobianConstraintSimulator.BlockState block)
            {
                int index = block.Index;
                this.m_bodies[index].InvMass = 1f / block.Mass;
                this.m_bodies[index].DeltaLinearAcceleration = Vector3.Zero;
                if (this.m_bodies[index].InvMass != 0f)
                {
                    block.ComputeTransformedInvInertia(out this.m_bodies[index].InvInertiaWorld);
                }
                else
                {
                    this.m_bodies[index].InvInertiaWorld = Matrix.Zero;
                }
                this.m_bodies[index].DeltaAngularAcceleration = Vector3.Zero;
            }

            private unsafe void AddConstraint(MyJacobianConstraintSimulator.ConstraintBase constraint)
            {
                constraint.SetAtoms(this.m_constraints, this.m_invDeltaTime);
                int num = constraint.Index + constraint.AtomCount;
                MyJacobianConstraintSimulator.BlockState blockA = constraint.BlockA;
                MyJacobianConstraintSimulator.BlockState blockB = constraint.BlockB;
                for (int i = constraint.Index; i < num; i++)
                {
                    Vector3 vector3;
                    Vector3 vector4;
                    int index = blockA.Index;
                    int num4 = blockB.Index;
                    this.m_constraints[i].m_solverBodyIdA = index;
                    this.m_constraints[i].m_solverBodyIdB = num4;
                    Vector3.TransformNormal(ref this.m_constraints[i].m_JaAngularAxis, ref this.m_bodies[index].InvInertiaWorld, out this.m_constraints[i].m_angularComponentA);
                    Vector3.TransformNormal(ref this.m_constraints[i].m_JbAngularAxis, ref this.m_bodies[num4].InvInertiaWorld, out this.m_constraints[i].m_angularComponentB);
                    Vector3 vector = this.m_constraints[i].m_JaLinearAxis * this.m_bodies[index].InvMass;
                    Vector3 vector2 = this.m_constraints[i].m_JbLinearAxis * this.m_bodies[num4].InvMass;
                    Vector3.TransformNormal(ref this.m_constraints[i].m_JaAngularAxis, ref this.m_bodies[index].InvInertiaWorld, out vector3);
                    Vector3.TransformNormal(ref this.m_constraints[i].m_JbAngularAxis, ref this.m_bodies[num4].InvInertiaWorld, out vector4);
                    float num5 = Math.Abs((float) (((vector.Dot(ref this.m_constraints[i].m_JaLinearAxis) + vector2.Dot(ref this.m_constraints[i].m_JbLinearAxis)) + vector3.Dot(ref this.m_constraints[i].m_JaAngularAxis)) + vector4.Dot(ref this.m_constraints[i].m_JbAngularAxis)));
                    this.m_constraints[i].m_jacDiagABInv = (num5 > 1E-05f) ? (1f / num5) : 0f;
                    float num6 = this.m_constraints[i].m_JaLinearAxis.Dot(((Vector3) (this.m_invDeltaTime * blockA.LinearVelocity)) + blockA.LinearAcceleration) + this.m_constraints[i].m_JaAngularAxis.Dot(((Vector3) (this.m_invDeltaTime * blockA.AngularVelocity)) + blockA.AngularAcceleration);
                    this.m_constraints[i].m_rhs = (this.m_constraints[i].m_rhs * this.m_constraints[i].m_jacDiagABInv) - ((num6 + (this.m_constraints[i].m_JbLinearAxis.Dot(((Vector3) (this.m_invDeltaTime * blockB.LinearVelocity)) + blockB.LinearAcceleration) + this.m_constraints[i].m_JbAngularAxis.Dot(((Vector3) (this.m_invDeltaTime * blockB.AngularVelocity)) + blockB.AngularAcceleration))) * this.m_constraints[i].m_jacDiagABInv);
                    float* singlePtr1 = (float*) ref this.m_constraints[i].m_appliedImpulse;
                    singlePtr1[0] *= 0.25f;
                    Vector3 linearComponent = this.m_constraints[i].m_JaLinearAxis * this.m_bodies[index].InvMass;
                    Vector3 vector6 = this.m_constraints[i].m_JbLinearAxis * this.m_bodies[num4].InvMass;
                    this.m_bodies[index].ApplyImpulse(ref linearComponent, ref this.m_constraints[i].m_angularComponentA, this.m_constraints[i].m_appliedImpulse);
                    this.m_bodies[num4].ApplyImpulse(ref vector6, ref this.m_constraints[i].m_angularComponentB, this.m_constraints[i].m_appliedImpulse);
                }
            }

            public void ApplySolution<TBlocks, TConstraints>(TBlocks blocks, TConstraints constraints) where TBlocks: struct, IEnumerator<MyJacobianConstraintSimulator.BlockState> where TConstraints: struct, IEnumerator<MyJacobianConstraintSimulator.ConstraintBase>
            {
                while (blocks.MoveNext())
                {
                    MyJacobianConstraintSimulator.BlockState current = blocks.Current;
                    int index = current.Index;
                    current.LinearAcceleration += this.m_bodies[index].DeltaLinearAcceleration;
                    current.AngularAcceleration += this.m_bodies[index].DeltaAngularAcceleration;
                }
                MyJacobianConstraintSimulator.ConstraintBase.ResetMaxStrain();
                while (constraints.MoveNext())
                {
                    constraints.Current.ReadStrain(this.m_constraints);
                }
            }

            private unsafe float resolveSingleConstraint(ref MyJacobianConstraintSimulator.SolverBody bodyA, ref MyJacobianConstraintSimulator.SolverBody bodyB, ref MyJacobianConstraintSimulator.SolverConstraint c)
            {
                float impulseMagnitude = (c.m_rhs - ((c.m_JaLinearAxis.Dot(ref bodyA.DeltaLinearAcceleration) + c.m_JaAngularAxis.Dot(ref bodyA.DeltaAngularAcceleration)) * c.m_jacDiagABInv)) - ((c.m_JbLinearAxis.Dot(ref bodyB.DeltaLinearAcceleration) + c.m_JbAngularAxis.Dot(ref bodyB.DeltaAngularAcceleration)) * c.m_jacDiagABInv);
                float* singlePtr1 = (float*) ref c.m_appliedImpulse;
                singlePtr1[0] += impulseMagnitude;
                Vector3 linearComponent = (Vector3) (bodyA.InvMass * c.m_JaLinearAxis);
                Vector3 vector2 = (Vector3) (bodyB.InvMass * c.m_JbLinearAxis);
                bodyA.ApplyImpulse(ref linearComponent, ref c.m_angularComponentA, impulseMagnitude);
                bodyB.ApplyImpulse(ref vector2, ref c.m_angularComponentB, impulseMagnitude);
                return impulseMagnitude;
            }

            private void SetSize(int bodyCount, int constraintAtomCount)
            {
                if ((this.m_bodies == null) || (this.m_bodies.Length < bodyCount))
                {
                    this.m_bodies = new MyJacobianConstraintSimulator.SolverBody[bodyCount];
                }
                else
                {
                    for (int i = 0; i < bodyCount; i++)
                    {
                        this.m_bodies[i].Reset();
                    }
                }
                if ((this.m_constraints == null) || (this.m_constraints.Length < constraintAtomCount))
                {
                    this.m_constraints = new MyJacobianConstraintSimulator.SolverConstraint[constraintAtomCount];
                }
                else
                {
                    for (int i = 0; i < constraintAtomCount; i++)
                    {
                        this.m_constraints[i].Reset();
                    }
                }
                this.m_constraintAtomCount = constraintAtomCount;
            }

            public void Setup<TBlocks, TConstraints>(float deltaTime, int blocksCount, TBlocks blocks, int constraintAtomCount, TConstraints constraints) where TBlocks: struct, IEnumerator<MyJacobianConstraintSimulator.BlockState> where TConstraints: struct, IEnumerator<MyJacobianConstraintSimulator.ConstraintBase>
            {
                this.m_deltaTime = deltaTime;
                this.m_invDeltaTime = 1f / deltaTime;
                this.SetSize(blocksCount, constraintAtomCount);
                while (blocks.MoveNext())
                {
                    this.AddBlock(blocks.Current);
                }
                while (constraints.MoveNext())
                {
                    this.AddConstraint(constraints.Current);
                }
            }

            public void Solve(int maxIterations)
            {
                float num = 0f;
                float positiveInfinity = float.PositiveInfinity;
                int num3 = 0;
                while (true)
                {
                    if (num3 >= maxIterations)
                    {
                        break;
                    }
                    num = 0f;
                    int index = 0;
                    while (true)
                    {
                        if (index < this.m_constraintAtomCount)
                        {
                            float num5 = this.resolveSingleConstraint(ref this.m_bodies[this.m_constraints[index].m_solverBodyIdA], ref this.m_bodies[this.m_constraints[index].m_solverBodyIdB], ref this.m_constraints[index]);
                            num += Math.Abs(num5);
                            index++;
                            continue;
                        }
                        if ((positiveInfinity <= num) || ((positiveInfinity - num) >= 1f))
                        {
                            positiveInfinity = num;
                            num3++;
                            break;
                        }
                        break;
                    }
                }
            }
        }
    }
}

