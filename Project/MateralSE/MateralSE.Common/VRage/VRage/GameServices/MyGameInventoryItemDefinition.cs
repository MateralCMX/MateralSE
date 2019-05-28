namespace VRage.GameServices
{
    using System;
    using System.Runtime.CompilerServices;

    public class MyGameInventoryItemDefinition
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string DisplayType { get; set; }

        public string IconTexture { get; set; }

        public string AssetModifierId { get; set; }

        public MyGameInventoryItemSlot ItemSlot { get; set; }

        public string ToolName { get; set; }

        public string NameColor { get; set; }

        public string BackgroundColor { get; set; }

        public MyGameInventoryItemDefinitionType DefinitionType { get; set; }

        public bool Hidden { get; set; }

        public bool IsStoreHidden { get; set; }

        public MyGameInventoryItemQuality ItemQuality { get; set; }

        public string Exchange { get; set; }
    }
}

