namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_MedicalRoomDefinition), (Type) null)]
    public class MyMedicalRoomDefinition : MyCubeBlockDefinition
    {
        public string ResourceSinkGroup;
        public string IdleSound;
        public string ProgressSound;
        public string RespawnSuitName;
        public HashSet<string> CustomWardrobeNames;
        public bool RespawnAllowed;
        public bool HealingAllowed;
        public bool RefuelAllowed;
        public bool SuitChangeAllowed;
        public bool CustomWardrobesEnabled;
        public bool ForceSuitChangeOnRespawn;
        public bool SpawnWithoutOxygenEnabled;
        public Vector3D WardrobeCharacterOffset;
        public float WardrobeCharacterOffsetLength;
        public List<ScreenArea> ScreenAreas;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_MedicalRoomDefinition definition = builder as MyObjectBuilder_MedicalRoomDefinition;
            this.ResourceSinkGroup = definition.ResourceSinkGroup;
            this.IdleSound = definition.IdleSound;
            this.ProgressSound = definition.ProgressSound;
            this.RespawnSuitName = definition.RespawnSuitName;
            this.RespawnAllowed = definition.RespawnAllowed;
            this.HealingAllowed = definition.HealingAllowed;
            this.RefuelAllowed = definition.RefuelAllowed;
            this.SuitChangeAllowed = definition.SuitChangeAllowed;
            this.CustomWardrobesEnabled = definition.CustomWardrobesEnabled;
            this.ForceSuitChangeOnRespawn = definition.ForceSuitChangeOnRespawn;
            this.SpawnWithoutOxygenEnabled = definition.SpawnWithoutOxygenEnabled;
            this.WardrobeCharacterOffset = definition.WardrobeCharacterOffset;
            this.WardrobeCharacterOffsetLength = (float) this.WardrobeCharacterOffset.Length();
            this.CustomWardrobeNames = (definition.CustomWardRobeNames != null) ? new HashSet<string>(definition.CustomWardRobeNames) : new HashSet<string>();
            this.ScreenAreas = (definition.ScreenAreas != null) ? definition.ScreenAreas.ToList<ScreenArea>() : null;
        }
    }
}

