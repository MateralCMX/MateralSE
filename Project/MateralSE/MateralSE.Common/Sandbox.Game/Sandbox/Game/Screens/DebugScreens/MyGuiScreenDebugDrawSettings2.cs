namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Game.Gui;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRageMath;

    [MyDebugScreen("VRage", "Debug draw settings 2")]
    internal class MyGuiScreenDebugDrawSettings2 : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugDrawSettings2() : base(nullable, false)
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
            base.AddCaption("Debug draw settings 2", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            Vector4? color = null;
            captionOffset = null;
            base.AddCheckBox("Entity components", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_ENTITY_COMPONENTS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Controlled entities", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CONTROLLED_ENTITIES)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Copy paste", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_COPY_PASTE)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Voxel geometry cell", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VOXEL_GEOMETRY_CELL)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Voxel map AABB", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VOXEL_MAP_AABB)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Respawn ship counters", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_RESPAWN_SHIP_COUNTERS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Explosion Havok raycasts", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_EXPLOSION_HAVOK_RAYCASTS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Explosion DDA raycasts", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_EXPLOSION_DDA_RAYCASTS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Physics clusters", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_CLUSTERS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Environment items (trees, bushes, ...)", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_ENVIRONMENT_ITEMS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Block groups - small to large", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_SMALL_TO_LARGE_BLOCK_GROUPS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Ropes", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_ROPES)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Oxygen", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_OXYGEN)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Voxel physics prediction", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VOXEL_PHYSICS_PREDICTION)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Update trigger", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_UPDATE_TRIGGER)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugDrawSettings2.<>c <>9 = new MyGuiScreenDebugDrawSettings2.<>c();
        }
    }
}

