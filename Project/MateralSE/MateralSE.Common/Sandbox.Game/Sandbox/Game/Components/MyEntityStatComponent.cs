namespace Sandbox.Game.Components
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ObjectBuilders;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Network;
    using VRage.Utils;

    [StaticEventOwner, MyComponentType(typeof(MyEntityStatComponent)), MyComponentBuilder(typeof(MyObjectBuilder_EntityStatComponent), true)]
    public class MyEntityStatComponent : MyEntityComponentBase
    {
        private Dictionary<MyStringHash, MyEntityStat> m_stats = new Dictionary<MyStringHash, MyEntityStat>(MyStringHash.Comparer);
        protected List<MyStatLogic> m_scripts = new List<MyStatLogic>();
        private static List<MyEntityStat> m_statSyncList = new List<MyEntityStat>();
        private int m_updateCounter;
        private bool m_statActionsRequested;

        private MyEntityStat AddStat(MyStringHash statId, MyObjectBuilder_EntityStat objectBuilder, bool forceNewValues = false)
        {
            MyEntityStat stat = null;
            if (this.m_stats.TryGetValue(statId, out stat))
            {
                if (!forceNewValues)
                {
                    objectBuilder.Value = stat.CurrentRatio;
                }
                stat.ClearEffects();
                this.m_stats.Remove(statId);
            }
            stat = new MyEntityStat();
            stat.Init(objectBuilder);
            this.m_stats.Add(statId, stat);
            return stat;
        }

        public void ApplyModifier(string modifierId)
        {
            using (List<MyStatLogic>.Enumerator enumerator = this.m_scripts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ApplyModifier(modifierId);
                }
            }
        }

        public bool CanDoAction(string actionId, out MyTuple<ushort, MyStringHash> message, bool continuous = false)
        {
            message = new MyTuple<ushort, MyStringHash>(0, MyStringHash.NullOrEmpty);
            if ((this.m_scripts == null) || (this.m_scripts.Count == 0))
            {
                return true;
            }
            bool flag = true;
            foreach (MyStatLogic logic in this.m_scripts)
            {
                MyTuple<ushort, MyStringHash> tuple;
                flag &= !logic.CanDoAction(actionId, continuous, out tuple);
                if (tuple.Item1 != 0)
                {
                    message = tuple;
                }
            }
            return !flag;
        }

        public override void Deserialize(MyObjectBuilder_ComponentBase objectBuilder)
        {
            MyObjectBuilder_CharacterStatComponent component = objectBuilder as MyObjectBuilder_CharacterStatComponent;
            using (List<MyStatLogic>.Enumerator enumerator = this.m_scripts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Close();
                }
            }
            this.m_scripts.Clear();
            if (component != null)
            {
                if (component.Stats != null)
                {
                    foreach (MyObjectBuilder_EntityStat stat in component.Stats)
                    {
                        MyEntityStatDefinition definition = null;
                        if ((MyDefinitionManager.Static.TryGetDefinition<MyEntityStatDefinition>(new MyDefinitionId(stat.TypeId, stat.SubtypeId), out definition) && definition.Enabled) && ((definition.EnabledInCreative && MySession.Static.CreativeMode) || (definition.AvailableInSurvival && MySession.Static.SurvivalMode)))
                        {
                            this.AddStat(MyStringHash.GetOrCompute(definition.Name), stat, true);
                        }
                    }
                }
                if ((component.ScriptNames != null) && Sync.IsServer)
                {
                    component.ScriptNames = component.ScriptNames.Distinct<string>().ToArray<string>();
                    foreach (string str in component.ScriptNames)
                    {
                        this.InitScript(str);
                    }
                }
            }
            base.Deserialize(objectBuilder);
        }

        public bool DoAction(string actionId)
        {
            bool flag = false;
            using (List<MyStatLogic>.Enumerator enumerator = this.m_scripts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!enumerator.Current.DoAction(actionId))
                    {
                        continue;
                    }
                    flag = true;
                }
            }
            return flag;
        }

        public float GetEfficiencyModifier(string modifierId)
        {
            float num = 1f;
            foreach (MyStatLogic logic in this.m_scripts)
            {
                num *= logic.GetEfficiencyModifier(modifierId);
            }
            return num;
        }

        public override void Init(MyComponentDefinitionBase definition)
        {
            base.Init(definition);
            MyEntityStatComponentDefinition definition2 = definition as MyEntityStatComponentDefinition;
            if (((definition2 == null) || (!definition2.Enabled || (MySession.Static == null))) || (!definition2.AvailableInSurvival && MySession.Static.SurvivalMode))
            {
                if (Sync.IsServer)
                {
                    this.m_statActionsRequested = true;
                }
            }
            else
            {
                foreach (MyDefinitionId id in definition2.Stats)
                {
                    MyEntityStatDefinition definition3 = null;
                    if (MyDefinitionManager.Static.TryGetDefinition<MyEntityStatDefinition>(id, out definition3) && (definition3.Enabled && ((definition3.EnabledInCreative || !MySession.Static.CreativeMode) && (definition3.AvailableInSurvival || !MySession.Static.SurvivalMode))))
                    {
                        MyStringHash orCompute = MyStringHash.GetOrCompute(definition3.Name);
                        MyEntityStat stat = null;
                        if (!this.m_stats.TryGetValue(orCompute, out stat) || (stat.StatDefinition.Id.SubtypeId != definition3.Id.SubtypeId))
                        {
                            MyObjectBuilder_EntityStat objectBuilder = new MyObjectBuilder_EntityStat {
                                SubtypeName = id.SubtypeName,
                                MaxValue = 1f,
                                Value = definition3.DefaultValue / definition3.MaxValue
                            };
                            this.AddStat(orCompute, objectBuilder, false);
                        }
                    }
                }
                if (Sync.IsServer)
                {
                    foreach (string str in definition2.Scripts)
                    {
                        this.InitScript(str);
                    }
                    this.m_statActionsRequested = true;
                }
            }
        }

        private void InitScript(string scriptName)
        {
            if (scriptName == "SpaceStatEffect")
            {
                MySpaceStatEffect item = new MySpaceStatEffect();
                item.Init(base.Entity as IMyCharacter, this.m_stats, "SpaceStatEffect");
                this.m_scripts.Add(item);
            }
            else
            {
                Type type;
                if (MyScriptManager.Static.StatScripts.TryGetValue(scriptName, out type))
                {
                    MyStatLogic item = (MyStatLogic) Activator.CreateInstance(type);
                    if (item != null)
                    {
                        item.Init(base.Entity as IMyCharacter, this.m_stats, scriptName);
                        this.m_scripts.Add(item);
                    }
                }
            }
        }

        public override bool IsSerialized() => 
            true;

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            using (List<MyStatLogic>.Enumerator enumerator = this.m_scripts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Character = base.Entity as IMyCharacter;
                }
            }
        }

        public override void OnBeforeRemovedFromContainer()
        {
            using (List<MyStatLogic>.Enumerator enumerator = this.m_scripts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Close();
                }
            }
            base.OnBeforeRemovedFromContainer();
        }

        [Event(null, 0x6b), Reliable, Client]
        private static void OnStatActionMessage(long entityId, Dictionary<string, MyStatLogic.MyStatAction> statActions)
        {
            MyEntity entity = null;
            if (MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                MyEntityStatComponent component = null;
                if (entity.Components.TryGet<MyEntityStatComponent>(out component))
                {
                    MyStatLogic item = new MyStatLogic();
                    item.Init(entity as IMyCharacter, component.m_stats, "LocalStatActionScript");
                    foreach (KeyValuePair<string, MyStatLogic.MyStatAction> pair in statActions)
                    {
                        item.AddAction(pair.Key, pair.Value);
                    }
                    component.m_scripts.Add(item);
                }
            }
        }

        [Event(null, 0x4b), Reliable, Server]
        private static void OnStatActionRequest(long entityId)
        {
            MyEntity entity = null;
            if (MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                MyEntityStatComponent component = null;
                if (entity.Components.TryGet<MyEntityStatComponent>(out component))
                {
                    Dictionary<string, MyStatLogic.MyStatAction> statActions = new Dictionary<string, MyStatLogic.MyStatAction>();
                    using (List<MyStatLogic>.Enumerator enumerator = component.m_scripts.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            foreach (KeyValuePair<string, MyStatLogic.MyStatAction> pair in enumerator.Current.StatActions)
                            {
                                if (!statActions.ContainsKey(pair.Key))
                                {
                                    statActions.Add(pair.Key, pair.Value);
                                }
                            }
                        }
                    }
                    if (MyEventContext.Current.IsLocallyInvoked)
                    {
                        OnStatActionMessage(entityId, statActions);
                    }
                    else
                    {
                        Vector3D? position = null;
                        MyMultiplayer.RaiseStaticEvent<long, Dictionary<string, MyStatLogic.MyStatAction>>(s => new Action<long, Dictionary<string, MyStatLogic.MyStatAction>>(MyEntityStatComponent.OnStatActionMessage), entityId, statActions, MyEventContext.Current.Sender, position);
                    }
                }
            }
        }

        [Event(null, 0x36), Reliable, Broadcast]
        private static void OnStatChangedMessage(long entityId, List<StatInfo> changedStats)
        {
            MyEntity entity;
            if (MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                MyEntityStatComponent component = null;
                if (entity.Components.TryGet<MyEntityStatComponent>(out component))
                {
                    foreach (StatInfo info in changedStats)
                    {
                        MyEntityStat stat;
                        if (component.TryGetStat(info.StatId, out stat))
                        {
                            stat.Value = info.Amount;
                            stat.StatRegenLeft = info.RegenLeft;
                        }
                    }
                }
            }
        }

        private void SendStatsChanged(List<MyEntityStat> stats)
        {
            List<StatInfo> list = new List<StatInfo>();
            foreach (MyEntityStat stat in stats)
            {
                stat.CalculateRegenLeftForLongestEffect();
                StatInfo item = new StatInfo {
                    StatId = stat.StatId,
                    Amount = stat.Value,
                    RegenLeft = stat.StatRegenLeft
                };
                list.Add(item);
            }
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, List<StatInfo>>(s => new Action<long, List<StatInfo>>(MyEntityStatComponent.OnStatChangedMessage), base.Entity.EntityId, list, targetEndpoint, position);
        }

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_ComponentBase base2 = base.Serialize(false);
            MyObjectBuilder_CharacterStatComponent component = base2 as MyObjectBuilder_CharacterStatComponent;
            if (component == null)
            {
                return base2;
            }
            component.Stats = null;
            component.ScriptNames = null;
            if ((this.m_stats != null) && (this.m_stats.Count > 0))
            {
                component.Stats = new MyObjectBuilder_EntityStat[this.m_stats.Count];
                int index = 0;
                foreach (KeyValuePair<MyStringHash, MyEntityStat> pair in this.m_stats)
                {
                    index++;
                    component.Stats[index] = pair.Value.GetObjectBuilder();
                }
            }
            if ((this.m_scripts != null) && (this.m_scripts.Count > 0))
            {
                component.ScriptNames = new string[this.m_scripts.Count];
                int index = 0;
                foreach (MyStatLogic logic in this.m_scripts)
                {
                    index++;
                    component.ScriptNames[index] = logic.Name;
                }
            }
            return component;
        }

        public bool TryGetStat(MyStringHash statId, out MyEntityStat outStat) => 
            this.m_stats.TryGetValue(statId, out outStat);

        public virtual void Update()
        {
            if (base.Container.Entity != null)
            {
                List<MyStatLogic>.Enumerator enumerator;
                if (!this.m_statActionsRequested)
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MyEntityStatComponent.OnStatActionRequest), base.Entity.EntityId, targetEndpoint, position);
                    this.m_statActionsRequested = true;
                }
                using (enumerator = this.m_scripts.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Update();
                    }
                }
                int updateCounter = this.m_updateCounter;
                this.m_updateCounter = updateCounter + 1;
                if ((updateCounter % 10) == 0)
                {
                    using (enumerator = this.m_scripts.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.Update10();
                        }
                    }
                }
                foreach (MyEntityStat stat in this.m_stats.Values)
                {
                    stat.Update();
                    if (Sync.IsServer && stat.ShouldSync)
                    {
                        m_statSyncList.Add(stat);
                    }
                }
                if (m_statSyncList.Count > 0)
                {
                    this.SendStatsChanged(m_statSyncList);
                    m_statSyncList.Clear();
                }
            }
        }

        public DictionaryValuesReader<MyStringHash, MyEntityStat> Stats =>
            new DictionaryValuesReader<MyStringHash, MyEntityStat>(this.m_stats);

        public override string ComponentTypeDebugString =>
            "Stats";

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyEntityStatComponent.<>c <>9 = new MyEntityStatComponent.<>c();
            public static Func<IMyEventOwner, Action<long, List<MyEntityStatComponent.StatInfo>>> <>9__1_0;
            public static Func<IMyEventOwner, Action<long, Dictionary<string, MyStatLogic.MyStatAction>>> <>9__3_0;
            public static Func<IMyEventOwner, Action<long>> <>9__17_0;

            internal Action<long, Dictionary<string, MyStatLogic.MyStatAction>> <OnStatActionRequest>b__3_0(IMyEventOwner s) => 
                new Action<long, Dictionary<string, MyStatLogic.MyStatAction>>(MyEntityStatComponent.OnStatActionMessage);

            internal Action<long, List<MyEntityStatComponent.StatInfo>> <SendStatsChanged>b__1_0(IMyEventOwner s) => 
                new Action<long, List<MyEntityStatComponent.StatInfo>>(MyEntityStatComponent.OnStatChangedMessage);

            internal Action<long> <Update>b__17_0(IMyEventOwner s) => 
                new Action<long>(MyEntityStatComponent.OnStatActionRequest);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct StatInfo
        {
            public MyStringHash StatId;
            public float Amount;
            public float RegenLeft;
        }
    }
}

