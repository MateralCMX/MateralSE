namespace Sandbox.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities.Cube;
    using System;

    [MyCubeBlockType(typeof(MyObjectBuilder_RealWheel))]
    public class MyRealWheel : MyMotorRotor
    {
        public override void ContactPointCallback(ref MyGridContactInfo value)
        {
            HkContactPointProperties contactProperties = value.Event.ContactProperties;
            contactProperties.Friction = 0.85f;
            contactProperties.Restitution = 0.2f;
            value.EnableParticles = false;
            value.RubberDeformation = true;
        }
    }
}

