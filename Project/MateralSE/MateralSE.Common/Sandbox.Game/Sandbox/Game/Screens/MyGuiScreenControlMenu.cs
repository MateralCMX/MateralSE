namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenControlMenu : MyGuiScreenBase
    {
        private const float ITEM_SIZE = 0.03f;
        private MyGuiControlScrollablePanel m_scrollPanel;
        private List<MyGuiControlItem> m_items;
        private int m_selectedItem;
        private RectangleF m_itemsRect;

        public MyGuiScreenControlMenu() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.4f, 0.7f), false, null, 0f, 0f)
        {
            base.DrawMouseCursor = false;
            base.CanHideOthers = false;
            this.m_items = new List<MyGuiControlItem>();
        }

        public void AddItem(MyAbstractControlMenuItem item)
        {
            this.m_items.Add(new MyGuiControlItem(item, new Vector2(base.Size.Value.X - 0.1f, 0.03f)));
        }

        public void AddItems(params MyAbstractControlMenuItem[] items)
        {
            foreach (MyAbstractControlMenuItem item in items)
            {
                this.AddItem(item);
            }
        }

        public void ClearItems()
        {
            this.m_items.Clear();
        }

        public override bool Draw()
        {
            base.Draw();
            if (this.m_selectedItem != -1)
            {
                MyGuiControlItem item = this.m_items[this.m_selectedItem];
                if (item != null)
                {
                    this.m_itemsRect.Position = this.m_scrollPanel.GetPositionAbsoluteTopLeft();
                    using (MyGuiManager.UsingScissorRectangle(ref this.m_itemsRect))
                    {
                        Vector2 positionAbsoluteTopLeft = item.GetPositionAbsoluteTopLeft();
                        MyGuiManager.DrawSpriteBatch(MyGuiConstants.TEXTURE_HIGHLIGHT_DARK.Center.Texture, positionAbsoluteTopLeft, item.Size, new Color(1f, 1f, 1f, 0.8f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                    }
                }
            }
            return true;
        }

        public override string GetFriendlyName() => 
            "Control menu screen";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            if ((MyInput.Static.IsNewKeyPressed(MyKeys.Up) || MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.MOVE_UP, MyControlStateType.NEW_PRESSED, false)) || (MyInput.Static.DeltaMouseScrollWheelValue() > 0))
            {
                this.UpdateSelectedItem(true);
                this.UpdateScroll();
            }
            else if ((MyInput.Static.IsNewKeyPressed(MyKeys.Down) || MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.MOVE_DOWN, MyControlStateType.NEW_PRESSED, false)) || (MyInput.Static.DeltaMouseScrollWheelValue() < 0))
            {
                this.UpdateSelectedItem(false);
                this.UpdateScroll();
            }
            else if ((MyInput.Static.IsNewKeyPressed(MyKeys.Escape) || (MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.CANCEL, MyControlStateType.NEW_PRESSED, false) || MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsSpace.CONTROL_MENU, MyControlStateType.NEW_PRESSED, false))) || MyInput.Static.IsNewRightMousePressed())
            {
                this.Canceling();
            }
            if (this.m_selectedItem != -1)
            {
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Right))
                {
                    goto TR_0000;
                }
                else if (!MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.MOVE_RIGHT, MyControlStateType.NEW_PRESSED, false))
                {
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.Left))
                    {
                        goto TR_0001;
                    }
                    else if (!MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.MOVE_LEFT, MyControlStateType.NEW_PRESSED, false))
                    {
                        if ((MyInput.Static.IsNewKeyPressed(MyKeys.Enter) || MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.ACCEPT, MyControlStateType.NEW_PRESSED, false)) || MyInput.Static.IsNewLeftMousePressed())
                        {
                            this.m_items[this.m_selectedItem].UpdateItem(ItemUpdateType.Activate);
                        }
                    }
                    else
                    {
                        goto TR_0001;
                    }
                }
                else
                {
                    goto TR_0000;
                }
            }
            return;
        TR_0000:
            this.m_items[this.m_selectedItem].UpdateItem(ItemUpdateType.Next);
            return;
        TR_0001:
            this.m_items[this.m_selectedItem].UpdateItem(ItemUpdateType.Previous);
        }

        protected override void OnClosed()
        {
            MyGuiScreenGamePlay.ActiveGameplayScreen = null;
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            base.AddCaption(MyCommonTexts.ScreenControlMenu_Title, captionTextColor, captionOffset, 1.3f);
            captionOffset = null;
            captionTextColor = null;
            MyGuiControlParent scrolledControl = new MyGuiControlParent(captionOffset, new Vector2(base.Size.Value.X - 0.05f, this.m_items.Count * 0.03f), captionTextColor, null);
            this.m_scrollPanel = new MyGuiControlScrollablePanel(scrolledControl);
            this.m_scrollPanel.ScrollbarVEnabled = true;
            this.m_scrollPanel.ScrollBarVScale = 1f;
            this.m_scrollPanel.Size = new Vector2(base.Size.Value.X - 0.05f, base.Size.Value.Y - 0.1f);
            this.m_scrollPanel.Position = new Vector2(0f, 0.05f);
            MyLayoutVertical vertical = new MyLayoutVertical(scrolledControl, 20f);
            foreach (MyGuiControlItem item in this.m_items)
            {
                vertical.Add(item, MyAlignH.Left, true);
            }
            this.m_itemsRect.Position = this.m_scrollPanel.GetPositionAbsoluteTopLeft();
            this.m_itemsRect.Size = new Vector2(base.Size.Value.X - 0.05f, base.Size.Value.Y - 0.1f);
            base.FocusedControl = scrolledControl;
            this.m_selectedItem = (this.m_items.Count != 0) ? 0 : -1;
            this.Controls.Add(this.m_scrollPanel);
        }

        private unsafe void UpdateScroll()
        {
            if (this.m_selectedItem != -1)
            {
                MyGuiControlItem item = this.m_items[this.m_selectedItem];
                MyGuiControlItem item2 = this.m_items[this.m_items.Count - 1];
                Vector2 positionAbsoluteTopLeft = item.GetPositionAbsoluteTopLeft();
                Vector2 vector2 = item2.GetPositionAbsoluteTopLeft() + item2.Size;
                float y = this.m_scrollPanel.GetPositionAbsoluteTopLeft().Y;
                float* singlePtr1 = (float*) ref positionAbsoluteTopLeft.Y;
                singlePtr1[0] -= y;
                float* singlePtr2 = (float*) ref vector2.Y;
                singlePtr2[0] -= y;
                float num3 = (positionAbsoluteTopLeft.Y / vector2.Y) * this.m_scrollPanel.ScrolledAreaSize.Y;
                float num4 = ((positionAbsoluteTopLeft.Y + item.Size.Y) / vector2.Y) * this.m_scrollPanel.ScrolledAreaSize.Y;
                if (num3 < this.m_scrollPanel.ScrollbarVPosition)
                {
                    this.m_scrollPanel.ScrollbarVPosition = num3;
                }
                if (num4 > this.m_scrollPanel.ScrollbarVPosition)
                {
                    this.m_scrollPanel.ScrollbarVPosition = num4;
                }
            }
        }

        private void UpdateSelectedItem(bool up)
        {
            bool flag = false;
            if (!up)
            {
                for (int i = 0; i < this.m_items.Count; i++)
                {
                    this.m_selectedItem = (this.m_selectedItem + 1) % this.m_items.Count;
                    if (this.m_items[this.m_selectedItem].IsItemEnabled)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.m_items.Count; i++)
                {
                    this.m_selectedItem--;
                    if (this.m_selectedItem < 0)
                    {
                        this.m_selectedItem = this.m_items.Count - 1;
                    }
                    if (this.m_items[this.m_selectedItem].IsItemEnabled)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag)
            {
                this.m_selectedItem = -1;
            }
        }

        private enum ItemUpdateType
        {
            Activate,
            Next,
            Previous
        }

        private class MyGuiControlItem : MyGuiControlParent
        {
            private MyAbstractControlMenuItem m_item;
            private MyGuiControlLabel m_label;
            private MyGuiControlLabel m_value;

            public MyGuiControlItem(MyAbstractControlMenuItem item, Vector2? size = new Vector2?()) : base(position, size, colorMask, null)
            {
                Vector2? position = null;
                VRageMath.Vector4? colorMask = null;
                this.m_item = item;
                this.m_item.UpdateValue();
                position = null;
                position = null;
                colorMask = null;
                this.m_label = new MyGuiControlLabel(position, position, item.ControlLabel, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                position = null;
                position = null;
                colorMask = null;
                this.m_value = new MyGuiControlLabel(position, position, item.CurrentValue, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                new MyLayoutVertical(this, 28f).Add(this.m_label, this.m_value);
            }

            public override MyGuiControlBase GetNextFocusControl(MyGuiControlBase currentFocusControl, bool forwardMovement) => 
                (!base.HasFocus ? this : base.Owner.GetNextFocusControl(this, forwardMovement));

            private void RefreshValueLabel()
            {
                this.m_item.UpdateValue();
                this.m_value.Text = this.m_item.CurrentValue;
            }

            public override void Update()
            {
                base.Update();
                this.RefreshValueLabel();
                if (this.IsItemEnabled)
                {
                    this.m_label.Enabled = true;
                    this.m_value.Enabled = true;
                }
                else
                {
                    this.m_label.Enabled = false;
                    this.m_value.Enabled = false;
                }
            }

            internal void UpdateItem(MyGuiScreenControlMenu.ItemUpdateType updateType)
            {
                switch (updateType)
                {
                    case MyGuiScreenControlMenu.ItemUpdateType.Activate:
                        if (this.m_item.Enabled)
                        {
                            this.m_item.Activate();
                        }
                        break;

                    case MyGuiScreenControlMenu.ItemUpdateType.Next:
                        this.m_item.Next();
                        break;

                    case MyGuiScreenControlMenu.ItemUpdateType.Previous:
                        this.m_item.Previous();
                        break;

                    default:
                        break;
                }
                this.RefreshValueLabel();
            }

            public bool IsItemEnabled =>
                this.m_item.Enabled;
        }
    }
}

