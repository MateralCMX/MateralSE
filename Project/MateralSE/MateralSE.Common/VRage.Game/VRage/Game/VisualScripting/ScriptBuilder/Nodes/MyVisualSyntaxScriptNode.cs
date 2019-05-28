namespace VRage.Game.VisualScripting.ScriptBuilder.Nodes
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.VisualScripting.Utils;

    public class MyVisualSyntaxScriptNode : MyVisualSyntaxNode
    {
        private readonly string m_instanceName;

        public MyVisualSyntaxScriptNode(MyObjectBuilder_ScriptNode ob) : base(ob)
        {
            this.m_instanceName = "m_scriptInstance_" + this.ObjectBuilder.ID;
        }

        internal override void CollectInputExpressions(List<StatementSyntax> expressions)
        {
            base.CollectInputExpressions(expressions);
            expressions.AddRange((IEnumerable<StatementSyntax>) this.ObjectBuilder.Outputs.Select<MyOutputParameterSerializationData, LocalDeclarationStatementSyntax>((t, index) => MySyntaxFactory.LocalVariable(t.Type, this.Outputs[index].VariableSyntaxName(t.Name), null)));
        }

        internal override void CollectSequenceExpressions(List<StatementSyntax> expressions)
        {
            this.CollectInputExpressions(expressions);
            List<StatementSyntax> list = new List<StatementSyntax>();
            using (List<MyVisualSyntaxNode>.Enumerator enumerator = this.SequenceOutputs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.CollectSequenceExpressions(list);
                }
            }
            StatementSyntax item = this.CreateScriptInvocationSyntax(list);
            expressions.Add(item);
        }

        private StatementSyntax CreateScriptInvocationSyntax(List<StatementSyntax> dependentStatements)
        {
            List<string> inputVariableNames = this.ObjectBuilder.Inputs.Select<MyInputParameterSerializationData, string>((t, index) => this.Inputs[index].VariableSyntaxName(t.Input.VariableName)).ToList<string>();
            InvocationExpressionSyntax condition = MySyntaxFactory.MethodInvocation("RunScript", inputVariableNames, this.ObjectBuilder.Outputs.Select<MyOutputParameterSerializationData, string>((t, index) => this.Outputs[index].VariableSyntaxName(t.Name)).ToList<string>(), this.m_instanceName);
            return ((dependentStatements != null) ? ((StatementSyntax) MySyntaxFactory.IfExpressionSyntax(condition, dependentStatements, null)) : ((StatementSyntax) SyntaxFactory.ExpressionStatement(condition)));
        }

        public StatementSyntax DisposeCallDeclaration() => 
            SyntaxFactory.ExpressionStatement(MySyntaxFactory.MethodInvocation("Dispose", null, this.m_instanceName));

        public MemberDeclarationSyntax InstanceDeclaration()
        {
            SeparatedSyntaxList<ArgumentSyntax> arguments = new SeparatedSyntaxList<ArgumentSyntax>();
            return SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(this.ObjectBuilder.Name)).WithVariables(SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(this.m_instanceName)).WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName(this.ObjectBuilder.Name)).WithArgumentList(SyntaxFactory.ArgumentList(arguments)))))));
        }

        protected internal override void Preprocess(int currentDepth)
        {
            if (!base.Preprocessed)
            {
                if (this.ObjectBuilder.SequenceOutput != -1)
                {
                    MyVisualSyntaxNode nodeByID = base.Navigator.GetNodeByID(this.ObjectBuilder.SequenceOutput);
                    this.SequenceOutputs.Add(nodeByID);
                }
                if (this.ObjectBuilder.SequenceInput != -1)
                {
                    MyVisualSyntaxNode nodeByID = base.Navigator.GetNodeByID(this.ObjectBuilder.SequenceInput);
                    this.SequenceInputs.Add(nodeByID);
                }
                foreach (MyInputParameterSerializationData data in this.ObjectBuilder.Inputs)
                {
                    if (data.Input.NodeID == -1)
                    {
                        throw new Exception("Output node missing input data. NodeID: " + this.ObjectBuilder.ID);
                    }
                    MyVisualSyntaxNode nodeByID = base.Navigator.GetNodeByID(data.Input.NodeID);
                    this.Inputs.Add(nodeByID);
                }
                foreach (MyOutputParameterSerializationData data2 in this.ObjectBuilder.Outputs)
                {
                    if (data2.Outputs.Ids.Count != 0)
                    {
                        foreach (MyVariableIdentifier identifier in data2.Outputs.Ids)
                        {
                            MyVisualSyntaxNode nodeByID = base.Navigator.GetNodeByID(identifier.NodeID);
                            this.Outputs.Add(nodeByID);
                        }
                        continue;
                    }
                    MyVisualSyntaxFakeOutputNode item = new MyVisualSyntaxFakeOutputNode(this.ObjectBuilder.ID);
                    this.Outputs.Add(item);
                }
            }
            base.Preprocess(currentDepth);
        }

        protected internal override string VariableSyntaxName(string variableIdentifier = null)
        {
            MyOutputParameterSerializationData item = this.ObjectBuilder.Outputs.FirstOrDefault<MyOutputParameterSerializationData>(o => o.Name == variableIdentifier);
            if (item != null)
            {
                int index = this.ObjectBuilder.Outputs.IndexOf(item);
                if (index != -1)
                {
                    variableIdentifier = this.Outputs[index].VariableSyntaxName(variableIdentifier);
                }
            }
            return variableIdentifier;
        }

        public MyObjectBuilder_ScriptScriptNode ObjectBuilder =>
            ((MyObjectBuilder_ScriptScriptNode) base.m_objectBuilder);
    }
}

