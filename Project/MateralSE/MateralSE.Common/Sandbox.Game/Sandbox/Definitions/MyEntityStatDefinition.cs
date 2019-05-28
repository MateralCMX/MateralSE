namespace Sandbox.Definitions
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_EntityStatDefinition), (Type) null)]
    public class MyEntityStatDefinition : MyDefinitionBase
    {
        public float MinValue;
        public float MaxValue;
        public float DefaultValue;
        public bool EnabledInCreative;
        public string Name;
        public GuiDefinition GuiDef;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_EntityStatDefinition objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_EntityStatDefinition;
            objectBuilder.MinValue = this.MinValue;
            objectBuilder.MaxValue = this.MaxValue;
            objectBuilder.DefaultValue = this.DefaultValue;
            objectBuilder.EnabledInCreative = this.EnabledInCreative;
            objectBuilder.Name = this.Name;
            objectBuilder.GuiDef = new MyObjectBuilder_EntityStatDefinition.GuiDefinition();
            objectBuilder.GuiDef.HeightMultiplier = this.GuiDef.HeightMultiplier;
            objectBuilder.GuiDef.Priority = this.GuiDef.Priority;
            objectBuilder.GuiDef.Color = this.GuiDef.Color;
            objectBuilder.GuiDef.CriticalRatio = this.GuiDef.CriticalRatio;
            objectBuilder.GuiDef.DisplayCriticalDivider = this.GuiDef.DisplayCriticalDivider;
            objectBuilder.GuiDef.CriticalColorFrom = this.GuiDef.CriticalColorFrom;
            objectBuilder.GuiDef.CriticalColorTo = this.GuiDef.CriticalColorTo;
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_EntityStatDefinition definition = builder as MyObjectBuilder_EntityStatDefinition;
            this.MinValue = definition.MinValue;
            this.MaxValue = definition.MaxValue;
            this.DefaultValue = definition.DefaultValue;
            this.EnabledInCreative = definition.EnabledInCreative;
            this.Name = definition.Name;
            if (float.IsNaN(this.DefaultValue))
            {
                this.DefaultValue = this.MaxValue;
            }
            this.GuiDef = new GuiDefinition();
            if (definition.GuiDef != null)
            {
                this.GuiDef.HeightMultiplier = definition.GuiDef.HeightMultiplier;
                this.GuiDef.Priority = definition.GuiDef.Priority;
                this.GuiDef.Color = (Vector3I) definition.GuiDef.Color;
                this.GuiDef.CriticalRatio = definition.GuiDef.CriticalRatio;
                this.GuiDef.DisplayCriticalDivider = definition.GuiDef.DisplayCriticalDivider;
                this.GuiDef.CriticalColorFrom = (Vector3I) definition.GuiDef.CriticalColorFrom;
                this.GuiDef.CriticalColorTo = (Vector3I) definition.GuiDef.CriticalColorTo;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GuiDefinition
        {
            public float HeightMultiplier;
            public int Priority;
            public Vector3I Color;
            public float CriticalRatio;
            public bool DisplayCriticalDivider;
            public Vector3I CriticalColorFrom;
            public Vector3I CriticalColorTo;
        }
    }
}

