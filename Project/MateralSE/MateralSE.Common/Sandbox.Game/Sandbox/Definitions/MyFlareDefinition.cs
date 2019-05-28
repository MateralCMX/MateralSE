namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;
    using VRageRender.Messages;

    [MyDefinitionType(typeof(MyObjectBuilder_FlareDefinition), (Type) null)]
    public class MyFlareDefinition : MyDefinitionBase
    {
        public float Intensity;
        public Vector2 Size;
        public MySubGlare[] SubGlares;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_FlareDefinition definition = (MyObjectBuilder_FlareDefinition) builder;
            float? intensity = definition.Intensity;
            this.Intensity = (intensity != null) ? intensity.GetValueOrDefault() : 1f;
            Vector2? size = definition.Size;
            this.Size = (size != null) ? size.GetValueOrDefault() : new Vector2(1f, 1f);
            this.SubGlares = new MySubGlare[definition.SubGlares.Length];
            int index = 0;
            foreach (MySubGlare glare in definition.SubGlares)
            {
                this.SubGlares[index] = glare;
                this.SubGlares[index].Color = glare.Color.ToLinearRGB();
                index++;
            }
        }
    }
}

