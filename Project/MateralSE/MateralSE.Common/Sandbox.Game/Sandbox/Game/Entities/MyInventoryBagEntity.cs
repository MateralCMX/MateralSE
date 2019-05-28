namespace Sandbox.Game.Entities
{
    using Havok;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ObjectBuilders;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;

    [MyEntityType(typeof(MyObjectBuilder_ReplicableEntity), false), MyEntityType(typeof(MyObjectBuilder_InventoryBagEntity), true)]
    public class MyInventoryBagEntity : MyEntity, IMyInventoryBag, VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity
    {
        private Vector3 m_gravity = Vector3.Zero;
        private MyDefinitionId m_definitionId;
        public long OwnerIdentityId;

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            MyObjectBuilder_EntityBase objectBuilder = base.GetObjectBuilder(copy);
            MyObjectBuilder_InventoryBagEntity entity = objectBuilder as MyObjectBuilder_InventoryBagEntity;
            if (entity != null)
            {
                entity.OwnerIdentityId = this.OwnerIdentityId;
            }
            return objectBuilder;
        }

        internal static MyObjectBuilder_PhysicsComponentBase GetPhysicsComponentBuilder(MyObjectBuilder_InventoryBagEntity builder)
        {
            if ((builder.ComponentContainer != null) && (builder.ComponentContainer.Components.Count > 0))
            {
                using (List<MyObjectBuilder_ComponentContainer.ComponentData>.Enumerator enumerator = builder.ComponentContainer.Components.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyObjectBuilder_ComponentContainer.ComponentData current = enumerator.Current;
                        if (current.Component is MyObjectBuilder_PhysicsComponentBase)
                        {
                            return (current.Component as MyObjectBuilder_PhysicsComponentBase);
                        }
                    }
                }
            }
            return null;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            if ((objectBuilder.EntityDefinitionId != null) && (objectBuilder.EntityDefinitionId.Value.TypeId != typeof(MyObjectBuilder_InventoryBagEntity)))
            {
                objectBuilder.EntityDefinitionId = new SerializableDefinitionId(typeof(MyObjectBuilder_InventoryBagEntity), objectBuilder.EntityDefinitionId.Value.SubtypeName);
            }
            base.Init(objectBuilder);
            if (!Sync.IsServer)
            {
                HkShape shape = base.Physics.RigidBody.GetShape();
                HkMassProperties properties = new HkMassProperties {
                    Mass = base.Physics.RigidBody.Mass
                };
                MyPhysicsBody physics = base.Physics as MyPhysicsBody;
                physics.Close();
                physics.ReportAllContacts = true;
                physics.Flags = RigidBodyFlag.RBF_STATIC;
                physics.CreateFromCollisionObject(shape, Vector3.Zero, base.WorldMatrix, new HkMassProperties?(properties), 0x17);
                physics.RigidBody.ContactPointCallbackEnabled = true;
                physics.ContactPointCallback += new MyPhysicsBody.PhysicsContactHandler(this.OnPhysicsContactPointCallback);
            }
            if (objectBuilder is MyObjectBuilder_InventoryBagEntity)
            {
                MyObjectBuilder_InventoryBagEntity builder = (MyObjectBuilder_InventoryBagEntity) objectBuilder;
                if (GetPhysicsComponentBuilder(builder) == null)
                {
                    base.Physics.LinearVelocity = (Vector3) builder.LinearVelocity;
                    base.Physics.AngularVelocity = (Vector3) builder.AngularVelocity;
                }
                if (builder != null)
                {
                    this.OwnerIdentityId = builder.OwnerIdentityId;
                }
            }
            else if (objectBuilder is MyObjectBuilder_ReplicableEntity)
            {
                MyObjectBuilder_ReplicableEntity entity2 = (MyObjectBuilder_ReplicableEntity) objectBuilder;
                base.Physics.LinearVelocity = (Vector3) entity2.LinearVelocity;
                base.Physics.AngularVelocity = (Vector3) entity2.AngularVelocity;
            }
            base.OnClosing += new Action<MyEntity>(this.MyInventoryBagEntity_OnClosing);
        }

        private void MyInventoryBagEntity_OnClosing(MyEntity obj)
        {
            if (Sync.IsServer)
            {
                MyGps gpsByEntityId = MySession.Static.Gpss.GetGpsByEntityId(this.OwnerIdentityId, base.EntityId);
                if (gpsByEntityId != null)
                {
                    MySession.Static.Gpss.SendDelete(this.OwnerIdentityId, gpsByEntityId.Hash);
                }
            }
        }

        private void OnPhysicsContactPointCallback(ref MyPhysics.MyContactPointEvent e)
        {
            if ((base.Physics.LinearVelocity.LengthSquared() > 225f) && (e.ContactPointEvent.GetOtherEntity(this) is MyCharacter))
            {
                e.ContactPointEvent.Base.Disable();
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            this.UpdateGravity();
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if ((base.Physics != null) && !base.Physics.IsStatic)
            {
                base.Physics.RigidBody.Gravity = this.m_gravity;
            }
        }

        private void UpdateGravity()
        {
            if ((base.Physics != null) && !base.Physics.IsStatic)
            {
                this.m_gravity = MyGravityProviderSystem.CalculateTotalGravityInPoint(base.PositionComp.GetPosition());
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            this.UpdateGravity();
        }
    }
}

