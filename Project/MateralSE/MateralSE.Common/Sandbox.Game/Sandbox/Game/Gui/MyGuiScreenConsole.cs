namespace Sandbox.Game.GUI
{
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq;
    using System.Text;
    using VRage;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenConsole : MyGuiScreenBase
    {
        private static MyGuiScreenConsole m_instance;
        private MyGuiControlTextbox m_commandLine;
        private MyGuiControlMultilineText m_displayScreen;
        private MyGuiControlContextMenu m_autoComplete;
        private StringBuilder m_commandText;
        private string BufferText;
        private float m_screenScale;
        private Vector2 m_margin;
        private static MyConsoleKeyTimerController[] m_keys;

        public MyGuiScreenConsole() : base(nullable, nullable2, nullable, false, null, 0f, 0f)
        {
            this.m_commandText = new StringBuilder();
            this.BufferText = "";
            Vector2? nullable = null;
            nullable = null;
            base.m_backgroundTexture = MyGuiConstants.TEXTURE_MESSAGEBOX_BACKGROUND_INFO.Texture;
            base.m_backgroundColor = new Vector4(0f, 0f, 0f, 0.75f);
            base.m_position = new Vector2(0.5f, 0.25f);
            this.m_screenScale = (MyGuiManager.GetHudSize().X / MyGuiManager.GetHudSize().Y) / 1.333333f;
            base.m_size = new Vector2(this.m_screenScale, 0.5f);
            this.m_margin = new Vector2(0.06f, 0.04f);
            m_keys = new MyConsoleKeyTimerController[] { new MyConsoleKeyTimerController(MyKeys.Up), new MyConsoleKeyTimerController(MyKeys.Down), new MyConsoleKeyTimerController(MyKeys.Enter) };
        }

        public void autoComplete_ItemClicked(MyGuiControlContextMenu sender, MyGuiControlContextMenu.EventArgs args)
        {
            this.m_commandLine.Text = this.m_commandLine.Text + ((string) this.m_autoComplete.Items[args.ItemIndex].UserData);
            this.m_commandLine.MoveCarriageToEnd();
            base.FocusedControl = this.m_commandLine;
        }

        public void commandLine_TextChanged(MyGuiControlTextbox sender)
        {
            string text = sender.Text;
            if ((text.Length == 0) || !sender.Text.ElementAt<char>((sender.Text.Length - 1)).Equals('.'))
            {
                if (this.m_autoComplete.Enabled)
                {
                    this.m_autoComplete.Enabled = false;
                    this.m_autoComplete.Deactivate();
                }
            }
            else
            {
                MyCommand command;
                if (MyConsole.TryGetCommand(text.Substring(0, text.Length - 1), out command))
                {
                    this.m_autoComplete.CreateNewContextMenu();
                    this.m_autoComplete.Position = new Vector2(((1f - this.m_screenScale) / 2f) + this.m_margin.X, base.m_size.Value.Y - (2f * this.m_margin.Y)) + MyGuiManager.MeasureString("Debug", new StringBuilder(this.m_commandLine.Text), this.m_commandLine.TextScaleWithLanguage);
                    foreach (string str2 in command.Methods)
                    {
                        this.m_autoComplete.AddItem(new StringBuilder(str2).Append(" ").Append(command.GetHint(str2)), "", "", str2);
                    }
                    this.m_autoComplete.Enabled = true;
                    this.m_autoComplete.Activate(false);
                }
            }
        }

        public override string GetFriendlyName() => 
            "Console Screen";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            bool flag = false;
            if ((ReferenceEquals(base.FocusedControl, this.m_commandLine) && MyInput.Static.IsKeyPress(MyKeys.Up)) && !this.m_autoComplete.Visible)
            {
                if (this.IsEnoughDelay(MyConsoleKeys.UP, 100) && !this.m_autoComplete.Visible)
                {
                    this.UpdateLastKeyPressTimes(MyConsoleKeys.UP);
                    if (MyConsole.GetLine() == "")
                    {
                        this.BufferText = this.m_commandLine.Text;
                    }
                    MyConsole.PreviousLine();
                    this.m_commandLine.Text = (MyConsole.GetLine() != "") ? MyConsole.GetLine() : this.BufferText;
                    this.m_commandLine.MoveCarriageToEnd();
                }
                flag = true;
            }
            if ((ReferenceEquals(base.FocusedControl, this.m_commandLine) && MyInput.Static.IsKeyPress(MyKeys.Down)) && !this.m_autoComplete.Visible)
            {
                if (this.IsEnoughDelay(MyConsoleKeys.DOWN, 100) && !this.m_autoComplete.Visible)
                {
                    this.UpdateLastKeyPressTimes(MyConsoleKeys.DOWN);
                    if (MyConsole.GetLine() == "")
                    {
                        this.BufferText = this.m_commandLine.Text;
                    }
                    MyConsole.NextLine();
                    this.m_commandLine.Text = (MyConsole.GetLine() != "") ? MyConsole.GetLine() : this.BufferText;
                    this.m_commandLine.MoveCarriageToEnd();
                }
                flag = true;
            }
            if ((ReferenceEquals(base.FocusedControl, this.m_commandLine) && (MyInput.Static.IsKeyPress(MyKeys.Enter) && (!this.m_commandLine.Text.Equals("") && !this.m_autoComplete.Visible))) && this.IsEnoughDelay(MyConsoleKeys.ENTER, 100))
            {
                this.UpdateLastKeyPressTimes(MyConsoleKeys.ENTER);
                if (!this.m_autoComplete.Visible)
                {
                    this.BufferText = "";
                    MyConsole.ParseCommand(this.m_commandLine.Text);
                    MyConsole.NextLine();
                    this.m_displayScreen.Text = MyConsole.DisplayScreen;
                    this.m_displayScreen.ScrollbarOffsetV = 1f;
                    this.m_commandLine.Text = "";
                    flag = true;
                }
            }
            if (!flag)
            {
                base.HandleInput(receivedFocusInThisUpdate);
            }
        }

        private bool IsEnoughDelay(MyConsoleKeys key, int forcedDelay)
        {
            MyConsoleKeyTimerController controller = m_keys[(int) key];
            return ((controller != null) ? ((MyGuiManager.TotalTimeInMilliseconds - controller.LastKeyPressTime) > forcedDelay) : true);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
        }

        public override void RecreateControls(bool constructor)
        {
            this.m_screenScale = (MyGuiManager.GetHudSize().X / MyGuiManager.GetHudSize().Y) / 1.333333f;
            base.m_size = new Vector2(this.m_screenScale, 0.5f);
            base.RecreateControls(constructor);
            Vector4 vector = new Vector4(1f, 1f, 0f, 1f);
            float num = 1f;
            this.m_commandLine = new MyGuiControlTextbox(new Vector2(0f, 0.25f), null, 0x200, new Vector4?(vector), 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_commandLine.Position -= new Vector2(0f, this.m_commandLine.Size.Y + (this.m_margin.Y / 2f));
            this.m_commandLine.Size = new Vector2(this.m_screenScale, this.m_commandLine.Size.Y) - (2f * this.m_margin);
            this.m_commandLine.ColorMask = new Vector4(0f, 0f, 0f, 0.5f);
            this.m_commandLine.VisualStyle = MyGuiControlTextboxStyleEnum.Debug;
            this.m_commandLine.TextChanged += new Action<MyGuiControlTextbox>(this.commandLine_TextChanged);
            this.m_commandLine.Name = "CommandLine";
            this.m_autoComplete = new MyGuiControlContextMenu();
            this.m_autoComplete.ItemClicked += new Action<MyGuiControlContextMenu, MyGuiControlContextMenu.EventArgs>(this.autoComplete_ItemClicked);
            this.m_autoComplete.Deactivate();
            this.m_autoComplete.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_autoComplete.ColorMask = new Vector4(0f, 0f, 0f, 0.5f);
            this.m_autoComplete.AllowKeyboardNavigation = true;
            this.m_autoComplete.Name = "AutoComplete";
            Vector4? backgroundColor = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            this.m_displayScreen = new MyGuiControlMultilineText(new Vector2?(new Vector2(-0.5f * this.m_screenScale, -0.25f) + this.m_margin), new Vector2?(new Vector2(this.m_screenScale, 0.5f - this.m_commandLine.Size.Y) - (2f * this.m_margin)), backgroundColor, "Debug", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, visibleLinesCount, true, false, null, textPadding);
            this.m_displayScreen.TextColor = Color.Yellow;
            this.m_displayScreen.TextScale = num;
            this.m_displayScreen.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_displayScreen.Text = MyConsole.DisplayScreen;
            this.m_displayScreen.ColorMask = new Vector4(0f, 0f, 0f, 0.5f);
            this.m_displayScreen.Name = "DisplayScreen";
            this.Controls.Add(this.m_displayScreen);
            this.Controls.Add(this.m_commandLine);
            this.Controls.Add(this.m_autoComplete);
        }

        public static void Show()
        {
            m_instance = new MyGuiScreenConsole();
            m_instance.RecreateControls(true);
            MyGuiSandbox.AddScreen(m_instance);
        }

        private void UpdateLastKeyPressTimes(MyConsoleKeys key)
        {
            MyConsoleKeyTimerController controller = m_keys[(int) key];
            if (controller != null)
            {
                controller.LastKeyPressTime = MyGuiManager.TotalTimeInMilliseconds;
            }
        }

        private enum MyConsoleKeys
        {
            UP,
            DOWN,
            ENTER
        }

        private class MyConsoleKeyTimerController
        {
            public MyKeys Key;
            public int LastKeyPressTime;

            public MyConsoleKeyTimerController(MyKeys key)
            {
                this.Key = key;
                this.LastKeyPressTime = -60000;
            }
        }
    }
}

