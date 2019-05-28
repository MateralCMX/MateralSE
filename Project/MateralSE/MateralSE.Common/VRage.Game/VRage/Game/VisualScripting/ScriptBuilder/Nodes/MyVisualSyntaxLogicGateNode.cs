namespace VRage.Game.VisualScripting.ScriptBuilder.Nodes
{
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.VisualScripting;
    using VRage.Game.VisualScripting;
    using VRage.Game.VisualScripting.Utils;

    public class MyVisualSyntaxLogicGateNode : MyVisualSyntaxNode
    {
        private readonly Dictionary<MyVisualSyntaxNode, string> m_inputsToVariableNames;

        public MyVisualSyntaxLogicGateNode(MyObjectBuilder_ScriptNode ob) : base(ob)
        {
            this.m_inputsToVariableNames = new Dictionary<MyVisualSyntaxNode, string>();
        }

        internal override void CollectInputExpressions(List<StatementSyntax> expressions)
        {
            base.CollectInputExpressions(expressions);
            if (this.Inputs.Count == 1)
            {
                ExpressionSyntax initializer = (this.ObjectBuilder.Operation != MyObjectBuilder_LogicGateScriptNode.LogicOperation.NOT) ? ((ExpressionSyntax) SyntaxFactory.IdentifierName(this.Inputs[0].VariableSyntaxName(this.m_inputsToVariableNames[this.Inputs[0]]))) : ((ExpressionSyntax) SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, SyntaxFactory.IdentifierName(this.Inputs[0].VariableSyntaxName(this.m_inputsToVariableNames[this.Inputs[0]]))));
                LocalDeclarationStatementSyntax item = MySyntaxFactory.LocalVariable(typeof(bool).Signature(), this.VariableSyntaxName(null), initializer);
                expressions.Add(item);
            }
            else if (this.Inputs.Count > 1)
            {
                ExpressionSyntax expression = SyntaxFactory.BinaryExpression(this.OperationKind, SyntaxFactory.IdentifierName(this.Inputs[0].VariableSyntaxName(this.m_inputsToVariableNames[this.Inputs[0]])), SyntaxFactory.IdentifierName(this.Inputs[1].VariableSyntaxName(this.m_inputsToVariableNames[this.Inputs[1]])));
                int num = 2;
                while (true)
                {
                    if (num >= this.Inputs.Count)
                    {
                        if ((this.ObjectBuilder.Operation == MyObjectBuilder_LogicGateScriptNode.LogicOperation.NAND) || (this.ObjectBuilder.Operation == MyObjectBuilder_LogicGateScriptNode.LogicOperation.NOR))
                        {
                            expression = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, SyntaxFactory.ParenthesizedExpression(expression));
                        }
                        LocalDeclarationStatementSyntax item = MySyntaxFactory.LocalVariable(typeof(bool).Signature(), this.VariableSyntaxName(null), expression);
                        expressions.Add(item);
                        break;
                    }
                    MyVisualSyntaxNode node = this.Inputs[num];
                    expression = SyntaxFactory.BinaryExpression(this.OperationKind, expression, SyntaxFactory.IdentifierName(node.VariableSyntaxName(this.m_inputsToVariableNames[node])));
                    num++;
                }
            }
        }

        protected internal override void Preprocess(int currentDepth)
        {
            if (!base.Preprocessed)
            {
                foreach (MyVariableIdentifier identifier in this.ObjectBuilder.ValueInputs)
                {
                    MyVisualSyntaxNode key = base.TryRegisterNode(identifier.NodeID, this.Inputs);
                    if (key != null)
                    {
                        this.m_inputsToVariableNames.Add(key, identifier.VariableName);
                    }
                }
                foreach (MyVariableIdentifier identifier2 in this.ObjectBuilder.ValueOutputs)
                {
                    base.TryRegisterNode(identifier2.NodeID, this.Outputs);
                }
            }
            base.Preprocess(currentDepth);
        }

        protected internal override string VariableSyntaxName(string variableIdentifier = null) => 
            ("logicGate_" + this.ObjectBuilder.ID + "_output");

        public MyObjectBuilder_LogicGateScriptNode ObjectBuilder =>
            ((MyObjectBuilder_LogicGateScriptNode) base.m_objectBuilder);

        internal override bool SequenceDependent =>
            false;

        private SyntaxKind OperationKind
        {
            get
            {
                switch (this.ObjectBuilder.Operation)
                {
                    case MyObjectBuilder_LogicGateScriptNode.LogicOperation.AND:
                    case MyObjectBuilder_LogicGateScriptNode.LogicOperation.NAND:
                        return SyntaxKind.LogicalAndExpression;

                    case MyObjectBuilder_LogicGateScriptNode.LogicOperation.OR:
                    case MyObjectBuilder_LogicGateScriptNode.LogicOperation.NOR:
                        return SyntaxKind.LogicalOrExpression;

                    case MyObjectBuilder_LogicGateScriptNode.LogicOperation.XOR:
                        return SyntaxKind.ExclusiveOrExpression;
                }
                return SyntaxKind.LogicalNotExpression;
            }
        }
    }
}

