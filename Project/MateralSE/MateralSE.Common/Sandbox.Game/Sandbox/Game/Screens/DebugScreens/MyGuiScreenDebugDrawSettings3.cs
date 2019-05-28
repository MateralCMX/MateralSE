namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Definitions.GUI;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.GUI;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("VRage", "Debug draw settings 3")]
    internal class MyGuiScreenDebugDrawSettings3 : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugDrawSettings3() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        private static void ClearDecals(MyGuiControlButton button)
        {
            MyRenderProxy.ClearDecals();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugDrawSettings3";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Debug draw settings 3", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            Vector4? color = null;
            captionOffset = null;
            this.AddCheckBox("Debug decals", MyRenderProxy.Settings.DebugDrawDecals, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DebugDrawDecals = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Decals default material", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.ENABLE_USE_DEFAULT_DAMAGE_DECAL)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Clear decals"), new Action<MyGuiControlButton>(MyGuiScreenDebugDrawSettings3.ClearDecals), null, color, captionOffset, true, true);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Debug Particles", (Func<bool>) (() => MyDebugDrawSettings.DEBUG_DRAW_PARTICLES), (Action<bool>) (x => (MyDebugDrawSettings.DEBUG_DRAW_PARTICLES = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Entity update statistics", (Func<bool>) (() => MyDebugDrawSettings.DEBUG_DRAW_ENTITY_STATISTICS), (Action<bool>) (x => (MyDebugDrawSettings.DEBUG_DRAW_ENTITY_STATISTICS = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("3rd person camera", (Func<bool>) (() => ((MyThirdPersonSpectator.Static != null) && MyThirdPersonSpectator.Static.EnableDebugDraw)), delegate (bool x) {
                if (MyThirdPersonSpectator.Static != null)
                {
                    MyThirdPersonSpectator.Static.EnableDebugDraw = x;
                }
            }, true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Inverse kinematics", (Func<bool>) (() => MyDebugDrawSettings.DEBUG_DRAW_INVERSE_KINEMATICS), (Action<bool>) (x => (MyDebugDrawSettings.DEBUG_DRAW_INVERSE_KINEMATICS = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Character tools", (Func<bool>) (() => MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_TOOLS), (Action<bool>) (x => (MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_TOOLS = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Force tools 1st person", (Func<bool>) (() => MyFakes.FORCE_CHARTOOLS_1ST_PERSON), (Action<bool>) (x => (MyFakes.FORCE_CHARTOOLS_1ST_PERSON = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("HUD", (Func<bool>) (() => MyDebugDrawSettings.DEBUG_DRAW_HUD), (Action<bool>) (x => (MyDebugDrawSettings.DEBUG_DRAW_HUD = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Server Messages (Performance Warnings)", (Func<bool>) (() => MyDebugDrawSettings.DEBUG_DRAW_SERVER_WARNINGS), (Action<bool>) (x => (MyDebugDrawSettings.DEBUG_DRAW_SERVER_WARNINGS = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Network Sync", (Func<bool>) (() => MyDebugDrawSettings.DEBUG_DRAW_NETWORK_SYNC), (Action<bool>) (x => (MyDebugDrawSettings.DEBUG_DRAW_NETWORK_SYNC = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Grid hierarchy", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_GRID_HIERARCHY)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddButton("Reload HUD", x => ReloadHud(), null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Turret Target Prediction", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyLargeTurretBase.DEBUG_DRAW_TARGET_PREDICTION)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Projectile Trajectory", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyProjectile.DEBUG_DRAW_PROJECTILE_TRAJECTORY)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Missile Trajectory", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyMissile.DEBUG_DRAW_MISSILE_TRAJECTORY)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Show Joystick Controls", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_JOYSTICK_CONTROL_HINTS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw Gui Control Borders", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyGuiControlBase.DEBUG_CONTROL_BORDERS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Voxel - full cells", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VOXEL_FULLCELLS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Voxel - content micro nodes", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VOXEL_CONTENT_MICRONODES)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Voxel - content micro nodes scaled", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VOXEL_CONTENT_MICRONODES_SCALED)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Voxel - content macro nodes", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VOXEL_CONTENT_MACRONODES)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Voxel - content macro leaves", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VOXEL_CONTENT_MACROLEAVES)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Voxel - content macro scaled", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VOXEL_CONTENT_MACRO_SCALED)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Voxel - materials macro nodes", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VOXEL_MATERIALS_MACRONODES)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Voxel - materials macro leaves", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VOXEL_MATERIALS_MACROLEAVES)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
        }

        private static bool ReloadHud()
        {
            MyObjectBuilder_Definitions definitions;
            MyHudDefinition hudDefinition = MyHud.HudDefinition;
            MyGuiTextureAtlasDefinition definition = MyDefinitionManagerBase.Static.GetDefinition<MyGuiTextureAtlasDefinition>(MyStringHash.GetOrCompute("Base"));
            if (!MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Definitions>(hudDefinition.Context.CurrentFile, out definitions))
            {
                MyAPIGateway.Utilities.ShowNotification("Failed to load Hud.sbc!", 0xbb8, "Red");
                return false;
            }
            hudDefinition.Init(definitions.Definitions[0], hudDefinition.Context);
            if (!MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Definitions>(definition.Context.CurrentFile, out definitions))
            {
                MyAPIGateway.Utilities.ShowNotification("Failed to load GuiTextures.sbc!", 0xbb8, "Red");
                return false;
            }
            definition.Init(definitions.Definitions[0], definition.Context);
            MyGuiTextures.Static.Reload();
            MyScreenManager.CloseScreen(MyPerGameSettings.GUI.HUDScreen);
            MyScreenManager.AddScreen(Activator.CreateInstance(MyPerGameSettings.GUI.HUDScreen) as MyGuiScreenBase);
            return true;
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderProxy.SetSettingsDirty();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugDrawSettings3.<>c <>9 = new MyGuiScreenDebugDrawSettings3.<>c();
            public static Action<MyGuiControlCheckbox> <>9__2_0;
            public static Func<bool> <>9__2_2;
            public static Action<bool> <>9__2_3;
            public static Func<bool> <>9__2_4;
            public static Action<bool> <>9__2_5;
            public static Func<bool> <>9__2_6;
            public static Action<bool> <>9__2_7;
            public static Func<bool> <>9__2_8;
            public static Action<bool> <>9__2_9;
            public static Func<bool> <>9__2_10;
            public static Action<bool> <>9__2_11;
            public static Func<bool> <>9__2_12;
            public static Action<bool> <>9__2_13;
            public static Func<bool> <>9__2_14;
            public static Action<bool> <>9__2_15;
            public static Func<bool> <>9__2_16;
            public static Action<bool> <>9__2_17;
            public static Func<bool> <>9__2_18;
            public static Action<bool> <>9__2_19;
            public static Action<MyGuiControlButton> <>9__2_21;

            internal void <RecreateControls>b__2_0(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DebugDrawDecals = x.IsChecked;
            }

            internal bool <RecreateControls>b__2_10() => 
                MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_TOOLS;

            internal void <RecreateControls>b__2_11(bool x)
            {
                MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_TOOLS = x;
            }

            internal bool <RecreateControls>b__2_12() => 
                MyFakes.FORCE_CHARTOOLS_1ST_PERSON;

            internal void <RecreateControls>b__2_13(bool x)
            {
                MyFakes.FORCE_CHARTOOLS_1ST_PERSON = x;
            }

            internal bool <RecreateControls>b__2_14() => 
                MyDebugDrawSettings.DEBUG_DRAW_HUD;

            internal void <RecreateControls>b__2_15(bool x)
            {
                MyDebugDrawSettings.DEBUG_DRAW_HUD = x;
            }

            internal bool <RecreateControls>b__2_16() => 
                MyDebugDrawSettings.DEBUG_DRAW_SERVER_WARNINGS;

            internal void <RecreateControls>b__2_17(bool x)
            {
                MyDebugDrawSettings.DEBUG_DRAW_SERVER_WARNINGS = x;
            }

            internal bool <RecreateControls>b__2_18() => 
                MyDebugDrawSettings.DEBUG_DRAW_NETWORK_SYNC;

            internal void <RecreateControls>b__2_19(bool x)
            {
                MyDebugDrawSettings.DEBUG_DRAW_NETWORK_SYNC = x;
            }

            internal bool <RecreateControls>b__2_2() => 
                MyDebugDrawSettings.DEBUG_DRAW_PARTICLES;

            internal void <RecreateControls>b__2_21(MyGuiControlButton x)
            {
                MyGuiScreenDebugDrawSettings3.ReloadHud();
            }

            internal void <RecreateControls>b__2_3(bool x)
            {
                MyDebugDrawSettings.DEBUG_DRAW_PARTICLES = x;
            }

            internal bool <RecreateControls>b__2_4() => 
                MyDebugDrawSettings.DEBUG_DRAW_ENTITY_STATISTICS;

            internal void <RecreateControls>b__2_5(bool x)
            {
                MyDebugDrawSettings.DEBUG_DRAW_ENTITY_STATISTICS = x;
            }

            internal bool <RecreateControls>b__2_6() => 
                ((MyThirdPersonSpectator.Static != null) && MyThirdPersonSpectator.Static.EnableDebugDraw);

            internal void <RecreateControls>b__2_7(bool x)
            {
                if (MyThirdPersonSpectator.Static != null)
                {
                    MyThirdPersonSpectator.Static.EnableDebugDraw = x;
                }
            }

            internal bool <RecreateControls>b__2_8() => 
                MyDebugDrawSettings.DEBUG_DRAW_INVERSE_KINEMATICS;

            internal void <RecreateControls>b__2_9(bool x)
            {
                MyDebugDrawSettings.DEBUG_DRAW_INVERSE_KINEMATICS = x;
            }
        }
    }
}

