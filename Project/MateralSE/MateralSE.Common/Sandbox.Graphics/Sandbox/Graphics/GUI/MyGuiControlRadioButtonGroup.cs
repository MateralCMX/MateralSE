namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class MyGuiControlRadioButtonGroup : IEnumerable<MyGuiControlRadioButton>, IEnumerable
    {
        public const int INVALID_INDEX = -1;
        private List<MyGuiControlRadioButton> m_radioButtons = new List<MyGuiControlRadioButton>();
        private int? m_selectedIndex = null;
        [CompilerGenerated]
        private Action<MyGuiControlRadioButtonGroup> SelectedChanged;
        [CompilerGenerated]
        private Action<MyGuiControlRadioButton> MouseDoubleClick;

        public event Action<MyGuiControlRadioButton> MouseDoubleClick
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlRadioButton> mouseDoubleClick = this.MouseDoubleClick;
                while (true)
                {
                    Action<MyGuiControlRadioButton> a = mouseDoubleClick;
                    Action<MyGuiControlRadioButton> action3 = (Action<MyGuiControlRadioButton>) Delegate.Combine(a, value);
                    mouseDoubleClick = Interlocked.CompareExchange<Action<MyGuiControlRadioButton>>(ref this.MouseDoubleClick, action3, a);
                    if (ReferenceEquals(mouseDoubleClick, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlRadioButton> mouseDoubleClick = this.MouseDoubleClick;
                while (true)
                {
                    Action<MyGuiControlRadioButton> source = mouseDoubleClick;
                    Action<MyGuiControlRadioButton> action3 = (Action<MyGuiControlRadioButton>) Delegate.Remove(source, value);
                    mouseDoubleClick = Interlocked.CompareExchange<Action<MyGuiControlRadioButton>>(ref this.MouseDoubleClick, action3, source);
                    if (ReferenceEquals(mouseDoubleClick, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlRadioButtonGroup> SelectedChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlRadioButtonGroup> selectedChanged = this.SelectedChanged;
                while (true)
                {
                    Action<MyGuiControlRadioButtonGroup> a = selectedChanged;
                    Action<MyGuiControlRadioButtonGroup> action3 = (Action<MyGuiControlRadioButtonGroup>) Delegate.Combine(a, value);
                    selectedChanged = Interlocked.CompareExchange<Action<MyGuiControlRadioButtonGroup>>(ref this.SelectedChanged, action3, a);
                    if (ReferenceEquals(selectedChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlRadioButtonGroup> selectedChanged = this.SelectedChanged;
                while (true)
                {
                    Action<MyGuiControlRadioButtonGroup> source = selectedChanged;
                    Action<MyGuiControlRadioButtonGroup> action3 = (Action<MyGuiControlRadioButtonGroup>) Delegate.Remove(source, value);
                    selectedChanged = Interlocked.CompareExchange<Action<MyGuiControlRadioButtonGroup>>(ref this.SelectedChanged, action3, source);
                    if (ReferenceEquals(selectedChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public void Add(MyGuiControlRadioButton radioButton)
        {
            this.m_radioButtons.Add(radioButton);
            radioButton.SelectedChanged += new Action<MyGuiControlRadioButton>(this.OnRadioButtonSelected);
            radioButton.MouseDoubleClick += new Action<MyGuiControlRadioButton>(this.OnRadioButtonMouseDoubleClick);
        }

        public void Clear()
        {
            foreach (MyGuiControlRadioButton local1 in this.m_radioButtons)
            {
                local1.SelectedChanged -= new Action<MyGuiControlRadioButton>(this.OnRadioButtonSelected);
                local1.MouseDoubleClick -= new Action<MyGuiControlRadioButton>(this.OnRadioButtonMouseDoubleClick);
            }
            this.m_radioButtons.Clear();
            this.m_selectedIndex = null;
        }

        public IEnumerator<MyGuiControlRadioButton> GetEnumerator() => 
            this.m_radioButtons.GetEnumerator();

        private void OnRadioButtonMouseDoubleClick(MyGuiControlRadioButton button)
        {
            this.MouseDoubleClick.InvokeIfNotNull<MyGuiControlRadioButton>(button);
        }

        private void OnRadioButtonSelected(MyGuiControlRadioButton sender)
        {
            this.SelectedIndex = new int?(this.m_radioButtons.IndexOf(sender));
        }

        public void Remove(MyGuiControlRadioButton radioButton)
        {
            radioButton.SelectedChanged -= new Action<MyGuiControlRadioButton>(this.OnRadioButtonSelected);
            radioButton.MouseDoubleClick -= new Action<MyGuiControlRadioButton>(this.OnRadioButtonMouseDoubleClick);
            this.m_radioButtons.Remove(radioButton);
        }

        public void SelectByIndex(int index)
        {
            if (this.SelectedIndex != null)
            {
                this.m_radioButtons[this.SelectedIndex.Value].Selected = false;
            }
            if (index < this.m_radioButtons.Count)
            {
                this.SelectedIndex = new int?(index);
                this.m_radioButtons[index].Selected = true;
            }
        }

        public void SelectByKey(int key)
        {
            for (int i = 0; i < this.m_radioButtons.Count; i++)
            {
                MyGuiControlRadioButton button = this.m_radioButtons[i];
                if (button.Key != key)
                {
                    button.Selected = false;
                }
                else
                {
                    this.SelectedIndex = new int?(i);
                    button.Selected = true;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        private MyGuiControlRadioButton TryGetButton(int buttonIdx)
        {
            if ((buttonIdx >= this.m_radioButtons.Count) || (buttonIdx < 0))
            {
                return null;
            }
            return this.m_radioButtons[buttonIdx];
        }

        public MyGuiControlRadioButton SelectedButton
        {
            get
            {
                int? selectedIndex = this.SelectedIndex;
                return this.TryGetButton((selectedIndex != null) ? selectedIndex.GetValueOrDefault() : -1);
            }
        }

        public int? SelectedIndex
        {
            get => 
                this.m_selectedIndex;
            set
            {
                int? selectedIndex = this.m_selectedIndex;
                int? nullable2 = value;
                if (!((selectedIndex.GetValueOrDefault() == nullable2.GetValueOrDefault()) & ((selectedIndex != null) == (nullable2 != null))))
                {
                    if (this.m_selectedIndex != null)
                    {
                        this.m_radioButtons[this.m_selectedIndex.Value].Selected = false;
                    }
                    this.m_selectedIndex = value;
                    if (this.m_selectedIndex != null)
                    {
                        this.m_radioButtons[this.m_selectedIndex.Value].Selected = true;
                    }
                    if (this.SelectedChanged != null)
                    {
                        this.SelectedChanged(this);
                    }
                }
            }
        }

        public int Count =>
            this.m_radioButtons.Count;
    }
}

