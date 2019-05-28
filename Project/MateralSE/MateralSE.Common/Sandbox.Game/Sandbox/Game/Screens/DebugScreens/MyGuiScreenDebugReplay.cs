namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Game.Gui;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Graphics.GUI;
    using System;
    using VRageMath;

    [MyDebugScreen("Game", "Replay")]
    internal class MyGuiScreenDebugReplay : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugReplay() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugReplay";

        private void OnAddNewCharacterClick(MyGuiControlButton button)
        {
            MyCharacterInputComponent.SpawnCharacter(null);
        }

        private void OnClearClick(MyGuiControlButton button)
        {
            MySessionComponentReplay.Static.DeleteRecordings();
        }

        private void OnStartRecordingClick(MyGuiControlButton button)
        {
            MySessionComponentReplay.Static.StartRecording();
            MySessionComponentReplay.Static.StartReplay();
        }

        private void OnStartReplayClick(MyGuiControlButton button)
        {
            MySessionComponentReplay.Static.StartReplay();
        }

        private void OnStopRecordingClick(MyGuiControlButton button)
        {
            MySessionComponentReplay.Static.StopRecording();
        }

        private void OnStopReplayClick(MyGuiControlButton button)
        {
            MySessionComponentReplay.Static.StopReplay();
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.13f);
            Vector4? textColor = null;
            Vector2? size = null;
            base.AddButton("Record + replay", new Action<MyGuiControlButton>(this.OnStartRecordingClick), null, textColor, size);
            textColor = null;
            size = null;
            base.AddButton("Stop recording", new Action<MyGuiControlButton>(this.OnStopRecordingClick), null, textColor, size);
            textColor = null;
            size = null;
            base.AddButton("Replay", new Action<MyGuiControlButton>(this.OnStartReplayClick), null, textColor, size);
            textColor = null;
            size = null;
            base.AddButton("Clear all", new Action<MyGuiControlButton>(this.OnClearClick), null, textColor, size);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            textColor = null;
            size = null;
            base.AddButton("Add new character", new Action<MyGuiControlButton>(this.OnAddNewCharacterClick), null, textColor, size);
        }
    }
}

