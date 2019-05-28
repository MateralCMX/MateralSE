namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Definitions;
    using System;

    internal class MyCubeBlockDefinitionWithVariants
    {
        private MyCubeBlockDefinition m_baseDefinition;
        private int m_variantIndex = -1;

        public MyCubeBlockDefinitionWithVariants(MyCubeBlockDefinition definition, int variantIndex)
        {
            this.m_baseDefinition = definition;
            this.m_variantIndex = variantIndex;
            if ((this.m_baseDefinition.Variants == null) || (this.m_baseDefinition.Variants.Count == 0))
            {
                this.m_variantIndex = -1;
            }
            else if (this.m_variantIndex != -1)
            {
                this.m_variantIndex = this.m_variantIndex % this.m_baseDefinition.Variants.Count;
            }
        }

        public void Next()
        {
            if ((this.m_baseDefinition.Variants != null) && (this.m_baseDefinition.Variants.Count > 0))
            {
                this.m_variantIndex++;
                this.m_variantIndex++;
                this.m_variantIndex = this.m_variantIndex % (this.m_baseDefinition.Variants.Count + 1);
                this.m_variantIndex--;
            }
        }

        public static implicit operator MyCubeBlockDefinitionWithVariants(MyCubeBlockDefinition definition) => 
            new MyCubeBlockDefinitionWithVariants(definition, -1);

        public static implicit operator MyCubeBlockDefinition(MyCubeBlockDefinitionWithVariants definition) => 
            ((definition != null) ? ((definition.m_variantIndex != -1) ? definition.m_baseDefinition.Variants[definition.m_variantIndex] : definition.m_baseDefinition) : null);

        public void Prev()
        {
            if ((this.m_baseDefinition.Variants != null) && (this.m_baseDefinition.Variants.Count > 0))
            {
                this.m_variantIndex = ((this.m_variantIndex + this.m_baseDefinition.Variants.Count) + 1) % (this.m_baseDefinition.Variants.Count + 1);
                this.m_variantIndex--;
            }
        }

        public void Reset()
        {
            this.m_variantIndex = -1;
        }

        public int VariantIndex =>
            this.m_variantIndex;

        public MyCubeBlockDefinition Base =>
            this.m_baseDefinition;
    }
}

