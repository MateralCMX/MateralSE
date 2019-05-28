namespace VRage.Game.VisualScripting.ScriptBuilder.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.VisualScripting.ScriptBuilder;

    public class MyVisualSyntaxNode
    {
        internal int Depth = 0x7fffffff;
        internal HashSet<MyVisualSyntaxNode> SubTreeNodes = new HashSet<MyVisualSyntaxNode>();
        protected MyObjectBuilder_ScriptNode m_objectBuilder;
        private static readonly MyBinaryStructHeap<int, HeapNodeWrapper> m_activeHeap = new MyBinaryStructHeap<int, HeapNodeWrapper>(0x80, null);
        private static readonly HashSet<MyVisualSyntaxNode> m_commonParentSet = new HashSet<MyVisualSyntaxNode>();
        private static readonly HashSet<MyVisualSyntaxNode> m_sequenceHelper = new HashSet<MyVisualSyntaxNode>();

        internal MyVisualSyntaxNode(MyObjectBuilder_ScriptNode ob)
        {
            this.m_objectBuilder = ob;
            this.Inputs = new List<MyVisualSyntaxNode>();
            this.Outputs = new List<MyVisualSyntaxNode>();
            this.SequenceInputs = new List<MyVisualSyntaxNode>();
            this.SequenceOutputs = new List<MyVisualSyntaxNode>();
        }

        internal virtual void CollectInputExpressions(List<StatementSyntax> expressions)
        {
            this.Collected = true;
            foreach (MyVisualSyntaxNode node in this.SubTreeNodes)
            {
                if (!node.Collected)
                {
                    node.CollectInputExpressions(expressions);
                }
            }
        }

        internal virtual void CollectSequenceExpressions(List<StatementSyntax> expressions)
        {
            this.CollectInputExpressions(expressions);
            using (List<MyVisualSyntaxNode>.Enumerator enumerator = this.SequenceOutputs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.CollectSequenceExpressions(expressions);
                }
            }
        }

        protected static MyVisualSyntaxNode CommonParent(IEnumerable<MyVisualSyntaxNode> nodes)
        {
            // Invalid method body.
        }

        public override int GetHashCode() => 
            ((this.ObjectBuilder != null) ? this.ObjectBuilder.ID : base.GetType().GetHashCode());

        public IEnumerable<MyVisualSyntaxNode> GetSequenceDependentOutputs()
        {
            m_sequenceHelper.Clear();
            this.SequenceDependentChildren(m_sequenceHelper);
            return m_sequenceHelper;
        }

        protected internal virtual void Preprocess(int currentDepth)
        {
            if (currentDepth < this.Depth)
            {
                this.Depth = currentDepth;
            }
            if (!this.Preprocessed)
            {
                using (List<MyVisualSyntaxNode>.Enumerator enumerator = this.SequenceOutputs.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Preprocess(this.Depth + 1);
                    }
                }
            }
            foreach (MyVisualSyntaxNode node in this.Inputs)
            {
                if (!node.SequenceDependent)
                {
                    node.Preprocess(this.Depth);
                }
            }
            if (!this.SequenceDependent && !this.Preprocessed)
            {
                if ((this.Outputs.Count == 1) && !this.Outputs[0].SequenceDependent)
                {
                    this.Outputs[0].SubTreeNodes.Add(this);
                }
                else if (this.Outputs.Count > 0)
                {
                    this.Navigator.FreshNodes.Add(this);
                }
            }
            this.Preprocessed = true;
        }

        internal virtual void Reset()
        {
            this.Depth = 0x7fffffff;
            this.SubTreeNodes.Clear();
            this.Inputs.Clear();
            this.Outputs.Clear();
            this.SequenceOutputs.Clear();
            this.SequenceInputs.Clear();
            this.Collected = false;
            this.Preprocessed = false;
        }

        private void SequenceDependentChildren(HashSet<MyVisualSyntaxNode> results)
        {
            if ((this.Outputs.Count != 0) && (this.Depth != 0x7fffffff))
            {
                foreach (MyVisualSyntaxNode node in this.Outputs)
                {
                    if (node.Depth != 0x7fffffff)
                    {
                        if (node.SequenceDependent)
                        {
                            results.Add(node);
                            continue;
                        }
                        node.SequenceDependentChildren(results);
                    }
                }
            }
        }

        protected MyVisualSyntaxNode TryRegisterNode(int nodeID, List<MyVisualSyntaxNode> collection)
        {
            if (nodeID == -1)
            {
                return null;
            }
            MyVisualSyntaxNode nodeByID = this.Navigator.GetNodeByID(nodeID);
            if (nodeByID != null)
            {
                collection.Add(nodeByID);
            }
            return nodeByID;
        }

        protected internal virtual string VariableSyntaxName(string variableIdentifier = null)
        {
            throw new NotImplementedException();
        }

        protected bool Preprocessed { get; set; }

        internal virtual bool SequenceDependent =>
            true;

        internal bool Collected { get; private set; }

        internal List<MyVisualSyntaxNode> SequenceInputs { virtual get; private set; }

        internal List<MyVisualSyntaxNode> SequenceOutputs { virtual get; private set; }

        internal List<MyVisualSyntaxNode> Outputs { virtual get; private set; }

        internal List<MyVisualSyntaxNode> Inputs { virtual get; private set; }

        public MyObjectBuilder_ScriptNode ObjectBuilder =>
            this.m_objectBuilder;

        internal MyVisualScriptNavigator Navigator { get; set; }

        [StructLayout(LayoutKind.Sequential)]
        protected struct HeapNodeWrapper
        {
            public MyVisualSyntaxNode Node;
        }
    }
}

