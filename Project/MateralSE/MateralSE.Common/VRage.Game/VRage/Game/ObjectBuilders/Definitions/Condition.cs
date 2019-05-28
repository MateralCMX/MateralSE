namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;

    [XmlSerializerAssembly("VRage.Game.XmlSerializers"), XmlType("Condition"), MyObjectBuilderDefinition((Type) null, null)]
    public class Condition : ConditionBase
    {
        [XmlArrayItem("Term", Type=typeof(MyAbstractXmlSerializer<ConditionBase>))]
        public ConditionBase[] Terms;
        public StatLogicOperator Operator;

        public override bool Eval()
        {
            if (this.Terms == null)
            {
                return false;
            }
            bool flag = this.Terms[0].Eval();
            if (this.Operator == StatLogicOperator.Not)
            {
                return !flag;
            }
            for (int i = 1; i < this.Terms.Length; i++)
            {
                ConditionBase base2 = this.Terms[i];
                StatLogicOperator @operator = this.Operator;
                if (@operator == StatLogicOperator.And)
                {
                    flag &= base2.Eval();
                }
                else if (@operator == StatLogicOperator.Or)
                {
                    flag |= base2.Eval();
                }
            }
            return flag;
        }
    }
}

