namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    internal class MyGuiScreenDebugPlanets : MyGuiScreenDebugBase
    {
        private float[] m_lodRanges;
        private static bool m_massive;
        private static MyGuiScreenDebugPlanets m_instance;

        public MyGuiScreenDebugPlanets() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        private void ChangeValue(float value, int lod)
        {
            this.m_lodRanges[lod] = value;
            float[][] lodClipmapRanges = MyRenderConstants.RenderQualityProfile.LodClipmapRanges;
            for (int i = 0; i < this.m_lodRanges.Length; i++)
            {
                lodClipmapRanges[(int) this.ScaleGroup][i] = this.m_lodRanges[i];
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugPlanets";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.BackgroundColor = new Vector4(1f, 1f, 1f, 0.5f);
            m_instance = this;
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.13f);
            Vector4? color = null;
            Vector2? checkBoxOffset = null;
            base.AddCheckBox("Debug draw areas: ", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_FLORA_BOXES)), Array.Empty<ParameterExpression>())), true, null, color, checkBoxOffset);
            color = null;
            checkBoxOffset = null;
            base.AddCheckBox("Massive", this, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Property(null, (MethodInfo) methodof(MyGuiScreenDebugPlanets.get_Massive)), Array.Empty<ParameterExpression>())), true, null, color, checkBoxOffset);
            this.m_lodRanges = new float[MyRenderConstants.RenderQualityProfile.LodClipmapRanges[(int) this.ScaleGroup].Length];
            for (int i = 0; i < this.m_lodRanges.Length; i++)
            {
                int lod = i;
                color = null;
                this.AddSlider("LOD " + i, this.m_lodRanges[i], 0f, (i < 4) ? ((float) 0x3e8) : ((i < 7) ? ((float) 0x2710) : ((float) 0x493e0)), s => this.ChangeValue(s.Value, lod), color);
            }
        }

        private MyClipmapScaleEnum ScaleGroup =>
            (m_massive ? MyClipmapScaleEnum.Massive : MyClipmapScaleEnum.Normal);

        private static bool Massive
        {
            get => 
                m_massive;
            set
            {
                if (m_massive != value)
                {
                    m_instance.RecreateControls(false);
                    m_massive = value;
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugPlanets.<>c <>9 = new MyGuiScreenDebugPlanets.<>c();
        }
    }
}

