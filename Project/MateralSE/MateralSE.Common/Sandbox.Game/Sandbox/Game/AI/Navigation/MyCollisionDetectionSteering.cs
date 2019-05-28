namespace Sandbox.Game.AI.Navigation
{
    using Sandbox.Engine.Physics;
    using System;
    using System.Collections.Generic;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyCollisionDetectionSteering : MySteeringBase
    {
        private bool m_hitLeft;
        private bool m_hitRight;
        private float m_hitLeftFraction;
        private float m_hitRightFraction;

        public MyCollisionDetectionSteering(MyBotNavigation parent) : base(parent, 1f)
        {
        }

        public override void AccumulateCorrection(ref Vector3 correction, ref float weight)
        {
            this.m_hitLeft = false;
            this.m_hitRight = false;
            MatrixD positionAndOrientation = base.Parent.PositionAndOrientation;
            Vector3 forwardVector = base.Parent.ForwardVector;
            Vector3 vector2 = Vector3.Cross((Vector3) positionAndOrientation.Up, forwardVector);
            List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
            MyPhysics.CastRay(positionAndOrientation.Translation + positionAndOrientation.Up, ((positionAndOrientation.Translation + positionAndOrientation.Up) + (forwardVector * 0.1f)) + (vector2 * 1.3f), toList, 0);
            if (toList.Count > 0)
            {
                this.m_hitLeft = true;
                this.m_hitLeftFraction = toList[0].HkHitInfo.HitFraction;
            }
            toList.Clear();
            MyPhysics.CastRay(positionAndOrientation.Translation + positionAndOrientation.Up, ((positionAndOrientation.Translation + positionAndOrientation.Up) + (forwardVector * 0.1f)) - (vector2 * 1.3f), toList, 0);
            if (toList.Count > 0)
            {
                this.m_hitRight = true;
                this.m_hitRightFraction = toList[0].HkHitInfo.HitFraction;
            }
            toList.Clear();
            float num = (base.Weight * 0.01f) * (1f - this.m_hitLeftFraction);
            float num2 = (base.Weight * 0.01f) * (1f - this.m_hitRightFraction);
            if (this.m_hitLeft)
            {
                correction -= vector2 * num;
                weight += num;
            }
            if (this.m_hitRight)
            {
                correction += vector2 * num2;
                weight += num2;
            }
            if (this.m_hitLeft && this.m_hitRight)
            {
                correction -= vector2;
                weight += num;
            }
        }

        public override void DebugDraw()
        {
            MatrixD positionAndOrientation = base.Parent.PositionAndOrientation;
            Vector3 forwardVector = base.Parent.ForwardVector;
            Vector3 vector2 = Vector3.Cross((Vector3) positionAndOrientation.Up, forwardVector);
            Color colorFrom = this.m_hitLeft ? Color.Orange : Color.Green;
            MyRenderProxy.DebugDrawLine3D(positionAndOrientation.Translation + positionAndOrientation.Up, ((positionAndOrientation.Translation + positionAndOrientation.Up) + (forwardVector * 0.1f)) + (vector2 * 1.3f), colorFrom, colorFrom, true, false);
            MyRenderProxy.DebugDrawText3D(positionAndOrientation.Translation + (positionAndOrientation.Up * 3.0), "Hit LT: " + this.m_hitLeftFraction.ToString(), colorFrom, 0.7f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            colorFrom = this.m_hitRight ? Color.Orange : Color.Green;
            MyRenderProxy.DebugDrawLine3D(positionAndOrientation.Translation + positionAndOrientation.Up, ((positionAndOrientation.Translation + positionAndOrientation.Up) + (forwardVector * 0.1f)) - (vector2 * 1.3f), colorFrom, colorFrom, true, false);
            MyRenderProxy.DebugDrawText3D(positionAndOrientation.Translation + (positionAndOrientation.Up * 3.2000000476837158), "Hit RT: " + this.m_hitRightFraction.ToString(), colorFrom, 0.7f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
        }

        public override string GetName() => 
            "Collision detection steering";

        public override void Update()
        {
            base.Update();
        }
    }
}

