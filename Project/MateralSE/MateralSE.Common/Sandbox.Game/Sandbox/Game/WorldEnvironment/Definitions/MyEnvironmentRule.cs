namespace Sandbox.Game.WorldEnvironment.Definitions
{
    using System;
    using VRageMath;

    public class MyEnvironmentRule
    {
        public SerializableRange Height = new SerializableRange(0f, 1f);
        public SymmetricSerializableRange Latitude = new SymmetricSerializableRange(-90f, 90f, true);
        public SerializableRange Longitude = new SerializableRange(-180f, 180f);
        public SerializableRange Slope = new SerializableRange(0f, 90f);

        public bool Check(float height, float latitude, float longitude, float slope) => 
            (this.Height.ValueBetween(height) && (this.Latitude.ValueBetween(latitude) && (this.Longitude.ValueBetween(longitude) && this.Slope.ValueBetween(slope))));

        public void ConvertRanges()
        {
            this.Latitude.ConvertToSine();
            this.Longitude.ConvertToCosineLongitude();
            this.Slope.ConvertToCosine();
        }
    }
}

