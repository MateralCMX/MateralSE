namespace Sandbox.Game.Components
{
    using System;
    using VRage.Game.Entity;

    internal class MyRenderComponentTextPanel : MyRenderComponentScreenAreas
    {
        public const string PANEL_MATERIAL_NAME = "ScreenArea";

        public MyRenderComponentTextPanel(MyEntity entity) : base(entity)
        {
        }

        public override void AddRenderObjects()
        {
            base.AddRenderObjects();
            base.AddScreenArea(base.RenderObjectIDs, "ScreenArea");
        }
    }
}

