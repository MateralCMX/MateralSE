namespace Sandbox.Game.EntityComponents.Renders
{
    using Sandbox.RenderDirect.ActorComponents;
    using System;
    using System.Runtime.CompilerServices;
    using VRageRender;
    using VRageRender.Messages;

    public class MyRenderComponentShipGrinder : MyRenderComponentCubeBlockWithParentedSubpart
    {
        public class MyRenderComponentShipGrinderBlade : MyParentedSubpartRenderComponent
        {
            private float m_speed;

            public override void OnParented()
            {
                base.OnParented();
                this.m_speed = 0f;
                MyRenderProxy.UpdateRenderComponent<MyRotationAnimatorInitData, object>(base.GetRenderObjectID(), null, delegate (MyRotationAnimatorInitData d, object _) {
                    d.RotationAxis = MyRotationAnimator.RotationAxis.AxisX;
                    d.SpinUpSpeed = 41.8879f;
                    d.SpinDownSpeed = 20.94395f;
                });
            }

            public void UpdateBladeSpeed(float speed)
            {
                if (this.m_speed != speed)
                {
                    this.m_speed = speed;
                    uint renderObjectID = base.GetRenderObjectID();
                    if (renderObjectID != uint.MaxValue)
                    {
                        FloatData.Update<MyRotationAnimator>(renderObjectID, this.m_speed);
                    }
                }
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyRenderComponentShipGrinder.MyRenderComponentShipGrinderBlade.<>c <>9 = new MyRenderComponentShipGrinder.MyRenderComponentShipGrinderBlade.<>c();
                public static Action<MyRotationAnimatorInitData, object> <>9__1_0;

                internal void <OnParented>b__1_0(MyRotationAnimatorInitData d, object _)
                {
                    d.RotationAxis = MyRotationAnimator.RotationAxis.AxisX;
                    d.SpinUpSpeed = 41.8879f;
                    d.SpinDownSpeed = 20.94395f;
                }
            }
        }
    }
}

