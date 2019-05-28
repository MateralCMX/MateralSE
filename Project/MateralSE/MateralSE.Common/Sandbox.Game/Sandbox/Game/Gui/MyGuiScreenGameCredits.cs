namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Game;
    using Sandbox.Game.GUI;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenGameCredits : MyGuiScreenBase
    {
        private Color color;
        private const float NUMBER_OF_SECONDS_TO_SCROLL_THROUGH_WHOLE_SCREEN = 30f;
        private float m_movementSpeedMultiplier;
        private float m_scrollingPositionY;
        private string m_keenswhLogoTexture;
        private float m_startTimeInMilliseconds;

        public MyGuiScreenGameCredits() : base(new Vector2?(Vector2.Zero), nullable, nullable2, false, null, 0f, 0f)
        {
            this.color = new Color(0xff, 0xff, 0xff, 220);
            this.m_movementSpeedMultiplier = 1f;
            this.m_startTimeInMilliseconds = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            this.RecreateControls(true);
        }

        private Color ChangeTextAlpha(Color origColor, float coordY)
        {
            float num = 0.05f;
            float num2 = 0.3f;
            float num3 = MathHelper.Clamp((float) ((coordY - num) / (num2 - num)), (float) 0f, (float) 1f);
            return (origColor * num3);
        }

        public override bool Draw()
        {
            if (!base.Draw())
            {
                return false;
            }
            float scrollingPositionY = this.m_scrollingPositionY;
            string font = "GameCredits";
            int num2 = 0;
            while (num2 < MyPerGameSettings.Credits.Departments.Count)
            {
                MyGuiManager.DrawString(font, new StringBuilder(MyStatControlText.SubstituteTexts(MyPerGameSettings.Credits.Departments[num2].Name.ToString(), null)), new Vector2(0.5f, scrollingPositionY), 0.78f, new Color?(this.ChangeTextAlpha(this.color, scrollingPositionY)), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, float.PositiveInfinity);
                scrollingPositionY += 0.05f;
                int num3 = 0;
                while (true)
                {
                    if (num3 >= MyPerGameSettings.Credits.Departments[num2].Persons.Count)
                    {
                        MyCreditsDepartment department = MyPerGameSettings.Credits.Departments[num2];
                        if (department.LogoTexture != null)
                        {
                            scrollingPositionY += department.LogoOffsetPre;
                            if (department.LogoNormalizedSize != null)
                            {
                                MyGuiManager.DrawSpriteBatch(department.LogoTexture, new Vector2(0.5f, scrollingPositionY), department.LogoNormalizedSize.Value, this.ChangeTextAlpha(this.color, scrollingPositionY), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, true);
                            }
                            else
                            {
                                if (department.LogoScale == null)
                                {
                                    throw new InvalidBranchException();
                                }
                                MyGuiManager.DrawSpriteBatch(department.LogoTexture, new Vector2(0.5f, scrollingPositionY), department.LogoScale.Value, this.ChangeTextAlpha(this.color, scrollingPositionY), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0f, new Vector2?(Vector2.Zero), true);
                            }
                            scrollingPositionY += department.LogoOffsetPost;
                        }
                        scrollingPositionY += 0.04f;
                        num2++;
                        break;
                    }
                    MyGuiManager.DrawString(font, new StringBuilder(MyStatControlText.SubstituteTexts(MyPerGameSettings.Credits.Departments[num2].Persons[num3].Name.ToString(), null)), new Vector2(0.5f, scrollingPositionY), 1.04f, new Color?(this.ChangeTextAlpha(this.color, scrollingPositionY)), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, float.PositiveInfinity);
                    scrollingPositionY += 0.05f;
                    num3++;
                }
            }
            scrollingPositionY += 0.05f;
            int num4 = 0;
            while (num4 < MyPerGameSettings.Credits.CreditNotices.Count)
            {
                MyCreditsNotice notice = MyPerGameSettings.Credits.CreditNotices[num4];
                if (notice.LogoTexture != null)
                {
                    if (notice.LogoNormalizedSize != null)
                    {
                        MyGuiManager.DrawSpriteBatch(notice.LogoTexture, new Vector2(0.5f, scrollingPositionY), notice.LogoNormalizedSize.Value, this.ChangeTextAlpha(this.color, scrollingPositionY), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, true);
                    }
                    else
                    {
                        if (notice.LogoScale == null)
                        {
                            throw new InvalidBranchException();
                        }
                        MyGuiManager.DrawSpriteBatch(notice.LogoTexture, new Vector2(0.5f, scrollingPositionY), notice.LogoScale.Value, this.ChangeTextAlpha(this.color, scrollingPositionY), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0f, new Vector2?(Vector2.Zero), true);
                    }
                    scrollingPositionY += notice.LogoOffset;
                }
                int num5 = 0;
                while (true)
                {
                    if (num5 >= notice.CreditNoticeLines.Count)
                    {
                        scrollingPositionY += 0.15f;
                        num4++;
                        break;
                    }
                    MyGuiManager.DrawString(font, new StringBuilder(MyStatControlText.SubstituteTexts(notice.CreditNoticeLines[num5].ToString(), null)), new Vector2(0.5f, scrollingPositionY), 0.78f, new Color?(this.ChangeTextAlpha(this.color, scrollingPositionY)), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, float.PositiveInfinity);
                    scrollingPositionY += 0.025f;
                    num5++;
                }
            }
            if (scrollingPositionY <= 0f)
            {
                this.ResetScrollingPositionY(0f);
            }
            MyGuiSandbox.DrawGameLogoHandler(base.m_transitionAlpha, MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, 0x2c, 0x44));
            return true;
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenGameCredits";

        public Vector2 GetScreenLeftTopPosition()
        {
            MyGuiManager.GetSafeFullscreenRectangle();
            return MyGuiManager.GetNormalizedCoordinateFromScreenCoordinate_FULLSCREEN(new Vector2(25f * MyGuiManager.GetSafeScreenScale(), 25f * MyGuiManager.GetSafeScreenScale()));
        }

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if (MyInput.Static.ENABLE_DEVELOPER_KEYS)
            {
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Down))
                {
                    this.m_movementSpeedMultiplier = 10f;
                }
                else if (MyInput.Static.IsNewKeyReleased(MyKeys.Down))
                {
                    this.m_movementSpeedMultiplier = 1f;
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Up))
                {
                    this.m_movementSpeedMultiplier = -10f;
                }
                else if (MyInput.Static.IsNewKeyReleased(MyKeys.Up))
                {
                    this.m_movementSpeedMultiplier = 1f;
                }
            }
        }

        public override void LoadContent()
        {
            base.DrawMouseCursor = false;
            base.m_closeOnEsc = true;
            this.m_keenswhLogoTexture = @"Textures\GUI\GameLogoLarge.dds";
            this.ResetScrollingPositionY(0f);
            base.LoadContent();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            MyGuiDrawAlignEnum originAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            Vector4? backgroundColor = null;
            MyGuiControlPanel control = new MyGuiControlPanel(new Vector2?(MyGuiManager.ComputeFullscreenGuiCoordinate(originAlign, 0x36, 0x54)), new Vector2?(MyGuiConstants.TEXTURE_KEEN_LOGO.MinSizeGui), backgroundColor, null, null, originAlign) {
                BackgroundTexture = MyGuiConstants.TEXTURE_KEEN_LOGO
            };
            this.Controls.Add(control);
        }

        private void ResetScrollingPositionY(float offset = 0f)
        {
            this.m_scrollingPositionY = 0.99f + offset;
        }

        public override bool Update(bool hasFocus)
        {
            if (!base.Update(hasFocus))
            {
                return false;
            }
            this.m_scrollingPositionY -= 0.0005555556f * this.m_movementSpeedMultiplier;
            return true;
        }
    }
}

