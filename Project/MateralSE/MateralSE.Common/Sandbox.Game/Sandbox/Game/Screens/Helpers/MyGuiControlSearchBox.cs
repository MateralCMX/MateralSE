namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlSearchBox : MyGuiControlParent
    {
        private MyGuiControlLabel m_label;
        private MyGuiControlTextbox m_textbox;
        private MyGuiControlButton m_clearButton;
        [CompilerGenerated]
        private TextChangedDelegate OnTextChanged;

        public event TextChangedDelegate OnTextChanged
        {
            [CompilerGenerated] add
            {
                TextChangedDelegate onTextChanged = this.OnTextChanged;
                while (true)
                {
                    TextChangedDelegate a = onTextChanged;
                    TextChangedDelegate delegate4 = (TextChangedDelegate) Delegate.Combine(a, value);
                    onTextChanged = Interlocked.CompareExchange<TextChangedDelegate>(ref this.OnTextChanged, delegate4, a);
                    if (ReferenceEquals(onTextChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                TextChangedDelegate onTextChanged = this.OnTextChanged;
                while (true)
                {
                    TextChangedDelegate source = onTextChanged;
                    TextChangedDelegate delegate4 = (TextChangedDelegate) Delegate.Remove(source, value);
                    onTextChanged = Interlocked.CompareExchange<TextChangedDelegate>(ref this.OnTextChanged, delegate4, source);
                    if (ReferenceEquals(onTextChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiControlSearchBox(Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), MyGuiDrawAlignEnum originAlign = 4) : base(position, size, nullable, null)
        {
            base.OriginAlign = originAlign;
            float y = 0.004f;
            this.m_textbox = new MyGuiControlTextbox();
            this.m_textbox.VisualStyle = MyGuiControlTextboxStyleEnum.Default;
            this.m_textbox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            this.m_textbox.Position = new Vector2(-base.Size.X / 2f, 0f);
            this.m_textbox.Size = base.Size;
            this.m_textbox.TextChanged += new Action<MyGuiControlTextbox>(this.m_textbox_TextChanged);
            base.Controls.Add(this.m_textbox);
            this.m_label = new MyGuiControlLabel();
            this.m_label.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            this.m_label.Position = new Vector2((-base.Size.X / 2f) + 0.0075f, y);
            this.m_label.Text = MyTexts.GetString(MyCommonTexts.ScreenMods_SearchLabel);
            this.m_label.Font = "DarkBlue";
            base.Controls.Add(this.m_label);
            MyGuiControlButton button1 = new MyGuiControlButton();
            button1.Position = this.m_textbox.Position + new Vector2(this.m_textbox.Size.X - 0.005f, y);
            button1.Size = new Vector2(0.0234f, 0.029466f);
            button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            button1.ActivateOnMouseRelease = true;
            this.m_clearButton = button1;
            this.m_clearButton.VisualStyle = MyGuiControlButtonStyleEnum.Close;
            this.m_clearButton.Size = new Vector2(0.0234f, 0.029466f);
            this.m_clearButton.ButtonClicked += new Action<MyGuiControlButton>(this.m_clearButton_ButtonClicked);
            base.Controls.Add(this.m_clearButton);
        }

        private void m_clearButton_ButtonClicked(MyGuiControlButton obj)
        {
            this.m_textbox.Text = "";
        }

        private void m_textbox_TextChanged(MyGuiControlTextbox obj)
        {
            this.m_label.Visible = string.IsNullOrEmpty(obj.Text);
            if (this.OnTextChanged != null)
            {
                this.OnTextChanged(obj.Text);
            }
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            this.m_label.Position = new Vector2((-base.Size.X / 2f) + 0.0075f, 0f);
            this.m_textbox.Position = new Vector2(-base.Size.X / 2f, 0f);
            this.m_textbox.Size = base.Size;
            this.m_clearButton.Position = this.m_textbox.Position + new Vector2(this.m_textbox.Size.X - 0.005f, 0f);
        }

        public MyGuiControlTextbox TextBox =>
            this.m_textbox;

        public VRageMath.Vector4 SearchLabelColor
        {
            get => 
                ((this.m_label == null) ? VRageMath.Vector4.One : this.m_label.ColorMask);
            set
            {
                if (this.m_label != null)
                {
                    this.m_label.ColorMask = value;
                }
            }
        }

        public string SearchText
        {
            get => 
                this.m_textbox.Text;
            set => 
                (this.m_textbox.Text = value);
        }

        public delegate void TextChangedDelegate(string newText);
    }
}

