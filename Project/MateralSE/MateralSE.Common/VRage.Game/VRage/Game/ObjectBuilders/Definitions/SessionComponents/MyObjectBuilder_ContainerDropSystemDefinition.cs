namespace VRage.Game.ObjectBuilders.Definitions.SessionComponents
{
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ContainerDropSystemDefinition : MyObjectBuilder_SessionComponentDefinition
    {
        public float PersonalContainerRatio = 0.95f;
        public float ContainerDropTime = 30f;
        public float PersonalContainerDistMin = 1f;
        public float PersonalContainerDistMax = 15f;
        public float CompetetiveContainerDistMin = 15f;
        public float CompetetiveContainerDistMax = 30f;
        public float CompetetiveContainerGPSTimeOut = 5f;
        public float CompetetiveContainerGridTimeOut = 60f;
        public float PersonalContainerGridTimeOut = 45f;
        public RGBColor CompetetiveContainerGPSColorFree;
        public RGBColor CompetetiveContainerGPSColorClaimed;
        public RGBColor PersonalContainerGPSColor;
        public string ContainerAudioCue = "BlockContainer";
    }
}

