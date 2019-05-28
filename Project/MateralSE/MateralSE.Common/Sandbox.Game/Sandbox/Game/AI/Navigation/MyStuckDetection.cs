namespace Sandbox.Game.AI.Navigation
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyStuckDetection
    {
        private static readonly int STUCK_COUNTDOWN = 60;
        private static readonly int LONGTERM_COUNTDOWN = 300;
        private static readonly double LONGTERM_TOLERANCE = 0.025;
        private Vector3D m_translationStuckDetection;
        private Vector3D m_longTermTranslationStuckDetection;
        private Vector3 m_rotationStuckDetection;
        private float m_positionToleranceSq;
        private float m_rotationToleranceSq;
        private double m_previousDistanceFromTarget;
        private bool m_isRotating;
        private int m_counter;
        private int m_longTermCounter;
        private int m_tickCounter;
        private int m_stoppedTime;
        private BoundingBoxD m_boundingBox;

        public MyStuckDetection(float positionTolerance, float rotationTolerance)
        {
            this.m_positionToleranceSq = positionTolerance * positionTolerance;
            this.m_rotationToleranceSq = rotationTolerance * rotationTolerance;
            this.Reset(false);
        }

        public MyStuckDetection(float positionTolerance, float rotationTolerance, BoundingBoxD box) : this(positionTolerance, rotationTolerance)
        {
            this.m_boundingBox = box;
        }

        public void Reset(bool force = false)
        {
            if (force || (this.m_stoppedTime != this.m_tickCounter))
            {
                this.m_translationStuckDetection = Vector3D.Zero;
                this.m_rotationStuckDetection = Vector3.Zero;
                this.IsStuck = false;
                this.m_counter = STUCK_COUNTDOWN;
                this.m_longTermCounter = LONGTERM_COUNTDOWN;
                this.m_isRotating = false;
            }
        }

        public void SetCurrentTicks(int behaviorTicks)
        {
            this.m_tickCounter = behaviorTicks;
        }

        public void SetRotating(bool rotating)
        {
            this.m_isRotating = rotating;
        }

        public void Stop()
        {
            this.m_stoppedTime = this.m_tickCounter;
        }

        public void Update(Vector3D worldPosition, Vector3 rotation, Vector3D targetLocation = new Vector3D())
        {
            int num1;
            this.m_translationStuckDetection = (this.m_translationStuckDetection * 0.8) + (worldPosition * 0.2);
            this.m_rotationStuckDetection = (this.m_rotationStuckDetection * 0.95f) + (rotation * 0.05f);
            if (((this.m_translationStuckDetection - worldPosition).LengthSquared() >= this.m_positionToleranceSq) || ((this.m_rotationStuckDetection - rotation).LengthSquared() >= this.m_rotationToleranceSq))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) !this.m_isRotating;
            }
            bool flag = (bool) num1;
            double num = (worldPosition - targetLocation).Length();
            if (((targetLocation != Vector3D.Zero) && !flag) && (num < (2.0 * this.m_boundingBox.Extents.Min())))
            {
                if (Math.Abs((double) (this.m_previousDistanceFromTarget - num)) > 1.0)
                {
                    this.m_previousDistanceFromTarget = num + 1.0;
                }
                this.m_previousDistanceFromTarget = (this.m_previousDistanceFromTarget * 0.7) + (num * 0.3);
                flag = Math.Abs((double) (num - this.m_previousDistanceFromTarget)) < this.m_positionToleranceSq;
            }
            if (this.m_counter > 0)
            {
                if ((this.m_counter == STUCK_COUNTDOWN) && !flag)
                {
                    this.IsStuck = false;
                    return;
                }
                this.m_counter--;
            }
            else if (flag)
            {
                this.IsStuck = true;
            }
            else
            {
                this.m_counter = STUCK_COUNTDOWN;
            }
            if (this.m_longTermCounter > 0)
            {
                this.m_longTermCounter--;
            }
            else if ((this.m_longTermTranslationStuckDetection - worldPosition).LengthSquared() < LONGTERM_TOLERANCE)
            {
                this.IsStuck = true;
            }
            else
            {
                this.m_longTermCounter = LONGTERM_COUNTDOWN;
                this.m_longTermTranslationStuckDetection = worldPosition;
            }
        }

        public bool IsStuck { get; private set; }
    }
}

