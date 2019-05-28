namespace Sandbox.Game.Components
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Character;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Animations;

    internal class MyDebugRenderComponentCharacter : MyDebugRenderComponent
    {
        private MyCharacter m_character;
        private List<Matrix> m_simulatedBonesDebugDraw;
        private List<Matrix> m_simulatedBonesAbsoluteDebugDraw;
        private long m_counter;
        private float m_lastDamage;
        private float m_lastCharacterVelocity;

        public MyDebugRenderComponentCharacter(MyCharacter character) : base(character)
        {
            this.m_simulatedBonesDebugDraw = new List<Matrix>();
            this.m_simulatedBonesAbsoluteDebugDraw = new List<Matrix>();
            this.m_character = character;
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_MISC && (this.m_character.CurrentWeapon != null))
            {
                MyRenderProxy.DebugDrawAxis(((MyEntity) this.m_character.CurrentWeapon).WorldMatrix, 1.4f, false, false, false);
                MyRenderProxy.DebugDrawText3D(((MyEntity) this.m_character.CurrentWeapon).WorldMatrix.Translation, "Weapon", Color.White, 0.7f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                MyRenderProxy.DebugDrawSphere((this.m_character.AnimationController.CharacterBones[this.m_character.WeaponBone].AbsoluteTransform * this.m_character.PositionComp.WorldMatrix).Translation, 0.02f, Color.White, 1f, false, false, true, false);
                MyRenderProxy.DebugDrawText3D((this.m_character.AnimationController.CharacterBones[this.m_character.WeaponBone].AbsoluteTransform * this.m_character.PositionComp.WorldMatrix).Translation, "Weapon Bone", Color.White, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_MISC && (this.m_character.IsUsing != null))
            {
                Matrix worldMatrix = (Matrix) this.m_character.IsUsing.WorldMatrix;
                worldMatrix.Translation = Vector3.Zero;
                worldMatrix *= Matrix.CreateFromAxisAngle(worldMatrix.Up, 3.141593f);
                worldMatrix.Translation = (((Vector3) (this.m_character.IsUsing.PositionComp.GetPosition() - ((this.m_character.IsUsing.WorldMatrix.Up * MyDefinitionManager.Static.GetCubeSize(MyCubeSize.Large)) / 2.0))) + (worldMatrix.Up * 0.28f)) - (worldMatrix.Forward * 0.22f);
                MyRenderProxy.DebugDrawAxis(worldMatrix, 1.4f, false, false, false);
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_SUIT_BATTERY_CAPACITY)
            {
                MatrixD worldMatrix = this.m_character.PositionComp.WorldMatrix;
                MyRenderProxy.DebugDrawText3D(worldMatrix.Translation + (2.0 * worldMatrix.Up), $"{this.m_character.SuitBattery.ResourceSource.RemainingCapacity} MWh", Color.White, 1f, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
            }
            this.m_simulatedBonesDebugDraw.Clear();
            this.m_simulatedBonesAbsoluteDebugDraw.Clear();
            if (MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_BONES)
            {
                this.m_character.AnimationController.UpdateTransformations();
                for (int i = 0; i < this.m_character.AnimationController.CharacterBones.Length; i++)
                {
                    MyCharacterBone bone = this.m_character.AnimationController.CharacterBones[i];
                    if (bone.Parent != null)
                    {
                        MatrixD matrix = (Matrix.CreateScale((float) 0.1f) * bone.AbsoluteTransform) * this.m_character.PositionComp.WorldMatrix;
                        Vector3 translation = (Vector3) matrix.Translation;
                        Vector3 pointFrom = (Vector3) (bone.Parent.AbsoluteTransform * this.m_character.PositionComp.WorldMatrix).Translation;
                        MyRenderProxy.DebugDrawLine3D(pointFrom, translation, Color.White, Color.White, false, false);
                        MyRenderProxy.DebugDrawText3D((pointFrom + translation) * 0.5f, bone.Name + " (" + i.ToString() + ")", Color.Red, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                        MyRenderProxy.DebugDrawAxis(matrix, 0.1f, false, false, false);
                    }
                }
            }
        }
    }
}

