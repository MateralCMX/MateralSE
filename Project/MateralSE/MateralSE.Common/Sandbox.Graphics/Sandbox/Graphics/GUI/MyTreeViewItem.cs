namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Graphics;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    internal class MyTreeViewItem : MyTreeViewBase
    {
        public EventHandler _Action;
        public EventHandler RightClick;
        public object Tag;
        public bool Visible = true;
        public bool Enabled = true;
        public bool IsExpanded;
        public StringBuilder Text;
        public MyToolTips ToolTip;
        public MyIconTexts IconTexts;
        public MyTreeViewBase Parent;
        private readonly float padding = 0.002f;
        private readonly float spacing = 0.01f;
        private readonly float rightBorder = 0.01f;
        private string m_icon;
        private string m_expandIcon;
        private string m_collapseIcon;
        private Vector2 m_iconSize;
        private Vector2 m_expandIconSize;
        private Vector2 m_currentOrigin;
        private Vector2 m_currentSize;
        private Vector2 m_currentTextSize;
        private float m_loadingIconRotation;

        public MyTreeViewItem(StringBuilder text, string icon, Vector2 iconSize, string expandIcon, string collapseIcon, Vector2 expandIconSize)
        {
            this.Text = text;
            this.m_icon = icon;
            this.m_expandIcon = expandIcon;
            this.m_collapseIcon = collapseIcon;
            this.m_iconSize = iconSize;
            this.m_expandIconSize = expandIconSize;
        }

        public void DoAction()
        {
            if (this._Action != null)
            {
                this._Action(this, System.EventArgs.Empty);
            }
        }

        public void Draw(float transitionAlpha)
        {
            if (this.Visible && base.TreeView.Contains(this.m_currentOrigin, this.m_currentSize))
            {
                bool isHighlight = ReferenceEquals(base.TreeView.HooveredItem, this);
                Vector2 expandIconPosition = this.GetExpandIconPosition();
                Vector2 iconPosition = this.GetIconPosition();
                Vector2 textPosition = this.GetTextPosition();
                Vector4 baseColor = this.Enabled ? Vector4.One : MyGuiConstants.TREEVIEW_DISABLED_ITEM_COLOR;
                if (ReferenceEquals(base.TreeView.FocusedItem, this))
                {
                    Color color = base.TreeView.GetColor(MyGuiConstants.TREEVIEW_SELECTED_ITEM_COLOR * baseColor, transitionAlpha);
                    if (base.TreeView.WholeRowHighlight())
                    {
                        MyGUIHelper.FillRectangle(new Vector2(base.TreeView.GetPosition().X, this.m_currentOrigin.Y), new Vector2(base.TreeView.GetBodySize().X, this.m_currentSize.Y), color);
                    }
                    else
                    {
                        MyGUIHelper.FillRectangle(this.m_currentOrigin, this.m_currentSize, color);
                    }
                }
                if (base.GetItemCount() > 0)
                {
                    Vector4 color = isHighlight ? (baseColor * MyGuiConstants.CONTROL_MOUSE_OVER_BACKGROUND_COLOR_MULTIPLIER) : baseColor;
                    MyGuiManager.DrawSpriteBatch(this.IsExpanded ? this.m_collapseIcon : this.m_expandIcon, this.m_currentOrigin + expandIconPosition, this.m_expandIconSize, base.TreeView.GetColor(color, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                }
                if (this.m_icon == null)
                {
                    this.DrawLoadingIcon(baseColor, iconPosition, transitionAlpha);
                }
                else
                {
                    MyGuiManager.DrawSpriteBatch(this.m_icon, this.m_currentOrigin + iconPosition, this.m_iconSize, base.TreeView.GetColor(baseColor, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                }
                Vector4 vector5 = isHighlight ? (MyGuiConstants.CONTROL_MOUSE_OVER_BACKGROUND_COLOR_MULTIPLIER * baseColor) : (MyGuiConstants.TREEVIEW_TEXT_COLOR * baseColor);
                MyGuiManager.DrawString("Blue", this.Text, this.m_currentOrigin + textPosition, 0.8f, new Color?(base.TreeView.GetColor(vector5, transitionAlpha)), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
                if (this.IconTexts != null)
                {
                    this.IconTexts.Draw(this.m_currentOrigin + iconPosition, this.m_iconSize, transitionAlpha, isHighlight, 1f);
                }
            }
        }

        public void DrawDraged(Vector2 position, float transitionAlpha)
        {
            if ((this.m_icon != null) || (this.Text != null))
            {
                if (this.m_icon != null)
                {
                    if (this.m_icon == null)
                    {
                        this.DrawLoadingIcon(Vector4.One, this.GetIconPosition(), transitionAlpha);
                    }
                    else
                    {
                        MyGUIHelper.OutsideBorder(position + this.GetIconPosition(), this.m_iconSize, 2, MyGuiConstants.THEMED_GUI_LINE_COLOR, true, true, true, true);
                        MyGuiManager.DrawSpriteBatch(this.m_icon, position + this.GetIconPosition(), this.m_iconSize, Color.White, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                    }
                }
                else if (this.Text != null)
                {
                    Vector2 normalizedSize = MyGuiManager.MeasureString("Blue", this.Text, 0.8f);
                    Vector2 normalizedPosition = position + this.GetTextPosition();
                    MyGUIHelper.OutsideBorder(normalizedPosition, normalizedSize, 2, MyGuiConstants.THEMED_GUI_LINE_COLOR, true, true, true, true);
                    MyGUIHelper.FillRectangle(normalizedPosition, normalizedSize, base.TreeView.GetColor(MyGuiConstants.TREEVIEW_SELECTED_ITEM_COLOR, transitionAlpha));
                    Color color = base.TreeView.GetColor(MyGuiConstants.CONTROL_MOUSE_OVER_BACKGROUND_COLOR_MULTIPLIER, transitionAlpha);
                    MyGuiManager.DrawString("Blue", this.Text, position + this.GetTextPosition(), 0.8f, new Color?(color), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
                }
            }
        }

        private void DrawLoadingIcon(Vector4 baseColor, Vector2 iconPosition, float transitionAlpha)
        {
            MyGuiManager.DrawSpriteBatch(@"Textures\GUI\screens\screen_loading_wheel.dds", (this.m_currentOrigin + iconPosition) + (this.m_iconSize / 2f), (Vector2) (0.5f * this.m_iconSize), base.TreeView.GetColor(baseColor, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, this.m_loadingIconRotation, true);
            this.m_loadingIconRotation += 0.02f;
            this.m_loadingIconRotation = this.m_loadingIconRotation % 6.283186f;
        }

        private Vector2 GetExpandIconPosition() => 
            new Vector2(this.padding, this.padding + ((this.m_currentSize.Y - this.m_expandIconSize.Y) / 2f));

        private float GetHeight() => 
            Math.Max(this.m_currentTextSize.Y, Math.Max(this.m_iconSize.Y, this.m_expandIconSize.Y));

        private Vector2 GetIconPosition() => 
            new Vector2((this.padding + this.m_expandIconSize.X) + this.spacing, this.padding);

        public Vector2 GetIconSize() => 
            this.m_iconSize;

        public int GetIndex() => 
            this.Parent.GetIndex(this);

        public Vector2 GetOffset() => 
            new Vector2(this.padding + (this.m_expandIconSize.X / 2f), (2f * this.padding) + this.GetHeight());

        public Vector2 GetPosition() => 
            this.m_currentOrigin;

        public Vector2 GetSize() => 
            this.m_currentSize;

        private Vector2 GetTextPosition()
        {
            float num = (this.m_icon != null) ? (this.m_iconSize.X + this.spacing) : 0f;
            return new Vector2(((this.padding + this.m_expandIconSize.X) + this.spacing) + num, (this.m_currentSize.Y - this.m_currentTextSize.Y) / 2f);
        }

        public bool HandleInputEx(bool hasKeyboardActiveControl)
        {
            if (!this.Visible)
            {
                return false;
            }
            bool flag = false;
            if (base.TreeView.Contains(MyGuiManager.MouseCursorPosition.X, MyGuiManager.MouseCursorPosition.Y) && MyGUIHelper.Contains(this.m_currentOrigin, this.m_currentSize, MyGuiManager.MouseCursorPosition.X, MyGuiManager.MouseCursorPosition.Y))
            {
                base.TreeView.HooveredItem = this;
            }
            if (this.Enabled && (this.DragDrop != null))
            {
                flag = this.DragDrop.HandleInput(this);
            }
            if (MyInput.Static.IsNewLeftMouseReleased())
            {
                if ((base.GetItemCount() > 0) && MyGUIHelper.Contains(this.m_currentOrigin + this.GetExpandIconPosition(), this.m_expandIconSize, MyGuiManager.MouseCursorPosition.X, MyGuiManager.MouseCursorPosition.Y))
                {
                    this.IsExpanded = !this.IsExpanded;
                    flag = true;
                    MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
                }
                else if (ReferenceEquals(base.TreeView.HooveredItem, this))
                {
                    base.TreeView.FocusItem(this);
                    flag = true;
                    MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
                }
            }
            if (this.Enabled && ReferenceEquals(base.TreeView.HooveredItem, this))
            {
                if (this._Action != null)
                {
                    this.DoAction();
                }
                else if (base.GetItemCount() > 0)
                {
                    this.IsExpanded = !this.IsExpanded;
                }
                flag = true;
            }
            if (MyInput.Static.IsNewRightMousePressed() && ReferenceEquals(base.TreeView.HooveredItem, this))
            {
                if (this.RightClick != null)
                {
                    this.RightClick(this, System.EventArgs.Empty);
                }
                flag = true;
                MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
            }
            return flag;
        }

        public Vector2 LayoutItem(Vector2 origin)
        {
            this.m_currentOrigin = origin;
            if (!this.Visible)
            {
                this.m_currentSize = Vector2.Zero;
                return Vector2.Zero;
            }
            this.m_currentTextSize = MyGuiManager.MeasureString("Blue", this.Text, 0.8f);
            float num = (this.m_icon != null) ? (this.m_iconSize.X + this.spacing) : 0f;
            float x = (((((this.padding + this.m_expandIconSize.X) + this.spacing) + num) + this.m_currentTextSize.X) + this.rightBorder) + this.padding;
            float y = (this.padding + this.GetHeight()) + this.padding;
            this.m_currentSize = new Vector2(x, y);
            if (this.IsExpanded)
            {
                Vector2 vector2 = base.LayoutItems(origin + this.GetOffset());
                x = Math.Max(x, this.GetOffset().X + vector2.X);
                y += vector2.Y;
            }
            return new Vector2(x, y);
        }

        public MyTreeViewItemDragAndDrop DragDrop { get; set; }
    }
}

