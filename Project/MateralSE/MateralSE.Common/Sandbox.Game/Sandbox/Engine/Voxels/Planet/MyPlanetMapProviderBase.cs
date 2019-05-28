namespace Sandbox.Engine.Voxels.Planet
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Voxels;
    using System;
    using VRage.Factory;
    using VRage.Game;
    using VRage.Game.ObjectBuilders;

    [MyFactorable(typeof(MyObjectFactory<MyPlanetMapProviderAttribute, MyPlanetMapProviderBase>))]
    public abstract class MyPlanetMapProviderBase
    {
        protected MyPlanetMapProviderBase()
        {
        }

        public abstract MyHeightCubemap GetHeightmap();
        public abstract MyCubemap[] GetMaps(MyPlanetMapTypeSet types);
        public abstract void Init(long seed, MyPlanetGeneratorDefinition generator, MyObjectBuilder_PlanetMapProvider builder);

        public static MyObjectFactory<MyPlanetMapProviderAttribute, MyPlanetMapProviderBase> Factory =>
            MyObjectFactory<MyPlanetMapProviderAttribute, MyPlanetMapProviderBase>.Get();
    }
}

