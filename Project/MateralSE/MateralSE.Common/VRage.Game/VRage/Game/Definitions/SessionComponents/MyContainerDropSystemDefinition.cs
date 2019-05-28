namespace VRage.Game.Definitions.SessionComponents
{
    using System;
    using VRage.Game;
    using VRage.Game.Components.Session;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions.SessionComponents;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_ContainerDropSystemDefinition), (Type) null)]
    public class MyContainerDropSystemDefinition : MySessionComponentDefinition
    {
        [Obsolete]
        public float PersonalContainerRatio;
        [Obsolete]
        public int ContainerDropTime;
        public float PersonalContainerDistMin;
        public float PersonalContainerDistMax;
        public float CompetetiveContainerDistMin;
        public float CompetetiveContainerDistMax;
        public int CompetetiveContainerGPSTimeOut;
        public int CompetetiveContainerGridTimeOut;
        public int PersonalContainerGridTimeOut;
        public Color CompetetiveContainerGPSColorFree;
        public Color CompetetiveContainerGPSColorClaimed;
        public Color PersonalContainerGPSColor;
        public string ContainerAudioCue;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ContainerDropSystemDefinition definition = (MyObjectBuilder_ContainerDropSystemDefinition) builder;
            this.PersonalContainerRatio = 0.9f;
            this.ContainerDropTime = (int) Math.Max((float) (definition.ContainerDropTime * 60f), (float) 1f);
            this.PersonalContainerDistMin = definition.PersonalContainerDistMin * 1000f;
            this.PersonalContainerDistMax = definition.PersonalContainerDistMax * 1000f;
            this.CompetetiveContainerDistMin = definition.CompetetiveContainerDistMin * 1000f;
            this.CompetetiveContainerDistMax = definition.CompetetiveContainerDistMax * 1000f;
            this.CompetetiveContainerGPSTimeOut = ((int) definition.CompetetiveContainerGPSTimeOut) * 60;
            this.CompetetiveContainerGridTimeOut = ((int) definition.CompetetiveContainerGridTimeOut) * 60;
            this.PersonalContainerGridTimeOut = ((int) definition.PersonalContainerGridTimeOut) * 60;
            this.CompetetiveContainerGPSColorFree = new Color(definition.CompetetiveContainerGPSColorFree.R, definition.CompetetiveContainerGPSColorFree.G, definition.CompetetiveContainerGPSColorFree.B);
            this.CompetetiveContainerGPSColorClaimed = new Color(definition.CompetetiveContainerGPSColorClaimed.R, definition.CompetetiveContainerGPSColorClaimed.G, definition.CompetetiveContainerGPSColorClaimed.B);
            this.PersonalContainerGPSColor = new Color(definition.PersonalContainerGPSColor.R, definition.PersonalContainerGPSColor.G, definition.PersonalContainerGPSColor.B);
            this.ContainerAudioCue = definition.ContainerAudioCue;
        }
    }
}

