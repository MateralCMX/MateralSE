namespace Sandbox.Game.Components
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents.Renders;
    using Sandbox.Game.Multiplayer;
    using Sandbox.RenderDirect.ActorComponents;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    public class MyRenderComponentThrust : MyRenderComponentCubeBlockWithParentedSubpart
    {
        private float m_strength;
        private bool m_flamesEnabled;
        private float m_propellerSpeed;
        private MyThrust m_thrust;
        private bool m_flameAnimatorInitialized;

        public override void AddRenderObjects()
        {
            base.AddRenderObjects();
            float strength = this.m_strength;
            bool flamesEnabled = this.m_flamesEnabled;
            this.m_strength = 0f;
            this.m_propellerSpeed = 0f;
            this.m_flamesEnabled = false;
            this.m_flameAnimatorInitialized = false;
            this.UpdateFlameAnimatorData();
            if (this.m_flameAnimatorInitialized)
            {
                this.UpdateFlameProperties(flamesEnabled, strength);
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_thrust = base.Container.Entity as MyThrust;
        }

        public void UpdateFlameAnimatorData()
        {
            if ((this.m_thrust.CubeGrid.Physics != null) && !Sync.IsDedicated)
            {
                uint renderObjectID = base.GetRenderObjectID();
                if (renderObjectID != uint.MaxValue)
                {
                    if (this.m_thrust.Flames.Count != 0)
                    {
                        this.m_flameAnimatorInitialized = true;
                        MyRenderProxy.UpdateRenderComponent<FlameData, MyThrust>(renderObjectID, this.m_thrust, delegate (FlameData d, MyThrust t) {
                            MatrixD localMatrix = t.PositionComp.LocalMatrix;
                            d.LightPosition = Vector3D.TransformNormal(t.Flames[0].Position, localMatrix) + localMatrix.Translation;
                            d.Flames = t.Flames;
                            d.FlareSize = t.Flares.Size;
                            d.Glares = t.Flares.SubGlares;
                            d.GridScale = t.CubeGrid.GridScale;
                            d.FlareIntensity = t.Flares.Intensity;
                            d.FlamePointMaterial = t.FlamePointMaterial;
                            d.FlameLengthMaterial = t.FlameLengthMaterial;
                            d.GlareQuerySize = t.CubeGrid.GridSize / 2.5f;
                            d.IdleColor = t.BlockDefinition.FlameIdleColor;
                            d.FullColor = t.BlockDefinition.FlameFullColor;
                            d.FlameLengthScale = t.BlockDefinition.FlameLengthScale;
                        });
                    }
                    else if (this.m_flameAnimatorInitialized)
                    {
                        this.UpdateFlameProperties(false, 0f);
                        this.m_flameAnimatorInitialized = false;
                        MyRenderProxy.RemoveRenderComponent<MyThrustFlameAnimator>(renderObjectID);
                    }
                }
            }
        }

        public void UpdateFlameProperties(bool enabled, float strength)
        {
            if ((this.m_thrust.CubeGrid.Physics != null) && this.m_flameAnimatorInitialized)
            {
                bool flag = false;
                if (this.m_strength != strength)
                {
                    flag = true;
                    this.m_strength = strength;
                }
                if (this.m_flamesEnabled != enabled)
                {
                    flag = true;
                    this.m_flamesEnabled = enabled;
                }
                if (flag)
                {
                    FloatData.Update<MyThrustFlameAnimator>(base.GetRenderObjectID(), this.m_flamesEnabled ? this.m_strength : -1f);
                }
            }
        }

        public void UpdatePropellerSpeed(float propellerSpeed)
        {
            if (this.m_propellerSpeed != propellerSpeed)
            {
                this.m_propellerSpeed = propellerSpeed;
                ((MyPropellerRenderComponent) this.m_thrust.Propeller.Render).SendPropellerSpeed(propellerSpeed);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyRenderComponentThrust.<>c <>9 = new MyRenderComponentThrust.<>c();
            public static Action<FlameData, MyThrust> <>9__8_0;

            internal void <UpdateFlameAnimatorData>b__8_0(FlameData d, MyThrust t)
            {
                MatrixD localMatrix = t.PositionComp.LocalMatrix;
                d.LightPosition = Vector3D.TransformNormal(t.Flames[0].Position, localMatrix) + localMatrix.Translation;
                d.Flames = t.Flames;
                d.FlareSize = t.Flares.Size;
                d.Glares = t.Flares.SubGlares;
                d.GridScale = t.CubeGrid.GridScale;
                d.FlareIntensity = t.Flares.Intensity;
                d.FlamePointMaterial = t.FlamePointMaterial;
                d.FlameLengthMaterial = t.FlameLengthMaterial;
                d.GlareQuerySize = t.CubeGrid.GridSize / 2.5f;
                d.IdleColor = t.BlockDefinition.FlameIdleColor;
                d.FullColor = t.BlockDefinition.FlameFullColor;
                d.FlameLengthScale = t.BlockDefinition.FlameLengthScale;
            }
        }

        public class MyPropellerRenderComponent : MyParentedSubpartRenderComponent
        {
            public override void OnParented()
            {
                base.OnParented();
                MyThrust parent = (MyThrust) base.Entity.Parent;
                MyRenderProxy.UpdateRenderComponent<MyRotationAnimatorInitData, MyThrust>(base.GetRenderObjectID(), parent, delegate (MyRotationAnimatorInitData d, MyThrust t) {
                    MyThrustDefinition blockDefinition = t.BlockDefinition;
                    float num = blockDefinition.PropellerFullSpeed * 6.283185f;
                    d.SpinUpSpeed = num / blockDefinition.PropellerAcceleration;
                    d.SpinDownSpeed = num / blockDefinition.PropellerDeceleration;
                    d.RotationAxis = MyRotationAnimator.RotationAxis.AxisZ;
                });
                this.SendPropellerSpeed(parent.Render.m_propellerSpeed);
            }

            public void SendPropellerSpeed(float speed)
            {
                FloatData.Update<MyRotationAnimator>(base.GetRenderObjectID(), speed);
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyRenderComponentThrust.MyPropellerRenderComponent.<>c <>9 = new MyRenderComponentThrust.MyPropellerRenderComponent.<>c();
                public static Action<MyRotationAnimatorInitData, MyThrust> <>9__0_0;

                internal void <OnParented>b__0_0(MyRotationAnimatorInitData d, MyThrust t)
                {
                    MyThrustDefinition blockDefinition = t.BlockDefinition;
                    float num = blockDefinition.PropellerFullSpeed * 6.283185f;
                    d.SpinUpSpeed = num / blockDefinition.PropellerAcceleration;
                    d.SpinDownSpeed = num / blockDefinition.PropellerDeceleration;
                    d.RotationAxis = MyRotationAnimator.RotationAxis.AxisZ;
                }
            }
        }
    }
}

