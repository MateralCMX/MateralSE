namespace Sandbox.Game.AI.Navigation
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.AI.Pathfinding;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRageMath;

    public class MyPathSteering : MyTargetSteering
    {
        private IMyPath m_path;
        private float m_weight;
        private const float END_RADIUS = 0.5f;
        private const float DISTANCE_FOR_FINAL_APPROACH = 2f;

        public MyPathSteering(MyBotNavigation navigation) : base(navigation)
        {
        }

        public override void Cleanup()
        {
            base.Cleanup();
            if ((this.m_path != null) && this.m_path.IsValid)
            {
                this.m_path.Invalidate();
            }
        }

        public override void DebugDraw()
        {
            if (((this.m_path != null) && this.m_path.IsValid) && (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyFakes.DEBUG_DRAW_FOUND_PATH))
            {
                this.m_path.DebugDraw();
            }
        }

        public override string GetName() => 
            "Path steering";

        private void SetNextTarget()
        {
            Vector3D? targetWorld = base.TargetWorld;
            if ((this.m_path == null) || !this.m_path.IsValid)
            {
                base.UnsetTarget();
            }
            else
            {
                Vector3D closestPoint = this.m_path.Destination.GetClosestPoint(base.CapsuleCenter());
                double num = base.TargetDistanceSq(ref closestPoint);
                if (num > 0.25)
                {
                    float num2;
                    Vector3D vectord2;
                    IMyEntity entity2;
                    Vector3D translation = base.Parent.PositionAndOrientation.Translation;
                    if (this.m_path.PathCompleted)
                    {
                        if (num < 4.0)
                        {
                            this.UnsetPath();
                            base.SetTarget(closestPoint, 0.5f, this.m_path.EndEntity as MyEntity, this.m_weight, false);
                            return;
                        }
                        if (targetWorld != null)
                        {
                            this.m_path.Reinit(targetWorld.Value);
                        }
                        else
                        {
                            this.m_path.Reinit(translation);
                        }
                    }
                    bool flag1 = this.m_path.GetNextTarget(base.Parent.PositionAndOrientation.Translation, out vectord2, out num2, out entity2);
                    MyEntity relativeEntity = entity2 as MyEntity;
                    if (flag1)
                    {
                        base.SetTarget(vectord2, num2, relativeEntity, this.m_weight, false);
                        return;
                    }
                }
                this.UnsetPath();
            }
        }

        public void SetPath(IMyPath path, float weight = 1f)
        {
            if ((path == null) || !path.IsValid)
            {
                this.UnsetPath();
            }
            else
            {
                if (this.m_path != null)
                {
                    this.m_path.Invalidate();
                }
                this.m_path = path;
                this.m_weight = weight;
                this.PathFinished = false;
                this.SetNextTarget();
            }
        }

        public void UnsetPath()
        {
            if (this.m_path != null)
            {
                this.m_path.Invalidate();
            }
            this.m_path = null;
            base.UnsetTarget();
            this.PathFinished = true;
        }

        public override void Update()
        {
            if (this.m_path == null)
            {
                base.Update();
            }
            else if (!this.m_path.IsValid)
            {
                this.UnsetPath();
            }
            else if (base.TargetReached())
            {
                this.SetNextTarget();
            }
        }

        public bool PathFinished { get; private set; }
    }
}

