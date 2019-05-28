namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Entities.Character;
    using System;
    using VRage;

    public class MySuicideControlHelper : MyAbstractControlMenuItem
    {
        private MyCharacter m_character;

        public MySuicideControlHelper() : base(MyControlsSpace.SUICIDE, MySupportKeysEnum.NONE)
        {
        }

        public override void Activate()
        {
            this.m_character.Die();
        }

        public void SetCharacter(MyCharacter character)
        {
            this.m_character = character;
        }

        public override string Label =>
            MyTexts.GetString(MyCommonTexts.ControlMenuItemLabel_CommitSuicide);
    }
}

