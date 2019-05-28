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

    public class MyVisualSyntaxForLoopNode : MyVisualSyntaxNode
    {
        private MyVisualSyntaxNode m_bodySequence;
        private MyVisualSyntaxNode m_finishSequence;
        private MyVisualSyntaxNode m_firstInput;
        private MyVisualSyntaxNode m_lastInput;
        private MyVisualSyntaxNode m_incrementInput;
        private readonly List<MyVisualSyntaxNode> m_toCollectNodeCache;

        public MyVisualSyntaxForLoopNode(MyObjectBuilder_ScriptNode ob) : base(ob)
        {
            this.m_toCollectNodeCache = new List<MyVisualSyntaxNode>();
        }

        internal override void CollectSequenceExpressions(List<StatementSyntax> expressions)
        {
            this.m_toCollectNodeCache.Clear();
            foreach (MyVisualSyntaxNode node in base.SubTreeNodes)
            {
                bool flag = false;
                foreach (MyVisualSyntaxNode node2 in node.Outputs)
                {
                    if (ReferenceEquals(node2, this) && !node2.Collected)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    node.CollectInputExpressions(expressions);
                }
                else
                {
                    this.m_toCollectNodeCache.Add(node);
                }
            }
            if (this.m_bodySequence != null)
            {
                List<StatementSyntax> list = new List<StatementSyntax>();
                using (List<MyVisualSyntaxNode>.Enumerator enumerator2 = this.m_toCollectNodeCache.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        enumerator2.Current.CollectInputExpressions(list);
                    }
                }
                this.m_bodySequence.CollectSequenceExpressions(list);
                ExpressionSyntax syntax = (this.m_firstInput == null) ? ((ExpressionSyntax) MySyntaxFactory.Literal(typeof(int).Signature(), this.ObjectBuilder.FirstIndexValue)) : ((ExpressionSyntax) SyntaxFactory.IdentifierName(this.m_firstInput.VariableSyntaxName(this.ObjectBuilder.FirstIndexValueInput.VariableName)));
                ExpressionSyntax right = (this.m_lastInput == null) ? ((ExpressionSyntax) MySyntaxFactory.Literal(typeof(int).Signature(), this.ObjectBuilder.LastIndexValue)) : ((ExpressionSyntax) SyntaxFactory.IdentifierName(this.m_lastInput.VariableSyntaxName(this.ObjectBuilder.LastIndexValueInput.VariableName)));
                ExpressionSyntax syntax3 = (this.m_incrementInput == null) ? ((ExpressionSyntax) MySyntaxFactory.Literal(typeof(int).Signature(), this.ObjectBuilder.IncrementValue)) : ((ExpressionSyntax) SyntaxFactory.IdentifierName(this.m_incrementInput.VariableSyntaxName(this.ObjectBuilder.IncrementValueInput.VariableName)));
                ForStatementSyntax item = SyntaxFactory.ForStatement(SyntaxFactory.Block(list)).WithDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword))).WithVariables(SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(this.VariableSyntaxName(null))).WithInitializer(SyntaxFactory.EqualsValueClause(syntax))))).WithCondition(SyntaxFactory.BinaryExpression(SyntaxKind.LessThanOrEqualExpression, SyntaxFactory.IdentifierName(this.VariableSyntaxName(null)), right)).WithIncrementors(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.AssignmentExpression(SyntaxKind.AddAssignmentExpression, SyntaxFactory.IdentifierName(this.VariableSyntaxName(null)), syntax3)));
                expressions.Add(item);
            }
            if (this.m_finishSequence != null)
            {
                this.m_finishSequence.CollectSequenceExpressions(expressions);
            }
        }

        protected internal override void Preprocess(int currentDepth)
        {
            if (!base.Preprocessed)
            {
                foreach (int num in this.ObjectBuilder.SequenceInputs)
                {
                    base.TryRegisterNode(num, this.SequenceInputs);
                }
                if (this.ObjectBuilder.SequenceOutput != -1)
                {
                    this.m_finishSequence = base.Navigator.GetNodeByID(this.ObjectBuilder.SequenceOutput);
                    if (this.m_finishSequence != null)
                    {
                        this.SequenceOutputs.Add(this.m_finishSequence);
                    }
                }
                if (this.ObjectBuilder.SequenceBody != -1)
                {
                    this.m_bodySequence = base.Navigator.GetNodeByID(this.ObjectBuilder.SequenceBody);
                    if (this.m_bodySequence != null)
                    {
                        this.SequenceOutputs.Add(this.m_bodySequence);
                    }
                }
                foreach (MyVariableIdentifier identifier in this.ObjectBuilder.CounterValueOutputs)
                {
                    base.TryRegisterNode(identifier.NodeID, this.Outputs);
                }
                this.m_firstInput = base.TryRegisterNode(this.ObjectBuilder.FirstIndexValueInput.NodeID, this.Inputs);
                this.m_lastInput = base.TryRegisterNode(this.ObjectBuilder.LastIndexValueInput.NodeID, this.Inputs);
                this.m_incrementInput = base.TryRegisterNode(this.ObjectBuilder.IncrementValueInput.NodeID, this.Inputs);
            }
            base.Preprocess(currentDepth);
        }

        protected internal override string VariableSyntaxName(string variableIdentifier = null) => 
            ("forEach_" + this.ObjectBuilder.ID + "_counter");

        public MyObjectBuilder_ForLoopScriptNode ObjectBuilder =>
            ((MyObjectBuilder_ForLoopScriptNode) base.m_objectBuilder);
    }
}

