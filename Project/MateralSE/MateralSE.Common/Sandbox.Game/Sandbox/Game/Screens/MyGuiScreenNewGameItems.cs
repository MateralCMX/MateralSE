namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage;
    using VRage.Audio;
    using VRage.Data.Audio;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenNewGameItems : MyGuiScreenBase
    {
        private List<MyGameInventoryItem> items;
        private MyGuiControlLabel itemName;
        private MyGuiControlImage itemBackground;
        private MyGuiControlImage itemImage;

        public MyGuiScreenNewGameItems(List<MyGameInventoryItem> newItems) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.41f, 0.4f), true, null, 0f, 0f)
        {
            this.items = newItems;
            MyCueId cueId = MySoundPair.GetCueId("ArcNewItemImpact");
            MyAudio.Static.PlaySound(cueId, null, MySoundDimensions.D2, false, false);
            base.EnabledBackgroundFade = true;
            base.CloseButtonEnabled = true;
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenNewGameItems";

        private void LoadFirstItem()
        {
            MyGameInventoryItem item = this.items.FirstOrDefault<MyGameInventoryItem>();
            if (item != null)
            {
                this.itemName.Text = item.ItemDefinition.Name;
                this.itemBackground.ColorMask = string.IsNullOrEmpty(item.ItemDefinition.BackgroundColor) ? VRageMath.Vector4.One : ColorExtensions.HexToVector4(item.ItemDefinition.BackgroundColor);
                string[] textures = new string[] { @"Textures\GUI\Blank.dds" };
                if (!string.IsNullOrEmpty(item.ItemDefinition.IconTexture))
                {
                    textures[0] = item.ItemDefinition.IconTexture;
                }
                this.itemImage.SetTextures(textures);
            }
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            MyGuiScreenGamePlay.ActiveGameplayScreen = null;
        }

        private void OnOkButtonClick(MyGuiControlButton obj)
        {
            if (this.items.Count<MyGameInventoryItem>() <= 1)
            {
                this.CloseScreen();
            }
            else
            {
                this.items.RemoveAt(0);
                this.LoadFirstItem();
            }
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.ScreenCaptionClaimGameItem, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.74f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.74f, 0f, captionTextColor);
            this.Controls.Add(control);
            Vector2? size = null;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2(0f, -0.096f), size, MyTexts.GetString(MyCommonTexts.ScreenCaptionNewItem), new VRageMath.Vector4?(VRageMath.Vector4.One), 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                Font = "White"
            };
            base.Elements.Add(label);
            size = null;
            this.itemName = new MyGuiControlLabel(new Vector2(0f, 0.03f), size, "Item Name", new VRageMath.Vector4?(VRageMath.Vector4.One), 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP);
            this.itemName.Font = "Blue";
            this.Controls.Add(this.itemName);
            size = new Vector2(0.07f, 0.09f);
            captionTextColor = null;
            string[] textures = new string[] { @"Textures\GUI\blank.dds" };
            this.itemBackground = new MyGuiControlImage(new Vector2(0f, -0.025f), size, captionTextColor, null, textures, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.itemBackground.Margin = new Thickness(0.005f);
            this.Controls.Add(this.itemBackground);
            size = new Vector2(0.06f, 0.08f);
            captionTextColor = null;
            this.itemImage = new MyGuiControlImage(new Vector2(0f, -0.025f), size, captionTextColor, null, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.itemImage.Margin = new Thickness(0.005f);
            this.Controls.Add(this.itemImage);
            size = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2(0f, 0.085f), size, MyTexts.GetString(MyCommonTexts.ScreenNewItemVisit), new VRageMath.Vector4?(VRageMath.Vector4.One), 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                Font = "White"
            };
            base.Elements.Add(label2);
            size = null;
            captionTextColor = null;
            int? buttonIndex = null;
            MyGuiControlButton button = new MyGuiControlButton(new Vector2(0f, 0.168f), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button.ButtonClicked += new Action<MyGuiControlButton>(this.OnOkButtonClick);
            this.Controls.Add(button);
            this.LoadFirstItem();
        }
    }
}

