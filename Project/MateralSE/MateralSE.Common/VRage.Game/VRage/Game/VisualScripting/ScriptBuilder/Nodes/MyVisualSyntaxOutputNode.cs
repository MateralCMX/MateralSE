namespace VRage.Game.VisualScripting.ScriptBuilder.Nodes
{
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.VisualScripting.Utils;

    public class MyVisualSyntaxOutputNode : MyVisualSyntaxNode
    {
        private readonly List<MyVisualSyntaxNode> m_inputNodes;

        public MyVisualSyntaxOutputNode(MyObjectBuilder_ScriptNode ob) : base(ob)
        {
            this.m_inputNodes = new List<MyVisualSyntaxNode>();
        }

        internal override void CollectInputExpressions(List<StatementSyntax> expressions)
        {
            base.CollectInputExpressions(expressions);
            List<StatementSyntax> collection = new List<StatementSyntax>(this.ObjectBuilder.Inputs.Count);
            for (int i = 0; i < this.ObjectBuilder.Inputs.Count; i++)
            {
                string name = this.m_inputNodes[i].VariableSyntaxName(this.ObjectBuilder.Inputs[i].Input.VariableName);
                ExpressionStatementSyntax item = MySyntaxFactory.SimpleAssignment(this.ObjectBuilder.Inputs[i].Name, SyntaxFactory.IdentifierName(name));
                collection.Add(item);
            }
            expressions.AddRange(collection);
            expressions.Add(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)));
        }

        protected internal override void Preprocess(int currentDepth)
        {
            if (!base.Preprocessed)
            {
                if (this.ObjectBuilder.SequenceInputID != -1)
                {
                    MyVisualSyntaxNode nodeByID = base.Navigator.GetNodeByID(this.ObjectBuilder.SequenceInputID);
                    this.SequenceInputs.Add(nodeByID);
                }
                foreach (MyInputParameterSerializationData data in this.ObjectBuilder.Inputs)
                {
                    if (data.Input.NodeID == -1)
                    {
                        object[] objArray1 = new object[] { "Output node missing input for ", data.Name, ". NodeID: ", this.ObjectBuilder.ID };
                        throw new Exception(string.Concat(objArray1));
                    }
                    MyVisualSyntaxNode nodeByID = base.Navigator.GetNodeByID(data.Input.NodeID);
                    this.m_inputNodes.Add(nodeByID);
                    this.Inputs.Add(nodeByID);
                }
            }
            base.Preprocess(currentDepth);
        }

        internal override void Reset()
        {
            base.Reset();
            this.m_inputNodes.Clear();
        }

        public MyObjectBuilder_OutputScriptNode ObjectBuilder =>
            ((MyObjectBuilder_OutputScriptNode) base.m_objectBuilder);
    }
}

