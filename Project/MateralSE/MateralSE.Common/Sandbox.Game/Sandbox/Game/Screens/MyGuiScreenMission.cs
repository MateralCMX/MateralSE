namespace Sandbox.Game.Screens
{
    using Sandbox.Game.Gui;
    using System;
    using System.Runtime.InteropServices;

    public class MyGuiScreenMission : MyGuiScreenText
    {
        public MyGuiScreenMission(string missionTitle = null, string currentObjectivePrefix = null, string currentObjective = null, string description = null, Action<ResultEnum> resultCallback = null, string okButtonCaption = null, Vector2? windowSize = new Vector2?(), Vector2? descSize = new Vector2?(), bool editEnabled = false, bool canHideOthers = true, bool enableBackgroundFade = false, MyMissionScreenStyleEnum style = 1) : base(missionTitle, currentObjectivePrefix, currentObjective, description, resultCallback, okButtonCaption, windowSize, descSize, editEnabled, canHideOthers, enableBackgroundFade, style)
        {
        }

        public override bool Draw() => 
            base.Draw();

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
        }
    }
}

