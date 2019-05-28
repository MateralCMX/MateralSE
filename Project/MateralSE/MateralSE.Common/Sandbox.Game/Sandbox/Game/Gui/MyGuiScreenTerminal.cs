namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GUI.HudViewers;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Replication;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.Screens.Terminal;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using Sandbox.Graphics.GUI.IME;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Input;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenTerminal : MyGuiScreenBase
    {
        private MyGuiControlTabControl m_terminalTabs;
        private MyGuiControlParent m_propertiesTopMenuParent;
        private MyGuiControlParent m_propertiesTableParent;
        private MyTerminalControlPanel m_controllerControlPanel;
        private MyTerminalInventoryController m_controllerInventory;
        private MyTerminalProductionController m_controllerProduction;
        private MyTerminalInfoController m_controllerInfo;
        private MyTerminalFactionController m_controllerFactions;
        private MyTerminalPropertiesController m_controllerProperties;
        private MyTerminalChatController m_controllerChat;
        private MyTerminalGpsController m_controllerGps;
        private MyGridColorHelper m_colorHelper;
        private MyGuiControlLabel m_terminalNotConnected;
        private MyGuiControlSearchBox m_functionalBlockSearchBox;
        private bool m_autoSelectControlSearch;
        private MyCharacter m_user;
        private static VRage.Game.Entity.MyEntity m_interactedEntity;
        private static VRage.Game.Entity.MyEntity m_openInventoryInteractedEntity;
        private MyTerminalPageEnum m_initialPage;
        private Dictionary<long, Action<long>> m_requestedEntities;
        private static Action<VRage.Game.Entity.MyEntity> m_closeHandler;
        public static bool IsRemote;
        private static bool m_screenOpen;
        private bool m_connected;
        private static MyGuiScreenTerminal m_instance;

        public static  event MyGuiScreenBase.ScreenHandler ClosedCallback
        {
            add
            {
                m_instance.Closed += value;
            }
            remove
            {
                m_instance.Closed -= value;
            }
        }

        private MyGuiScreenTerminal() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(1.0157f, 0.9172f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.m_autoSelectControlSearch = true;
            this.m_requestedEntities = new Dictionary<long, Action<long>>();
            this.m_connected = true;
            base.EnabledBackgroundFade = true;
            m_closeHandler = new Action<VRage.Game.Entity.MyEntity>(this.OnInteractedClose);
            this.m_colorHelper = new MyGridColorHelper();
        }

        public static void ChangeInteractedEntity(VRage.Game.Entity.MyEntity interactedEntity, bool isRemote)
        {
            IsRemote = isRemote;
            InteractedEntity = interactedEntity;
        }

        public override bool CloseScreen()
        {
            if (!base.CloseScreen())
            {
                return false;
            }
            if (m_interactedEntity != null)
            {
                m_interactedEntity.OnClose -= m_closeHandler;
            }
            return true;
        }

        private static void CreateAntennaSlider(MyGuiControlTabPage infoPage, string labelText, string name, float startY)
        {
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2(-0.123f, startY), size, labelText, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER
            };
            infoPage.Controls.Add(control);
            size = null;
            colorMask = null;
            MyGuiControlLabel friendAntennaRangeValueLabel = new MyGuiControlLabel(new Vector2(-0.123f, startY + 0.09f), size, null, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER
            };
            infoPage.Controls.Add(friendAntennaRangeValueLabel);
            float? defaultValue = null;
            colorMask = null;
            MyGuiControlSlider slider = new MyGuiControlSlider(new Vector2(0.126f, startY + 0.05f), 0f, 1f, 0.29f, defaultValue, colorMask, null, 1, 0.8f, 0f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER,
                Name = name,
                Size = new Vector2(0.25f, 1f),
                MinValue = 0f,
                MaxValue = 1f
            };
            slider.DefaultValue = new float?(slider.MaxValue);
            slider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(slider.ValueChanged, s => friendAntennaRangeValueLabel.Text = MyValueFormatter.GetFormatedFloat(MyHudMarkerRender.Denormalize(s.Value), 0) + "m");
            slider.SliderClicked = new Func<MyGuiControlSlider, bool>(MyGuiScreenTerminal.OnAntennaSliderClicked);
            infoPage.Controls.Add(slider);
        }

        private void CreateChatPageControls(MyGuiControlTabPage chatPage)
        {
            chatPage.Name = "PageComms";
            chatPage.TextEnum = MySpaceTexts.TerminalTab_Chat;
            chatPage.TextScale = 0.7005405f;
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddVertical(new Vector2(-0.1435f, -0.333f), 0.676f, 0.002f, color);
            chatPage.Controls.Add(control);
            float x = -0.452f;
            float num2 = -x;
            float y = -0.332f;
            int num4 = 11;
            float num5 = 0.35f;
            float num6 = 0f;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(x + 0.01f, y + 0.005f);
            label1.Name = "PlayerLabel";
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MySpaceTexts.TerminalTab_PlayersTableLabel);
            MyGuiControlLabel label = label1;
            color = null;
            MyGuiControlPanel panel1 = new MyGuiControlPanel(new Vector2(-0.452f, -0.332f), new Vector2(0.29f, 0.035f), color, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            MyGuiControlPanel panel = panel1;
            chatPage.Controls.Add(panel);
            chatPage.Controls.Add(label);
            y += label.GetTextSize().Y + 0.012f;
            MyGuiControlListbox listbox1 = new MyGuiControlListbox();
            listbox1.Position = new Vector2(x, y);
            listbox1.Size = new Vector2(num5, 0f);
            listbox1.Name = "PlayerListbox";
            listbox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            listbox1.VisibleRowsCount = num4;
            listbox1.VisualStyle = MyGuiControlListboxStyleEnum.ChatScreen;
            MyGuiControlListbox listbox = listbox1;
            chatPage.Controls.Add(listbox);
            y += ((listbox.ItemSize.Y * num4) + 0.02f) + 0.004f;
            num4 = 6;
            MyGuiControlLabel label3 = new MyGuiControlLabel();
            label3.Position = new Vector2(x + 0.01f, y + 0.003f);
            label3.Name = "PlayerLabel";
            label3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label3.Text = MyTexts.GetString(MySpaceTexts.TerminalTab_FactionsTableLabel);
            MyGuiControlLabel label2 = label3;
            color = null;
            MyGuiControlPanel panel4 = new MyGuiControlPanel(new Vector2(-0.452f, 0.097f), new Vector2(0.29f, 0.035f), color, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            panel4.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            MyGuiControlPanel panel2 = panel4;
            chatPage.Controls.Add(panel2);
            chatPage.Controls.Add(label2);
            MyGuiControlListbox listbox3 = new MyGuiControlListbox();
            listbox3.Position = new Vector2(x, y + (label.GetTextSize().Y + 0.01f));
            listbox3.Size = new Vector2(num5, 0f);
            listbox3.Name = "FactionListbox";
            listbox3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            listbox3.VisibleRowsCount = num4;
            listbox3.VisualStyle = MyGuiControlListboxStyleEnum.ChatScreen;
            MyGuiControlListbox listbox2 = listbox3;
            chatPage.Controls.Add(listbox2);
            y = -0.34f;
            num5 = 0.6f;
            num6 = 0.515f;
            float num7 = 0.038f;
            MyGuiControlPanel panel5 = new MyGuiControlPanel();
            panel5.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            panel5.Position = new Vector2(-0.125f, y + 0.008f);
            panel5.Size = new Vector2(num5 - 0.019f, num6 + 0.1f);
            panel5.BackgroundTexture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            MyGuiControlPanel panel3 = panel5;
            chatPage.Controls.Add(panel3);
            color = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text = new MyGuiControlMultilineText(new Vector2(num2 + 0.003f, y + 0.02f), new Vector2(num5 - 0.033f, num6 + 0.08f), color, "Blue", 0.7394595f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, visibleLinesCount, false, false, null, textPadding) {
                Name = "ChatHistory",
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP,
                TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
            };
            chatPage.Controls.Add(text);
            y += num6 + num7;
            num6 = 0.05f;
            MyGuiControlTextbox textbox1 = new MyGuiControlTextbox();
            textbox1.Position = new Vector2(num2 - 0.5765f, y + 0.104f);
            textbox1.Size = new Vector2(num5 - 0.165f, num6);
            textbox1.Name = "Chatbox";
            textbox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            MyGuiControlTextbox textbox = textbox1;
            chatPage.Controls.Add(textbox);
            num5 = 0.75f;
            y += num6 + num7;
            MyGuiControlButton button1 = new MyGuiControlButton();
            button1.Position = new Vector2(num2 + 0.007f, y + 0.023f);
            button1.Text = MyTexts.Get(MyCommonTexts.SendMessage).ToString();
            button1.Name = "SendButton";
            button1.Size = new Vector2(num5, 0.05f);
            button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            MyGuiControlButton button = button1;
            button.VisualStyle = MyGuiControlButtonStyleEnum.ComboBoxButton;
            chatPage.Controls.Add(button);
        }

        private void CreateControlPanelPageControls(MyGuiControlTabPage page)
        {
            page.Name = "PageControlPanel";
            page.TextEnum = MySpaceTexts.ControlPanel;
            page.TextScale = 0.7005405f;
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddVertical(new Vector2(0.145f, -0.333f), 0.676f, 0.002f, color);
            color = null;
            control.AddVertical(new Vector2(-0.1435f, -0.333f), 0.676f, 0.002f, color);
            page.Controls.Add(control);
            MyGuiControlSearchBox box1 = new MyGuiControlSearchBox(new Vector2(-0.452f, -0.342f), new Vector2(0.255f, 0.052f), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            box1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            box1.Name = "FunctionalBlockSearch";
            this.m_functionalBlockSearchBox = box1;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(-0.442f, -0.271f);
            label1.Name = "ControlLabel";
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MySpaceTexts.ControlScreen_GridBlocksLabel);
            MyGuiControlLabel label = label1;
            color = null;
            MyGuiControlPanel panel1 = new MyGuiControlPanel(new Vector2(-0.452f, -0.276f), new Vector2(0.29f, 0.035f), color, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            MyGuiControlPanel panel = panel1;
            page.Controls.Add(panel);
            page.Controls.Add(label);
            MyGuiControlListbox listbox1 = new MyGuiControlListbox();
            listbox1.Position = new Vector2(-0.452f, -0.2426f);
            listbox1.Size = new Vector2(0.29f, 0.5f);
            listbox1.Name = "FunctionalBlockListbox";
            listbox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            listbox1.VisualStyle = MyGuiControlListboxStyleEnum.ChatScreen;
            listbox1.VisibleRowsCount = 0x11;
            MyGuiControlListbox listbox = listbox1;
            MyGuiControlCompositePanel panel2 = new MyGuiControlCompositePanel();
            panel2.Position = new Vector2(-0.1525f, 0f);
            panel2.Size = new Vector2(0.615f, 0.7125f);
            panel2.Name = "Control";
            panel2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            panel2.InnerHeight = 0.685f;
            panel2.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            MyGuiControlCompositePanel panel3 = panel2;
            MyGuiControlPanel panel4 = new MyGuiControlPanel();
            panel4.Position = new Vector2(-0.1425f, -0.32f);
            panel4.Size = new Vector2(0.595f, 0.035f);
            panel4.Name = "SelectedBlockNamePanel";
            panel4.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            panel4.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            MyGuiControlPanel panel5 = panel4;
            MyGuiControlLabel label4 = new MyGuiControlLabel();
            label4.Position = new Vector2(-0.1325f, -0.322f);
            label4.Size = new Vector2(0.04702703f, 0.02666667f);
            label4.Name = "BlockNameLabel";
            label4.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            label4.Visible = false;
            label4.TextEnum = MySpaceTexts.Afterburner;
            MyGuiControlLabel label2 = label4;
            MyGuiControlLabel label5 = new MyGuiControlLabel();
            label5.Position = new Vector2(0.165f, -0.32f);
            label5.Size = new Vector2(0.04702703f, 0.02666667f);
            label5.Name = "GroupTitleLabel";
            label5.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            label5.TextEnum = MySpaceTexts.Terminal_GroupTitle;
            MyGuiControlLabel label3 = label5;
            MyGuiControlTextbox textbox1 = new MyGuiControlTextbox();
            textbox1.Position = new Vector2(0.165f, -0.283f);
            textbox1.Size = new Vector2(0.29f, 0.052f);
            textbox1.Name = "GroupName";
            textbox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            MyGuiControlTextbox textbox = textbox1;
            color = null;
            int? buttonIndex = null;
            MyGuiControlButton button1 = new MyGuiControlButton(new Vector2(0.167f, -0.228f), MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(new Vector2(222f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.TerminalButton_GroupSave), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button1.Name = "GroupSave";
            MyGuiControlButton button = button1;
            color = null;
            buttonIndex = null;
            MyGuiControlButton button4 = new MyGuiControlButton(new Vector2?(button.Position + new Vector2(button.Size.X + 0.013f, 0f)), MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(new Vector2(222f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.TerminalTab_GPS_Delete), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button4.Name = "GroupDelete";
            MyGuiControlButton button2 = button4;
            Vector2? size = null;
            color = null;
            buttonIndex = null;
            MyGuiControlButton button5 = new MyGuiControlButton(new Vector2(-0.19f, -0.332f), MyGuiControlButtonStyleEnum.SquareSmall, size, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 0.5f, buttonIndex, false);
            button5.Name = "ShowAll";
            MyGuiControlButton button3 = button5;
            page.Controls.Add(this.m_functionalBlockSearchBox);
            page.Controls.Add(listbox);
            page.Controls.Add(label2);
            page.Controls.Add(label3);
            page.Controls.Add(textbox);
            page.Controls.Add(button);
            page.Controls.Add(button3);
            page.Controls.Add(button2);
        }

        public static MyGuiControlLabel CreateErrorLabel(MyStringId text, string name)
        {
            Vector2? position = null;
            position = null;
            VRageMath.Vector4? colorMask = null;
            MyGuiControlLabel label1 = new MyGuiControlLabel(position, position, MyTexts.GetString(text), colorMask, 1.2f, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            label1.Name = name;
            label1.Visible = true;
            return label1;
        }

        private void CreateFactionsPageControls(MyGuiControlTabPage page)
        {
            page.Name = "PageFactions";
            page.TextEnum = MySpaceTexts.TerminalTab_Factions;
            page.TextScale = 0.7005405f;
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddVertical(new Vector2(-0.1435f, -0.333f), 0.676f, 0.002f, color);
            color = null;
            control.AddHorizontal(new Vector2(-0.123f, 0.04f), 0.578f, 0.001f, color);
            page.Controls.Add(control);
            color = null;
            MyGuiControlPanel panel1 = new MyGuiControlPanel(new Vector2(-0.452f, -0.332f), new Vector2(0.29f, 0.035f), color, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            MyGuiControlPanel panel = panel1;
            page.Controls.Add(panel);
            float x = -0.462f;
            float y = -0.34f;
            float num3 = 0.0045f;
            float num4 = 0.01f;
            Vector2 vector1 = new Vector2(0.29f, 0.052f);
            Vector2 vector = new Vector2(0.13f, 0.04f);
            MyGuiControlCompositePanel panel5 = new MyGuiControlCompositePanel();
            panel5.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            panel5.Position = new Vector2(x, y);
            panel5.Size = new Vector2(0.4f, 0.69f);
            panel5.Name = "Factions";
            MyGuiControlCompositePanel panel2 = panel5;
            x += num3;
            y += num4;
            MyGuiControlPanel panel6 = new MyGuiControlPanel();
            panel6.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            panel6.Position = new Vector2(x, y);
            panel6.Size = new Vector2(panel2.Size.X - 0.01f, 0.035f);
            panel6.BackgroundTexture = MyGuiConstants.TEXTURE_HIGHLIGHT_DARK;
            MyGuiControlPanel panel7 = panel6;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(-0.442f, -0.327f);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MySpaceTexts.TerminalTab_FactionsTableLabel);
            MyGuiControlLabel label = label1;
            y += label.Size.Y + num4;
            MyGuiControlTable table1 = new MyGuiControlTable();
            table1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            table1.Position = new Vector2(x + 0.0055f, y);
            table1.Size = new Vector2(0.29f, 0.15f);
            table1.Name = "FactionsTable";
            table1.ColumnsCount = 3;
            table1.VisibleRowsCount = 15;
            MyGuiControlTable table = table1;
            table.SetCustomColumnWidths(new float[] { 0.24f, 0.56f, 0.15f });
            table.SetColumnName(0, MyTexts.Get(MyCommonTexts.Tag));
            table.SetColumnName(1, MyTexts.Get(MyCommonTexts.Name));
            y += table.Size.Y + num4;
            Vector2? size = new Vector2?(new Vector2(225f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            color = null;
            int? buttonIndex = null;
            MyGuiControlButton button1 = new MyGuiControlButton(new Vector2(-0.449f, 0.305f), MyGuiControlButtonStyleEnum.Rectangular, size, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button1.Name = "buttonJoin";
            button1.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button = button1;
            size = new Vector2?(new Vector2(225f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            color = null;
            buttonIndex = null;
            MyGuiControlButton button16 = new MyGuiControlButton(new Vector2(-0.449f, 0.305f), MyGuiControlButtonStyleEnum.Rectangular, size, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button16.Name = "buttonCancelJoin";
            button16.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button2 = button16;
            size = new Vector2?(new Vector2(225f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            color = null;
            buttonIndex = null;
            MyGuiControlButton button17 = new MyGuiControlButton(new Vector2(-0.449f, 0.305f), MyGuiControlButtonStyleEnum.Rectangular, size, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button17.Name = "buttonLeave";
            button17.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button3 = button17;
            size = new Vector2?(new Vector2(225f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            color = null;
            buttonIndex = null;
            MyGuiControlButton button18 = new MyGuiControlButton(new Vector2(-0.16f, 0.305f), MyGuiControlButtonStyleEnum.Rectangular, size, color, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button18.Name = "buttonEnemy";
            button18.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button4 = button18;
            size = new Vector2?(new Vector2(225f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            color = null;
            buttonIndex = null;
            MyGuiControlButton button19 = new MyGuiControlButton(new Vector2(-0.449f, 0.255f), MyGuiControlButtonStyleEnum.Rectangular, size, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button19.Name = "buttonCreate";
            button19.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button5 = button19;
            size = new Vector2?(new Vector2(225f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            color = null;
            buttonIndex = null;
            MyGuiControlButton button20 = new MyGuiControlButton(new Vector2(-0.16f, 0.255f), MyGuiControlButtonStyleEnum.Rectangular, size, color, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button20.Name = "buttonSendPeace";
            button20.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button6 = button20;
            size = new Vector2?(new Vector2(225f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            color = null;
            buttonIndex = null;
            MyGuiControlButton button21 = new MyGuiControlButton(new Vector2(-0.16f, 0.255f), MyGuiControlButtonStyleEnum.Rectangular, size, color, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button21.Name = "buttonCancelPeace";
            button21.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button7 = button21;
            size = new Vector2?(new Vector2(225f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            color = null;
            buttonIndex = null;
            MyGuiControlButton button22 = new MyGuiControlButton(new Vector2(-0.16f, 0.255f), MyGuiControlButtonStyleEnum.Rectangular, size, color, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button22.Name = "buttonAcceptPeace";
            button22.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button8 = button22;
            page.Controls.Add(label);
            page.Controls.Add(table);
            page.Controls.Add(button5);
            page.Controls.Add(button);
            page.Controls.Add(button2);
            page.Controls.Add(button3);
            page.Controls.Add(button6);
            page.Controls.Add(button7);
            page.Controls.Add(button8);
            page.Controls.Add(button4);
            x = -0.0475f;
            y = -0.34f;
            MyGuiControlCompositePanel panel8 = new MyGuiControlCompositePanel();
            panel8.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            panel8.Position = new Vector2(-0.05f, y);
            panel8.Size = new Vector2(0.5f, 0.69f);
            panel8.Name = "compositeFaction";
            MyGuiControlCompositePanel panel9 = panel8;
            x += num3;
            MyGuiControlPanel panel10 = new MyGuiControlPanel();
            panel10.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            panel10.Position = new Vector2(-0.125f, -0.306f);
            panel10.Size = new Vector2(0.58f, 0.04f);
            panel10.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            panel10.Name = "panelFactionNamePanel";
            MyGuiControlPanel panel3 = panel10;
            MyGuiControlPanel panel11 = new MyGuiControlPanel();
            panel11.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            panel11.Position = new Vector2(-0.125f, 0.068f);
            panel11.Size = new Vector2(0.44f, 0.04f);
            panel11.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            panel11.Name = "panelFactionMembersNamePanel";
            MyGuiControlPanel panel4 = panel11;
            color = null;
            MyGuiControlLabel label9 = new MyGuiControlLabel(new Vector2(-0.125f, -0.325f), new Vector2(0.4f, 0.035f), null, color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label9.Name = "labelFactionName";
            label9.Text = MyTexts.Get(MySpaceTexts.TerminalTab_GPS_Name).ToString();
            MyGuiControlLabel label2 = label9;
            color = null;
            MyGuiControlLabel label10 = new MyGuiControlLabel(new Vector2(-0.117f, -0.299f), new Vector2(0.4f, 0.035f), null, color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            label10.Name = "labelFactionName";
            MyGuiControlLabel label3 = label10;
            Vector2 vector2 = panel3.Size - new Vector2(0.14f, 0.01f);
            color = null;
            MyGuiControlLabel label11 = new MyGuiControlLabel(new Vector2(-0.124f, -0.255f), new Vector2(0.4f, 0.035f), null, color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            label11.Name = "labelFactionDesc";
            MyGuiControlLabel label4 = label11;
            MyGuiBorderThickness? textPadding = new MyGuiBorderThickness(0.002f, 0f, 0f, 0f);
            color = null;
            buttonIndex = null;
            MyGuiControlMultilineText text1 = new MyGuiControlMultilineText(new Vector2(-0.125f, -0.226f), new Vector2(0.58f, 0.08f), color, "Blue", 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, buttonIndex, false, false, null, textPadding);
            text1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL;
            text1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text1.Name = "textFactionDesc";
            MyGuiControlMultilineText text = text1;
            color = null;
            MyGuiControlLabel label12 = new MyGuiControlLabel(new Vector2(-0.124f, -0.135f), new Vector2?(panel3.Size - new Vector2(0.01f, 0.01f)), null, color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            label12.Name = "labelFactionPrivate";
            MyGuiControlLabel label5 = label12;
            textPadding = new MyGuiBorderThickness(0.002f, 0f, 0f, 0f);
            color = null;
            buttonIndex = null;
            MyGuiControlMultilineText text3 = new MyGuiControlMultilineText(new Vector2(-0.125f, -0.105f), new Vector2(0.58f, 0.08f), color, "Blue", 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, buttonIndex, false, false, null, textPadding);
            text3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text3.Name = "textFactionPrivate";
            MyGuiControlMultilineText text2 = text3;
            color = null;
            MyGuiControlLabel label13 = new MyGuiControlLabel(new Vector2(-0.114f, 0.073f), new Vector2?(panel3.Size - new Vector2(0.01f, 0.01f)), null, color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            label13.Name = "labelFactionMembers";
            MyGuiControlLabel label6 = label13;
            color = null;
            MyGuiControlLabel label14 = new MyGuiControlLabel(new Vector2(-0.125f, -0.003f), new Vector2?(label6.Size - new Vector2(0.01f, 0.01f)), MyTexts.Get(MySpaceTexts.TerminalTab_Factions_AutoAccept).ToString(), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            label14.Name = "labelFactionMembersAcceptEveryone";
            MyGuiControlLabel label7 = label14;
            color = null;
            MyGuiControlCheckbox checkbox1 = new MyGuiControlCheckbox(new Vector2((label7.PositionX + label7.Size.X) + 0.02f, label7.PositionY + 0.012f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            checkbox1.Name = "checkFactionMembersAcceptEveryone";
            MyGuiControlCheckbox checkbox = checkbox1;
            color = null;
            MyGuiControlLabel label15 = new MyGuiControlLabel(new Vector2((label7.PositionX + label7.Size.X) + 0.08f, label7.PositionY), new Vector2?(label6.Size - new Vector2(0.01f, 0.01f)), MyTexts.Get(MySpaceTexts.TerminalTab_Factions_AutoAcceptRequest).ToString(), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            label15.Name = "labelFactionMembersAcceptPeace";
            MyGuiControlLabel label8 = label15;
            color = null;
            MyGuiControlCheckbox checkbox3 = new MyGuiControlCheckbox(new Vector2((label8.PositionX + label8.Size.X) + 0.02f, label8.PositionY + 0.012f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            checkbox3.Name = "checkFactionMembersAcceptPeace";
            MyGuiControlCheckbox checkbox2 = checkbox3;
            y = ((((((y + num4) + (label.Size.Y + (2f * num4))) + (label4.Size.Y + num4)) + (text.Size.Y + (2f * num4))) + (label5.Size.Y + num4)) + (text.Size.Y + 0.0275f)) + (label5.Size.Y + num4);
            MyGuiControlTable table3 = new MyGuiControlTable();
            table3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            table3.Position = new Vector2(-0.125f, 0.341f);
            table3.Size = new Vector2(vector2.X, 0.15f);
            table3.Name = "tableMembers";
            table3.ColumnsCount = 2;
            table3.VisibleRowsCount = 6;
            table3.HeaderVisible = false;
            MyGuiControlTable table2 = table3;
            float[] p = new float[] { 0.5f, 0.5f };
            table2.SetCustomColumnWidths(p);
            table2.SetColumnName(0, MyTexts.Get(MyCommonTexts.Name));
            table2.SetColumnName(1, MyTexts.Get(MyCommonTexts.Status));
            float num5 = (vector.Y + num4) - 0.0034f;
            size = new Vector2?(vector);
            color = null;
            buttonIndex = null;
            MyGuiControlButton button23 = new MyGuiControlButton(new Vector2(((x + table2.Size.X) + num4) - 0.081f, -0.01f), MyGuiControlButtonStyleEnum.Rectangular, size, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button23.Name = "buttonEdit";
            button23.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button9 = button23;
            size = new Vector2?(vector);
            color = null;
            buttonIndex = null;
            MyGuiControlButton button24 = new MyGuiControlButton(new Vector2(((x + table2.Size.X) + num4) - 0.081f, 0.07f), MyGuiControlButtonStyleEnum.Rectangular, size, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button24.Name = "buttonPromote";
            button24.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button10 = button24;
            size = new Vector2?(vector);
            color = null;
            buttonIndex = null;
            MyGuiControlButton button25 = new MyGuiControlButton(new Vector2(((x + table2.Size.X) + num4) - 0.081f, 0.07f + num5), MyGuiControlButtonStyleEnum.Rectangular, size, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button25.Name = "buttonKick";
            button25.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button11 = button25;
            size = new Vector2?(vector);
            color = null;
            buttonIndex = null;
            MyGuiControlButton button26 = new MyGuiControlButton(new Vector2(((x + table2.Size.X) + num4) - 0.081f, 0.07f + (2f * num5)), MyGuiControlButtonStyleEnum.Rectangular, size, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button26.Name = "buttonAcceptJoin";
            button26.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button12 = button26;
            size = new Vector2?(vector);
            color = null;
            buttonIndex = null;
            MyGuiControlButton button27 = new MyGuiControlButton(new Vector2(((x + table2.Size.X) + num4) - 0.081f, 0.07f + (3f * num5)), MyGuiControlButtonStyleEnum.Rectangular, size, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button27.Name = "buttonDemote";
            button27.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button13 = button27;
            size = new Vector2?(vector);
            color = null;
            buttonIndex = null;
            MyGuiControlButton button28 = new MyGuiControlButton(new Vector2(((x + table2.Size.X) + num4) - 0.081f, 0.07f + (4f * num5)), MyGuiControlButtonStyleEnum.Rectangular, size, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button28.Name = "buttonShareProgress";
            button28.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button14 = button28;
            size = new Vector2?(vector);
            color = null;
            buttonIndex = null;
            MyGuiControlButton button29 = new MyGuiControlButton(new Vector2(((x + table2.Size.X) + num4) - 0.081f, 0.07f + (5f * num5)), MyGuiControlButtonStyleEnum.Rectangular, size, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button29.Name = "buttonAddNpc";
            button29.ShowTooltipWhenDisabled = true;
            MyGuiControlButton button15 = button29;
            page.Controls.Add(panel3);
            page.Controls.Add(panel4);
            page.Controls.Add(label3);
            page.Controls.Add(label2);
            page.Controls.Add(label4);
            page.Controls.Add(text);
            page.Controls.Add(label5);
            page.Controls.Add(text2);
            page.Controls.Add(label6);
            page.Controls.Add(label7);
            page.Controls.Add(label8);
            page.Controls.Add(checkbox);
            page.Controls.Add(checkbox2);
            page.Controls.Add(table2);
            page.Controls.Add(button9);
            page.Controls.Add(button10);
            page.Controls.Add(button11);
            page.Controls.Add(button13);
            page.Controls.Add(button12);
            page.Controls.Add(button14);
            page.Controls.Add(button15);
        }

        private void CreateFixedTerminalElements()
        {
            this.m_terminalNotConnected = CreateErrorLabel(MySpaceTexts.ScreenTerminalError_ShipHasBeenDisconnected, "DisconnectedMessage");
            this.m_terminalNotConnected.Visible = false;
            this.Controls.Add(this.m_terminalNotConnected);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MySpaceTexts.Terminal, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2(base.m_size.Value.X * 0.447f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.894f, 0f, captionTextColor);
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2(base.m_size.Value.X * 0.447f, (base.m_size.Value.Y / 2f) - 0.1435f), base.m_size.Value.X * 0.894f, 0f, captionTextColor);
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2(base.m_size.Value.X * 0.447f, (-base.m_size.Value.Y / 2f) + 0.048f), base.m_size.Value.X * 0.894f, 0f, captionTextColor);
            this.Controls.Add(control);
            if (MyFakes.ENABLE_TERMINAL_PROPERTIES)
            {
                MyGuiControlParent parent1 = new MyGuiControlParent();
                parent1.Position = new Vector2(-0.855f, -0.514f);
                parent1.Size = new Vector2(0.8f, 0.15f);
                parent1.Name = "PropertiesPanel";
                parent1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.m_propertiesTopMenuParent = parent1;
                MyGuiControlParent parent2 = new MyGuiControlParent();
                parent2.Position = new Vector2(-0.02f, -0.67f);
                parent2.Size = new Vector2(0.93f, 0.78f);
                parent2.Name = "PropertiesTable";
                parent2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
                this.m_propertiesTableParent = parent2;
                this.CreatePropertiesPageControls(this.m_propertiesTopMenuParent, this.m_propertiesTableParent);
                if (this.m_controllerProperties == null)
                {
                    this.m_controllerProperties = new MyTerminalPropertiesController();
                }
                else
                {
                    this.m_controllerProperties.Close();
                }
                this.m_controllerProperties.ButtonClicked += new Action(this.PropertiesButtonClicked);
                this.Controls.Add(this.m_propertiesTableParent);
                this.Controls.Add(this.m_propertiesTopMenuParent);
            }
        }

        private void CreateGpsPageControls(MyGuiControlTabPage gpsPage)
        {
            gpsPage.Name = "PageIns";
            gpsPage.TextEnum = MySpaceTexts.TerminalTab_GPS;
            gpsPage.TextScale = 0.7005405f;
            float num = 0.01f;
            float num2 = 0.01f;
            Vector2 vector1 = new Vector2(0.29f, 0.052f);
            Vector2 vector2 = new Vector2(0.13f, 0.04f);
            float x = -0.4625f;
            float y = -0.325f;
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddVertical(new Vector2(-0.1435f, -0.333f), 0.676f, 0.002f, color);
            color = null;
            control.AddHorizontal(new Vector2(-0.123f, -0.085f), 0.578f, 0.001f, color);
            gpsPage.Controls.Add(control);
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(-0.442f, -0.267f);
            label1.Name = "GpsLabel";
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MySpaceTexts.GpsScreen_GpsListLabel);
            MyGuiControlLabel label = label1;
            color = null;
            MyGuiControlPanel panel1 = new MyGuiControlPanel(new Vector2(-0.452f, -0.272f), new Vector2(0.29f, 0.035f), color, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            MyGuiControlPanel panel = panel1;
            gpsPage.Controls.Add(panel);
            gpsPage.Controls.Add(label);
            MyGuiControlSearchBox box = new MyGuiControlSearchBox(new Vector2(-0.452f, y), new Vector2(0.29f, 0.02f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP) {
                Name = "SearchIns"
            };
            gpsPage.Controls.Add(box);
            y += (box.Size.Y + 0.01f) + num2;
            MyGuiControlTable table1 = new MyGuiControlTable();
            table1.Position = new Vector2(x + 0.0105f, y + 0.044f);
            table1.Size = new Vector2(0.29f, 0.5f);
            table1.Name = "TableINS";
            table1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            table1.ColumnsCount = 1;
            table1.VisibleRowsCount = 13;
            table1.HeaderVisible = false;
            MyGuiControlTable table = table1;
            float[] p = new float[] { 1f };
            table.SetCustomColumnWidths(p);
            y += (table.Size.Y + num2) + 0.055f;
            x += 0.013f;
            color = null;
            StringBuilder text = MyTexts.Get(MySpaceTexts.TerminalTab_GPS_Add);
            int? buttonIndex = null;
            MyGuiControlButton button1 = new MyGuiControlButton(new Vector2(x, y), MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(new Vector2(140f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_Add_ToolTip), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button1.Name = "buttonAdd";
            MyGuiControlButton button = button1;
            color = null;
            text = MyTexts.Get(MySpaceTexts.TerminalTab_GPS_Delete);
            buttonIndex = null;
            MyGuiControlButton button6 = new MyGuiControlButton(new Vector2(x, (y + button.Size.Y) + num2), MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(new Vector2(140f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_Delete_ToolTip), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button6.Name = "buttonDelete";
            MyGuiControlButton button2 = button6;
            button2.ShowTooltipWhenDisabled = true;
            color = null;
            text = MyTexts.Get(MySpaceTexts.TerminalTab_GPS_NewFromCurrent);
            buttonIndex = null;
            MyGuiControlButton button7 = new MyGuiControlButton(new Vector2((x + button.Size.X) + num, y), MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(new Vector2(310f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_NewFromCurrent_ToolTip), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button7.Name = "buttonFromCurrent";
            MyGuiControlButton button3 = button7;
            color = null;
            text = MyTexts.Get(MySpaceTexts.TerminalTab_GPS_NewFromClipboard);
            buttonIndex = null;
            MyGuiControlButton button8 = new MyGuiControlButton(new Vector2((x + button.Size.X) + num, (y + button.Size.Y) + num2), MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(new Vector2(310f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_NewFromClipboard_ToolTip), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button8.Name = "buttonFromClipboard";
            MyGuiControlButton button4 = button8;
            gpsPage.Controls.Add(table);
            gpsPage.Controls.Add(button);
            gpsPage.Controls.Add(button2);
            gpsPage.Controls.Add(button3);
            gpsPage.Controls.Add(button4);
            x = -0.15f + num;
            y = -0.325f + (num2 + 0.05f);
            color = null;
            MyGuiControlLabel label11 = new MyGuiControlLabel(new Vector2(-0.125f, -0.325f), new Vector2(0.4f, 0.035f), null, color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label11.Name = "labelInsName";
            label11.Text = MyTexts.Get(MySpaceTexts.TerminalTab_GPS_Name).ToString();
            MyGuiControlLabel label2 = label11;
            Vector2? position = null;
            color = null;
            MyGuiControlTextbox textbox1 = new MyGuiControlTextbox(position, null, 0x20, color, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            textbox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            textbox1.Position = new Vector2(-0.125f, -0.29f);
            textbox1.Size = new Vector2(0.58f, 0.035f);
            textbox1.Name = "panelInsName";
            MyGuiControlTextbox textbox = textbox1;
            textbox.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_NewCoord_Name_ToolTip));
            Vector2 vector3 = textbox.Size - new Vector2(0.14f, 0.01f);
            color = null;
            MyGuiControlLabel label12 = new MyGuiControlLabel(new Vector2(-0.125f, -0.245f), new Vector2(0.288f, 0.035f), null, color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label12.Name = "labelInsDesc";
            label12.Text = MyTexts.Get(MySpaceTexts.TerminalTab_GPS_Description).ToString();
            MyGuiControlLabel label3 = label12;
            y = (y + (textbox.Size.Y + (2f * num2))) + (label3.Size.Y + num2);
            color = null;
            MyGuiControlTextbox textbox6 = new MyGuiControlTextbox(new Vector2(-0.125f, -0.21f), null, 0xff, color, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            textbox6.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            textbox6.Name = "textInsDesc";
            textbox6.Size = new Vector2(0.58f, 0.035f);
            MyGuiControlTextbox textbox2 = textbox6;
            textbox2.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_NewCoord_Desc_ToolTip));
            y += textbox2.Size.Y + (2f * num2);
            color = null;
            MyGuiControlLabel label13 = new MyGuiControlLabel(new Vector2(-0.125f, -0.165f), new Vector2(0.4f, 0.035f), null, color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label13.Name = "labelInsCoordinates";
            label13.Text = MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_Coordinates);
            MyGuiControlLabel label4 = label13;
            color = null;
            MyGuiControlLabel label14 = new MyGuiControlLabel(new Vector2(x + 0.017f, y), new Vector2(0.01f, 0.035f), MyTexts.Get(MySpaceTexts.TerminalTab_GPS_X).ToString(), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label14.Name = "labelInsX";
            MyGuiControlLabel label5 = label14;
            x += label5.Size.X + num;
            MyGuiControlTextbox textbox7 = new MyGuiControlTextbox();
            textbox7.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            textbox7.Position = new Vector2(x + 0.017f, y);
            textbox7.Size = new Vector2((((0.598f - num) / 3f) - (2f * num)) - label5.Size.X, 0.035f);
            textbox7.Name = "textInsX";
            MyGuiControlTextbox textbox3 = textbox7;
            textbox3.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_X_ToolTip));
            x += textbox3.Size.X + num;
            color = null;
            MyGuiControlLabel label15 = new MyGuiControlLabel(new Vector2(x + 0.017f, y), new Vector2(0.586f, 0.035f), MyTexts.Get(MySpaceTexts.TerminalTab_GPS_Y).ToString(), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label15.Name = "labelInsY";
            MyGuiControlLabel label6 = label15;
            x += label5.Size.X + num;
            MyGuiControlTextbox textbox8 = new MyGuiControlTextbox();
            textbox8.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            textbox8.Position = new Vector2(x + 0.017f, y);
            textbox8.Size = new Vector2((((0.598f - num) / 3f) - (2f * num)) - label5.Size.X, 0.035f);
            textbox8.Name = "textInsY";
            MyGuiControlTextbox textbox4 = textbox8;
            textbox4.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_Y_ToolTip));
            x += textbox4.Size.X + num;
            color = null;
            MyGuiControlLabel label16 = new MyGuiControlLabel(new Vector2(x + 0.017f, y), new Vector2(0.01f, 0.035f), MyTexts.Get(MySpaceTexts.TerminalTab_GPS_Z).ToString(), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label16.Name = "labelInsZ";
            MyGuiControlLabel label7 = label16;
            x += label5.Size.X + num;
            MyGuiControlTextbox textbox9 = new MyGuiControlTextbox();
            textbox9.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            textbox9.Position = new Vector2(x + 0.017f, y);
            textbox9.Size = new Vector2((((0.598f - num) / 3f) - (2f * num)) - label5.Size.X, 0.035f);
            textbox9.Name = "textInsZ";
            MyGuiControlTextbox textbox5 = textbox9;
            textbox5.SetTooltip(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_Z_ToolTip));
            y += (textbox.Size.Y + (2f * num2)) + 0.032f;
            x = num - 0.135f;
            color = null;
            MyGuiControlCheckbox checkbox1 = new MyGuiControlCheckbox(new Vector2(x, y), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            checkbox1.Name = "checkInsShowOnHud";
            MyGuiControlCheckbox checkbox = checkbox1;
            color = null;
            MyGuiControlLabel label17 = new MyGuiControlLabel(new Vector2((x + checkbox.Size.X) + num, y), new Vector2?(checkbox.Size - new Vector2(0.01f, 0.01f)), null, color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label17.Name = "labelInsShowOnHud";
            label17.Text = MyTexts.Get(MySpaceTexts.TerminalTab_GPS_ShowOnHud).ToString();
            MyGuiControlLabel label8 = label17;
            color = null;
            text = MyTexts.Get(MySpaceTexts.TerminalTab_GPS_CopyToClipboard);
            buttonIndex = null;
            MyGuiControlButton button9 = new MyGuiControlButton(new Vector2(0.456f, y + 0.006f), MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(new Vector2(300f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), color, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER, MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_CopyToClipboard_ToolTip), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button9.Name = "buttonToClipboard";
            MyGuiControlButton button5 = button9;
            y += button5.Size.Y * 1.1f;
            color = null;
            MyGuiControlCheckbox checkbox3 = new MyGuiControlCheckbox(new Vector2(x, y), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            checkbox3.Name = "checkInsAlwaysVisible";
            MyGuiControlCheckbox checkbox2 = checkbox3;
            checkbox2.SetToolTip(MySpaceTexts.TerminalTab_GPS_AlwaysVisible_Tooltip);
            color = null;
            MyGuiControlLabel label18 = new MyGuiControlLabel(new Vector2((x + checkbox.Size.X) + num, y), new Vector2?(checkbox.Size - new Vector2(0.01f, 0.01f)), null, color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label18.Name = "labelInsAlwaysVisible";
            label18.Text = MyTexts.Get(MySpaceTexts.TerminalTab_GPS_AlwaysVisible).ToString();
            MyGuiControlLabel label9 = label18;
            y += checkbox.Size.Y;
            color = null;
            MyGuiControlLabel label19 = new MyGuiControlLabel(new Vector2(x + num, y), new Vector2(0.288f, 0.035f), null, color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label19.Name = "TerminalTab_GPS_SaveWarning";
            label19.Text = MyTexts.Get(MySpaceTexts.TerminalTab_GPS_SaveWarning).ToString();
            label19.ColorMask = Color.Red.ToVector4();
            MyGuiControlLabel label10 = label19;
            gpsPage.Controls.Add(textbox);
            gpsPage.Controls.Add(label2);
            gpsPage.Controls.Add(label3);
            gpsPage.Controls.Add(textbox2);
            gpsPage.Controls.Add(label4);
            gpsPage.Controls.Add(label5);
            gpsPage.Controls.Add(textbox3);
            gpsPage.Controls.Add(label6);
            gpsPage.Controls.Add(textbox4);
            gpsPage.Controls.Add(label7);
            gpsPage.Controls.Add(textbox5);
            gpsPage.Controls.Add(button5);
            gpsPage.Controls.Add(checkbox);
            gpsPage.Controls.Add(label8);
            gpsPage.Controls.Add(label10);
            gpsPage.Controls.Add(checkbox2);
            gpsPage.Controls.Add(label9);
        }

        private void CreateInfoPageControls(MyGuiControlTabPage infoPage)
        {
            infoPage.Name = "PageInfo";
            infoPage.TextEnum = MySpaceTexts.TerminalTab_Info;
            infoPage.TextScale = 0.7005405f;
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddVertical(new Vector2(0.145f, -0.333f), 0.676f, 0.002f, color);
            color = null;
            control.AddVertical(new Vector2(-0.1435f, -0.333f), 0.676f, 0.002f, color);
            color = null;
            control.AddHorizontal(new Vector2(0.168f, 0.148f), 0.27f, 0.001f, color);
            infoPage.Controls.Add(control);
            MyGuiControlPanel panel1 = new MyGuiControlPanel();
            panel1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            panel1.Position = new Vector2(-0.452f, -0.332f);
            panel1.Size = new Vector2(0.29f, 0.035f);
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            MyGuiControlPanel panel = panel1;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(-0.442f, -0.327f);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MySpaceTexts.TerminalTab_Info_GridInfoLabel);
            MyGuiControlLabel label = label1;
            label.Name = "Infolabel";
            infoPage.Controls.Add(panel);
            infoPage.Controls.Add(label);
            color = null;
            MyGuiControlList list2 = new MyGuiControlList(new Vector2(-0.452f, -0.299f), new Vector2(0.29f, 0.6405f), color, null, MyGuiControlListStyleEnum.Default) {
                Name = "InfoList",
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
            };
            infoPage.Controls.Add(list2);
            MyGuiControlLabel label9 = new MyGuiControlLabel();
            label9.Position = new Vector2(0.168f, 0.05f);
            label9.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label9.Text = MyTexts.GetString(MySpaceTexts.TerminalTab_Info_ShipName);
            MyGuiControlLabel label2 = label9;
            label2.Name = "RenameShipLabel";
            infoPage.Controls.Add(label2);
            MyGuiControlButton button = new MyGuiControlButton {
                Position = new Vector2(0.31f, 0.225f),
                TextEnum = MySpaceTexts.TerminalTab_Info_ConvertButton
            };
            button.SetToolTip(MySpaceTexts.TerminalTab_Info_ConvertButton_TT);
            button.ShowTooltipWhenDisabled = true;
            button.Name = "ConvertBtn";
            infoPage.Controls.Add(button);
            MyGuiControlButton button2 = new MyGuiControlButton {
                Position = new Vector2(0.31f, 0.285f),
                TextEnum = MySpaceTexts.TerminalTab_Info_ConvertToStationButton
            };
            button2.SetToolTip(MySpaceTexts.TerminalTab_Info_ConvertToStationButton_TT);
            button2.ShowTooltipWhenDisabled = true;
            button2.Name = "ConvertToStationBtn";
            button2.Visible = MySession.Static.EnableConvertToStation;
            infoPage.Controls.Add(button2);
            Vector2? position = null;
            position = null;
            color = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text1 = new MyGuiControlMultilineText(position, position, color, "Blue", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, visibleLinesCount, false, false, null, textPadding);
            text1.Name = "InfoHelpMultilineText";
            text1.Position = new Vector2(0.167f, -0.3345f);
            text1.Size = new Vector2(0.297f, 0.36f);
            text1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text1.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text1.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text1.Text = MyTexts.Get(MySpaceTexts.TerminalTab_Info_Description);
            MyGuiControlMultilineText text = text1;
            infoPage.Controls.Add(text);
            if (MyFakes.ENABLE_CENTER_OF_MASS)
            {
                position = null;
                color = null;
                MyGuiControlLabel label8 = new MyGuiControlLabel(new Vector2(-0.123f, -0.313f), position, MyTexts.GetString(MySpaceTexts.TerminalTab_Info_ShowMassCenter), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                    OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER
                };
                infoPage.Controls.Add(label8);
                color = null;
                MyGuiControlCheckbox checkbox6 = new MyGuiControlCheckbox(new Vector2(0.135f, label8.Position.Y), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                    OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER
                };
                checkbox6.SetToolTip(MyTexts.GetString(MySpaceTexts.TerminalTab_Info_ShowMassCenter_ToolTip));
                checkbox6.Name = "CenterBtn";
                infoPage.Controls.Add(checkbox6);
            }
            position = null;
            color = null;
            MyGuiControlLabel label3 = new MyGuiControlLabel(new Vector2(-0.123f, -0.263f), position, MyTexts.GetString(MySpaceTexts.TerminalTab_Info_ShowGravityGizmo), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER
            };
            infoPage.Controls.Add(label3);
            color = null;
            MyGuiControlCheckbox checkbox = new MyGuiControlCheckbox(new Vector2(0.135f, label3.Position.Y), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER
            };
            checkbox.SetToolTip(MyTexts.GetString(MySpaceTexts.TerminalTab_Info_ShowGravityGizmo_ToolTip));
            checkbox.Name = "ShowGravityGizmo";
            infoPage.Controls.Add(checkbox);
            position = null;
            color = null;
            MyGuiControlLabel label4 = new MyGuiControlLabel(new Vector2(-0.123f, -0.213f), position, MyTexts.GetString(MySpaceTexts.TerminalTab_Info_ShowSenzorGizmo), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER
            };
            infoPage.Controls.Add(label4);
            color = null;
            MyGuiControlCheckbox checkbox2 = new MyGuiControlCheckbox(new Vector2(0.135f, label4.Position.Y), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER
            };
            checkbox2.SetToolTip(MyTexts.GetString(MySpaceTexts.TerminalTab_Info_ShowSenzorGizmo_ToolTip));
            checkbox2.Name = "ShowSenzorGizmo";
            infoPage.Controls.Add(checkbox2);
            position = null;
            color = null;
            MyGuiControlLabel label5 = new MyGuiControlLabel(new Vector2(-0.123f, -0.163f), position, MyTexts.GetString(MySpaceTexts.TerminalTab_Info_ShowAntenaGizmo), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER
            };
            infoPage.Controls.Add(label5);
            color = null;
            MyGuiControlCheckbox checkbox3 = new MyGuiControlCheckbox(new Vector2(0.135f, label5.Position.Y), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER
            };
            checkbox3.SetToolTip(MyTexts.GetString(MySpaceTexts.TerminalTab_Info_ShowAntenaGizmo_ToolTip));
            checkbox3.Name = "ShowAntenaGizmo";
            infoPage.Controls.Add(checkbox3);
            CreateAntennaSlider(infoPage, MyTexts.GetString(MySpaceTexts.TerminalTab_Info_FriendlyAntennaRange), "FriendAntennaRange", -0.05f);
            CreateAntennaSlider(infoPage, MyTexts.GetString(MySpaceTexts.TerminalTab_Info_EnemyAntennaRange), "EnemyAntennaRange", 0.09f);
            CreateAntennaSlider(infoPage, MyTexts.GetString(MySpaceTexts.TerminalTab_Info_OwnedAntennaRange), "OwnedAntennaRange", 0.23f);
            position = null;
            color = null;
            MyGuiControlLabel label6 = new MyGuiControlLabel(new Vector2(-0.123f, -0.113f), position, MyTexts.GetString(MySpaceTexts.TerminalTab_Info_PivotBtn), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER
            };
            infoPage.Controls.Add(label6);
            color = null;
            MyGuiControlCheckbox checkbox4 = new MyGuiControlCheckbox(new Vector2(0.135f, label6.Position.Y), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER
            };
            checkbox4.SetToolTip(MyTexts.GetString(MySpaceTexts.TerminalTab_Info_PivotBtn_ToolTip));
            checkbox4.Name = "PivotBtn";
            infoPage.Controls.Add(checkbox4);
            if (MyFakes.ENABLE_TERMINAL_PROPERTIES)
            {
                MyGuiControlTextbox textbox1 = new MyGuiControlTextbox();
                textbox1.Name = "RenameShipText";
                textbox1.Position = new Vector2(0.168f, label2.PositionY + 0.048f);
                textbox1.Size = new Vector2(0.225f, 0.005f);
                textbox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                MyGuiControlTextbox textbox = textbox1;
                MyGuiControlButton button1 = new MyGuiControlButton();
                button1.Name = "RenameShipButton";
                button1.Position = new Vector2((textbox.PositionX + textbox.Size.X) + 0.025f, textbox.PositionY + 0.006f);
                button1.Text = MyTexts.Get(MyCommonTexts.Ok).ToString();
                button1.VisualStyle = MyGuiControlButtonStyleEnum.Rectangular;
                button1.Size = new Vector2(0.036f, 0.0392f);
                MyGuiControlButton button3 = button1;
                button3.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Ok));
                infoPage.Controls.Add(button3);
                textbox.SetTooltip(MyTexts.Get(MySpaceTexts.TerminalName).ToString());
                textbox.ShowTooltipWhenDisabled = true;
                infoPage.Controls.Add(textbox);
            }
            position = null;
            color = null;
            MyGuiControlLabel label7 = new MyGuiControlLabel(new Vector2(-0.123f, 0.28f), position, MyTexts.GetString(MySpaceTexts.TerminalTab_Info_DestructibleBlocks), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER,
                Visible = MySession.Static.Settings.ScenarioEditMode || MySession.Static.IsScenario
            };
            infoPage.Controls.Add(label7);
            color = null;
            MyGuiControlCheckbox checkbox5 = new MyGuiControlCheckbox(new Vector2(0.135f, label7.Position.Y), color, MyTexts.GetString(MySpaceTexts.TerminalTab_Info_DestructibleBlocks_Tooltip), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER,
                Name = "SetDestructibleBlocks"
            };
            infoPage.Controls.Add(checkbox5);
        }

        private void CreateInventoryPageControls(MyGuiControlTabPage page)
        {
            page.Name = "PageInventory";
            page.TextEnum = MySpaceTexts.Inventory;
            page.TextScale = 0.7005405f;
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddVertical(new Vector2(0.001f, -0.333f), 0.676f, 0.002f, color);
            page.Controls.Add(control);
            MyGuiControlRadioButton button1 = new MyGuiControlRadioButton();
            button1.Position = new Vector2(-0.4565f, -0.338f);
            button1.Size = new Vector2(0.056875f, 0.0575f);
            button1.Name = "LeftSuitButton";
            button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            button1.Key = 0;
            button1.VisualStyle = MyGuiControlRadioButtonStyleEnum.FilterCharacter;
            MyGuiControlRadioButton button = button1;
            MyGuiControlRadioButton button15 = new MyGuiControlRadioButton();
            button15.Position = new Vector2(-0.41f, -0.338f);
            button15.Size = new Vector2(0.056875f, 0.0575f);
            button15.Name = "LeftGridButton";
            button15.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            button15.Key = 0;
            button15.VisualStyle = MyGuiControlRadioButtonStyleEnum.FilterGrid;
            MyGuiControlRadioButton button2 = button15;
            MyGuiControlRadioButton button16 = new MyGuiControlRadioButton();
            button16.Position = new Vector2(-0.275f, -0.338f);
            button16.Size = new Vector2(0.045f, 0.05666667f);
            button16.Name = "LeftFilterStorageButton";
            button16.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            button16.Key = 0;
            button16.VisualStyle = MyGuiControlRadioButtonStyleEnum.FilterStorage;
            MyGuiControlRadioButton button3 = button16;
            MyGuiControlRadioButton button17 = new MyGuiControlRadioButton();
            button17.Position = new Vector2(-0.2285f, -0.338f);
            button17.Size = new Vector2(0.045f, 0.05666667f);
            button17.Name = "LeftFilterSystemButton";
            button17.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            button17.Key = 0;
            button17.VisualStyle = MyGuiControlRadioButtonStyleEnum.FilterSystem;
            MyGuiControlRadioButton button4 = button17;
            MyGuiControlRadioButton button18 = new MyGuiControlRadioButton();
            button18.Position = new Vector2(-0.182f, -0.338f);
            button18.Size = new Vector2(0.045f, 0.05666667f);
            button18.Name = "LeftFilterEnergyButton";
            button18.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            button18.Key = 0;
            button18.VisualStyle = MyGuiControlRadioButtonStyleEnum.FilterEnergy;
            MyGuiControlRadioButton button5 = button18;
            MyGuiControlRadioButton button19 = new MyGuiControlRadioButton();
            button19.Position = new Vector2(-0.1355f, -0.338f);
            button19.Size = new Vector2(0.045f, 0.05666667f);
            button19.Name = "LeftFilterAllButton";
            button19.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            button19.Key = 0;
            button19.VisualStyle = MyGuiControlRadioButtonStyleEnum.FilterAll;
            MyGuiControlRadioButton button6 = button19;
            Vector2? position = null;
            color = null;
            MyGuiControlCheckbox checkbox1 = new MyGuiControlCheckbox(position, color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            checkbox1.Position = new Vector2(-0.006f, -0.255f);
            checkbox1.Name = "CheckboxHideEmptyLeft";
            checkbox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            MyGuiControlCheckbox checkbox = checkbox1;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(-0.055f, -0.255f);
            label1.Name = "LabelHideEmptyLeft";
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            label1.TextEnum = MySpaceTexts.HideEmpty;
            MyGuiControlLabel label = label1;
            MyGuiControlSearchBox box1 = new MyGuiControlSearchBox(new Vector2(-0.452f, -0.26f), new Vector2(0.361f - label.Size.X, 0.052f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            box1.Name = "BlockSearchLeft";
            MyGuiControlSearchBox box = box1;
            MyGuiControlList list1 = new MyGuiControlList();
            list1.Position = new Vector2(-0.465f, -0.26f);
            list1.Size = new Vector2(0.44f, 0.616f);
            list1.Name = "LeftInventory";
            list1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            MyGuiControlList list2 = list1;
            page.Controls.Add(button);
            page.Controls.Add(button2);
            page.Controls.Add(button3);
            page.Controls.Add(button4);
            page.Controls.Add(button5);
            page.Controls.Add(button6);
            page.Controls.Add(box);
            page.Controls.Add(checkbox);
            page.Controls.Add(label);
            page.Controls.Add(list2);
            MyGuiControlRadioButton button20 = new MyGuiControlRadioButton();
            button20.Position = new Vector2(0.0145f, -0.338f);
            button20.Size = new Vector2(0.056875f, 0.0575f);
            button20.Name = "RightSuitButton";
            button20.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            button20.Key = 0;
            button20.VisualStyle = MyGuiControlRadioButtonStyleEnum.FilterCharacter;
            MyGuiControlRadioButton button7 = button20;
            MyGuiControlRadioButton button21 = new MyGuiControlRadioButton();
            button21.Position = new Vector2(0.061f, -0.338f);
            button21.Size = new Vector2(0.056875f, 0.0575f);
            button21.Name = "RightGridButton";
            button21.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            button21.Key = 0;
            button21.VisualStyle = MyGuiControlRadioButtonStyleEnum.FilterGrid;
            MyGuiControlRadioButton button8 = button21;
            MyGuiControlRadioButton button22 = new MyGuiControlRadioButton();
            button22.Position = new Vector2(0.275f, -0.338f);
            button22.Size = new Vector2(0.045f, 0.05666667f);
            button22.Name = "RightFilterShipButton";
            button22.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            button22.Key = 0;
            button22.VisualStyle = MyGuiControlRadioButtonStyleEnum.FilterShip;
            MyGuiControlRadioButton button9 = button22;
            MyGuiControlRadioButton button23 = new MyGuiControlRadioButton();
            button23.Position = new Vector2(0.321f, -0.338f);
            button23.Size = new Vector2(0.045f, 0.05666667f);
            button23.Name = "RightFilterStorageButton";
            button23.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            button23.Key = 0;
            button23.VisualStyle = MyGuiControlRadioButtonStyleEnum.FilterStorage;
            MyGuiControlRadioButton button10 = button23;
            MyGuiControlRadioButton button24 = new MyGuiControlRadioButton();
            button24.Position = new Vector2(0.3675f, -0.338f);
            button24.Size = new Vector2(0.045f, 0.05666667f);
            button24.Name = "RightFilterSystemButton";
            button24.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            button24.Key = 0;
            button24.VisualStyle = MyGuiControlRadioButtonStyleEnum.FilterSystem;
            MyGuiControlRadioButton button11 = button24;
            MyGuiControlRadioButton button25 = new MyGuiControlRadioButton();
            button25.Position = new Vector2(0.414f, -0.338f);
            button25.Size = new Vector2(0.045f, 0.05666667f);
            button25.Name = "RightFilterEnergyButton";
            button25.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            button25.Key = 0;
            button25.VisualStyle = MyGuiControlRadioButtonStyleEnum.FilterEnergy;
            MyGuiControlRadioButton button12 = button25;
            MyGuiControlRadioButton button26 = new MyGuiControlRadioButton();
            button26.Position = new Vector2(0.4605f, -0.338f);
            button26.Size = new Vector2(0.045f, 0.05666667f);
            button26.Name = "RightFilterAllButton";
            button26.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            button26.Key = 0;
            button26.VisualStyle = MyGuiControlRadioButtonStyleEnum.FilterAll;
            MyGuiControlRadioButton button13 = button26;
            position = null;
            color = null;
            MyGuiControlCheckbox checkbox3 = new MyGuiControlCheckbox(position, color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            checkbox3.Position = new Vector2(0.464f, -0.255f);
            checkbox3.Name = "CheckboxHideEmptyRight";
            checkbox3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            MyGuiControlCheckbox checkbox2 = checkbox3;
            MyGuiControlLabel label3 = new MyGuiControlLabel();
            label3.Position = new Vector2(0.415f, -0.255f);
            label3.Name = "LabelHideEmptyRight";
            label3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            label3.TextEnum = MySpaceTexts.HideEmpty;
            MyGuiControlLabel label2 = label3;
            MyGuiControlSearchBox box3 = new MyGuiControlSearchBox(new Vector2(0.0185f, -0.26f), new Vector2(0.361f - label2.Size.X, 0.052f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            box3.Name = "BlockSearchRight";
            MyGuiControlSearchBox box2 = box3;
            MyGuiControlList list4 = new MyGuiControlList();
            list4.Position = new Vector2(0.465f, -0.295f);
            list4.Size = new Vector2(0.44f, 0.65f);
            list4.Name = "RightInventory";
            list4.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            MyGuiControlList list3 = list4;
            page.Controls.Add(button7);
            page.Controls.Add(button8);
            page.Controls.Add(button9);
            page.Controls.Add(button10);
            page.Controls.Add(button11);
            page.Controls.Add(button12);
            page.Controls.Add(button13);
            page.Controls.Add(box2);
            page.Controls.Add(checkbox2);
            page.Controls.Add(label2);
            page.Controls.Add(list3);
            MyGuiControlButton button27 = new MyGuiControlButton();
            button27.Position = new Vector2(-0.0915f, -0.337f);
            button27.Size = new Vector2(0.044375f, 0.1366667f);
            button27.Name = "ThrowOutButtonLeft";
            button27.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            button27.TextEnum = MySpaceTexts.Afterburner;
            button27.TextScale = 0f;
            button27.TextAlignment = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            button27.DrawCrossTextureWhenDisabled = true;
            button27.VisualStyle = MyGuiControlButtonStyleEnum.InventoryTrash;
            button27.ActivateOnMouseRelease = true;
            MyGuiControlButton button14 = button27;
            page.Controls.Add(button14);
        }

        private void CreateProductionPageControls(MyGuiControlTabPage productionPage)
        {
            productionPage.Name = "PageProduction";
            productionPage.TextEnum = MySpaceTexts.TerminalTab_Production;
            productionPage.TextScale = 0.7005405f;
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddVertical(new Vector2(0.145f, -0.333f), 0.676f, 0.002f, color);
            color = null;
            control.AddVertical(new Vector2(-0.1435f, -0.333f), 0.676f, 0.002f, color);
            productionPage.Controls.Add(control);
            float num = 0.03f;
            float x = 0.01f;
            float num3 = 0.05f;
            float y = 0.08f;
            Vector2? size = null;
            color = null;
            size = null;
            size = null;
            color = null;
            MyGuiControlCombobox combobox1 = new MyGuiControlCombobox(new Vector2?(((Vector2) (-0.5f * productionPage.Size)) + new Vector2(0.001f, x + 0.174f)), size, color, size, 10, size, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, color);
            combobox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            combobox1.Name = "AssemblersCombobox";
            MyGuiControlCombobox combobox = combobox1;
            color = null;
            MyGuiControlPanel panel1 = new MyGuiControlPanel(new Vector2?((((Vector2) (-0.5f * productionPage.Size)) + new Vector2(0f, x + 0.028f)) + new Vector2(0.001f, ((combobox.Size.Y + x) - 0.001f) - 0.048f)), new Vector2(1f, y), color, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            panel1.Name = "BlueprintsBackgroundPanel";
            MyGuiControlPanel panel = panel1;
            color = null;
            MyGuiControlPanel panel8 = new MyGuiControlPanel(new Vector2(-0.452f, -0.332f), new Vector2(0.29f, 0.035f), color, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            panel8.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            MyGuiControlPanel panel2 = panel8;
            size = null;
            color = null;
            MyGuiControlLabel label1 = new MyGuiControlLabel(new Vector2?(panel.Position + new Vector2(x, x - 0.005f)), size, MyTexts.GetString(MySpaceTexts.ScreenTerminalProduction_Blueprints), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            label1.Name = "BlueprintsLabel";
            MyGuiControlLabel label = label1;
            MyGuiControlSearchBox box1 = new MyGuiControlSearchBox(new Vector2?(panel.Position + new Vector2(0f, y + x)), new Vector2?(combobox.Size), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            box1.Name = "BlueprintsSearchBox";
            MyGuiControlSearchBox box = box1;
            MyGuiControlGrid scrolledControl = new MyGuiControlGrid();
            scrolledControl.VisualStyle = MyGuiControlGridStyleEnum.Blueprints;
            scrolledControl.RowsCount = MyTerminalProductionController.BLUEPRINT_GRID_ROWS;
            scrolledControl.ColumnsCount = 5;
            scrolledControl.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            MyGuiControlScrollablePanel panel9 = new MyGuiControlScrollablePanel(scrolledControl);
            panel9.Name = "BlueprintsScrollableArea";
            panel9.ScrollbarVEnabled = true;
            panel9.Position = combobox.Position + new Vector2(0f, combobox.Size.Y + x);
            panel9.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            panel9.BackgroundTexture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            panel9.Size = new Vector2(panel.Size.X, 0.5f);
            panel9.ScrolledAreaPadding = new MyGuiBorderThickness(0.005f);
            panel9.DrawScrollBarSeparator = true;
            MyGuiControlScrollablePanel panel3 = panel9;
            panel3.FitSizeToScrolledControl();
            combobox.Size = new Vector2(panel3.Size.X, combobox.Size.Y);
            box.Size = combobox.Size;
            panel.Size = new Vector2(panel3.Size.X, y);
            scrolledControl.RowsCount = 20;
            productionPage.Controls.Add(combobox);
            productionPage.Controls.Add(panel);
            productionPage.Controls.Add(panel2);
            productionPage.Controls.Add(label);
            productionPage.Controls.Add(box);
            productionPage.Controls.Add(panel3);
            Vector2 vector = panel.Position + new Vector2((panel.Size.X + num) + 0.05f, 0f);
            size = null;
            color = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2?((vector + new Vector2(x, x)) + new Vector2(-0.05f, 0.002f)), size, MyTexts.GetString(MySpaceTexts.ScreenTerminalProduction_StoredMaterials), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            productionPage.Controls.Add(label2);
            size = null;
            color = null;
            MyGuiControlLabel label3 = new MyGuiControlLabel(new Vector2?((vector + new Vector2(x, x)) + new Vector2(-0.05f, 0.028f)), size, MyTexts.GetString(MySpaceTexts.ScreenTerminalProduction_MaterialType), color, 0.704f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            productionPage.Controls.Add(label3);
            size = null;
            color = null;
            MyGuiControlLabel label4 = new MyGuiControlLabel(new Vector2?((vector + new Vector2(x, x)) + new Vector2(0.2f, 0.028f)), size, null, color, 0.704f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) {
                Name = "RequiredLabel"
            };
            productionPage.Controls.Add(label4);
            MyGuiControlComponentList list1 = new MyGuiControlComponentList();
            list1.Position = vector + new Vector2(-0.062f, num3 - 0.002f);
            list1.Size = new Vector2(0.29f, (num3 + panel3.Size.Y) - num3);
            list1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            list1.BackgroundTexture = null;
            list1.Name = "MaterialsList";
            MyGuiControlComponentList list2 = list1;
            productionPage.Controls.Add(list2);
            color = null;
            MyGuiControlRadioButton button1 = new MyGuiControlRadioButton(new Vector2?(vector + new Vector2((panel.Size.X + num) - 0.071f, 0f)), new Vector2?(new Vector2(210f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), 0, color);
            button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            button1.Icon = new MyGuiHighlightTexture?(MyGuiConstants.TEXTURE_BUTTON_ICON_COMPONENT);
            button1.IconOriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            button1.TextAlignment = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            button1.Text = MyTexts.Get(MySpaceTexts.ScreenTerminalProduction_AssemblingButton);
            button1.Name = "AssemblingButton";
            MyGuiControlRadioButton button = button1;
            button.SetToolTip(MySpaceTexts.ToolTipTerminalProduction_AssemblingMode);
            color = null;
            MyGuiControlRadioButton button6 = new MyGuiControlRadioButton(new Vector2?(button.Position + new Vector2(button.Size.X + x, 0f)), new Vector2?(new Vector2(238f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), 0, color);
            button6.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            button6.Icon = new MyGuiHighlightTexture?(MyGuiConstants.TEXTURE_BUTTON_ICON_DISASSEMBLY);
            button6.IconOriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            button6.TextAlignment = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            button6.Text = MyTexts.Get(MySpaceTexts.ScreenTerminalProduction_DisassemblingButton);
            button6.Name = "DisassemblingButton";
            MyGuiControlRadioButton button2 = button6;
            button2.SetToolTip(MySpaceTexts.ToolTipTerminalProduction_DisassemblingMode);
            MyGuiControlCompositePanel panel10 = new MyGuiControlCompositePanel();
            panel10.Position = button.Position + new Vector2(0f, button.Size.Y + x);
            panel10.Size = new Vector2(0.4f, y);
            panel10.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            panel10.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            MyGuiControlCompositePanel panel4 = panel10;
            size = null;
            color = null;
            MyGuiControlLabel label5 = new MyGuiControlLabel(new Vector2?(panel4.Position + new Vector2(x, x)), size, MyTexts.GetString(MySpaceTexts.ScreenTerminalProduction_ProductionQueue), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            MyGuiControlGrid grid2 = new MyGuiControlGrid();
            grid2.VisualStyle = MyGuiControlGridStyleEnum.Blueprints;
            grid2.RowsCount = 2;
            grid2.ColumnsCount = 5;
            grid2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            MyGuiControlScrollablePanel panel11 = new MyGuiControlScrollablePanel(grid2);
            panel11.Name = "QueueScrollableArea";
            panel11.ScrollbarVEnabled = true;
            panel11.Position = panel4.Position + new Vector2(0f, panel4.Size.Y);
            panel11.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            panel11.BackgroundTexture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            panel11.ScrolledAreaPadding = new MyGuiBorderThickness(0.005f);
            panel11.DrawScrollBarSeparator = true;
            MyGuiControlScrollablePanel panel5 = panel11;
            panel5.FitSizeToScrolledControl();
            grid2.RowsCount = 10;
            panel4.Size = new Vector2(panel5.Size.X, panel4.Size.Y);
            color = null;
            MyGuiControlCheckbox checkbox1 = new MyGuiControlCheckbox(new Vector2?(panel4.Position + new Vector2(panel4.Size.X - x, x)), color, MyTexts.GetString(MySpaceTexts.ToolTipTerminalProduction_RepeatMode), false, MyGuiControlCheckboxStyleEnum.Repeat, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            checkbox1.Name = "RepeatCheckbox";
            MyGuiControlCheckbox checkbox = checkbox1;
            color = null;
            MyGuiControlCheckbox checkbox3 = new MyGuiControlCheckbox(new Vector2?(panel4.Position + new Vector2((panel4.Size.X - 0.1f) - x, x)), color, MyTexts.GetString(MySpaceTexts.ToolTipTerminalProduction_SlaveMode), false, MyGuiControlCheckboxStyleEnum.Slave, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            checkbox3.Name = "SlaveCheckbox";
            MyGuiControlCheckbox checkbox2 = checkbox3;
            MyGuiControlCompositePanel panel12 = new MyGuiControlCompositePanel();
            panel12.Position = panel5.Position + new Vector2(0f, panel5.Size.Y + x);
            panel12.Size = new Vector2(0.4f, y);
            panel12.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            panel12.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            MyGuiControlCompositePanel panel6 = panel12;
            size = null;
            color = null;
            MyGuiControlLabel label6 = new MyGuiControlLabel(new Vector2?(panel6.Position + new Vector2(x, x)), size, MyTexts.GetString(MySpaceTexts.ScreenTerminalProduction_Inventory), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            MyGuiControlGrid grid3 = new MyGuiControlGrid();
            grid3.VisualStyle = MyGuiControlGridStyleEnum.Blueprints;
            grid3.RowsCount = 3;
            grid3.ColumnsCount = 5;
            grid3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            MyGuiControlScrollablePanel panel13 = new MyGuiControlScrollablePanel(grid3);
            panel13.Name = "InventoryScrollableArea";
            panel13.ScrollbarVEnabled = true;
            panel13.Position = panel6.Position + new Vector2(0f, panel6.Size.Y);
            panel13.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            panel13.BackgroundTexture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
            panel13.ScrolledAreaPadding = new MyGuiBorderThickness(0.005f);
            panel13.DrawScrollBarSeparator = true;
            MyGuiControlScrollablePanel panel7 = panel13;
            panel7.FitSizeToScrolledControl();
            grid3.RowsCount = 10;
            panel6.Size = new Vector2(panel7.Size.X, panel6.Size.Y);
            color = null;
            StringBuilder text = MyTexts.Get(MySpaceTexts.ScreenTerminalProduction_DisassembleAllButton);
            int? buttonIndex = null;
            MyGuiControlButton button7 = new MyGuiControlButton(new Vector2?(panel6.Position + new Vector2(panel6.Size.X - x, x)), MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(new Vector2(220f, 40f) / MyGuiConstants.GUI_OPTIMAL_SIZE), color, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, MyTexts.GetString(MySpaceTexts.ToolTipTerminalProduction_DisassembleAll), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button7.Name = "DisassembleAllButton";
            MyGuiControlButton button3 = button7;
            color = null;
            buttonIndex = null;
            MyGuiControlButton button8 = new MyGuiControlButton(new Vector2?(panel7.Position + new Vector2(0.002f, (panel7.Size.Y + x) + 0.048f)), MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(new Vector2(224f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, MyTexts.Get(MySpaceTexts.ScreenTerminalProduction_InventoryButton), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button8.Name = "InventoryButton";
            MyGuiControlButton button4 = button8;
            color = null;
            buttonIndex = null;
            MyGuiControlButton button9 = new MyGuiControlButton(new Vector2?(button4.Position + new Vector2(button4.Size.X + x, 0f)), MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(button4.Size), color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, MyTexts.Get(MySpaceTexts.ScreenTerminalProduction_ControlPanelButton), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button9.Name = "ControlPanelButton";
            MyGuiControlButton button5 = button9;
            productionPage.Controls.Add(button);
            productionPage.Controls.Add(button2);
            productionPage.Controls.Add(panel4);
            productionPage.Controls.Add(label5);
            productionPage.Controls.Add(checkbox);
            productionPage.Controls.Add(checkbox2);
            productionPage.Controls.Add(panel5);
            productionPage.Controls.Add(panel6);
            productionPage.Controls.Add(label6);
            productionPage.Controls.Add(button3);
            productionPage.Controls.Add(panel7);
            productionPage.Controls.Add(button4);
            productionPage.Controls.Add(button5);
        }

        private void CreateProperties()
        {
            if (this.m_controllerProperties == null)
            {
                this.m_controllerProperties = new MyTerminalPropertiesController();
            }
            else
            {
                this.m_controllerProperties.Close();
            }
            this.m_controllerProperties.Init(this.m_propertiesTopMenuParent, this.m_propertiesTableParent, InteractedEntity, m_openInventoryInteractedEntity, IsRemote);
            if (this.m_propertiesTableParent != null)
            {
                this.m_propertiesTableParent.Visible = this.m_initialPage == MyTerminalPageEnum.Properties;
            }
        }

        private void CreatePropertiesPageControls(MyGuiControlParent menuParent, MyGuiControlParent panelParent)
        {
            this.m_propertiesTableParent.Name = "PropertiesTable";
            this.m_propertiesTopMenuParent.Name = "PropertiesTopMenu";
            MyGuiControlCombobox combobox1 = new MyGuiControlCombobox();
            combobox1.Position = new Vector2(0f, 0f);
            combobox1.Size = new Vector2(0.25f, 0.1f);
            combobox1.Name = "ShipsInRange";
            combobox1.Visible = false;
            combobox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            MyGuiControlCombobox control = combobox1;
            control.SetToolTip(MySpaceTexts.ScreenTerminal_ShipCombobox);
            MyGuiControlButton button1 = new MyGuiControlButton();
            button1.Position = new Vector2(0.258f, 0.004f);
            button1.Size = new Vector2(0.2f, 0.05f);
            button1.Name = "SelectShip";
            button1.Text = MyTexts.GetString(MySpaceTexts.Terminal_RemoteControl_Button);
            button1.TextScale = 0.7005405f;
            button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            button1.VisualStyle = MyGuiControlButtonStyleEnum.Small;
            MyGuiControlButton button = button1;
            button.SetToolTip(MySpaceTexts.ScreenTerminal_ShipList);
            menuParent.Controls.Add(control);
            menuParent.Controls.Add(button);
            MyGuiControlSeparatorList list = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            list.AddVertical(new Vector2(0.164f, -0.011f), 0.676f, 0.002f, color);
            panelParent.Controls.Add(list);
            Vector2? position = null;
            position = null;
            color = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text1 = new MyGuiControlMultilineText(position, position, color, "Blue", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, visibleLinesCount, false, false, null, textPadding);
            text1.Name = "InfoHelpMultilineText";
            text1.Position = new Vector2(0.186f, -0.012f);
            text1.Size = new Vector2(0.3f, 0.68f);
            text1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text1.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text1.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text1.Text = new StringBuilder(MyTexts.GetString(MySpaceTexts.RemoteAccess_Description));
            MyGuiControlMultilineText text = text1;
            panelParent.Controls.Add(text);
            MyGuiControlTable table1 = new MyGuiControlTable();
            table1.Position = new Vector2(-0.142f, -0.01f);
            table1.Size = new Vector2(0.582f, 0.88f);
            table1.Name = "ShipsData";
            table1.ColumnsCount = 5;
            table1.VisibleRowsCount = 0x13;
            table1.HeaderVisible = true;
            table1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            MyGuiControlTable table = table1;
            table.SetCustomColumnWidths(new float[] { 0.267f, 0.15f, 0.15f, 0.22f, 0.22f });
            table.SetColumnName(0, MyTexts.Get(MySpaceTexts.TerminalName));
            table.SetColumnName(3, MyTexts.Get(MySpaceTexts.TerminalControl));
            table.SetColumnName(1, MyTexts.Get(MySpaceTexts.TerminalDistance));
            table.SetColumnName(2, MyTexts.Get(MySpaceTexts.TerminalStatus));
            table.SetColumnName(4, MyTexts.Get(MySpaceTexts.TerminalAccess));
            table.SetColumnComparison(0, (a, b) => a.Text.CompareTo(b.Text));
            table.SetColumnComparison(1, (a, b) => ((double) a.UserData).CompareTo((double) b.UserData));
            panelParent.Controls.Add(table);
            panelParent.Visible = false;
            IsRemote = false;
        }

        private void CreateTabs()
        {
            MyGuiControlTabControl control1 = new MyGuiControlTabControl();
            control1.Position = new Vector2(-0.001f, -0.367f);
            control1.Size = new Vector2(0.907f, 0.78f);
            control1.Name = "TerminalTabs";
            control1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            this.m_terminalTabs = control1;
            if (MyFakes.ENABLE_COMMUNICATION)
            {
                this.m_terminalTabs.TabButtonScale = 0.875f;
            }
            MyGuiControlTabPage tabSubControl = this.m_terminalTabs.GetTabSubControl(0);
            MyGuiControlTabPage page = this.m_terminalTabs.GetTabSubControl(1);
            MyGuiControlTabPage productionPage = this.m_terminalTabs.GetTabSubControl(2);
            MyGuiControlTabPage infoPage = this.m_terminalTabs.GetTabSubControl(3);
            MyGuiControlTabPage page5 = this.m_terminalTabs.GetTabSubControl(4);
            MyGuiControlTabPage chatPage = null;
            if (MyFakes.ENABLE_COMMUNICATION)
            {
                chatPage = this.m_terminalTabs.GetTabSubControl(5);
            }
            MyGuiControlTabPage gpsPage = null;
            if (MyFakes.ENABLE_GPS)
            {
                gpsPage = this.m_terminalTabs.GetTabSubControl(6);
                this.m_terminalTabs.TabButtonScale = 0.75f;
            }
            this.CreateInventoryPageControls(tabSubControl);
            this.CreateControlPanelPageControls(page);
            this.CreateProductionPageControls(productionPage);
            this.CreateInfoPageControls(infoPage);
            this.CreateFactionsPageControls(page5);
            if (MyFakes.ENABLE_GPS)
            {
                this.CreateGpsPageControls(gpsPage);
            }
            if (MyFakes.ENABLE_COMMUNICATION)
            {
                this.CreateChatPageControls(chatPage);
            }
            MyCubeGrid mainGrid = (InteractedEntity != null) ? (InteractedEntity.Parent as MyCubeGrid) : null;
            this.m_colorHelper.Init(mainGrid);
            if (this.m_controllerInventory == null)
            {
                this.m_controllerInventory = new MyTerminalInventoryController();
            }
            else
            {
                this.m_controllerInventory.Close();
            }
            if (this.m_controllerControlPanel == null)
            {
                this.m_controllerControlPanel = new MyTerminalControlPanel();
            }
            else
            {
                this.m_controllerControlPanel.Close();
            }
            if (this.m_controllerProduction == null)
            {
                this.m_controllerProduction = new MyTerminalProductionController();
            }
            else
            {
                this.m_controllerProduction.Close();
            }
            if (this.m_controllerInfo == null)
            {
                this.m_controllerInfo = new MyTerminalInfoController();
            }
            else
            {
                this.m_controllerInfo.Close();
            }
            if (this.m_controllerFactions == null)
            {
                this.m_controllerFactions = new MyTerminalFactionController();
            }
            else
            {
                this.m_controllerFactions.Close();
            }
            if (MyFakes.ENABLE_GPS)
            {
                if (this.m_controllerGps == null)
                {
                    this.m_controllerGps = new MyTerminalGpsController();
                }
                else
                {
                    this.m_controllerGps.Close();
                }
            }
            if (MyFakes.ENABLE_COMMUNICATION)
            {
                if (this.m_controllerChat == null)
                {
                    this.m_controllerChat = new MyTerminalChatController();
                }
                else
                {
                    this.m_controllerChat.Close();
                }
            }
            this.m_controllerInventory.Init(tabSubControl, this.m_user, InteractedEntity, this.m_colorHelper);
            this.m_controllerControlPanel.Init(page, MySession.Static.LocalHumanPlayer, mainGrid, InteractedEntity as MyTerminalBlock, this.m_colorHelper);
            this.m_controllerProduction.Init(productionPage, mainGrid, InteractedEntity as MyCubeBlock);
            this.m_controllerInfo.Init(infoPage, (InteractedEntity != null) ? (InteractedEntity.Parent as MyCubeGrid) : null);
            this.m_controllerFactions.Init(page5);
            if (MyFakes.ENABLE_GPS)
            {
                this.m_controllerGps.Init(gpsPage);
            }
            if (MyFakes.ENABLE_COMMUNICATION)
            {
                this.m_controllerChat.Init(chatPage);
            }
            this.m_terminalTabs.SelectedPage = (int) this.m_initialPage;
            if ((this.m_terminalTabs.SelectedPage != -1) && !this.m_terminalTabs.GetTabSubControl(this.m_terminalTabs.SelectedPage).Enabled)
            {
                this.m_terminalTabs.SelectedPage = this.m_terminalTabs.Controls.IndexOf(page);
            }
            base.CloseButtonEnabled = true;
            base.SetDefaultCloseButtonOffset();
            this.Controls.Add(this.m_terminalTabs);
            if (MyFakes.ENABLE_TERMINAL_PROPERTIES)
            {
                this.m_terminalTabs.OnPageChanged += new Action(this.tabs_OnPageChanged);
            }
        }

        public override bool Draw()
        {
            if (this.m_controllerInfo != null)
            {
                this.m_controllerInfo.UpdateBeforeDraw();
            }
            if (this.m_controllerInventory != null)
            {
                this.m_controllerInventory.UpdateBeforeDraw();
            }
            return base.Draw();
        }

        public static MyTerminalPageEnum GetCurrentScreen() => 
            (!IsOpen ? MyTerminalPageEnum.None : ((MyTerminalPageEnum) m_instance.m_terminalTabs.SelectedPage));

        public override string GetFriendlyName() => 
            "MyGuiScreenTerminal";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Delete) && (this.m_terminalTabs.SelectedPage == 6))
            {
                this.m_controllerGps.OnDelKeyPressed();
            }
            base.HandleInput(receivedFocusInThisUpdate);
        }

        public override void HandleUnhandledInput(bool receivedFocusInThisUpdate)
        {
            bool flag1 = base.FocusedControl is MyGuiControlTextbox;
            if (!flag1 && (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.TERMINAL) || MyInput.Static.IsNewGameControlPressed(MyControlsSpace.USE)))
            {
                MyGuiSoundManager.PlaySound((base.m_closingCueEnum != null) ? base.m_closingCueEnum.Value : GuiSounds.MouseClick);
                this.CloseScreen();
            }
            bool local1 = flag1;
            if (!local1 && MyInput.Static.IsNewGameControlPressed(MyControlsSpace.INVENTORY))
            {
                if (this.m_terminalTabs.SelectedPage != 0)
                {
                    SwitchToInventory(null);
                }
                else
                {
                    MyGuiSoundManager.PlaySound((base.m_closingCueEnum != null) ? base.m_closingCueEnum.Value : GuiSounds.MouseClick);
                    this.CloseScreen();
                }
            }
            bool local2 = local1;
            if (!local2 && MyInput.Static.IsNewGameControlPressed(MyControlsSpace.PAUSE_GAME))
            {
                MySandboxGame.PauseToggle();
            }
            if ((!local2 && (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsKeyPress(MyKeys.A))) && (m_instance.m_terminalTabs.SelectedPage == 1))
            {
                this.m_controllerControlPanel.SelectAllBlocks();
            }
            base.HandleUnhandledInput(receivedFocusInThisUpdate);
        }

        internal static void Hide()
        {
            if (m_instance != null)
            {
                m_instance.CloseScreen();
            }
        }

        public void Info_ShipRenamed()
        {
            this.m_controllerProperties.Refresh();
        }

        private void InfoButton_OnButtonClick(MyGuiControlButton sender)
        {
            MyGuiSandbox.OpenUrlWithFallback(MySteamConstants.URL_HELP_TERMINAL_SCREEN, "Steam Guide", false);
        }

        private void InvokeWhenLoaded(IMyReplicable replicable)
        {
            MyCubeGridReplicable replicable2 = replicable as MyCubeGridReplicable;
            MyTerminalReplicable replicable3 = replicable as MyTerminalReplicable;
            long entityId = 0L;
            if (replicable2 != null)
            {
                entityId = replicable2.Instance.EntityId;
            }
            else
            {
                if (replicable3 == null)
                {
                    return;
                }
                entityId = replicable3.Instance.EntityId;
            }
            foreach (KeyValuePair<long, Action<long>> pair in this.m_requestedEntities)
            {
                if (pair.Value == null)
                {
                    continue;
                }
                if (pair.Key == entityId)
                {
                    pair.Value(entityId);
                }
            }
        }

        private static bool OnAntennaSliderClicked(MyGuiControlSlider arg)
        {
            if (!MyInput.Static.IsAnyCtrlKeyPressed())
            {
                return false;
            }
            float num = MyHudMarkerRender.Denormalize(0f);
            float max = MyHudMarkerRender.Denormalize(1f);
            float num3 = MyHudMarkerRender.Denormalize(arg.Value);
            bool parseAsInteger = true;
            if (parseAsInteger && (Math.Abs(num) < 1f))
            {
                num = 0f;
            }
            MyGuiScreenDialogAmount screen = new MyGuiScreenDialogAmount(num, max, MyCommonTexts.DialogAmount_SetValueCaption, 3, parseAsInteger, new float?(num3), MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity);
            screen.OnConfirmed += delegate (float v) {
                arg.Value = MyHudMarkerRender.Normalize(v);
            };
            MyGuiSandbox.AddScreen(screen);
            return true;
        }

        protected override void OnClosed()
        {
            if (m_instance != null)
            {
                MyGuiScreenGamePlay.ActiveGameplayScreen = null;
                m_interactedEntity = null;
                MyAnalyticsHelper.ReportActivityEnd(m_instance.m_user, "show_terminal");
                if (MyFakes.ENABLE_GPS)
                {
                    this.m_controllerGps.Close();
                }
                this.m_controllerControlPanel.Close();
                if (this.m_controllerInventory != null)
                {
                    this.m_controllerInventory.Close();
                }
                this.m_controllerProduction.Close();
                this.m_controllerInfo.Close();
                this.Controls.Clear();
                this.m_terminalTabs = null;
                this.m_controllerInventory = null;
                if (MyFakes.SHOW_FACTIONS_GUI)
                {
                    this.m_controllerFactions.Close();
                }
                if (MyFakes.ENABLE_TERMINAL_PROPERTIES)
                {
                    this.m_controllerProperties.Close();
                    this.m_controllerProperties.ButtonClicked -= new Action(this.PropertiesButtonClicked);
                    this.m_propertiesTableParent = null;
                    this.m_propertiesTopMenuParent = null;
                }
                if (MyFakes.ENABLE_COMMUNICATION)
                {
                    this.m_controllerChat.Close();
                }
                if (this.m_requestedEntities.Count > 0)
                {
                    MyMultiplayer.GetReplicationClient().OnReplicableReady -= new Action<IMyReplicable>(this.InvokeWhenLoaded);
                }
                foreach (KeyValuePair<long, Action<long>> pair in this.m_requestedEntities)
                {
                    MyReplicationClient replicationClient = MyMultiplayer.GetReplicationClient();
                    if (replicationClient != null)
                    {
                        replicationClient.RequestReplicable(pair.Key, 0, false);
                    }
                }
                this.m_requestedEntities.Clear();
                m_instance = null;
                m_screenOpen = false;
                base.OnClosed();
            }
        }

        private void OnInteractedClose(VRage.Game.Entity.MyEntity entity)
        {
            if (m_interactedEntity != null)
            {
                m_interactedEntity.OnClose -= m_closeHandler;
            }
            Hide();
        }

        public void PropertiesButtonClicked()
        {
            this.m_terminalTabs.SelectedPage = -1;
            this.m_controllerProperties.Refresh();
            this.m_propertiesTableParent.Visible = true;
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            this.CreateFixedTerminalElements();
            this.CreateTabs();
            if (MyFakes.ENABLE_TERMINAL_PROPERTIES)
            {
                this.CreateProperties();
            }
        }

        private void RecreateTabs()
        {
            this.Controls.RemoveControlByName("TerminalTabs");
            this.CreateTabs();
            if (MyFakes.ENABLE_TERMINAL_PROPERTIES)
            {
                this.CreateProperties();
            }
        }

        public override bool RegisterClicks() => 
            true;

        public static void RequestReplicable(long requestedId, long waitForId, Action<long> loadAction)
        {
            MyReplicationClient replicationClient = MyMultiplayer.GetReplicationClient();
            if (((replicationClient != null) && (m_instance != null)) && !m_instance.m_requestedEntities.ContainsKey(requestedId))
            {
                replicationClient.RequestReplicable(requestedId, 0, true);
                if (m_instance.m_requestedEntities.Count == 0)
                {
                    MyMultiplayer.GetReplicationClient().OnReplicableReady += new Action<IMyReplicable>(m_instance.InvokeWhenLoaded);
                }
                m_instance.m_requestedEntities.Add(requestedId, (requestedId == waitForId) ? loadAction : null);
                if ((requestedId != waitForId) && !m_instance.m_requestedEntities.ContainsKey(waitForId))
                {
                    m_instance.m_requestedEntities.Add(waitForId, loadAction);
                }
            }
        }

        public static void Show(MyTerminalPageEnum page, MyCharacter user, VRage.Game.Entity.MyEntity interactedEntity)
        {
            if (MyPerGameSettings.TerminalEnabled && MyPerGameSettings.GUI.EnableTerminalScreen)
            {
                bool flag = MyInput.Static.IsAnyShiftKeyPressed();
                m_instance = new MyGuiScreenTerminal();
                m_instance.m_user = user;
                m_openInventoryInteractedEntity = interactedEntity;
                m_instance.m_initialPage = !MyFakes.ENABLE_TERMINAL_PROPERTIES ? page : (flag ? MyTerminalPageEnum.Properties : page);
                InteractedEntity = interactedEntity;
                m_instance.RecreateControls(true);
                MyGuiScreenGamePlay.ActiveGameplayScreen = m_instance;
                MyGuiSandbox.AddScreen(m_instance);
                m_screenOpen = true;
                string activityFocus = (interactedEntity != null) ? interactedEntity.GetType().Name : "";
                MyAnalyticsHelper.ReportActivityStart(user, "show_terminal", activityFocus, "gui", string.Empty, true);
            }
        }

        public void ShowConnectScreen()
        {
            this.m_terminalTabs.Visible = true;
            this.m_propertiesTableParent.Visible = this.m_terminalTabs.SelectedPage == -1;
            this.m_terminalNotConnected.Visible = false;
        }

        public void ShowDisconnectScreen()
        {
            this.m_terminalTabs.Visible = false;
            this.m_propertiesTableParent.Visible = false;
            this.m_terminalNotConnected.Visible = true;
        }

        public static void SwitchToControlPanelBlock(MyTerminalBlock block)
        {
            m_instance.m_terminalTabs.SelectedPage = 1;
            MyTerminalBlock[] blocks = new MyTerminalBlock[] { block };
            m_instance.m_controllerControlPanel.SelectBlocks(blocks);
        }

        public static void SwitchToInventory(MyTerminalBlock block = null)
        {
            m_instance.m_terminalTabs.SelectedPage = 0;
            if (((m_instance.m_controllerInventory != null) && !ReferenceEquals(m_interactedEntity, block)) && (block != null))
            {
                m_instance.m_controllerInventory.SetSearch(block.DisplayNameText, true);
            }
        }

        public void tabs_OnPageChanged()
        {
            if (MyImeProcessor.Instance != null)
            {
                MyImeProcessor.Instance.Deactivate();
            }
            if (this.m_propertiesTableParent.Visible)
            {
                this.m_propertiesTableParent.Visible = false;
            }
            if ((m_instance.m_terminalTabs.SelectedPage == 0) && (m_instance.m_controllerInventory != null))
            {
                m_instance.m_controllerInventory.Refresh();
            }
            if (!this.m_autoSelectControlSearch)
            {
                this.m_autoSelectControlSearch = true;
            }
            if (m_instance.m_terminalTabs.SelectedPage == 3)
            {
                MyTerminalInfoController controllerInfo = this.m_controllerInfo;
                if (controllerInfo != null)
                {
                    controllerInfo.MarkControlsDirty();
                }
            }
        }

        public override bool Update(bool hasFocus)
        {
            MyCubeGrid parent;
            if (MyFakes.ENABLE_TERMINAL_PROPERTIES)
            {
                if ((this.m_connected && (this.m_terminalTabs.SelectedPage != -1)) && !this.m_controllerProperties.TestConnection())
                {
                    this.m_connected = false;
                    this.ShowDisconnectScreen();
                }
                else if (!this.m_connected && this.m_controllerProperties.TestConnection())
                {
                    this.m_connected = true;
                    this.ShowConnectScreen();
                }
                this.m_controllerProperties.Update();
                if (MyFakes.ENABLE_COMMUNICATION)
                {
                    this.m_controllerChat.Update();
                }
            }
            if ((InteractedEntity == null) || InteractedEntity.Closed)
            {
                parent = null;
            }
            else
            {
                parent = InteractedEntity.Parent as MyCubeGrid;
            }
            MyCubeGrid grid = parent;
            if ((grid != null) && !ReferenceEquals(grid.GridSystems.TerminalSystem, this.m_controllerControlPanel.TerminalSystem))
            {
                if (this.m_controllerControlPanel != null)
                {
                    this.m_controllerControlPanel.Close();
                    MyGuiControlTabPage controlByName = (MyGuiControlTabPage) this.m_terminalTabs.Controls.GetControlByName("PageControlPanel");
                    this.m_controllerControlPanel.Init(controlByName, MySession.Static.LocalHumanPlayer, grid, InteractedEntity as MyTerminalBlock, this.m_colorHelper);
                }
                if (this.m_controllerProduction != null)
                {
                    this.m_controllerProduction.Close();
                    MyGuiControlTabPage tabSubControl = this.m_terminalTabs.GetTabSubControl(2);
                    this.m_controllerProduction.Init(tabSubControl, grid, InteractedEntity as MyCubeBlock);
                }
                if (this.m_controllerInventory != null)
                {
                    this.m_controllerInventory.Close();
                    MyGuiControlTabPage controlByName = (MyGuiControlTabPage) this.m_terminalTabs.Controls.GetControlByName("PageInventory");
                    this.m_controllerInventory.Init(controlByName, this.m_user, InteractedEntity, this.m_colorHelper);
                }
            }
            if ((this.m_connected && (this.m_terminalTabs.SelectedPage == 1)) && this.m_autoSelectControlSearch)
            {
                this.m_autoSelectControlSearch = false;
                base.FocusedControl = this.m_functionalBlockSearchBox.TextBox;
            }
            return base.Update(hasFocus);
        }

        public static VRage.Game.Entity.MyEntity InteractedEntity
        {
            get => 
                m_interactedEntity;
            set
            {
                if (m_interactedEntity != null)
                {
                    m_interactedEntity.OnClose -= m_closeHandler;
                }
                if (m_instance.m_controllerControlPanel != null)
                {
                    m_instance.m_controllerControlPanel.ClearBlockList();
                }
                m_interactedEntity = value;
                if (m_interactedEntity != null)
                {
                    m_interactedEntity.OnClose += m_closeHandler;
                    if (!ReferenceEquals(m_interactedEntity, m_openInventoryInteractedEntity))
                    {
                        m_instance.m_initialPage = MyTerminalPageEnum.ControlPanel;
                    }
                }
                if (m_screenOpen)
                {
                    m_instance.RecreateTabs();
                }
            }
        }

        internal static bool IsOpen =>
            m_screenOpen;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenTerminal.<>c <>9 = new MyGuiScreenTerminal.<>c();
            public static Comparison<MyGuiControlTable.Cell> <>9__51_0;
            public static Comparison<MyGuiControlTable.Cell> <>9__51_1;

            internal int <CreatePropertiesPageControls>b__51_0(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b) => 
                a.Text.CompareTo(b.Text);

            internal int <CreatePropertiesPageControls>b__51_1(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b) => 
                ((double) a.UserData).CompareTo((double) b.UserData);
        }
    }
}

