namespace SpaceEngineers.Game.EntityComponents.Renders
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents.Renders;
    using Sandbox.RenderDirect.ActorComponents;
    using SpaceEngineers.Game.Entities.Blocks;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    public class MyRenderComponentHydrogenEngine : MyRenderComponentCubeBlockWithParentedSubpart
    {
        public class MyHydrogenEngineSubpartRenderComponent<TComponent> : MyParentedSubpartRenderComponent where TComponent: MyRenderDirectComponent
        {
            private float m_speed;
            private bool m_animatorInitialized;

            protected void FillAnimationParams(MySpinupAnimatorInitData data)
            {
                MyHydrogenEngineDefinition definition = this.Definition;
                data.SpinUpSpeed = definition.AnimationSpinUpSpeed;
                data.SpinDownSpeed = definition.AnimationSpinDownSpeed;
            }

            public override void OnParented()
            {
                base.OnParented();
                this.m_animatorInitialized = true;
                this.SendSpeed();
            }

            private void SendSpeed()
            {
                if (this.m_animatorInitialized)
                {
                    uint renderObjectID = base.GetRenderObjectID();
                    if (renderObjectID != uint.MaxValue)
                    {
                        FloatData.Update<TComponent>(renderObjectID, this.m_speed);
                    }
                }
            }

            public void SetSpeed(float speed)
            {
                if (speed != this.m_speed)
                {
                    this.m_speed = speed;
                    this.SendSpeed();
                }
            }

            protected MyHydrogenEngineDefinition Definition =>
                ((MyHydrogenEngine) base.Entity.Parent).BlockDefinition;
        }

        public class MyPistonRenderComponent : MyRenderComponentHydrogenEngine.MyHydrogenEngineSubpartRenderComponent<MyTranslationAnimator>
        {
            public override void OnParented()
            {
                if (!((MyCubeGrid) base.Entity.Parent.Parent).IsPreview)
                {
                    MyRenderProxy.UpdateRenderComponent<MyTranslationAnimatorInitData, MyRenderComponentHydrogenEngine.MyPistonRenderComponent>(base.GetRenderObjectID(), this, delegate (MyTranslationAnimatorInitData message, MyRenderComponentHydrogenEngine.MyPistonRenderComponent thiz) {
                        thiz.FillAnimationParams(message);
                        message.AnimationOffset = this.AnimationOffset;
                        message.TranslationAxis = Base6Directions.Direction.Up;
                        message.MinPosition = thiz.Definition.PistonAnimationMin;
                        message.MaxPosition = thiz.Definition.PistonAnimationMax;
                        thiz.GetCullObjectRelativeMatrix(out message.BaseRelativeTransform);
                    });
                    base.OnParented();
                }
            }

            public float AnimationOffset { get; set; }
        }

        public class MyRotatingSubpartRenderComponent : MyRenderComponentHydrogenEngine.MyHydrogenEngineSubpartRenderComponent<MyRotationAnimator>
        {
            public override void OnParented()
            {
                if (!((MyCubeGrid) base.Entity.Parent.Parent).IsPreview)
                {
                    MyRenderProxy.UpdateRenderComponent<MyRotationAnimatorInitData, MyRenderComponentHydrogenEngine.MyRotatingSubpartRenderComponent>(base.GetRenderObjectID(), this, delegate (MyRotationAnimatorInitData message, MyRenderComponentHydrogenEngine.MyRotatingSubpartRenderComponent thiz) {
                        thiz.FillAnimationParams(message);
                        message.RotationAxis = MyRotationAnimator.RotationAxis.AxisZ;
                    });
                    base.OnParented();
                }
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyRenderComponentHydrogenEngine.MyRotatingSubpartRenderComponent.<>c <>9 = new MyRenderComponentHydrogenEngine.MyRotatingSubpartRenderComponent.<>c();
                public static Action<MyRotationAnimatorInitData, MyRenderComponentHydrogenEngine.MyRotatingSubpartRenderComponent> <>9__0_0;

                internal void <OnParented>b__0_0(MyRotationAnimatorInitData message, MyRenderComponentHydrogenEngine.MyRotatingSubpartRenderComponent thiz)
                {
                    thiz.FillAnimationParams(message);
                    message.RotationAxis = MyRotationAnimator.RotationAxis.AxisZ;
                }
            }
        }
    }
}

