namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Utils;

    public abstract class MyToolbarItem
    {
        public MyToolbarItem()
        {
            this.Icons = new string[] { MyGuiConstants.TEXTURE_ICON_FAKE.Texture };
            this.IconText = new StringBuilder();
            this.DisplayName = new StringBuilder();
        }

        public abstract bool Activate();
        public abstract bool AllowedInToolbarType(MyToolbarType type);
        public ChangeInfo ClearIconText()
        {
            if (this.IconText.Length == 0)
            {
                return ChangeInfo.None;
            }
            this.IconText.Clear();
            return ChangeInfo.IconText;
        }

        public override bool Equals(object obj)
        {
            throw new InvalidOperationException("GetHashCode and Equals must be overridden");
        }

        public virtual void FillGridItem(MyGuiGridItem gridItem)
        {
            if (this.IconText.Length == 0)
            {
                gridItem.ClearText(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
            }
            else
            {
                gridItem.AddText(this.IconText, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
            }
        }

        public override int GetHashCode()
        {
            throw new InvalidOperationException("GetHashCode and Equals must be overridden");
        }

        public abstract MyObjectBuilder_ToolbarItem GetObjectBuilder();
        public abstract bool Init(MyObjectBuilder_ToolbarItem data);
        public virtual void OnAddedToToolbar(MyToolbar toolbar)
        {
        }

        public virtual void OnRemovedFromToolbar(MyToolbar toolbar)
        {
        }

        public ChangeInfo SetDisplayName(string newDisplayName)
        {
            if (newDisplayName == null)
            {
                return ChangeInfo.None;
            }
            if (this.DisplayName.CompareTo(newDisplayName) == 0)
            {
                return ChangeInfo.None;
            }
            this.DisplayName.Clear();
            this.DisplayName.Append(newDisplayName);
            return ChangeInfo.DisplayName;
        }

        public ChangeInfo SetEnabled(bool newEnabled)
        {
            if (newEnabled == this.Enabled)
            {
                return ChangeInfo.None;
            }
            this.Enabled = newEnabled;
            return ChangeInfo.Enabled;
        }

        public ChangeInfo SetIcons(string[] newIcons)
        {
            if (newIcons == this.Icons)
            {
                return ChangeInfo.None;
            }
            this.Icons = newIcons;
            return ChangeInfo.Icon;
        }

        public ChangeInfo SetIconText(StringBuilder newIconText)
        {
            if (newIconText == null)
            {
                return ChangeInfo.None;
            }
            if (this.IconText.CompareTo(newIconText) == 0)
            {
                return ChangeInfo.None;
            }
            this.IconText.Clear();
            this.IconText.AppendStringBuilder(newIconText);
            return ChangeInfo.IconText;
        }

        public ChangeInfo SetSubIcon(string newSubIcon)
        {
            if (newSubIcon == this.SubIcon)
            {
                return ChangeInfo.None;
            }
            this.SubIcon = newSubIcon;
            return ChangeInfo.SubIcon;
        }

        public abstract ChangeInfo Update(MyEntity owner, long playerID = 0L);

        public bool Enabled { get; private set; }

        public string[] Icons { get; private set; }

        public string SubIcon { get; private set; }

        public StringBuilder IconText { get; private set; }

        public StringBuilder DisplayName { get; private set; }

        public bool WantsToBeActivated { get; protected set; }

        public bool WantsToBeSelected { get; protected set; }

        public bool ActivateOnClick { get; protected set; }

        [Flags]
        public enum ChangeInfo
        {
            None = 0,
            Enabled = 1,
            Icon = 2,
            SubIcon = 4,
            IconText = 8,
            DisplayName = 0x10,
            All = 0x1f
        }
    }
}

