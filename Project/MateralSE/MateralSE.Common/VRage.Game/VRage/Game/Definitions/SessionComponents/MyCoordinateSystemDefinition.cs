namespace VRage.Game.Definitions.SessionComponents
{
    using System;
    using VRage.Game;
    using VRage.Game.Components.Session;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions.SessionComponents;

    [MyDefinitionType(typeof(MyObjectBuilder_CoordinateSystemDefinition), (Type) null)]
    public class MyCoordinateSystemDefinition : MySessionComponentDefinition
    {
        public double AngleTolerance = 0.0001;
        public double PositionTolerance = 0.001;
        public int CoordSystemSize = 0x3e8;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_CoordinateSystemDefinition definition = builder as MyObjectBuilder_CoordinateSystemDefinition;
            MyObjectBuilder_CoordinateSystemDefinition definition1 = definition;
            this.AngleTolerance = definition.AngleTolerance;
            this.PositionTolerance = definition.PositionTolerance;
            this.CoordSystemSize = definition.CoordSystemSize;
        }
    }
}

