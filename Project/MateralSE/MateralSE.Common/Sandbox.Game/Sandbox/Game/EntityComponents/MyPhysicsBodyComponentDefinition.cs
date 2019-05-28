namespace Sandbox.Game.EntityComponents
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.ComponentSystem;

    [MyDefinitionType(typeof(MyObjectBuilder_PhysicsBodyComponentDefinition), (Type) null)]
    public class MyPhysicsBodyComponentDefinition : MyPhysicsComponentDefinitionBase
    {
        public bool CreateFromCollisionObject;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_PhysicsBodyComponentDefinition objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_PhysicsBodyComponentDefinition;
            objectBuilder.CreateFromCollisionObject = this.CreateFromCollisionObject;
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_PhysicsBodyComponentDefinition definition = builder as MyObjectBuilder_PhysicsBodyComponentDefinition;
            this.CreateFromCollisionObject = definition.CreateFromCollisionObject;
        }
    }
}

