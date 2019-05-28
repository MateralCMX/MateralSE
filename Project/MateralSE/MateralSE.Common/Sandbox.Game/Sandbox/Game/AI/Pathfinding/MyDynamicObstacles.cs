namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using VRage.Collections;
    using VRage.Game.Entity;
    using VRageMath;

    public class MyDynamicObstacles
    {
        private CachingList<IMyObstacle> m_obstacles = new CachingList<IMyObstacle>();

        public void Clear()
        {
            this.m_obstacles.ClearImmediate();
        }

        public void DebugDraw()
        {
            using (List<IMyObstacle>.Enumerator enumerator = this.m_obstacles.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.DebugDraw();
                }
            }
        }

        public bool IsInObstacle(Vector3D point)
        {
            using (List<IMyObstacle>.Enumerator enumerator = this.m_obstacles.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (enumerator.Current.Contains(ref point))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void TryCreateObstacle(MyEntity newEntity)
        {
            if (((newEntity.Physics != null) && (newEntity is MyCubeGrid)) && (newEntity.PositionComp != null))
            {
                IMyObstacle entity = MyObstacleFactory.CreateObstacleForEntity(newEntity);
                if (entity != null)
                {
                    this.m_obstacles.Add(entity);
                }
            }
        }

        public void Update()
        {
            using (List<IMyObstacle>.Enumerator enumerator = this.m_obstacles.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Update();
                }
            }
            this.m_obstacles.ApplyChanges();
        }
    }
}

