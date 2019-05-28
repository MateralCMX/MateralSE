namespace VRage.Game.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Text;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_AnimationSMConditionsConjunction : MyObjectBuilder_Base
    {
        [ProtoMember(0x5e), XmlElement("Condition")]
        public MyObjectBuilder_AnimationSMCondition[] Conditions;

        public MyObjectBuilder_AnimationSMConditionsConjunction DeepCopy()
        {
            MyObjectBuilder_AnimationSMConditionsConjunction conjunction = new MyObjectBuilder_AnimationSMConditionsConjunction();
            if (this.Conditions == null)
            {
                conjunction.Conditions = null;
            }
            else
            {
                conjunction.Conditions = new MyObjectBuilder_AnimationSMCondition[this.Conditions.Length];
                for (int i = 0; i < this.Conditions.Length; i++)
                {
                    MyObjectBuilder_AnimationSMCondition condition1 = new MyObjectBuilder_AnimationSMCondition();
                    condition1.Operation = this.Conditions[i].Operation;
                    condition1.ValueLeft = this.Conditions[i].ValueLeft;
                    condition1.ValueRight = this.Conditions[i].ValueRight;
                    conjunction.Conditions[i] = condition1;
                }
            }
            return conjunction;
        }

        public override string ToString()
        {
            if ((this.Conditions == null) || (this.Conditions.Length == 0))
            {
                return "[no content, false]";
            }
            bool flag = true;
            StringBuilder builder = new StringBuilder(0x200);
            builder.Append("[");
            foreach (MyObjectBuilder_AnimationSMCondition condition in this.Conditions)
            {
                if (!flag)
                {
                    builder.Append(" AND ");
                }
                builder.Append(condition.ToString());
                flag = false;
            }
            builder.Append("]");
            return builder.ToString();
        }
    }
}

