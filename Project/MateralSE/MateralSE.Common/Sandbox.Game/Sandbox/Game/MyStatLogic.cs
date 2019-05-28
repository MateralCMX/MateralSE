namespace Sandbox.Game
{
    using ProtoBuf;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game.ModAPI;
    using VRage.Utils;

    public class MyStatLogic
    {
        private string m_scriptName;
        private IMyCharacter m_character;
        protected Dictionary<MyStringHash, MyEntityStat> m_stats;
        private bool m_enableAutoHealing = true;
        private Dictionary<string, MyStatAction> m_statActions = new Dictionary<string, MyStatAction>();
        private Dictionary<string, MyStatRegenModifier> m_statRegenModifiers = new Dictionary<string, MyStatRegenModifier>();
        private Dictionary<string, MyStatEfficiencyModifier> m_statEfficiencyModifiers = new Dictionary<string, MyStatEfficiencyModifier>();
        public const int STAT_VALUE_TOO_LOW = 4;

        public void AddAction(string actionId, MyStatAction action)
        {
            this.m_statActions.Add(actionId, action);
        }

        public void AddEfficiency(string modifierId, MyStatEfficiencyModifier modifier)
        {
            this.m_statEfficiencyModifiers.Add(modifierId, modifier);
        }

        public void AddModifier(string modifierId, MyStatRegenModifier modifier)
        {
            this.m_statRegenModifiers.Add(modifierId, modifier);
        }

        public void ApplyModifier(string modifierId)
        {
            MyStatRegenModifier modifier;
            MyEntityStat stat;
            if (this.m_statRegenModifiers.TryGetValue(modifierId, out modifier) && this.m_stats.TryGetValue(modifier.StatId, out stat))
            {
                stat.ApplyRegenAmountMultiplier(modifier.AmountMultiplier, modifier.Duration);
            }
        }

        public bool CanDoAction(string actionId, bool continuous, out MyTuple<ushort, MyStringHash> message)
        {
            MyStatAction action;
            MyEntityStat stat;
            if (!this.m_statActions.TryGetValue(actionId, out action))
            {
                message = new MyTuple<ushort, MyStringHash>(0, action.StatId);
                return true;
            }
            if (action.CanPerformWithout)
            {
                message = new MyTuple<ushort, MyStringHash>(0, action.StatId);
                return true;
            }
            if (!this.m_stats.TryGetValue(action.StatId, out stat))
            {
                message = new MyTuple<ushort, MyStringHash>(0, action.StatId);
                return true;
            }
            if (continuous)
            {
                if (stat.Value < action.Cost)
                {
                    message = new MyTuple<ushort, MyStringHash>(4, action.StatId);
                    return false;
                }
            }
            else if ((stat.Value < action.Cost) || (stat.Value < action.AmountToActivate))
            {
                message = new MyTuple<ushort, MyStringHash>(4, action.StatId);
                return false;
            }
            message = new MyTuple<ushort, MyStringHash>(0, action.StatId);
            return true;
        }

        public virtual void Close()
        {
        }

        public bool DoAction(string actionId)
        {
            MyStatAction action;
            MyEntityStat stat;
            if (!this.m_statActions.TryGetValue(actionId, out action))
            {
                return false;
            }
            if (!this.m_stats.TryGetValue(action.StatId, out stat))
            {
                return false;
            }
            if (action.CanPerformWithout)
            {
                stat.Value -= Math.Min(stat.Value, action.Cost);
                return true;
            }
            if ((((action.Cost >= 0f) && (stat.Value >= action.Cost)) || (action.Cost < 0f)) && (stat.Value >= action.AmountToActivate))
            {
                stat.Value -= action.Cost;
            }
            return true;
        }

        public float GetEfficiencyModifier(string modifierId)
        {
            MyStatEfficiencyModifier modifier;
            MyEntityStat stat;
            return (this.m_statEfficiencyModifiers.TryGetValue(modifierId, out modifier) ? (this.m_stats.TryGetValue(modifier.StatId, out stat) ? stat.GetEfficiencyMultiplier(modifier.EfficiencyMultiplier, modifier.Threshold) : 1f) : 1f);
        }

        public virtual void Init(IMyCharacter character, Dictionary<MyStringHash, MyEntityStat> stats, string scriptName)
        {
            this.m_scriptName = scriptName;
            this.Character = character;
            this.m_stats = stats;
            this.InitSettings();
        }

        private void InitSettings()
        {
            this.m_enableAutoHealing = MySession.Static.Settings.AutoHealing;
        }

        protected virtual void OnCharacterChanged(IMyCharacter oldCharacter)
        {
        }

        public virtual void Update()
        {
        }

        public virtual void Update10()
        {
        }

        public string Name =>
            this.m_scriptName;

        public IMyCharacter Character
        {
            get => 
                this.m_character;
            set
            {
                IMyCharacter oldCharacter = this.m_character;
                this.m_character = value;
                this.OnCharacterChanged(oldCharacter);
            }
        }

        protected bool EnableAutoHealing =>
            this.m_enableAutoHealing;

        public Dictionary<string, MyStatAction> StatActions =>
            this.m_statActions;

        public Dictionary<string, MyStatRegenModifier> StatRegenModifiers =>
            this.m_statRegenModifiers;

        public Dictionary<string, MyStatEfficiencyModifier> StatEfficiencyModifiers =>
            this.m_statEfficiencyModifiers;

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct MyStatAction
        {
            [ProtoMember(0x23)]
            public MyStringHash StatId;
            [ProtoMember(0x26)]
            public float Cost;
            [ProtoMember(0x29)]
            public float AmountToActivate;
            [ProtoMember(0x2c)]
            public bool CanPerformWithout;
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct MyStatEfficiencyModifier
        {
            [ProtoMember(0x40)]
            public MyStringHash StatId;
            [ProtoMember(0x43)]
            public float Threshold;
            [ProtoMember(70)]
            public float EfficiencyMultiplier;
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct MyStatRegenModifier
        {
            [ProtoMember(0x33)]
            public MyStringHash StatId;
            [ProtoMember(0x36)]
            public float AmountMultiplier;
            [ProtoMember(0x39)]
            public float Duration;
        }
    }
}

