namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Screens;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;
    using VRage.Utils;

    public class MyUseTerminalControlHelper : MyAbstractControlMenuItem
    {
        private MyCharacter m_character;
        private string m_label;

        public MyUseTerminalControlHelper() : base(MyControlsSpace.TERMINAL, MySupportKeysEnum.NONE)
        {
        }

        public override void Activate()
        {
            MyScreenManager.CloseScreen(typeof(MyGuiScreenControlMenu));
            this.m_character.UseTerminal();
        }

        public void SetCharacter(MyCharacter character)
        {
            this.m_character = character;
        }

        public void SetLabel(MyStringId id)
        {
            this.m_label = MyTexts.GetString(id);
        }

        public override string Label =>
            this.m_label;
    }
}

