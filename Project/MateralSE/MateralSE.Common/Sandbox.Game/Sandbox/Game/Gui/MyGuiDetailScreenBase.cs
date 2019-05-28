namespace Sandbox.Game.Gui
{
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    internal abstract class MyGuiDetailScreenBase : MyGuiBlueprintScreenBase
    {
        protected static readonly Vector2 SCREEN_SIZE = new Vector2(0.4f, 1.2f);
        protected float m_textScale;
        protected string m_blueprintName;
        protected MyGuiControlListbox.Item m_selectedItem;
        protected MyObjectBuilder_Definitions m_loadedPrefab;
        protected MyGuiControlMultilineText m_textField;
        protected MyGuiControlMultilineText m_descriptionField;
        protected MyGuiControlImage m_thumbnailImage;
        protected Action<MyGuiControlListbox.Item> callBack;
        protected MyGuiBlueprintScreenBase m_parent;
        protected MyGuiBlueprintTextDialog m_dialog;
        protected bool m_killScreen;
        protected Vector2 m_offset;
        protected int maxNameLenght;

        public MyGuiDetailScreenBase(bool isTopMostScreen, MyGuiBlueprintScreenBase parent, string thumbnailTexture, MyGuiControlListbox.Item selectedItem, float textScale) : base(new Vector2(0.5f, 0.5f), new Vector2(0.778f, 0.594f), MyGuiConstants.SCREEN_BACKGROUND_COLOR * MySandboxGame.Config.UIBkOpacity, isTopMostScreen)
        {
            this.m_offset = new Vector2(-0.01f, 0f);
            this.maxNameLenght = 40;
            MyGuiControlImage image1 = new MyGuiControlImage();
            image1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            this.m_thumbnailImage = image1;
            this.m_thumbnailImage.SetPadding(new MyGuiBorderThickness(2f, 2f, 2f, 2f));
            this.m_thumbnailImage.SetTexture(thumbnailTexture);
            this.m_thumbnailImage.BorderEnabled = true;
            this.m_thumbnailImage.BorderSize = 1;
            this.m_thumbnailImage.BorderColor = new VRageMath.Vector4(0.235f, 0.274f, 0.314f, 1f);
            this.m_selectedItem = selectedItem;
            this.m_blueprintName = selectedItem.Text.ToString();
            this.m_textScale = textScale;
            this.m_parent = parent;
            base.CloseButtonEnabled = true;
        }

        protected void CallResultCallback(MyGuiControlListbox.Item val)
        {
            this.callBack(val);
        }

        protected override void Canceling()
        {
            this.CallResultCallback(this.m_selectedItem);
            base.Canceling();
        }

        protected abstract void CreateButtons();
        protected void CreateDescription()
        {
            Vector2 position = new Vector2(-0.325f, -0.005f) + this.m_offset;
            Vector2 size = new Vector2(0.67f, 0.155f);
            Vector2 vector3 = new Vector2(0.005f, -0.04f);
            base.AddCompositePanel(MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER, position, size, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            Vector2? offset = new Vector2?(position + vector3);
            this.m_descriptionField = this.CreateMultilineText(new Vector2?(size - (vector3 + new Vector2(0f, 0.045f))), offset, this.m_textScale, false);
            this.RefreshDescriptionField();
        }

        protected MyGuiControlMultilineText CreateMultilineText(Vector2? size = new Vector2?(), Vector2? offset = new Vector2?(), float textScale = 1f, bool selectable = false)
        {
            Vector2 valueOrDefault;
            Vector2? nullable = size;
            if (nullable != null)
            {
                valueOrDefault = nullable.GetValueOrDefault();
            }
            else
            {
                Vector2? nullable2 = base.Size;
                valueOrDefault = (nullable2 != null) ? nullable2.GetValueOrDefault() : new Vector2(0.5f, 0.5f);
            }
            Vector2 vector = valueOrDefault;
            nullable = offset;
            VRageMath.Vector4? backgroundColor = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText control = new MyGuiControlMultilineText(new Vector2?((base.m_currentPosition + (vector / 2f)) + ((nullable != null) ? nullable.GetValueOrDefault() : Vector2.Zero)), new Vector2?(vector), backgroundColor, "Debug", base.m_scale * textScale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, visibleLinesCount, selectable, false, null, textPadding);
            this.Controls.Add(control);
            return control;
        }

        protected void CreateTextField()
        {
            Vector2 position = new Vector2(-0.325f, -0.2f) + this.m_offset;
            Vector2 size = new Vector2(0.175f, 0.175f);
            Vector2 vector3 = new Vector2(0.005f, -0.04f);
            base.AddCompositePanel(MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER, position, size, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            this.m_textField = new MyGuiControlMultilineText();
            Vector2? offset = new Vector2?(position + vector3);
            this.m_textField = this.CreateMultilineText(new Vector2?(size - vector3), offset, this.m_textScale, false);
            this.RefreshTextField();
        }

        protected int GetNumberOfBattlePoints() => 
            ((int) this.m_loadedPrefab.ShipBlueprints[0].Points);

        protected int GetNumberOfBlocks()
        {
            int num = 0;
            foreach (MyObjectBuilder_CubeGrid grid in this.m_loadedPrefab.ShipBlueprints[0].CubeGrids)
            {
                num += grid.CubeBlocks.Count;
            }
            return num;
        }

        protected void OnCloseButton(MyGuiControlButton button)
        {
            this.CloseScreen();
            this.CallResultCallback(this.m_selectedItem);
        }

        public override void RecreateControls(bool constructor)
        {
            if (this.m_loadedPrefab == null)
            {
                this.CloseScreen();
            }
            else
            {
                base.RecreateControls(constructor);
                VRageMath.Vector4? captionTextColor = null;
                base.AddCaption(MyCommonTexts.ScreenCaptionBlueprintDetails, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
                MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
                captionTextColor = null;
                control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.86f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.86f, 0f, captionTextColor);
                this.Controls.Add(control);
                MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
                captionTextColor = null;
                list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.86f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.86f, 0f, captionTextColor);
                this.Controls.Add(list2);
                float textScale = this.m_textScale;
                MyGuiControlButton button1 = MyBlueprintUtils.CreateButton(this, 1f, MyTexts.Get(MySpaceTexts.DetailScreen_Button_Close), new Action<MyGuiControlButton>(this.OnCloseButton), true, new MyStringId?(MySpaceTexts.ToolTipNewsletter_Close), textScale);
                button1.Position = new Vector2(0f, (base.m_size.Value.Y / 2f) - 0.097f);
                button1.VisualStyle = MyGuiControlButtonStyleEnum.Default;
                this.CreateTextField();
                this.CreateDescription();
                this.CreateButtons();
                this.m_thumbnailImage.Position = new Vector2(-0.035f, -0.112f) + this.m_offset;
                this.m_thumbnailImage.Size = new Vector2(0.2f, 0.175f);
                this.Controls.Add(this.m_thumbnailImage);
            }
        }

        protected void RefreshDescriptionField()
        {
            if (this.m_descriptionField != null)
            {
                this.m_descriptionField.Clear();
                string description = this.m_loadedPrefab.ShipBlueprints[0].Description;
                if (description != null)
                {
                    this.m_descriptionField.AppendText(description);
                }
            }
        }

        protected void RefreshTextField()
        {
            if (this.m_textField != null)
            {
                string blueprintName = this.m_blueprintName;
                if (blueprintName.Length > 0x19)
                {
                    blueprintName = blueprintName.Substring(0, 0x19) + "...";
                }
                this.m_textField.Clear();
                this.m_textField.AppendText(MyTexts.GetString(MySpaceTexts.BlueprintInfo_Name) + blueprintName);
                this.m_textField.AppendLine();
                MyCubeSize gridSizeEnum = this.m_loadedPrefab.ShipBlueprints[0].CubeGrids[0].GridSizeEnum;
                this.m_textField.AppendText(MyTexts.GetString(MyCommonTexts.BlockPropertiesText_Type));
                if (this.m_loadedPrefab.ShipBlueprints[0].CubeGrids[0].IsStatic && (gridSizeEnum == MyCubeSize.Large))
                {
                    this.m_textField.AppendText(MyTexts.GetString(MyCommonTexts.DetailStaticGrid));
                }
                else if (gridSizeEnum == MyCubeSize.Small)
                {
                    this.m_textField.AppendText(MyTexts.GetString(MyCommonTexts.DetailSmallGrid));
                }
                else
                {
                    this.m_textField.AppendText(MyTexts.GetString(MyCommonTexts.DetailLargeGrid));
                }
                this.m_textField.AppendLine();
                this.m_textField.AppendText(MyTexts.GetString(MySpaceTexts.BlueprintInfo_NumberOfBlocks) + this.GetNumberOfBlocks());
                this.m_textField.AppendLine();
                this.m_textField.AppendText(MyTexts.GetString(MySpaceTexts.BlueprintInfo_Author) + this.m_loadedPrefab.ShipBlueprints[0].DisplayName);
                this.m_textField.AppendLine();
            }
        }

        public override bool Update(bool hasFocus)
        {
            if (this.m_killScreen)
            {
                this.CallResultCallback(null);
                this.CloseScreen();
            }
            return base.Update(hasFocus);
        }
    }
}

