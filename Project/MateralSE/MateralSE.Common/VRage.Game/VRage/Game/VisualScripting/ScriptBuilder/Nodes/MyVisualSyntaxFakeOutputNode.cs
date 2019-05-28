namespace VRage.Game.VisualScripting.ScriptBuilder.Nodes
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class MyVisualSyntaxFakeOutputNode : MyVisualSyntaxNode
    {
        public MyVisualSyntaxFakeOutputNode(int id) : base(null)
        {
            this.ID = id;
        }

        protected internal override string VariableSyntaxName(string variableIdentifier = null)
        {
            object[] objArray1 = new object[] { "outParamFunctionNode_", this.ID, "_", variableIdentifier };
            return string.Concat(objArray1);
        }

        public int ID { get; private set; }
    }
}

