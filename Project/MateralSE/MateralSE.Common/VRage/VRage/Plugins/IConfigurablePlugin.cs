namespace VRage.Plugins
{
    using System;

    public interface IConfigurablePlugin : IPlugin, IDisposable
    {
        IPluginConfiguration GetConfiguration(string userDataPath);
        string GetPluginTitle();
    }
}

