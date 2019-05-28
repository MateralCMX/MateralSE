namespace Sandbox.Engine.Utils
{
    using Sandbox.Engine.Physics;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Lights;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Utils;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    public class MySpectatorCameraController : MySpectator, IMyCameraController
    {
        private const int REFLECTOR_RANGE_MULTIPLIER = 5;
        public static MySpectatorCameraController Static;
        private float m_orbitY;
        private float m_orbitX;
        private Vector3D ThirdPersonCameraOrbit = (Vector3D.UnitZ * 10.0);
        private CyclingOptions m_cycling;
        private float m_cyclingMetricValue = float.MinValue;
        private long m_entityID;
        private MyEntity m_character;
        private double m_yaw;
        private double m_pitch;
        private double m_roll;
        private Vector3D m_lastRightVec = Vector3D.Right;
        private Vector3D m_lastUpVec = Vector3D.Up;
        private MatrixD m_lastOrientation = MatrixD.Identity;
        private float m_lastOrientationWeight = 1f;
        private MyLight m_light;
        private Vector3 m_lightLocalPosition;
        private Matrix m_reflectorAngleMatrix;
        private Vector3D m_velocity;

        public MySpectatorCameraController()
        {
            Static = this;
        }

        public void CleanLight()
        {
            if (this.m_light != null)
            {
                MyLights.RemoveLight(this.m_light);
                this.m_light = null;
            }
        }

        private void ComputeGravityAlignedOrientation(out MatrixD resultOrientationStorage)
        {
            Vector3D vectord3;
            bool flag = true;
            Vector3D up = -MyGravityProviderSystem.CalculateTotalGravityInPoint(base.Position);
            if (up.LengthSquared() >= 9.9999997473787516E-06)
            {
                this.m_lastUpVec = up;
            }
            else
            {
                up = this.m_lastUpVec;
                this.m_lastOrientationWeight = 1f;
                flag = false;
            }
            up.Normalize();
            Vector3D right = this.m_lastRightVec - (Vector3D.Dot(this.m_lastRightVec, up) * up);
            if (right.LengthSquared() < 9.9999997473787516E-06)
            {
                right = base.m_orientation.Right - (Vector3D.Dot(base.m_orientation.Right, up) * up);
                if (right.LengthSquared() < 9.9999997473787516E-06)
                {
                    right = base.m_orientation.Forward - (Vector3D.Dot(base.m_orientation.Forward, up) * up);
                }
            }
            right.Normalize();
            this.m_lastRightVec = right;
            Vector3D.Cross(ref up, ref right, out vectord3);
            resultOrientationStorage = MatrixD.Identity;
            resultOrientationStorage.Right = right;
            resultOrientationStorage.Up = up;
            resultOrientationStorage.Forward = vectord3;
            resultOrientationStorage = (MatrixD.CreateFromAxisAngle(Vector3D.Right, this.m_pitch) * resultOrientationStorage) * MatrixD.CreateFromAxisAngle(up, this.m_yaw);
            up = resultOrientationStorage.Up;
            right = resultOrientationStorage.Right;
            resultOrientationStorage.Right = (Vector3D) ((Math.Cos(this.m_roll) * right) + (Math.Sin(this.m_roll) * up));
            resultOrientationStorage.Up = (Vector3D) ((-Math.Sin(this.m_roll) * right) + (Math.Cos(this.m_roll) * up));
            if (flag && (this.m_lastOrientationWeight > 0f))
            {
                this.m_lastOrientationWeight = Math.Max((float) 0f, (float) (this.m_lastOrientationWeight - 0.01666667f));
                resultOrientationStorage = MatrixD.Slerp(resultOrientationStorage, this.m_lastOrientation, MathHelper.SmoothStepStable(this.m_lastOrientationWeight));
                resultOrientationStorage = MatrixD.Orthogonalize(resultOrientationStorage);
                resultOrientationStorage.Forward = Vector3D.Cross(resultOrientationStorage.Up, resultOrientationStorage.Right);
            }
            if (!flag)
            {
                this.m_lastOrientation = resultOrientationStorage;
            }
        }

        public void InitLight(bool isLightOn)
        {
            this.m_light = MyLights.AddLight();
            if (this.m_light != null)
            {
                this.m_light.Start("SpectatorCameraController");
                this.m_light.ReflectorOn = true;
                this.m_light.ReflectorTexture = @"Textures\Lights\dual_reflector_2.dds";
                this.m_light.Range = 2f;
                this.m_light.ReflectorRange = 35f;
                this.m_light.ReflectorColor = MyCharacter.REFLECTOR_COLOR;
                this.m_light.ReflectorIntensity = MyCharacter.REFLECTOR_INTENSITY;
                this.m_light.ReflectorGlossFactor = MyCharacter.REFLECTOR_GLOSS_FACTOR;
                this.m_light.ReflectorDiffuseFactor = MyCharacter.REFLECTOR_DIFFUSE_FACTOR;
                this.m_light.Color = MyCharacter.POINT_COLOR;
                this.m_light.Intensity = MyCharacter.POINT_LIGHT_INTENSITY;
                this.m_light.UpdateReflectorRangeAndAngle(0.373f, 175f);
                this.m_light.LightOn = isLightOn;
                this.m_light.ReflectorOn = isLightOn;
            }
        }

        public override void MoveAndRotate(Vector3 moveIndicator, Vector2 rotationIndicator, float rollIndicator)
        {
            this.UpdateVelocity();
            if (MyInput.Static.IsAnyCtrlKeyPressed())
            {
                if (MyInput.Static.PreviousMouseScrollWheelValue() < MyInput.Static.MouseScrollWheelValue())
                {
                    base.SpeedModeAngular = Math.Min((float) (base.SpeedModeAngular * 1.5f), (float) 6f);
                }
                else if (MyInput.Static.PreviousMouseScrollWheelValue() > MyInput.Static.MouseScrollWheelValue())
                {
                    base.SpeedModeAngular = Math.Max((float) (base.SpeedModeAngular / 1.5f), (float) 0.0001f);
                }
            }
            else if (MyInput.Static.IsAnyShiftKeyPressed() || MyInput.Static.IsAnyAltKeyPressed())
            {
                if (MyInput.Static.PreviousMouseScrollWheelValue() < MyInput.Static.MouseScrollWheelValue())
                {
                    base.SpeedModeLinear = Math.Min((float) (base.SpeedModeLinear * 1.5f), (float) 8000f);
                }
                else if (MyInput.Static.PreviousMouseScrollWheelValue() > MyInput.Static.MouseScrollWheelValue())
                {
                    base.SpeedModeLinear = Math.Max((float) (base.SpeedModeLinear / 1.5f), (float) 0.0001f);
                }
            }
            switch (base.SpectatorCameraMovement)
            {
                case MySpectatorCameraMovementEnum.UserControlled:
                    this.MoveAndRotate_UserControlled(moveIndicator, rotationIndicator, rollIndicator);
                    if (!this.IsLightOn)
                    {
                        break;
                    }
                    this.UpdateLightPosition();
                    return;

                case MySpectatorCameraMovementEnum.ConstantDelta:
                    this.MoveAndRotate_ConstantDelta(moveIndicator, rotationIndicator, rollIndicator);
                    if (!this.IsLightOn)
                    {
                        break;
                    }
                    this.UpdateLightPosition();
                    return;

                case MySpectatorCameraMovementEnum.FreeMouse:
                    this.MoveAndRotate_FreeMouse(moveIndicator, rotationIndicator, rollIndicator);
                    return;

                case MySpectatorCameraMovementEnum.None:
                    break;

                case MySpectatorCameraMovementEnum.Orbit:
                    base.MoveAndRotate(moveIndicator, rotationIndicator, rollIndicator);
                    break;

                default:
                    return;
            }
        }

        private void MoveAndRotate_ConstantDelta(Vector3 moveIndicator, Vector2 rotationIndicator, float rollIndicator)
        {
            MyEntity entity;
            this.m_cycling.Enabled = true;
            bool flag = false;
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.TOOLBAR_UP) && MySession.Static.IsUserAdmin(Sync.MyId))
            {
                MyEntityCycling.FindNext(MyEntityCyclingOrder.Characters, ref this.m_cyclingMetricValue, ref this.m_entityID, false, this.m_cycling);
                flag = true;
            }
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.TOOLBAR_DOWN) && MySession.Static.IsUserAdmin(Sync.MyId))
            {
                MyEntityCycling.FindNext(MyEntityCyclingOrder.Characters, ref this.m_cyclingMetricValue, ref this.m_entityID, true, this.m_cycling);
                flag = true;
            }
            if (!MyInput.Static.IsAnyCtrlKeyPressed() && !MyInput.Static.IsAnyShiftKeyPressed())
            {
                if (MyInput.Static.PreviousMouseScrollWheelValue() < MyInput.Static.MouseScrollWheelValue())
                {
                    this.ThirdPersonCameraOrbit /= 1.1000000238418579;
                }
                else if (MyInput.Static.PreviousMouseScrollWheelValue() > MyInput.Static.MouseScrollWheelValue())
                {
                    this.ThirdPersonCameraOrbit *= 1.1000000238418579;
                }
            }
            if (flag)
            {
                MyEntities.TryGetEntityById(this.m_entityID, out this.m_character, false);
            }
            MyEntities.TryGetEntityById(this.TrackedEntity, out entity, false);
            if (entity != null)
            {
                Vector3D position = entity.PositionComp.GetPosition();
                if (!this.AlignSpectatorToGravity)
                {
                    base.Position = position + (Vector3D.Normalize(base.Position - base.Target) * base.ThirdPersonCameraDelta.Length());
                    base.Target = position;
                }
                else
                {
                    MatrixD xd;
                    this.m_roll = 0.0;
                    this.m_yaw = 0.0;
                    this.m_pitch = 0.0;
                    this.ComputeGravityAlignedOrientation(out xd);
                    base.Position = position + Vector3D.Transform(base.ThirdPersonCameraDelta, xd);
                    base.Target = position;
                    base.m_orientation.Up = xd.Up;
                }
            }
            if ((MyInput.Static.IsAnyAltKeyPressed() && !MyInput.Static.IsAnyCtrlKeyPressed()) && !MyInput.Static.IsAnyShiftKeyPressed())
            {
                base.MoveAndRotate(moveIndicator, rotationIndicator, rollIndicator);
            }
        }

        private void MoveAndRotate_FreeMouse(Vector3 moveIndicator, Vector2 rotationIndicator, float rollIndicator)
        {
            if (((MyCubeBuilder.Static.CubeBuilderState.CurrentBlockDefinition != null) || MySessionComponentVoxelHand.Static.Enabled) || MyInput.Static.IsRightMousePressed())
            {
                this.MoveAndRotate_UserControlled(moveIndicator, rotationIndicator, rollIndicator);
            }
            else
            {
                this.MoveAndRotate_UserControlled(moveIndicator, Vector2.Zero, rollIndicator);
            }
        }

        private void MoveAndRotate_UserControlled(Vector3 moveIndicator, Vector2 rotationIndicator, float rollIndicator)
        {
            float num = 1.666667f;
            float num2 = 0.0025f * base.m_speedModeAngular;
            rollIndicator = MyInput.Static.GetDeveloperRoll();
            float angle = 0f;
            if (rollIndicator != 0f)
            {
                Vector3D vectord;
                Vector3D vectord2;
                angle = MathHelper.Clamp((float) ((rollIndicator * base.m_speedModeAngular) * 0.1f), (float) -0.02f, (float) 0.02f);
                MyUtils.VectorPlaneRotation(base.m_orientation.Up, base.m_orientation.Right, out vectord2, out vectord, angle);
                base.m_orientation.Right = vectord;
                base.m_orientation.Up = vectord2;
            }
            if (this.AlignSpectatorToGravity)
            {
                rotationIndicator.Rotate(this.m_roll);
                this.m_yaw -= rotationIndicator.Y * num2;
                this.m_pitch -= rotationIndicator.X * num2;
                this.m_roll -= angle;
                MathHelper.LimitRadians2PI(ref this.m_yaw);
                this.m_pitch = MathHelper.Clamp(this.m_pitch, -1.5707963267948966, 1.5707963267948966);
                MathHelper.LimitRadians2PI(ref this.m_roll);
                this.ComputeGravityAlignedOrientation(out base.m_orientation);
            }
            else
            {
                if (this.m_lastOrientationWeight < 1f)
                {
                    base.m_orientation = MatrixD.Orthogonalize(base.m_orientation);
                    base.m_orientation.Forward = Vector3D.Cross(base.m_orientation.Up, base.m_orientation.Right);
                }
                if (rotationIndicator.Y != 0f)
                {
                    Vector3D vectord3;
                    Vector3D vectord4;
                    MyUtils.VectorPlaneRotation(base.m_orientation.Right, base.m_orientation.Forward, out vectord3, out vectord4, -rotationIndicator.Y * num2);
                    base.m_orientation.Right = vectord3;
                    base.m_orientation.Forward = vectord4;
                }
                if (rotationIndicator.X != 0f)
                {
                    Vector3D vectord5;
                    Vector3D vectord6;
                    MyUtils.VectorPlaneRotation(base.m_orientation.Up, base.m_orientation.Forward, out vectord5, out vectord6, rotationIndicator.X * num2);
                    base.m_orientation.Up = vectord5;
                    base.m_orientation.Forward = vectord6;
                }
                this.m_lastOrientation = base.m_orientation;
                this.m_lastOrientationWeight = 1f;
                this.m_roll = 0.0;
                this.m_pitch = 0.0;
            }
            float num4 = (MyInput.Static.IsAnyShiftKeyPressed() ? 1f : 0.35f) * (MyInput.Static.IsAnyCtrlKeyPressed() ? 0.3f : 1f);
            moveIndicator *= num4 * base.SpeedModeLinear;
            Vector3 position = moveIndicator * num;
            base.Position += Vector3.Transform(position, base.m_orientation);
        }

        public void OnAssumeControl(IMyCameraController previousCameraController)
        {
        }

        protected override void OnChangingMode(MySpectatorCameraMovementEnum oldMode, MySpectatorCameraMovementEnum newMode)
        {
            if ((newMode == MySpectatorCameraMovementEnum.UserControlled) && (oldMode == MySpectatorCameraMovementEnum.ConstantDelta))
            {
                MatrixD xd;
                this.ComputeGravityAlignedOrientation(out xd);
                base.m_orientation.Up = xd.Up;
                base.m_orientation.Forward = Vector3D.Normalize(base.Target - base.Position);
                base.m_orientation.Right = Vector3D.Cross(base.m_orientation.Forward, base.m_orientation.Up);
                this.AlignSpectatorToGravity = false;
            }
        }

        public void OnReleaseControl(IMyCameraController newCameraController)
        {
            this.TurnLightOff();
        }

        public void SwitchLight()
        {
            if (this.m_light != null)
            {
                this.m_light.LightOn = !this.m_light.LightOn;
                this.m_light.ReflectorOn = !this.m_light.ReflectorOn;
                this.m_light.UpdateLight();
            }
        }

        public void TurnLightOff()
        {
            if (this.m_light != null)
            {
                this.m_light.LightOn = false;
                this.m_light.ReflectorOn = false;
                this.m_light.UpdateLight();
            }
        }

        public override void Update()
        {
            base.Update();
            base.Position += this.m_velocity * 0.01666666753590107;
        }

        private void UpdateLightPosition()
        {
            if (this.m_light != null)
            {
                MatrixD xd = MatrixD.CreateWorld(base.Position, base.m_orientation.Forward, base.m_orientation.Up);
                this.m_reflectorAngleMatrix = (Matrix) MatrixD.CreateFromAxisAngle(xd.Backward, (double) MathHelper.ToRadians(MyCharacter.REFLECTOR_DIRECTION));
                this.m_light.ReflectorDirection = Vector3.Transform((Vector3) xd.Forward, this.m_reflectorAngleMatrix);
                this.m_light.ReflectorUp = (Vector3) xd.Up;
                this.m_light.Position = base.Position;
                this.m_light.UpdateLight();
            }
        }

        private void UpdateVelocity()
        {
            if (MyInput.Static.IsAnyShiftKeyPressed())
            {
                if (MyInput.Static.IsMousePressed(MyMouseButtonsEnum.Middle))
                {
                    IMyEntity entity;
                    MyCamera mainCamera = MySector.MainCamera;
                    List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
                    MyPhysics.CastRay(base.Position, base.Position + (base.Orientation.Forward * 1000.0), toList, 0);
                    if (toList.Count > 0)
                    {
                        entity = toList[0].HkHitInfo.Body.GetEntity(toList[0].HkHitInfo.GetShapeKey(0));
                    }
                    else
                    {
                        entity = null;
                    }
                    this.m_velocity = (entity == null) ? Vector3D.Zero : entity.Physics.LinearVelocity;
                }
                if (MyInput.Static.IsMousePressed(MyMouseButtonsEnum.Right))
                {
                    this.m_velocity = Vector3D.Zero;
                }
                if (MyInput.Static.PreviousMouseScrollWheelValue() < MyInput.Static.MouseScrollWheelValue())
                {
                    this.m_velocity *= 1.1000000238418579;
                }
                else if (MyInput.Static.PreviousMouseScrollWheelValue() > MyInput.Static.MouseScrollWheelValue())
                {
                    this.m_velocity /= 1.1000000238418579;
                }
            }
        }

        void IMyCameraController.ControlCamera(MyCamera currentCamera)
        {
            currentCamera.SetViewMatrix(base.GetViewMatrix());
        }

        bool IMyCameraController.HandlePickUp() => 
            false;

        bool IMyCameraController.HandleUse() => 
            false;

        void IMyCameraController.OnAssumeControl(IMyCameraController previousCameraController)
        {
            this.OnAssumeControl(previousCameraController);
        }

        void IMyCameraController.OnReleaseControl(IMyCameraController newCameraController)
        {
            this.OnReleaseControl(newCameraController);
        }

        void IMyCameraController.Rotate(Vector2 rotationIndicator, float rollIndicator)
        {
            base.Rotate(rotationIndicator, rollIndicator);
        }

        void IMyCameraController.RotateStopped()
        {
            base.RotateStopped();
        }

        public bool IsLightOn =>
            ((this.m_light != null) && this.m_light.LightOn);

        public bool AlignSpectatorToGravity { get; set; }

        public long TrackedEntity { get; set; }

        public MyEntity Entity =>
            null;

        public Vector3D Velocity
        {
            get => 
                this.m_velocity;
            set => 
                (this.m_velocity = value);
        }

        bool IMyCameraController.IsInFirstPersonView
        {
            get => 
                base.IsInFirstPersonView;
            set => 
                (base.IsInFirstPersonView = value);
        }

        bool IMyCameraController.ForceFirstPersonCamera
        {
            get => 
                base.ForceFirstPersonCamera;
            set => 
                (base.ForceFirstPersonCamera = value);
        }

        bool IMyCameraController.EnableFirstPersonView
        {
            get => 
                true;
            set
            {
            }
        }

        bool IMyCameraController.AllowCubeBuilding =>
            true;
    }
}

