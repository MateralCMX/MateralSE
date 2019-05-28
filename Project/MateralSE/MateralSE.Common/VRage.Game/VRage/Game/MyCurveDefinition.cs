namespace VRage.Game
{
    using System;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_CurveDefinition), (Type) null)]
    public class MyCurveDefinition : MyDefinitionBase
    {
        public VRageMath.Curve Curve;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            this.Curve = new VRageMath.Curve();
            foreach (MyObjectBuilder_CurveDefinition.Point point in (builder as MyObjectBuilder_CurveDefinition).Points)
            {
                this.Curve.Keys.Add(new CurveKey(point.Time, point.Value));
            }
        }
    }
}

