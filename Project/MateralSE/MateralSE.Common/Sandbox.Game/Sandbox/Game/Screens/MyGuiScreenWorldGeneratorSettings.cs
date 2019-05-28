namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGuiScreenWorldGeneratorSettings : MyGuiScreenBase
    {
        private MyGuiScreenWorldSettings m_parent;
        private int? m_asteroidAmount;
        private MyGuiControlButton m_okButton;
        private MyGuiControlButton m_cancelButton;
        private MyGuiControlCombobox m_asteroidAmountCombo;
        private MyGuiControlLabel m_asteroidAmountLabel;
        [CompilerGenerated]
        private Action OnOkButtonClicked;

        public event Action OnOkButtonClicked
        {
            [CompilerGenerated] add
            {
                Action onOkButtonClicked = this.OnOkButtonClicked;
                while (true)
                {
                    Action a = onOkButtonClicked;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onOkButtonClicked = Interlocked.CompareExchange<Action>(ref this.OnOkButtonClicked, action3, a);
                    if (ReferenceEquals(onOkButtonClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onOkButtonClicked = this.OnOkButtonClicked;
                while (true)
                {
                    Action source = onOkButtonClicked;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onOkButtonClicked = Interlocked.CompareExchange<Action>(ref this.OnOkButtonClicked, action3, source);
                    if (ReferenceEquals(onOkButtonClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiScreenWorldGeneratorSettings(MyGuiScreenWorldSettings parent) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2?(CalcSize()), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.m_parent = parent;
            this.RecreateControls(true);
            this.SetSettingsToControls();
        }

        protected MyGuiControlLabel AddCaption(MyStringId textEnum, VRageMath.Vector4? captionTextColor = new VRageMath.Vector4?(), Vector2? captionOffset = new Vector2?(), float captionScale = 0.8f) => 
            base.AddCaption(MyTexts.GetString(textEnum), captionTextColor, captionOffset, captionScale);

        public static Vector2 CalcSize() => 
            new Vector2(0.65f, 0.3f);

        private void CancelButtonClicked(object sender)
        {
            this.CloseScreen();
        }

        private MyFloraDensityEnum FloraDensityEnumKey(int floraDensity)
        {
            MyFloraDensityEnum enum2 = (MyFloraDensityEnum) floraDensity;
            return (!System.Enum.IsDefined(typeof(MyFloraDensityEnum), enum2) ? MyFloraDensityEnum.LOW : ((MyFloraDensityEnum) floraDensity));
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenWorldGeneratorSettings";

        public void GetSettings(MyObjectBuilder_SessionSettings output)
        {
        }

        private void grassDensitySlider_ValueChanged(MyGuiControlSlider slider)
        {
            MyRenderProxy.Settings.User.GrassDensityFactor = slider.Value;
            MyRenderProxy.SetSettingsDirty();
        }

        private void m_asteroidAmountCombo_ItemSelected()
        {
            this.m_asteroidAmount = new int?((int) this.m_asteroidAmountCombo.GetSelectedKey());
        }

        private MyGuiControlLabel MakeLabel(MyStringId textEnum)
        {
            Vector2? position = null;
            position = null;
            return new MyGuiControlLabel(position, position, MyTexts.GetString(textEnum), null, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
        }

        private void OkButtonClicked(object sender)
        {
            if (this.OnOkButtonClicked != null)
            {
                this.OnOkButtonClicked();
            }
            this.CloseScreen();
        }

        public override void RecreateControls(bool constructor)
        {
            int? nullable3;
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            this.AddCaption(MySpaceTexts.ScreenCaptionWorldGeneratorSettings, captionTextColor, captionOffset, 0.8f);
            captionOffset = null;
            captionTextColor = null;
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            this.m_asteroidAmountCombo = new MyGuiControlCombobox(captionOffset, new Vector2(0.309375f, 0.04f), captionTextColor, captionOffset, 10, captionOffset, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
            this.m_asteroidAmountCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_asteroidAmountCombo_ItemSelected);
            this.m_asteroidAmountCombo.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsAsteroidAmount));
            if (MyFakes.ENABLE_ASTEROID_FIELDS)
            {
                nullable3 = null;
                MyStringId? toolTip = null;
                this.m_asteroidAmountCombo.AddItem((long) (-4), MySpaceTexts.WorldSettings_AsteroidAmountProceduralNone, nullable3, toolTip);
                nullable3 = null;
                toolTip = null;
                this.m_asteroidAmountCombo.AddItem(-1L, MySpaceTexts.WorldSettings_AsteroidAmountProceduralLow, nullable3, toolTip);
                nullable3 = null;
                toolTip = null;
                this.m_asteroidAmountCombo.AddItem((long) (-2), MySpaceTexts.WorldSettings_AsteroidAmountProceduralNormal, nullable3, toolTip);
                if (System.Environment.Is64BitProcess)
                {
                    nullable3 = null;
                    toolTip = null;
                    this.m_asteroidAmountCombo.AddItem((long) (-3), MySpaceTexts.WorldSettings_AsteroidAmountProceduralHigh, nullable3, toolTip);
                }
            }
            this.m_asteroidAmountLabel = this.MakeLabel(MySpaceTexts.Asteroid_Amount);
            this.Controls.Add(this.m_asteroidAmountLabel);
            this.Controls.Add(this.m_asteroidAmountCombo);
            int num2 = 0;
            float x = 0.055f;
            Vector2 vector3 = new Vector2(0f, 0.052f);
            Vector2 vector = (-base.m_size.Value / 2f) + new Vector2(x, 0.12f);
            Vector2 vector2 = vector + new Vector2(0.25f, 0f);
            foreach (MyGuiControlBase base2 in this.Controls)
            {
                base2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                if (base2 is MyGuiControlLabel)
                {
                    base2.Position = vector + (vector3 * num2);
                    continue;
                }
                num2++;
                base2.Position = vector2 + (vector3 * num2);
            }
            Vector2 vector4 = (base.m_size.Value / 2f) - new Vector2(0.23f, 0.03f);
            captionTextColor = null;
            nullable3 = null;
            this.m_okButton = new MyGuiControlButton(new Vector2?(vector4 - new Vector2(0.01f, 0f)), MyGuiControlButtonStyleEnum.Default, new Vector2?(MyGuiConstants.BACK_BUTTON_SIZE), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OkButtonClicked), GuiSounds.MouseClick, 1f, nullable3, false);
            captionTextColor = null;
            nullable3 = null;
            this.m_cancelButton = new MyGuiControlButton(new Vector2?(vector4 + new Vector2(0.01f, 0f)), MyGuiControlButtonStyleEnum.Default, new Vector2?(MyGuiConstants.BACK_BUTTON_SIZE), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.CancelButtonClicked), GuiSounds.MouseClick, 1f, nullable3, false);
            this.Controls.Add(this.m_okButton);
            this.Controls.Add(this.m_cancelButton);
        }

        protected virtual void SetSettingsToControls()
        {
        }

        public int AsteroidAmount
        {
            get => 
                ((this.m_asteroidAmount != null) ? this.m_asteroidAmount.Value : -1);
            set
            {
                this.m_asteroidAmount = new int?(value);
                switch (value)
                {
                    case -4:
                        this.m_asteroidAmountCombo.SelectItemByKey((long) (-4), true);
                        return;

                    case -3:
                        this.m_asteroidAmountCombo.SelectItemByKey((long) (-3), true);
                        return;

                    case -2:
                        this.m_asteroidAmountCombo.SelectItemByKey((long) (-2), true);
                        return;

                    case -1:
                        this.m_asteroidAmountCombo.SelectItemByKey(-1L, true);
                        return;

                    case 0:
                        this.m_asteroidAmountCombo.SelectItemByKey(0L, true);
                        return;

                    case 1:
                    case 2:
                    case 3:
                    case 5:
                    case 6:
                        return;

                    case 4:
                        this.m_asteroidAmountCombo.SelectItemByKey(4L, true);
                        return;

                    case 7:
                        this.m_asteroidAmountCombo.SelectItemByKey(7L, true);
                        return;
                }
                if (value == 0x10)
                {
                    this.m_asteroidAmountCombo.SelectItemByKey((long) 0x10, true);
                }
            }
        }

        public enum AsteroidAmountEnum
        {
            None = 0,
            Normal = 4,
            More = 7,
            Many = 0x10,
            ProceduralLow = -1,
            ProceduralNormal = -2,
            ProceduralHigh = -3,
            ProceduralNone = -4
        }

        public enum MyFloraDensityEnum
        {
            NONE = 0,
            LOW = 10,
            MEDIUM = 20,
            HIGH = 30,
            EXTREME = 40
        }
    }
}

