namespace VRage.Game.Components
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Models;

    public static class MyRenderComponentBaseExtensions
    {
        public static MyModel GetModel(this MyRenderComponentBase obj) => 
            ((MyModel) obj.ModelStorage);
    }
}

