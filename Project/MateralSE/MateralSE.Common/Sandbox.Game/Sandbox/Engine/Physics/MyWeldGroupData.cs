namespace Sandbox.Engine.Physics
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Groups;

    public class MyWeldGroupData : IGroupData<MyEntity>
    {
        private MyGroups<MyEntity, MyWeldGroupData>.Group m_group;
        private MyEntity m_weldParent;

        public void OnCreate<TGroupData>(MyGroups<MyEntity, TGroupData>.Group group) where TGroupData: IGroupData<MyEntity>, new()
        {
            this.m_group = group as MyGroups<MyEntity, MyWeldGroupData>.Group;
        }

        public void OnNodeAdded(MyEntity entity)
        {
            if (entity.MarkedForClose)
            {
                return;
            }
            if (this.m_weldParent != null)
            {
                MyPhysicsBody physics = this.m_weldParent.Physics as MyPhysicsBody;
                if (!physics.IsStatic)
                {
                    if (!entity.Physics.IsStatic && ((physics.RigidBody2 != null) || (entity.Physics.RigidBody2 == null)))
                    {
                        physics.Weld(entity.Physics as MyPhysicsBody, true);
                        goto TR_0004;
                    }
                }
                else
                {
                    physics.Weld(entity.Physics as MyPhysicsBody, true);
                    goto TR_0004;
                }
            }
            else
            {
                this.m_weldParent = entity;
                goto TR_0004;
            }
            this.ReplaceParent(entity);
        TR_0004:
            if ((this.m_weldParent.Physics != null) && (this.m_weldParent.Physics.RigidBody != null))
            {
                this.m_weldParent.Physics.RigidBody.Activate();
            }
            this.m_weldParent.RaisePhysicsChanged();
        }

        public void OnNodeRemoved(MyEntity entity)
        {
            if (this.m_weldParent != null)
            {
                if (ReferenceEquals(this.m_weldParent, entity))
                {
                    if (((this.m_group.Nodes.Count != 1) || !this.m_group.Nodes.First().NodeData.MarkedForClose) && (this.m_group.Nodes.Count > 0))
                    {
                        this.ReplaceParent(null);
                    }
                }
                else if ((this.m_weldParent.Physics != null) && !entity.MarkedForClose)
                {
                    (this.m_weldParent.Physics as MyPhysicsBody).Unweld(entity.Physics as MyPhysicsBody, true, true);
                }
                if (((this.m_weldParent != null) && (this.m_weldParent.Physics != null)) && (this.m_weldParent.Physics.RigidBody != null))
                {
                    this.m_weldParent.Physics.RigidBody.Activate();
                    this.m_weldParent.RaisePhysicsChanged();
                }
                entity.RaisePhysicsChanged();
            }
        }

        public void OnRelease()
        {
            this.m_group = null;
            this.m_weldParent = null;
        }

        private void ReplaceParent(MyEntity newParent)
        {
            this.m_weldParent = MyWeldingGroups.ReplaceParent(this.m_group, this.m_weldParent, newParent);
        }

        public bool UpdateParent(MyEntity oldParent)
        {
            MyPhysicsBody physicsBody = oldParent.GetPhysicsBody();
            if (physicsBody.WeldedRigidBody.IsFixed)
            {
                return false;
            }
            MyPhysicsBody objA = physicsBody;
            foreach (MyPhysicsBody body3 in physicsBody.WeldInfo.Children)
            {
                if (body3.WeldedRigidBody.IsFixed)
                {
                    objA = body3;
                    break;
                }
                if (!objA.Flags.HasFlag(RigidBodyFlag.RBF_DOUBLED_KINEMATIC) && body3.Flags.HasFlag(RigidBodyFlag.RBF_DOUBLED_KINEMATIC))
                {
                    objA = body3;
                }
            }
            if (ReferenceEquals(objA, physicsBody))
            {
                return false;
            }
            this.ReplaceParent((MyEntity) objA.Entity);
            objA.Weld(physicsBody, true);
            return true;
        }

        public MyEntity Parent =>
            this.m_weldParent;
    }
}

