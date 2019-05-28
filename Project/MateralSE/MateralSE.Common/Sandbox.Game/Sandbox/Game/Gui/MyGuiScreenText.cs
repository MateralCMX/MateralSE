namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenText : MyGuiScreenBase
    {
        private static readonly Style[] m_styles = new Style[MyUtils.GetMaxValueFromEnum<MyMessageBoxStyleEnum>() + 1];
        private static Vector2 m_defaultWindowSize = new Vector2(0.6f, 0.7f);
        private static Vector2 m_defaultDescSize = new Vector2(0.5f, 0.44f);
        private Vector2 m_windowSize;
        protected Vector2 m_descSize;
        private string m_currentObjectivePrefix;
        private StringBuilder m_okButtonCaption;
        private string m_missionTitle;
        private string m_currentObjective;
        protected string m_description;
        protected bool m_enableEdit;
        protected MyGuiControlLabel m_titleLabel;
        private MyGuiControlLabel m_currentObjectiveLabel;
        protected MyGuiControlMultilineText m_descriptionBox;
        protected MyGuiControlButton m_okButton;
        protected MyGuiControlCompositePanel m_descriptionBackgroundPanel;
        private Action<VRage.Game.ModAPI.ResultEnum> m_resultCallback;
        private VRage.Game.ModAPI.ResultEnum m_screenResult;
        private Style m_style;

        static MyGuiScreenText()
        {
            Style style1 = new Style();
            style1.BackgroundTextureName = MyGuiConstants.TEXTURE_SCREEN_BACKGROUND_RED.Texture;
            style1.CaptionFont = "White";
            style1.TextFont = "White";
            style1.ButtonStyle = MyGuiControlButtonStyleEnum.Red;
            style1.ShowBackgroundPanel = false;
            m_styles[0] = style1;
            Style style2 = new Style();
            style2.BackgroundTextureName = MyGuiConstants.TEXTURE_SCREEN_BACKGROUND.Texture;
            style2.CaptionFont = "White";
            style2.TextFont = "Blue";
            style2.ButtonStyle = MyGuiControlButtonStyleEnum.Default;
            style2.ShowBackgroundPanel = true;
            m_styles[1] = style2;
        }

        public MyGuiScreenText(string missionTitle = null, string currentObjectivePrefix = null, string currentObjective = null, string description = null, Action<VRage.Game.ModAPI.ResultEnum> resultCallback = null, string okButtonCaption = null, Vector2? windowSize = new Vector2?(), Vector2? descSize = new Vector2?(), bool editEnabled = false, bool canHideOthers = true, bool enableBackgroundFade = false, MyMissionScreenStyleEnum style = 1) : this(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2?((windowSize != null) ? windowSize.Value : m_defaultWindowSize), false, null, 0f, 0f)
        {
            this.m_currentObjectivePrefix = "Current objective: ";
            this.m_missionTitle = "Mission Title";
            this.m_currentObjective = "";
            this.m_description = "";
            this.m_screenResult = VRage.Game.ModAPI.ResultEnum.CANCEL;
            this.m_style = m_styles[(int) style];
            this.m_enableEdit = editEnabled;
            this.m_descSize = (descSize != null) ? descSize.Value : m_defaultDescSize;
            this.m_windowSize = (windowSize != null) ? windowSize.Value : m_defaultWindowSize;
            this.m_missionTitle = missionTitle ?? this.m_missionTitle;
            this.m_currentObjectivePrefix = currentObjectivePrefix ?? this.m_currentObjectivePrefix;
            this.m_currentObjective = currentObjective ?? this.m_currentObjective;
            this.m_description = description ?? this.m_description;
            this.m_resultCallback = resultCallback;
            this.m_okButtonCaption = (okButtonCaption != null) ? new StringBuilder(okButtonCaption) : MyTexts.Get(MyCommonTexts.Ok);
            base.m_closeOnEsc = true;
            this.RecreateControls(true);
            this.m_titleLabel.Font = this.m_style.CaptionFont;
            this.m_currentObjectiveLabel.Font = this.m_style.CaptionFont;
            this.m_descriptionBox.Font = this.m_style.TextFont;
            base.m_backgroundTexture = this.m_style.BackgroundTextureName;
            this.m_okButton.VisualStyle = this.m_style.ButtonStyle;
            this.m_descriptionBackgroundPanel.Visible = this.m_style.ShowBackgroundPanel;
            base.m_isTopScreen = false;
            base.m_isTopMostScreen = false;
            base.CanHideOthers = canHideOthers;
            base.EnabledBackgroundFade = enableBackgroundFade;
        }

        protected MyGuiControlCompositePanel AddCompositePanel(MyGuiCompositeTexture texture, Vector2 position, Vector2 size, MyGuiDrawAlignEnum panelAlign)
        {
            MyGuiControlCompositePanel panel1 = new MyGuiControlCompositePanel();
            panel1.BackgroundTexture = texture;
            MyGuiControlCompositePanel control = panel1;
            control.Position = position;
            control.Size = size;
            control.OriginAlign = panelAlign;
            this.Controls.Add(control);
            return control;
        }

        protected virtual MyGuiControlMultilineText AddMultilineText(Vector2? size = new Vector2?(), Vector2? offset = new Vector2?(), float textScale = 1f, bool selectable = false, MyGuiDrawAlignEnum textAlign = 0, MyGuiDrawAlignEnum textBoxAlign = 0)
        {
            int? nullable3;
            MyGuiBorderThickness? nullable4;
            Vector2 valueOrDefault;
            Vector2? nullable = size;
            if (nullable != null)
            {
                valueOrDefault = nullable.GetValueOrDefault();
            }
            else
            {
                Vector2? nullable2 = base.Size;
                valueOrDefault = (nullable2 != null) ? nullable2.GetValueOrDefault() : new Vector2(1.2f, 0.5f);
            }
            Vector2 vector = valueOrDefault;
            MyGuiControlMultilineText control = null;
            if (this.m_enableEdit)
            {
                nullable = offset;
                nullable3 = null;
                nullable4 = null;
                control = new MyGuiControlMultilineEditableText(new Vector2?((vector / 2f) + ((nullable != null) ? nullable.GetValueOrDefault() : Vector2.Zero)), new Vector2?(vector), new VRageMath.Vector4?(Color.White.ToVector4()), "White", 0.8f, textAlign, null, true, true, textBoxAlign, nullable3, null, nullable4);
            }
            else
            {
                nullable = offset;
                nullable3 = null;
                nullable4 = null;
                control = new MyGuiControlMultilineText(new Vector2?((vector / 2f) + ((nullable != null) ? nullable.GetValueOrDefault() : Vector2.Zero)), new Vector2?(vector), new VRageMath.Vector4?(Color.White.ToVector4()), "White", 0.8f, textAlign, null, true, true, textBoxAlign, nullable3, this.m_enableEdit, false, null, nullable4);
            }
            this.Controls.Add(control);
            return control;
        }

        public void AppendTextToDescription(string text, string font = "White", float scale = 1f)
        {
            this.m_description = this.m_description + text;
            this.m_descriptionBox.AppendText(text, font, scale, VRageMath.Vector4.One);
        }

        public void AppendTextToDescription(string text, VRageMath.Vector4 color, string font = "White", float scale = 1f)
        {
            this.m_description = this.m_description + text;
            this.m_descriptionBox.AppendText(text, font, scale, color);
        }

        protected void CallResultCallback(VRage.Game.ModAPI.ResultEnum result)
        {
            if (this.m_resultCallback != null)
            {
                this.m_resultCallback(result);
            }
        }

        protected override void Canceling()
        {
            base.Canceling();
            this.m_screenResult = VRage.Game.ModAPI.ResultEnum.CANCEL;
        }

        public override bool CloseScreen()
        {
            this.CallResultCallback(this.m_screenResult);
            return base.CloseScreen();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenMission";

        private void OkButtonClicked(MyGuiControlButton button)
        {
            this.m_screenResult = VRage.Game.ModAPI.ResultEnum.OK;
            this.CloseScreen();
        }

        public override void RecreateControls(bool constructor)
        {
            Vector2 vector = new Vector2(0f, -0.3f);
            Vector2 descSize = this.m_descSize;
            Vector2 position = new Vector2(-descSize.X / 2f, vector.Y + 0.1f);
            Vector2 vector1 = new Vector2(0.2f, 0.3f);
            Vector2 vector6 = new Vector2(0.32f, 0f);
            Vector2 vector4 = new Vector2(0.005f, 0f);
            Vector2 vector5 = new Vector2(0f, vector.Y + 0.05f);
            base.RecreateControls(constructor);
            base.CloseButtonEnabled = true;
            VRageMath.Vector4? colorMask = null;
            int? buttonIndex = null;
            this.m_okButton = new MyGuiControlButton(new Vector2(0f, 0.29f), MyGuiControlButtonStyleEnum.Default, new Vector2?(MyGuiConstants.BACK_BUTTON_SIZE), colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, this.m_okButtonCaption, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OkButtonClicked), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_okButton);
            Vector2? size = null;
            colorMask = null;
            this.m_titleLabel = new MyGuiControlLabel(new Vector2?(vector), size, this.m_missionTitle, colorMask, 1.5f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.Controls.Add(this.m_titleLabel);
            size = null;
            colorMask = null;
            this.m_currentObjectiveLabel = new MyGuiControlLabel(new Vector2?(vector5), size, null, colorMask, 1f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.Controls.Add(this.m_currentObjectiveLabel);
            this.SetCurrentObjective(this.m_currentObjective);
            this.m_descriptionBackgroundPanel = this.AddCompositePanel(MyGuiConstants.TEXTURE_RECTANGLE_DARK, position, descSize, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            size = new Vector2?(position + vector4);
            this.m_descriptionBox = this.AddMultilineText(new Vector2?(descSize), size, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            this.m_descriptionBox.Text = new StringBuilder(this.m_description);
        }

        public void SetCurrentObjective(string objective)
        {
            this.m_currentObjective = objective;
            this.m_currentObjectiveLabel.Text = this.m_currentObjectivePrefix + this.m_currentObjective;
        }

        public void SetCurrentObjectivePrefix(string prefix)
        {
            this.m_currentObjectivePrefix = prefix;
        }

        public void SetDescription(string desc)
        {
            this.m_description = desc;
            this.m_descriptionBox.Clear();
            this.m_descriptionBox.Text = new StringBuilder(this.m_description);
        }

        public void SetOkButtonCaption(string caption)
        {
            this.m_okButtonCaption = new StringBuilder(caption);
        }

        public void SetTitle(string title)
        {
            this.m_missionTitle = title;
            this.m_titleLabel.Text = title;
        }

        public MyGuiControlMultilineText Description =>
            this.m_descriptionBox;

        public class Style
        {
            public string BackgroundTextureName;
            public string CaptionFont;
            public string TextFont;
            public MyGuiControlButtonStyleEnum ButtonStyle;
            public bool ShowBackgroundPanel;
        }
    }
}

