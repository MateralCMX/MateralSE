namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Input;
    using VRageMath;
    using VRageRender;

    public class MyGuiScreenLogo : MyGuiScreenBase
    {
        private int? m_startTime;
        private string m_textureName;
        private int m_fadeIn;
        private int m_fadeOut;
        private int m_openTime;
        private float m_scale;

        public MyGuiScreenLogo(string[] textures) : this(textures[0], 0.66f, 300, 300, 0x3e8)
        {
        }

        public MyGuiScreenLogo(string texture, float scale = 0.66f, int fadeIn = 300, int fadeOut = 300, int openTime = 0x3e8) : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2?(Vector2.One), false, null, 0f, 0f)
        {
            this.m_scale = scale;
            this.m_fadeIn = fadeIn;
            this.m_fadeOut = fadeOut;
            this.m_openTime = openTime;
            base.DrawMouseCursor = false;
            this.m_textureName = texture;
            base.m_closeOnEsc = true;
        }

        protected override void Canceling()
        {
            this.m_fadeOut = 0;
            base.Canceling();
        }

        public override unsafe bool Draw()
        {
            Rectangle rectangle2;
            Rectangle fullscreenRectangle = MyGuiManager.GetFullscreenRectangle();
            MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", fullscreenRectangle, Color.Black, true);
            MyGuiManager.GetSafeAspectRatioFullScreenPictureSize(MyGuiConstants.LOADING_BACKGROUND_TEXTURE_REAL_SIZE, out rectangle2);
            ((Rectangle*) ref rectangle2).Inflate(-((int) ((rectangle2.Width * (1f - this.m_scale)) / 2f)), -((int) ((rectangle2.Height * (1f - this.m_scale)) / 2f)));
            Color color = new Color(0.95f, 0.95f, 0.95f, 1f);
            MyGuiManager.DrawSpriteBatch(this.m_textureName, rectangle2, color * base.m_transitionAlpha, true);
            return true;
        }

        public override string GetFriendlyName() => 
            "Logo screen";

        public override int GetTransitionClosingTime() => 
            this.m_fadeOut;

        public override int GetTransitionOpeningTime() => 
            this.m_fadeIn;

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if ((MyInput.Static.IsNewLeftMousePressed() || (MyInput.Static.IsNewRightMousePressed() || MyInput.Static.IsNewKeyPressed(MyKeys.Space))) || MyInput.Static.IsNewKeyPressed(MyKeys.Enter))
            {
                this.Canceling();
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();
        }

        public override void UnloadContent()
        {
            MyRenderProxy.UnloadTexture(this.m_textureName);
            base.UnloadContent();
        }

        public override bool Update(bool hasFocus)
        {
            if (!base.Update(hasFocus))
            {
                return false;
            }
            if ((base.State == MyGuiScreenState.OPENED) && (this.m_startTime == null))
            {
                this.m_startTime = new int?(MyGuiManager.TotalTimeInMilliseconds);
            }
            if (this.m_startTime != null)
            {
                int? nullable1;
                int? startTime = this.m_startTime;
                int openTime = this.m_openTime;
                if (startTime != null)
                {
                    nullable1 = new int?(startTime.GetValueOrDefault() + openTime);
                }
                else
                {
                    nullable1 = null;
                }
                int? nullable = nullable1;
                if ((MyGuiManager.TotalTimeInMilliseconds > nullable.GetValueOrDefault()) & (nullable != null))
                {
                    this.CloseScreen();
                }
            }
            return true;
        }
    }
}

