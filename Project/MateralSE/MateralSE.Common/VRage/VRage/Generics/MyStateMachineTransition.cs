namespace VRage.Generics
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Utils;

    public class MyStateMachineTransition
    {
        public MyStringId Name = MyStringId.NullOrEmpty;
        public MyStateMachineNode TargetNode;
        public List<IMyCondition> Conditions = new List<IMyCondition>();
        public int? Priority;

        public void _SetId(int newId)
        {
            this.Id = newId;
        }

        public virtual bool Evaluate()
        {
            for (int i = 0; i < this.Conditions.Count; i++)
            {
                if (!this.Conditions[i].Evaluate())
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString() => 
            ((this.TargetNode == null) ? "transition -> (null)" : ("transition -> " + this.TargetNode.Name));

        public int Id { get; private set; }
    }
}

