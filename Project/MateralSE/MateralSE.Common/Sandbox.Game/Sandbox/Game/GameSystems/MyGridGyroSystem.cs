namespace Sandbox.Game.GameSystems
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGridGyroSystem
    {
        private static readonly float INV_TENSOR_MAX_LIMIT = 125000f;
        private static readonly float MAX_SLOWDOWN = (MyFakes.WELD_LANDING_GEARS ? 0.8f : 0.93f);
        private static readonly float MAX_ROLL = 1.570796f;
        private const float TORQUE_SQ_LEN_TH = 0.0001f;
        private Vector3 m_controlTorque;
        public bool AutopilotEnabled;
        private MyCubeGrid m_grid;
        private HashSet<MyGyro> m_gyros;
        private bool m_gyrosChanged;
        private float m_maxGyroForce;
        private float m_maxOverrideForce;
        private float m_maxRequiredPowerInput;
        private Vector3 m_overrideTargetVelocity;
        private int? m_overrideAccelerationRampFrames;
        public Vector3 SlowdownTorque;

        public MyGridGyroSystem(MyCubeGrid grid)
        {
            this.m_grid = grid;
            this.m_gyros = new HashSet<MyGyro>();
            this.m_gyrosChanged = false;
            this.ResourceSink = new MyResourceSinkComponent(1);
            this.ResourceSink.Init(MyStringHash.GetOrCompute("Gyro"), this.m_maxRequiredPowerInput, () => this.m_maxRequiredPowerInput);
            this.ResourceSink.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            this.MarkDirty();
        }

        public void DebugDraw()
        {
            double num = 4.5 * 0.045;
            Vector3D translation = this.m_grid.WorldMatrix.Translation;
            Vector3D up = MySector.MainCamera.WorldMatrix.Up;
            Vector3D right = MySector.MainCamera.WorldMatrix.Right;
            double num3 = Math.Atan(4.5 / Math.Max(Vector3D.Distance(translation, MySector.MainCamera.Position), 0.001));
            if (num3 > 0.27000001072883606)
            {
                int num1;
                MyRenderProxy.DebugDrawText3D(translation, $"Grid {this.m_grid} Gyro System", Color.Yellow, (float) num3, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, -1, false);
                bool flag = this.Torque.LengthSquared() >= 0.0001f;
                bool flag2 = this.SlowdownTorque.LengthSquared() > 0.0001f;
                bool flag3 = this.m_overrideTargetVelocity.LengthSquared() > 0.0001f;
                if (this.m_grid.Physics != null)
                {
                    num1 = (int) (this.m_grid.Physics.AngularVelocity.LengthSquared() > 1E-05f);
                }
                else
                {
                    num1 = 0;
                }
                bool flag4 = (bool) num1;
                this.DebugDrawText($"Gyro count: {this.GyroCount}", translation + ((-1.0 * up) * num), right, (float) num3);
                this.DebugDrawText(string.Format("Torque [above threshold - {1}]: {0}", this.Torque, flag), translation + ((-2.0 * up) * num), right, (float) num3);
                this.DebugDrawText(string.Format("Slowdown [above threshold - {1}]: {0}", this.SlowdownTorque, flag2), translation + ((-3.0 * up) * num), right, (float) num3);
                this.DebugDrawText(string.Format("Override [above threshold - {1}]: {0}", this.m_overrideTargetVelocity, flag3), translation + ((-4.0 * up) * num), right, (float) num3);
                this.DebugDrawText($"Angular velocity above threshold - {flag4}", translation + ((-5.0 * up) * num), right, (float) num3);
                this.DebugDrawText($"Needs per frame update - {this.NeedsPerFrameUpdate}", translation + ((-6.0 * up) * num), right, (float) num3);
                if (this.m_grid.Physics != null)
                {
                    this.DebugDrawText($"Automatic deactivation enabled - {this.m_grid.Physics.RigidBody.EnableDeactivation}", translation + ((-7.0 * up) * num), right, (float) num3);
                }
            }
        }

        private void DebugDrawText(string text, Vector3D origin, Vector3D rightVector, float textSize)
        {
            Vector3D vectord = (Vector3D) (0.05000000074505806 * rightVector);
            MyRenderProxy.DebugDrawLine3D(origin, origin + vectord, Color.White, Color.White, false, false);
            MyRenderProxy.DebugDrawText3D((origin + vectord) + (rightVector * 0.014999999664723873), text, Color.White, textSize, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, -1, false);
        }

        public Vector3 GetAngularVelocity(Vector3 control)
        {
            if (((this.ResourceSink.SuppliedRatio > 0f) && ((this.m_grid.Physics != null) && this.m_grid.Physics.Enabled)) && !this.m_grid.Physics.RigidBody.IsFixed)
            {
                Matrix orientation = (Matrix) this.m_grid.PositionComp.WorldMatrixInvScaled.GetOrientation();
                Matrix matrix = (Matrix) this.m_grid.WorldMatrix.GetOrientation();
                Vector3 vector = Vector3.Transform(this.m_grid.Physics.AngularVelocity, ref orientation);
                Matrix inverseInertiaTensor = this.m_grid.Physics.RigidBody.InverseInertiaTensor;
                Vector3 vector2 = new Vector3(inverseInertiaTensor.M11, inverseInertiaTensor.M22, inverseInertiaTensor.M33);
                float num = vector2.Min();
                float num2 = Math.Max((float) 1f, (float) (num * INV_TENSOR_MAX_LIMIT));
                Vector3 zero = Vector3.Zero;
                float radius = this.m_maxOverrideForce + (this.m_maxGyroForce * (1f - control.Length()));
                Vector3 vector4 = ((this.m_overrideTargetVelocity - vector) * 60f) * Vector3.Normalize(vector2);
                Vector3 vector5 = vector4 / vector2;
                float num4 = vector5.Length() / radius;
                if ((num4 < 0.5f) && (this.m_overrideTargetVelocity.LengthSquared() < 2.5E-05f))
                {
                    return this.m_overrideTargetVelocity;
                }
                if (!Vector3.IsZero(vector4, 0.0001f))
                {
                    float num5 = 1f - (0.8f / ((float) Math.Exp((double) (0.5f * num4))));
                    zero = ((Vector3.ClampToSphere(vector5, radius) * 0.95f) * num5) + ((vector5 * 0.05f) * (1f - num5));
                    if (this.m_grid.GridSizeEnum == MyCubeSize.Large)
                    {
                        zero *= 2f;
                    }
                }
                this.Torque = ((control * this.m_maxGyroForce) + zero) / num2;
                this.Torque *= this.ResourceSink.SuppliedRatio;
                if (this.Torque.LengthSquared() > 0.0001f)
                {
                    return Vector3.Transform(vector + ((this.Torque * new Vector3(num)) * 0.01666667f), ref matrix);
                }
                if (((control == Vector3.Zero) && ((this.m_overrideTargetVelocity == Vector3.Zero) && ((this.m_grid.Physics.AngularVelocity != Vector3.Zero) && (this.m_grid.Physics.AngularVelocity.LengthSquared() < 9.000001E-08f)))) && this.m_grid.Physics.RigidBody.IsActive)
                {
                    return Vector3.Zero;
                }
            }
            return ((this.m_grid.Physics == null) ? Vector3.Zero : this.m_grid.Physics.AngularVelocity);
        }

        private void gyro_EnabledChanged(MyTerminalBlock obj)
        {
            this.MarkDirty();
        }

        private void gyro_PropertiesChanged(MyTerminalBlock sender)
        {
            this.MarkDirty();
        }

        private bool IsUsed(MyGyro gyro) => 
            (gyro.Enabled && gyro.IsFunctional);

        public void MarkDirty()
        {
            this.m_gyrosChanged = true;
            this.m_grid.MarkForUpdate();
        }

        private void Receiver_IsPoweredChanged()
        {
            using (HashSet<MyGyro>.Enumerator enumerator = this.m_gyros.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateIsWorking();
                }
            }
        }

        private void RecomputeGyroParameters()
        {
            this.m_gyrosChanged = false;
            float maxRequiredPowerInput = this.m_maxRequiredPowerInput;
            this.m_maxGyroForce = 0f;
            this.m_maxOverrideForce = 0f;
            this.m_maxRequiredPowerInput = 0f;
            this.m_overrideTargetVelocity = Vector3.Zero;
            this.m_overrideAccelerationRampFrames = null;
            foreach (MyGyro gyro in this.m_gyros)
            {
                if (this.IsUsed(gyro))
                {
                    if (!gyro.GyroOverride || this.AutopilotEnabled)
                    {
                        this.m_maxGyroForce += gyro.MaxGyroForce;
                    }
                    else
                    {
                        this.m_overrideTargetVelocity += gyro.GyroOverrideVelocityGrid * gyro.MaxGyroForce;
                        this.m_maxOverrideForce += gyro.MaxGyroForce;
                    }
                    this.m_maxRequiredPowerInput += gyro.RequiredPowerInput;
                }
            }
            if (this.m_maxOverrideForce != 0f)
            {
                this.m_overrideTargetVelocity /= this.m_maxOverrideForce;
            }
            this.ResourceSink.MaxRequiredInput = this.m_maxRequiredPowerInput;
            this.ResourceSink.Update();
            this.UpdateAutomaticDeactivation();
        }

        public void Register(MyGyro gyro)
        {
            this.m_gyros.Add(gyro);
            this.m_gyrosChanged = true;
            gyro.EnabledChanged += new Action<MyTerminalBlock>(this.gyro_EnabledChanged);
            gyro.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            gyro.PropertiesChanged += new Action<MyTerminalBlock>(this.gyro_PropertiesChanged);
        }

        public void Unregister(MyGyro gyro)
        {
            this.m_gyros.Remove(gyro);
            this.m_gyrosChanged = true;
            gyro.EnabledChanged -= new Action<MyTerminalBlock>(this.gyro_EnabledChanged);
            gyro.SlimBlock.ComponentStack.IsFunctionalChanged -= new Action(this.ComponentStack_IsFunctionalChanged);
        }

        private void UpdateAutomaticDeactivation()
        {
            if ((this.m_grid.Physics != null) && !this.m_grid.Physics.RigidBody.IsFixed)
            {
                if (Vector3.IsZero(this.m_overrideTargetVelocity) || !this.ResourceSink.IsPowered)
                {
                    this.m_grid.Physics.RigidBody.EnableDeactivation = true;
                }
                else
                {
                    this.m_grid.Physics.RigidBody.EnableDeactivation = false;
                }
            }
        }

        public void UpdateBeforeSimulation()
        {
            MySimpleProfiler.Begin("Gyro", MySimpleProfiler.ProfilingBlockType.BLOCK, "UpdateBeforeSimulation");
            if (this.m_gyrosChanged)
            {
                this.RecomputeGyroParameters();
            }
            if (this.m_maxOverrideForce == 0f)
            {
                if (MyDebugDrawSettings.DEBUG_DRAW_GYROS)
                {
                    MyRenderProxy.DebugDrawText2D(new Vector2(0f, 0f), "Old gyros", Color.White, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                }
                this.UpdateGyros();
                MySimpleProfiler.End("UpdateBeforeSimulation");
            }
            else
            {
                if (MyDebugDrawSettings.DEBUG_DRAW_GYROS)
                {
                    MyRenderProxy.DebugDrawText2D(new Vector2(0f, 0f), "New gyros", Color.White, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                }
                if (this.m_grid.Physics != null)
                {
                    this.UpdateOverriddenGyros();
                }
                MySimpleProfiler.End("UpdateBeforeSimulation");
            }
        }

        private void UpdateGyros()
        {
            this.SlowdownTorque = Vector3.Zero;
            MyCubeGrid localGrid = this.m_grid;
            MyGridPhysics physics = localGrid.Physics;
            if ((physics != null) && !physics.IsKinematic)
            {
                if (!this.ControlTorque.IsValid())
                {
                    this.ControlTorque = Vector3.Zero;
                }
                if ((!Vector3.IsZero(physics.AngularVelocity, 0.001f) || !Vector3.IsZero(this.ControlTorque, 0.001f)) && (((this.ResourceSink.SuppliedRatio > 0f) && (physics.Enabled || physics.IsWelded)) && !physics.RigidBody.IsFixed))
                {
                    Vector3? nullable;
                    Vector3D? nullable2;
                    float? nullable3;
                    Matrix inverseInertiaTensor = physics.RigidBody.InverseInertiaTensor;
                    inverseInertiaTensor.M44 = 1f;
                    Matrix orientation = (Matrix) localGrid.PositionComp.WorldMatrixNormalizedInv.GetOrientation();
                    Vector3 zero = Vector3.Transform(physics.AngularVelocity, ref orientation);
                    float num = ((1f - MAX_SLOWDOWN) * (1f - this.ResourceSink.SuppliedRatio)) + MAX_SLOWDOWN;
                    this.SlowdownTorque = -zero;
                    float num2 = (localGrid.GridSizeEnum == MyCubeSize.Large) ? MyFakes.SLOWDOWN_FACTOR_TORQUE_MULTIPLIER_LARGE_SHIP : MyFakes.SLOWDOWN_FACTOR_TORQUE_MULTIPLIER;
                    Vector3 max = new Vector3(this.m_maxGyroForce * num2);
                    if (physics.IsWelded)
                    {
                        this.SlowdownTorque = Vector3.TransformNormal(this.SlowdownTorque, localGrid.WorldMatrix);
                        this.SlowdownTorque = Vector3.TransformNormal(this.SlowdownTorque, Matrix.Invert(physics.RigidBody.GetRigidBodyMatrix()));
                    }
                    if (!zero.IsValid())
                    {
                        zero = Vector3.Zero;
                    }
                    Vector3 vector4 = Vector3.One - Vector3.IsZeroVector(Vector3.Sign(zero) - Vector3.Sign(this.ControlTorque));
                    this.SlowdownTorque *= num2;
                    this.SlowdownTorque /= inverseInertiaTensor.Scale;
                    this.SlowdownTorque = Vector3.Clamp(this.SlowdownTorque, -max, max) * vector4;
                    if (this.SlowdownTorque.LengthSquared() > 0.0001f)
                    {
                        nullable = null;
                        nullable2 = null;
                        nullable3 = null;
                        physics.AddForce(MyPhysicsForceType.ADD_BODY_FORCE_AND_BODY_TORQUE, nullable, nullable2, new Vector3?(this.SlowdownTorque * num), nullable3, true, false);
                    }
                    Matrix inertiaTensor = MyGridPhysicalGroupData.GetGroupSharedProperties(localGrid, true).InertiaTensor;
                    float num4 = Math.Max((float) 1f, (float) ((1f / Math.Max(Math.Max(inertiaTensor.M11, inertiaTensor.M22), inertiaTensor.M33)) * INV_TENSOR_MAX_LIMIT));
                    this.Torque = (Vector3.Clamp(this.ControlTorque, -Vector3.One, Vector3.One) * this.m_maxGyroForce) / num4;
                    this.Torque *= this.ResourceSink.SuppliedRatio;
                    Vector3 scale = physics.RigidBody.InertiaTensor.Scale;
                    scale = Vector3.Abs(scale / scale.AbsMax());
                    if (this.Torque.LengthSquared() > 0.0001f)
                    {
                        Vector3 torque = this.Torque;
                        if (physics.IsWelded)
                        {
                            torque = Vector3.TransformNormal(Vector3.TransformNormal(torque, localGrid.WorldMatrix), Matrix.Invert(physics.RigidBody.GetRigidBodyMatrix()));
                        }
                        nullable = null;
                        nullable2 = null;
                        nullable3 = null;
                        physics.AddForce(MyPhysicsForceType.ADD_BODY_FORCE_AND_BODY_TORQUE, nullable, nullable2, new Vector3?(torque * scale), nullable3, true, false);
                    }
                    if (((this.ControlTorque == Vector3.Zero) && ((physics.AngularVelocity != Vector3.Zero) && (physics.AngularVelocity.LengthSquared() < 9.000001E-08f))) && physics.RigidBody.IsActive)
                    {
                        physics.AngularVelocity = Vector3.Zero;
                    }
                }
            }
        }

        private void UpdateOverriddenGyros()
        {
            if (((this.ResourceSink.SuppliedRatio > 0f) && this.m_grid.Physics.Enabled) && !this.m_grid.Physics.RigidBody.IsFixed)
            {
                Matrix orientation = (Matrix) this.m_grid.PositionComp.WorldMatrixInvScaled.GetOrientation();
                this.m_grid.WorldMatrix.GetOrientation();
                Vector3 vector = Vector3.Transform(this.m_grid.Physics.AngularVelocity, ref orientation);
                this.Torque = Vector3.Zero;
                Vector3 velocityDiff = this.m_overrideTargetVelocity - vector;
                if (velocityDiff != Vector3.Zero)
                {
                    this.UpdateOverrideAccelerationRampFrames(velocityDiff);
                    Matrix inverseInertiaTensor = this.m_grid.Physics.RigidBody.InverseInertiaTensor;
                    Vector3 vector3 = new Vector3(inverseInertiaTensor.M11, inverseInertiaTensor.M22, inverseInertiaTensor.M33);
                    Vector3 vector4 = Vector3.ClampToSphere((velocityDiff * (60f / ((float) this.m_overrideAccelerationRampFrames.Value))) / vector3, this.m_maxOverrideForce + (this.m_maxGyroForce * (1f - this.ControlTorque.Length())));
                    this.Torque = (this.ControlTorque * this.m_maxGyroForce) + vector4;
                    this.Torque *= this.ResourceSink.SuppliedRatio;
                    if (this.Torque.LengthSquared() >= 0.0001f)
                    {
                        this.m_grid.MarkForUpdate();
                        Vector3? force = null;
                        Vector3D? position = null;
                        float? maxSpeed = null;
                        this.m_grid.Physics.AddForce(MyPhysicsForceType.ADD_BODY_FORCE_AND_BODY_TORQUE, force, position, new Vector3?(this.Torque), maxSpeed, true, false);
                    }
                }
            }
        }

        private void UpdateOverrideAccelerationRampFrames(Vector3 velocityDiff)
        {
            if (this.m_overrideAccelerationRampFrames == null)
            {
                float num = velocityDiff.LengthSquared();
                if (num > 2.467401f)
                {
                    this.m_overrideAccelerationRampFrames = 120;
                }
                else
                {
                    this.m_overrideAccelerationRampFrames = new int?(((int) (num * 48.22889f)) + 1);
                }
            }
            else
            {
                int? overrideAccelerationRampFrames = this.m_overrideAccelerationRampFrames;
                int num2 = 1;
                if ((overrideAccelerationRampFrames.GetValueOrDefault() > num2) & (overrideAccelerationRampFrames != null))
                {
                    int? nullable1;
                    overrideAccelerationRampFrames = this.m_overrideAccelerationRampFrames;
                    if (overrideAccelerationRampFrames != null)
                    {
                        nullable1 = new int?(overrideAccelerationRampFrames.GetValueOrDefault() - 1);
                    }
                    else
                    {
                        nullable1 = null;
                    }
                    this.m_overrideAccelerationRampFrames = nullable1;
                }
            }
        }

        public Vector3 ControlTorque
        {
            get => 
                this.m_controlTorque;
            set
            {
                if (this.m_controlTorque != value)
                {
                    this.m_controlTorque = value;
                    this.m_grid.MarkForUpdate();
                }
            }
        }

        public bool HasOverrideInput =>
            (!Vector3.IsZero(ref this.m_controlTorque) || !Vector3.IsZero(ref this.m_overrideTargetVelocity));

        public bool IsDirty =>
            this.m_gyrosChanged;

        public MyResourceSinkComponent ResourceSink { get; private set; }

        public int GyroCount =>
            this.m_gyros.Count;

        public HashSet<MyGyro> Gyros =>
            this.m_gyros;

        public Vector3 Torque { get; private set; }

        public bool NeedsPerFrameUpdate =>
            (((this.ControlTorque != Vector3.Zero) || ((this.m_grid.Physics != null) && (this.m_grid.Physics.AngularVelocity.LengthSquared() > 1E-05f))) || ((this.Torque.LengthSquared() >= 0.0001f) || ((this.SlowdownTorque.LengthSquared() > 0.0001f) || (this.m_overrideTargetVelocity.LengthSquared() > 0.0001f))));
    }
}

