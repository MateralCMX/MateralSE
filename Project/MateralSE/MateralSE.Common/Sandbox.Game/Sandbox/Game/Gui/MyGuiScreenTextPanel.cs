namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;

    public class MyGuiScreenTextPanel : MyGuiScreenText
    {
        public MyGuiScreenTextPanel(string missionTitle = null, string currentObjectivePrefix = null, string currentObjective = null, string description = null, Action<ResultEnum> resultCallback = null, Action saveCodeCallback = null, string okButtonCaption = null, bool editable = false, MyGuiScreenBase previousScreen = null) : base(missionTitle, currentObjectivePrefix, currentObjective, description, resultCallback, okButtonCaption, nullable, nullable, editable, true, false, MyMissionScreenStyleEnum.BLUE)
        {
            Vector2? nullable = null;
            nullable = null;
            base.CanHideOthers = editable;
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            MyGuiScreenGamePlay.ActiveGameplayScreen = null;
        }
    }
}

