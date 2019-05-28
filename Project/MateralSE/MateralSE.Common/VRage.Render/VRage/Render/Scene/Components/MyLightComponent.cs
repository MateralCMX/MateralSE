namespace VRage.Render.Scene.Components
{
    using System;
    using VRageRender.Messages;

    public abstract class MyLightComponent : MyActorComponent
    {
        protected UpdateRenderLightData m_data;
        protected UpdateRenderLightData m_originalData;

        protected MyLightComponent()
        {
        }

        public abstract void UpdateData(ref UpdateRenderLightData data);

        public UpdateRenderLightData Data =>
            this.m_originalData;
    }
}

