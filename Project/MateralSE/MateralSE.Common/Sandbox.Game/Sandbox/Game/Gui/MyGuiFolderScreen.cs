namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiFolderScreen : MyGuiScreenBase
    {
        private static readonly Vector2 SCREEN_SIZE = new Vector2(0.4f, 0.6f);
        private Action<bool, string> m_onFinishedAction;
        private Func<string, bool> m_isItem;
        private string m_rootPath;
        private string m_pathLocalInitial;
        private string m_pathLocalCurrent;
        private MyGuiControlLabel m_pathLabel;
        private MyGuiControlListbox m_fileList;
        private MyGuiControlButton m_buttonOk;
        private MyGuiControlButton m_buttonClose;
        private MyGuiControlButton m_buttonRefresh;

        public MyGuiFolderScreen(bool hideOthers, Action<bool, string> OnFinished, string rootPath, string localPath, Func<string, bool> isItem = null) : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR * MySandboxGame.Config.UIBkOpacity), nullable, false, null, 0f, 0f)
        {
            this.m_rootPath = string.Empty;
            this.m_pathLocalInitial = string.Empty;
            this.m_pathLocalCurrent = string.Empty;
            Vector2? nullable = new Vector2?(SCREEN_SIZE);
            if (OnFinished == null)
            {
                this.CloseScreen();
            }
            base.CanHideOthers = hideOthers;
            this.m_onFinishedAction = OnFinished;
            this.m_rootPath = rootPath;
            this.m_pathLocalCurrent = this.m_pathLocalInitial = localPath;
            this.m_isItem = (isItem == null) ? new Func<string, bool>(MyGuiFolderScreen.IsItem_Default) : isItem;
            this.RecreateControls(true);
        }

        public string BuildNewPath()
        {
            string str = "";
            if (this.m_fileList.SelectedItems.Count != 1)
            {
                return this.m_pathLocalCurrent;
            }
            MyFileItem userData = (MyFileItem) this.m_fileList.SelectedItems[0].UserData;
            if (userData.Type != MyFileItemType.Directory)
            {
                return this.m_pathLocalCurrent;
            }
            if (!string.IsNullOrEmpty(userData.Path))
            {
                str = Path.Combine(this.m_pathLocalCurrent, userData.Name);
            }
            else
            {
                char[] separator = new char[] { Path.DirectorySeparatorChar };
                string[] paths = this.m_pathLocalCurrent.Split(separator);
                if (paths.Length <= 1)
                {
                    str = string.Empty;
                }
                else
                {
                    paths[paths.Length - 1] = string.Empty;
                    str = Path.Combine(paths);
                }
            }
            return str;
        }

        protected MyGuiControlButton CreateButton(Vector2 size, StringBuilder text, Action<MyGuiControlButton> onClick, bool enabled = true, MyStringId? tooltip = new MyStringId?(), float textScale = 1f)
        {
            Vector2? position = null;
            position = null;
            Vector4? colorMask = null;
            int? buttonIndex = null;
            MyGuiControlButton control = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Rectangular, position, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, text, 0.8f * MyGuiConstants.DEBUG_BUTTON_TEXT_SCALE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, buttonIndex, false) {
                TextScale = textScale,
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP,
                Size = size
            };
            if (tooltip != null)
            {
                control.SetToolTip(tooltip.Value);
            }
            this.Controls.Add(control);
            return control;
        }

        private MyGuiControlImage CreateButtonIcon(MyGuiControlButton butt, string texture)
        {
            float y = 0.95f * Math.Min(butt.Size.X, butt.Size.Y);
            Vector2? size = new Vector2(y * 0.75f, y);
            Vector4? backgroundColor = null;
            string[] textures = new string[] { texture };
            MyGuiControlImage control = new MyGuiControlImage(new Vector2?(butt.Position + new Vector2(-0.0016f, 0.015f)), size, backgroundColor, null, textures, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.Controls.Add(control);
            return control;
        }

        public override string GetFriendlyName() => 
            "MyGuiFolderScreen";

        private static bool IsItem_Default(string path) => 
            false;

        private void OnClose(MyGuiControlButton button)
        {
            this.m_onFinishedAction(false, this.m_pathLocalInitial);
            this.CloseScreen();
        }

        private void OnItemClick(MyGuiControlListbox list)
        {
            MyGuiControlListbox.Item local1 = list.SelectedItems[0];
            this.UpdatePathLabel();
        }

        private void OnItemDoubleClick(MyGuiControlListbox list)
        {
            MyGuiControlListbox.Item local1 = list.SelectedItems[0];
            this.m_pathLocalCurrent = this.BuildNewPath();
            this.RepopulateList();
        }

        private void OnOk(MyGuiControlButton button)
        {
            this.m_onFinishedAction(true, this.BuildNewPath());
            this.CloseScreen();
        }

        private void OnRefresh(MyGuiControlButton button)
        {
            this.RecreateControls(false);
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            Vector2 vector = new Vector2(0f, 0.23f);
            Vector2 vector2 = new Vector2(0.17f, -0.275f);
            Vector2 vector3 = new Vector2(0.15f, 0.23f);
            Vector2 vector4 = new Vector2(0f, 0.02f);
            Vector2 vector5 = new Vector2(-0.143f, -0.2f);
            Vector2 size = new Vector2(0.143f, 0.035f);
            Vector2 vector7 = new Vector2(0.026f, 0.035f);
            Vector2 vector8 = new Vector2(0.32f, 0.38f);
            Vector2 vector9 = new Vector2(0.5f, 0.5f);
            Vector2? position = null;
            this.m_fileList = new MyGuiControlListbox(position, MyGuiControlListboxStyleEnum.Blueprints);
            this.m_fileList.Position = vector4;
            this.m_fileList.Size = vector8;
            this.m_fileList.ItemDoubleClicked += new Action<MyGuiControlListbox>(this.OnItemDoubleClick);
            this.m_fileList.ItemClicked += new Action<MyGuiControlListbox>(this.OnItemClick);
            this.m_fileList.VisibleRowsCount = 11;
            this.Controls.Add(this.m_fileList);
            Vector4? captionTextColor = null;
            position = null;
            base.AddCaption(MySpaceTexts.ScreenFolders_Caption, captionTextColor, position, 0.8f);
            this.m_buttonOk = this.CreateButton(size, MyTexts.Get(MySpaceTexts.ScreenFolders_ButOpen), new Action<MyGuiControlButton>(this.OnOk), true, new MyStringId?(MySpaceTexts.ScreenFolders_Tooltip_Open), 1f);
            this.m_buttonOk.Position = vector;
            this.m_buttonOk.ShowTooltipWhenDisabled = true;
            this.m_buttonRefresh = this.CreateButton(vector7, null, new Action<MyGuiControlButton>(this.OnRefresh), true, new MyStringId?(MySpaceTexts.ScreenFolders_Tooltip_Refresh), 1f);
            this.m_buttonRefresh.Position = vector3;
            this.m_buttonRefresh.ShowTooltipWhenDisabled = true;
            captionTextColor = null;
            this.m_pathLabel = new MyGuiControlLabel(new Vector2?(vector5), new Vector2?(vector9), null, captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.Controls.Add(this.m_pathLabel);
            this.UpdatePathLabel();
            this.CreateButtonIcon(this.m_buttonRefresh, @"Textures\GUI\Icons\Blueprints\Refresh.png");
            MyGuiControlButton button1 = new MyGuiControlButton();
            button1.Position = vector2;
            button1.Size = new Vector2(0.045f, 0.05666667f);
            button1.Name = "Close";
            button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            button1.VisualStyle = MyGuiControlButtonStyleEnum.Close;
            button1.ActivateOnMouseRelease = true;
            this.m_buttonClose = button1;
            this.m_buttonClose.ButtonClicked += new Action<MyGuiControlButton>(this.OnClose);
            this.Controls.Add(this.m_buttonClose);
            this.RepopulateList();
        }

        private void RepopulateList()
        {
            this.m_fileList.Items.Clear();
            List<MyGuiControlListbox.Item> list = new List<MyGuiControlListbox.Item>();
            List<MyGuiControlListbox.Item> list2 = new List<MyGuiControlListbox.Item>();
            string path = Path.Combine(this.m_rootPath, this.m_pathLocalCurrent);
            if (Directory.Exists(path))
            {
                int? nullable;
                string[] directories = Directory.GetDirectories(path);
                List<string> list3 = new List<string>();
                string[] strArray2 = directories;
                for (int i = 0; i < strArray2.Length; i++)
                {
                    char[] separator = new char[] { '\\' };
                    string[] strArray3 = strArray2[i].Split(separator);
                    list3.Add(strArray3[strArray3.Length - 1]);
                }
                for (int j = 0; j < list3.Count; j++)
                {
                    if (this.m_isItem(directories[j]))
                    {
                        MyFileItem userData = new MyFileItem();
                        userData.Type = MyFileItemType.File;
                        userData.Name = list3[j];
                        userData.Path = directories[j];
                        list2.Add(new MyGuiControlListbox.Item(new StringBuilder(list3[j]), directories[j], MyGuiConstants.TEXTURE_ICON_BLUEPRINTS_LOCAL.Normal, userData, null));
                    }
                    else
                    {
                        MyFileItem userData = new MyFileItem();
                        userData.Type = MyFileItemType.Directory;
                        userData.Name = list3[j];
                        userData.Path = directories[j];
                        list.Add(new MyGuiControlListbox.Item(new StringBuilder(list3[j]), directories[j], MyGuiConstants.TEXTURE_ICON_MODS_LOCAL.Normal, userData, null));
                    }
                }
                if (!string.IsNullOrEmpty(this.m_pathLocalCurrent))
                {
                    MyFileItem item9 = new MyFileItem();
                    item9.Type = MyFileItemType.Directory;
                    item9.Name = string.Empty;
                    item9.Path = string.Empty;
                    MyFileItem item4 = item9;
                    object userData = item4;
                    MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(new StringBuilder("[..]"), this.m_pathLocalCurrent, MyGuiConstants.TEXTURE_ICON_MODS_LOCAL.Normal, userData, null);
                    nullable = null;
                    this.m_fileList.Add(item, nullable);
                }
                foreach (MyGuiControlListbox.Item item6 in list)
                {
                    nullable = null;
                    this.m_fileList.Add(item6, nullable);
                }
                foreach (MyGuiControlListbox.Item item7 in list2)
                {
                    nullable = null;
                    this.m_fileList.Add(item7, nullable);
                }
                this.UpdatePathLabel();
            }
        }

        public void UpdatePathLabel()
        {
            string str = "./" + this.BuildNewPath();
            if (str.Length > 40)
            {
                this.m_pathLabel.Text = str.Substring(str.Length - 0x29, 40);
            }
            else
            {
                this.m_pathLabel.Text = str;
            }
        }
    }
}

