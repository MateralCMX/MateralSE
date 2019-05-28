namespace Sandbox.Game.EntityComponents.Renders
{
    using Sandbox.RenderDirect.ActorComponents;
    using System;
    using System.Runtime.CompilerServices;
    using VRageRender;
    using VRageRender.Messages;

    public class MyShipDrillRenderComponent : MyRenderComponentCubeBlockWithParentedSubpart
    {
        public class MyDrillHeadRenderComponent : MyParentedSubpartRenderComponent
        {
            private float m_speed;

            public override void OnParented()
            {
                base.OnParented();
                MyRenderProxy.UpdateRenderComponent<MyRotationAnimatorInitData, float>(base.GetRenderObjectID(), 25.13274f, delegate (MyRotationAnimatorInitData d, float s) {
                    d.SpinUpSpeed = s;
                    d.SpinDownSpeed = s;
                    d.RotationAxis = MyRotationAnimator.RotationAxis.AxisZ;
                });
            }

            public void UpdateSpeed(float speed)
            {
                if (this.m_speed != speed)
                {
                    this.m_speed = speed;
                    FloatData.Update<MyRotationAnimator>(base.GetRenderObjectID(), speed);
                }
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyShipDrillRenderComponent.MyDrillHeadRenderComponent.<>c <>9 = new MyShipDrillRenderComponent.MyDrillHeadRenderComponent.<>c();
                public static Action<MyRotationAnimatorInitData, float> <>9__1_0;

                internal void <OnParented>b__1_0(MyRotationAnimatorInitData d, float s)
                {
                    d.SpinUpSpeed = s;
                    d.SpinDownSpeed = s;
                    d.RotationAxis = MyRotationAnimator.RotationAxis.AxisZ;
                }
            }
        }
    }
}

