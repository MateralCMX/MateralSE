namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using System;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    public class MyDebugRenderComponentDrawPowerReciever : MyDebugRenderComponent
    {
        private readonly MyResourceSinkComponent m_sink;
        private IMyEntity m_entity;

        public MyDebugRenderComponentDrawPowerReciever(MyResourceSinkComponent sink, IMyEntity entity) : base(null)
        {
            this.m_sink = sink;
            this.m_entity = entity;
            this.m_sink.IsPoweredChanged += new Action(this.IsPoweredChanged);
        }

        public override void DebugDraw()
        {
            this.m_sink.DebugDraw((Matrix) this.m_entity.PositionComp.WorldMatrix);
        }

        private void IsPoweredChanged()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_RESOURCE_RECEIVERS)
            {
                MyHud.Notifications.Add(new MyHudNotification(MyStringId.GetOrCompute($"{this.m_entity} PowerChanged:{this.m_sink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId)}"), 0xfa0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            }
        }
    }
}

