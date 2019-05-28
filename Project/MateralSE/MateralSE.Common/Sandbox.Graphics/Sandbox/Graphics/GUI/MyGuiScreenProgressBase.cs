namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyGuiScreenProgressBase : MyGuiScreenBase
    {
        private bool m_controlsCreated;
        private bool m_loaded;
        private MyStringId m_progressText;
        private string m_progressTextString;
        private MyStringId? m_cancelText;
        protected MyGuiControlLabel m_progressTextLabel;
        protected MyGuiControlRotatingWheel m_rotatingWheel;
        private string m_wheelTexture;
        [CompilerGenerated]
        private Action ProgressCancelled;
        protected bool ReturnToMainMenuOnError;

        public event Action ProgressCancelled
        {
            [CompilerGenerated] add
            {
                Action progressCancelled = this.ProgressCancelled;
                while (true)
                {
                    Action a = progressCancelled;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    progressCancelled = Interlocked.CompareExchange<Action>(ref this.ProgressCancelled, action3, a);
                    if (ReferenceEquals(progressCancelled, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action progressCancelled = this.ProgressCancelled;
                while (true)
                {
                    Action source = progressCancelled;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    progressCancelled = Interlocked.CompareExchange<Action>(ref this.ProgressCancelled, action3, source);
                    if (ReferenceEquals(progressCancelled, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiScreenProgressBase(MyStringId progressText, MyStringId? cancelText = new MyStringId?(), bool isTopMostScreen = true, bool enableBackgroundFade = true) : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, isTopMostScreen, null, 0f, 0f)
        {
            this.m_progressText = progressText;
            this.m_cancelText = cancelText;
            base.EnabledBackgroundFade = enableBackgroundFade;
            base.DrawMouseCursor = this.m_cancelText != null;
            base.m_closeOnEsc = this.m_cancelText != null;
            base.m_drawEvenWithoutFocus = true;
            base.CanHideOthers = false;
            base.CanBeHidden = false;
            this.RecreateControls(true);
        }

        public void Cancel()
        {
            this.Canceling();
        }

        protected override void Canceling()
        {
            base.Canceling();
            Action progressCancelled = this.ProgressCancelled;
            if (progressCancelled != null)
            {
                progressCancelled();
            }
            progressCancelled = null;
        }

        public override bool Draw()
        {
            if (!this.m_controlsCreated)
            {
                this.LoadControls();
            }
            return base.Draw();
        }

        public override void LoadContent()
        {
            if (!this.m_loaded)
            {
                this.m_loaded = true;
                this.ProgressStart();
            }
            base.LoadContent();
        }

        private void LoadControls()
        {
            Vector4? nullable;
            this.m_wheelTexture = @"Textures\GUI\screens\screen_loading_wheel.dds";
            base.m_size = new Vector2(0.37375f, 0.3066667f);
            MyGuiControlCompositePanel panel1 = new MyGuiControlCompositePanel();
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_LOAD_BORDER;
            MyGuiControlCompositePanel control = panel1;
            control.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            control.Position = new Vector2(0f, 0f);
            control.Size = base.m_size.Value;
            this.Controls.Add(control);
            MyGuiControlSeparatorList list = new MyGuiControlSeparatorList();
            if (this.m_cancelText == null)
            {
                nullable = null;
                list.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.718f) / 2f, (base.m_size.Value.Y / 2f) - 0.256f), base.m_size.Value.X * 0.718f, 0f, nullable);
            }
            nullable = null;
            list.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.718f) / 2f, (base.m_size.Value.Y / 2f) - 0.079f), base.m_size.Value.X * 0.718f, 0f, nullable);
            this.Controls.Add(list);
            Vector2? size = null;
            nullable = null;
            this.m_progressTextLabel = new MyGuiControlLabel(new Vector2(0f, -0.098f), size, null, nullable, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_progressTextLabel.TextEnum = this.m_progressText;
            this.Controls.Add(this.m_progressTextLabel);
            float x = 0f;
            float num2 = 0.015f;
            if (this.m_cancelText == null)
            {
                size = null;
                this.m_rotatingWheel = new MyGuiControlRotatingWheel(new Vector2(-x, num2 - 0.003f), new Vector4?(MyGuiConstants.ROTATING_WHEEL_COLOR), 0.36f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, this.m_wheelTexture, true, true, size, 1.5f);
            }
            else
            {
                size = null;
                this.m_rotatingWheel = new MyGuiControlRotatingWheel(new Vector2(-x, num2 - 0.028f), new Vector4?(MyGuiConstants.ROTATING_WHEEL_COLOR), 0.36f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, this.m_wheelTexture, true, true, size, 1.5f);
            }
            this.Controls.Add(this.m_rotatingWheel);
            if (this.m_cancelText != null)
            {
                nullable = null;
                int? buttonIndex = null;
                MyGuiControlButton button = new MyGuiControlButton(new Vector2(x, num2 + 0.069f), MyGuiControlButtonStyleEnum.ToolbarButton, new Vector2?(MyGuiConstants.BACK_BUTTON_SIZE), nullable, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(this.m_cancelText.Value), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancelClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
                this.Controls.Add(button);
            }
            this.m_controlsCreated = true;
        }

        protected virtual void OnCancelClick(MyGuiControlButton sender)
        {
            this.Canceling();
        }

        protected abstract void ProgressStart();
        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            this.m_controlsCreated = false;
            this.LoadControls();
        }

        public override void UnloadContent()
        {
            this.m_loaded = false;
            base.UnloadContent();
        }

        public MyGuiControlRotatingWheel RotatingWheel =>
            this.m_rotatingWheel;

        public MyStringId ProgressText
        {
            get => 
                this.m_progressText;
            set
            {
                if (this.m_progressText != value)
                {
                    this.m_progressText = value;
                    this.m_progressTextLabel.TextEnum = value;
                }
            }
        }

        public string ProgressTextString
        {
            get => 
                this.m_progressTextString;
            set
            {
                if (this.m_progressTextString != value)
                {
                    this.m_progressTextString = value;
                    this.m_progressTextLabel.Text = value;
                }
            }
        }
    }
}

