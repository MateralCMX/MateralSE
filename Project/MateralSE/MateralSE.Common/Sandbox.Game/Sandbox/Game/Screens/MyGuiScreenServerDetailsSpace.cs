namespace Sandbox.Game.Screens
{
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Game;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenServerDetailsSpace : MyGuiScreenServerDetailsBase
    {
        public MyGuiScreenServerDetailsSpace(MyCachedServerItem server) : base(server)
        {
        }

        protected override unsafe void DrawSettings()
        {
            Vector2? size;
            VRageMath.Vector4? nullable2;
            if (base.Server.Rules != null)
            {
                string str;
                base.Server.Rules.TryGetValue("SM", out str);
                if (!string.IsNullOrEmpty(str))
                {
                    base.AddLabel(MySpaceTexts.ServerDetails_ServerManagement, str);
                }
            }
            if (!string.IsNullOrEmpty(base.Server.Description))
            {
                base.AddLabel(MyCommonTexts.Description, null);
                float* singlePtr1 = (float*) ref base.CurrentPosition.Y;
                singlePtr1[0] += 0.008f;
                base.AddMultilineText(base.Server.Description, 0.15f);
                float* singlePtr2 = (float*) ref base.CurrentPosition.Y;
                singlePtr2[0] += 0.008f;
            }
            MyGuiControlLabel label = base.AddLabel(MyCommonTexts.ServerDetails_WorldSettings, null);
            SortedList<string, object> list = base.LoadSessionSettings(VRage.Game.Game.SpaceEngineers);
            if (list == null)
            {
                size = null;
                nullable2 = null;
                this.Controls.Add(new MyGuiControlLabel(new Vector2?(base.CurrentPosition), size, MyTexts.GetString(MyCommonTexts.ServerDetails_SettingError), nullable2, 0.8f, "Red", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            }
            else
            {
                MyGuiControlParent scrolledControl = new MyGuiControlParent();
                MyGuiControlScrollablePanel control = new MyGuiControlScrollablePanel(scrolledControl) {
                    ScrollbarVEnabled = true,
                    Position = base.CurrentPosition,
                    OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
                };
                size = base.Size;
                control.Size = new Vector2(base.Size.Value.X - 0.112f, ((size.Value.Y / 2f) - base.CurrentPosition.Y) - 0.145f);
                control.BackgroundTexture = MyGuiConstants.TEXTURE_SCROLLABLE_LIST;
                control.ScrolledAreaPadding = new MyGuiBorderThickness(0.005f);
                this.Controls.Add(control);
                Vector2 vector = -control.Size / 2f;
                float y = 0f;
                foreach (KeyValuePair<string, object> pair in list)
                {
                    y += (label.Size.Y / 2f) + base.Padding;
                    SerializableDictionary<string, short> dictionary = pair.Value as SerializableDictionary<string, short>;
                    if (dictionary != null)
                    {
                        int count = dictionary.Dictionary.Count;
                        y += ((label.Size.Y / 2f) + base.Padding) * count;
                    }
                }
                vector.Y = (-y / 2f) + (label.Size.Y / 2f);
                scrolledControl.Size = new Vector2(control.Size.X, y);
                foreach (KeyValuePair<string, object> pair2 in list)
                {
                    object obj2 = pair2.Value;
                    if (!(obj2 is SerializableDictionary<string, short>))
                    {
                        string text = string.Empty;
                        if (obj2 as bool)
                        {
                            text = ((bool) obj2) ? MyTexts.GetString(MyCommonTexts.ControlMenuItemValue_On) : MyTexts.GetString(MyCommonTexts.ControlMenuItemValue_Off);
                        }
                        else if (obj2 != null)
                        {
                            text = obj2.ToString();
                        }
                        size = null;
                        nullable2 = null;
                        MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2?(vector), size, MyTexts.Get(MyStringId.GetOrCompute(pair2.Key)) + ":", nullable2, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                        size = null;
                        nullable2 = null;
                        MyGuiControlLabel label3 = new MyGuiControlLabel(new Vector2(control.Size.X / 2.5f, vector.Y), size, text, nullable2, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
                        float* singlePtr3 = (float*) ref vector.Y;
                        singlePtr3[0] += (label2.Size.Y / 2f) + base.Padding;
                        scrolledControl.Controls.Add(label2);
                        scrolledControl.Controls.Add(label3);
                        base.AddSeparator(scrolledControl, vector, 1f);
                        continue;
                    }
                    Dictionary<string, short> dictionary = (obj2 as SerializableDictionary<string, short>).Dictionary;
                    if (dictionary != null)
                    {
                        size = null;
                        nullable2 = null;
                        MyGuiControlLabel label4 = new MyGuiControlLabel(new Vector2?(vector), size, MyTexts.Get(MyStringId.GetOrCompute(pair2.Key)) + ":", nullable2, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                        scrolledControl.Controls.Add(label4);
                        float* singlePtr4 = (float*) ref vector.Y;
                        singlePtr4[0] += (label4.Size.Y / 2f) + base.Padding;
                        base.AddSeparator(scrolledControl, vector, 1f);
                        foreach (KeyValuePair<string, short> pair3 in dictionary)
                        {
                            size = null;
                            nullable2 = null;
                            MyGuiControlLabel label5 = new MyGuiControlLabel(new Vector2?(vector), size, "     " + pair3.Key, nullable2, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                            size = null;
                            short num3 = pair3.Value;
                            nullable2 = null;
                            MyGuiControlLabel label6 = new MyGuiControlLabel(new Vector2(control.Size.X / 2.5f, vector.Y), size, num3.ToString(), nullable2, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
                            scrolledControl.Controls.Add(label5);
                            scrolledControl.Controls.Add(label6);
                            float* singlePtr5 = (float*) ref vector.Y;
                            singlePtr5[0] += (label5.Size.Y / 2f) + base.Padding;
                            base.AddSeparator(scrolledControl, vector, 1f);
                        }
                    }
                }
            }
        }
    }
}

