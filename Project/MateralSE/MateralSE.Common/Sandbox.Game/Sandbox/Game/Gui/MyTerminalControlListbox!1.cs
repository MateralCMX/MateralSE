namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Library.Collections;
    using VRage.ModAPI;
    using VRage.Utils;

    public class MyTerminalControlListbox<TBlock> : MyTerminalControl<TBlock>, ITerminalControlSync, IMyTerminalControlTitleTooltip, IMyTerminalControlListbox, IMyTerminalControl where TBlock: MyTerminalBlock
    {
        public MyStringId Title;
        public MyStringId Tooltip;
        public ListContentDelegate<TBlock> ListContent;
        public SelectItemDelegate<TBlock> ItemSelected;
        private MyGuiControlListbox m_listbox;
        private bool m_enableMultiSelect;
        private int m_visibleRowsCount;
        private bool m_keepScrolling;

        public MyTerminalControlListbox(string id, MyStringId title, MyStringId tooltip, bool multiSelect = false, int visibleRowsCount = 8) : base(id)
        {
            this.m_visibleRowsCount = 8;
            this.m_keepScrolling = true;
            this.Title = title;
            this.Tooltip = tooltip;
            this.m_enableMultiSelect = multiSelect;
            this.m_visibleRowsCount = visibleRowsCount;
        }

        protected override MyGuiControlBase CreateGui()
        {
            MyGuiControlListbox listbox1 = new MyGuiControlListbox();
            listbox1.VisualStyle = MyGuiControlListboxStyleEnum.Terminal;
            listbox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            listbox1.VisibleRowsCount = this.m_visibleRowsCount;
            listbox1.MultiSelect = this.m_enableMultiSelect;
            this.m_listbox = listbox1;
            this.m_listbox.ItemsSelected += new Action<MyGuiControlListbox>(this.OnItemsSelected);
            return new MyGuiControlBlockProperty(MyTexts.GetString(this.Title), MyTexts.GetString(this.Tooltip), this.m_listbox, MyGuiControlBlockPropertyLayoutEnum.Vertical, true);
        }

        private void OnItemsSelected(MyGuiControlListbox obj)
        {
            if ((this.ItemSelected != null) && (obj.SelectedItems.Count > 0))
            {
                foreach (TBlock local in base.TargetBlocks)
                {
                    this.ItemSelected(local, obj.SelectedItems);
                }
            }
        }

        protected override void OnUpdateVisual()
        {
            base.OnUpdateVisual();
            TBlock firstBlock = base.FirstBlock;
            if (firstBlock != null)
            {
                float scrollPosition = this.m_listbox.GetScrollPosition();
                this.m_listbox.Items.Clear();
                this.m_listbox.SelectedItems.Clear();
                if (this.ListContent != null)
                {
                    this.ListContent(firstBlock, this.m_listbox.Items, this.m_listbox.SelectedItems);
                }
                if (scrollPosition <= ((this.m_listbox.Items.Count - this.m_listbox.VisibleRowsCount) + 1f))
                {
                    this.m_listbox.SetScrollPosition(scrollPosition);
                }
                else
                {
                    this.m_listbox.SetScrollPosition(0f);
                }
            }
        }

        public void Serialize(BitStream stream, MyTerminalBlock block)
        {
        }

        private bool KeepScrolling
        {
            get => 
                this.m_keepScrolling;
            set => 
                (this.m_keepScrolling = value);
        }

        MyStringId IMyTerminalControlTitleTooltip.Title
        {
            get => 
                this.Title;
            set => 
                (this.Title = value);
        }

        MyStringId IMyTerminalControlTitleTooltip.Tooltip
        {
            get => 
                this.Tooltip;
            set => 
                (this.Tooltip = value);
        }

        bool IMyTerminalControlListbox.Multiselect
        {
            get => 
                this.m_enableMultiSelect;
            set => 
                (this.m_enableMultiSelect = value);
        }

        int IMyTerminalControlListbox.VisibleRowsCount
        {
            get => 
                this.m_visibleRowsCount;
            set => 
                (this.m_visibleRowsCount = value);
        }

        Action<IMyTerminalBlock, List<MyTerminalControlListBoxItem>, List<MyTerminalControlListBoxItem>> IMyTerminalControlListbox.ListContent
        {
            set => 
                (this.ListContent = delegate (TBlock block, ICollection<MyGuiControlListbox.Item> contentList, ICollection<MyGuiControlListbox.Item> selectedList) {
                    List<MyTerminalControlListBoxItem> list = new List<MyTerminalControlListBoxItem>();
                    List<MyTerminalControlListBoxItem> list2 = new List<MyTerminalControlListBoxItem>();
                    value(block, list, list2);
                    foreach (MyTerminalControlListBoxItem item in list)
                    {
                        MyStringId tooltip = item.Tooltip;
                        MyGuiControlListbox.Item item2 = new MyGuiControlListbox.Item(new StringBuilder(item.Text.ToString()), tooltip.ToString(), null, item.UserData, null);
                        contentList.Add(item2);
                        if (list2.Contains(item))
                        {
                            selectedList.Add(item2);
                        }
                    }
                });
        }

        Action<IMyTerminalBlock, List<MyTerminalControlListBoxItem>> IMyTerminalControlListbox.ItemSelected
        {
            set => 
                (this.ItemSelected = delegate (TBlock block, List<MyGuiControlListbox.Item> selectedList) {
                    List<MyTerminalControlListBoxItem> list = new List<MyTerminalControlListBoxItem>();
                    foreach (MyGuiControlListbox.Item item in selectedList)
                    {
                        string text1;
                        if ((item.ToolTip == null) || (item.ToolTip.ToolTips.Count <= 0))
                        {
                            text1 = null;
                        }
                        else
                        {
                            text1 = item.ToolTip.ToolTips.First<MyColoredText>().ToString();
                        }
                        string str = text1;
                        list.Add(new MyTerminalControlListBoxItem(MyStringId.GetOrCompute(item.Text.ToString()), MyStringId.GetOrCompute(str), item.UserData));
                    }
                    value(block, list);
                });
        }

        public delegate void ListContentDelegate(TBlock block, ICollection<MyGuiControlListbox.Item> listBoxContent, ICollection<MyGuiControlListbox.Item> listBoxSelectedItems);

        public delegate void SelectItemDelegate(TBlock block, List<MyGuiControlListbox.Item> items);
    }
}

