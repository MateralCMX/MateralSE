namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiDetailScreenScriptLocal : MyGuiBlueprintScreenBase
    {
        public bool WasPublished;
        protected MyGuiControlMultilineText m_textField;
        protected float m_textScale;
        protected Vector2 m_offset;
        protected int maxNameLenght;
        protected MyGuiControlImage m_thumbnailImage;
        private MyGuiBlueprintTextDialog m_dialog;
        private MyBlueprintItemInfo m_selectedItem;
        private MyGuiIngameScriptsPage m_parent;
        protected MyGuiControlMultilineText m_descriptionField;
        private Action<MyBlueprintItemInfo> callBack;

        public MyGuiDetailScreenScriptLocal(Action<MyBlueprintItemInfo> callBack, MyBlueprintItemInfo selectedItem, MyGuiIngameScriptsPage parent, float textScale) : base(new Vector2(0.5f, 0.5f), new Vector2(0.778f, 0.594f), MyGuiConstants.SCREEN_BACKGROUND_COLOR * MySandboxGame.Config.UIBkOpacity, true)
        {
            this.m_offset = new Vector2(-0.01f, 0f);
            this.maxNameLenght = 40;
            this.WasPublished = false;
            this.callBack = callBack;
            this.m_parent = parent;
            this.m_selectedItem = selectedItem;
            this.m_textScale = textScale;
            base.m_localRoot = Path.Combine(MyFileSystem.UserDataPath, "IngameScripts", "local");
            base.CanBeHidden = true;
            base.CanHideOthers = true;
            this.RecreateControls(true);
            base.EnabledBackgroundFade = true;
        }

        protected void CallResultCallback(MyBlueprintItemInfo val)
        {
            this.UnhideScreen();
            this.callBack(val);
        }

        protected void CreateButtons()
        {
            float textScale;
            Vector2 vector = new Vector2(0.148f, -0.197f) + this.m_offset;
            Vector2 vector2 = new Vector2(0.132f, 0.045f);
            float usableWidth = 0.13f;
            if (this.m_selectedItem.Item != null)
            {
                textScale = this.m_textScale;
                MyBlueprintUtils.CreateButton(this, usableWidth * 2f, MyTexts.Get(MyCommonTexts.ScreenLoadSubscribedWorldOpenInWorkshop), new Action<MyGuiControlButton>(this.OnOpenInWorkshop), true, new MyStringId?(MyCommonTexts.Scripts_OpenWorkshopTooltip), textScale).Position = new Vector2(0.215f, -0.197f) + this.m_offset;
            }
            else
            {
                textScale = this.m_textScale;
                MyBlueprintUtils.CreateButton(this, usableWidth, MyTexts.Get(MySpaceTexts.ProgrammableBlock_ButtonRename), new Action<MyGuiControlButton>(this.OnRename), true, new MyStringId?(MyCommonTexts.Scripts_RenameTooltip), textScale).Position = vector;
                textScale = this.m_textScale;
                MyGuiControlButton button1 = MyBlueprintUtils.CreateButton(this, usableWidth, MyTexts.Get(MyCommonTexts.LoadScreenButtonPublish), new Action<MyGuiControlButton>(this.OnPublish), true, new MyStringId?(MyCommonTexts.Scripts_PublishTooltip), textScale);
                button1.Position = vector + (new Vector2(1f, 0f) * vector2);
                button1.Enabled = MyFakes.ENABLE_WORKSHOP_PUBLISH;
                textScale = this.m_textScale;
                MyBlueprintUtils.CreateButton(this, usableWidth, MyTexts.Get(MyCommonTexts.LoadScreenButtonDelete), new Action<MyGuiControlButton>(this.OnDelete), true, new MyStringId?(MyCommonTexts.Scripts_DeleteTooltip), textScale).Position = vector + (new Vector2(0f, 1f) * vector2);
                textScale = this.m_textScale;
                MyBlueprintUtils.CreateButton(this, usableWidth, MyTexts.Get(MyCommonTexts.ScreenLoadSubscribedWorldBrowseWorkshop), new Action<MyGuiControlButton>(this.OnOpenWorkshop), true, new MyStringId?(MyCommonTexts.Scripts_OpenWorkshopTooltip), textScale).Position = vector + (new Vector2(1f, 1f) * vector2);
            }
        }

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

        public override string GetFriendlyName() => 
            "MyDetailScreenScripts";

        protected void OnCloseButton(MyGuiControlButton button)
        {
            this.CloseScreen();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            if (this.m_dialog != null)
            {
                this.m_dialog.CloseScreen();
            }
            this.CallResultCallback(this.m_selectedItem);
        }

        private void OnDelete(MyGuiControlButton button)
        {
            this.m_parent.OnDelete(button);
            this.CloseScreen();
        }

        private void OnOpenInWorkshop(MyGuiControlButton button)
        {
            MyGuiSandbox.OpenUrlWithFallback(string.Format(MySteamConstants.URL_WORKSHOP_VIEW_ITEM_FORMAT, this.m_selectedItem.Item.Id), "Steam Workshop", false);
        }

        private void OnOpenWorkshop(MyGuiControlButton button)
        {
            MyGuiSandbox.OpenUrlWithFallback(MySteamConstants.URL_BROWSE_WORKSHOP_INGAMESCRIPTS, "Steam Workshop", false);
        }

        private void OnPublish(MyGuiControlButton button)
        {
            MyObjectBuilder_ModInfo info;
            string path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, this.m_selectedItem.Data.Name, "modinfo.sbmi");
            if (File.Exists(path) && MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_ModInfo>(path, out info))
            {
                this.m_selectedItem.PublishedItemId = new ulong?(info.WorkshopId);
            }
            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.LoadScreenButtonPublish);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.ProgrammableBlock_PublishScriptDialogText), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum val) {
                if (val == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    this.WasPublished = true;
                    string fullPath = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, this.m_selectedItem.Data.Name);
                    MyWorkshop.PublishIngameScriptAsync(fullPath, this.m_selectedItem.Data.Name, this.m_selectedItem.Data.Description ?? "", this.m_selectedItem.PublishedItemId, MyPublishedFileVisibility.Public, delegate (bool success, MyGameServiceCallResult result, ulong publishedFileId) {
                        MyStringId? nullable;
                        Vector2? nullable2;
                        if (success)
                        {
                            MyWorkshop.GenerateModInfo(fullPath, publishedFileId, Sync.MyId);
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable2 = null;
                            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.MessageBoxTextWorldPublished), MySession.Platform), MyTexts.Get(MySpaceTexts.ProgrammableBlock_PublishScriptPublished), nullable, nullable, nullable, nullable, a => MyGameService.OpenOverlayUrl(string.Format(MySteamConstants.URL_WORKSHOP_VIEW_ITEM_FORMAT, publishedFileId)), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                        }
                        else
                        {
                            StringBuilder messageText = (result != MyGameServiceCallResult.AccessDenied) ? new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.MessageBoxTextWorldPublishFailed), MySession.Platform) : MyTexts.Get(MyCommonTexts.MessageBoxTextPublishFailed_AccessDenied);
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable2 = null;
                            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageText, MyTexts.Get(MyCommonTexts.MessageBoxCaptionWorldPublishFailed), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                        }
                    });
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private void OnRename(MyGuiControlButton button)
        {
            this.HideScreen();
            string caption = MyTexts.GetString(MySpaceTexts.ProgrammableBlock_NewScriptName);
            this.m_dialog = new MyGuiBlueprintTextDialog(new Vector2(0.5f, 0.5f), delegate (string result) {
                if (result != null)
                {
                    this.m_parent.ChangeName(result);
                }
            }, this.m_selectedItem.Data.Name, caption, this.maxNameLenght, 0.3f);
            MyScreenManager.AddScreen(this.m_dialog);
        }

        public override void RecreateControls(bool constructor)
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
            MyGuiControlImage image1 = new MyGuiControlImage();
            image1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            this.m_thumbnailImage = image1;
            this.m_thumbnailImage.SetPadding(new MyGuiBorderThickness(2f, 2f, 2f, 2f));
            this.m_thumbnailImage.BorderEnabled = true;
            this.m_thumbnailImage.BorderSize = 1;
            this.m_thumbnailImage.BorderColor = new VRageMath.Vector4(0.235f, 0.274f, 0.314f, 1f);
            this.m_thumbnailImage.Position = new Vector2(-0.035f, -0.112f) + this.m_offset;
            this.m_thumbnailImage.Size = new Vector2(0.2f, 0.175f);
            this.Controls.Add(this.m_thumbnailImage);
            MyGuiControlImage image2 = new MyGuiControlImage();
            image2.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            MyGuiControlImage image = image2;
            image.SetPadding(new MyGuiBorderThickness(2f, 2f, 2f, 2f));
            image.SetTexture((this.m_selectedItem.Item == null) ? MyGuiConstants.TEXTURE_ICON_BLUEPRINTS_LOCAL.Normal : MyGuiConstants.TEXTURE_ICON_MODS_WORKSHOP.Normal);
            image.Position = new Vector2(-0.035f, -0.112f) + this.m_offset;
            image.Size = new Vector2(0.027f, 0.029f);
            this.Controls.Add(image);
            this.CreateTextField();
            this.CreateDescription();
            this.CreateButtons();
        }

        protected void RefreshDescriptionField()
        {
            if (this.m_descriptionField != null)
            {
                this.m_descriptionField.Clear();
                if (this.m_selectedItem.Data.Description != null)
                {
                    this.m_descriptionField.AppendText(this.m_selectedItem.Data.Description);
                }
            }
        }

        protected void RefreshTextField()
        {
            if (this.m_textField != null)
            {
                string name = this.m_selectedItem.Data.Name;
                if (name.Length > 0x19)
                {
                    name = name.Substring(0, 0x19) + "...";
                }
                this.m_textField.Clear();
                this.m_textField.AppendText("Name: " + name);
                this.m_textField.AppendLine();
                this.m_textField.AppendText("Type: IngameScript");
            }
        }
    }
}

