namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Game.Gui;
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRageMath;

    [MyDebugScreen("VRage", "Debug draw settings")]
    internal class MyGuiScreenDebugDrawSettings : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugDrawSettings() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugDrawSettings";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Debug draw settings 1", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            Vector4? color = null;
            captionOffset = null;
            base.AddCheckBox("Debug draw", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Property(null, (MethodInfo) methodof(MyDebugDrawSettings.get_ENABLE_DEBUG_DRAW)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Entity IDs", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_ENTITY_IDS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("    Only root entities", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_ENTITY_IDS_ONLY_ROOT)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Model dummies", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_MODEL_DUMMIES)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Displaced bones", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_DISPLACED_BONES)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Interpolation", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_INTERPOLATION)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Mount points", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("GUI screen borders", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.DRAW_GUI_SCREEN_BORDERS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw physics", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_PHYSICS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Triangle physics", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_TRIANGLE_PHYSICS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Audio debug draw", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_AUDIO)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Show invalid triangles", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.SHOW_INVALID_TRIANGLES)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Show stockpile quantities", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_STOCKPILE_QUANTITIES)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Show suit battery capacity", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_SUIT_BATTERY_CAPACITY)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Show character bones", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_BONES)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Character miscellaneous", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_MISC)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Game prunning structure", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_GAME_PRUNNING)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Miscellaneous", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_MISCELLANEOUS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Events", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_EVENTS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Volumetric explosion coloring", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VOLUMETRIC_EXPLOSION_COLORING)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugDrawSettings.<>c <>9 = new MyGuiScreenDebugDrawSettings.<>c();
        }
    }
}

