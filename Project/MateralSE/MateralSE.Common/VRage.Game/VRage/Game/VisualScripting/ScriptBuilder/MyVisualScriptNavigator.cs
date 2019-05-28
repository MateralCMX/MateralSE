namespace VRage.Game.VisualScripting.ScriptBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.VisualScripting;
    using VRage.Game.VisualScripting;
    using VRage.Game.VisualScripting.ScriptBuilder.Nodes;

    internal class MyVisualScriptNavigator
    {
        private readonly Dictionary<int, MyVisualSyntaxNode> m_idToNode = new Dictionary<int, MyVisualSyntaxNode>();
        private readonly Dictionary<Type, List<MyVisualSyntaxNode>> m_nodesByType = new Dictionary<Type, List<MyVisualSyntaxNode>>();
        private readonly Dictionary<string, MyVisualSyntaxVariableNode> m_variablesByName = new Dictionary<string, MyVisualSyntaxVariableNode>();
        private readonly List<MyVisualSyntaxNode> m_freshNodes = new List<MyVisualSyntaxNode>();

        public MyVisualScriptNavigator(MyObjectBuilder_VisualScript scriptOb)
        {
            Type baseClass = string.IsNullOrEmpty(scriptOb.Interface) ? null : MyVisualScriptingProxy.GetType(scriptOb.Interface);
            foreach (MyObjectBuilder_ScriptNode node in scriptOb.Nodes)
            {
                MyVisualSyntaxNode node2;
                switch (node)
                {
                    case (MyObjectBuilder_NewListScriptNode _):
                        node2 = new MyVisualSyntaxNewListNode(node);
                        break;

                    case (MyObjectBuilder_SwitchScriptNode _):
                        node2 = new MyVisualSyntaxSwitchNode(node);
                        break;

                    case (MyObjectBuilder_LocalizationScriptNode _):
                        node2 = new MyVisualSyntaxLocalizationNode(node);
                        break;

                    case (MyObjectBuilder_LogicGateScriptNode _):
                        node2 = new MyVisualSyntaxLogicGateNode(node);
                        break;

                    case (MyObjectBuilder_ForLoopScriptNode _):
                        node2 = new MyVisualSyntaxForLoopNode(node);
                        break;

                    case (MyObjectBuilder_SequenceScriptNode _):
                        node2 = new MyVisualSyntaxSequenceNode(node);
                        break;

                    case (MyObjectBuilder_ArithmeticScriptNode _):
                        node2 = new MyVisualSyntaxArithmeticNode(node);
                        break;

                    case (MyObjectBuilder_InterfaceMethodNode _):
                        node2 = new MyVisualSyntaxInterfaceMethodNode(node, baseClass);
                        break;

                    case (MyObjectBuilder_KeyEventScriptNode _):
                        node2 = new MyVisualSyntaxKeyEventNode(node);
                        break;

                    case (MyObjectBuilder_BranchingScriptNode _):
                        node2 = new MyVisualSyntaxBranchingNode(node);
                        break;

                    case (MyObjectBuilder_InputScriptNode _):
                        node2 = new MyVisualSyntaxInputNode(node);
                        break;

                    case (MyObjectBuilder_CastScriptNode _):
                        node2 = new MyVisualSyntaxCastNode(node);
                        break;

                    case (MyObjectBuilder_EventScriptNode _):
                        node2 = new MyVisualSyntaxEventNode(node);
                        break;

                    case (MyObjectBuilder_FunctionScriptNode _):
                        node2 = new MyVisualSyntaxFunctionNode(node, baseClass);
                        break;

                    case (MyObjectBuilder_VariableSetterScriptNode _):
                        node2 = new MyVisualSyntaxSetterNode(node);
                        break;

                    case (MyObjectBuilder_TriggerScriptNode _):
                        node2 = new MyVisualSyntaxTriggerNode(node);
                        break;

                    case (MyObjectBuilder_VariableScriptNode _):
                        node2 = new MyVisualSyntaxVariableNode(node);
                        break;

                    case (MyObjectBuilder_ConstantScriptNode _):
                        node2 = new MyVisualSyntaxConstantNode(node);
                        break;

                    case (MyObjectBuilder_GetterScriptNode _):
                        node2 = new MyVisualSyntaxGetterNode(node);
                        break;

                    case (MyObjectBuilder_OutputScriptNode _):
                        node2 = new MyVisualSyntaxOutputNode(node);
                        break;

                    case (MyObjectBuilder_ScriptScriptNode _):
                        break;

                    default:
                        continue;
                        break;
                }
                node2.Navigator = this;
                this.m_idToNode.Add(node.ID, node2);
                Type type = node2.GetType();
                if (!this.m_nodesByType.ContainsKey(type))
                {
                    this.m_nodesByType.Add(type, new List<MyVisualSyntaxNode>());
                }
                this.m_nodesByType[type].Add(node2);
                if (type == typeof(MyVisualSyntaxVariableNode))
                {
                    this.m_variablesByName.Add(((MyObjectBuilder_VariableScriptNode) node).VariableName, (MyVisualSyntaxVariableNode) node2);
                }
            }
        }

        public MyVisualSyntaxNode GetNodeByID(int id)
        {
            MyVisualSyntaxNode node;
            this.m_idToNode.TryGetValue(id, out node);
            return node;
        }

        public MyVisualSyntaxVariableNode GetVariable(string name)
        {
            MyVisualSyntaxVariableNode node;
            this.m_variablesByName.TryGetValue(name, out node);
            return node;
        }

        public List<T> OfType<T>() where T: MyVisualSyntaxNode
        {
            List<MyVisualSyntaxNode> list = new List<MyVisualSyntaxNode>();
            foreach (KeyValuePair<Type, List<MyVisualSyntaxNode>> pair in this.m_nodesByType)
            {
                if (typeof(T) == pair.Key)
                {
                    list.AddRange(pair.Value);
                }
            }
            return list.ConvertAll<T>(node => (T) node);
        }

        public void ResetNodes()
        {
            foreach (KeyValuePair<int, MyVisualSyntaxNode> pair in this.m_idToNode)
            {
                pair.Value.Reset();
            }
        }

        public List<MyVisualSyntaxNode> FreshNodes =>
            this.m_freshNodes;

        [Serializable, CompilerGenerated]
        private sealed class <>c__8<T> where T: MyVisualSyntaxNode
        {
            public static readonly MyVisualScriptNavigator.<>c__8<T> <>9;
            public static Converter<MyVisualSyntaxNode, T> <>9__8_0;

            static <>c__8()
            {
                MyVisualScriptNavigator.<>c__8<T>.<>9 = new MyVisualScriptNavigator.<>c__8<T>();
            }

            internal T <OfType>b__8_0(MyVisualSyntaxNode node) => 
                ((T) node);
        }
    }
}

