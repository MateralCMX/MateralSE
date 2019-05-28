namespace Sandbox.Engine.Physics
{
    using Sandbox;
    using Sandbox.Game.Entities;
    using System;
    using System.Threading;
    using VRage.Game.Entity;
    using VRage.Groups;

    public class MyWeldingGroups : MyGroups<MyEntity, MyWeldGroupData>, IMySceneComponent
    {
        private static MyWeldingGroups m_static;

        public MyWeldingGroups() : base(false, null)
        {
        }

        public override void CreateLink(long linkId, MyEntity parentNode, MyEntity childNode)
        {
            if (ReferenceEquals(MySandboxGame.Static.UpdateThread, Thread.CurrentThread))
            {
                base.CreateLink(linkId, parentNode, childNode);
            }
        }

        public bool IsEntityParent(MyEntity entity)
        {
            MyGroups<MyEntity, MyWeldGroupData>.Group group = base.GetGroup(entity);
            return ((group != null) ? ReferenceEquals(entity, group.GroupData.Parent) : true);
        }

        public void Load()
        {
            m_static = this;
            base.SupportsOphrans = true;
        }

        public static MyEntity ReplaceParent(MyGroups<MyEntity, MyWeldGroupData>.Group group, MyEntity oldParent, MyEntity newParent)
        {
            if ((oldParent != null) && (oldParent.Physics != null))
            {
                oldParent.GetPhysicsBody().UnweldAll(false);
            }
            else
            {
                if (group == null)
                {
                    return oldParent;
                }
                foreach (MyGroups<MyEntity, MyWeldGroupData>.Node node in group.Nodes)
                {
                    if (!node.NodeData.MarkedForClose)
                    {
                        node.NodeData.GetPhysicsBody().Unweld(false);
                    }
                }
            }
            if (group == null)
            {
                return oldParent;
            }
            if (newParent == null)
            {
                foreach (MyGroups<MyEntity, MyWeldGroupData>.Node node2 in group.Nodes)
                {
                    if (node2.NodeData.MarkedForClose)
                    {
                        continue;
                    }
                    if (node2.NodeData != oldParent)
                    {
                        if (node2.NodeData.Physics.IsStatic)
                        {
                            newParent = node2.NodeData;
                            break;
                        }
                        if (node2.NodeData.Physics.RigidBody2 != null)
                        {
                            newParent = node2.NodeData;
                        }
                    }
                }
            }
            foreach (MyGroups<MyEntity, MyWeldGroupData>.Node node3 in group.Nodes)
            {
                if (node3.NodeData.MarkedForClose)
                {
                    continue;
                }
                if (newParent != node3.NodeData)
                {
                    if (newParent == null)
                    {
                        newParent = node3.NodeData;
                        continue;
                    }
                    newParent.GetPhysicsBody().Weld(node3.NodeData.Physics, false);
                }
            }
            if ((newParent != null) && !newParent.Physics.IsInWorld)
            {
                newParent.Physics.Activate();
            }
            return newParent;
        }

        public void Unload()
        {
            m_static = null;
        }

        public static MyWeldingGroups Static =>
            m_static;
    }
}

