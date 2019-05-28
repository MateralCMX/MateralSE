namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenWorkshopTags : MyGuiScreenBase
    {
        private MyGuiControlButton m_okButton;
        private MyGuiControlButton m_cancelButton;
        private static List<MyGuiControlCheckbox> m_checkboxes = new List<MyGuiControlCheckbox>();
        private Action<MyGuiScreenMessageBox.ResultEnum, string[]> m_callback;
        private string m_typeTag;
        private static MyGuiScreenWorkshopTags Static;
        private const int TAGS_MAX_LENGTH = 0x80;
        private static Dictionary<string, MyStringId> m_activeTags;

        public MyGuiScreenWorkshopTags(string typeTag, MyWorkshop.Category[] categories, string[] tags, Action<MyGuiScreenMessageBox.ResultEnum, string[]> callback) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.6535714f, 0.596374f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            Static = this;
            this.m_typeTag = typeTag ?? "";
            m_activeTags = new Dictionary<string, MyStringId>(categories.Length);
            foreach (MyWorkshop.Category category in categories)
            {
                m_activeTags.Add(category.Id, category.LocalizableName);
            }
            this.m_callback = callback;
            base.EnabledBackgroundFade = true;
            base.CanHideOthers = true;
            this.RecreateControls(true);
            this.SetSelectedTags(tags);
        }

        private MyGuiControlImage AddIcon(Vector2 position, string texture, Vector2 size)
        {
            MyGuiControlImage image1 = new MyGuiControlImage();
            image1.Position = position;
            image1.Size = size;
            MyGuiControlImage control = image1;
            control.SetTexture(texture);
            this.Controls.Add(control);
            return control;
        }

        private MyGuiControlCheckbox AddLabeledCheckbox(Vector2 position, string tag, MyStringId text)
        {
            MyGuiControlLabel control = this.MakeLabel(position, text);
            MyGuiControlCheckbox checkbox = this.MakeCheckbox(position, text);
            this.Controls.Add(control);
            this.Controls.Add(checkbox);
            checkbox.UserData = tag;
            m_checkboxes.Add(checkbox);
            return checkbox;
        }

        protected override void Canceling()
        {
            base.Canceling();
            this.m_callback(MyGuiScreenMessageBox.ResultEnum.CANCEL, this.GetSelectedTags());
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenWorkshopTags";

        public string[] GetSelectedTags()
        {
            List<string> list = new List<string>();
            if (!string.IsNullOrEmpty(this.m_typeTag))
            {
                list.Add(this.m_typeTag);
            }
            foreach (MyGuiControlCheckbox checkbox in m_checkboxes)
            {
                if (checkbox.IsChecked)
                {
                    list.Add((string) checkbox.UserData);
                }
            }
            return list.ToArray();
        }

        public int GetSelectedTagsLength()
        {
            int length = this.m_typeTag.Length;
            foreach (MyGuiControlCheckbox checkbox in m_checkboxes)
            {
                if (checkbox.IsChecked)
                {
                    length += ((string) checkbox.UserData).Length;
                }
            }
            return length;
        }

        private MyGuiControlButton MakeButton(Vector2 position, MyStringId text, MyStringId toolTip, Action<MyGuiControlButton> onClick, MyGuiDrawAlignEnum originAlign = 0)
        {
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            string str = MyTexts.GetString(toolTip);
            return new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.Default, size, colorMask, originAlign, str, MyTexts.Get(text), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, null, false);
        }

        private MyGuiControlCheckbox MakeCheckbox(Vector2 position, MyStringId tooltip)
        {
            VRageMath.Vector4? color = null;
            MyGuiControlCheckbox checkbox1 = new MyGuiControlCheckbox(new Vector2?(position), color, MyTexts.GetString(tooltip), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
            checkbox1.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(checkbox1.IsCheckedChanged, new Action<MyGuiControlCheckbox>(MyGuiScreenWorkshopTags.OnCheckboxChanged));
            return checkbox1;
        }

        private MyGuiControlLabel MakeLabel(Vector2 position, MyStringId text)
        {
            Vector2? size = null;
            return new MyGuiControlLabel(new Vector2?(position), size, MyTexts.GetString(text), null, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
        }

        private void OnCancelClick(MyGuiControlButton obj)
        {
            this.CloseScreen();
            this.m_callback(MyGuiScreenMessageBox.ResultEnum.CANCEL, this.GetSelectedTags());
        }

        private static void OnCheckboxChanged(MyGuiControlCheckbox obj)
        {
            if ((obj != null) && (obj.IsChecked && (Static.GetSelectedTagsLength() >= 0x80)))
            {
                obj.IsChecked = false;
            }
        }

        private void OnOkClick(MyGuiControlButton obj)
        {
            this.CloseScreen();
            this.m_callback(MyGuiScreenMessageBox.ResultEnum.YES, this.GetSelectedTags());
        }

        public override void RecreateControls(bool constructor)
        {
            MyGuiControlButton button;
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.ScreenCaptionWorkshopTags, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.83f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.83f, 0f, captionTextColor);
            this.Controls.Add(list2);
            Vector2 position = new Vector2(-0.125f, -0.2f);
            Vector2 vector2 = new Vector2(0f, 0.05f);
            m_checkboxes.Clear();
            int num = 0;
            foreach (KeyValuePair<string, MyStringId> pair in m_activeTags)
            {
                num++;
                if (num == 8)
                {
                    position = new Vector2(0.125f, -0.2f);
                }
                this.AddLabeledCheckbox(position += vector2, pair.Key, pair.Value);
                if (this.m_typeTag == "mod")
                {
                    string str = pair.Key.Replace(" ", string.Empty);
                    string[] paths = new string[] { MyFileSystem.ContentPath, "Textures", "GUI", "Icons", "buttons", str + ".dds" };
                    string path = Path.Combine(paths);
                    if (File.Exists(path))
                    {
                        this.AddIcon(position + new Vector2(-0.05f, 0f), path, new Vector2(0.04f, 0.05f));
                    }
                }
            }
            position += vector2;
            this.m_okButton = button = this.MakeButton(position += vector2, MyCommonTexts.Ok, MySpaceTexts.ToolTipNewsletter_Ok, new Action<MyGuiControlButton>(this.OnOkClick), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
            this.Controls.Add(button);
            this.m_cancelButton = button = this.MakeButton(position, MyCommonTexts.Cancel, MySpaceTexts.ToolTipOptionsSpace_Cancel, new Action<MyGuiControlButton>(this.OnCancelClick), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.Controls.Add(button);
            Vector2 vector3 = new Vector2(54f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            Vector2 vector4 = ((base.m_size.Value / 2f) - vector3) * new Vector2(0f, 1f);
            float x = 25f;
            this.m_okButton.Position = vector4 + (new Vector2(-x, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            this.m_okButton.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            this.m_cancelButton.Position = vector4 + (new Vector2(x, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            this.m_cancelButton.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            base.CloseButtonEnabled = true;
        }

        public void SetSelectedTags(string[] tags)
        {
            if (tags != null)
            {
                foreach (string str in tags)
                {
                    foreach (MyGuiControlCheckbox checkbox in m_checkboxes)
                    {
                        if (str.Equals((string) checkbox.UserData, StringComparison.InvariantCultureIgnoreCase))
                        {
                            checkbox.IsChecked = true;
                        }
                    }
                }
            }
        }
    }
}

