namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Game.Gui;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRageMath;

    [MyDebugScreen("VRage", "Asteroids")]
    public class MyGuiScreenDebugAsteroids : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugAsteroids() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            base.GetType().Name;

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.13f);
            Vector2? captionOffset = null;
            base.AddCaption("Asteroids", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            Vector4? captionTextColor = null;
            captionOffset = null;
            base.AddCaption("Asteroid generator", captionTextColor, captionOffset, 0.8f);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Draw voxelmap AABB", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VOXEL_MAP_AABB)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Draw asteroid composition", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_ASTEROID_COMPOSITION)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Draw asteroid seeds", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_ASTEROID_SEEDS)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Draw asteroid content", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_ASTEROID_COMPOSITION_CONTENT)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Draw asteroid ores", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_ASTEROID_ORES)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugAsteroids.<>c <>9 = new MyGuiScreenDebugAsteroids.<>c();
        }
    }
}

