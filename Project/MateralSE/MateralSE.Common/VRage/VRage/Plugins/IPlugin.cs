namespace VRage.Plugins
{
    using System;

    public interface IPlugin : IDisposable
    {
        void Init(object gameInstance);
        void Update();
    }
}

