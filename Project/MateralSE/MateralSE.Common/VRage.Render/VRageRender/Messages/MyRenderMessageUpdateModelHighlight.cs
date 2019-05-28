namespace VRageRender.Messages
{
    using System;

    public class MyRenderMessageUpdateModelHighlight : MyRenderMessageBase
    {
        public uint ID;
        public int InstanceIndex;
        public string Model;
        public string[] SectionNames;
        public uint[] SubpartIndices;
        public Color? OutlineColor;
        public float Thickness;
        public float PulseTimeInSeconds;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateModelHighlight;
    }
}

