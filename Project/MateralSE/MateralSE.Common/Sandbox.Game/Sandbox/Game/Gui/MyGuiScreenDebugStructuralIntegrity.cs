namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.StructuralIntegrity;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Game.Entity;
    using VRageMath;

    internal class MyGuiScreenDebugStructuralIntegrity : MyGuiScreenDebugBase
    {
        private MyGuiControlCombobox m_animationComboA;
        private MyGuiControlCombobox m_animationComboB;
        private MyGuiControlSlider m_blendSlider;
        private MyGuiControlCombobox m_animationCombo;
        private MyGuiControlCheckbox m_loopCheckbox;

        public MyGuiScreenDebugStructuralIntegrity() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        private void DeleteFractures()
        {
            if (Sync.IsServer)
            {
                foreach (MyEntity entity in MyEntities.GetEntities())
                {
                    if (entity is MyFracturedPiece)
                    {
                        MyFracturedPiecesManager.Static.RemoveFracturePiece(entity as MyFracturedPiece, 0f, false, true);
                    }
                    if (entity is MyCubeGrid)
                    {
                        foreach (MySlimBlock local1 in (entity as MyCubeGrid).GetBlocks())
                        {
                        }
                    }
                }
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugStructuralIntegrity";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Structural integrity", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f * base.m_scale;
            Vector4? color = null;
            captionOffset = null;
            base.AddCheckBox("Enabled", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Property(null, (MethodInfo) methodof(MyStructuralIntegrity.get_Enabled)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw numbers", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyAdvancedStaticSimulator.DrawText)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            this.AddSlider("Closest distance threshold", 0f, 16f, (Func<float>) (() => MyAdvancedStaticSimulator.ClosestDistanceThreshold), (Action<float>) (x => (MyAdvancedStaticSimulator.ClosestDistanceThreshold = x)), color);
            color = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Delete fractures"), <p0> => this.DeleteFractures(), null, color, captionOffset, true, true);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugStructuralIntegrity.<>c <>9 = new MyGuiScreenDebugStructuralIntegrity.<>c();
            public static Func<float> <>9__6_2;
            public static Action<float> <>9__6_3;

            internal float <RecreateControls>b__6_2() => 
                MyAdvancedStaticSimulator.ClosestDistanceThreshold;

            internal void <RecreateControls>b__6_3(float x)
            {
                MyAdvancedStaticSimulator.ClosestDistanceThreshold = x;
            }
        }
    }
}

