namespace VRage.ModAPI
{
    using System;
    using VRage.Utils;

    public interface IMyHudStat
    {
        string GetValueString();
        void Update();

        MyStringHash Id { get; }

        float CurrentValue { get; }

        float MaxValue { get; }

        float MinValue { get; }
    }
}

