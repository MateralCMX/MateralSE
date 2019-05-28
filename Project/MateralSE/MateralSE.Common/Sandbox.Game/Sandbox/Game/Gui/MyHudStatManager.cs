namespace Sandbox.Game.GUI
{
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using VRage.ModAPI;
    using VRage.Utils;

    public class MyHudStatManager
    {
        private readonly Dictionary<MyStringHash, IMyHudStat> m_stats = new Dictionary<MyStringHash, IMyHudStat>();
        private readonly Dictionary<Type, IMyHudStat> m_statsByType = new Dictionary<Type, IMyHudStat>();

        public MyHudStatManager()
        {
            this.RegisterFromAssembly(base.GetType().Assembly);
            this.RegisterModStats();
        }

        public T GetStat<T>() where T: IMyHudStat
        {
            IMyHudStat stat = null;
            this.m_statsByType.TryGetValue(typeof(T), out stat);
            return (T) stat;
        }

        public IMyHudStat GetStat(MyStringHash id)
        {
            IMyHudStat stat = null;
            this.m_stats.TryGetValue(id, out stat);
            return stat;
        }

        public bool Register(IMyHudStat stat)
        {
            Type key = stat.GetType();
            if (this.m_stats.ContainsKey(stat.Id))
            {
                return false;
            }
            if (this.m_statsByType.ContainsKey(key))
            {
                return false;
            }
            this.m_stats[stat.Id] = stat;
            this.m_statsByType[key] = stat;
            return true;
        }

        public void RegisterFromAssembly(Assembly assembly)
        {
            if (assembly != null)
            {
                Type derivedType = typeof(IMyHudStat);
                (from t in assembly.GetTypes()
                    where (t != derivedType) && (derivedType.IsAssignableFrom(t) && !t.IsAbstract)
                    select t).ForEach<Type>(delegate (Type stat) {
                    IMyHudStat stat2 = (IMyHudStat) Activator.CreateInstance(stat);
                    this.m_stats[stat2.Id] = stat2;
                    this.m_statsByType[stat] = stat2;
                });
            }
        }

        private void RegisterModStats()
        {
            if ((MyScriptManager.Static != null) && (MyScriptManager.Static.Scripts != null))
            {
                MyScriptManager.Static.Scripts.ForEach<KeyValuePair<MyStringId, Assembly>>(pair => this.RegisterFromAssembly(pair.Value));
            }
        }

        public void Update()
        {
            if (MySession.Static != null)
            {
                using (Dictionary<MyStringHash, IMyHudStat>.ValueCollection.Enumerator enumerator = this.m_stats.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Update();
                    }
                }
            }
        }
    }
}

