namespace Sandbox.Game.GameSystems
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game.Components;
    using VRage.Game.ModAPI;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MyDamageSystem : MySessionComponentBase, IMyDamageSystem
    {
        private List<Tuple<int, Action<object, MyDamageInformation>>> m_destroyHandlers = new List<Tuple<int, Action<object, MyDamageInformation>>>();
        private List<Tuple<int, BeforeDamageApplied>> m_beforeDamageHandlers = new List<Tuple<int, BeforeDamageApplied>>();
        private List<Tuple<int, Action<object, MyDamageInformation>>> m_afterDamageHandlers = new List<Tuple<int, Action<object, MyDamageInformation>>>();

        public override void LoadData()
        {
            Static = this;
            base.LoadData();
        }

        public void RaiseAfterDamageApplied(object target, MyDamageInformation info)
        {
            using (List<Tuple<int, Action<object, MyDamageInformation>>>.Enumerator enumerator = this.m_afterDamageHandlers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Item2(target, info);
                }
            }
        }

        public void RaiseBeforeDamageApplied(object target, ref MyDamageInformation info)
        {
            if (this.m_beforeDamageHandlers.Count > 0)
            {
                this.RaiseBeforeDamageAppliedIntenal(target, ref info);
            }
        }

        private void RaiseBeforeDamageAppliedIntenal(object target, ref MyDamageInformation info)
        {
            using (List<Tuple<int, BeforeDamageApplied>>.Enumerator enumerator = this.m_beforeDamageHandlers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Item2(target, ref info);
                }
            }
        }

        public void RaiseDestroyed(object target, MyDamageInformation info)
        {
            using (List<Tuple<int, Action<object, MyDamageInformation>>>.Enumerator enumerator = this.m_destroyHandlers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Item2(target, info);
                }
            }
        }

        public void RegisterAfterDamageHandler(int priority, Action<object, MyDamageInformation> handler)
        {
            Tuple<int, Action<object, MyDamageInformation>> item = new Tuple<int, Action<object, MyDamageInformation>>(priority, handler);
            this.m_afterDamageHandlers.Add(item);
            this.m_afterDamageHandlers.Sort((x, y) => x.Item1 - y.Item1);
        }

        public void RegisterBeforeDamageHandler(int priority, BeforeDamageApplied handler)
        {
            Tuple<int, BeforeDamageApplied> item = new Tuple<int, BeforeDamageApplied>(priority, handler);
            this.m_beforeDamageHandlers.Add(item);
            this.m_beforeDamageHandlers.Sort((x, y) => x.Item1 - y.Item1);
        }

        public void RegisterDestroyHandler(int priority, Action<object, MyDamageInformation> handler)
        {
            Tuple<int, Action<object, MyDamageInformation>> item = new Tuple<int, Action<object, MyDamageInformation>>(priority, handler);
            this.m_destroyHandlers.Add(item);
            this.m_destroyHandlers.Sort((x, y) => x.Item1 - y.Item1);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            this.m_destroyHandlers.Clear();
            this.m_beforeDamageHandlers.Clear();
            this.m_afterDamageHandlers.Clear();
        }

        public static MyDamageSystem Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<Static>k__BackingField = value);
        }

        public bool HasAnyBeforeHandler =>
            (this.m_beforeDamageHandlers.Count > 0);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyDamageSystem.<>c <>9 = new MyDamageSystem.<>c();
            public static Comparison<Tuple<int, Action<object, MyDamageInformation>>> <>9__15_0;
            public static Comparison<Tuple<int, BeforeDamageApplied>> <>9__16_0;
            public static Comparison<Tuple<int, Action<object, MyDamageInformation>>> <>9__17_0;

            internal int <RegisterAfterDamageHandler>b__17_0(Tuple<int, Action<object, MyDamageInformation>> x, Tuple<int, Action<object, MyDamageInformation>> y) => 
                (x.Item1 - y.Item1);

            internal int <RegisterBeforeDamageHandler>b__16_0(Tuple<int, BeforeDamageApplied> x, Tuple<int, BeforeDamageApplied> y) => 
                (x.Item1 - y.Item1);

            internal int <RegisterDestroyHandler>b__15_0(Tuple<int, Action<object, MyDamageInformation>> x, Tuple<int, Action<object, MyDamageInformation>> y) => 
                (x.Item1 - y.Item1);
        }
    }
}

