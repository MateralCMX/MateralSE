namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game;
    using VRage.ObjectBuilders;

    public class MyGuiControls : MyGuiControlBase.Friend, IEnumerable<MyGuiControlBase>, IEnumerable
    {
        private IMyGuiControlsOwner m_owner;
        private ObservableCollection<MyGuiControlBase> m_controls;
        private Dictionary<string, MyGuiControlBase> m_controlsByName;
        private List<MyGuiControlBase> m_visibleControls;
        private bool m_refreshVisibleControls;
        [CompilerGenerated]
        private Action<MyGuiControls> CollectionChanged;
        [CompilerGenerated]
        private Action<MyGuiControls> CollectionMembersVisibleChanged;

        public event Action<MyGuiControls> CollectionChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControls> collectionChanged = this.CollectionChanged;
                while (true)
                {
                    Action<MyGuiControls> a = collectionChanged;
                    Action<MyGuiControls> action3 = (Action<MyGuiControls>) Delegate.Combine(a, value);
                    collectionChanged = Interlocked.CompareExchange<Action<MyGuiControls>>(ref this.CollectionChanged, action3, a);
                    if (ReferenceEquals(collectionChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControls> collectionChanged = this.CollectionChanged;
                while (true)
                {
                    Action<MyGuiControls> source = collectionChanged;
                    Action<MyGuiControls> action3 = (Action<MyGuiControls>) Delegate.Remove(source, value);
                    collectionChanged = Interlocked.CompareExchange<Action<MyGuiControls>>(ref this.CollectionChanged, action3, source);
                    if (ReferenceEquals(collectionChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControls> CollectionMembersVisibleChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControls> collectionMembersVisibleChanged = this.CollectionMembersVisibleChanged;
                while (true)
                {
                    Action<MyGuiControls> a = collectionMembersVisibleChanged;
                    Action<MyGuiControls> action3 = (Action<MyGuiControls>) Delegate.Combine(a, value);
                    collectionMembersVisibleChanged = Interlocked.CompareExchange<Action<MyGuiControls>>(ref this.CollectionMembersVisibleChanged, action3, a);
                    if (ReferenceEquals(collectionMembersVisibleChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControls> collectionMembersVisibleChanged = this.CollectionMembersVisibleChanged;
                while (true)
                {
                    Action<MyGuiControls> source = collectionMembersVisibleChanged;
                    Action<MyGuiControls> action3 = (Action<MyGuiControls>) Delegate.Remove(source, value);
                    collectionMembersVisibleChanged = Interlocked.CompareExchange<Action<MyGuiControls>>(ref this.CollectionMembersVisibleChanged, action3, source);
                    if (ReferenceEquals(collectionMembersVisibleChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiControls(IMyGuiControlsOwner owner)
        {
            this.m_owner = owner;
            this.m_controls = new ObservableCollection<MyGuiControlBase>();
            this.m_controlsByName = new Dictionary<string, MyGuiControlBase>();
            this.m_visibleControls = new List<MyGuiControlBase>();
            this.m_controls.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnPrivateCollectionChanged);
            this.m_refreshVisibleControls = true;
        }

        public void Add(MyGuiControlBase control)
        {
            SetOwner(control, this.m_owner);
            control.Name = this.ChangeToNonCollidingName(control.Name);
            this.m_controlsByName.Add(control.Name, control);
            if (control.Visible)
            {
                this.m_visibleControls.Add(control);
            }
            this.m_controls.Add(control);
            control.VisibleChanged += new VisibleChangedDelegate(this.control_VisibleChanged);
            control.NameChanged += new Action<MyGuiControlBase, MyGuiControlBase.NameChangedArgs>(this.control_NameChanged);
        }

        public void AddWeak(MyGuiControlBase control)
        {
            if (control.Visible)
            {
                this.m_visibleControls.Add(control);
            }
            this.m_controls.Add(control);
            control.VisibleChanged += new VisibleChangedDelegate(this.control_VisibleChanged);
            control.NameChanged += new Action<MyGuiControlBase, MyGuiControlBase.NameChangedArgs>(this.control_NameChanged);
        }

        private string ChangeToNonCollidingName(string originalName)
        {
            string key = originalName;
            for (int i = 1; this.m_controlsByName.ContainsKey(key); i++)
            {
                key = originalName + i;
            }
            return key;
        }

        public void Clear()
        {
            foreach (MyGuiControlBase local1 in this.m_controls)
            {
                local1.OnRemoving();
                local1.VisibleChanged -= new VisibleChangedDelegate(this.control_VisibleChanged);
                local1.NameChanged -= new Action<MyGuiControlBase, MyGuiControlBase.NameChangedArgs>(this.control_NameChanged);
            }
            this.m_controls.Clear();
            this.m_controlsByName.Clear();
            this.m_visibleControls.Clear();
        }

        public void ClearWeaks()
        {
            this.m_controls.Clear();
            this.m_controlsByName.Clear();
            this.m_visibleControls.Clear();
        }

        public bool Contains(MyGuiControlBase control) => 
            this.m_controls.Contains(control);

        private void control_NameChanged(MyGuiControlBase control, MyGuiControlBase.NameChangedArgs args)
        {
            this.m_controlsByName.Remove(args.OldName);
            control.NameChanged -= new Action<MyGuiControlBase, MyGuiControlBase.NameChangedArgs>(this.control_NameChanged);
            control.Name = this.ChangeToNonCollidingName(control.Name);
            control.NameChanged += new Action<MyGuiControlBase, MyGuiControlBase.NameChangedArgs>(this.control_NameChanged);
            this.m_controlsByName.Add(control.Name, control);
        }

        private void control_VisibleChanged(object control, bool isVisible)
        {
            this.m_refreshVisibleControls = true;
            if (this.CollectionMembersVisibleChanged != null)
            {
                this.CollectionMembersVisibleChanged(this);
            }
        }

        public int FindIndex(Predicate<MyGuiControlBase> match) => 
            this.m_controls.FindIndex(match);

        public MyGuiControlBase GetControlByName(string name)
        {
            MyGuiControlBase base2 = null;
            this.m_controlsByName.TryGetValue(name, out base2);
            return base2;
        }

        public ObservableCollection<MyGuiControlBase>.Enumerator GetEnumerator() => 
            this.m_controls.GetEnumerator();

        public MyObjectBuilder_GuiControls GetObjectBuilder()
        {
            MyObjectBuilder_GuiControls controls = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_GuiControls>();
            controls.Controls = new List<MyObjectBuilder_GuiControlBase>();
            foreach (KeyValuePair<string, MyGuiControlBase> pair in this.m_controlsByName)
            {
                MyObjectBuilder_GuiControlBase objectBuilder = pair.Value.GetObjectBuilder();
                controls.Controls.Add(objectBuilder);
            }
            return controls;
        }

        public List<MyGuiControlBase> GetVisibleControls()
        {
            this.RefreshVisibleControls();
            return this.m_visibleControls;
        }

        public int IndexOf(MyGuiControlBase item) => 
            this.m_controls.IndexOf(item);

        public void Init(MyObjectBuilder_GuiControls objectBuilder)
        {
            this.Clear();
            if (objectBuilder.Controls != null)
            {
                foreach (MyObjectBuilder_GuiControlBase base2 in objectBuilder.Controls)
                {
                    MyGuiControlBase control = MyGuiControlsFactory.CreateGuiControl(base2);
                    if (control != null)
                    {
                        control.Init(base2);
                        this.Add(control);
                    }
                }
            }
        }

        private void OnPrivateCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this);
            }
        }

        private void RefreshVisibleControls()
        {
            if (this.m_refreshVisibleControls)
            {
                this.m_visibleControls.Clear();
                foreach (MyGuiControlBase base2 in this.m_controls)
                {
                    if (base2.Visible)
                    {
                        this.m_visibleControls.Add(base2);
                    }
                }
                this.m_refreshVisibleControls = false;
            }
        }

        public bool Remove(MyGuiControlBase control)
        {
            this.m_controlsByName.Remove(control.Name);
            bool flag1 = this.m_controls.Remove(control);
            if (flag1)
            {
                this.m_visibleControls.Remove(control);
                control.OnRemoving();
                control.VisibleChanged -= new VisibleChangedDelegate(this.control_VisibleChanged);
                control.NameChanged -= new Action<MyGuiControlBase, MyGuiControlBase.NameChangedArgs>(this.control_NameChanged);
            }
            return flag1;
        }

        public bool RemoveControlByName(string name)
        {
            MyGuiControlBase controlByName = this.GetControlByName(name);
            return ((controlByName != null) ? this.Remove(controlByName) : false);
        }

        IEnumerator<MyGuiControlBase> IEnumerable<MyGuiControlBase>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public int Count =>
            this.m_controls.Count;

        public MyGuiControlBase this[int index]
        {
            get => 
                this.m_controls[index];
            set
            {
                MyGuiControlBase item = this.m_controls[index];
                if (item != null)
                {
                    item.VisibleChanged -= new VisibleChangedDelegate(this.control_VisibleChanged);
                    this.m_visibleControls.Remove(item);
                }
                if (value != null)
                {
                    MyGuiControlBase base3 = value;
                    base3.VisibleChanged -= new VisibleChangedDelegate(this.control_VisibleChanged);
                    base3.VisibleChanged += new VisibleChangedDelegate(this.control_VisibleChanged);
                    this.m_controls[index] = base3;
                    this.m_refreshVisibleControls = true;
                }
            }
        }
    }
}

