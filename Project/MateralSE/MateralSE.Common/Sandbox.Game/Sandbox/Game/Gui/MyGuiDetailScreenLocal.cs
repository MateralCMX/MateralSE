namespace Sandbox.Game.Gui
{
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.IO;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyGuiDetailScreenLocal : MyGuiDetailScreenBase
    {
        private string m_currentLocalDirectory;

        public MyGuiDetailScreenLocal(Action<MyGuiControlListbox.Item> callBack, MyGuiControlListbox.Item selectedItem, MyGuiBlueprintScreenBase parent, string thumbnailTexture, float textScale, string currentLocalDirectory) : base(false, parent, thumbnailTexture, selectedItem, textScale)
        {
            this.m_currentLocalDirectory = currentLocalDirectory;
            string path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, currentLocalDirectory, base.m_blueprintName, "bp.sbc");
            base.callBack = callBack;
            if (!File.Exists(path))
            {
                base.m_killScreen = true;
            }
            else
            {
                base.m_loadedPrefab = MyBlueprintUtils.LoadPrefab(path);
                if (base.m_loadedPrefab != null)
                {
                    this.RecreateControls(true);
                }
                else
                {
                    StringBuilder messageCaption = new StringBuilder("Error");
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("Failed to load the blueprint file."), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                    base.m_killScreen = true;
                }
            }
        }

        private void ChangeDescription(string newDescription)
        {
            if (Directory.Exists(Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, this.m_currentLocalDirectory, base.m_blueprintName)))
            {
                base.m_loadedPrefab.ShipBlueprints[0].Description = newDescription;
                MyBlueprintUtils.SavePrefabToFile(base.m_loadedPrefab, base.m_blueprintName, this.m_currentLocalDirectory, true, MyBlueprintTypeEnum.LOCAL);
                base.RefreshDescriptionField();
            }
        }

        private void ChangeName(string name)
        {
            name = MyUtils.StripInvalidChars(name);
            string blueprintName = base.m_blueprintName;
            string file = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, this.m_currentLocalDirectory, blueprintName);
            string newFile = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, this.m_currentLocalDirectory, name);
            if ((file != newFile) && Directory.Exists(file))
            {
                StringBuilder builder;
                MyStringId? nullable;
                Vector2? nullable2;
                if (Directory.Exists(newFile))
                {
                    if (file.ToLower() != newFile.ToLower())
                    {
                        builder = new StringBuilder("Replace");
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, new StringBuilder("Blueprint with the name \"" + name + "\" already exists. Do you want to replace it?"), builder, nullable, nullable, nullable, nullable, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                            if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                            {
                                string str = Path.Combine(this.m_localRoot, this.m_currentLocalDirectory, name);
                                this.DeleteItem(str);
                                this.m_loadedPrefab.ShipBlueprints[0].Id.SubtypeId = name;
                                this.m_loadedPrefab.ShipBlueprints[0].Id.SubtypeName = name;
                                this.m_loadedPrefab.ShipBlueprints[0].CubeGrids[0].DisplayName = name;
                                Directory.Move(file, newFile);
                                MyRenderProxy.UnloadTexture(Path.Combine(newFile, "thumb.png"));
                                this.m_thumbnailImage.SetTexture(Path.Combine(newFile, "thumb.png"));
                                MyBlueprintUtils.SavePrefabToFile(this.m_loadedPrefab, name, this.m_currentLocalDirectory, true, MyBlueprintTypeEnum.LOCAL);
                                this.m_blueprintName = name;
                                this.RefreshTextField();
                                this.m_parent.RefreshBlueprintList(false);
                            }
                        }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    }
                    else
                    {
                        base.m_loadedPrefab.ShipBlueprints[0].Id.SubtypeId = name;
                        base.m_loadedPrefab.ShipBlueprints[0].Id.SubtypeName = name;
                        base.m_loadedPrefab.ShipBlueprints[0].CubeGrids[0].DisplayName = name;
                        string path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_LOCAL, "temp");
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                        Directory.Move(file, path);
                        Directory.Move(path, newFile);
                        base.m_thumbnailImage.SetTexture(Path.Combine(newFile, "thumb.png"));
                        MyBlueprintUtils.SavePrefabToFile(base.m_loadedPrefab, name, this.m_currentLocalDirectory, true, MyBlueprintTypeEnum.LOCAL);
                        base.m_blueprintName = name;
                        base.RefreshTextField();
                        base.m_parent.RefreshBlueprintList(false);
                    }
                }
                else
                {
                    base.m_loadedPrefab.ShipBlueprints[0].Id.SubtypeId = name;
                    base.m_loadedPrefab.ShipBlueprints[0].Id.SubtypeName = name;
                    base.m_loadedPrefab.ShipBlueprints[0].CubeGrids[0].DisplayName = name;
                    try
                    {
                        Directory.Move(file, newFile);
                    }
                    catch (IOException)
                    {
                        builder = new StringBuilder("Delete");
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("Cannot rename blueprint because it is used by another process."), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                        return;
                    }
                    MyRenderProxy.UnloadTexture(Path.Combine(newFile, "thumb.png"));
                    base.m_thumbnailImage.SetTexture(Path.Combine(newFile, "thumb.png"));
                    MyBlueprintUtils.SavePrefabToFile(base.m_loadedPrefab, name, this.m_currentLocalDirectory, true, MyBlueprintTypeEnum.LOCAL);
                    base.m_blueprintName = name;
                    base.RefreshTextField();
                    base.m_parent.RefreshBlueprintList(false);
                }
            }
        }

        protected override void CreateButtons()
        {
            Vector2 vector = new Vector2(0.148f, -0.197f) + base.m_offset;
            Vector2 vector2 = new Vector2(0.132f, 0.045f);
            float usableWidth = 0.13f;
            float textScale = base.m_textScale;
            MyBlueprintUtils.CreateButton(this, usableWidth, MyTexts.Get(MySpaceTexts.DetailScreen_Button_Rename), new Action<MyGuiControlButton>(this.OnRename), true, new MyStringId?(MyCommonTexts.Blueprints_RenameTooltip), textScale).Position = vector;
            textScale = base.m_textScale;
            MyBlueprintUtils.CreateButton(this, usableWidth, MyTexts.Get(MySpaceTexts.DetailScreen_Button_Publish), new Action<MyGuiControlButton>(this.OnPublish), true, new MyStringId?(MyCommonTexts.ToolTipBlueprintPublish), textScale).Position = vector + (new Vector2(1f, 0f) * vector2);
            textScale = base.m_textScale;
            MyBlueprintUtils.CreateButton(this, usableWidth, MyTexts.Get(MySpaceTexts.DetailScreen_Button_Delete), new Action<MyGuiControlButton>(this.OnDelete), true, new MyStringId?(MyCommonTexts.Blueprints_DeleteTooltip), textScale).Position = vector + (new Vector2(0f, 1f) * vector2);
            textScale = base.m_textScale;
            MyBlueprintUtils.CreateButton(this, usableWidth, MyTexts.Get(MySpaceTexts.DetailScreen_Button_OpenWorkshop), new Action<MyGuiControlButton>(this.OnOpenWorkshop), true, new MyStringId?(MyCommonTexts.ScreenLoadSubscribedWorldBrowseWorkshop), textScale).Position = vector + (new Vector2(1f, 1f) * vector2);
        }

        public override string GetFriendlyName() => 
            "MyDetailScreen";

        protected override void OnClosed()
        {
            base.OnClosed();
            base.CallResultCallback(base.m_selectedItem);
            if (base.m_dialog != null)
            {
                base.m_dialog.CloseScreen();
            }
        }

        private void OnDelete(MyGuiControlButton button)
        {
            StringBuilder messageCaption = new StringBuilder("Delete");
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, new StringBuilder("Are you sure you want to delete this blueprint?"), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    base.DeleteItem(Path.Combine(base.m_localRoot, this.m_currentLocalDirectory, base.m_blueprintName));
                    base.CallResultCallback(null);
                    this.CloseScreen();
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private void OnDeleteDescription(MyGuiControlButton button)
        {
            StringBuilder messageCaption = new StringBuilder("Delete description");
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, new StringBuilder("Are you sure you want to delete the description of this blueprint?"), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    this.ChangeDescription("");
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private void OnEditDescription(MyGuiControlButton button)
        {
            base.m_dialog = new MyGuiBlueprintTextDialog(base.m_position, delegate (string result) {
                if (result != null)
                {
                    this.ChangeDescription(result);
                }
            }, base.m_loadedPrefab.ShipBlueprints[0].Description, "Enter new description", 0x1f40, 0.2f);
            MyScreenManager.AddScreen(base.m_dialog);
        }

        private void OnOpenWorkshop(MyGuiControlButton button)
        {
            MyGuiSandbox.OpenUrlWithFallback(MySteamConstants.URL_BROWSE_WORKSHOP_BLUEPRINTS, "Steam Workshop", false);
        }

        private void OnPublish(MyGuiControlButton button)
        {
            MyBlueprintUtils.PublishBlueprint(base.m_loadedPrefab, base.m_blueprintName, this.m_currentLocalDirectory, null);
        }

        private void OnRename(MyGuiControlButton button)
        {
            string caption = MyTexts.GetString(MySpaceTexts.DetailScreen_Button_Rename);
            base.m_dialog = new MyGuiBlueprintTextDialog(base.m_position, delegate (string result) {
                if (result != null)
                {
                    this.ChangeName(result);
                }
            }, base.m_blueprintName, caption, base.maxNameLenght, 0.3f);
            MyScreenManager.AddScreen(base.m_dialog);
        }
    }
}

