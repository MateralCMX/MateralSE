namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiGridItem
    {
        public readonly Dictionary<MyGuiDrawAlignEnum, StringBuilder> TextsByAlign;
        public readonly Dictionary<MyGuiDrawAlignEnum, ColoredIcon> IconsByAlign;
        public string[] Icons;
        public string SubIcon;
        public string SubIcon2;
        public Vector2? SubIconOffset;
        public MyToolTips ToolTip;
        public object UserData;
        public bool Enabled;
        public float OverlayPercent;
        public Vector4 IconColorMask;
        public Vector4 OverlayColorMask;
        public long blinkCount;
        public const int MILISSECONDS_TO_BLINK = 400;

        public MyGuiGridItem(string icon = null, string subicon = null, MyToolTips toolTips = null, object userData = null, bool enabled = true)
        {
            this.TextsByAlign = new Dictionary<MyGuiDrawAlignEnum, StringBuilder>();
            this.IconsByAlign = new Dictionary<MyGuiDrawAlignEnum, ColoredIcon>();
            this.Icons = new string[] { icon };
            this.SubIcon = subicon;
            this.ToolTip = toolTips;
            this.UserData = userData;
            this.Enabled = enabled;
            this.IconColorMask = Vector4.One;
            this.OverlayColorMask = Vector4.One;
            this.blinkCount = 0L;
        }

        public MyGuiGridItem(string[] icons = null, string subicon = null, MyToolTips toolTips = null, object userData = null, bool enabled = true)
        {
            this.TextsByAlign = new Dictionary<MyGuiDrawAlignEnum, StringBuilder>();
            this.IconsByAlign = new Dictionary<MyGuiDrawAlignEnum, ColoredIcon>();
            this.Icons = icons;
            this.SubIcon = subicon;
            this.ToolTip = toolTips;
            this.UserData = userData;
            this.Enabled = enabled;
            this.IconColorMask = Vector4.One;
            this.OverlayColorMask = Vector4.One;
            this.blinkCount = 0L;
        }

        public MyGuiGridItem(string icon = null, string subicon = null, string toolTip = null, object userData = null, bool enabled = true) : this(textArray1, subicon, (toolTip != null) ? new MyToolTips(toolTip) : null, userData, enabled)
        {
            string[] textArray1 = new string[] { icon };
        }

        public MyGuiGridItem(string[] icons = null, string subicon = null, string toolTip = null, object userData = null, bool enabled = true) : this(icons, subicon, (toolTip != null) ? new MyToolTips(toolTip) : null, userData, enabled)
        {
        }

        public void AddIcon(ColoredIcon icon, MyGuiDrawAlignEnum iconAlign = 6)
        {
            if (!this.IconsByAlign.ContainsKey(iconAlign))
            {
                this.IconsByAlign.Add(iconAlign, icon);
            }
            else
            {
                this.IconsByAlign[iconAlign] = icon;
            }
        }

        public void AddText(string text, MyGuiDrawAlignEnum textAlign = 0)
        {
            if (!this.TextsByAlign.ContainsKey(textAlign))
            {
                this.TextsByAlign[textAlign] = new StringBuilder();
            }
            if (this.TextsByAlign[textAlign].CompareTo(text) != 0)
            {
                this.TextsByAlign[textAlign].Clear().Append(text);
            }
        }

        public void AddText(StringBuilder text, MyGuiDrawAlignEnum textAlign = 0)
        {
            if (!this.TextsByAlign.ContainsKey(textAlign))
            {
                this.TextsByAlign[textAlign] = new StringBuilder();
            }
            if (this.TextsByAlign[textAlign].CompareTo(text) != 0)
            {
                this.TextsByAlign[textAlign].Clear().AppendStringBuilder(text);
            }
        }

        public float blinkingTransparency()
        {
            if ((MyGuiManager.TotalTimeInMilliseconds - this.blinkCount) > 400L)
            {
                return 1f;
            }
            return ((3f + ((float) Math.Cos((((MyGuiManager.TotalTimeInMilliseconds - this.blinkCount) * 4L) * 3.1415926535897931) / 400.0))) / 4f);
        }

        public void ClearAllText()
        {
            this.TextsByAlign.Clear();
        }

        public void ClearText(MyGuiDrawAlignEnum textAlign = 0)
        {
            this.TextsByAlign.Remove(textAlign);
        }

        public void startBlinking()
        {
            this.blinkCount = MyGuiManager.TotalTimeInMilliseconds;
        }

        public MyDefinitionBase ItemDefinition { get; set; }

        public Vector2 Position { get; set; }
    }
}

