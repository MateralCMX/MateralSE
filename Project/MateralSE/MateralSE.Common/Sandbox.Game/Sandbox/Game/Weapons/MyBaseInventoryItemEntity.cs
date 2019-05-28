namespace Sandbox.Game.Weapons
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Components;
    using System;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.ObjectBuilders;

    public class MyBaseInventoryItemEntity : MyEntity
    {
        private MyPhysicalItemDefinition m_definition;
        private float m_amount;

        public MyBaseInventoryItemEntity()
        {
            base.Render = new MyRenderComponentInventoryItem();
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            this.m_definition = MyDefinitionManager.Static.GetPhysicalItemDefinition(objectBuilder.GetId());
            float? scale = null;
            this.Init(null, this.m_definition.Model, null, scale, null);
            base.Render.SkipIfTooSmall = false;
            base.Render.NeedsDraw = true;
            this.InitSpherePhysics(MyMaterialType.METAL, base.Model, 1f, 1f, 1f, 0, RigidBodyFlag.RBF_DEFAULT);
            base.Physics.Enabled = true;
        }

        public string[] IconTextures =>
            this.m_definition.Icons;
    }
}

