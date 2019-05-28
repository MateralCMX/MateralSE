namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class MyGuiControlElementGroup : IEnumerable<MyGuiControlBase>, IEnumerable
    {
        public const int INVALID_INDEX = -1;
        private List<MyGuiControlBase> m_controlElements = new List<MyGuiControlBase>();
        private int? m_selectedIndex = null;
        [CompilerGenerated]
        private Action<MyGuiControlElementGroup> HighlightChanged;

        public event Action<MyGuiControlElementGroup> HighlightChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlElementGroup> highlightChanged = this.HighlightChanged;
                while (true)
                {
                    Action<MyGuiControlElementGroup> a = highlightChanged;
                    Action<MyGuiControlElementGroup> action3 = (Action<MyGuiControlElementGroup>) Delegate.Combine(a, value);
                    highlightChanged = Interlocked.CompareExchange<Action<MyGuiControlElementGroup>>(ref this.HighlightChanged, action3, a);
                    if (ReferenceEquals(highlightChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlElementGroup> highlightChanged = this.HighlightChanged;
                while (true)
                {
                    Action<MyGuiControlElementGroup> source = highlightChanged;
                    Action<MyGuiControlElementGroup> action3 = (Action<MyGuiControlElementGroup>) Delegate.Remove(source, value);
                    highlightChanged = Interlocked.CompareExchange<Action<MyGuiControlElementGroup>>(ref this.HighlightChanged, action3, source);
                    if (ReferenceEquals(highlightChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public void Add(MyGuiControlBase controlElement)
        {
            if (controlElement.CanHaveFocus)
            {
                this.m_controlElements.Add(controlElement);
                controlElement.HightlightChanged += new Action<MyGuiControlBase>(this.OnControlElementSelected);
            }
        }

        public void Clear()
        {
            using (List<MyGuiControlBase>.Enumerator enumerator = this.m_controlElements.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.HightlightChanged -= new Action<MyGuiControlBase>(this.OnControlElementSelected);
                }
            }
            this.m_controlElements.Clear();
            this.m_selectedIndex = null;
        }

        public IEnumerator<MyGuiControlBase> GetEnumerator() => 
            this.m_controlElements.GetEnumerator();

        private void OnControlElementSelected(MyGuiControlBase sender)
        {
            if (sender.HasHighlight)
            {
                this.SelectedIndex = new int?(this.m_controlElements.IndexOf(sender));
            }
        }

        public void Remove(MyGuiControlBase controlElement)
        {
            controlElement.HightlightChanged -= new Action<MyGuiControlBase>(this.OnControlElementSelected);
            this.m_controlElements.Remove(controlElement);
        }

        public void SelectByIndex(int index)
        {
            if (this.SelectedIndex != null)
            {
                this.m_controlElements[this.SelectedIndex.Value].HasHighlight = false;
            }
            this.SelectedIndex = new int?(index);
            this.m_controlElements[index].HasHighlight = true;
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        private MyGuiControlBase TryGetElement(int elementIdx)
        {
            if ((elementIdx >= this.m_controlElements.Count) || (elementIdx < 0))
            {
                return null;
            }
            return this.m_controlElements[elementIdx];
        }

        public MyGuiControlBase SelectedElement
        {
            get
            {
                int? selectedIndex = this.SelectedIndex;
                return this.TryGetElement((selectedIndex != null) ? selectedIndex.GetValueOrDefault() : -1);
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
                        this.m_controlElements[this.m_selectedIndex.Value].HasHighlight = false;
                    }
                    this.m_selectedIndex = value;
                    if (this.m_selectedIndex != null)
                    {
                        this.m_controlElements[this.m_selectedIndex.Value].HasHighlight = true;
                    }
                    if (this.HighlightChanged != null)
                    {
                        this.HighlightChanged(this);
                    }
                }
            }
        }
    }
}

