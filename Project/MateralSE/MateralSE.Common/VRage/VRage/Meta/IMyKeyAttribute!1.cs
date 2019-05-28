namespace VRage.Meta
{
    public interface IMyKeyAttribute<TKey>
    {
        TKey Key { get; }
    }
}

