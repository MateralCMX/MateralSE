namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Graphics.GUI;
    using System;

    internal class MyAnselGuiScreen : MyGuiScreenBase
    {
        public MyAnselGuiScreen() : base(nullable, nullable2, nullable, true, null, 0f, 0f)
        {
            Vector2? nullable = null;
            nullable = null;
            base.DrawMouseCursor = false;
            base.m_canShareInput = false;
        }

        public override string GetFriendlyName() => 
            "MyAnselGuiScreen";
    }
}

