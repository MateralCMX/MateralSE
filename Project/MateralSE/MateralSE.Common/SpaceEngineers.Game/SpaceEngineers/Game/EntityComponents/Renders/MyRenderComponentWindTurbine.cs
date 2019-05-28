namespace SpaceEngineers.Game.EntityComponents.Renders
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents.Renders;
    using Sandbox.RenderDirect.ActorComponents;
    using SpaceEngineers.Game.Entities.Blocks;
    using System;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    public class MyRenderComponentWindTurbine : MyRenderComponentCubeBlockWithParentedSubpart
    {
        public class TurbineRenderComponent : MyParentedSubpartRenderComponent
        {
            private float m_speed;
            private Color m_color;
            private bool m_animatorInitialized;

            public override void OnParented()
            {
                if (!((MyCubeGrid) base.Entity.Parent.Parent).IsPreview)
                {
                    MyRenderProxy.UpdateRenderComponent<MyRotationAnimatorInitData, MyRenderComponentWindTurbine.TurbineRenderComponent>(base.GetRenderObjectID(), this, delegate (MyRotationAnimatorInitData message, MyRenderComponentWindTurbine.TurbineRenderComponent thiz) {
                        MyWindTurbineDefinition definition = this.Definition;
                        message.SpinUpSpeed = definition.TurbineSpinUpSpeed;
                        message.SpinDownSpeed = definition.TurbineSpinDownSpeed;
                        message.RotationAxis = MyRotationAnimator.RotationAxis.AxisY;
                    });
                    base.OnParented();
                    this.m_animatorInitialized = true;
                    this.SendSpeed();
                    this.SendColor();
                }
            }

            private void SendColor()
            {
                if (base.GetRenderObjectID() != uint.MaxValue)
                {
                    base.Entity.SetEmissiveParts("Emissive", this.m_color, 1f);
                }
            }

            private void SendSpeed()
            {
                if (this.m_animatorInitialized)
                {
                    uint renderObjectID = base.GetRenderObjectID();
                    if (renderObjectID != uint.MaxValue)
                    {
                        FloatData.Update<MyRotationAnimator>(renderObjectID, this.m_speed);
                    }
                }
            }

            public void SetColor(Color color)
            {
                if (this.m_color != color)
                {
                    this.m_color = color;
                    this.SendColor();
                }
            }

            public void SetSpeed(float speed)
            {
                if (this.m_speed != speed)
                {
                    this.m_speed = speed;
                    this.SendSpeed();
                }
            }

            protected MyWindTurbineDefinition Definition =>
                ((MyWindTurbine) base.Entity.Parent).BlockDefinition;
        }
    }
}

