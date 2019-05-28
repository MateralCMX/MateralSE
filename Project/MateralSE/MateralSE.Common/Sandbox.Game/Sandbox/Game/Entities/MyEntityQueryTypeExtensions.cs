namespace Sandbox.Game.Entities
{
    using System;
    using System.Runtime.CompilerServices;

    public static class MyEntityQueryTypeExtensions
    {
        public static bool HasDynamic(this MyEntityQueryType qtype) => 
            (((int) (qtype & MyEntityQueryType.Dynamic)) != 0);

        public static bool HasStatic(this MyEntityQueryType qtype) => 
            (((int) (qtype & MyEntityQueryType.Static)) != 0);
    }
}

