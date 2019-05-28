namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlInventoryOwner : MyGuiControlBase
    {
        private static readonly StringBuilder m_textCache = new StringBuilder();
        private static readonly Vector2 m_internalPadding = ((Vector2) (15f / MyGuiConstants.GUI_OPTIMAL_SIZE));
        private MyGuiControlLabel m_nameLabel;
        private List<MyGuiControlLabel> m_massLabels;
        private List<MyGuiControlLabel> m_volumeLabels;
        private List<MyGuiControlGrid> m_inventoryGrids;
        private MyEntity m_inventoryOwner;
        [CompilerGenerated]
        private Action<MyGuiControlInventoryOwner> InventoryContentsChanged;

        public event Action<MyGuiControlInventoryOwner> InventoryContentsChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlInventoryOwner> inventoryContentsChanged = this.InventoryContentsChanged;
                while (true)
                {
                    Action<MyGuiControlInventoryOwner> a = inventoryContentsChanged;
                    Action<MyGuiControlInventoryOwner> action3 = (Action<MyGuiControlInventoryOwner>) Delegate.Combine(a, value);
                    inventoryContentsChanged = Interlocked.CompareExchange<Action<MyGuiControlInventoryOwner>>(ref this.InventoryContentsChanged, action3, a);
                    if (ReferenceEquals(inventoryContentsChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlInventoryOwner> inventoryContentsChanged = this.InventoryContentsChanged;
                while (true)
                {
                    Action<MyGuiControlInventoryOwner> source = inventoryContentsChanged;
                    Action<MyGuiControlInventoryOwner> action3 = (Action<MyGuiControlInventoryOwner>) Delegate.Remove(source, value);
                    inventoryContentsChanged = Interlocked.CompareExchange<Action<MyGuiControlInventoryOwner>>(ref this.InventoryContentsChanged, action3, source);
                    if (ReferenceEquals(inventoryContentsChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiControlInventoryOwner(MyEntity owner, VRageMath.Vector4 labelColorMask) : base(nullable, nullable, nullable2, null, texture1, false, true, true, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            Vector2? nullable = null;
            nullable = null;
            MyGuiSizedTexture texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\item_highlight_dark.dds"
            };
            MyGuiCompositeTexture texture1 = new MyGuiCompositeTexture(null);
            texture1.Center = texture;
            MyStringId? text = null;
            this.m_nameLabel = this.MakeLabel(text, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            this.m_nameLabel.ColorMask = labelColorMask;
            this.m_massLabels = new List<MyGuiControlLabel>();
            this.m_volumeLabels = new List<MyGuiControlLabel>();
            this.m_inventoryGrids = new List<MyGuiControlGrid>();
            base.ShowTooltipWhenDisabled = true;
            this.m_nameLabel.Name = "NameLabel";
            base.Elements.Add(this.m_nameLabel);
            this.InventoryOwner = owner;
        }

        private void AttachOwner(MyEntity owner)
        {
            if (owner != null)
            {
                this.m_nameLabel.Text = owner.DisplayNameText;
                for (int i = 0; i < owner.InventoryCount; i++)
                {
                    MyInventory inventory = owner.GetInventory(i);
                    inventory.UserData = this;
                    inventory.ContentsChanged += new Action<MyInventoryBase>(this.inventory_OnContentsChanged);
                    MyGuiControlLabel control = this.MakeMassLabel(inventory);
                    base.Elements.Add(control);
                    this.m_massLabels.Add(control);
                    MyGuiControlLabel label2 = this.MakeVolumeLabel(inventory);
                    base.Elements.Add(label2);
                    this.m_volumeLabels.Add(label2);
                    MyGuiControlGrid grid = this.MakeInventoryGrid(inventory);
                    base.Elements.Add(grid);
                    this.m_inventoryGrids.Add(grid);
                }
                this.m_inventoryOwner = owner;
                this.RefreshInventoryContents();
            }
        }

        private Vector2 ComputeControlPositionFromTopCenter(Vector2 offset) => 
            (new Vector2(0f, m_internalPadding.Y + (base.Size.Y * -0.5f)) + offset);

        private Vector2 ComputeControlPositionFromTopLeft(Vector2 offset) => 
            ((m_internalPadding + (base.Size * -0.5f)) + offset);

        private Vector2 ComputeControlSize()
        {
            float y = this.m_nameLabel.Size.Y + (m_internalPadding.Y * 2f);
            for (int i = 0; i < this.m_inventoryGrids.Count; i++)
            {
                MyGuiControlGrid grid = this.m_inventoryGrids[i];
                MyGuiControlLabel label = this.m_massLabels[i];
                y = (y + (label.Size.Y + (m_internalPadding.Y * 0.5f))) + (grid.Size.Y + m_internalPadding.Y);
            }
            return new Vector2(base.Size.X, y);
        }

        public static MyGuiGridItem CreateInventoryGridItem(MyPhysicalInventoryItem item)
        {
            MyPhysicalItemDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(item.Content);
            double num = physicalItemDefinition.Mass * ((double) item.Amount);
            double num2 = (physicalItemDefinition.Volume * 1000f) * ((double) item.Amount);
            object userData = item;
            object[] objArray1 = new object[5];
            objArray1[0] = physicalItemDefinition.DisplayNameText;
            objArray1[1] = (num < 0.01) ? "<0.01" : num.ToString("N", CultureInfo.InvariantCulture);
            object[] local5 = objArray1;
            object[] local6 = objArray1;
            local6[2] = (num2 < 0.01) ? "<0.01" : num2.ToString("N", CultureInfo.InvariantCulture);
            object[] local3 = local6;
            object[] local4 = local6;
            local4[3] = (item.Content.Flags == MyItemFlags.Damaged) ? MyTexts.Get(MyCommonTexts.ItemDamagedDescription) : MyTexts.Get(MySpaceTexts.Blank);
            object[] local1 = local4;
            object[] args = local4;
            args[4] = (physicalItemDefinition.ExtraInventoryTooltipLine != null) ? physicalItemDefinition.ExtraInventoryTooltipLine : MyTexts.Get(MySpaceTexts.Blank);
            MyGuiGridItem item2 = new MyGuiGridItem(physicalItemDefinition.Icons, null, new StringBuilder().AppendFormat(MyTexts.GetString(MySpaceTexts.ToolTipTerminalInventory_ItemInfo), args).ToString(), userData, true);
            if (MyFakes.SHOW_INVENTORY_ITEM_IDS)
            {
                item2.ToolTip.AddToolTip(new StringBuilder().AppendFormat("ItemID: {0}", item.ItemId).ToString(), 0.7f, "Blue");
            }
            FormatItemAmount(item, m_textCache);
            item2.AddText(m_textCache, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
            m_textCache.Clear();
            if (physicalItemDefinition.IconSymbol != null)
            {
                item2.AddText(MyTexts.Get(physicalItemDefinition.IconSymbol.Value), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            }
            return item2;
        }

        private void DetachOwner()
        {
            if (this.m_inventoryOwner != null)
            {
                for (int i = 0; i < this.m_inventoryOwner.InventoryCount; i++)
                {
                    MyInventory inventory = this.m_inventoryOwner.GetInventory(i);
                    if (inventory.UserData == this)
                    {
                        inventory.UserData = null;
                    }
                    inventory.ContentsChanged -= new Action<MyInventoryBase>(this.inventory_OnContentsChanged);
                }
                for (int j = 0; j < this.m_inventoryGrids.Count; j++)
                {
                    base.Elements.Remove(this.m_massLabels[j]);
                    base.Elements.Remove(this.m_volumeLabels[j]);
                    base.Elements.Remove(this.m_inventoryGrids[j]);
                }
                this.m_inventoryGrids.Clear();
                this.m_massLabels.Clear();
                this.m_volumeLabels.Clear();
                this.m_inventoryOwner = null;
            }
        }

        public static void FormatItemAmount(MyPhysicalInventoryItem item, StringBuilder text)
        {
            double amount = (double) item.Amount;
            if ((item.Content.GetType() == typeof(MyObjectBuilder_GasContainerObject)) || (item.Content.GetType().BaseType == typeof(MyObjectBuilder_GasContainerObject)))
            {
                amount = ((MyObjectBuilder_GasContainerObject) item.Content).GasLevel * 100f;
            }
            FormatItemAmount(item.Content.GetType(), amount, text);
        }

        public static void FormatItemAmount(System.Type typeId, double amount, StringBuilder text)
        {
            try
            {
                if ((typeId != typeof(MyObjectBuilder_Ore)) && !(typeId == typeof(MyObjectBuilder_Ingot)))
                {
                    if (typeId != typeof(MyObjectBuilder_PhysicalGunObject))
                    {
                        if ((typeId == typeof(MyObjectBuilder_GasContainerObject)) || (typeId.BaseType == typeof(MyObjectBuilder_GasContainerObject)))
                        {
                            text.Append(((int) amount).ToString() + "%");
                        }
                        else
                        {
                            int num3 = (int) amount;
                            if ((amount - num3) > 0.0)
                            {
                                text.Append('~');
                            }
                            text.Append(num3.ToString("#,##0.x", CultureInfo.InvariantCulture));
                        }
                    }
                }
                else if (amount < 0.01)
                {
                    text.Append(amount.ToString("<0.01", CultureInfo.InvariantCulture));
                }
                else if (amount < 10.0)
                {
                    text.Append(amount.ToString("0.##", CultureInfo.InvariantCulture));
                }
                else if (amount < 100.0)
                {
                    text.Append(amount.ToString("0.#", CultureInfo.InvariantCulture));
                }
                else if (amount < 1000.0)
                {
                    text.Append(amount.ToString("0.", CultureInfo.InvariantCulture));
                }
                else if (amount < 10000.0)
                {
                    text.Append((amount / 1000.0).ToString("0.##k", CultureInfo.InvariantCulture));
                }
                else if (amount < 100000.0)
                {
                    text.Append((amount / 1000.0).ToString("0.#k", CultureInfo.InvariantCulture));
                }
                else
                {
                    text.Append((amount / 1000.0).ToString("#,##0.k", CultureInfo.InvariantCulture));
                }
            }
            catch (OverflowException)
            {
                text.Append("ERROR");
            }
        }

        public override MyGuiControlBase HandleInput()
        {
            base.HandleInput();
            return ((base.HandleInputElements() == null) ? null : this);
        }

        private void inventory_OnContentsChanged(MyInventoryBase obj)
        {
            this.RefreshInventoryContents();
            if (this.InventoryContentsChanged != null)
            {
                this.InventoryContentsChanged(this);
            }
        }

        private MyGuiControlGrid MakeInventoryGrid(MyInventory inventory)
        {
            MyGuiControlGrid grid1 = new MyGuiControlGrid();
            grid1.Name = "InventoryGrid";
            grid1.VisualStyle = MyGuiControlGridStyleEnum.Inventory;
            grid1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            grid1.ColumnsCount = 7;
            grid1.RowsCount = 1;
            grid1.ShowTooltipWhenDisabled = true;
            grid1.UserData = inventory;
            return grid1;
        }

        private MyGuiControlLabel MakeLabel(MyStringId? text = new MyStringId?(), MyGuiDrawAlignEnum labelAlign = 0)
        {
            float textScale = 0.6616216f;
            Vector2? position = null;
            position = null;
            VRageMath.Vector4? colorMask = null;
            MyGuiControlLabel label1 = new MyGuiControlLabel(position, position, (text != null) ? MyTexts.GetString(text.Value) : null, colorMask, textScale, "Blue", labelAlign);
            label1.AutoEllipsis = true;
            return label1;
        }

        private MyGuiControlLabel MakeMassLabel(MyInventory inventory)
        {
            MyGuiControlLabel label1 = this.MakeLabel(new MyStringId?(MySpaceTexts.ScreenTerminalInventory_Mass), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            label1.Name = "MassLabel";
            return label1;
        }

        private MyGuiControlLabel MakeVolumeLabel(MyInventory inventory)
        {
            MyGuiControlLabel label1 = this.MakeLabel(new MyStringId?(MySpaceTexts.ScreenTerminalInventory_Volume), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            label1.Name = "VolumeLabel";
            return label1;
        }

        protected override void OnEnabledChanged()
        {
            this.RefreshInternals();
            base.OnEnabledChanged();
        }

        public override void OnRemoving()
        {
            if (this.m_inventoryOwner != null)
            {
                this.DetachOwner();
            }
            this.m_inventoryGrids.Clear();
            this.InventoryContentsChanged = null;
            base.OnRemoving();
        }

        protected override void OnSizeChanged()
        {
            this.RefreshInternals();
            base.Size = this.ComputeControlSize();
            base.OnSizeChanged();
        }

        private unsafe void RefreshInternals()
        {
            if (this.m_nameLabel != null)
            {
                Vector2 vector = base.Size - (m_internalPadding * 2f);
                this.m_nameLabel.Position = this.ComputeControlPositionFromTopLeft(Vector2.Zero);
                this.m_nameLabel.Size = new Vector2(vector.X, this.m_nameLabel.Size.Y);
                Vector2 vector2 = this.ComputeControlPositionFromTopLeft(new Vector2(0f, 0.03f));
                this.RefreshInventoryGridSizes();
                for (int i = 0; i < this.m_inventoryGrids.Count; i++)
                {
                    MyGuiControlLabel label = this.m_massLabels[i];
                    MyGuiControlLabel label2 = this.m_volumeLabels[i];
                    MyGuiControlGrid grid = this.m_inventoryGrids[i];
                    label.Position = vector2 + new Vector2(0.005f, -0.005f);
                    label2.Position = new Vector2(-0.04f, label.Position.Y);
                    label.Size = new Vector2(label2.Position.X - label.Position.X, label.Size.Y);
                    label2.Size = new Vector2(vector.X - label.Size.X, label2.Size.Y);
                    float* singlePtr1 = (float*) ref vector2.Y;
                    singlePtr1[0] += label.Size.Y + (m_internalPadding.Y * 0.5f);
                    grid.Position = vector2;
                    float* singlePtr2 = (float*) ref vector2.Y;
                    singlePtr2[0] += grid.Size.Y + m_internalPadding.Y;
                }
            }
        }

        private void RefreshInventoryContents()
        {
            if (this.m_inventoryOwner != null)
            {
                for (int i = 0; i < this.m_inventoryOwner.InventoryCount; i++)
                {
                    MyInventory inventory = this.m_inventoryOwner.GetInventory(i);
                    if (inventory != null)
                    {
                        MyGuiControlGrid grid = this.m_inventoryGrids[i];
                        MyGuiControlLabel label = this.m_volumeLabels[i];
                        int? selectedIndex = grid.SelectedIndex;
                        grid.Clear();
                        object[] args = new object[] { ((double) inventory.CurrentMass).ToString("N", CultureInfo.InvariantCulture) };
                        this.m_massLabels[i].UpdateFormatParams(args);
                        string str = ((double) MyFixedPoint.MultiplySafe(inventory.CurrentVolume, 0x3e8)).ToString("N", CultureInfo.InvariantCulture);
                        if (inventory.IsConstrained)
                        {
                            str = str + " / " + ((double) MyFixedPoint.MultiplySafe(inventory.MaxVolume, 0x3e8)).ToString("N", CultureInfo.InvariantCulture);
                        }
                        object[] objArray2 = new object[] { str };
                        label.UpdateFormatParams(objArray2);
                        if (inventory.Constraint != null)
                        {
                            grid.EmptyItemIcon = inventory.Constraint.Icon;
                            grid.SetEmptyItemToolTip(inventory.Constraint.Description);
                        }
                        else
                        {
                            grid.EmptyItemIcon = null;
                            grid.SetEmptyItemToolTip(null);
                        }
                        foreach (MyPhysicalInventoryItem item in inventory.GetItems())
                        {
                            grid.Add(CreateInventoryGridItem(item), 0);
                        }
                        if (selectedIndex == null)
                        {
                            grid.SelectedIndex = null;
                        }
                        else if (grid.IsValidIndex(selectedIndex.Value))
                        {
                            grid.SelectedIndex = selectedIndex;
                        }
                        else
                        {
                            grid.SelectLastItem();
                        }
                    }
                }
                this.RefreshInventoryGridSizes();
                base.Size = this.ComputeControlSize();
                this.RefreshInternals();
            }
        }

        private void RefreshInventoryGridSizes()
        {
            foreach (MyGuiControlGrid grid in this.m_inventoryGrids)
            {
                int count = ((MyInventory) grid.UserData).GetItems().Count;
                grid.ColumnsCount = Math.Max(1, (int) ((base.Size.X - (m_internalPadding.X * 2f)) / (grid.ItemSize.X * 1.01f)));
                grid.RowsCount = Math.Max(1, (int) Math.Ceiling((double) (((float) (count + 1)) / ((float) grid.ColumnsCount))));
                grid.TrimEmptyItems();
            }
        }

        public void RefreshOwnerInventory()
        {
            for (int i = 0; i < this.m_inventoryOwner.InventoryCount; i++)
            {
                MyInventory inventory = this.m_inventoryOwner.GetInventory(i);
                inventory.UserData = this;
                inventory.ContentsChanged += new Action<MyInventoryBase>(this.inventory_OnContentsChanged);
            }
        }

        public void RemoveInventoryEvents()
        {
            for (int i = 0; i < this.m_inventoryOwner.InventoryCount; i++)
            {
                this.m_inventoryOwner.GetInventory(i).ContentsChanged -= new Action<MyInventoryBase>(this.inventory_OnContentsChanged);
            }
        }

        private void ReplaceCurrentInventoryOwner(MyEntity owner)
        {
            this.DetachOwner();
            this.AttachOwner(owner);
        }

        public override void Update()
        {
            this.m_nameLabel.Text = this.m_inventoryOwner.DisplayNameText;
            this.m_nameLabel.Size = new Vector2(base.Size.X - (m_internalPadding.X * 2f), this.m_nameLabel.Size.Y);
            base.Update();
        }

        public MyEntity InventoryOwner
        {
            get => 
                this.m_inventoryOwner;
            set
            {
                if (!ReferenceEquals(this.m_inventoryOwner, value))
                {
                    this.ReplaceCurrentInventoryOwner(value);
                }
            }
        }

        public List<MyGuiControlGrid> ContentGrids =>
            this.m_inventoryGrids;
    }
}

