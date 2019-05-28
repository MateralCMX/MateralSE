namespace Sandbox.Game.World
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ObjectBuilders;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ObjectBuilders;
    using VRageMath;

    public class MySessionCompatHelper
    {
        public virtual void AfterEntitiesLoad(int saveVersion)
        {
        }

        public virtual void CheckAndFixPrefab(MyObjectBuilder_Definitions prefab)
        {
        }

        protected MyObjectBuilder_EntityBase ConvertBuilderToEntityBase(MyObjectBuilder_EntityBase origEntity, string subTypeNamePrefix)
        {
            string str = !string.IsNullOrEmpty(origEntity.SubtypeName) ? origEntity.SubtypeName : ((origEntity.EntityDefinitionId != null) ? origEntity.EntityDefinitionId.Value.SubtypeName : null);
            if (str == null)
            {
                return null;
            }
            string subtypeName = (subTypeNamePrefix != null) ? subTypeNamePrefix : (str ?? "");
            MyObjectBuilder_EntityBase base2 = MyObjectBuilderSerializer.CreateNewObject(typeof(MyObjectBuilder_EntityBase), subtypeName) as MyObjectBuilder_EntityBase;
            base2.EntityId = origEntity.EntityId;
            base2.PersistentFlags = origEntity.PersistentFlags;
            base2.Name = origEntity.Name;
            base2.PositionAndOrientation = origEntity.PositionAndOrientation;
            base2.ComponentContainer = origEntity.ComponentContainer;
            if ((base2.ComponentContainer != null) && (base2.ComponentContainer.Components.Count > 0))
            {
                foreach (MyObjectBuilder_ComponentContainer.ComponentData data in base2.ComponentContainer.Components)
                {
                    if (string.IsNullOrEmpty(data.Component.SubtypeName))
                    {
                        continue;
                    }
                    if (data.Component.SubtypeName == str)
                    {
                        data.Component.SubtypeName = subtypeName;
                    }
                }
            }
            return base2;
        }

        protected MyObjectBuilder_EntityBase ConvertInventoryBagToEntityBase(MyObjectBuilder_EntityBase oldBuilder)
        {
            MyObjectBuilder_ReplicableEntity oldBagBuilder = oldBuilder as MyObjectBuilder_ReplicableEntity;
            if (oldBagBuilder != null)
            {
                return this.ConvertInventoryBagToEntityBase(oldBagBuilder, (Vector3) oldBagBuilder.LinearVelocity, (Vector3) oldBagBuilder.AngularVelocity);
            }
            MyObjectBuilder_InventoryBagEntity entity2 = oldBuilder as MyObjectBuilder_InventoryBagEntity;
            return ((entity2 == null) ? null : this.ConvertInventoryBagToEntityBase(entity2, (Vector3) entity2.LinearVelocity, (Vector3) entity2.AngularVelocity));
        }

        private MyObjectBuilder_EntityBase ConvertInventoryBagToEntityBase(MyObjectBuilder_EntityBase oldBagBuilder, Vector3 linearVelocity, Vector3 angularVelocity)
        {
            MyObjectBuilder_EntityBase base2 = this.ConvertBuilderToEntityBase(oldBagBuilder, null);
            if (base2 == null)
            {
                return null;
            }
            if (base2.ComponentContainer == null)
            {
                base2.ComponentContainer = MyObjectBuilderSerializer.CreateNewObject(typeof(MyObjectBuilder_ComponentContainer), base2.SubtypeName) as MyObjectBuilder_ComponentContainer;
            }
            using (List<MyObjectBuilder_ComponentContainer.ComponentData>.Enumerator enumerator = base2.ComponentContainer.Components.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (enumerator.Current.Component is MyObjectBuilder_PhysicsComponentBase)
                    {
                        return base2;
                    }
                }
            }
            MyObjectBuilder_PhysicsComponentBase base3 = MyObjectBuilderSerializer.CreateNewObject(typeof(MyObjectBuilder_PhysicsBodyComponent), base2.SubtypeName) as MyObjectBuilder_PhysicsComponentBase;
            MyObjectBuilder_ComponentContainer.ComponentData item = new MyObjectBuilder_ComponentContainer.ComponentData();
            item.Component = base3;
            item.TypeId = typeof(MyPhysicsComponentBase).Name;
            base2.ComponentContainer.Components.Add(item);
            base3.LinearVelocity = linearVelocity;
            base3.AngularVelocity = angularVelocity;
            return base2;
        }

        public virtual void FixSessionComponentObjectBuilders(MyObjectBuilder_Checkpoint checkpoint, MyObjectBuilder_Sector sector)
        {
        }

        public virtual void FixSessionObjectBuilders(MyObjectBuilder_Checkpoint checkpoint, MyObjectBuilder_Sector sector)
        {
        }
    }
}

