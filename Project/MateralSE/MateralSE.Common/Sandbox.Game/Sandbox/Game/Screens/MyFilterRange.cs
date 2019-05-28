namespace Sandbox.Game.Screens
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyFilterRange : IMyFilterOption
    {
        public MyFilterRange()
        {
            SerializableRange range = new SerializableRange();
            this.Value = range;
            this.Active = false;
        }

        public MyFilterRange(SerializableRange value, bool active = false)
        {
            this.Value = value;
            this.Active = active;
        }

        public void Configure(string value)
        {
            SerializableRange range;
            if (string.IsNullOrEmpty(value))
            {
                range = new SerializableRange();
                this.Value = range;
            }
            else
            {
                char[] separator = new char[] { ':' };
                string[] strArray = value.Split(separator);
                range = new SerializableRange {
                    Min = float.Parse(strArray[0]),
                    Max = float.Parse(strArray[1])
                };
                this.Value = range;
                this.Active = bool.Parse(strArray[2]);
            }
        }

        public bool IsMatch(float value)
        {
            if (this.Active)
            {
                return this.Value.ValueBetween(value);
            }
            return true;
        }

        public SerializableRange Value { get; set; }

        public bool Active { get; set; }

        public string SerializedValue
        {
            get
            {
                object[] objArray1 = new object[] { this.Value.Min, ":", this.Value.Max, ":", this.Active.ToString() };
                return string.Concat(objArray1);
            }
        }
    }
}

