namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI.Interfaces;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Library.Collections;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    internal class MyTerminalControlCombobox<TBlock> : MyTerminalValueControl<TBlock, long>, IMyTerminalControlCombobox, IMyTerminalControl, IMyTerminalValueControl<long>, ITerminalProperty, IMyTerminalControlTitleTooltip where TBlock: MyTerminalBlock
    {
        private static List<MyTerminalControlComboBoxItem> m_handlerItems;
        public MyStringId Title;
        public MyStringId Tooltip;
        private MyGuiControlCombobox m_comboBox;
        public ComboBoxContentDelegate<TBlock> ComboBoxContentWithBlock;
        public Action<List<MyTerminalControlComboBoxItem>> ComboBoxContent;

        static MyTerminalControlCombobox()
        {
            MyTerminalControlCombobox<TBlock>.m_handlerItems = new List<MyTerminalControlComboBoxItem>();
        }

        public MyTerminalControlCombobox(string id, MyStringId title, MyStringId tooltip) : base(id)
        {
            this.Title = title;
            this.Tooltip = tooltip;
            this.SetSerializerDefault();
        }

        protected override MyGuiControlBase CreateGui()
        {
            Vector2? position = null;
            Vector4? backgroundColor = null;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_comboBox = new MyGuiControlCombobox(position, new Vector2(0.23f, 0.04f), backgroundColor, position, 10, position, false, MyTexts.GetString(this.Tooltip), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            this.m_comboBox.VisualStyle = MyGuiControlComboboxStyleEnum.Terminal;
            this.m_comboBox.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnItemSelected);
            return new MyGuiControlBlockProperty(MyTexts.GetString(this.Title), MyTexts.GetString(this.Tooltip), this.m_comboBox, MyGuiControlBlockPropertyLayoutEnum.Vertical, true);
        }

        public override long GetDefaultValue(TBlock block) => 
            this.GetMinimum(block);

        public override long GetMaximum(TBlock block)
        {
            long key = 0L;
            if (this.ComboBoxContent != null)
            {
                MyTerminalControlCombobox<TBlock>.m_handlerItems.Clear();
                this.ComboBoxContent(MyTerminalControlCombobox<TBlock>.m_handlerItems);
                if (MyTerminalControlCombobox<TBlock>.m_handlerItems.Count > 0)
                {
                    key = MyTerminalControlCombobox<TBlock>.m_handlerItems[MyTerminalControlCombobox<TBlock>.m_handlerItems.Count - 1].Key;
                }
            }
            return key;
        }

        public override long GetMinimum(TBlock block)
        {
            long key = 0L;
            if (this.ComboBoxContent != null)
            {
                MyTerminalControlCombobox<TBlock>.m_handlerItems.Clear();
                this.ComboBoxContent(MyTerminalControlCombobox<TBlock>.m_handlerItems);
                if (MyTerminalControlCombobox<TBlock>.m_handlerItems.Count > 0)
                {
                    key = MyTerminalControlCombobox<TBlock>.m_handlerItems[0].Key;
                }
            }
            return key;
        }

        private void OnItemSelected()
        {
            if (this.m_comboBox.GetItemsCount() > 0)
            {
                long selectedKey = this.m_comboBox.GetSelectedKey();
                foreach (TBlock local in base.TargetBlocks)
                {
                    this.SetValue(local, selectedKey);
                }
            }
        }

        protected override void OnUpdateVisual()
        {
            base.OnUpdateVisual();
            TBlock firstBlock = base.FirstBlock;
            if (firstBlock != null)
            {
                int? nullable;
                MyStringId? nullable2;
                this.m_comboBox.ClearItems();
                MyTerminalControlCombobox<TBlock>.m_handlerItems.Clear();
                if (this.ComboBoxContent != null)
                {
                    this.ComboBoxContent(MyTerminalControlCombobox<TBlock>.m_handlerItems);
                    foreach (MyTerminalControlComboBoxItem item in MyTerminalControlCombobox<TBlock>.m_handlerItems)
                    {
                        nullable = null;
                        nullable2 = null;
                        this.m_comboBox.AddItem(item.Key, item.Value, nullable, nullable2);
                    }
                    long key = this.GetValue(firstBlock);
                    if (this.m_comboBox.GetSelectedKey() != key)
                    {
                        this.m_comboBox.SelectItemByKey(key, true);
                    }
                }
                if (this.ComboBoxContentWithBlock != null)
                {
                    this.ComboBoxContentWithBlock(firstBlock, MyTerminalControlCombobox<TBlock>.m_handlerItems);
                    foreach (MyTerminalControlComboBoxItem item2 in MyTerminalControlCombobox<TBlock>.m_handlerItems)
                    {
                        nullable = null;
                        nullable2 = null;
                        this.m_comboBox.AddItem(item2.Key, item2.Value, nullable, nullable2);
                    }
                    long key = this.GetValue(firstBlock);
                    if (this.m_comboBox.GetSelectedKey() != key)
                    {
                        this.m_comboBox.SelectItemByKey(key, true);
                    }
                }
            }
        }

        public void SetSerializerBit()
        {
            this.Serializer = delegate (BitStream stream, ref long value) {
                if (stream.Reading)
                {
                    value = stream.ReadBool() ? ((long) 1) : ((long) 0);
                }
                else
                {
                    stream.WriteBool(value != 0L);
                }
            };
        }

        public void SetSerializerDefault()
        {
            this.Serializer = (stream, value) => stream.Serialize(ref value, 0x40);
        }

        public void SetSerializerRange(int minInclusive, int maxInclusive)
        {
            int bitCount = MathHelper.Log2(MathHelper.GetNearestBiggerPowerOfTwo((uint) ((maxInclusive - minInclusive) + 1L)));
            base.Serializer = delegate (BitStream stream, ref long value) {
                if (stream.Reading)
                {
                    value = (long) (stream.ReadUInt64(0x40) + minInclusive);
                }
                else
                {
                    stream.WriteUInt64((ulong) (value - minInclusive), bitCount);
                }
            };
        }

        public void SetSerializerVariant(bool usesNegativeValues = false)
        {
            if (usesNegativeValues)
            {
                this.Serializer = (stream, value) => stream.SerializeVariant(ref value);
            }
            else
            {
                this.Serializer = delegate (BitStream stream, ref long value) {
                    if (stream.Reading)
                    {
                        value = stream.ReadInt64(0x40);
                    }
                    else
                    {
                        stream.WriteInt64(value, 0x40);
                    }
                };
            }
        }

        public override void SetValue(TBlock block, long value)
        {
            if (base.Getter(block) != value)
            {
                base.SetValue(block, value);
            }
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

        Action<List<MyTerminalControlComboBoxItem>> IMyTerminalControlCombobox.ComboBoxContent
        {
            get
            {
                Action<List<MyTerminalControlComboBoxItem>> oldComboBoxContent = this.ComboBoxContent;
                return delegate (List<MyTerminalControlComboBoxItem> x) {
                    oldComboBoxContent(x);
                };
            }
            set => 
                (this.ComboBoxContent = value);
        }

        private Action<IMyTerminalBlock, List<MyTerminalControlComboBoxItem>> ComboBoxContentWithBlockAction
        {
            set => 
                (this.ComboBoxContentWithBlock = delegate (TBlock block, ICollection<MyTerminalControlComboBoxItem> comboBoxContent) {
                    List<MyTerminalControlComboBoxItem> list = new List<MyTerminalControlComboBoxItem>();
                    value(block, list);
                    foreach (MyTerminalControlComboBoxItem item in list)
                    {
                        MyTerminalControlComboBoxItem item2 = new MyTerminalControlComboBoxItem {
                            Key = item.Key,
                            Value = item.Value
                        };
                        comboBoxContent.Add(item2);
                    }
                });
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTerminalControlCombobox<TBlock>.<>c <>9;
            public static MyTerminalValueControl<TBlock, long>.SerializerDelegate <>9__8_0;
            public static MyTerminalValueControl<TBlock, long>.SerializerDelegate <>9__9_0;
            public static MyTerminalValueControl<TBlock, long>.SerializerDelegate <>9__11_0;
            public static MyTerminalValueControl<TBlock, long>.SerializerDelegate <>9__11_1;

            static <>c()
            {
                MyTerminalControlCombobox<TBlock>.<>c.<>9 = new MyTerminalControlCombobox<TBlock>.<>c();
            }

            internal void <SetSerializerBit>b__9_0(BitStream stream, ref long value)
            {
                if (stream.Reading)
                {
                    value = stream.ReadBool() ? ((long) 1) : ((long) 0);
                }
                else
                {
                    stream.WriteBool(value != 0L);
                }
            }

            internal void <SetSerializerDefault>b__8_0(BitStream stream, ref long value)
            {
                stream.Serialize(ref value, 0x40);
            }

            internal void <SetSerializerVariant>b__11_0(BitStream stream, ref long value)
            {
                stream.SerializeVariant(ref value);
            }

            internal void <SetSerializerVariant>b__11_1(BitStream stream, ref long value)
            {
                if (stream.Reading)
                {
                    value = stream.ReadInt64(0x40);
                }
                else
                {
                    stream.WriteInt64(value, 0x40);
                }
            }
        }

        public delegate void ComboBoxContentDelegate(TBlock block, ICollection<MyTerminalControlComboBoxItem> comboBoxContent);
    }
}

