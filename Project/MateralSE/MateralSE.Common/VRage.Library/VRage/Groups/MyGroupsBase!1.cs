namespace VRage.Groups
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public abstract class MyGroupsBase<TNode> where TNode: class
    {
        protected MyGroupsBase()
        {
        }

        public abstract void AddNode(TNode nodeToAdd);
        public abstract bool BreakLink(long linkId, TNode parentNode, TNode childNode = null);
        public abstract void CreateLink(long linkId, TNode parentNode, TNode childNode);
        public abstract List<TNode> GetGroupNodes(TNode nodeInGroup);
        public abstract void GetGroupNodes(TNode nodeInGroup, List<TNode> result);
        public abstract bool HasSameGroup(TNode nodeA, TNode nodeB);
        public abstract bool LinkExists(long linkId, TNode parentNode, TNode childNode = null);
        public abstract void RemoveNode(TNode nodeToRemove);
    }
}

