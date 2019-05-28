namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using Sandbox.Game.Components;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Network;
    using VRage.ObjectBuilders;

    [MyEntityType(typeof(MyObjectBuilder_Rope), true)]
    public class MyRope : MyEntity, IMyEventProxy, IMyEventOwner
    {
        public MyRope()
        {
            base.Render = new MyRenderComponentRope();
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            MyRopeData data;
            MyRopeComponent.GetRopeData(base.EntityId, out data);
            MyObjectBuilder_Rope objectBuilder = (MyObjectBuilder_Rope) base.GetObjectBuilder(copy);
            objectBuilder.MaxRopeLength = data.MaxRopeLength;
            objectBuilder.CurrentRopeLength = data.CurrentRopeLength;
            objectBuilder.EntityIdHookA = data.HookEntityIdA;
            objectBuilder.EntityIdHookB = data.HookEntityIdB;
            objectBuilder.SubtypeName = data.Definition.Id.SubtypeName;
            return objectBuilder;
        }

        public override unsafe void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            MyDefinitionId id;
            base.Init(objectBuilder);
            MyObjectBuilder_Rope rope = (MyObjectBuilder_Rope) objectBuilder;
            string subtypeName = rope.SubtypeName;
            MyDefinitionId* idPtr1 = (MyDefinitionId*) new MyDefinitionId(typeof(MyObjectBuilder_RopeDefinition), subtypeName ?? "BasicRope");
            idPtr1 = (MyDefinitionId*) ref id;
            MyRopeData publicData = new MyRopeData {
                HookEntityIdA = rope.EntityIdHookA,
                HookEntityIdB = rope.EntityIdHookB,
                MaxRopeLength = rope.MaxRopeLength,
                CurrentRopeLength = rope.CurrentRopeLength,
                Definition = MyDefinitionManager.Static.GetRopeDefinition(id)
            };
            MyRopeComponent.AddRopeData(publicData, rope.EntityId);
        }
    }
}

