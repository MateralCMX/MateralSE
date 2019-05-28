namespace VRage.Game.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [XmlSerializerAssembly("VRage.Game.XmlSerializers"), ProtoContract, XmlType("StatCondition"), MyObjectBuilderDefinition((Type) null, null)]
    public class StatCondition : ConditionBase
    {
        public StatConditionOperator Operator;
        public float Value;
        public MyStringHash StatId;
        private IMyHudStat m_relatedStat;

        public override bool Eval()
        {
            if (this.m_relatedStat != null)
            {
                switch (this.Operator)
                {
                    case StatConditionOperator.Below:
                        return (this.m_relatedStat.CurrentValue < (this.Value * this.m_relatedStat.MaxValue));

                    case StatConditionOperator.Above:
                        return (this.m_relatedStat.CurrentValue > (this.Value * this.m_relatedStat.MaxValue));

                    case StatConditionOperator.Equal:
                        return (this.m_relatedStat.CurrentValue == (this.Value * this.m_relatedStat.MaxValue));

                    case StatConditionOperator.NotEqual:
                        return !(this.m_relatedStat.CurrentValue == (this.Value * this.m_relatedStat.MaxValue));
                }
            }
            return false;
        }

        public void SetStat(IMyHudStat stat)
        {
            this.m_relatedStat = stat;
        }
    }
}

