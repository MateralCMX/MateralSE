namespace Sandbox.Game.Screens
{
    using System;

    public interface IMyFilterOption
    {
        void Configure(string value);

        string SerializedValue { get; }
    }
}

