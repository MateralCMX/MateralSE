namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlContextMenu : MyGuiControlBase
    {
        private const int NUM_VISIBLE_ITEMS = 20;
        private int m_numItems;
        private MyGuiControlListbox m_itemsList;
        [CompilerGenerated]
        private Action<MyGuiControlContextMenu, EventArgs> ItemClicked;
        private MyContextMenuKeyTimerController[] m_keys;
        private bool m_allowKeyboardNavigation;

        public event Action<MyGuiControlContextMenu, EventArgs> ItemClicked
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlContextMenu, EventArgs> itemClicked = this.ItemClicked;
                while (true)
                {
                    Action<MyGuiControlContextMenu, EventArgs> a = itemClicked;
                    Action<MyGuiControlContextMenu, EventArgs> action3 = (Action<MyGuiControlContextMenu, EventArgs>) Delegate.Combine(a, value);
                    itemClicked = Interlocked.CompareExchange<Action<MyGuiControlContextMenu, EventArgs>>(ref this.ItemClicked, action3, a);
                    if (ReferenceEquals(itemClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlContextMenu, EventArgs> itemClicked = this.ItemClicked;
                while (true)
                {
                    Action<MyGuiControlContextMenu, EventArgs> source = itemClicked;
                    Action<MyGuiControlContextMenu, EventArgs> action3 = (Action<MyGuiControlContextMenu, EventArgs>) Delegate.Remove(source, value);
                    itemClicked = Interlocked.CompareExchange<Action<MyGuiControlContextMenu, EventArgs>>(ref this.ItemClicked, action3, source);
                    if (ReferenceEquals(itemClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiControlContextMenu() : base(nullable, nullable, nullable2, null, null, true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            Vector2? nullable = null;
            nullable = null;
            this.m_itemsList = new MyGuiControlListbox();
            this.m_itemsList.Name = "ContextMenuListbox";
            this.m_itemsList.VisibleRowsCount = 20;
            base.Enabled = false;
            this.m_keys = new MyContextMenuKeyTimerController[] { new MyContextMenuKeyTimerController(MyKeys.Up), new MyContextMenuKeyTimerController(MyKeys.Down), new MyContextMenuKeyTimerController(MyKeys.Enter) };
            base.Name = "ContextMenu";
            base.Elements.Add(this.m_itemsList);
        }

        public void Activate(bool autoPositionOnMouseTip = true)
        {
            if (!autoPositionOnMouseTip)
            {
                this.m_itemsList.Position = base.Position;
                this.m_itemsList.OriginAlign = base.OriginAlign;
            }
            else
            {
                this.m_itemsList.Position = MyGuiManager.MouseCursorPosition;
                this.m_itemsList.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.FitContextMenuToScreen();
            }
            this.m_itemsList.Visible = true;
            this.m_itemsList.IsActiveControl = true;
            base.Visible = true;
            base.IsActiveControl = true;
        }

        public void AddItem(StringBuilder text, string tooltip = "", string icon = "", object userData = null)
        {
            MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(text, null, icon, userData, null);
            int? position = null;
            this.m_itemsList.Add(item, position);
            int numItems = this.m_numItems;
            this.m_numItems = numItems + 1;
            this.m_itemsList.VisibleRowsCount = Math.Min(20, numItems) + 1;
        }

        public void Clear()
        {
            this.m_itemsList.Items.Clear();
            this.m_numItems = 0;
        }

        private void CreateContextMenu()
        {
            Vector2? position = null;
            this.m_itemsList = new MyGuiControlListbox(position, MyGuiControlListboxStyleEnum.ContextMenu);
            this.m_itemsList.HighlightType = MyGuiControlHighlightType.WHEN_CURSOR_OVER;
            this.m_itemsList.Enabled = true;
            this.m_itemsList.ItemClicked += new Action<MyGuiControlListbox>(this.list_ItemClicked);
            this.m_itemsList.MultiSelect = false;
        }

        public void CreateNewContextMenu()
        {
            this.Clear();
            this.Deactivate();
            this.CreateContextMenu();
        }

        public void Deactivate()
        {
            this.m_itemsList.IsActiveControl = false;
            this.m_itemsList.Visible = false;
            base.IsActiveControl = false;
            base.Visible = false;
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            this.m_itemsList.Draw(transitionAlpha * this.m_itemsList.Alpha, backgroundTransitionAlpha * this.m_itemsList.Alpha);
        }

        private void FitContextMenuToScreen()
        {
            if (this.m_itemsList.Position.X < 0f)
            {
                this.m_itemsList.Position = new Vector2(0f, this.m_itemsList.Position.Y);
            }
            if ((this.m_itemsList.Position.X + this.m_itemsList.Size.X) >= 1f)
            {
                this.m_itemsList.Position = new Vector2(1f - this.m_itemsList.Size.X, this.m_itemsList.Position.Y);
            }
            if (this.m_itemsList.Position.Y < 0f)
            {
                this.m_itemsList.Position = new Vector2(this.m_itemsList.Position.X, 0f);
            }
            if ((this.m_itemsList.Position.Y + this.m_itemsList.Size.Y) >= 1f)
            {
                this.m_itemsList.Position = new Vector2(this.m_itemsList.Position.X, 1f - this.m_itemsList.Size.Y);
            }
        }

        public Vector2 GetListBoxSize() => 
            this.m_itemsList.Size;

        public override MyGuiControlBase HandleInput()
        {
            if (((MyInput.Static.IsNewMousePressed(MyMouseButtonsEnum.Left) || MyInput.Static.IsNewMousePressed(MyMouseButtonsEnum.Right)) && base.Visible) && !base.IsMouseOver)
            {
                this.Deactivate();
            }
            if (MyInput.Static.IsKeyPress(MyKeys.Escape) && base.Visible)
            {
                this.Deactivate();
                return this;
            }
            if (this.AllowKeyboardNavigation)
            {
                Vector2 mouseCursorPosition = MyGuiManager.MouseCursorPosition;
                if (((mouseCursorPosition.X >= this.m_itemsList.Position.X) && ((mouseCursorPosition.X <= (this.m_itemsList.Position.X + this.m_itemsList.Size.X)) && (mouseCursorPosition.Y >= this.m_itemsList.Position.Y))) && (mouseCursorPosition.Y <= (this.m_itemsList.Position.Y + this.m_itemsList.Size.Y)))
                {
                    this.m_itemsList.SelectedItems.Clear();
                }
                else
                {
                    if (MyInput.Static.IsKeyPress(MyKeys.Up) && this.IsEnoughDelay(MyContextMenuKeys.UP, 100))
                    {
                        this.UpdateLastKeyPressTimes(MyContextMenuKeys.UP);
                        this.SelectPrevious();
                        return this;
                    }
                    if (MyInput.Static.IsKeyPress(MyKeys.Down) && this.IsEnoughDelay(MyContextMenuKeys.DOWN, 100))
                    {
                        this.UpdateLastKeyPressTimes(MyContextMenuKeys.DOWN);
                        this.SelectNext();
                        return this;
                    }
                    if (MyInput.Static.IsKeyPress(MyKeys.Enter) && this.IsEnoughDelay(MyContextMenuKeys.ENTER, 100))
                    {
                        this.UpdateLastKeyPressTimes(MyContextMenuKeys.ENTER);
                        if (this.m_itemsList.SelectedItems.Count > 0)
                        {
                            EventArgs args;
                            args.ItemIndex = this.m_itemsList.Items.IndexOf(this.m_itemsList.SelectedItems[0]);
                            args.UserData = this.m_itemsList.SelectedItems[0].UserData;
                            this.ItemClicked(this, args);
                            this.Deactivate();
                            return this;
                        }
                    }
                }
            }
            return this.m_itemsList.HandleInput();
        }

        private bool IsEnoughDelay(MyContextMenuKeys key, int forcedDelay)
        {
            MyContextMenuKeyTimerController controller = this.m_keys[(int) key];
            return ((controller != null) ? ((MyGuiManager.TotalTimeInMilliseconds - controller.LastKeyPressTime) > forcedDelay) : true);
        }

        public bool IsGuiControlEqual(MyGuiControlBase control) => 
            ReferenceEquals(this.m_itemsList, control);

        private void list_ItemClicked(MyGuiControlListbox sender)
        {
            if (base.Visible)
            {
                int index = -1;
                object userData = null;
                using (List<MyGuiControlListbox.Item>.Enumerator enumerator = sender.SelectedItems.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        MyGuiControlListbox.Item current = enumerator.Current;
                        index = sender.Items.IndexOf(current);
                        userData = current.UserData;
                    }
                }
                if (this.ItemClicked != null)
                {
                    EventArgs args = new EventArgs {
                        ItemIndex = index,
                        UserData = userData
                    };
                    this.ItemClicked(this, args);
                }
                if (!this.m_itemsList.IsOverScrollBar())
                {
                    this.Deactivate();
                }
            }
        }

        private void SelectNext()
        {
            int num = 0;
            int num2 = -1;
            int count = this.m_itemsList.Items.Count;
            foreach (MyGuiControlListbox.Item item in this.m_itemsList.Items)
            {
                if (this.m_itemsList.SelectedItems.Contains(item))
                {
                    num2 = num;
                }
                num++;
            }
            this.m_itemsList.SelectedItems.Clear();
            if (num2 >= 0)
            {
                this.m_itemsList.SelectedItems.Add(this.m_itemsList.Items[(((num2 + 1) % count) + count) % count]);
            }
            else if (this.m_itemsList.Items.Count > 0)
            {
                this.m_itemsList.SelectedItems.Add(this.m_itemsList.Items[0]);
            }
        }

        private void SelectPrevious()
        {
            int num = -1;
            int num2 = 0;
            int count = this.m_itemsList.Items.Count;
            foreach (MyGuiControlListbox.Item item in this.m_itemsList.Items)
            {
                if (this.m_itemsList.SelectedItems.Contains(item))
                {
                    num = num2;
                }
                num2++;
            }
            this.m_itemsList.SelectedItems.Clear();
            if (num >= 0)
            {
                this.m_itemsList.SelectedItems.Add(this.m_itemsList.Items[(((num - 1) % count) + count) % count]);
            }
            else if (this.m_itemsList.Items.Count > 0)
            {
                this.m_itemsList.SelectedItems.Add(this.m_itemsList.Items[0]);
            }
        }

        private void UpdateLastKeyPressTimes(MyContextMenuKeys key)
        {
            MyContextMenuKeyTimerController controller = this.m_keys[(int) key];
            if (controller != null)
            {
                controller.LastKeyPressTime = MyGuiManager.TotalTimeInMilliseconds;
            }
        }

        public List<MyGuiControlListbox.Item> Items =>
            this.m_itemsList.Items.ToList<MyGuiControlListbox.Item>();

        public bool AllowKeyboardNavigation
        {
            get => 
                this.m_allowKeyboardNavigation;
            set
            {
                if (this.m_allowKeyboardNavigation != value)
                {
                    this.m_allowKeyboardNavigation = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EventArgs
        {
            public int ItemIndex;
            public object UserData;
        }

        private enum MyContextMenuKeys
        {
            UP,
            DOWN,
            ENTER
        }

        private class MyContextMenuKeyTimerController
        {
            public MyKeys Key;
            public int LastKeyPressTime;

            public MyContextMenuKeyTimerController(MyKeys key)
            {
                this.Key = key;
                this.LastKeyPressTime = -60000;
            }
        }
    }
}

