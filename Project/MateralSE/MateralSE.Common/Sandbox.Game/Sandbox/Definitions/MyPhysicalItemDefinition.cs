namespace Sandbox.Definitions
{
    using System;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Library;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_PhysicalItemDefinition), (Type) null)]
    public class MyPhysicalItemDefinition : MyDefinitionBase
    {
        public Vector3 Size;
        public float Mass;
        public string Model;
        public string[] Models;
        public MyStringId? IconSymbol;
        public float Volume;
        public float ModelVolume;
        public MyStringHash PhysicalMaterial;
        public MyStringHash VoxelMaterial;
        public bool CanSpawnFromScreen;
        public bool RotateOnSpawnX;
        public bool RotateOnSpawnY;
        public bool RotateOnSpawnZ;
        public int Health;
        public MyDefinitionId? DestroyedPieceId;
        public int DestroyedPieces;
        public StringBuilder ExtraInventoryTooltipLine;
        public MyFixedPoint MaxStackAmount;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_PhysicalItemDefinition definition = builder as MyObjectBuilder_PhysicalItemDefinition;
            this.Size = definition.Size;
            this.Mass = definition.Mass;
            this.Model = definition.Model;
            this.Models = definition.Models;
            this.Volume = (definition.Volume != null) ? (definition.Volume.Value / 1000f) : definition.Size.Volume;
            this.ModelVolume = (definition.ModelVolume != null) ? (definition.ModelVolume.Value / 1000f) : this.Volume;
            this.IconSymbol = !string.IsNullOrEmpty(definition.IconSymbol) ? new MyStringId?(MyStringId.GetOrCompute(definition.IconSymbol)) : null;
            this.PhysicalMaterial = MyStringHash.GetOrCompute(definition.PhysicalMaterial);
            this.VoxelMaterial = MyStringHash.GetOrCompute(definition.VoxelMaterial);
            this.CanSpawnFromScreen = definition.CanSpawnFromScreen;
            this.RotateOnSpawnX = definition.RotateOnSpawnX;
            this.RotateOnSpawnY = definition.RotateOnSpawnY;
            this.RotateOnSpawnZ = definition.RotateOnSpawnZ;
            this.Health = definition.Health;
            if (definition.DestroyedPieceId != null)
            {
                this.DestroyedPieceId = new MyDefinitionId?(definition.DestroyedPieceId.Value);
            }
            this.DestroyedPieces = definition.DestroyedPieces;
            this.ExtraInventoryTooltipLine = (definition.ExtraInventoryTooltipLine == null) ? new StringBuilder() : new StringBuilder().Append(MyEnvironment.NewLine).Append(definition.ExtraInventoryTooltipLine);
            this.MaxStackAmount = definition.MaxStackAmount;
        }

        public bool HasIntegralAmounts =>
            ((base.Id.TypeId != typeof(MyObjectBuilder_Ingot)) && (base.Id.TypeId != typeof(MyObjectBuilder_Ore)));

        public bool HasModelVariants =>
            ((this.Models != null) && (this.Models.Length != 0));
    }
}

