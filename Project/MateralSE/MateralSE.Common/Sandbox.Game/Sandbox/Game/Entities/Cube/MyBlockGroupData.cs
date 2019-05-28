namespace Sandbox.Game.Entities.Cube
{
    using System;
    using VRage.Groups;

    public class MyBlockGroupData : IGroupData<MySlimBlock>
    {
        public void OnCreate<TGroupData>(MyGroups<MySlimBlock, TGroupData>.Group group) where TGroupData: IGroupData<MySlimBlock>, new()
        {
        }

        public void OnNodeAdded(MySlimBlock entity)
        {
        }

        public void OnNodeRemoved(MySlimBlock entity)
        {
        }

        public void OnRelease()
        {
        }
    }
}

