namespace VRage.Groups
{
    using System;

    public interface IGroupData<TNode> where TNode: class
    {
        void OnCreate<TGroupData>(MyGroups<TNode, TGroupData>.Group group) where TGroupData: IGroupData<TNode>, new();
        void OnNodeAdded(TNode entity);
        void OnNodeRemoved(TNode entity);
        void OnRelease();
    }
}

