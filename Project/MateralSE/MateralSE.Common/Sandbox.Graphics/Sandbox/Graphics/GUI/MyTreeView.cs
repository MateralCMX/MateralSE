namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Graphics;
    using System;
    using System.Text;
    using VRage.Input;
    using VRageMath;

    internal class MyTreeView
    {
        private MyGuiControlTreeView m_control;
        private Vector2 m_position;
        private Vector2 m_size;
        private MyTreeViewBody m_body;
        private MyHScrollbar m_hScrollbar;
        private MyVScrollbar m_vScrollbar;
        private Vector2 m_scrollbarSize;
        public MyTreeViewItem FocusedItem;
        public MyTreeViewItem HooveredItem;

        public MyTreeView(MyGuiControlTreeView control, Vector2 position, Vector2 size)
        {
            this.m_control = control;
            this.m_position = position;
            this.m_size = size;
            this.m_body = new MyTreeViewBody(this, position, size);
            this.m_vScrollbar = new MyVScrollbar(control);
            this.m_hScrollbar = new MyHScrollbar(control);
            this.m_scrollbarSize = new Vector2(MyGuiConstants.TREEVIEW_VSCROLLBAR_SIZE.X, MyGuiConstants.TREEVIEW_HSCROLLBAR_SIZE.Y);
        }

        public MyTreeViewItem AddItem(StringBuilder text, string icon, Vector2 iconSize, string expandIcon, string collapseIcon, Vector2 expandIconSize) => 
            this.m_body.AddItem(text, icon, iconSize, expandIcon, collapseIcon, expandIconSize);

        public void ClearItems()
        {
            this.m_body.ClearItems();
        }

        public bool Contains(float x, float y) => 
            MyGUIHelper.Contains(this.m_body.GetPosition(), this.m_body.GetSize(), x, y);

        public bool Contains(Vector2 position, Vector2 size) => 
            MyGUIHelper.Intersects(this.m_body.GetPosition(), this.m_body.GetSize(), position, size);

        public void DeleteItem(MyTreeViewItem item)
        {
            if (ReferenceEquals(item, this.FocusedItem))
            {
                int index = item.GetIndex();
                this.FocusedItem = ((index + 1) >= this.GetItemCount()) ? (((index - 1) < 0) ? (this.FocusedItem.Parent as MyTreeViewItem) : this.GetItem((int) (index - 1))) : this.GetItem((int) (index + 1));
            }
            this.m_body.DeleteItem(item);
        }

        public void Draw(float transitionAlpha)
        {
            RectangleF normalizedRectangle = new RectangleF(this.m_body.GetPosition(), this.m_body.GetSize());
            using (MyGuiManager.UsingScissorRectangle(ref normalizedRectangle))
            {
                this.m_body.Draw(transitionAlpha);
            }
            Color color = MyGuiControlBase.ApplyColorMaskModifiers(MyGuiConstants.TREEVIEW_VERTICAL_LINE_COLOR, true, transitionAlpha);
            MyGUIHelper.OutsideBorder(this.m_position, this.m_size, 2, color, true, true, true, true);
            this.m_vScrollbar.Draw(Color.White);
            this.m_hScrollbar.Draw(Color.White);
        }

        public static bool FilterTree(ITreeView treeView, Predicate<MyTreeViewItem> itemFilter)
        {
            MyTreeViewItem item;
            int num = 0;
            int index = 0;
            goto TR_0009;
        TR_0001:
            index++;
        TR_0009:
            while (true)
            {
                if (index >= treeView.GetItemCount())
                {
                    return (num > 0);
                }
                item = treeView.GetItem(index);
                if (FilterTree(item, itemFilter) || ((item.GetItemCount() == 0) && itemFilter(item)))
                {
                    break;
                }
                item.Visible = false;
                goto TR_0001;
            }
            item.Visible = true;
            num++;
            goto TR_0001;
        }

        public void FocusItem(MyTreeViewItem item)
        {
            if (item != null)
            {
                Vector2 vector = MyGUIHelper.GetOffset(this.m_body.GetPosition(), this.m_body.GetSize(), item.GetPosition(), item.GetSize());
                this.m_vScrollbar.ChangeValue(-vector.Y);
                this.m_hScrollbar.ChangeValue(-vector.X);
            }
            this.FocusedItem = item;
        }

        public Vector2 GetBodySize() => 
            this.m_body.GetSize();

        public Color GetColor(Vector4 color, float transitionAlpha) => 
            MyGuiControlBase.ApplyColorMaskModifiers(color, true, transitionAlpha);

        public MyTreeViewItem GetItem(int index) => 
            this.m_body[index];

        public MyTreeViewItem GetItem(StringBuilder name) => 
            this.m_body.GetItem(name);

        public int GetItemCount() => 
            this.m_body.GetItemCount();

        public Vector2 GetPosition() => 
            this.m_body.GetPosition();

        public bool HandleInput()
        {
            int num1;
            int num2;
            MyTreeViewItem hooveredItem = this.HooveredItem;
            this.HooveredItem = null;
            if (this.m_body.HandleInput(this.m_control.HasFocus) || this.m_vScrollbar.HandleInput())
            {
                num1 = 1;
            }
            else
            {
                num1 = (int) this.m_hScrollbar.HandleInput();
            }
            bool flag = (bool) num1;
            if (!this.m_control.HasFocus)
            {
                goto TR_0002;
            }
            else if (((this.FocusedItem == null) && (this.m_body.GetItemCount() > 0)) && ((MyInput.Static.IsNewKeyPressed(MyKeys.Up) || (MyInput.Static.IsNewKeyPressed(MyKeys.Down) || (MyInput.Static.IsNewKeyPressed(MyKeys.Left) || MyInput.Static.IsNewKeyPressed(MyKeys.Right)))) || (MyInput.Static.DeltaMouseScrollWheelValue() != 0)))
            {
                this.FocusItem(this.m_body[0]);
                goto TR_0012;
            }
            if (this.FocusedItem != null)
            {
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Down) || ((MyInput.Static.DeltaMouseScrollWheelValue() < 0) && this.Contains(MyGuiManager.MouseCursorPosition.X, MyGuiManager.MouseCursorPosition.Y)))
                {
                    this.FocusItem(this.NextVisible(this.m_body, this.FocusedItem));
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Up) || ((MyInput.Static.DeltaMouseScrollWheelValue() > 0) && this.Contains(MyGuiManager.MouseCursorPosition.X, MyGuiManager.MouseCursorPosition.Y)))
                {
                    this.FocusItem(this.PrevVisible(this.m_body, this.FocusedItem));
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Right) && (this.FocusedItem.GetItemCount() > 0))
                {
                    if (!this.FocusedItem.IsExpanded)
                    {
                        this.FocusedItem.IsExpanded = true;
                    }
                    else
                    {
                        MyTreeViewItem item = this.NextVisible(this.FocusedItem, this.FocusedItem);
                        this.FocusItem(item);
                    }
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Left))
                {
                    if ((this.FocusedItem.GetItemCount() > 0) && this.FocusedItem.IsExpanded)
                    {
                        this.FocusedItem.IsExpanded = false;
                    }
                    else if (this.FocusedItem.Parent is MyTreeViewItem)
                    {
                        this.FocusItem(this.FocusedItem.Parent as MyTreeViewItem);
                    }
                }
                if (this.FocusedItem.GetItemCount() > 0)
                {
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.Add))
                    {
                        this.FocusedItem.IsExpanded = true;
                    }
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.Subtract))
                    {
                        this.FocusedItem.IsExpanded = false;
                    }
                }
            }
            goto TR_0012;
        TR_0002:
            if (!ReferenceEquals(this.HooveredItem, hooveredItem))
            {
                this.m_control.ShowToolTip((this.HooveredItem == null) ? null : this.HooveredItem.ToolTip);
                MyGuiSoundManager.PlaySound(GuiSounds.MouseOver);
            }
            return flag;
        TR_0012:
            if (MyInput.Static.IsNewKeyPressed(MyKeys.PageDown))
            {
                this.m_vScrollbar.PageDown();
            }
            if (MyInput.Static.IsNewKeyPressed(MyKeys.PageUp))
            {
                this.m_vScrollbar.PageUp();
            }
            if ((flag || (MyInput.Static.IsNewKeyPressed(MyKeys.PageDown) || (MyInput.Static.IsNewKeyPressed(MyKeys.PageUp) || (MyInput.Static.IsNewKeyPressed(MyKeys.Down) || (MyInput.Static.IsNewKeyPressed(MyKeys.Up) || (MyInput.Static.IsNewKeyPressed(MyKeys.Left) || (MyInput.Static.IsNewKeyPressed(MyKeys.Right) || MyInput.Static.IsNewKeyPressed(MyKeys.Add)))))))) || MyInput.Static.IsNewKeyPressed(MyKeys.Subtract))
            {
                num2 = 1;
            }
            else
            {
                num2 = (int) (MyInput.Static.DeltaMouseScrollWheelValue() != 0);
            }
            flag = (bool) num2;
            goto TR_0002;
        }

        public unsafe void Layout()
        {
            Vector2 vector2;
            this.m_body.Layout(Vector2.Zero);
            Vector2 realSize = this.m_body.GetRealSize();
            bool local1 = ((this.m_size.Y - this.m_scrollbarSize.Y) < realSize.Y) && ((this.m_size.X - this.m_scrollbarSize.X) < realSize.X);
            bool flag = local1 || (this.m_size.Y < realSize.Y);
            bool flag2 = local1 || (this.m_size.X < realSize.X);
            Vector2* vectorPtr1 = (Vector2*) new Vector2(flag ? (this.m_size.X - this.m_scrollbarSize.X) : this.m_size.X, flag2 ? (this.m_size.Y - this.m_scrollbarSize.Y) : this.m_size.Y);
            this.m_vScrollbar.Visible = flag;
            vectorPtr1 = (Vector2*) ref vector2;
            this.m_vScrollbar.Init(realSize.Y, vector2.Y);
            this.m_hScrollbar.Visible = flag2;
            this.m_hScrollbar.Init(realSize.X, vector2.X);
            this.m_body.SetSize(vector2);
            this.m_body.Layout(new Vector2(this.m_hScrollbar.Value, this.m_vScrollbar.Value));
        }

        private MyTreeViewItem NextVisible(ITreeView iTreeView, MyTreeViewItem focused)
        {
            bool found = false;
            this.TraverseVisible(this.m_body, delegate (MyTreeViewItem a) {
                if (ReferenceEquals(a, focused))
                {
                    found = true;
                }
                else if (found)
                {
                    focused = a;
                    found = false;
                }
            });
            return focused;
        }

        private MyTreeViewItem PrevVisible(ITreeView iTreeView, MyTreeViewItem focused)
        {
            MyTreeViewItem pred = focused;
            this.TraverseVisible(this.m_body, delegate (MyTreeViewItem a) {
                if (ReferenceEquals(a, focused))
                {
                    focused = pred;
                }
                else
                {
                    pred = a;
                }
            });
            return focused;
        }

        public void SetPosition(Vector2 position)
        {
            this.m_position = position;
            this.m_body.SetPosition(position);
        }

        public void SetSize(Vector2 size)
        {
            this.m_size = size;
            this.m_body.SetSize(size);
        }

        private void TraverseVisible(ITreeView iTreeView, Action<MyTreeViewItem> action)
        {
            for (int i = 0; i < iTreeView.GetItemCount(); i++)
            {
                MyTreeViewItem item = iTreeView.GetItem(i);
                if (item.Visible)
                {
                    action(item);
                    if (item.IsExpanded)
                    {
                        this.TraverseVisible(item, action);
                    }
                }
            }
        }

        public bool WholeRowHighlight() => 
            this.m_control.WholeRowHighlight;
    }
}

