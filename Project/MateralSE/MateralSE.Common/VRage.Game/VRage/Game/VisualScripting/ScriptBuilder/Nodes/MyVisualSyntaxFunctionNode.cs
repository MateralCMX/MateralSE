namespace VRage.Game.VisualScripting.ScriptBuilder.Nodes
{
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.VisualScripting;
    using VRage.Game.VisualScripting.Utils;

    public class MyVisualSyntaxFunctionNode : MyVisualSyntaxNode
    {
        private readonly MethodInfo m_methodInfo;
        private MyVisualSyntaxNode m_sequenceOutputNode;
        private MyVisualSyntaxNode m_instance;
        private readonly Type m_scriptBaseType;
        private readonly Dictionary<ParameterInfo, MyTuple<MyVisualSyntaxNode, MyVariableIdentifier>> m_parametersToInputs;

        public MyVisualSyntaxFunctionNode(MyObjectBuilder_ScriptNode ob, Type scriptBaseType) : base(ob)
        {
            this.m_parametersToInputs = new Dictionary<ParameterInfo, MyTuple<MyVisualSyntaxNode, MyVariableIdentifier>>();
            base.m_objectBuilder = (MyObjectBuilder_FunctionScriptNode) ob;
            this.m_methodInfo = MyVisualScriptingProxy.GetMethod(this.ObjectBuilder.Type);
            this.m_scriptBaseType = scriptBaseType;
            if (this.m_methodInfo == null)
            {
                string name = this.ObjectBuilder.Type.Remove(0, this.ObjectBuilder.Type.LastIndexOf('.') + 1);
                int index = name.IndexOf('(');
                if ((scriptBaseType != null) && (index > 0))
                {
                    name = name.Remove(index);
                    this.m_methodInfo = scriptBaseType.GetMethod(name);
                }
            }
            if ((this.m_methodInfo == null) && !string.IsNullOrEmpty(this.ObjectBuilder.DeclaringType))
            {
                Type type = MyVisualScriptingProxy.GetType(this.ObjectBuilder.DeclaringType);
                if (type != null)
                {
                    this.m_methodInfo = MyVisualScriptingProxy.GetMethod(type, this.ObjectBuilder.Type);
                }
            }
            if ((this.m_methodInfo == null) && !string.IsNullOrEmpty(this.ObjectBuilder.ExtOfType))
            {
                Type type = MyVisualScriptingProxy.GetType(this.ObjectBuilder.ExtOfType);
                this.m_methodInfo = MyVisualScriptingProxy.GetMethod(type, this.ObjectBuilder.Type);
            }
            if (this.m_methodInfo != null)
            {
                this.InitUsing();
            }
        }

        internal override void CollectInputExpressions(List<StatementSyntax> expressions)
        {
            base.CollectInputExpressions(expressions);
            List<SyntaxNodeOrToken> list = new List<SyntaxNodeOrToken>();
            ParameterInfo[] parameters = this.m_methodInfo.GetParameters();
            int index = 0;
            if (this.m_methodInfo.IsDefined(typeof(ExtensionAttribute), false))
            {
                index++;
            }
            while (true)
            {
                while (true)
                {
                    if (index < parameters.Length)
                    {
                        ParameterInfo parameter = parameters[index];
                        if (parameter.IsOut)
                        {
                            string variableName = this.VariableSyntaxName(parameter.Name);
                            expressions.Add(MySyntaxFactory.LocalVariable(parameter.ParameterType.GetElementType().Signature(), variableName, null));
                            list.Add(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(variableName)).WithNameColon(SyntaxFactory.NameColon(parameter.Name)).WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword)));
                        }
                        else
                        {
                            MyTuple<MyVisualSyntaxNode, MyVariableIdentifier> tuple;
                            if (this.m_parametersToInputs.TryGetValue(parameter, out tuple))
                            {
                                list.Add(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(tuple.Item1.VariableSyntaxName(tuple.Item2.VariableName))).WithNameColon(SyntaxFactory.NameColon(parameter.Name)));
                            }
                            else
                            {
                                MyParameterValue value2 = this.ObjectBuilder.InputParameterValues.Find(value => value.ParameterName == parameter.Name);
                                if (value2 != null)
                                {
                                    list.Add(MySyntaxFactory.ConstantArgument(parameter.ParameterType.Signature(), MyTexts.SubstituteTexts(value2.Value, null)).WithNameColon(SyntaxFactory.NameColon(parameter.Name)));
                                }
                                else
                                {
                                    if (parameter.HasDefaultValue)
                                    {
                                        break;
                                    }
                                    list.Add(MySyntaxFactory.ConstantDefaultArgument(parameter.ParameterType).WithNameColon(SyntaxFactory.NameColon(parameter.Name)));
                                }
                            }
                        }
                        list.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                        break;
                    }
                    if (list.Count > 0)
                    {
                        list.RemoveAt(list.Count - 1);
                    }
                    InvocationExpressionSyntax expression = null;
                    if (this.m_methodInfo.IsStatic && !this.m_methodInfo.IsDefined(typeof(ExtensionAttribute)))
                    {
                        expression = MySyntaxFactory.MethodInvocationExpressionSyntax(SyntaxFactory.IdentifierName(this.m_methodInfo.DeclaringType.FullName + "." + this.m_methodInfo.Name), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(list)), null);
                    }
                    else if (this.m_methodInfo.DeclaringType == this.m_scriptBaseType)
                    {
                        expression = MySyntaxFactory.MethodInvocationExpressionSyntax(SyntaxFactory.IdentifierName(this.m_methodInfo.Name), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(list)), null);
                    }
                    else
                    {
                        if (this.m_instance == null)
                        {
                            throw new Exception("FunctionNode: " + this.ObjectBuilder.ID + " Is missing mandatory instance input.");
                        }
                        string name = this.m_instance.VariableSyntaxName(this.ObjectBuilder.InstanceInputID.VariableName);
                        expression = MySyntaxFactory.MethodInvocationExpressionSyntax(SyntaxFactory.IdentifierName(this.m_methodInfo.Name), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(list)), SyntaxFactory.IdentifierName(name));
                    }
                    if (this.m_methodInfo.ReturnType == typeof(void))
                    {
                        expressions.Add(SyntaxFactory.ExpressionStatement(expression));
                        return;
                    }
                    expressions.Add(MySyntaxFactory.LocalVariable(string.Empty, this.VariableSyntaxName("Return"), expression));
                    return;
                }
                index++;
            }
        }

        private void InitUsing()
        {
            if (this.m_methodInfo.DeclaringType != null)
            {
                this.Using = MySyntaxFactory.UsingStatementSyntax(this.m_methodInfo.DeclaringType.Namespace);
            }
        }

        protected internal override void Preprocess(int currentDepth)
        {
            if (!base.Preprocessed)
            {
                if (this.SequenceDependent)
                {
                    if (this.ObjectBuilder.SequenceOutputID != -1)
                    {
                        this.m_sequenceOutputNode = base.Navigator.GetNodeByID(this.ObjectBuilder.SequenceOutputID);
                        this.SequenceOutputs.Add(this.m_sequenceOutputNode);
                    }
                    MyVisualSyntaxNode nodeByID = base.Navigator.GetNodeByID(this.ObjectBuilder.SequenceInputID);
                    this.SequenceInputs.Add(nodeByID);
                }
                else
                {
                    using (List<IdentifierList>.Enumerator enumerator = this.ObjectBuilder.OutputParametersIDs.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            foreach (MyVariableIdentifier identifier in enumerator.Current.Ids)
                            {
                                MyVisualSyntaxNode nodeByID = base.Navigator.GetNodeByID(identifier.NodeID);
                                this.Outputs.Add(nodeByID);
                            }
                        }
                    }
                }
                ParameterInfo[] parameters = this.m_methodInfo.GetParameters();
                this.Inputs.Capacity = this.ObjectBuilder.InputParameterIDs.Count;
                if (this.ObjectBuilder.Version == 0)
                {
                    for (int i = 0; i < this.ObjectBuilder.InputParameterIDs.Count; i++)
                    {
                        MyVariableIdentifier identifier2 = this.ObjectBuilder.InputParameterIDs[i];
                        MyVisualSyntaxNode nodeByID = base.Navigator.GetNodeByID(identifier2.NodeID);
                        if (nodeByID != null)
                        {
                            this.Inputs.Add(nodeByID);
                            this.m_parametersToInputs.Add(parameters[i], new MyTuple<MyVisualSyntaxNode, MyVariableIdentifier>(nodeByID, identifier2));
                        }
                    }
                }
                else
                {
                    int index = 0;
                    if (this.m_methodInfo.IsDefined(typeof(ExtensionAttribute), false))
                    {
                        index++;
                    }
                    while (true)
                    {
                        if (index >= parameters.Length)
                        {
                            if (this.ObjectBuilder.InstanceInputID.NodeID != -1)
                            {
                                this.m_instance = base.Navigator.GetNodeByID(this.ObjectBuilder.InstanceInputID.NodeID);
                                if (this.m_instance != null)
                                {
                                    this.Inputs.Add(this.m_instance);
                                }
                            }
                            break;
                        }
                        ParameterInfo parameter = parameters[index];
                        MyVariableIdentifier identifier3 = this.ObjectBuilder.InputParameterIDs.Find(ident => ident.OriginName == parameter.Name);
                        if (!string.IsNullOrEmpty(identifier3.OriginName))
                        {
                            MyVisualSyntaxNode nodeByID = base.Navigator.GetNodeByID(identifier3.NodeID);
                            if (nodeByID == null)
                            {
                                if (!parameter.HasDefaultValue)
                                {
                                }
                            }
                            else
                            {
                                this.Inputs.Add(nodeByID);
                                this.m_parametersToInputs.ContainsKey(parameter);
                                this.m_parametersToInputs.Add(parameter, new MyTuple<MyVisualSyntaxNode, MyVariableIdentifier>(nodeByID, identifier3));
                            }
                        }
                        index++;
                    }
                }
            }
            base.Preprocess(currentDepth);
        }

        internal override void Reset()
        {
            base.Reset();
            this.m_parametersToInputs.Clear();
        }

        protected internal override string VariableSyntaxName(string variableIdentifier = null)
        {
            object[] objArray1 = new object[] { "outParamFunctionNode_", this.ObjectBuilder.ID, "_", variableIdentifier };
            return string.Concat(objArray1);
        }

        internal override bool SequenceDependent =>
            this.m_methodInfo.IsSequenceDependent();

        public UsingDirectiveSyntax Using { get; private set; }

        public MyObjectBuilder_FunctionScriptNode ObjectBuilder =>
            ((MyObjectBuilder_FunctionScriptNode) base.m_objectBuilder);
    }
}

