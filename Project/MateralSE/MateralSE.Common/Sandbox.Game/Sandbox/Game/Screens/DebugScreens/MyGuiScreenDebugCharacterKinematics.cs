namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRageMath;

    [MyDebugScreen("VRage", "Character kinematics")]
    internal class MyGuiScreenDebugCharacterKinematics : MyGuiScreenDebugBase
    {
        public bool updating;

        public MyGuiScreenDebugCharacterKinematics() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        private void activateRagdollAction(MyGuiControlButton obj)
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if (this.PlayerRagdollMapper == null)
            {
                MyCharacterRagdollComponent component = new MyCharacterRagdollComponent();
                localCharacter.Components.Add<MyCharacterRagdollComponent>(component);
                component.InitRagdoll();
            }
            if (this.PlayerRagdollMapper.IsActive)
            {
                this.PlayerRagdollMapper.Deactivate();
            }
            localCharacter.Physics.SwitchToRagdollMode(false, 1);
            this.PlayerRagdollMapper.Activate();
            this.PlayerRagdollMapper.SetRagdollToKeyframed();
            localCharacter.Physics.Ragdoll.DisableConstraints();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugCharacterKinematics";

        private void killRagdollAction(MyGuiControlButton obj)
        {
            MyFakes.CHARACTER_CAN_DIE_EVEN_IN_CREATIVE_MODE = true;
            MySession.Static.LocalCharacter.DoDamage(1000000f, MyDamageType.Suicide, true, 0L);
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Character kinematics debug draw", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            Vector4? color = null;
            captionOffset = null;
            base.AddCheckBox("Enable permanent IK/Ragdoll simulation ", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.ENABLE_PERMANENT_SIMULATIONS_COMPUTATION)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw Ragdoll Rig Pose", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_RAGDOLL_ORIGINAL_RIG)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw Bones Rig Pose", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_RAGDOLL_BONES_ORIGINAL_RIG)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw Ragdoll Pose", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_RAGDOLL_POSE)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw Bones", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_RAGDOLL_COMPUTED_BONES)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw bones intended transforms", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_RAGDOLL_BONES_DESIRED)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Draw Hip Ragdoll and Char. Position", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_RAGDOLL_HIPPOSITIONS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Enable Ragdoll", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyPerGameSettings.EnableRagdollModels)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Enable Bones Translation", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.ENABLE_RAGDOLL_BONES_TRANSLATION)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            base.AddSlider("Animation weighting", 0f, 5f, null, MemberHelper.GetMember<float>(Expression.Lambda<Func<float>>(Expression.Field(null, fieldof(MyFakes.RAGDOLL_ANIMATION_WEIGHTING)), Array.Empty<ParameterExpression>())), color);
            color = null;
            base.AddSlider("Ragdoll gravity multiplier", 0f, 50f, null, MemberHelper.GetMember<float>(Expression.Lambda<Func<float>>(Expression.Field(null, fieldof(MyFakes.RAGDOLL_GRAVITY_MULTIPLIER)), Array.Empty<ParameterExpression>())), color);
            StringBuilder text = new StringBuilder("Kill Ragdoll");
            color = null;
            captionOffset = null;
            base.AddButton(text, new Action<MyGuiControlButton>(this.killRagdollAction), null, color, captionOffset, true, true);
            StringBuilder builder2 = new StringBuilder("Activate Ragdoll");
            color = null;
            captionOffset = null;
            base.AddButton(builder2, new Action<MyGuiControlButton>(this.activateRagdollAction), null, color, captionOffset, true, true);
            StringBuilder builder3 = new StringBuilder("Switch to Dynamic / Keyframed");
            color = null;
            captionOffset = null;
            base.AddButton(builder3, new Action<MyGuiControlButton>(this.switchRagdoll), null, color, captionOffset, true, true);
        }

        private void switchRagdoll(MyGuiControlButton obj)
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if (this.PlayerRagdollMapper.IsActive)
            {
                if (localCharacter.Physics.Ragdoll.IsKeyframed)
                {
                    localCharacter.Physics.Ragdoll.EnableConstraints();
                    this.PlayerRagdollMapper.SetRagdollToDynamic();
                }
                else
                {
                    localCharacter.Physics.Ragdoll.DisableConstraints();
                    this.PlayerRagdollMapper.SetRagdollToKeyframed();
                }
            }
        }

        public MyRagdollMapper PlayerRagdollMapper
        {
            get
            {
                MyCharacterRagdollComponent component = MySession.Static.LocalCharacter.Components.Get<MyCharacterRagdollComponent>();
                return ((component != null) ? component.RagdollMapper : null);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugCharacterKinematics.<>c <>9 = new MyGuiScreenDebugCharacterKinematics.<>c();
        }
    }
}

