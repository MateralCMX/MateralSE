namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Game.Localization;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Library;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenBriefing : MyGuiScreenBase
    {
        public static MyGuiScreenBriefing Static;
        private MyGuiControlLabel m_mainLabel;
        private MyGuiControlMultilineText m_descriptionBox;
        protected MyGuiControlButton m_okButton;

        public MyGuiScreenBriefing() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2?(new Vector2(1620f, 1125f) / MyGuiConstants.GUI_OPTIMAL_SIZE), false, null, 0f, 0f)
        {
            Static = this;
            this.RecreateControls(true);
            this.FillData();
        }

        private void FillData()
        {
            this.m_descriptionBox.Text.Clear().Append(MySession.Static.GetWorld(true).Checkpoint.Briefing).Append(MyEnvironment.NewLine).Append(MyEnvironment.NewLine);
            this.m_descriptionBox.Text.Append(MyEnvironment.NewLine).Append(MySessionComponentMissionTriggers.GetProgress(MySession.Static.LocalHumanPlayer));
            this.m_descriptionBox.RefreshText(false);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenBriefing";

        protected override void OnClosed()
        {
            base.OnClosed();
        }

        protected virtual void OnOkClicked(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            MyLayoutTable table = new MyLayoutTable(this);
            table.SetColumnWidthsNormalized(new float[] { 50f, 250f, 150f, 250f, 50f });
            table.SetRowHeightsNormalized(new float[] { 50f, 450f, 30f, 50f });
            Vector2? position = null;
            position = null;
            VRageMath.Vector4? colorMask = null;
            this.m_mainLabel = new MyGuiControlLabel(position, position, MyTexts.GetString(MySpaceTexts.GuiScenarioDescription), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            table.AddWithSize(this.m_mainLabel, MyAlignH.Left, MyAlignV.Center, 0, 1, 1, 3);
            colorMask = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            this.m_descriptionBox = new MyGuiControlMultilineText(new Vector2(0f, 0f), new Vector2(0.2f, 0.2f), colorMask, "Blue", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, visibleLinesCount, false, false, null, textPadding);
            table.AddWithSize(this.m_descriptionBox, MyAlignH.Left, MyAlignV.Top, 1, 1, 1, 3);
            position = null;
            StringBuilder text = MyTexts.Get(MyCommonTexts.Ok);
            colorMask = null;
            visibleLinesCount = null;
            this.m_okButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(new Vector2(200f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkClicked), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            table.AddWithSize(this.m_okButton, MyAlignH.Left, MyAlignV.Top, 2, 2, 1, 1);
        }

        public override bool Update(bool hasFocus) => 
            base.Update(hasFocus);

        public string Briefing
        {
            set => 
                (this.m_descriptionBox.Text = new StringBuilder(value));
        }
    }
}

