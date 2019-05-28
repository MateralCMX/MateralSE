namespace VRage
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Utils;
    using VRageMath;

    public class MySpectator
    {
        public static MySpectator Static;
        public const float DEFAULT_SPECTATOR_LINEAR_SPEED = 0.1f;
        public const float MIN_SPECTATOR_LINEAR_SPEED = 0.0001f;
        public const float MAX_SPECTATOR_LINEAR_SPEED = 8000f;
        public const float DEFAULT_SPECTATOR_ANGULAR_SPEED = 1f;
        public const float MIN_SPECTATOR_ANGULAR_SPEED = 0.0001f;
        public const float MAX_SPECTATOR_ANGULAR_SPEED = 6f;
        public Vector3D ThirdPersonCameraDelta = new Vector3D(-10.0, 10.0, -10.0);
        private MySpectatorCameraMovementEnum m_spectatorCameraMovement;
        private Vector3D m_position;
        private Vector3D m_targetDelta = Vector3D.Forward;
        private Vector3D? m_up;
        protected float m_speedModeLinear = 0.1f;
        protected float m_speedModeAngular = 1f;
        protected MatrixD m_orientation = MatrixD.Identity;
        protected bool m_orientationDirty = true;
        private float m_orbitY;
        private float m_orbitX;

        public MySpectator()
        {
            Static = this;
        }

        public MatrixD GetViewMatrix() => 
            MatrixD.Invert(MatrixD.CreateWorld(this.Position, this.Orientation.Forward, this.Orientation.Up));

        public virtual void MoveAndRotate(Vector3 moveIndicator, Vector2 rotationIndicator, float rollIndicator)
        {
            Vector3D position = this.Position;
            moveIndicator *= this.m_speedModeLinear;
            float num = 0.1f;
            float num2 = 0.0025f * this.m_speedModeAngular;
            Vector3D vectord = moveIndicator * num;
            switch (this.SpectatorCameraMovement)
            {
                case MySpectatorCameraMovementEnum.UserControlled:
                    if (rollIndicator != 0f)
                    {
                        Vector3D vectord2;
                        Vector3D vectord3;
                        MyUtils.VectorPlaneRotation(this.m_orientation.Up, this.m_orientation.Right, out vectord3, out vectord2, MathHelper.Clamp((float) ((rollIndicator * this.m_speedModeLinear) * 0.1f), (float) -0.02f, (float) 0.02f));
                        this.m_orientation.Right = vectord2;
                        this.m_orientation.Up = vectord3;
                    }
                    if (rotationIndicator.X != 0f)
                    {
                        Vector3D vectord4;
                        Vector3D vectord5;
                        MyUtils.VectorPlaneRotation(this.m_orientation.Up, this.m_orientation.Forward, out vectord4, out vectord5, rotationIndicator.X * num2);
                        this.m_orientation.Up = vectord4;
                        this.m_orientation.Forward = vectord5;
                    }
                    if (rotationIndicator.Y != 0f)
                    {
                        Vector3D vectord6;
                        Vector3D vectord7;
                        MyUtils.VectorPlaneRotation(this.m_orientation.Right, this.m_orientation.Forward, out vectord6, out vectord7, -rotationIndicator.Y * num2);
                        this.m_orientation.Right = vectord6;
                        this.m_orientation.Forward = vectord7;
                    }
                    this.Position += Vector3D.Transform(vectord, this.m_orientation);
                    return;

                case MySpectatorCameraMovementEnum.ConstantDelta:
                {
                    this.m_orbitY += rotationIndicator.Y * 0.01f;
                    this.m_orbitX += rotationIndicator.X * 0.01f;
                    Vector3D vectord13 = this.Position + this.m_targetDelta;
                    rotationIndicator *= 0.01f;
                    MatrixD matrix = (MatrixD.CreateRotationX((double) this.m_orbitX) * MatrixD.CreateRotationY((double) this.m_orbitY)) * MatrixD.CreateRotationZ((double) rollIndicator);
                    Vector3D vectord12 = Vector3D.Transform(Vector3D.Transform(-this.m_targetDelta, (MatrixD) Matrix.Invert((Matrix) this.Orientation)), matrix);
                    this.Position = vectord13 + vectord12;
                    this.m_targetDelta = -vectord12;
                    this.m_orientation = matrix;
                    break;
                }
                case MySpectatorCameraMovementEnum.FreeMouse:
                case MySpectatorCameraMovementEnum.None:
                    break;

                case MySpectatorCameraMovementEnum.Orbit:
                {
                    this.m_orbitY += rotationIndicator.Y * 0.01f;
                    this.m_orbitX += rotationIndicator.X * 0.01f;
                    Vector3D vectord9 = this.Position + this.m_targetDelta;
                    rotationIndicator *= 0.01f;
                    MatrixD matrix = (MatrixD.CreateRotationX((double) this.m_orbitX) * MatrixD.CreateRotationY((double) this.m_orbitY)) * MatrixD.CreateRotationZ((double) rollIndicator);
                    Vector3D vectord8 = Vector3D.Transform(Vector3D.Transform(-this.m_targetDelta, (MatrixD) Matrix.Invert((Matrix) this.Orientation)), matrix);
                    this.Position = vectord9 + vectord8;
                    this.m_targetDelta = -vectord8;
                    Vector3D vectord10 = (this.m_orientation.Right * vectord.X) + (this.m_orientation.Up * vectord.Y);
                    this.Position += vectord10;
                    Vector3D vectord11 = this.m_orientation.Forward * -vectord.Z;
                    this.Position += vectord11;
                    this.m_targetDelta -= vectord11;
                    this.m_orientation = matrix;
                    return;
                }
                default:
                    return;
            }
        }

        public virtual void MoveAndRotateStopped()
        {
        }

        protected virtual void OnChangingMode(MySpectatorCameraMovementEnum oldMode, MySpectatorCameraMovementEnum newMode)
        {
        }

        public void Reset()
        {
            this.m_position = Vector3.Zero;
            this.m_targetDelta = Vector3.Forward;
            this.ThirdPersonCameraDelta = new Vector3D(-10.0, 10.0, -10.0);
            this.m_orientationDirty = true;
            this.m_orbitX = 0f;
            this.m_orbitY = 0f;
        }

        public void Rotate(Vector2 rotationIndicator, float rollIndicator)
        {
            this.MoveAndRotate(Vector3.Zero, rotationIndicator, rollIndicator);
        }

        public void RotateStopped()
        {
            this.MoveAndRotateStopped();
        }

        public void SetTarget(Vector3D target, Vector3D? up)
        {
            this.Target = target;
            Vector3D? nullable = this.m_up;
            Vector3D? nullable2 = up;
            this.m_orientationDirty |= ((nullable != null) == (nullable2 != null)) ? ((nullable != null) ? (nullable.GetValueOrDefault() != nullable2.GetValueOrDefault()) : false) : true;
            this.m_up = up;
        }

        public void SetViewMatrix(MatrixD viewMatrix)
        {
            MatrixD xd = MatrixD.Invert(viewMatrix);
            this.Position = xd.Translation;
            this.m_orientation = MatrixD.Identity;
            this.m_orientation.Right = xd.Right;
            this.m_orientation.Up = xd.Up;
            this.m_orientation.Forward = xd.Forward;
            this.m_orientationDirty = false;
        }

        public virtual void Update()
        {
        }

        public void UpdateOrientation()
        {
            Vector3D dir = MyUtils.Normalize(this.m_targetDelta);
            dir = (dir.LengthSquared() > 0.0) ? dir : Vector3D.Forward;
            this.m_orientation = MatrixD.CreateFromDir(dir, (this.m_up != null) ? this.m_up.Value : Vector3D.Up);
        }

        public MySpectatorCameraMovementEnum SpectatorCameraMovement
        {
            get => 
                this.m_spectatorCameraMovement;
            set
            {
                if (this.m_spectatorCameraMovement != value)
                {
                    this.OnChangingMode(this.m_spectatorCameraMovement, value);
                }
                this.m_spectatorCameraMovement = value;
            }
        }

        public bool IsInFirstPersonView { get; set; }

        public bool ForceFirstPersonCamera { get; set; }

        public bool Initialized { get; set; }

        public Vector3D Position
        {
            get => 
                this.m_position;
            set => 
                (this.m_position = value);
        }

        public float SpeedModeLinear
        {
            get => 
                this.m_speedModeLinear;
            set => 
                (this.m_speedModeLinear = value);
        }

        public float SpeedModeAngular
        {
            get => 
                this.m_speedModeAngular;
            set => 
                (this.m_speedModeAngular = value);
        }

        public Vector3D Target
        {
            get => 
                (this.Position + this.m_targetDelta);
            set
            {
                Vector3D vectord = value - this.Position;
                this.m_orientationDirty = this.m_targetDelta != vectord;
                this.m_targetDelta = vectord;
                this.m_up = null;
            }
        }

        public MatrixD Orientation
        {
            get
            {
                if (this.m_orientationDirty)
                {
                    this.UpdateOrientation();
                    this.m_orientationDirty = false;
                }
                return this.m_orientation;
            }
        }
    }
}

