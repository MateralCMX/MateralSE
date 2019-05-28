namespace Sandbox.Game.AI.Navigation
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRageMath;
    using VRageRender;

    public class MyTargetSteering : MySteeringBase
    {
        protected Vector3D? m_target;
        protected MyEntity m_entity;
        private const float m_slowdownRadius = 4f;
        private const float m_maxSpeed = 1f;
        private float m_capsuleRadiusSq;
        private float m_capsuleHeight;
        private float m_capsuleOffset;

        public MyTargetSteering(MyBotNavigation navigation) : base(navigation, 1f)
        {
            this.m_capsuleRadiusSq = 1f;
            this.m_capsuleHeight = 0.5f;
            this.m_capsuleOffset = -0.8f;
            this.m_target = null;
        }

        public override void AccumulateCorrection(ref Vector3 correctionHint, ref float weight)
        {
            Vector3 vector;
            Vector3 vector2;
            if ((this.m_entity != null) && this.m_entity.MarkedForClose)
            {
                this.m_entity = null;
            }
            this.GetMovements(out vector, out vector2);
            correctionHint += (vector2 - vector) * base.Weight;
            weight += base.Weight;
        }

        protected Vector3D CapsuleCenter()
        {
            Vector3D up = base.Parent.PositionAndOrientation.Up;
            return (base.Parent.PositionAndOrientation.Translation + ((up * (this.m_capsuleOffset + this.m_capsuleHeight)) * 0.5));
        }

        public override void DebugDraw()
        {
            Vector3 vector;
            Vector3 vector2;
            Vector3D vectord = base.Parent.PositionAndOrientation.Translation + (base.Parent.PositionAndOrientation.Up * this.m_capsuleOffset);
            Vector3D vectord2 = vectord + (base.Parent.PositionAndOrientation.Up * this.m_capsuleHeight);
            Vector3D pointFrom = (vectord + vectord2) * 0.5;
            this.GetMovements(out vector, out vector2);
            Vector3D? targetWorld = this.TargetWorld;
            if (targetWorld != null)
            {
                MyRenderProxy.DebugDrawLine3D(pointFrom, targetWorld.Value, Color.White, Color.White, true, false);
                MyRenderProxy.DebugDrawSphere(targetWorld.Value, 0.05f, Color.White.ToVector3(), 1f, false, false, true, false);
                MyRenderProxy.DebugDrawCapsule(vectord, vectord2, (float) Math.Sqrt((double) this.m_capsuleRadiusSq), Color.Yellow, false, false, false);
            }
            MyRenderProxy.DebugDrawLine3D(vectord2, vectord2 + vector2, Color.Red, Color.Red, false, false);
            MyRenderProxy.DebugDrawLine3D(vectord2, vectord2 + vector, Color.Green, Color.Green, false, false);
        }

        private void GetMovements(out Vector3 currentMovement, out Vector3 wantedMovement)
        {
            Vector3? nullable1;
            Vector3D? targetWorld = this.TargetWorld;
            if (targetWorld != null)
            {
                nullable1 = new Vector3?(targetWorld.GetValueOrDefault());
            }
            else
            {
                nullable1 = null;
            }
            Vector3? nullable = nullable1;
            currentMovement = base.Parent.ForwardVector * base.Parent.Speed;
            if (nullable == null)
            {
                wantedMovement = Vector3.Zero;
            }
            else
            {
                wantedMovement = nullable.Value - base.Parent.PositionAndOrientation.Translation;
                float num = wantedMovement.Length();
                if (num > 4f)
                {
                    wantedMovement = (wantedMovement * 1f) / num;
                }
                else
                {
                    wantedMovement = (wantedMovement * 1f) / 4f;
                }
            }
        }

        public override string GetName() => 
            "Target steering";

        public void SetTarget(Vector3D target, float radius = 1f, MyEntity relativeEntity = null, float weight = 1f, bool fly = false)
        {
            if ((relativeEntity == null) || relativeEntity.MarkedForClose)
            {
                this.m_entity = null;
                this.m_target = new Vector3D?(target);
            }
            else
            {
                this.m_entity = relativeEntity;
                this.m_target = new Vector3D?(Vector3D.Transform(target, this.m_entity.PositionComp.WorldMatrixNormalizedInv));
            }
            this.m_capsuleRadiusSq = radius * radius;
            base.Weight = weight;
            this.Flying = fly;
        }

        public double TargetDistanceSq(ref Vector3D target)
        {
            double num;
            double num2;
            double num3;
            Vector3D up = base.Parent.PositionAndOrientation.Up;
            Vector3D vectord2 = base.Parent.PositionAndOrientation.Translation + (up * this.m_capsuleOffset);
            Vector3D.Dot(ref vectord2, ref up, out num);
            Vector3D.Dot(ref target, ref up, out num2);
            num2 -= num;
            if (num2 >= this.m_capsuleHeight)
            {
                vectord2 += up;
            }
            else if (num2 >= 0.0)
            {
                vectord2 += up * num2;
            }
            Vector3D.DistanceSquared(ref target, ref vectord2, out num3);
            return num3;
        }

        public bool TargetReached()
        {
            if (this.TargetWorld == null)
            {
                return false;
            }
            Vector3D target = this.TargetWorld.Value;
            return this.TargetReached(ref target, this.m_capsuleRadiusSq);
        }

        public bool TargetReached(ref Vector3D target, float radiusSq) => 
            (this.TargetDistanceSq(ref target) < radiusSq);

        public void UnsetTarget()
        {
            this.m_target = null;
        }

        public override void Update()
        {
            base.Update();
            if (this.TargetReached())
            {
                this.UnsetTarget();
            }
        }

        public bool TargetSet =>
            (this.m_target != null);

        public bool Flying { get; private set; }

        public Vector3D? TargetWorld
        {
            get
            {
                if ((this.m_entity == null) || this.m_entity.MarkedForClose)
                {
                    return this.m_target;
                }
                if (this.m_target != null)
                {
                    return new Vector3D?(Vector3D.Transform(this.m_target.Value, this.m_entity.WorldMatrix));
                }
                return null;
            }
        }
    }
}

