namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlContentButton : MyGuiControlRadioButton
    {
        private readonly MyGuiControlLabel m_titleLabel;
        private MyGuiControlImage m_previewImage;
        private string m_previewImagePath;
        private readonly MyGuiControlImage m_workshopIconNormal;
        private readonly MyGuiControlImage m_workshopIconHighlight;
        private readonly MyGuiControlImage m_localmodIconNormal;
        private readonly MyGuiControlImage m_localmodIconHighlight;
        private readonly List<MyGuiControlImage> m_dlcIcons = new List<MyGuiControlImage>();
        private bool m_isWorkshopMod;
        private bool m_isLocalMod;
        private readonly MyGuiCompositeTexture m_noThumbnailTexture = new MyGuiCompositeTexture(@"Textures\GUI\Icons\Blueprints\NoThumbnailFound.png");
        private readonly Color m_noThumbnailColor = new Color(0x5e, 0x73, 0x7f);

        public MyGuiControlContentButton(string title, string imagePath)
        {
            this.IsWorkshopMod = false;
            this.IsLocalMod = false;
            base.VisualStyle = MyGuiControlRadioButtonStyleEnum.ScenarioButton;
            base.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = title;
            this.m_titleLabel = label1;
            Vector2? position = null;
            position = null;
            Vector4? backgroundColor = null;
            string[] textures = new string[] { MyGuiConstants.TEXTURE_ICON_MODS_WORKSHOP.Normal };
            MyGuiControlImage image1 = new MyGuiControlImage(position, position, backgroundColor, null, textures, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            image1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            image1.Size = MyGuiConstants.TEXTURE_ICON_MODS_WORKSHOP.SizeGui;
            this.m_workshopIconNormal = image1;
            position = null;
            position = null;
            backgroundColor = null;
            string[] textArray2 = new string[] { MyGuiConstants.TEXTURE_ICON_MODS_WORKSHOP.Highlight };
            MyGuiControlImage image2 = new MyGuiControlImage(position, position, backgroundColor, null, textArray2, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            image2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            image2.Size = MyGuiConstants.TEXTURE_ICON_MODS_WORKSHOP.SizeGui;
            this.m_workshopIconHighlight = image2;
            position = null;
            position = null;
            backgroundColor = null;
            string[] textArray3 = new string[] { MyGuiConstants.TEXTURE_ICON_MODS_LOCAL.Normal };
            MyGuiControlImage image3 = new MyGuiControlImage(position, position, backgroundColor, null, textArray3, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            image3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            image3.Size = MyGuiConstants.TEXTURE_ICON_MODS_WORKSHOP.SizeGui;
            this.m_localmodIconNormal = image3;
            position = null;
            position = null;
            backgroundColor = null;
            string[] textArray4 = new string[] { MyGuiConstants.TEXTURE_ICON_MODS_LOCAL.Highlight };
            MyGuiControlImage image4 = new MyGuiControlImage(position, position, backgroundColor, null, textArray4, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            image4.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            image4.Size = MyGuiConstants.TEXTURE_ICON_MODS_WORKSHOP.SizeGui;
            this.m_localmodIconHighlight = image4;
            this.m_previewImagePath = imagePath;
            this.CreatePreview(imagePath);
            base.Elements.Add(this.m_titleLabel);
        }

        public void AddDlcIcon(string path)
        {
            Vector2? position = null;
            position = null;
            Vector4? backgroundColor = null;
            string[] textures = new string[] { path };
            MyGuiControlImage image1 = new MyGuiControlImage(position, position, backgroundColor, null, textures, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            image1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            image1.Size = new Vector2(32f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            MyGuiControlImage item = image1;
            this.m_dlcIcons.Add(item);
            base.Elements.Add(item);
        }

        public void ClearDlcIcons()
        {
            if ((this.m_dlcIcons != null) && (this.m_dlcIcons.Count != 0))
            {
                foreach (MyGuiControlImage image in this.m_dlcIcons)
                {
                    base.Elements.Remove(image);
                }
                this.m_dlcIcons.Clear();
            }
        }

        public void CreatePreview(string path)
        {
            if ((this.m_previewImage != null) && base.Elements.Contains(this.m_previewImage))
            {
                base.Elements.Remove(this.m_previewImage);
            }
            this.m_previewImagePath = path;
            Vector2? position = null;
            position = null;
            Vector4? backgroundColor = null;
            string[] textures = new string[] { path };
            MyGuiControlImage image1 = new MyGuiControlImage(position, position, backgroundColor, null, textures, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            image1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_previewImage = image1;
            if (!this.m_previewImage.IsAnyTextureValid())
            {
                this.m_previewImage.BackgroundTexture = this.m_noThumbnailTexture;
                this.m_previewImage.ColorMask = (Vector4) this.m_noThumbnailColor;
            }
            base.Elements.Add(this.m_previewImage);
            this.UpdatePositions();
            this.SetPreviewVisibility(true);
        }

        protected override void OnHasHighlightChanged()
        {
            base.OnHasHighlightChanged();
            if (!base.HasHighlight)
            {
                base.BorderEnabled = false;
                base.BorderColor = new Vector4(0.23f, 0.27f, 0.3f, 1f);
                base.BorderSize = 1;
                this.m_titleLabel.Font = "Blue";
                if (this.IsWorkshopMod)
                {
                    base.Elements.Remove(this.m_workshopIconHighlight);
                    base.Elements.Add(this.m_workshopIconNormal);
                }
                else if (this.IsLocalMod)
                {
                    base.Elements.Remove(this.m_localmodIconHighlight);
                    base.Elements.Add(this.m_localmodIconNormal);
                }
            }
            else
            {
                base.BorderEnabled = true;
                base.BorderColor = new Vector4(0.41f, 0.45f, 0.48f, 1f);
                base.BorderSize = 2;
                this.m_titleLabel.Font = "White";
                if (this.IsWorkshopMod)
                {
                    base.Elements.Remove(this.m_workshopIconNormal);
                    base.Elements.Add(this.m_workshopIconHighlight);
                }
                else if (this.IsLocalMod)
                {
                    base.Elements.Remove(this.m_localmodIconNormal);
                    base.Elements.Add(this.m_localmodIconHighlight);
                }
            }
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            this.UpdatePositions();
        }

        public void SetPreviewVisibility(bool visible)
        {
            this.m_previewImage.Visible = visible;
            Vector2 vector = new Vector2(242f, 128f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            this.m_titleLabel.Size = new Vector2(vector.X, this.m_titleLabel.Size.Y);
            this.m_titleLabel.AutoEllipsis = true;
            if (visible)
            {
                this.m_previewImage.Size = vector;
                this.m_previewImage.BorderEnabled = true;
                this.m_previewImage.BorderColor = new Vector4(0.23f, 0.27f, 0.3f, 1f);
                base.Size = new Vector2(this.m_previewImage.Size.X, this.m_titleLabel.Size.Y + this.m_previewImage.Size.Y);
                int num = 0;
                Vector2 vector2 = new Vector2(base.Size.X * 0.48f, (-base.Size.Y * 0.48f) + this.m_titleLabel.Size.Y);
                foreach (MyGuiControlImage image in this.m_dlcIcons)
                {
                    image.Visible = true;
                    image.Position = vector2 + new Vector2(0f, num * (image.Size.Y + 0.002f));
                    num++;
                }
            }
            else
            {
                this.m_previewImage.Size = new Vector2(0f, 0f);
                this.m_previewImage.BorderEnabled = true;
                this.m_previewImage.BorderColor = new Vector4(0.23f, 0.27f, 0.3f, 1f);
                base.Size = new Vector2(vector.X, this.m_titleLabel.Size.Y + 0.002f);
            }
            using (List<MyGuiControlImage>.Enumerator enumerator = this.m_dlcIcons.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Visible = visible;
                }
            }
        }

        private void UpdatePositions()
        {
            if (this.m_previewImage.Visible)
            {
                Vector2 vector = new Vector2(base.Size.X * -0.5f, base.Size.Y * -0.52f);
                this.m_titleLabel.Position = vector + new Vector2(0.003f, 0.002f);
                this.m_previewImage.Position = vector + new Vector2(0f, this.m_titleLabel.Size.Y * 1f);
                this.m_workshopIconNormal.Position = (base.Size * 0.5f) - new Vector2(0.001f, 0.002f);
                this.m_workshopIconHighlight.Position = (base.Size * 0.5f) - new Vector2(0.001f, 0.002f);
                this.m_localmodIconNormal.Position = (base.Size * 0.5f) - new Vector2(0.001f, 0.002f);
                this.m_localmodIconHighlight.Position = (base.Size * 0.5f) - new Vector2(0.001f, 0.002f);
                this.m_localmodIconHighlight.Size = new Vector2(this.m_localmodIconHighlight.Size.X, this.m_localmodIconHighlight.Size.Y + 0.002f);
                int num = 0;
                vector = new Vector2(base.Size.X * 0.48f, (-base.Size.Y * 0.5f) + this.m_titleLabel.Size.Y);
                foreach (MyGuiControlImage image in this.m_dlcIcons)
                {
                    image.Visible = true;
                    image.Position = vector + new Vector2(0f, num * (image.Size.Y + 0.002f));
                    num++;
                }
            }
            else
            {
                Vector2 vector2 = new Vector2(base.Size.X * -0.5f, base.Size.Y * -0.61f);
                this.m_titleLabel.Position = vector2 + new Vector2(0.003f, 0.002f);
                this.m_previewImage.Position = vector2 + new Vector2(0f, this.m_titleLabel.Size.Y * 1f);
                this.m_workshopIconNormal.Position = (base.Size * 0.5f) - new Vector2(0.001f, 0.002f);
                this.m_workshopIconHighlight.Position = (base.Size * 0.5f) - new Vector2(0.001f, 0.002f);
                this.m_localmodIconNormal.Position = (base.Size * 0.5f) - new Vector2(0.001f, 0.002f);
                this.m_localmodIconHighlight.Position = (base.Size * 0.5f) - new Vector2(0.001f, 0.002f);
                this.m_localmodIconHighlight.Size = new Vector2(this.m_localmodIconHighlight.Size.X, this.m_localmodIconHighlight.Size.Y + 0.002f);
                using (List<MyGuiControlImage>.Enumerator enumerator = this.m_dlcIcons.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Visible = false;
                    }
                }
            }
        }

        public string Title =>
            this.m_titleLabel.Text;

        public bool IsWorkshopMod
        {
            get => 
                this.m_isWorkshopMod;
            set
            {
                if (this.m_workshopIconNormal != null)
                {
                    if (!value)
                    {
                        base.Elements.Remove(this.m_workshopIconNormal);
                        base.Elements.Remove(this.m_workshopIconHighlight);
                    }
                    else
                    {
                        base.Elements.Add(base.HasHighlight ? this.m_workshopIconHighlight : this.m_workshopIconNormal);
                        if (this.IsLocalMod)
                        {
                            this.IsLocalMod = false;
                        }
                    }
                    this.m_isWorkshopMod = value;
                }
            }
        }

        public string PreviewImagePath =>
            this.m_previewImagePath;

        public bool IsLocalMod
        {
            get => 
                this.m_isLocalMod;
            set
            {
                if (this.m_localmodIconNormal != null)
                {
                    if (!value)
                    {
                        base.Elements.Remove(this.m_localmodIconNormal);
                        base.Elements.Remove(this.m_localmodIconHighlight);
                    }
                    else
                    {
                        base.Elements.Add(base.HasHighlight ? this.m_localmodIconHighlight : this.m_localmodIconNormal);
                        if (this.IsWorkshopMod)
                        {
                            this.IsWorkshopMod = false;
                        }
                    }
                    this.m_isLocalMod = value;
                }
            }
        }
    }
}

