namespace Sandbox.Game.Replication
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Replication.StateGroups;
    using System;
    using VRage.Game.ObjectBuilders;
    using VRage.Library.Collections;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    public class MyInventoryBagReplicable : MyEntityReplicableBase<MyInventoryBagEntity>
    {
        protected override IMyStateGroup CreatePhysicsGroup() => 
            new MyEntityPhysicsStateGroup(base.Instance, this);

        protected override void OnLoad(BitStream stream, Action<MyInventoryBagEntity> loadingDoneHandler)
        {
            MyObjectBuilder_InventoryBagEntity builder = (MyObjectBuilder_InventoryBagEntity) MySerializer.CreateAndRead<MyObjectBuilder_EntityBase>(stream, MyObjectBuilderSerializer.Dynamic);
            if (MyInventoryBagEntity.GetPhysicsComponentBuilder(builder) != null)
            {
                base.TryRemoveExistingEntity(builder.EntityId);
                MyInventoryBagEntity entity2 = (MyInventoryBagEntity) MyEntities.CreateFromObjectBuilderAndAdd(builder, false);
                loadingDoneHandler(entity2);
            }
        }

        public override bool OnSave(BitStream stream, Endpoint clientEndpoint)
        {
            MyObjectBuilder_InventoryBagEntity objectBuilder = (MyObjectBuilder_InventoryBagEntity) base.Instance.GetObjectBuilder(false);
            if (string.IsNullOrEmpty(objectBuilder.SubtypeName))
            {
                return false;
            }
            if (MyInventoryBagEntity.GetPhysicsComponentBuilder(objectBuilder) == null)
            {
                return false;
            }
            MySerializer.Write<MyObjectBuilder_InventoryBagEntity>(stream, ref objectBuilder, MyObjectBuilderSerializer.Dynamic);
            return true;
        }
    }
}

