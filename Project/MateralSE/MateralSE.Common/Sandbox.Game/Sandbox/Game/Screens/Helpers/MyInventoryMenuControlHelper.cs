namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;

    public class MyInventoryMenuControlHelper : MyAbstractControlMenuItem
    {
        private IMyControllableEntity m_entity;

        public MyInventoryMenuControlHelper() : base(MyControlsSpace.INVENTORY, MySupportKeysEnum.NONE)
        {
        }

        public override void Activate()
        {
            MyScreenManager.CloseScreen(typeof(MyGuiScreenControlMenu));
            MyGuiScreenHudSpace.Static.HideScreen();
            this.m_entity.ShowInventory();
        }

        public void SetEntity(IMyControllableEntity entity)
        {
            this.m_entity = entity;
        }

        public override string Label =>
            MyTexts.GetString(MySpaceTexts.ControlMenuItemLabel_OpenInventory);
    }
}

