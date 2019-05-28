namespace VRage.Game.Components
{
    using System;

    public interface IMyComponentAggregate
    {
        void AfterComponentAdd(MyComponentBase component);
        void BeforeComponentRemove(MyComponentBase component);

        MyAggregateComponentList ChildList { get; }

        MyComponentContainer ContainerBase { get; }
    }
}

